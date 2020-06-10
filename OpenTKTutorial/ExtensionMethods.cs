using OpenToolkit.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace OpenTKTutorial
{
    public static class ExtensionMethods
    {
        public static float MapValue(this float value, float fromStart, float fromStop, float toStart, float toStop) =>
            toStart + (toStop - toStart) * ((value - fromStart) / (fromStop - fromStart));


        public static Vector4 MapValues(this Vector4 value, float fromStart, float fromStop, float toStart, float toStop)
        {
            return new Vector4
            {
                X = value.X.MapValue(fromStart, fromStop, toStart, toStop),
                Y = value.Y.MapValue(fromStart, fromStop, toStart, toStop),
                Z = value.Z.MapValue(fromStart, fromStop, toStart, toStop),
                W = value.W.MapValue(fromStart, fromStop, toStart, toStop)
            };
        }


        public static Vector4 ToVector4(this Color clr) => new Vector4(clr.R, clr.G, clr.B, clr.A);

        public static Vector4 ToGLColor(this Color value)
        {
            var vec4 = value.ToVector4();
            return vec4.MapValues(0, 255, 0, 1);
        }


        public static int IndexOf<T>(this List<T> batchPool, Predicate<T> predicate)
        {
            for (int i = 0; i < batchPool.Count(); i++)
            {
                if (predicate(batchPool[i]))
                    return i;
            }


            return -1;
        }


        public static TextureData ToTextureData(this Texture texture)
        {
            TextureData result = new TextureData();

            result.X = texture.X;
            result.Y = texture.Y;
            result.Width = texture.Width;
            result.Height = texture.Height;
            result.Angle = texture.Angle;
            result.Size = texture.Size;
            result.TintColor = texture.TintColor.ToGLColor();
            result.TextureID = texture.ID;


            return result;
        }



        public static bool IsEmpty(this Vector4 vector)
        {
            return vector.X == 0 && vector.Y == 0 && vector.Z == 0 && vector.W == 0;
        }
    }
}
