using System;
using System.Collections.Generic;
using System.Text;

namespace OpenTKTutorial
{
    public struct QuadData
    {
        public VertexData Vertex1;

        public VertexData Vertex2;

        public VertexData Vertex3;

        public VertexData Vertex4;

        public VertexData[] GetVertices() => new [] { Vertex1, Vertex2, Vertex3, Vertex4 };
    }
}
