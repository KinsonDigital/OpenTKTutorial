// <copyright file="Texture.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace OpenTKTutorial
{
    using System;
    using System.IO;
    using Silk.NET.OpenGL;

    public class Texture : ITexture
    {
        private bool disposedValue = false;
        private GL GL;

        public Texture(GL gl, byte[] pixelData, uint width, uint height, string name)
        {
            this.GL = gl;
            ID = this.GL.GenTexture();

            Bind();

            Width = width;
            Height = height;

            Name = Path.GetFileNameWithoutExtension(name);

            UploadDataToGPU(pixelData, width, height, name);

            Unbind();
        }

        public uint ID { get; protected set; }

        public string Name { get; private set; }

        public uint Width { get; protected set; }

        public uint Height { get; protected set; }

        public void Bind() => this.GL.BindTexture(TextureTarget.Texture2D, ID);

        public void Unbind()
        {
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        /// <param name="disposing">True if managed resources should be disposed of.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.disposedValue)
                return;

            // NOTE: Finalizers cannot call this method and then invoke GL calls.
            // GL calls are not on the same thread as the finalizer and they will not work.
            // To avoid this problem, you have to make sure that all dispose methods are called
            // manually for anything using these objects where they contain GL calls in there
            // Dispose() methods
            Unbind();
            this.GL.DeleteTexture(ID);

            this.disposedValue = true;
        }

        private unsafe void UploadDataToGPU(byte[] pixelData, uint width, uint height, string name)
        {
            this.GL.ObjectLabel(ObjectIdentifier.Texture, ID, 0, Path.GetFileName(name));

            // Set the min and mag filters to linear
            this.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            this.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            // Sett the x(S) and y(T) axis wrap mode to repeat
            this.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            this.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            // Load the texture data to the GPU for the currently active texture slot

            fixed (byte* pixelPtr = pixelData)
            {
                this.GL.TexImage2D(TextureTarget.Texture2D, 0, (int)PixelFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixelPtr);
            }
        }
    }
}
