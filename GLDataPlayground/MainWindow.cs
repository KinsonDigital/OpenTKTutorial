using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace GLDataPlayground
{
    public class MainWindow : GameWindow
    {
        private static int _vertexArrayID;
        private static int _vertexBufferID;
        private static int _indexBufferID;
        #region Private Fields
        #endregion


        #region Constructors
        public MainWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            string name = "Hello World";

            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);
            GL.DebugMessageCallback(DebugCallback, Marshal.StringToHGlobalAnsi(name));

            SetupData();
        }
        #endregion


        #region Protected Methods
        protected override void OnLoad()
        {
        }


        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            var transform = new Matrix4();
            transform.Column0 = new Vector4(1, 2, 3, 4);
            transform.Column1 = new Vector4(5, 6, 7, 8);
            transform.Column2 = new Vector4(9, 10, 11, 12);
            transform.Column3 = new Vector4(13, 14, 15, 16);

            var srcRect = new Rectangle(0, 0, 10, 10);

            UpdateQuad(0, srcRect, transform, 100, 200);

            base.OnUpdateFrame(args);
        }


        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
        }


        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
        }


        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
        }


        protected override void OnUnload()
        {
            base.OnUnload();
        }
        #endregion


        private void DebugCallback(DebugSource src, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            var errorMessage = Marshal.PtrToStringAnsi(message);

            errorMessage += errorMessage;
            errorMessage += $"\n\tSrc: {src}";
            errorMessage += $"\n\tType: {type}";
            errorMessage += $"\n\tID: {id}";
            errorMessage += $"\n\tSeverity: {severity}";
            errorMessage += $"\n\tLength: {length}";
            errorMessage += $"\n\tUser Param: {Marshal.PtrToStringAnsi(userParam)}";

            if (severity != DebugSeverity.DebugSeverityNotification)
            {
                throw new Exception(errorMessage);
            }
        }


        public static void SetupData()
        {
            var transformLocation = 2;

            CreateVertexBuffer(1);
            CreateIndexBuffer(1);

            _vertexArrayID = GL.GenVertexArray();

            //Bind the buffers to setup the attrib pointers
            GL.BindVertexArray(_vertexArrayID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBufferID);

            //Setup aPosition attribute
            GL.EnableVertexArrayAttrib(_vertexArrayID, 0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 21 * sizeof(float), 0);

            //Setup aTexCoord attribute
            GL.EnableVertexArrayAttrib(_vertexArrayID, 1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 21 * sizeof(float), 3 * sizeof(float));

            //Setup aTransform Column 1 attribute
            GL.EnableVertexArrayAttrib(_vertexArrayID, transformLocation);
            GL.VertexAttribPointer(transformLocation, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            //Setup aTransform Column 2 attribute
            GL.EnableVertexArrayAttrib(_vertexArrayID, transformLocation + 1);
            GL.VertexAttribPointer(transformLocation + 1, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            //Setup aTransform Column 3 attribute
            GL.EnableVertexArrayAttrib(_vertexArrayID, transformLocation + 2);
            GL.VertexAttribPointer(transformLocation + 2, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            //Setup aTransform Column 4 attribute
            GL.EnableVertexArrayAttrib(_vertexArrayID, transformLocation + 3);
            GL.VertexAttribPointer(transformLocation + 3, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        }


        public static void UpdateQuad(int quadID, Rectangle srcRect, Matrix4 transform, int textureWidth, int textureHeight)
        {
            //TODO: Condense/improve this to get a perf boost

            //TODO: Cache this value to avoid reflection for perf boost
            var totalVertexBytes = VertexDataAnalyzer.GetTotalBytesForStruct(typeof(VertexData));
            var totalQuadSizeInBytes = totalVertexBytes * 4;

            var quadData = CreateQuad();

            UpdateTextureCoordinates(ref quadData, srcRect, textureWidth, textureHeight);

            quadData.Vertex1.Transform = transform;
            quadData.Vertex2.Transform = transform;
            quadData.Vertex3.Transform = transform;
            quadData.Vertex4.Transform = transform;

            var offset = totalQuadSizeInBytes * quadID;
            GL.BufferSubData(BufferTarget.ArrayBuffer, new IntPtr(offset), totalQuadSizeInBytes, ref quadData);
        }


        private static void UpdateTextureCoordinates(ref QuadData quad, Rectangle srcRect, int textureWidth, int textureHeight)
        {
            //TODO: Condense/improve this to get a perf boost

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


        private static void CreateVertexBuffer(int totalQuads)
        {
            _vertexBufferID = GL.GenBuffer();

            var quadData = new List<QuadData>();

            for (int i = 0; i < totalQuads; i++)
            {
                quadData.Add(CreateQuad());
            }

            UploadQuadData(quadData.ToArray());
        }


        private static void CreateIndexBuffer(int totalQuads)
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


        private static QuadData CreateQuad()
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


        private static void UploadQuadData(QuadData[] data)
        {
            const int totalValuesPerVertice = 21;
            const int floatByteSize = sizeof(float);
            int totalVertices = data.Length * 4;
            int dataSizeInBytes = totalVertices * totalValuesPerVertice * floatByteSize;

            var verticeData = new List<VertexData>();

            foreach (var vertice in data)
            {
                verticeData.AddRange(vertice.Vertices);
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferID);
            GL.BufferData(BufferTarget.ArrayBuffer, dataSizeInBytes, verticeData.ToArray(), BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }


        private static void UploadIndexBufferData(uint[] data)
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBufferID);

            GL.BufferData(
                BufferTarget.ElementArrayBuffer,
                data.Length * sizeof(uint),
                data,
                BufferUsageHint.DynamicDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }
    }
}
