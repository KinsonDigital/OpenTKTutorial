using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace OpenTKTutorial
{
    public class Renderer : IDisposable
    {
        /* TODO:  Look into these
         * GL.GetInteger(GetPName.MaxCombinedTextureImageUnits,       out MaxTextureUnitsCombined);
            GL.GetInteger(GetPName.MaxVertexTextureImageUnits,         out MaxTextureUnitsVertex);
            GL.GetInteger(GetPName.MaxGeometryTextureImageUnits,       out MaxTextureUnitsGeometry);
            GL.GetInteger(GetPName.MaxTessControlTextureImageUnits,    out MaxTextureUnitsTessControl);
            GL.GetInteger(GetPName.MaxTessEvaluationTextureImageUnits, out MaxTextureUnitsTessEval);
            GL.GetInteger(GetPName.MaxTextureImageUnits,               out MaxTextureUnitsFragment);

        This has to do with finding out what our max texture slots are on the current GPU
         */
        #region Private Fields
        private readonly int _renderSurfaceWidth;
        private readonly int _renderSurfaceHeight;
        private VertexData[] _vertexBufferData;
        private VertexBuffer<VertexData> _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private VertexArray<VertexData> _vertexArray;
        private bool _disposedValue = false;
        private Dictionary<int, Texture> _textures = new Dictionary<int, Texture>();
        private readonly int _transDataLocation;
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

            _vertexBuffer = new VertexBuffer<VertexData>(_vertexBufferData);
            _indexBuffer = new IndexBuffer(new uint[]
            {
                0, 1, 3, 1, 2, 3 //Quad indices
            });

            _vertexArray = new VertexArray<VertexData>(_vertexBuffer, _indexBuffer);
            _vertexArray.Bind();


            _transDataLocation = GL.GetUniformLocation(Shader.ProgramId, "u_Transform");
        }
        #endregion


        #region Props
        public ShaderProgram Shader { get; private set; }
        #endregion


        #region Public Methods
        public void Render(Texture texture)
        {
            try
            {
                texture.Bind();

                UpdateGPUColorData(texture.TintColor);

                UpdateGPUTransform(texture.X,
                    texture.Y,
                    texture.Width,
                    texture.Height,
                    texture.Size,
                    texture.Angle);

                GL.DrawElements(PrimitiveType.Triangles, 8, DrawElementsType.UnsignedInt, IntPtr.Zero);

                texture.Unbind();
            }
            catch (Exception ex)
            {
                throw;
            }
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
                new VertexData()
                {
                    Vertex = new Vector3(-1, 1, 0),//Top Left
                    TextureCoord = new Vector2(0, 1)
                },
                new VertexData()
                {
                    Vertex = new Vector3(1, 1, 0),//Top Right
                    TextureCoord = new Vector2(1, 1)
                },
                new VertexData()
                {
                    Vertex = new Vector3(1, -1, 0),//Bottom Right
                    TextureCoord = new Vector2(1, 0)
                },
                new VertexData()
                {
                    Vertex = new Vector3(-1, -1, 0),//Bottom Left
                    TextureCoord = new Vector2(0, 0)
                }
            };
        }


        private void UpdateGPUColorData(Color tintClr)
        {
            var tintClrData = tintClr.ToGLColor();

            GL.Uniform4(3, tintClrData);
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

            
            GL.UniformMatrix4(_transDataLocation, true, ref transMatrix);
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
