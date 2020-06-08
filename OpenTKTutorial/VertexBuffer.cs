using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using OpenToolkit.Graphics.OpenGL4;

namespace OpenTKTutorial
{
    /// <summary>
    /// A vertex buffer object used to hold and describe data for a the GLSL shader program.
    /// </summary>
    public class VertexBuffer<T> : IDisposable where T : struct
    {
        #region Private Fields
        private static readonly List<int> _boundBuffers = new List<int>();
        private bool _disposedValue = false;
        private int _totalTextureSlots = 0;
        #endregion


        #region Constructors
        /// <summary>
        /// Creates a new instance of <see cref="VertexBuffer"/>.
        /// </summary>
        /// <param name="gl">Provides access to OpenGL funtionality.</param>
        /// <param name="vertexData">The vertex data to send to the GPU.</param>
        public VertexBuffer(T[] vertexData, int totalTextureSlots)
        {
            if (vertexData is null)
                throw new ArgumentNullException(nameof(vertexData), "The param must not be null");

            _totalTextureSlots = totalTextureSlots;
            ID = GL.GenBuffer();

            AllocateVertexBufferMemory(vertexData);
        }
        #endregion


        #region Props
        /// <summary>
        /// Gets the ID of the <see cref="VertexBuffer"/>.
        /// </summary>
        public int ID { get; private set; }
        #endregion


        #region Public Methods
        /// <summary>
        /// Binds ths <see cref="VertexBuffer"/>.
        /// </summary>
        public void Bind()
        {
            if (_boundBuffers.Contains(ID))
                return;

            GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
            _boundBuffers.Add(ID);
        }


        /// <summary>
        /// Unbinds the <see cref="VertexBuffer"/>.
        /// </summary>
        public void Unbind()
        {
            if (!_boundBuffers.Contains(ID))
                return;

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            _boundBuffers.Remove(ID);
        }


        /// <summary>
        /// Updates the tint color on the GPU for the given texture slot.
        /// </summary>
        /// <param name="textureSlot">The texture slot to apply the color to.</param>
        /// <param name="tintColor">The color to apply.</param>
        public void UpdateTintColor(int textureSlot, Color tintColor)
        {
            var quadByteStart = textureSlot == 0
                ? 0
                : _totalSingleVertexBytes * 4;

            var glTintColor = tintColor.ToGLColor();

            //Vert 1
            var offset = quadByteStart + _tintColorByteStart;
            GL.BufferSubData(BufferTarget.ArrayBuffer, new IntPtr(offset), 4 * sizeof(float), ref glTintColor);

            //Vert 2
            offset = quadByteStart + (_totalSingleVertexBytes * 1) + _tintColorByteStart;
            GL.BufferSubData(BufferTarget.ArrayBuffer, new IntPtr(offset), 4 * sizeof(float), ref glTintColor);

            //Vert 3
            offset = quadByteStart + (_totalSingleVertexBytes * 2) + _tintColorByteStart;
            GL.BufferSubData(BufferTarget.ArrayBuffer, new IntPtr(offset), 4 * sizeof(float), ref glTintColor);

            //Vert 4
            offset = quadByteStart + (_totalSingleVertexBytes * 3) + _tintColorByteStart;
            GL.BufferSubData(BufferTarget.ArrayBuffer, new IntPtr(offset), 4 * sizeof(float), ref glTintColor);
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

            var id = ID;
            GL.DeleteBuffers(1, ref id);

            _disposedValue = true;
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// Allocates the required amount of GPU memory to hold the vertex buffer data.
        /// </summary>
        /// <param name="vertexData">The data to upload.</param>
        [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        private void AllocateVertexBufferMemory(T[] vertexData)
        {
            Bind();

            //NOTE: For right now, hard code enough memory to have 2 quads worth of data.
            //The idea is that eventually this will be dynamic in the renderer depending on
            //how many texture slots the GPU can handle

            //2 quads of data. which is 8 vertices
            var totalQuadBytes = VertexDataAnalyzer.GetTotalBytesForStruct(vertexData[0].GetType()) * vertexData.Length;

            GL.BufferData(BufferTarget.ArrayBuffer, _totalTextureSlots * totalQuadBytes, vertexData, BufferUsageHint.DynamicDraw);

            Unbind();
        }
        #endregion
    }
}
