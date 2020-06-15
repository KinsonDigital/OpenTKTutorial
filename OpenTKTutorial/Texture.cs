using System;
using System.Collections.Generic;
using System.IO;
using OpenToolkit.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using NETColor = System.Drawing.Color;

namespace OpenTKTutorial
{
    public class Texture : IDisposable
    {
        private bool _disposedValue = false;
        private float _angle;
        private bool _textureUnit0NotBound = true;


        #region Constructors
        public Texture(string texturePath)
        {
            ID = GL.GenTexture();

            //NOTE: Some GPU's automatically default to texture unit 0 but
            //some do not.  This is just in case a GPU does not
            if (_textureUnit0NotBound)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                _textureUnit0NotBound = true;
            }

            Bind();

            var (pixelData, width, height) = LoadImageData(texturePath);

            Width = width;
            Height = height;

            UploadDataToGPU(pixelData, width, height, Path.GetFileName(texturePath));

            Unbind();
        }


        public Texture(byte[] pixelData, int width, int height, string name)
        {
            ID = GL.GenTexture();

            //NOTE: Some GPU's automatically default to texture unit 0 but
            //some do not.  This is just in case a GPU does not
            if (_textureUnit0NotBound)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                _textureUnit0NotBound = true;
            }

            Bind();

            Width = width;
            Height = height;

            UploadDataToGPU(pixelData, width, height, name);

            Unbind();
        }
        #endregion


        #region Props
        public int ID { get; private set; }

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
        #endregion


        #region Public Methods
        /// <summary>
        /// Bind the texture for performing operations on it.
        /// </summary>
        public void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2D, ID);
        }


        /// <summary>
        /// Unbind the texture.
        /// </summary>
        public void Unbind()
        {
            //GL.BindTexture(TextureTarget.Texture2D, 0);
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

            //NOTE: Finalizers cannot call this method and then invoke GL calls.
            //GL calls are not on the same thread as the finalizer and they will not work.
            //To avoid this problem, you have to make sure that all dispose methods are called
            //manually for anything using these objects where they contain GL calls in there
            //Dispose() methods
            Unbind();
            GL.DeleteTexture(ID);

            _disposedValue = true;
        }
        #endregion


        #region Private Methods
        private (byte[] pixelData, int width, int height) LoadImageData(string texturePath)
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


            return (pixels.ToArray(), image.Width, image.Height);
        }


        private void UploadDataToGPU(byte[] pixelData, int width, int height, string name)
        {
            GL.ObjectLabel(ObjectLabelIdentifier.Texture, ID, -1, Path.GetFileName(name));

            //Set the min and mag filters to linear
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            //Sett the x(S) and y(T) axis wrap mode to repeat
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            //Load the texture data to the GPU for the currently active texture slot
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixelData);
        }
        #endregion
    }
}
