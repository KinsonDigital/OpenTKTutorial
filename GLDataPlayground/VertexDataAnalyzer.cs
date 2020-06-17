using OpenToolkit.Mathematics;
using System;
using System.Collections.Generic;

namespace GLDataPlayground
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
            { typeof(Vector4d), sizeof(double) * 4 },
            { typeof(Matrix4), sizeof(float) * 16 }
        };


        public static int GetTotalBytesForStruct(Type structType)
        {
            if (!IsStruct(structType))
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


        public static int GetVertexSubDataOffset(Type structType, string subDataName)
        {
            if (!IsStruct(structType))
                throw new Exception($"The given '{nameof(structType)}' must be a struct.");

            var publicFields = structType.GetFields();
            var result = 0;


            //If any types are not of the valid type list, throw an exception
            foreach (var field in publicFields)
            {
                if (!_validTypeSizes.ContainsKey(field.FieldType))
                    throw new Exception($"The type '{field.FieldType}' is not allowed in vertex buffer data structure.");

                //If the type is not the field of the given name.
                //Get all of the fields sequentially up unto the sub data name field
                if (field.Name != subDataName)
                {
                    result += _validTypeSizes[field.FieldType];
                }
                else
                {
                    break;
                }
            }


            return result;
        }


        public static int GetVertexDataOffset<T>(T[] vertexData, int vertexIndex, string subDataName)
        {

            return -1;
        }


        private static bool IsStruct(Type type) => type.IsValueType && !type.IsEnum;
    }
}
