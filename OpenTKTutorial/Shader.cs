using System;
using System.IO;
using System.Text;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;

namespace OpenTKTutorial
{
    public class Shader : IDisposable
    {
        private int _shaderProgramHandle;
        private bool _disposedValue;


        public Shader(string vertexPath, string fragmentPath)
        {
            int VertexShader;
            int FragmentShader;

            //Load the source code from the shader files
            string VertexShaderSource;

            using (StreamReader reader = new StreamReader(vertexPath, Encoding.UTF8))
            {
                VertexShaderSource = reader.ReadToEnd();
            }

            string FragmentShaderSource;

            using (StreamReader reader = new StreamReader(fragmentPath, Encoding.UTF8))
            {
                FragmentShaderSource = reader.ReadToEnd();
            }


            //Generate shaders and bind the source code to the shaders
            VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, VertexShaderSource);

            FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);


            //Compile the shaders and check for errors
            GL.CompileShader(VertexShader);

            string infoLogVert = GL.GetShaderInfoLog(VertexShader);
            if (infoLogVert != string.Empty)
                throw new Exception(infoLogVert);

            GL.CompileShader(FragmentShader);

            string infoLogFrag = GL.GetShaderInfoLog(FragmentShader);

            if (infoLogFrag != string.Empty)
                throw new Exception(infoLogVert);

            //Link shaders together into a single program
            _shaderProgramHandle = GL.CreateProgram();

            GL.AttachShader(_shaderProgramHandle, VertexShader);
            GL.AttachShader(_shaderProgramHandle, FragmentShader);

            GL.LinkProgram(_shaderProgramHandle);

            /*Before we leave the constructor, we should do a little cleanup. The individual 
             * vertex and fragment shaders are useless now that they've been linked; the compiled 
             * data is copied to the shader program when you link it. You also don't need to 
             * have those individual shaders attached to the program; let's detach and then delete them.
             */
            GL.DetachShader(_shaderProgramHandle, VertexShader);
            GL.DetachShader(_shaderProgramHandle, FragmentShader);
            GL.DeleteShader(FragmentShader);
            GL.DeleteShader(VertexShader);
        }


        public void SetTransformationMatrix(Matrix4 matrix)
        {
            var uniformTransformationLocation = GetUniformLocation();
            GL.UniformMatrix4(uniformTransformationLocation, true, ref matrix);
        }

        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(_shaderProgramHandle, attribName);
        }


        public int GetUniformLocation()
        {
            return GL.GetUniformLocation(_shaderProgramHandle, "u_transform");
        }


        public void UseProgram()
        {
            GL.UseProgram(_shaderProgramHandle);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue)
                return;

            GL.DeleteProgram(_shaderProgramHandle);
            _disposedValue = true;
        }


        ~Shader()
        {
            Dispose(false);
        }
    }
}
