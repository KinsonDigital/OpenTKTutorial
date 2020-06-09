using System;
using System.Collections.Generic;
using System.Linq;
using OpenToolkit.Graphics.OpenGL4;

namespace OpenTKTutorial
{
    /// <summary>
    /// The index buffer used describe the layout of a <see cref="VertexBuffer"/>.
    /// </summary>
    public class IndexBuffer : IDisposable
    {
        #region Private Fields
        //TODO:  Need to create a static list of bound buffers. This will allow  the ability
        //to keep track if the buffer for a particular instance is bound
        private readonly static List<int> _boundIDNumbers = new List<int>();
        private int _id;
        private bool _disposedValue = false;
        private readonly GPU _gpu = GPU.Instance;
        #endregion


        #region Constructors
        /// <summary>
        /// Creates a new instance of <see cref="IndexBuffer"/>.
        /// </summary>
        public IndexBuffer()
        {
            _id = GL.GenBuffer();

            UploadDataToGPU();
        }
        #endregion


        #region Props
        /// <summary>
        /// The ID of the <see cref="IndexBuffer"/>.
        /// </summary>
        public int ID => _id;
        #endregion


        #region Public Methods
        /// <summary>
        /// Binds the <see cref="IndexBuffer"/>.
        /// </summary>
        public void Bind()
        {
            //NOTE: Only one index buffer can be bound at a time
            if (_boundIDNumbers.Contains(_id))
                return;

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _id);
            _boundIDNumbers.Add(_id);
        }


        /// <summary>
        /// Unbinds the <see cref="IndexBuffer"/>.
        /// </summary>
        public void Unbind()
        {
            //If the buffer is already unbound
            if (!_boundIDNumbers.Contains(_id))
                return;

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            _boundIDNumbers.Remove(_id);
        }


        /// <summary>
        /// Disposes of <see cref="IndexBuffer"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// Uploads the index buffer data to the GPU.
        /// </summary>
        private void UploadDataToGPU()
        {
            var indexBufferData = CreateIndexBufferData();

            Bind();

            GL.BufferData(BufferTarget.ElementArrayBuffer, indexBufferData.Length * sizeof(uint), indexBufferData, BufferUsageHint.DynamicDraw);

            Unbind();
        }


        private uint[] CreateIndexBufferData()
        {
            /*Index Buffer Data Pattern
                0,  1,  3,  1,  2,  3,  //Quad 1
                4,  5,  7,  5,  6,  7,  //Quad 2
                8,  9,  11, 9,  10, 11, //Quad 3
                12, 13, 15, 13, 14, 15  //Quad 4
             */
            var result = new List<uint>();

            for (uint i = 0; i < _gpu.TotalTextureSlots; i++)
            {
                var maxIndex = result.Count <= 0 ? 0 : result.Max() + 1;

                result.AddRange(new uint[]
                {
                    maxIndex,
                    maxIndex + 1,
                    maxIndex + 3,
                    maxIndex + 1,
                    maxIndex + 2,
                    maxIndex + 3
                });
            }


            return result.ToArray();
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
    }
}
