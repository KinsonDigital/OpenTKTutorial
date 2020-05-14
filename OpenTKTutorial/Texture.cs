using System;
using System.Collections.Generic;
using OpenToolkit.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using NETColor = System.Drawing.Color;

namespace OpenTKTutorial
{
    public class Texture : IDisposable
    {
        private readonly int _textureId;
        private readonly float[] _vertices = {
            //      Positions      Texture Coordinates
            //X       Y       Z         X     Y
            -1f,     1f,     0.0f,     0.0f, 1.0f, // top left
             1f,     1f,     0.0f,     1.0f, 1.0f, // top right
             1f,    -1f,     0.0f,     1.0f, 0.0f, // bottom right
            -1f,    -1f,     0.0f,     0.0f, 0.0f  // bottom left
        };
        private readonly uint[] _indices = {  // note that we start from 0!
            0, 1, 3,   // first triangle
            1, 2, 3    // second triangle
        };

        /*
            First Triangle(Indices 0, 1, 3:

    TopLeft(Indice 0)|---------/TopRight(Indice 1)
                     |        /
                     |       /
                     |      /
                     |     /
                     |    /
                     |   /
                     |  /
                     | /
                     |/
                     *BottomLeft(Indice 3)
                                    

            Second Triangle(Indices 1, 2, 3):

                                 /|TopRight(Indice 1)
                                / |
                               /  |
                              /   |
                             /    |
                            /     |
                           /      |
                          /       |
                         /        |
    BottomLeft(Indice 3)/_________|BottomRight(Indice 2)
         */
        private bool _isBound;
        private bool _disposedValue = false;
        private float _angle;
        private readonly VertexBuffer _vertexBuffer;
        private readonly IndexBuffer _indexBuffer;

        #region Constructors
        public Texture(string texturePath, ShaderProgram shader)
        {
            _textureId = GL.GenTexture();
            
            Bind();

            LoadTextureData(texturePath);

            Unbind();

            Use();

            _vertexBuffer = new VertexBuffer(_vertices);
            _indexBuffer = new IndexBuffer(_indices);
            VA = new VertexArray(_vertexBuffer, _indexBuffer);

            // Because there is 5 floats between the start of the first vertex and the start of the second,
            // we set this to 5 * sizeof(float).
            // This will now pass the new vertex array to the buffer.
            SetupVertexShaderAttribute(shader, "aPosition", 3, 0);

            // Next, we also setup texture coordinates. It works in much the same way.
            // We add an offset of 3, since the first vertex coordinate comes after the first vertex
            // and change the amount of data to 2 because there's only 2 floats for vertex coordinates
            SetupVertexShaderAttribute(shader, "aTexCoord", 2, 3 * sizeof(float));
        }
        #endregion


        #region Props
        public VertexArray VA { get; private set; }

        public float X { get; set; }

        public float Y { get; set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public float Size { get; set; } = 1f;

        /// <summary>
        /// Gets or sets the angle in degrees.
        /// </summary>
        public float Angle
        {
            get => _angle;
            set
            {
                if (_angle > 360)
                    _angle = 0;

                if (_angle < 0)
                    _angle = 360;

                _angle = value;
            }
        }

        public NETColor TintColor { get; set; } = NETColor.White;

        public int TotalIndices => _indices.Length;
        #endregion


        #region Public Methods
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


        public void Dispose()
        {
            Dispose(true);
        }
        #endregion


        #region Protected Methods
        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue)
                return;

            Unbind();
            GL.DeleteTexture(_textureId);

            _disposedValue = true;
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// Sets up the vertex shader attribute.
        /// </summary>
        /// <param name="shaderProgram">The shader program that contains the vertex shader to setup.</param>
        /// <param name="attrName">The name of the vertex shader attribute to setup.</param>
        /// <param name="size">The size/stride of the vertex bufferdata.</param>
        /// <param name="offSet">The offset of the vertex buffer data.</param>
        private void SetupVertexShaderAttribute(ShaderProgram shaderProgram, string attrName, int size, int offSet)
        {
            //TODO: This needs to be called for each VAO and each texture has its own VAO
            var vertexLocation = GL.GetAttribLocation(shaderProgram.ProgramId, attrName);
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, size, VertexAttribPointerType.Float, false, 5 * sizeof(float), offSet);
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
        #endregion
    }
}
