using OpenToolkit.Mathematics;
using System.Runtime.InteropServices;

namespace OpenTKTutorial
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexData : System.IEquatable<VertexData>
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "<Pending>")]
        public Vector3 Vertex;//Location 0 | aPosition

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "<Pending>")]
        public Vector2 TextureCoord;//Location 1 | aTexCoord

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "<Pending>")]
        public Vector4 TintColor;//Location 2 | aTintColor

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "<Pending>")]
        public float TransformIndex;//Location 3 | aTransformIndex

        public override bool Equals(object obj)
        {
            if (!(obj is VertexData data))
                return false;

            return data == this;
        }


        public bool Equals(VertexData other)
        {
            return other == this;
        }


        public override int GetHashCode()
        {
            return Vertex.GetHashCode() + TextureCoord.GetHashCode() + TintColor.GetHashCode() + TransformIndex.GetHashCode();
        }


        public static bool operator ==(VertexData left, VertexData right)
        {
            return left.Equals(right);
        }


        public static bool operator !=(VertexData left, VertexData right)
        {
            return !(left == right);
        }
    }
}
