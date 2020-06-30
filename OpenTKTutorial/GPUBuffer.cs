// <copyright file="GPUBuffer.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace OpenTKTutorial
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Numerics;
    using Silk.NET.OpenGL;

    public class GPUBuffer<T>
        where T : struct
    {
        private readonly uint vertexArrayID = 0;
        private uint vertexBufferID = 0;
        private uint indexBufferID = 0;
        private uint totalVertexBytes;
        private uint totalQuadSizeInBytes;
        private GL GL;

        public GPUBuffer(GL gl, int totalQuads)
        {
            this.GL = gl;

            CreateVertexBuffer(totalQuads);
            CreateIndexBuffer(totalQuads);

            this.vertexArrayID = this.GL.GenVertexArray();

            // Bind the buffers to setup the attrib pointers
            BindVertexArray();
            BindVertexBuffer();
            BindIndexBuffer();

            SetupAttribPointers(this.vertexArrayID);
        }

        public unsafe void UpdateQuad(int quadID, Rectangle srcRect, int textureWidth, int textureHeight, Color tintColor)
        {
            var quadData = CreateQuad();

            UpdateTextureCoordinates(ref quadData, srcRect, textureWidth, textureHeight);

            // Update the color
            quadData.Vertex1.TintColor = tintColor.ToGLColor();
            quadData.Vertex2.TintColor = tintColor.ToGLColor();
            quadData.Vertex3.TintColor = tintColor.ToGLColor();
            quadData.Vertex4.TintColor = tintColor.ToGLColor();

            // Update the quad ID
            quadData.Vertex1.TransformIndex = quadID;
            quadData.Vertex2.TransformIndex = quadID;
            quadData.Vertex3.TransformIndex = quadID;
            quadData.Vertex4.TransformIndex = quadID;

            var offset = (int)(this.totalQuadSizeInBytes * quadID);
            this.GL.BufferSubData(BufferTargetARB.ArrayBuffer, offset, this.totalQuadSizeInBytes, &quadData);
        }

        private static QuadData CreateQuad() => new QuadData
        {
            Vertex1 = new VertexData()
            {
                Vertex = new Vector3(-1, 1, 0), // Top Left
                TextureCoord = new Vector2(0, 1),
            },

            Vertex2 = new VertexData()
            {
                Vertex = new Vector3(1, 1, 0), // Top Right
                TextureCoord = new Vector2(1, 1),
            },

            Vertex3 = new VertexData()
            {
                Vertex = new Vector3(1, -1, 0), // Bottom Right
                TextureCoord = new Vector2(1, 0),
            },

            Vertex4 = new VertexData()
            {
                Vertex = new Vector3(-1, -1, 0), // Bottom Left
                TextureCoord = new Vector2(0, 0),
            },
        };

        private static void UpdateTextureCoordinates(ref QuadData quad, Rectangle srcRect, int textureWidth, int textureHeight)
        {
            // TODO: Cache this value to avoid reflection for perf boost
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

        private void UnbindIndexBuffer() => this.GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);

        private void UnbindVertexBuffer() => this.GL.BindBuffer(BufferTargetARB.ArrayBuffer, 0);

        private unsafe void SetupAttribPointers(uint vertexArrayID)
        {
            var props = typeof(T).GetFields();

            /*TODO:
             * 3. Possibly use an custom attribute to set the shader location of a field
             * 4. Need to check if type is a struct and throw exception if it is not
             */

            var offset = 0u;
            var previousSize = 0u; // The element size of the previous field

            for (uint i = 0; i < props.Length; i++)
            {
                var stride = this.totalVertexBytes;

                // The number of float elements in the field. Ex: Vector3 has a size of 3
                var size = VertexDataAnalyzer.TotalItemsForType(props[i].FieldType);

                // The type of OpenGL VertexAttribPointerType based on the field type
                var attribType = VertexDataAnalyzer.GetVertexPointerType(props[i].FieldType);

                this.GL.EnableVertexArrayAttrib(vertexArrayID, i);

                offset = i == 0 ? 0 : offset + (previousSize * VertexDataAnalyzer.GetTypeByteSize(typeof(float)));
                this.GL.VertexAttribPointer(i, (int)size, attribType, false, stride, &offset);

                previousSize = size;
            }
        }

        private void CreateVertexBuffer(int totalQuads)
        {
            this.totalVertexBytes = VertexDataAnalyzer.GetTotalBytesForStruct(typeof(T));
            this.totalQuadSizeInBytes = this.totalVertexBytes * 4;

            this.vertexBufferID = this.GL.GenBuffer();

            var quadData = new List<QuadData>();

            for (var i = 0; i < totalQuads; i++)
            {
                quadData.Add(CreateQuad());
            }

            AllocateBuffer(quadData.ToArray());
        }

        private void CreateIndexBuffer(int totalQuads)
        {
            this.indexBufferID = this.GL.GenBuffer();

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
                    maxIndex + 3,
                });
            }

            UploadIndexBufferData(indexData.ToArray());
        }

        private unsafe void AllocateBuffer(QuadData[] data)
        {
            var sizeInBytes = (uint)(this.totalQuadSizeInBytes * data.Length);

            BindVertexBuffer();
            this.GL.BufferData(BufferTargetARB.ArrayBuffer, sizeInBytes, IntPtr.Zero.ToPointer(), BufferUsageARB.DynamicDraw);
            UnbindVertexBuffer();
        }

        private void BindVertexBuffer() => this.GL.BindBuffer(BufferTargetARB.ArrayBuffer, this.vertexBufferID);

        /// <summary>
        /// Uploads the given <paramref name="data"/> to the GPU.
        /// </summary>
        /// <param name="data">The data to upload.</param>
        private unsafe void UploadIndexBufferData(uint[] data)
        {
            BindIndexBuffer();

            var gpuData = new Span<uint>(data);

            this.GL.BufferData(
                BufferTargetARB.ElementArrayBuffer,
                (uint)data.Length * sizeof(uint),
                gpuData,
                BufferUsageARB.DynamicDraw);

            UnbindIndexBuffer();
        }

        private void BindIndexBuffer() => this.GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, this.indexBufferID);

        private void BindVertexArray() => this.GL.BindVertexArray(this.vertexArrayID);
    }
}
