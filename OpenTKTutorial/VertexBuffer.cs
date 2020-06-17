using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
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
        private bool _disposedValue = false;
        #endregion


        #region Constructors
        /// <summary>
        /// Creates a new instance of <see cref="VertexBuffer"/>.
        /// </summary>
        /// <param name="totalQuads">The total number of quads to allocate in the data buffer.</param>
        public VertexBuffer(int totalQuads)
        {
            ID = GL.GenBuffer();

            var quadData = new List<QuadData>();

            for (int i = 0; i < totalQuads; i++)
            {
                quadData.Add(CreateQuad());
            }

            UploadQuadData(quadData.ToArray());
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
            GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
        }


        /// <summary>
        /// Unbinds the <see cref="VertexBuffer"/>.
        /// </summary>
        public void Unbind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }


        public void UpdateSrcRectangle(Rectangle srcRect, int textureWidth, int textureHeight)
        {
            //Map texture topleft origin and width and height to bottomleft corner
            var topLeftCornerX = srcRect.Left.MapValue(0, textureWidth, 0, 1);
            var topLeftCornerY = srcRect.Top.MapValue(0, textureHeight, 1, 0);
            var topLeftCoord = new Vector2(topLeftCornerX, topLeftCornerY);

            var topRightCornerX = srcRect.Right.MapValue(0, textureWidth, 0, 1);
            var topRightCornerY = srcRect.Top.MapValue(0, textureHeight, 1, 0);
            var topRightCoord = new Vector2(topRightCornerX, topRightCornerY);

            var bottomRightCornerX = srcRect.Right.MapValue(0, textureWidth, 0, 1);
            var bottomRightCornerY = srcRect.Bottom.MapValue(0, textureHeight, 1, 0);
            var bottomRightCoord = new Vector2(bottomRightCornerX, bottomRightCornerY);

            var bottomLeftCornerX = srcRect.Left.MapValue(0, textureWidth, 0, 1);
            var bottomLeftCornerY = srcRect.Bottom.MapValue(0, textureHeight, 1, 0);
            var bottomLeftCoord = new Vector2(bottomLeftCornerX, bottomLeftCornerY);

            var totalVertexBytes = VertexDataAnalyzer.GetTotalBytesForStruct(typeof(VertexData));
            var totalQuadSizeInBytes = totalVertexBytes * 4;

            var quadData = CreateQuad();
            quadData.Vertex1.TextureCoord = topLeftCoord;
            quadData.Vertex2.TextureCoord = topRightCoord;
            quadData.Vertex3.TextureCoord = bottomRightCoord;
            quadData.Vertex4.TextureCoord = bottomLeftCoord;

            GL.BufferSubData(BufferTarget.ArrayBuffer, new IntPtr(0), totalQuadSizeInBytes, ref quadData);
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
        private void UploadQuadData(QuadData[] data)
        {
            const int totalValuesPerVertice = 5;
            const int floatByteSize = sizeof(float);
            int totalVertices = data.Length * 4;
            int dataSizeInBytes = totalVertices * totalValuesPerVertice * floatByteSize;

            var verticeData = new List<VertexData>();

            foreach (var vertice in data)
            {
                verticeData.AddRange(vertice.Vertices);
            }

            Bind();
            GL.BufferData(BufferTarget.ArrayBuffer, dataSizeInBytes, verticeData.ToArray(), BufferUsageHint.StaticDraw);
            Unbind();
        }


        private QuadData CreateQuad()
        {
            return new QuadData
            {
                Vertex1 = new VertexData()
                {
                    Vertex = new Vector3(-1, 1, 0),//Top Left
                    TextureCoord = new Vector2(0, 1)
                },

                Vertex2 = new VertexData()
                {
                    Vertex = new Vector3(1, 1, 0),//Top Right
                    TextureCoord = new Vector2(1, 1)
                },

                Vertex3 = new VertexData()
                {
                    Vertex = new Vector3(1, -1, 0),//Bottom Right
                    TextureCoord = new Vector2(1, 0)
                },

                Vertex4 = new VertexData()
                {
                    Vertex = new Vector3(-1, -1, 0),//Bottom Left
                    TextureCoord = new Vector2(0, 0)
                }
            };
        }
        #endregion
    }
}
