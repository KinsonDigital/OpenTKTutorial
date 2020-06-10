using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;

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
        private readonly GPU _gpu = GPU.Instance;
        private QuadData _quadData;
        private static int _totalSingleVertexBytes = VertexDataAnalyzer.GetTotalBytesForStruct(typeof(VertexData));
        private static int _totalQuadBytes = _totalSingleVertexBytes * 4;
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

            GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
            GL.ObjectLabel(ObjectLabelIdentifier.Buffer, ID, -1, "VertexBuffer");

            AllocateVertexBufferMemory(vertexData);

            _quadData = new QuadData()
            {
                Vertex1 = new VertexData()
                {
                    Vertex = new Vector3(-1, 1, 0),//Top Left
                    TextureCoord = new Vector2(0, 1),
                },
                Vertex2 = new VertexData()
                {
                    Vertex = new Vector3(1, 1, 0),//Top Right
                    TextureCoord = new Vector2(1, 1),
                },
                Vertex3 = new VertexData()
                {
                    Vertex = new Vector3(1, -1, 0),//Bottom Right
                    TextureCoord = new Vector2(1, 0),
                },
                Vertex4 = new VertexData()
                {
                    Vertex = new Vector3(-1, -1, 0),//Bottom Left
                    TextureCoord = new Vector2(0, 0),
                }
            };
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
        public void UpdateTintColor(int textureSlot, Vector4 tintColor)
        {
            var quadByteStart = _totalQuadBytes * textureSlot;

            _quadData.Vertex1.TextureSlot = textureSlot;
            _quadData.Vertex2.TextureSlot = textureSlot;
            _quadData.Vertex3.TextureSlot = textureSlot;
            _quadData.Vertex4.TextureSlot = textureSlot;

            _quadData.Vertex1.TintColor = tintColor;
            _quadData.Vertex2.TintColor = tintColor;
            _quadData.Vertex3.TintColor = tintColor;
            _quadData.Vertex4.TintColor = tintColor;


            GL.BufferSubData(BufferTarget.ArrayBuffer, new IntPtr(quadByteStart), _totalQuadBytes, ref _quadData);
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

            //2 quads of data. which is 8 vertices
            var bytesPerVertex = VertexDataAnalyzer.GetTotalBytesForStruct(vertexData[0].GetType());
            var bytesPerQuad = bytesPerVertex * 4;

            GL.BufferData(BufferTarget.ArrayBuffer, bytesPerQuad * _gpu.TotalTextureSlots, vertexData, BufferUsageHint.DynamicDraw);

            Unbind();
        }
        #endregion
    }
}
