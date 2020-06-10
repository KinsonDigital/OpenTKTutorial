using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;

namespace OpenTKTutorial
{
    public class Renderer : IDisposable
    {
        #region Private Fields
        private readonly int _renderSurfaceWidth;
        private readonly int _renderSurfaceHeight;
        private VertexData[] _vertexBufferData;
        private readonly VertexBuffer<VertexData> _vertexBuffer;
        private readonly IndexBuffer _indexBuffer;
        private VertexArray<VertexData> _vertexArray;
        private bool _disposedValue = false;
        private const int ELEMENTS_PER_QUAD = 6;
        private bool _hasBegun;
        private int _currentBatch = 0;
        private readonly List<Batch> _batchPool = new List<Batch>();
        private readonly GPU _gpu = GPU.Instance;//SINGLETON
        #endregion


        #region Constructors
        public Renderer(int renderSurfaceWidth, int renderSurfaceHeight)
        {
            Shader = _gpu.GetShaderProgram();

            _renderSurfaceWidth = renderSurfaceWidth;
            _renderSurfaceHeight = renderSurfaceHeight;

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);//TODO: Allow changing of this

            Shader.UseProgram();

            _vertexBufferData = CreateVertexBufferData();

            _vertexBuffer = new VertexBuffer<VertexData>(_vertexBufferData);

            _indexBuffer = new IndexBuffer();

            _vertexArray = new VertexArray<VertexData>(_vertexBuffer, _indexBuffer);
            _vertexArray.Bind();

            //Must have at least 1 batch
            _batchPool.Add(new Batch());
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


        public void Render(Texture texture)
        {
            //texture.Bind();//TODO: This will move to the end() method

            var batchIndex = _batchPool.IndexOf(b => b.HasSpace);

            //If no batches have any room, add a new batch to the batch pool
            if (batchIndex == -1)
            {
                _batchPool.Add(new Batch());
                batchIndex = _batchPool.Count - 1;
            }

            _batchPool[batchIndex].AddTextureData(texture);
        }
        


        public void End()
        {
            //TODO: This will update the data and render each batch one batch at a time
            if (!_hasBegun)
                throw new Exception("Must call begin first");

            //Update all of the data on the GPU
            foreach (var batch in _batchPool)
            {
                if (batch.IsEmpty)
                    continue;

                batch.BindBatch();

                batch.UpdateTintColors();
                batch.UpdateTransforms();
            }

            var totalElements = ELEMENTS_PER_QUAD * _gpu.TotalTextureSlots;
            GL.DrawElements(PrimitiveType.Triangles, totalElements, DrawElementsType.UnsignedInt, IntPtr.Zero);

            _batchPool.ForEach(b => b.Clear());

            _hasBegun = false;
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion


        #region Private Methods
        private QuadData CreateQuad(int textureSlot)
        {
            return new QuadData()
            {
                Vertex1 = new VertexData()
                {
                    Vertex = new Vector3(-1, 1, 0),//Top Left
                    TextureCoord = new Vector2(0, 1),
                    TextureSlot = textureSlot
                },
                Vertex2 = new VertexData()
                {
                    Vertex = new Vector3(1, 1, 0),//Top Right
                    TextureCoord = new Vector2(1, 1),
                    TextureSlot = textureSlot
                },
                Vertex3 = new VertexData()
                {
                    Vertex = new Vector3(1, -1, 0),//Bottom Right
                    TextureCoord = new Vector2(1, 0),
                    TextureSlot = textureSlot
                },
                Vertex4 = new VertexData()
                {
                    Vertex = new Vector3(-1, -1, 0),//Bottom Left
                    TextureCoord = new Vector2(0, 0),
                    TextureSlot = textureSlot
                }
            };
        }


        private VertexData[] CreateVertexBufferData()
        {
            var result = new List<VertexData>();

            for (int i = 0; i < _gpu.TotalTextureSlots; i++)
            {
                result.AddRange(CreateQuad(i).GetVertices());
            }

            return result.ToArray();
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


        private void UpdateBatchData(Texture texture)
        {
            //_batchPool[_currentBatch].SetTextureSlot(texture.TextureSlot);
            //_batchPool[_currentBatch].SetX(texture.TextureSlot, texture.X);
            //_batchPool[_currentBatch].SetY(texture.TextureSlot, texture.Y);
            //_batchPool[_currentBatch].SetWidth(texture.TextureSlot, texture.Width);
            //_batchPool[_currentBatch].SetHeight(texture.TextureSlot, texture.Height);
            //_batchPool[_currentBatch].SetSize(texture.TextureSlot, texture.Size);
            //_batchPool[_currentBatch].SetAngle(texture.TextureSlot, texture.Angle);
            //_batchPool[_currentBatch].SetTintColor(texture.TextureSlot, texture.TintColor.ToGLColor());
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
                _vertexBuffer.Dispose();
                _indexBuffer.Dispose();
                _vertexArray.Dispose();
            }

            _disposedValue = true;
        }
        #endregion
    }
}
