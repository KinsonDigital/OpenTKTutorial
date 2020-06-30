// <copyright file="SpriteBatch.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace OpenTKTutorial
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Numerics;
    using Silk.NET.OpenGL;

    public class SpriteBatch : IDisposable
    {
        private readonly int renderSurfaceWidth;
        private readonly int renderSurfaceHeight;
        private readonly GPUBuffer<VertexData> gpuBuffer;
        private readonly int transDataLocation;
        private readonly Dictionary<int, SpriteBatchItem> batchItems = new Dictionary<int, SpriteBatchItem>();
        private readonly GL GL;
        private readonly ShaderProgram shader;
        private readonly int maxBatchSize = 2;
        private bool disposedValue = false;
        private bool hasBegun;
        private int currentBatchItem = 0;
        private uint previousTextureID = 0;
        private bool firstRenderMethodInvoke = true;
        private uint currentTextureID;

        public SpriteBatch(GL gl, int renderSurfaceWidth, int renderSurfaceHeight)
        {
            this.GL = gl;
            this.shader = new ShaderProgram(gl, this.maxBatchSize, "shader.vert", "shader.frag");

            for (var i = 0; i < this.maxBatchSize; i++)
            {
                this.batchItems.Add(i, SpriteBatchItem.Empty);
            }

            this.renderSurfaceWidth = renderSurfaceWidth;
            this.renderSurfaceHeight = renderSurfaceHeight;

            this.GL.Enable(EnableCap.Blend);
            this.GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            this.GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f); // TODO: Allow changing of this

            this.GL.ActiveTexture(TextureUnit.Texture0);

            this.shader.UseProgram();

            this.gpuBuffer = new GPUBuffer<VertexData>(this.GL, this.maxBatchSize);

            this.transDataLocation = this.GL.GetUniformLocation(this.shader.ProgramId, "uTransform");
        }

        public void Begin() => this.hasBegun = true;

        /// <summary>
        /// Renders the given <see cref="Texture"/> using the given parametters.
        /// </summary>
        /// <param name="texture">The texture to render.</param>
        /// <param name="srcRect">The rectangle of the sub texture within the texture to render.</param>
        /// <param name="destRect">The destination rectangle of rendering.</param>
        /// <param name="size">The size to render the texture at. 1 is for 100%/normal size.</param>
        /// <param name="angle">The angle of rotation in degrees of the rendering.</param>
        /// <param name="tintColor">The color to apply to the rendering.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Exception message only used inside method.")]
        public void Render(ITexture texture, Rectangle srcRect, Rectangle destRect, float size, float angle, Color tintColor)
        {
            if (!this.hasBegun)
                throw new Exception("Must call begin() first");

            this.currentTextureID = texture.ID;

            bool hasSwitchedTexture = this.currentTextureID != this.previousTextureID && !this.firstRenderMethodInvoke;

            // var totalBatchItems = _batchItems.Count(i => !i.Value.IsEmpty);
            var batchIsFull = this.batchItems.Values.ToArray().All(i => !i.IsEmpty);

            // Has the textures switched
            if (hasSwitchedTexture || batchIsFull)
            {
                RenderBatch();
                this.currentBatchItem = 0;
                this.previousTextureID = 0;
            }

            this.currentBatchItem = this.currentBatchItem >= this.maxBatchSize ? 0 : this.currentBatchItem;

            var batchItem = this.batchItems[this.currentBatchItem];
            batchItem.TextureID = texture.ID;
            batchItem.SrcRect = srcRect;
            batchItem.DestRect = destRect;
            batchItem.Size = size;
            batchItem.Angle = angle;
            batchItem.TintColor = tintColor;

            this.batchItems[this.currentBatchItem] = batchItem;

            this.currentBatchItem += 1;
            this.previousTextureID = this.currentTextureID;
            this.firstRenderMethodInvoke = false;
        }

        public void End()
        {
            if (this.batchItems.All(i => i.Value.IsEmpty))
                return;

            RenderBatch();
            this.currentBatchItem = 0;
            this.previousTextureID = 0;
            this.hasBegun = false;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        /// <param name="disposing">True if managed resources should be disposed of.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.disposedValue)
                return;

            if (disposing)
            {
                this.shader.Dispose();

                // _vertexBuffer.Dispose();
                // _indexBuffer.Dispose();
                // _vertexArray.Dispose();
            }

            this.disposedValue = true;
        }

        private unsafe void RenderBatch()
        {
            var batchAmountToRender = (uint)this.batchItems.Count(i => !i.Value.IsEmpty);

            for (var i = 0; i < this.batchItems.Values.Count; i++)
            {
                if (this.batchItems[i].IsEmpty)
                    continue;

                this.GL.BindTexture(TextureTarget.Texture2D, this.batchItems[i].TextureID);

                UpdateGPUTransform(
                    i,
                    this.batchItems[i].DestRect.X,
                    this.batchItems[i].DestRect.Y,
                    this.batchItems[i].SrcRect.Width,
                    this.batchItems[i].SrcRect.Height,
                    this.batchItems[i].Size,
                    this.batchItems[i].Angle);

                this.gpuBuffer.UpdateQuad(
                    i,
                    this.batchItems[i].SrcRect,
                    this.batchItems[i].DestRect.Width,
                    this.batchItems[i].DestRect.Height,
                    this.batchItems[i].TintColor);
            }

            // Only render the amount of elements for the amount of batch items to render.
            // 6 = the number of vertices/quad and each batch is a quad. batchAmontToRender is the total quads to render
            if (batchAmountToRender > 0)
            {
                this.GL.DrawElements(PrimitiveType.Triangles, 6 * batchAmountToRender, DrawElementsType.UnsignedInt, IntPtr.Zero.ToPointer());
            }

            // Empty the batch items
            for (var i = 0; i < this.batchItems.Count; i++)
            {
                this.batchItems[i] = SpriteBatchItem.Empty;
            }
        }

        private unsafe void UpdateGPUTransform(int quadID, float x, float y, int width, int height, float size, float angle)
        {
            // Create and send the transformation data to the GPU
            var transMatrix = BuildTransformationMatrix(
                x,
                y,
                width,
                height,
                size,
                angle);

            this.GL.UniformMatrix4(this.transDataLocation + quadID, 1, true, (float*)&transMatrix);
        }

        /// <summary>
        /// Builds a complete transformation matrix using the given params.
        /// </summary>
        /// <param name="x">The x position of a texture.</param>
        /// <param name="y">The y position of a texture.</param>
        /// <param name="width">The width of a texture.</param>
        /// <param name="height">The height of a texture.</param>
        /// <param name="size">The size of a texture. 1 represents normal size and 1.5 represents 150%.</param>
        /// <param name="angle">The angle of the texture.</param>
        private Matrix4x4 BuildTransformationMatrix(float x, float y, int width, int height, float size, float angle)
        {
            var scaleX = (float)width / this.renderSurfaceWidth;
            var scaleY = (float)height / this.renderSurfaceHeight;

            scaleX *= size;
            scaleY *= size;

            var ndcX = x.MapValue(0f, this.renderSurfaceWidth, -1f, 1f);
            var ndcY = y.MapValue(0f, this.renderSurfaceHeight, 1f, -1f);

            // NOTE: (+ degrees) rotates CCW and (- degress) rotates CW
            var angleRadians = angle.ToRadians();

            // Invert angle to rotate CW instead of CCW
            angleRadians *= -1;

            var rotation = Matrix4x4.CreateRotationZ(angleRadians);
            var scaleMatrix = Matrix4x4.CreateScale(scaleX, scaleY, 1f);
            var positionMatrix = Matrix4x4.CreateTranslation(new Vector3(ndcX, ndcY, 0));

            return rotation * scaleMatrix * positionMatrix;
        }
    }
}
