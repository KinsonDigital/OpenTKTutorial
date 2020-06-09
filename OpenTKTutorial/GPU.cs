using OpenToolkit.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace OpenTKTutorial
{
    public sealed class GPU
    {
        private const int MAX_SLOTS = 1000;
        private static readonly List<int> _freeSlots = new List<int>();
        private static readonly List<int> _usedSlots = new List<int>();
        private static readonly Lazy<GPU> _gpuSingleton = new Lazy<GPU>(() => new GPU());

        public static GPU Instance => _gpuSingleton.Value;


        private GPU()
        {
            //Get the total number of available texture slots for the vertex and fragment shaders
            GL.GetInteger(GetPName.MaxVertexTextureImageUnits, out int maxVertexStageTextureSlots);
            GL.GetInteger(GetPName.MaxTextureImageUnits, out int maxFragmentStageTextureSlots);

            TotalTextureSlots = maxVertexStageTextureSlots < maxFragmentStageTextureSlots
                ? maxVertexStageTextureSlots
                : maxFragmentStageTextureSlots;

            //Create 1000 available GPU slots
            for (int i = 0; i < MAX_SLOTS; i++)
                _freeSlots.Add(i);
        }



        public int TotalTextureSlots { get; private set; }


        public int GetFreeSlot()
        {
            //TODO: Check to see if you can get the number of used slots
            //Start with the lowest number and work up.
            //This is to get the lowest available slot
            for (int i = 0; i < MAX_SLOTS; i++)
            {
                if (_freeSlots.Contains(i))
                {
                    _usedSlots.Add(i);
                    _freeSlots.Remove(i);

                    return i;
                }
            }


            throw new Exception($"No free slots available.  Maximum slots are '{MAX_SLOTS}'");
        }


        public void ReleaseSlot(int slotNumber)
        {
            _freeSlots.Add(slotNumber);
            _usedSlots.Remove(slotNumber);
        }
    }
}
