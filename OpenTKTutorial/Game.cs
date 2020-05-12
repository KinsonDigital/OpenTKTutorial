﻿using System;
using System.Collections.Generic;
using System.Text;
using OpenToolkit;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Desktop;
using OpenToolkit.Input;
using OpenToolkit.Windowing.Common.Input;
using OpenToolkit.Graphics.OpenGL4;

namespace OpenTKTutorial
{
    public class Game : GameWindow
    {
        private int _vertexBufferObject;
        private int _vertexArrayObject;
        private int _elementBufferObject;

        float[] _vertices = {
            0.5f,  0.5f, 0.0f,  // top right
            0.5f, -0.5f, 0.0f,  // bottom right
            -0.5f, -0.5f, 0.0f,  // bottom left
            -0.5f,  0.5f, 0.0f   // top left
        };
        uint[] _indices = {  // note that we start from 0!
            0, 1, 3,   // first triangle
            1, 2, 3    // second triangle
        };

        /*
            First Triangle(Indices 0, 1, 3:

   TopLeft(Indice 3)*---------------*TopRight(Indice 0)
                     \              |
                      \             |
                       \            |
                        \           |
                         \          |
                          \         |
                           \        |
                            \       |
                             \      |
                              \     |
                               \    |
                                \   |
                                 \  |
                                  \ |
                                   \|
                                    *BottomRight(Indice 1)
            
            Second Triangle(Indices 1, 2, 3):

                            *TopLeft(Indice 3)
                            |\
                            | \
                            |  \
                            |   \
                            |    \
                            |     \
                            |      \
                            |       \
                            |        \
                            |         \
                            |          \
                            |           \
                            |            \
                            |             \
                            |              \
                            |               \
                            |                \
                            |                 \
                            |                  \
                            |___________________\
        BottomLeft(Indice 2)*                    *BottomRight(Indice 1)
         */

        private Shader _shader;


        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {

        }


        
        protected override void OnLoad()
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            _vertexBufferObject = GL.GenBuffer();//Create a buffer object handle

            //Bind an array buffer to the buffer object
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

            //Send the array buffer data to the GPU's memory
            /*
            The fourth parameter is a BufferUsageHint, which specifies how we want the graphics
            card to manage the given data.This can take 3 forms:

            StaticDraw: the data will most likely not change at all or very rarely.
            DynamicDraw: the data is likely to change a lot.
            StreamDraw: the data will change every time it is drawn.

            The position data of the triangle does not change and stays the same for every render 
            call so its usage type should best be StaticDraw.If, for instance, one would have a buffer 
            with data that is likely to change frequently, a usage type of DynamicDraw or StreamDraw 
            ensures the graphics card will place the data in memory that allows for faster writes.
            */
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            //Create the VertexArrayObject(VAO)
            _vertexArrayObject = GL.GenVertexArray();//Create a vertext array object handle

            // Initialization code (done once unless your object frequently changes)
            //A VAO(Vertext Array Object) is basically a saved setup/configuration of a VBO
            // 1. bind Vertex Array Object
            GL.BindVertexArray(_vertexArrayObject);
            // 2. copy our vertices array in a buffer for OpenGL to use
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);
            // 3. then set our vertex attributes pointers
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);


            //Create the ElementBufferObject(EBO)
            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);


            _shader = new Shader("shader.vert", "shader.frag");

            base.OnLoad();
        }


        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            if (KeyboardState.IsKeyDown(Key.Escape))
            {
                Close();
            }


            base.OnUpdateFrame(args);
        }


        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);//Clear the screen to the color that was set in the OnLoad method

            _shader.Use();
            GL.BindVertexArray(_vertexArrayObject);

            //Draw the triangles described in _vertices and _indices
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();

            base.OnRenderFrame(args);
        }


        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, Size.X, Size.Y);

            base.OnResize(e);
        }


        protected override void OnUnload()
        {
            //Unbinds the buffer to prevent any accidental calls to this buffer which would end in a crash
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            //Delete the buffer data
            GL.DeleteBuffer(_vertexBufferObject);

            _shader.Dispose();
            base.OnUnload();
        }
    }
}
