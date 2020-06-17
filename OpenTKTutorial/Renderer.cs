using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace OpenTKTutorial
{
    public class Renderer : IDisposable
    {
        #region Private Fields
        private readonly int _renderSurfaceWidth;
        private readonly int _renderSurfaceHeight;
        private VertexData[] _vertexBufferData;
        private readonly GPUBuffer _gpuBuffer;
        private bool _disposedValue = false;
        private readonly int _transDataLocation;
        private readonly Dictionary<int, SpriteBatchItem> _batchItems = new Dictionary<int, SpriteBatchItem>();
        private bool _hasBegun;
        private int _maxBatchSize = 48;
        private int _currentBatchItem = 0;
        private int _previousTextureID = -1;
        private int _currentTextureID;
        #endregion


        #region Constructors
        public Renderer(ShaderProgram shader, int renderSurfaceWidth, int renderSurfaceHeight)
        {
            for (int i = 0; i < _maxBatchSize; i++)
            {
                _batchItems.Add(i, SpriteBatchItem.Empty);
            }

            Shader = shader;

            _renderSurfaceWidth = renderSurfaceWidth;
            _renderSurfaceHeight = renderSurfaceHeight;

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);//TODO: Allow changing of this

            Shader.UseProgram();

            _gpuBuffer = new GPUBuffer(_maxBatchSize);

            _transDataLocation = GL.GetUniformLocation(Shader.ProgramId, "uTransform");
        }
        #endregion


        #region Props
        public ShaderProgram Shader { get; private set; }
        #endregion


        #region Public Methods
        public void Begin()
        {
            _hasBegun = true;
        }


        public void Render(ITexture texture, Rectangle srcRect, Rectangle destRect, float size, float angle, Color tintColor)
        {
            if (!_hasBegun)
                throw new Exception("Must call begin() first");

            _currentTextureID = texture.ID;

            //Has the textures switched
            if (texture.ID != _previousTextureID && _previousTextureID != -1)
            {
                RenderBatch();
                _currentBatchItem = 0;
                _previousTextureID = 0;
            }

            _currentBatchItem = _currentBatchItem >= _maxBatchSize ? 0 : _currentBatchItem;

            var batchItem = _batchItems[_currentBatchItem];
            batchItem.TextureID = texture.ID;
            batchItem.SrcRect = srcRect;
            batchItem.DestRect = destRect;
            batchItem.Size = size;
            batchItem.Angle = angle;
            batchItem.TintColor = tintColor;

            _batchItems[_currentBatchItem] = batchItem;

            _currentBatchItem += 1;
            _previousTextureID = texture.ID;
        }


        public void End()
        {
            //DEBUGGING ONLY
            RenderBatch();
            _currentBatchItem = 0;
            _previousTextureID = 0;
            _hasBegun = false;
        }


        private void RenderBatch()
        {
            //DEBUGGING ONLY
            var nonEmptyItems = _batchItems.Where(i => !i.Value.IsEmpty).ToArray();
            var sameIDForEntireBatch = (from i in _batchItems
                                        select i.Value.TextureID).ToArray().Distinct().Count() == 1;

            //TODO: This can probably just be set one time at the creation of the renderer.
            //Only if using the first texture slot for everything
            GL.ActiveTexture(TextureUnit.Texture0);

            var batchAmountToRender = 0;

            for (int i = 0; i < _batchItems.Values.Count; i++)
            {
                if (_batchItems[i].IsEmpty)
                    continue;

                GL.BindTexture(TextureTarget.Texture2D, _batchItems[i].TextureID);

                //Add GPU data update
                UpdateGPUColorData(_batchItems[i].TintColor);

                UpdateGPUTransform(
                    i,
                    _batchItems[i].DestRect.X,
                    _batchItems[i].DestRect.Y,
                    _batchItems[i].SrcRect.Width,
                    _batchItems[i].SrcRect.Height,
                    _batchItems[i].Size,
                    _batchItems[i].Angle);

                _gpuBuffer.UpdateQuad(i, _batchItems[i].SrcRect, _batchItems[i].DestRect.Width, _batchItems[i].DestRect.Height);

                batchAmountToRender += 1;
            }

            GL.DrawElements(PrimitiveType.Triangles, 6 * batchAmountToRender, DrawElementsType.UnsignedInt, IntPtr.Zero);

            EmptyBatchItems();
        }


        private void EmptyBatchItems()
        {
            for (int i = 0; i < _batchItems.Count; i++)
            {
                _batchItems[i] = SpriteBatchItem.Empty;
            }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion


        #region Private Methods
        private void UpdateGPUColorData(Color tintClr)
        {
            var tintClrData = tintClr.ToGLColor();

            //TODO: Work on caching this for performance
            var tintClrLocation = GL.GetUniformLocation(Shader.ProgramId, "u_TintColor");
            GL.Uniform4(tintClrLocation, tintClrData);
        }


        private void UpdateGPUTransform(int quadID, float x, float y, int width, int height, float size, float angle)
        {
            //Create and send the transformation data to the GPU
            var transMatrix = BuildTransformationMatrix(
                x,
                y,
                width,
                height,
                size,
                angle);

            
            GL.UniformMatrix4(_transDataLocation + quadID, true, ref transMatrix);
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
        /// <returns></returns>
        private Matrix4 BuildTransformationMatrix(float x, float y, int width, int height, float size, float angle)
        {
            var scaleX = (float)width / _renderSurfaceWidth;
            var scaleY = (float)height / _renderSurfaceHeight;

            scaleX *= size;
            scaleY *= size;

            var ndcX = x.MapValue(0f, _renderSurfaceWidth, -1f, 1f);
            var ndcY = y.MapValue(0f, _renderSurfaceHeight, 1f, -1f);

            //NOTE: (+ degrees) rotates CCW and (- degress) rotates CW
            var angleRadians = MathHelper.DegreesToRadians(angle);

            //Invert angle to rotate CW instead of CCW
            angleRadians *= -1;

            var rotation = Matrix4.CreateRotationZ(angleRadians);
            var scaleMatrix = Matrix4.CreateScale(scaleX, scaleY, 1f);
            var positionMatrix = Matrix4.CreateTranslation(new Vector3(ndcX, ndcY, 0));


            return rotation * scaleMatrix * positionMatrix;
        }
        #endregion


        #region Protected Methods
        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue)
                return;

            if (disposing)
            {
                Shader.Dispose();
                //_vertexBuffer.Dispose();
                //_indexBuffer.Dispose();
                //_vertexArray.Dispose();
            }

            _disposedValue = true;
        }
        #endregion
    }
}
