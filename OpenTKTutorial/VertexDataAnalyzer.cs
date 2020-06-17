using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using System;
using System.Collections.Generic;

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
            { typeof(Vector4d), sizeof(double) * 4 },
            { typeof(Matrix4), sizeof(float) * 16 }
        };

        private static readonly Dictionary<Type, int> _totalItemsForTypes = new Dictionary<Type, int>()
        {
            //In order from least to greatest bytes
            { typeof(byte), 1},
            { typeof(sbyte), 1},
            { typeof(short), 1},
            { typeof(ushort), 1},
            { typeof(int), 1},
            { typeof(uint), 1},
            { typeof(float), 1},
            { typeof(long), 1},
            { typeof(ulong), 1},
            { typeof(double), 1},
            { typeof(Vector2), 2},
            { typeof(Vector2i), 2},
            { typeof(Vector3), 3},
            { typeof(Vector3i), 3},
            { typeof(Vector2d), 2},
            { typeof(Vector4), 4},
            { typeof(Vector4i), 4},
            { typeof(Vector3d), 3},
            { typeof(Vector4d), 3},
            { typeof(Matrix4), 16}
        };

        //TODO: Need to find out the rest of the mappings
        private static readonly Dictionary<Type, VertexAttribPointerType> _pointerTypeMappings = new Dictionary<Type, VertexAttribPointerType>()
        {
            { typeof(float), VertexAttribPointerType.Float },
            { typeof(Vector2), VertexAttribPointerType.Float },
            { typeof(Vector3), VertexAttribPointerType.Float }
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


        public static int GetTypeByteSize(Type type)
        {
            return _validTypeSizes[type];
        }


        public static int TotalItemsForType(Type type) => _totalItemsForTypes[type];


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


        public static VertexAttribPointerType GetVertexPointerType(Type type) => _pointerTypeMappings[type];


        public static int GetVertexDataOffset<T>(T[] vertexData, int vertexIndex, string subDataName)
        {

            return -1;
        }


        private static bool IsStruct(Type type) => type.IsValueType && !type.IsEnum;
    }
}
