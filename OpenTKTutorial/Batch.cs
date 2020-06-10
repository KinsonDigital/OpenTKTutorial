using System;
using System.Collections.Generic;
using System.Linq;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;

namespace OpenTKTutorial
{
    public class Batch
    {
        private GPU _gpu = GPU.Instance;
        private readonly Dictionary<int, TextureData> _textureData = new Dictionary<int, TextureData>();
        private static int _totalSingleVertexBytes = VertexDataAnalyzer.GetTotalBytesForStruct(typeof(VertexData));
        private static int _totalQuadBytes = _totalSingleVertexBytes * 4;
        private static readonly Dictionary<int, int> _textureLocations = new Dictionary<int, int>();
        private static QuadData _quadData = new QuadData()
        {
            Vertex1 = new VertexData()
            {
                Vertex = new Vector3(-1, 1, 0),//Top Left
                TextureCoord = new Vector2(0, 1),
            },
            Vertex2 = new VertexData()
            {
                Vertex = new Vector3(1, 1, 0),//Top Right
                TextureCoord = new Vector2(1, 1),
            },
            Vertex3 = new VertexData()
            {
                Vertex = new Vector3(1, -1, 0),//Bottom Right
                TextureCoord = new Vector2(1, 0),
            },
            Vertex4 = new VertexData()
            {
                Vertex = new Vector3(-1, -1, 0),//Bottom Left
                TextureCoord = new Vector2(0, 0),
            }
        };

        public Batch()
        {
            for (int slot = 0; slot < _gpu.TotalTextureSlots; slot++)
            {
                _textureData.Add(slot, new TextureData());
            }

            if (_textureLocations.Count <= 0)
            {
                for (int slot = 0; slot < _gpu.TotalTextureSlots; slot++)
                {
                    _textureLocations.Add(slot, GL.GetUniformLocation(_gpu.GetShaderProgram().ProgramId, $"textures[{slot}]"));
                }
            }
        }


        public bool IsFull => _textureData.All(i => !i.Value.IsEmpty);

        public bool IsEmpty => _textureData.All(i => i.Value.IsEmpty);

        public bool HasSpace => _textureData.Any(i => i.Value.IsEmpty);


        public void AddTextureData(Texture texture)
        {
            var freeSlot = GetFreeSlot();

            _textureData[freeSlot] = texture.ToTextureData();
        }


        public void BindBatch()
        {
            foreach (var slot in _textureData.Keys)
            {
                if (_textureData[slot].IsEmpty)
                    continue;

                GL.ActiveTexture(TextureUnit.Texture0 + slot);
                GL.BindTexture(TextureTarget.Texture2D, _textureData[slot].TextureID);

                GL.Uniform1(_textureLocations[slot], slot);
            }
        }


        public void UpdateTintColors()
        {
            foreach (var dataItem in _textureData)
            {
                if (!dataItem.Value.IsEmpty)
                    UpdateTintColor(dataItem.Key, dataItem.Value.TintColor);
            }
        }


        public void UpdateTransforms()
        {
            var itemsToUpdate = (from d in _textureData
                                 where !d.Value.IsEmpty
                                 select d).ToArray();

            if (itemsToUpdate is null || itemsToUpdate.Length <= 0)
                return;

            foreach (var item in itemsToUpdate)
            {
                UpdateGPUTransform(item.Key,
                                   item.Value.X,
                                   item.Value.Y,
                                   item.Value.Width,
                                   item.Value.Height,
                                   item.Value.Size,
                                   item.Value.Angle);
            }
        }


        public void Clear()
        {
            var totalFullItems = _textureData.Values.Count(i => !i.IsEmpty);

            var keys = _textureData.Keys.ToArray();

            for (int i = 0; i < keys.Length; i++)
            {
                _textureData[keys[i]] = new TextureData();
            }

            totalFullItems = _textureData.Values.Count(i => !i.IsEmpty);
        }


        private void UpdateTintColor(int textureSlot, Vector4 tintColor)
        {
            var quadByteStart = _totalQuadBytes * textureSlot;

            _quadData.Vertex1.TextureSlot = textureSlot;
            _quadData.Vertex2.TextureSlot = textureSlot;
            _quadData.Vertex3.TextureSlot = textureSlot;
            _quadData.Vertex4.TextureSlot = textureSlot;

            _quadData.Vertex1.TintColor = tintColor;
            _quadData.Vertex2.TintColor = tintColor;
            _quadData.Vertex3.TintColor = tintColor;
            _quadData.Vertex4.TintColor = tintColor;


            GL.BufferSubData(BufferTarget.ArrayBuffer, new IntPtr(quadByteStart), _totalQuadBytes, ref _quadData);
        }


        private int GetFreeSlot()
        {
            var freeSlot = _textureData.Values.ToList().IndexOf(i => i.IsEmpty);

            if (freeSlot == -1)
                throw new Exception($"Only {_gpu.TotalTextureSlots} available per batch.");


            return freeSlot;
        }


        private void UpdateGPUTransform(int textureSlot, float x, float y, int width, int height, float size, float angle)
        {
            //Create and send the transformation data to the GPU
            var transMatrix = BuildTransformationMatrix(x,
                y,
                width,
                height,
                size,
                angle);

            GL.UniformMatrix4(_gpu.TransformLocations[textureSlot], true, ref transMatrix);
        }


        /// <summary>
        /// Builds a complete transformation matrix using the given params.
        /// </summary>
        /// <param name="x">The x position of a texture.</param>
        /// <param name="y">The y position of a texture.</param>
        /// <param name="width">The width of a texture.</param>
        /// <param name="height">The height of a texture.</param>
        /// <param name="size">The size of a texture. 1 represents normal size and 1.5 represents 150%.</param>
        /// <param name="angle">The angle of the texture.</param>
        /// <returns></returns>
        private Matrix4 BuildTransformationMatrix(float x, float y, int width, int height, float size, float angle)
        {
            var scaleX = (float)width / _gpu.ViewPortWidth;
            var scaleY = (float)height / _gpu.ViewPortHeight;

            scaleX *= size;
            scaleY *= size;

            var ndcX = x.MapValue(0f, _gpu.ViewPortWidth, -1f, 1f);
            var ndcY = y.MapValue(0f, _gpu.ViewPortHeight, 1f, -1f);

            //NOTE: (+ degrees) rotates CCW and (- degress) rotates CW
            var angleRadians = MathHelper.DegreesToRadians(angle);

            //Invert angle to rotate CW instead of CCW
            angleRadians *= -1;

            var rotation = Matrix4.CreateRotationZ(angleRadians);
            var scaleMatrix = Matrix4.CreateScale(scaleX, scaleY, 1f);
            var positionMatrix = Matrix4.CreateTranslation(new Vector3(ndcX, ndcY, 0));


            return rotation * scaleMatrix * positionMatrix;
        }
    }
}
