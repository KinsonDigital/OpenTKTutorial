// <copyright file="ShaderProgram.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace OpenTKTutorial
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Silk.NET.OpenGL;

    /// <summary>
    /// A shader program consisting of a vertex and fragment shader.
    /// </summary>
    public class ShaderProgram : IDisposable
    {
        private readonly Dictionary<string, int> uniformLocations = new Dictionary<string, int>();
        private readonly GL GL;
        private readonly int batchSize;
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderProgram"/> class.
        /// </summary>
        /// <param name="batchSize">The batch size that the shader will support.</param>
        /// <param name="vertexShaderPath">The path to the vertex shader code.</param>
        /// <param name="fragmentShaderPath">The path to the fragment shader code.</param>
        public ShaderProgram(GL gl, int batchSize, string vertexShaderPath, string fragmentShaderPath)
        {
            this.GL = gl;
            this.batchSize = batchSize;

            var shaderSource = LoadShaderData(vertexShaderPath);
            VertexShaderId = CreateShader(ShaderType.VertexShader, shaderSource);

            // We do the same for the fragment shader
            shaderSource = LoadShaderData(fragmentShaderPath);
            FragmentShaderId = CreateShader(ShaderType.FragmentShader, shaderSource);

            // Merge both shaders into a shader program, which can then be used by OpenGL.
            ProgramId = SetupShaderProgram(VertexShaderId, FragmentShaderId);

            // When the shader program is linked, it no longer needs the individual shaders attacked to it.
            // The compiled code is copied into the shader program.
            // Detach and then delete them.
            DestroyShader(ProgramId, VertexShaderId);
            DestroyShader(ProgramId, FragmentShaderId);

            // This is for the purpose of caching the locations of the uniforms.
            // The reason is because GetUniformLocation() is a slow call.
            // Get the number of active uniforms in the shader.
            this.GL.GetProgram(ProgramId, ProgramPropertyARB.ActiveUniforms, out var totalActiveUniforms);

            // Loop over all the uniforms
            for (uint i = 0; i < totalActiveUniforms; i++)
            {
                // get the name of this uniform,
                var key = this.GL.GetActiveUniform(ProgramId, i, out _, out _);

                // Get the location of the uniform on the GPU
                var location = this.GL.GetUniformLocation(ProgramId, key);

                if (location == -1)
                    throw new Exception($"The uniform with the name '{key}' does not exist.");

                // Cache it
                this.uniformLocations.Add(key, location);
            }
        }

        /// <summary>
        /// Gets the ID of the vertex shader.
        /// </summary>
        public uint VertexShaderId { get; private set; }

        /// <summary>
        /// Gets the ID of the fragment shader on the GPU.
        /// </summary>
        public uint FragmentShaderId { get; private set; }

        /// <summary>
        /// Gets the shader program ID on the GPU.
        /// </summary>
        public uint ProgramId { get; private set; }

        /// <summary>
        /// Sets the active shader program to use on the GPU.
        /// </summary>
        public void UseProgram() => this.GL.UseProgram(ProgramId);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        /// <param name="disposing">True if managed resources should be disposed of.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.disposedValue)
                return;

            // Dipose of managed resources
            if (disposing)
                this.uniformLocations.Clear();

            // Delete unmanaged resources
            this.GL.DeleteProgram(ProgramId);

            this.disposedValue = true;
        }

        /// <summary>
        /// Creates a shader program using the given vertex and fragment shader ID numbers.
        /// </summary>
        /// <param name="vertexShaderId">The ID of the vertex shader.</param>
        /// <param name="fragmentShaderId">The ID of the fragment shader.</param>
        /// <returns>The shader program ID.</returns>
        private uint SetupShaderProgram(uint vertexShaderId, uint fragmentShaderId)
        {
            var programHandle = this.GL.CreateProgram();

            // Attach both shaders...
            this.GL.AttachShader(programHandle, vertexShaderId);

            this.GL.AttachShader(programHandle, fragmentShaderId);

            // Link them together
            LinkProgram(programHandle);

            return programHandle;
        }

        /// <summary>
        /// Links the program using the given <paramref name="shaderProgramId"/>.
        /// </summary>
        /// <param name="shaderProgramId">The ID of the shader program.</param>
        private void LinkProgram(uint shaderProgramId)
        {
            // We link the program
            this.GL.LinkProgram(shaderProgramId);

            // Check for linking errors
            this.GL.GetProgram(shaderProgramId, ProgramPropertyARB.LinkStatus, out var statusCode);

            if (statusCode != 1)
            {
                // We can use `GL.GetProgramInfoLog(program)` to get information about the error.
                _ = this.GL.GetProgramInfoLog(shaderProgramId);

                throw new Exception($"Error occurred while linking Program({shaderProgramId})");
            }
        }

        /// <summary>
        /// Creates a shader of the given <paramref name="shaderType"/> using the
        /// given <paramref name="shaderSrc"/> code.
        /// </summary>
        /// <param name="shaderType">The type of shader to create.</param>
        /// <param name="shaderSrc">The shader source code to use for the shader program.</param>
        /// <returns>The OpenGL shader ID.</returns>
        private uint CreateShader(ShaderType shaderType, string shaderSrc)
        {
            var shaderId = this.GL.CreateShader(shaderType);

            this.GL.ShaderSource(shaderId, shaderSrc);

            CompileShader(shaderId);

            return shaderId;
        }

        /// <summary>
        /// Destroys the shaders.  Any shaders setup and sent to the GPU will still reside there.
        /// </summary>
        /// <param name="shaderProgramId">The program ID of the shader.</param>
        /// <param name="shaderId">The shader ID of the shader.</param>
        private void DestroyShader(uint shaderProgramId, uint shaderId)
        {
            this.GL.DetachShader(shaderProgramId, shaderId);
            this.GL.DeleteShader(shaderId);
        }

        /// <summary>
        /// Compiles the currently set shader source code on the GPU.
        /// </summary>
        /// <param name="shaderId">The shader ID.</param>
        private void CompileShader(uint shaderId)
        {
            // Try to compile the shader
            this.GL.CompileShader(shaderId);

            // Check for compilation errors
            this.GL.GetShader(shaderId, ShaderParameterName.CompileStatus, out var statusCode);

            if (statusCode != 1)
            {
                var errorInfo = this.GL.GetShaderInfoLog(shaderId);

                throw new Exception($"Error occurred while compiling Shader({shaderId})\n{errorInfo}");
            }
        }

        private string LoadShaderData(string shaderFilePath)
        {
            // Load the source code from the shader files
            var result = new StringBuilder();

            using (var reader = new StreamReader(shaderFilePath, Encoding.UTF8))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    line = ProcessLine(line);

                    result.AppendLine(line);
                }
            }

            return result.ToString();
        }

        private string ProcessLine(string line)
        {
            var result = string.Empty;

            // If the line of code has a replacement tag on it
            if (line.Contains("//$REPLACE_INDEX", StringComparison.Ordinal))
            {
                var sections = line.Split(' ');

                // Find the section with the array brackets
                for (var i = 0; i < sections.Length; i++)
                {
                    if (sections[i].Contains('[', StringComparison.Ordinal) && sections[i].Contains(']', StringComparison.Ordinal))
                    {
                        result += $"{sections[i].Split('[')[0]}[{this.batchSize}];//MODIFIED_DURING_COMPILE_TIME";
                    }
                    else
                    {
                        result += sections[i] + ' ';
                    }
                }
            }
            else
            {
                result = line;
            }

            return result;
        }
    }
}
