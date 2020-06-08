using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

namespace OpenTKTutorial
{
    public class Renderer : IDisposable
    {
        #region Private Fields
        private readonly int _renderSurfaceWidth;
        private readonly int _renderSurfaceHeight;
        private VertexData[] _vertexBufferData;
        private VertexBuffer<VertexData> _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private VertexArray<VertexData> _vertexArray;
        private bool _disposedValue = false;
        private bool _hasBegun;
        private int _totalTextureSlots = -1;
        private Dictionary<int, Texture> _textures = new Dictionary<int, Texture>();
        #endregion


        #region Constructors
        public Renderer(int renderSurfaceWidth, int renderSurfaceHeight)
        {
            //Get the total number of available texture slots for the vertex and fragment shaders
            GL.GetInteger(GetPName.MaxVertexTextureImageUnits, out int maxVertexStageTextureSlots);
            GL.GetInteger(GetPName.MaxTextureImageUnits, out int maxFragmentStageTextureSlots);

            _totalTextureSlots = maxVertexStageTextureSlots < maxFragmentStageTextureSlots
                ? maxVertexStageTextureSlots
                : maxFragmentStageTextureSlots;

            Shader = new ShaderProgram("shader.vert", "shader.frag");

            _renderSurfaceWidth = renderSurfaceWidth;
            _renderSurfaceHeight = renderSurfaceHeight;

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);//TODO: Allow changing of this

            Shader.UseProgram();

            InitBufferData();

            _vertexBuffer = new VertexBuffer<VertexData>(_vertexBufferData, 2);

            _indexBuffer = new IndexBuffer(new uint[]
            {
                0, 1, 3, 1, 2, 3, //Quad 1
                4, 5, 7, 5, 6, 7  //Quad 2
            });

            _vertexArray = new VertexArray<VertexData>(_vertexBuffer, _indexBuffer);
            _vertexArray.Bind();
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
            texture.Bind(Shader.ProgramId);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer.ID);

            _vertexBuffer.UpdateTintColor(texture.TextureSlot, texture.TintColor);

            if (_textures.ContainsKey(texture.TextureSlot))
            {
                //Just update the texture
                _textures[texture.TextureSlot] = texture;
            }
            else
            {
                //Add the texture
                _textures.Add(texture.TextureSlot, texture);
            }
        }


        public void End()
        {
            if (!_hasBegun)
                throw new Exception("Must call begin first");


            foreach (var kvp in _textures)
            {
                var texture = _textures[kvp.Key];
                UpdateGPUTransform(kvp.Key,
                    texture.X,
                    texture.Y,
                    texture.Width,
                    texture.Height,
                    texture.Size,
                    texture.Angle);
            }

            GL.DrawElements(PrimitiveType.Triangles, 12, DrawElementsType.UnsignedInt, IntPtr.Zero);

            //foreach (var kvp in _textures)
            //    _textures[kvp.Key].Unbind();

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
                    TextureIndex = textureSlot
                },
                Vertex2 = new VertexData()
                {
                    Vertex = new Vector3(1, 1, 0),//Top Right
                    TextureCoord = new Vector2(1, 1),
                    TextureIndex = textureSlot
                },
                Vertex3 = new VertexData()
                {
                    Vertex = new Vector3(1, -1, 0),//Bottom Right
                    TextureCoord = new Vector2(1, 0),
                    TextureIndex = textureSlot
                },
                Vertex4 = new VertexData()
                {
                    Vertex = new Vector3(-1, -1, 0),//Bottom Left
                    TextureCoord = new Vector2(0, 0),
                    TextureIndex = textureSlot
                }
            };
        }


        private void InitBufferData()
        {
            var quad1 = CreateQuad(0);
            var quad2 = CreateQuad(1);

            var result = new List<VertexData>();

            result.AddRange(quad1.GetVertices());

            result.AddRange(quad2.GetVertices());

            _vertexBufferData = result.ToArray();
        }


        private void UpdateGPUTransform(int textureIndex, float x, float y, int width, int height, float size, float angle)
        {
            //Create and send the transformation data to the GPU
            var transMatrix = BuildTransformationMatrix(x,
                y,
                width,
                height,
                size,
                angle);

            //TODO: Hard code location to improve performance
            var transDataLocation = GL.GetUniformLocation(Shader.ProgramId, "u_Transforms");
            GL.UniformMatrix4(transDataLocation + textureIndex, true, ref transMatrix);
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
