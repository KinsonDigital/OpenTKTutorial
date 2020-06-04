using OpenToolkit.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenTKTutorial
{
    public static class VertexDataAnalyzer
    {
        private static readonly Dictionary<Type, int> _validTypeSizes = new Dictionary<Type, int>()
        {
            //In order from least to greatest bytes
            { typeof(byte), sizeof(byte) },
            { typeof(sbyte), sizeof(sbyte) },
            { typeof(short), sizeof(short) },
            { typeof(ushort), sizeof(ushort) },
            { typeof(int), sizeof(int) },
            { typeof(uint), sizeof(uint) },
            { typeof(float), sizeof(float) },
            { typeof(long), sizeof(long) },
            { typeof(ulong), sizeof(ulong) },
            { typeof(double), sizeof(double) },
            { typeof(Vector2), sizeof(float) * 2 },
            { typeof(Vector2i), sizeof(int) * 2 },
            { typeof(Vector3), sizeof(float) * 3 },
            { typeof(Vector3i), sizeof(int) * 3 },
            { typeof(Vector2d), sizeof(double) * 2 },
            { typeof(Vector4), sizeof(float) * 4 },
            { typeof(Vector4i), sizeof(int) * 4 },
            { typeof(Vector3d), sizeof(double) * 3 },
            { typeof(Vector4d), sizeof(double) * 4 }
        };


        public static int GetTotalBytesForStruct<T>(T structType) where T : Type
        {
            if (!structType.IsValueType || structType.IsEnum)
                throw new Exception($"The given '{nameof(structType)}' must be a struct.");

            var publicFields = structType.GetFields();
            var result = 0;

            //If any types are not of the valid type list, throw an exception
            foreach (var field in publicFields)
            {
                if (!_validTypeSizes.ContainsKey(field.FieldType))
                    throw new Exception($"The type '{field.FieldType}' is not allowed in vertex buffer data structure.");

                result += _validTypeSizes[field.FieldType];
            }


            return result;
        }
    }
}
