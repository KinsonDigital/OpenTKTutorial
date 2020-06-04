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
        #endregion


        #region Constructors
        /// <summary>
        /// Creates a new instance of <see cref="VertexBuffer"/>.
        /// </summary>
        /// <param name="gl">Provides access to OpenGL funtionality.</param>
        /// <param name="vertexData">The vertex data to send to the GPU.</param>
        public VertexBuffer(T[] vertexData)
        {
            if (vertexData is null)
                throw new ArgumentNullException(nameof(vertexData), "The param must not be null");

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


        public void UpdateTintColor(int textureSlot, Color tintColor)
        {
            var totalVertexBytes = VertexDataAnalyzer.GetTotalBytesForStruct(typeof(VertexData));

            var tintColorByteStart = VertexDataAnalyzer.GetVertexSubDataOffset(typeof(VertexData), nameof(VertexData.TintColor));

            var glTintColor = tintColor.ToGLColor();

            //Vert 1
            var offset = (textureSlot * totalVertexBytes) + tintColorByteStart;
            GL.BufferSubData(BufferTarget.ArrayBuffer, new IntPtr(offset), 4 * sizeof(float), ref glTintColor);

            //Vert 2
            offset = ((textureSlot + 1) * totalVertexBytes) + tintColorByteStart;
            GL.BufferSubData(BufferTarget.ArrayBuffer, new IntPtr(offset), 4 * sizeof(float), ref glTintColor);

            //Vert 3
            offset = ((textureSlot + 2) * totalVertexBytes) + tintColorByteStart;
            GL.BufferSubData(BufferTarget.ArrayBuffer, new IntPtr(offset), 4 * sizeof(float), ref glTintColor);

            //Vert 4
            offset = ((textureSlot + 3) * totalVertexBytes) + tintColorByteStart;
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
        /// <param name="data">The data to upload.</param>
        [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        private void AllocateVertexBufferMemory(T[] data)
        {
            /*TODO: 
             * To do this, you use the GL call below like this:
             *      GL.BufferData(BufferTarget.ArrayBuffer, <total-size-in-bites-here>, null, BufferUsageHint.DynamicDraw);
             *      
             * This would have to using this GL call below only a single time for allocating the data on the GPU
             * that is enough memory for the total number of quads that matches the total number of texture slots
             * supported.
             */
            Bind();

            //NOTE: For right now, hard code enough memory to have 2 quads worth of data.
            //The idea is that eventually this will be dynamic in the renderer depending on
            //how many texture slots the GPU can handle

            //2 quads of data. which is 8 vertices
            var size = VertexDataAnalyzer.GetTotalBytesForStruct(data[0].GetType()) * data.Length;

            //new way
            //GL.BufferData(BufferTarget.ArrayBuffer, size, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            //old way
            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * (sizeof(float) * 10), data, BufferUsageHint.DynamicDraw);
            Unbind();
        }
        #endregion
    }
}
