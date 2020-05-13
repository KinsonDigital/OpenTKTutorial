using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Desktop;
using OpenToolkit.Windowing.Common.Input;
using OpenToolkit.Graphics.OpenGL4;
using System.IO;
using System.Reflection;
using System;
using System.Runtime.InteropServices;

namespace OpenTKTutorial
{
    public class Game : GameWindow
    {
        private readonly string _appPathDir = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\";
        private readonly string _contentDir;
        private readonly string _graphicsContent;
        private Renderer _renderer;
        private int _vertexBufferObject;
        
        private int _elementBufferObject;


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
        private Texture _linkTexture;
        private readonly Texture _backgroundTexture;

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            _contentDir = $@"{_appPathDir}Content\";
            _graphicsContent = $@"{_contentDir}Graphics\";
            _linkTexture = new Texture($"{_graphicsContent}Link.png")
            {
                X = 410,
                Y = 310
            };

            _backgroundTexture = new Texture($"{_graphicsContent}Dungeon.png")
            {
                X = Size.X / 2,
                Y = Size.Y / 2
            };
        }


        
        protected override void OnLoad()
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            _renderer = new Renderer(Size.X, Size.Y);

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
            GL.Clear(ClearBufferMask.ColorBufferBit);

            _renderer.Render(_backgroundTexture);
            _renderer.Render(_linkTexture);

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
