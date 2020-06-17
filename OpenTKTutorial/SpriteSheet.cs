using OpenToolkit.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace OpenTKTutorial
{
    public class SpriteSheet
    {
        public SpriteSheet(string textureAtlasFile, string atlasDataFile)
        {
            TextureAtlas = ContentLoader.CreateTexture(textureAtlasFile);
            SubTextures = ContentLoader.LoadAtlasData(atlasDataFile);
        }


        public SubTextureRect [] SubTextures { get; private set; }


        public ITexture TextureAtlas { get; private set; }
    }
}
