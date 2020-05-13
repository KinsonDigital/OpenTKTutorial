using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using System.Drawing;

namespace OpenTKTutorial
{
    public class Renderer
    {
        private readonly ShaderProgram _shaderProgram;
        private readonly int _renderSurfaceWidth;
        private readonly int _renderSurfaceHeight;

        public Renderer(int renderSurfaceWidth, int renderSurfaceHeight)
        {
            _shaderProgram = new ShaderProgram("shader.vert", "shader.frag");

            SetupShader();

            _renderSurfaceWidth = renderSurfaceWidth;
            _renderSurfaceHeight = renderSurfaceHeight;

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }


        public void Render(Texture texture)
        {
            var transMatrix = BuildTransMatrix(texture.X,
                                               texture.Y,
                                               texture.Width,
                                               texture.Height,
                                               1,
                                               0);

            _shaderProgram.SetTintColor(Color.FromArgb(0, 0, 0, 0));
            _shaderProgram.SetTransformationMatrix(transMatrix);

            texture.Bind();
            _shaderProgram.UseProgram();
            texture.VA.Bind();//Bind and unbind this inside of the texture.Bind()?

            GL.DrawElements(PrimitiveType.Triangles, texture.TotalIndices, DrawElementsType.UnsignedInt, 0);

            texture.Unbind();
        }

        private void SetupShader()
        {
            _shaderProgram.UseProgram();

            // Because there is 5 floats between the start of the first vertex and the start of the second,
            // we set this to 5 * sizeof(float).
            // This will now pass the new vertex array to the buffer.
            SetupVertexShaderAttribute(_shaderProgram, "aPosition", 3, 0);

            // Next, we also setup texture coordinates. It works in much the same way.
            // We add an offset of 3, since the first vertex coordinate comes after the first vertex
            // and change the amount of data to 2 because there's only 2 floats for vertex coordinates
            SetupVertexShaderAttribute(_shaderProgram, "aTexCoord", 2, 3 * sizeof(float));

        }


        /// <summary>
        /// Sets up the vertex shader attribute.
        /// </summary>
        /// <param name="shaderProgram">The shader program that contains the vertex shader to setup.</param>
        /// <param name="attrName">The name of the vertex shader attribute to setup.</param>
        /// <param name="size">The size/stride of the vertex bufferdata.</param>
        /// <param name="offSet">The offset of the vertex buffer data.</param>
        private void SetupVertexShaderAttribute(ShaderProgram shaderProgram, string attrName, int size, int offSet)
        {
            var vertexLocation = GL.GetAttribLocation(shaderProgram.ProgramId, attrName);
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, size, VertexAttribPointerType.Float, false, 5 * sizeof(float), offSet);
        }


        private Matrix4 BuildTransMatrix(float x, float y, int width, int height, float size, float angle)
        {
            var scaleX = (float)width / _renderSurfaceWidth;
            var scaleY = (float)height / _renderSurfaceHeight;

            scaleX *= 1f;
            scaleY *= 1f;

            var ndcX = x.MapValue(0f, _renderSurfaceWidth, -1f, 1f);
            var ndcY = y.MapValue(0f, _renderSurfaceHeight, 1f, -1f);

            var scaleMatrix = Matrix4.CreateScale(scaleX, scaleY, 1f);
            var positionMatrix = Matrix4.CreateTranslation(new Vector3(ndcX, ndcY, 0));

            return scaleMatrix * positionMatrix;
        }
    }
}
