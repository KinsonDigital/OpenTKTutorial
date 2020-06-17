using System.Runtime.InteropServices;

namespace OpenTKTutorial
{
    [StructLayout(LayoutKind.Sequential)]
    public struct QuadData
    {
        public VertexData Vertex1;
        public VertexData Vertex2;
        public VertexData Vertex3;
        public VertexData Vertex4;


        public VertexData[] Vertices =>
            new[] { Vertex1, Vertex2, Vertex3, Vertex4 };
    }
}
