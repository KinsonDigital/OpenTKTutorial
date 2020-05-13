using System;
using OpenToolkit.Graphics.OpenGL4;

namespace OpenTKTutorial
{
    public class VertexArray : IDisposable
    {
        #region Private Fields
        private bool _isBound = false;//BindTexture is expensive.  This prevents the call if it is already bound
        private bool _disposedValue = false;
        #endregion


        #region Constructors
        /// <summary>
        /// Creates a new instance of <see cref="VertexArray"/>.
        /// </summary>
        /// <param name="vb">The <see cref="VertexBuffer"/> and <see cref="IndexBuffer"/> required to create the <see cref="VertextArray"/>.</param>
        /// <param name="ib">The <see cref="IndexBuffer"/> that describes the layout of the <see cref="VertextBuffer"/> for the <see cref="VertexArray"/>.</param>
        public VertexArray(VertexBuffer vb, IndexBuffer ib)
        {
            if (vb is null)
                throw new ArgumentNullException(nameof(vb), "The param must not be null");

            if (ib is null)
                throw new ArgumentNullException(nameof(ib), "The param must not be null");

            VertexArrayID = GL.GenVertexArray();

            Bind();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vb.BufferID);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ib.BufferID);
        }
        #endregion


        #region Props
        /// <summary>
        /// Gets the ID of the vertex array on the GPU.
        /// </summary>
        public int VertexArrayID { get; }
        #endregion


        #region Public Methods
        /// <summary>
        /// Binds the <see cref="VertexArray"/>.
        /// </summary>
        public void Bind()
        {
            if (_isBound)
                return;

            GL.BindVertexArray(VertexArrayID);

            _isBound = false;
        }


        /// <summary>
        /// Disposes of the <see cref="VertexArray"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Cleans up unmanaged resources.
        /// </summary>
        ~VertexArray() => Dispose(false);
        #endregion


        #region Protected Methods
        /// <summary>
        /// Disposes of the internal resources if the given <paramref name="disposing"/> value is true.
        /// </summary>
        /// <param name="disposing">True to dispose of internal resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue)
                return;

            //Clean up unmanaged resources
            GL.DeleteVertexArray(VertexArrayID);

            _disposedValue = true;
        }
        #endregion
    }
}
