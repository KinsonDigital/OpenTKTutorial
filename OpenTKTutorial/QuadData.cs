using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace OpenTKTutorial
{
    [StructLayout(LayoutKind.Sequential)]
    public struct QuadData : System.IEquatable<QuadData>
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "<Pending>")]
        public VertexData Vertex1;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "<Pending>")]
        public VertexData Vertex2;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "<Pending>")]
        public VertexData Vertex3;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "<Pending>")]
        public VertexData Vertex4;


        public override bool Equals(object obj)
        {
            if (!(obj is QuadData data))
                return false;

            return data == this;
        }


        public bool Equals(QuadData other)
        {
            return other == this;
        }


        public override int GetHashCode()
        {
            return Vertex1.GetHashCode() + Vertex2.GetHashCode() + Vertex3.GetHashCode() + Vertex4.GetHashCode();
        }


        public static bool operator ==(QuadData left, QuadData right)
        {
            return left.Equals(right);
        }


        public static bool operator !=(QuadData left, QuadData right)
        {
            return !(left == right);
        }
    }
}
