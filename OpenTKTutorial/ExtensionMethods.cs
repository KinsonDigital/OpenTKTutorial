// <copyright file="ExtensionMethods.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace OpenTKTutorial
{
    using System;
    using System.Drawing;
    using System.Numerics;

    public static class ExtensionMethods
    {
        private const float PI = 3.1415926535897931f;

        public static float ToRadians(this float degrees) => degrees * PI / 180f;

        public static int CountKD<T>(this T[] items, Predicate<T> predicate)
        {
            var result = 0;

            for (var i = 0; i < items.Length; i++)
            {
                if (predicate(items[i]))
                    result += 1;
            }

            return result;
        }

        public static float MapValue(this int value, float fromStart, float fromStop, float toStart, float toStop) =>
            MapValue((float)value, fromStart, fromStop, toStart, toStop);

        public static float MapValue(this float value, float fromStart, float fromStop, float toStart, float toStop) =>
            toStart + ((toStop - toStart) * ((value - fromStart) / (fromStop - fromStart)));

        public static Vector4 MapValues(this Vector4 value, float fromStart, float fromStop, float toStart, float toStop) => new Vector4
        {
            X = value.X.MapValue(fromStart, fromStop, toStart, toStop),
            Y = value.Y.MapValue(fromStart, fromStop, toStart, toStop),
            Z = value.Z.MapValue(fromStart, fromStop, toStart, toStop),
            W = value.W.MapValue(fromStart, fromStop, toStart, toStop),
        };

        public static Vector4 ToVector4(this Color clr) => new Vector4(clr.R, clr.G, clr.B, clr.A);

        public static Vector4 ToGLColor(this Color value)
        {
            var vec4 = value.ToVector4();
            return vec4.MapValues(0, 255, 0, 1);
        }

        public static Rectangle ToRectangle(this AtlasSubRect rect) => new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);

        public static bool IsPowerOfTwo(this ulong x) => (x != 0) && ((x & (x - 1)) == 0);

        public static bool IsPowerOfTwo(this long x) => (x != 0) && ((x & (x - 1)) == 0);

        public static bool IsPowerOfTwo(this int x) => (x != 0) && ((x & (x - 1)) == 0);

        public static bool IsPowerOfTwo(this byte x) => (x != 0) && ((x & (x - 1)) == 0);
    }
}
