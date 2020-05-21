using OpenToolkit.Mathematics;
using System.Runtime.InteropServices;

namespace OpenTKTutorial
{
    [StructLayout(LayoutKind.Sequential)]
    public struct QuadBufferData
    {
        public Vector3 CornerVertice { get; set; }

        public Vector2 TextureCoords { get; set; }

        public Vector4 TintColor { get; set; }
    }
}
