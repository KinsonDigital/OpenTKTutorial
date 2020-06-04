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
        /* TODO:  Look into these
         * GL.GetInteger(GetPName.MaxCombinedTextureImageUnits,       out MaxTextureUnitsCombined);
            GL.GetInteger(GetPName.MaxVertexTextureImageUnits,         out MaxTextureUnitsVertex);
            GL.GetInteger(GetPName.MaxGeometryTextureImageUnits,       out MaxTextureUnitsGeometry);
            GL.GetInteger(GetPName.MaxTessControlTextureImageUnits,    out MaxTextureUnitsTessControl);
            GL.GetInteger(GetPName.MaxTessEvaluationTextureImageUnits, out MaxTextureUnitsTessEval);
            GL.GetInteger(GetPName.MaxTextureImageUnits,               out MaxTextureUnitsFragment);

        This has to do with finding out what our max texture slots are on the current GPU

        TODO: Cache any location calls to improve performance.  Get all of our locations during
        shader creation/compilation?  It might be a good idea to get a list of all of the attribute
        and uniform names, then use those names to pull and cache all of the locations.
         */
        #region Private Fields
        private readonly int _renderSurfaceWidth;
        private readonly int _renderSurfaceHeight;
        private VertexData[] _vertexBufferData;
        private VertexArrayBuffer<VertexData> _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private VertexArray<VertexData> _vertexArray;
        private bool _disposedValue = false;
        private bool _hasBegun;
        private Dictionary<int, Texture> _textures = new Dictionary<int, Texture>();
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

            _vertexBuffer = new VertexArrayBuffer<VertexData>(_vertexBufferData);
            _indexBuffer = new IndexBuffer(new uint[]
            {
                0, 1, 3, 1, 2, 3, //Quad 1
                4, 5, 7, 5, 6, 7  //Quad 2
            });

            _vertexArray = new VertexArray<VertexData>(_vertexBuffer, _indexBuffer);
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
            /*TODO:
             * https://www.youtube.com/watch?v=5df3NvQNzUs&list=PLlrATfBNZ98foTJPJ_Ev03o2oq3-GGOS2&index=31
             * Add code here to dynamically update the data in our vertex buffer.  This should
             * be done for each "render" call.
             * 
             * Steps:
             *      1. Bind buffer => glBindBuffer(GL_ARRAY_BUFFER, buffer_id)
             *      2. GL.BufferSubData(BufferTarget.ArrayBuffer, start, size_of_region_to_update, data_to_send);
             *          Use the generic version to make use of the structs
             */

            _vertexArray.Bind();

            //Update the list of textures
            if (_textures.ContainsKey(texture.TextureIndex))
            {
                //Just update the texture
                _textures[texture.TextureIndex] = texture;
            }
            else
            {
                //Add the texture
                _textures.Add(texture.TextureIndex, texture);
            }
        }


        public void End()
        {
            if (!_hasBegun)
                throw new Exception("Must call begin first");

            var dataSize = 48 * sizeof(float);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer.ID);

            /*TODO:
             * https://www.youtube.com/watch?v=5df3NvQNzUs&list=PLlrATfBNZ98foTJPJ_Ev03o2oq3-GGOS2&index=31
             * Refer to the TODO comment in the UpdateGPUTransform() method.
             * This would have to using this GL call below only a single time for allocating the data on the GPU
             * that is enough memory for the total number of quads that matches the total number of texture slots
             * supported.
             * 
             * To do this, you use the GL call below like this:
             *      GL.BufferData(BufferTarget.ArrayBuffer, <total-size-in-bites-here>, null, BufferUsageHint.DynamicDraw);
             */
            GL.BufferData(BufferTarget.ArrayBuffer, dataSize, _vertexBufferData, BufferUsageHint.DynamicDraw);


            foreach (var kvp in _textures)
            {
                _textures[kvp.Key].Bind(Shader.ProgramId);
            }


            foreach (var kvp in _textures)
            {
                var texture = _textures[kvp.Key];

                //Update color in GPU
                UpdateGPUColorData(kvp.Key, texture.TintColor);

                //Update transform in GPU
                UpdateGPUTransform(kvp.Key,
                    texture.X,
                    texture.Y,
                    texture.Width,
                    texture.Height,
                    texture.Size,
                    texture.Angle);
            }

            GL.DrawElements(PrimitiveType.Triangles, 12, DrawElementsType.UnsignedInt, IntPtr.Zero);

            foreach (var kvp in _textures)
                _textures[kvp.Key].Unbind();

            _hasBegun = false;
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
                    TextureCoord = new Vector2(0, 1),
                    TextureIndex = 0
                },
                new VertexData()
                {
                    Vertex = new Vector3(1, 1, 0),//Top Right
                    TextureCoord = new Vector2(1, 1),
                    TextureIndex = 0
                },
                new VertexData()
                {
                    Vertex = new Vector3(1, -1, 0),//Bottom Right
                    TextureCoord = new Vector2(1, 0),
                    TextureIndex = 0
                },
                new VertexData()
                {
                    Vertex = new Vector3(-1, -1, 0),//Bottom Left
                    TextureCoord = new Vector2(0, 0),
                    TextureIndex = 0
                },

                //Quad 2
                new VertexData()
                {
                    Vertex = new Vector3(-1, 1, 0),
                    TextureCoord = new Vector2(0, 1),
                    TextureIndex = 1
                },
                new VertexData()
                {
                    Vertex = new Vector3(1, 1, 0),
                    TextureCoord = new Vector2(1, 1),
                    TextureIndex = 1
                },
                new VertexData()
                {
                    Vertex = new Vector3(1, -1, 0),
                    TextureCoord = new Vector2(1, 0),
                    TextureIndex = 1
                },
                new VertexData()
                {
                    Vertex = new Vector3(-1, -1, 0),
                    TextureCoord = new Vector2(0, 0),
                    TextureIndex = 1
                }
            };
        }


        private void UpdateGPUColorData(int textureIndex, Color tintClr)
        {
            var tintClrData = tintClr.ToGLColor();

            //TODO: Hard code location to improve performance
            var tintClrLocation = GL.GetUniformLocation(Shader.ProgramId, "u_TintColor");
            GL.Uniform4(tintClrLocation + textureIndex, tintClrData);
        }


        private void UpdateGPUTransform(int textureIndex, float x, float y, int width, int height, float size, float angle)
        {
            /*TODO: Need to think about possibly adding the transformation data into the
                vertex buffer.  To find out which way is the best, we are going to need to
                do some perf testing to see if using uniforms or vertex buffers are the
                best performance vs easier to use to do this.

                The idea here is that instead of using vertex data that holds the vertices,
                and the uniform array for holding the matrices for every single quad,
                we could just update the vertices themselves for that "section" of data
                for all of the quads before we perform our quad.

            */

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
