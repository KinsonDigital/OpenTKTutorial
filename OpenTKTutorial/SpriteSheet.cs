using System.Drawing;

namespace OpenTKTutorial
{
    public class SpriteSheet
    {
        public SpriteSheet(string textureAtlasFile, string atlasDataFile)
        {
            TextureAtlas = ContentLoader.CreateTexture(textureAtlasFile);
            SubTextures = ContentLoader.LoadAtlasData(atlasDataFile);
        }


        public Rectangle[] SubTextures { get; private set; }


        public ITexture TextureAtlas { get; private set; }
    }
}
