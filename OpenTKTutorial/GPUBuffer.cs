using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;

namespace OpenTKTutorial
{
    public class GPUBuffer
    {
        private int _vertexBufferID = -1;
        private int _indexBufferID;
        private int _vertexArrayID;


        public GPUBuffer(int totalQuads)
        {
            CreateVertexBuffer(totalQuads);
            CreateIndexBuffer(totalQuads);

            _vertexArrayID = GL.GenVertexArray();

            //Bind the buffers to setup the attrib pointers
            BindVertexArray();
            BindVertexBuffer();
            BindIndexBuffer();

            SetupAttribPointers(_vertexArrayID, typeof(VertexData));
        }


        private void SetupAttribPointers<T>(int vertexArrayID, T structType) where T : Type
        {
            var props = structType.GetFields();

            /*TODO:
             * 3. Possibly use an custom attribute to set the shader location of a field
             * 4. Need to check if type is a struct and throw exception if it is not
             */

            var offset = 0;

            for (int i = 0; i < props.Length; i++)
            {
                var stride = VertexDataAnalyzer.GetTotalBytesForStruct(structType);
                var size = VertexDataAnalyzer.TotalItemsForType(props[i].FieldType);
                var attribType = VertexDataAnalyzer.GetVertexPointerType(props[i].FieldType);

                GL.EnableVertexArrayAttrib(vertexArrayID, i);

                offset = i == 0 ? 0 : offset + (size + 1) * VertexDataAnalyzer.GetTypeByteSize(typeof(float));
                GL.VertexAttribPointer(i, size, attribType, false, stride, offset);
            }
        }


        #region Public Methods
        public void UpdateQuad(int quadID, Rectangle srcRect, int textureWidth, int textureHeight)
        {
            //TODO: Condense/improve this to get a perf boost

            //TODO: Cache this value to avoid reflection for perf boost
            var totalVertexBytes = VertexDataAnalyzer.GetTotalBytesForStruct(typeof(VertexData));
            var totalQuadSizeInBytes = totalVertexBytes * 4;

            var quadData = CreateQuad();

            UpdateTextureCoordinates(ref quadData, srcRect, textureWidth, textureHeight);

            quadData.Vertex1.TransformIndex = quadID;
            quadData.Vertex2.TransformIndex = quadID;
            quadData.Vertex3.TransformIndex = quadID;
            quadData.Vertex4.TransformIndex = quadID;

            var offset = totalQuadSizeInBytes * quadID;
            GL.BufferSubData(BufferTarget.ArrayBuffer, new IntPtr(offset), totalQuadSizeInBytes, ref quadData);
        }
        #endregion


        #region Private Methods
        //GOOD
        private void UpdateTextureCoordinates(ref QuadData quad, Rectangle srcRect, int textureWidth, int textureHeight)
        {
            //TODO: Cache this value to avoid reflection for perf boost
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


            quad.Vertex1.TextureCoord = topLeftCoord;
            quad.Vertex2.TextureCoord = topRightCoord;
            quad.Vertex3.TextureCoord = bottomRightCoord;
            quad.Vertex4.TextureCoord = bottomLeftCoord;
        }


        private static Vector2 ToGLTextureCoord(int x, int y, int width, int height)
        {
            return new Vector2(x.MapValue(0, width, 0, 1), y.MapValue(0, height, 1, 0));
        }


        private void CreateVertexBuffer(int totalQuads)
        {
            _vertexBufferID = GL.GenBuffer();

            var quadData = new List<QuadData>();

            for (int i = 0; i < totalQuads; i++)
            {
                quadData.Add(CreateQuad());
            }

            UploadQuadData(quadData.ToArray());
        }


        private void CreateIndexBuffer(int totalQuads)
        {
            _indexBufferID = GL.GenBuffer();

            var indexData = new List<uint>();
            
            for (uint i = 0; i < totalQuads; i++)
            {
                var maxIndex = indexData.Count <= 0 ? 0 : indexData.Max() + 1;

                indexData.AddRange(new uint[]
                {
                    maxIndex,
                    maxIndex + 1,
                    maxIndex + 3,
                    maxIndex + 1,
                    maxIndex + 2,
                    maxIndex + 3
                });
            }


            UploadIndexBufferData(indexData.ToArray());
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


        private void UploadQuadData(QuadData[] data)
        {
            var totalVertexBytes = VertexDataAnalyzer.GetTotalBytesForStruct(typeof(VertexData));
            var totalQuadSizeInBytes = totalVertexBytes * 4;
            var dataTotal = totalQuadSizeInBytes * data.Length;


            var verticeData = new List<VertexData>();

            foreach (var vertice in data)
            {
                verticeData.AddRange(vertice.Vertices);
            }

            BindVertexBuffer();
            GL.BufferData(BufferTarget.ArrayBuffer, dataTotal, verticeData.ToArray(), BufferUsageHint.DynamicDraw);
            UnbindVertexBuffer();
        }


        private void BindVertexBuffer() => GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferID);


        private void UnbindVertexBuffer() => GL.BindBuffer(BufferTarget.ArrayBuffer, 0);


        /// <summary>
        /// Uploads the given <paramref name="data"/> to the GPU.
        /// </summary>
        /// <param name="data">The data to upload.</param>
        private void UploadIndexBufferData(uint[] data)
        {
            BindIndexBuffer();

            GL.BufferData(
                BufferTarget.ElementArrayBuffer,
                data.Length * sizeof(uint),
                data,
                BufferUsageHint.DynamicDraw);

            UnbindIndexBuffer();
        }


        private void BindIndexBuffer() => GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBufferID);


        private void UnbindIndexBuffer() => GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);


        private void BindVertexArray() => GL.BindVertexArray(_vertexArrayID);
        #endregion
    }
}
