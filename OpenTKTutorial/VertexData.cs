using OpenToolkit.Mathematics;
using System.Runtime.InteropServices;

namespace OpenTKTutorial
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexData
    {
        public Vector3 Vertex;//Location 0 | aPosition

        public Vector2 TextureCoord;//Location 1 | aTexCoord

        public Vector4 TintColor;//Location 2 | aTintColor

        public float TextureIndex;//Location 3 | aTextureIndex 
    }
}
