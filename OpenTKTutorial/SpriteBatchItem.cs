using System.Drawing;

namespace OpenTKTutorial
{
    internal struct SpriteBatchItem
    {
        public int TextureID;

        public Rectangle SrcRect;

        public Rectangle DestRect;

        public float Size;

        public float Angle;

        public Color TintColor;


        public bool IsEmpty
        {
            get
            {
                return TextureID == -1 &&
                    SrcRect.IsEmpty &&
                    DestRect.IsEmpty &&
                    Size == 0f &&
                    Angle == 0f &&
                    TintColor.IsEmpty;
            }
        }


        public static SpriteBatchItem Empty => new SpriteBatchItem() { TextureID = -1 };
    }
}
