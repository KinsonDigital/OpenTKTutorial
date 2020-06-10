using OpenToolkit.Mathematics;

namespace OpenTKTutorial
{
    public struct TextureData
    {
        public float X;

        public float Y;

        public int Width;

        public int Height;

        public float Size;

        public float Angle;

        public Vector4 TintColor;

        public int TextureID;


        public bool IsEmpty
        {
            get
            {
                return X == 0f && Y == 0f && Width == 0 && Height == 0 &&
                    Size == 0f && Angle == 0f && TintColor.IsEmpty() && TextureID == 0;
            }
        }


        public void Empty()
        {
            X = 0;
            Y = 0;
            Width = 0;
            Height = 0;
            Size = 0;
            Angle = 0;
            TintColor.X = 0;
            TintColor.Y = 0;
            TintColor.Z = 0;
            TintColor.W = 0;
            TextureID = 0;
        }
    }
}
