using System;
using System.Diagnostics.CodeAnalysis;
using OpenToolkit.Graphics.OpenGL4;

namespace OpenTKTutorial
{
    /// <summary>
    /// A vertex buffer object used to hold and describe data for a the GLSL shader program.
    /// </summary>
    public class VertexBuffer<T> : IDisposable where T : struct
    {
        #region Private Fields
        //TODO:  Need to create a static list of bound buffers. This will allow  the ability
        //to keep track if the buffer for a particular instance is bound
        private int _id;
        private bool _isBound = false;//BindTexture is expensive.  This prevents the call if it is already bound
        private bool _disposedValue = false;
        #endregion


        #region Constructors
        /// <summary>
        /// Creates a new instance of <see cref="VertexBuffer"/>.
        /// </summary>
        /// <param name="gl">Provides access to OpenGL funtionality.</param>
        /// <param name="data">The vertex data to send to the GPU.</param>
        public VertexBuffer(T[] data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data), "The param must not be null");

            _id = GL.GenBuffer();

            SetLayout(data);
        }
        #endregion


        #region Props
        /// <summary>
        /// Gets the ID of the <see cref="VertexBuffer"/>.
        /// </summary>
        public int ID => _id;
        #endregion


        #region Public Methods
        /// <summary>
        /// Binds ths <see cref="VertexBuffer"/>.
        /// </summary>
        public void Bind()
        {
            if (_isBound)
                return;

            GL.BindBuffer(BufferTarget.ArrayBuffer, ID);

            _isBound = true;
        }


        /// <summary>
        /// Unbinds the <see cref="VertexBuffer"/>.
        /// </summary>
        public void Unbind()
        {
            if (!_isBound)
                return;

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            _isBound = false;
        }


        /// <summary>
        /// Disposes of the <see cref="VertexBuffer"/>.
        /// </summary>
        [SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "<Pending>")]
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
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
            GL.DeleteBuffers(1, ref _id);

            _disposedValue = true;
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// Sets up the data layout of the <see cref="VertexBuffer"/> on the GPU.
        /// </summary>
        /// <param name="data">The data to goto the GPU.</param>
        [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        private void SetLayout(T[] data)
        {
            Bind();
            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, BufferUsageHint.DynamicDraw);
            Unbind();
        }
        #endregion
    }
}
