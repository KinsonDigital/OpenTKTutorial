using OpenToolkit.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace OpenTKTutorial
{
    public class Entity
    {
        public Entity(int textureID) { TextureID = textureID; }


        public int TextureID { get; set; }

        public Vector2 Position { get; set; }

        public float Angle { get; set; }

        public float Size { get; set; } = 1;

        public Color TintColor { get; set; } = Color.White;
    }
}
