using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using System;

namespace OpenTKTutorial
{
    public class Renderer : IDisposable
    {
        #region Private Fields
        private readonly int _renderSurfaceWidth;
        private readonly int _renderSurfaceHeight;
        private bool disposedValue = false;
        #endregion


        #region Constructors
        public Renderer(int renderSurfaceWidth, int renderSurfaceHeight)
        {
            Shader = new ShaderProgram("shader.vert", "shader.frag");

            _renderSurfaceWidth = renderSurfaceWidth;
            _renderSurfaceHeight = renderSurfaceHeight;

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }
        #endregion


        #region Props
        public ShaderProgram Shader { get; private set; }
        #endregion


        #region Public Methods
        public void Render(Texture texture)
        {
            var transMatrix = BuildTransMatrix(texture.X,
                                               texture.Y,
                                               texture.Width,
                                               texture.Height,
                                               texture.Size,
                                               texture.Angle);

            Shader.UseProgram();
            Shader.SetTintColor(texture.TintColor);
            Shader.SetTransformationMatrix(transMatrix);
             
            texture.Bind();
            texture.VA.Bind();

            GL.DrawElements(PrimitiveType.Triangles, texture.TotalIndices, DrawElementsType.UnsignedInt, 0);

            texture.Unbind();
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        ~Renderer() => Dispose(false);
        #endregion


        #region Private Methods
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
        private Matrix4 BuildTransMatrix(float x, float y, int width, int height, float size, float angle)
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
            if (!disposedValue)
            {
                if (disposing)
                    Shader.Dispose();


                disposedValue = true;
            }
        }
        #endregion
    }
}
