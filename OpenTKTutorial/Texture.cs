using System;
using System.Collections.Generic;
using OpenToolkit.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace OpenTKTutorial
{
    public class Texture
    {
        private int _textureId;
        float[] _vertices = {
            //Position              Texture coordinates
            //X     Y     Z  
           // 0.5f,  0.5f, 0.0f,     1.0f, 1.0f, // top right
           // 0.5f, -0.5f, 0.0f,     1.0f, 0.0f, // bottom right
           //-0.5f, -0.5f, 0.0f,     0.0f, 0.0f, // bottom left
           //-0.5f,  0.5f, 0.0f,     0.0f, 1.0f  // top left
           -1f,  1f, 0.0f,     0.0f, 1.0f, // top left
            1f,  1f, 0.0f,     1.0f, 1.0f, // top right
            1f, -1f, 0.0f,     1.0f, 0.0f, // bottom right
           -1f, -1f, 0.0f,     0.0f, 0.0f  // bottom left
        };
        uint[] _indices = {  // note that we start from 0!
            0, 1, 3,   // first triangle
            1, 2, 3    // second triangle
        };
        private bool _isBound;
        private readonly VertexBuffer _vertexBuffer;
        private readonly IndexBuffer _indexBuffer;

        public Texture(string texturePath)
        {
            _textureId = GL.GenTexture();
            
            Bind();

            LoadTextureData(texturePath);

            Unbind();

            Use();

            _vertexBuffer = new VertexBuffer(_vertices);
            _indexBuffer = new IndexBuffer(_indices);
            VA = new VertexArray(_vertexBuffer, _indexBuffer);
        }


        public VertexArray VA { get; private set; }

        public float X { get; set; }

        public float Y { get; set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public int TotalIndices => _indices.Length;

        public void Use()
        {
            GL.BindTexture(TextureTarget.Texture2D, _textureId);
        }


        /// <summary>
        /// Bind the texture for performing operations on it.
        /// </summary>
        public void Bind()
        {
            if (_isBound)
                return;

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _textureId);

            _isBound = true;
        }


        /// <summary>
        /// Unbind the texture.
        /// </summary>
        public void Unbind()
        {
            if (!_isBound)
                return;

            GL.BindTexture(TextureTarget.Texture2D, 0);

            _isBound = false;
        }


        private void LoadTextureData(string texturePath)
        {
            //Load the image
            var image = (Image<Rgba32>)Image.Load(texturePath);

            Width = image.Width;
            Height = image.Height;


            //ImageSharp loads from the top-left pixel, whereas OpenGL loads from the bottom-left, causing the texture to be flipped vertically.
            //This will correct that, making the texture display properly.
            image.Mutate(x => x.Flip(FlipMode.Vertical));

            //Get an array of the pixels, in ImageSharp's internal format.
            var tempPixels = new List<Rgba32>();

            for (int i = 0; i < image.Height; i++)
            {
                tempPixels.AddRange(image.GetPixelRowSpan(i).ToArray());
            }

            //Convert ImageSharp's format into a byte array, so we can use it with OpenGL.
            List<byte> pixels = new List<byte>();

            foreach (Rgba32 p in tempPixels)
            {
                pixels.Add(p.R);
                pixels.Add(p.G);
                pixels.Add(p.B);
                pixels.Add(p.A);
            }


            //Set the min and mag filters to linear
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            //Sett the x(S) and y(T) axis wrap mode to repeat
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            //Generate the texture
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.ToArray());
        }
    }
}
