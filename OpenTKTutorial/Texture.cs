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
        private static readonly List<int> _boundTextures = new List<int>();
        private bool _disposedValue = false;
        private float _angle;
        private GPU _gpu = GPU.Instance;


        #region Constructors
        public Texture(string texturePath, int shaderProgramID)
        {
            ID = GL.GenTexture();

            Bind();

            var (pixelData, width, height) = LoadImageData(texturePath);

            UploadDataToGPU(pixelData, width, height, Path.GetFileName(texturePath));

            Width = width;
            Height = height;

            Unbind();
        }


        public Texture(byte[] pixelData, int width, int height, string name)
        {
            ID = GL.GenTexture();

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
            if (_boundTextures.Contains(ID))
                return;

            //TODO: Need to measure the performance of the
            /*ActiveTexture and BindTexture calls. The reason for this performance measuring
                is because if you had 32 textures to render in 1 draw call, you would have to
                bind all 32 textures.  If the performance of this is poor, then texture batching
                is useless and it matters more to utilize sprite sheets.

                It also might be useful to come up with a way to batch many texture geometries
                to a single draw call by having a single texture but having the vertex buffer
                have many vertices for all of the geometry.  This might also mean that it would
                be better to have a different shader setup for this. This setup would be better
                for particle systems

            TODO: Cache the "textures" location for improved performance.
            */
            //GL.ActiveTexture(TextureUnit.Texture0 + TextureSlot);
            //GL.BindTexture(TextureTarget.Texture2D, ID);

            _boundTextures.Add(ID);
        }


        /// <summary>
        /// Unbind the texture.
        /// </summary>
        public void Unbind()
        {
            if (!_boundTextures.Contains(ID))
                return;

            GL.BindTexture(TextureTarget.Texture2D, 0);
            _boundTextures.Clear();
        }


        public void Dispose() => Dispose(true);
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
            GL.BindTexture(TextureTarget.Texture2D, ID);
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
