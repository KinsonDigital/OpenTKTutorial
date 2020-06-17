using OpenToolkit.Mathematics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace GLDataPlayground
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexData
    {
        public Vector3 Vertex;//Location 0 | aPosition

        public Vector2 TextureCoord;//Location 1 | aTexCoord

        public Matrix4 Transform;//Locations 2,3,4 and 5
    }
}
