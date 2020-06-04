using OpenToolkit.Mathematics;
using System.Runtime.InteropServices;

namespace OpenTKTutorial
{
    /*TODO:
     * Need to come up with a way to be able to write some helper methods
     * that will give the ability to figure out how many bytes of data
     * an entire struct comes to.  This will either have to use reflection
     * to traverse over all of the properties and there data types to figure 
     * this out, or there might be an easier way.  Research how to do this.
     * 
     * The purpose of this is to make it easier to make changes to structs
     * for holding data to send to the GPU for easily creating shader attribute
     * pointers for vertex buffer data layouts. The C# sizeof() method might be able
     * to automatically do all of this.
     * 
     * C++ also has a handy built in function called offsetof() which automatically
     * figures out the layout offset for data in a struct.  Check if C# has this as well.
     */
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexData
    {
        public Vector3 Vertex;//Location 0 | aPosition

        public Vector2 TextureCoord;//Location 1 | aTexCoord

        //public Vector4 TintColor;//Location 2 | aTintColor

        public float TextureIndex;//Location 3 | aTextureIndex 
    }
}
