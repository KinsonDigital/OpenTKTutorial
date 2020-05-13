using System;
using OpenToolkit.Graphics.OpenGL4;

namespace OpenTKTutorial
{
    /// <summary>
    /// The index buffer used describe the layout of a <see cref="VertexBuffer"/>.
    /// </summary>
    public class IndexBuffer : IDisposable
    {
        #region Private Fields
        private int _bufferId;
        private bool _isBound = false;//BindTexture is expensive.  This prevents the call if it is already bound
        private bool _disposedValue = false;
        #endregion


        #region Constructors
        /// <summary>
        /// Creates a new instance of <see cref="IndexBuffer"/>.
        /// </summary>
        /// <param name="data">The index buffer data.</param>
        public IndexBuffer(uint[] data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data), "The param must not be null");

            Count = data.Length;
            _bufferId = GL.GenBuffer();
            Bind();
            GL.BufferData(BufferTarget.ElementArrayBuffer, data.Length * sizeof(uint), data, BufferUsageHint.StaticDraw);
        }
        #endregion


        #region Props
        /// <summary>
        /// The total number of indexes in the buffer.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// The ID of the <see cref="IndexBuffer"/>.
        /// </summary>
        public int BufferID => _bufferId;
        #endregion


        #region Public Methods
        /// <summary>
        /// Binds the <see cref="IndexBuffer"/>.
        /// </summary>
        public void Bind()
        {
            if (_isBound)
                return;

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _bufferId);
            
            _isBound = true;
        }


        /// <summary>
        /// Unbinds the <see cref="IndexBuffer"/>.
        /// </summary>
        public void Unbind()
        {
            if (!_isBound)
                return;

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            _isBound = false;
        }


        /// <summary>
        /// Disposes of <see cref="IndexBuffer"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Cleans up unmanaged resources.
        /// </summary>
        ~IndexBuffer() => Dispose(false);
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
            Unbind();
            GL.DeleteBuffers(1, ref _bufferId);

            _disposedValue = true;
        }
        #endregion
    }
}
