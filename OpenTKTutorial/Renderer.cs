using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using System;
using System.Drawing;

namespace OpenTKTutorial
{
    public class Renderer : IDisposable
    {
        #region Private Fields
        private readonly int _renderSurfaceWidth;
        private readonly int _renderSurfaceHeight;
        private QuadBufferData[] _vertexBufferData;
        private VertexBuffer<QuadBufferData> _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private VertexArray<QuadBufferData> _vertexArray;
        private bool _disposedValue = false;
        #endregion


        #region Constructors
        public Renderer(int renderSurfaceWidth, int renderSurfaceHeight)
        {
            Shader = new ShaderProgram("shader.vert", "shader.frag");

            _renderSurfaceWidth = renderSurfaceWidth;
            _renderSurfaceHeight = renderSurfaceHeight;

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);//TODO: Allow changing of this

            Shader.UseProgram();

            InitBufferData();

            _vertexBuffer = new VertexBuffer<QuadBufferData>(_vertexBufferData);
            _indexBuffer = new IndexBuffer(new uint[] { 0, 1, 3, 1, 2, 3, 4, 5, 7, 5, 6, 7 });

            _vertexArray = new VertexArray<QuadBufferData>(_vertexBuffer, _indexBuffer);
        }
        #endregion


        #region Props
        public ShaderProgram Shader { get; private set; }
        #endregion


        #region Public Methods
        public void Render(Texture texture)
        {
            _vertexArray.Bind();
            texture.Bind();


            UpdateGPUColorData(texture.TintColor);
            UpdateGPUTransform(texture.X,
                texture.Y,
                texture.Width,
                texture.Height,
                texture.Size,
                texture.Angle);


            //TODO: Try and use 4 instead of 8
            GL.DrawElements(PrimitiveType.Triangles, 8, DrawElementsType.UnsignedInt, IntPtr.Zero);

            texture.Unbind();
        }


        public void Render(Texture[] textures)
        {
            foreach (var texture in textures)
            {
                Render(texture);
            }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion


        #region Private Methods
        private void InitBufferData()
        {
            _vertexBufferData = new[]
{
                new QuadBufferData()
                {
                    CornerVertice = new Vector3(-1, 1, 0),
                    TextureCoords = new Vector2(0, 1),
                    TintColor = Color.FromArgb(255, 255, 0, 255).ToVector4(),
                    TextureSlot = 0
                },
                new QuadBufferData()
                {
                    CornerVertice = new Vector3(1, 1, 0),
                    TextureCoords = new Vector2(1, 1),
                    TintColor = Color.FromArgb(255, 255, 0, 255).ToVector4(),
                    TextureSlot = 0
                },
                new QuadBufferData()
                {
                    CornerVertice = new Vector3(1, -1, 0),
                    TextureCoords = new Vector2(1, 0),
                    TintColor = Color.FromArgb(255, 255, 0, 255).ToVector4(),
                    TextureSlot = 0
                },
                new QuadBufferData()
                {
                    CornerVertice = new Vector3(-1, -1, 0),
                    TextureCoords = new Vector2(0, 0),
                    TintColor = Color.FromArgb(255, 255, 0, 255).ToVector4(),
                    TextureSlot = 0
                }
            };
        }


        private void UpdateGPUColorData(Color tintClr)
        {
            for (int i = 0; i < _vertexBufferData.Length; i++)
            {
                _vertexBufferData[i].TintColor = tintClr.ToGLColor();
            }

            var dataSize = 48 * sizeof(float);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer.ID);
            GL.BufferData(BufferTarget.ArrayBuffer, dataSize, _vertexBufferData, BufferUsageHint.DynamicDraw);
        }

        private void UpdateGPUTransform(float x, float y, int width, int height, float size, float angle)
        {
            //Create and send the transformation data to the GPU
            var transMatrix = BuildTransformationMatrix(x,
                                               y,
                                               width,
                                               height,
                                               size,
                                               angle);

            var transDataLocation = GL.GetUniformLocation(Shader.ProgramId, "u_Transform");
            GL.UniformMatrix4(transDataLocation, true, ref transMatrix);
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
                _vertexBuffer.Dispose();
                _indexBuffer.Dispose();
                _vertexArray.Dispose();
            }

            _disposedValue = true;
        }
        #endregion
    }
}
