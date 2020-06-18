using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace OpenTKTutorial
{
    public class SpriteBatch : IDisposable
    {
        #region Private Fields
        private readonly int _renderSurfaceWidth;
        private readonly int _renderSurfaceHeight;
        private VertexData[] _vertexBufferData;
        private readonly GPUBuffer<VertexData> _gpuBuffer;
        private bool _disposedValue = false;
        private readonly int _transDataLocation;
        private readonly int _tintClrLocation;
        private readonly Dictionary<int, SpriteBatchItem> _batchItems = new Dictionary<int, SpriteBatchItem>();
        private readonly ShaderProgram _shader;
        private bool _hasBegun;
        private int _maxBatchSize = 2;
        private int _currentBatchItem = 0;
        private int _previousTextureID = -1;
        private bool _firstRender;
        #endregion


        #region Constructors
        public SpriteBatch(int renderSurfaceWidth, int renderSurfaceHeight)
        {
            _shader = new ShaderProgram(_maxBatchSize, "shader.vert", "shader.frag");

            for (int i = 0; i < _maxBatchSize; i++)
            {
                _batchItems.Add(i, SpriteBatchItem.Empty);
            }

            _renderSurfaceWidth = renderSurfaceWidth;
            _renderSurfaceHeight = renderSurfaceHeight;

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);//TODO: Allow changing of this

            GL.ActiveTexture(TextureUnit.Texture0);

            _shader.UseProgram();

            _gpuBuffer = new GPUBuffer<VertexData>(_maxBatchSize);

            _transDataLocation = GL.GetUniformLocation(_shader.ProgramId, "uTransform");
            _tintClrLocation = GL.GetUniformLocation(_shader.ProgramId, "u_TintColor");
        }
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

            bool HasSwitchedTexture() => texture.ID != _previousTextureID && !_firstRender;

            var totalBatchItems = _batchItems.Count(i => !i.Value.IsEmpty);

            //Has the textures switched
            if (HasSwitchedTexture() || totalBatchItems >= _maxBatchSize)
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
            _firstRender = true;
        }


        public void End()
        {
            if (_batchItems.Count(i => !i.Value.IsEmpty) <= 0)
                return;

            //DEBUGGING ONLY
            RenderBatch();
            _currentBatchItem = 0;
            _previousTextureID = 0;
            _hasBegun = false;
        }


        private void RenderBatch()
        {
            var batchAmountToRender = _batchItems.Count(i => !i.Value.IsEmpty);

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

            //Only render the amount of elements for the amount of batch items to render.
            //6 = the number of vertices/quad and each batch is a quad. batchAmontToRender is the total quads to render
            if (batchAmountToRender > 0)
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

            GL.Uniform4(_tintClrLocation, tintClrData);
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
                _shader.Dispose();
                //_vertexBuffer.Dispose();
                //_indexBuffer.Dispose();
                //_vertexArray.Dispose();
            }

            _disposedValue = true;
        }
        #endregion
    }
}
