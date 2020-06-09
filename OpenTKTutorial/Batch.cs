using OpenToolkit.Mathematics;
using System;

namespace OpenTKTutorial
{
    public class Batch
    {
        private GPU _gpu = GPU.Instance;

        
        public Batch()
        {
            Data = new TextureData[_gpu.TotalTextureSlots];
        }



        public TextureData[] Data { get; private set; }


        public void SetX(int textureSlot, float value)
        {
            CheckSlot(textureSlot);
            Data[textureSlot].X = value;
        }


        public void SetY(int textureSlot, float value)
        {
            CheckSlot(textureSlot);
            Data[textureSlot].Y = value;
        }


        public void SetWidth(int textureSlot, int value)
        {
            CheckSlot(textureSlot);
            Data[textureSlot].Width = value;
        }


        public void SetHeight(int textureSlot, int value)
        {
            CheckSlot(textureSlot);
            Data[textureSlot].Height = value;
        }


        public void SetSize(int textureSlot, float value)
        {
            CheckSlot(textureSlot);
            Data[textureSlot].Size = value;
        }


        public void SetAngle(int textureSlot, float value)
        {
            CheckSlot(textureSlot);
            Data[textureSlot].Angle = value;
        }


        public void SetTintColor(int textureSlot, Vector4 value)
        {
            CheckSlot(textureSlot);
            Data[textureSlot].TintColor = value;
        }


        public void SetTextureSlot(int textureSlot)
        {
            CheckSlot(textureSlot);
            Data[textureSlot].TextureSlot = textureSlot;
        }


        public void Clear()
        {
            for (int i = 0; i < Data.Length; i++)
            {
                Data[i].X = 0;
                Data[i].Y = 0;
                Data[i].Width = 0;
                Data[i].Height = 0;
                Data[i].Size = 0;
                Data[i].Angle = 0;
                Data[i].TintColor.X = 0;
                Data[i].TintColor.Y = 0;
                Data[i].TintColor.Z = 0;
                Data[i].TintColor.W = 0;
            }
        }


        private void CheckSlot(int textureSlot)
        {
            if (textureSlot < 0 || textureSlot > Data.Length - 1)
                throw new Exception($"The texture slot '{textureSlot}' is invalid.  Must be a value between 0 and {Data.Length}");
        }
    }
}
