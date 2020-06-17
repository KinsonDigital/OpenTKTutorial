using OpenToolkit.Graphics.OpenGL4;

namespace OpenTKTutorial
{
    public class AtlasTexture : Texture
    {
        #region Constructors
        public AtlasTexture(byte[] pixelData, int width, int height, string name)
            : base(pixelData, width, height, name) { }
        #endregion


        #region Props
        public int ID { get; private set; }


        public int Width { get; private set; }


        public int Height { get; private set; }
        #endregion


        public override void Bind()
        {
            GL.ActiveTexture(TextureUnit.Texture1);
            base.Bind();
        }


        public override void Unbind()
        {
        }
    }
}
