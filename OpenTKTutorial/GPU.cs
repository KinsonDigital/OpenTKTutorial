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

        //This dictionary is key(slot) and value(shader location)
        private static readonly Dictionary<int, int> _transformLocations = new Dictionary<int, int>();
        private static ShaderProgram _shader;

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


        public ShaderProgram GetShaderProgram()
        {
            if (!(_shader is null))
                return _shader;

            _shader = new ShaderProgram("shader.vert", "shader.frag");


            for (int i = 0; i < TotalTextureSlots; i++)
            {
                var slotTransformLocation = GL.GetUniformLocation(_shader.ProgramId, $"u_Transforms[{i}]");

                _transformLocations.Add(i, slotTransformLocation);
            }

            return _shader;
        }


        public Dictionary<int, int> TransformLocations => _transformLocations;


        public int TotalTextureSlots { get; private set; }


        public int ViewPortWidth { get; set; } = 800;

        public int ViewPortHeight { get; set; } = 600;


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
