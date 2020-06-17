using OpenToolkit.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GLDataPlayground
{
    public static class ExtensionMethods
    {
        public static float MapValue(this int value, float fromStart, float fromStop, float toStart, float toStop) =>
            MapValue((float)value, fromStart, fromStop, toStart, toStop);


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
    }
}
