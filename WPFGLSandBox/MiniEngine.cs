using Accessibility;
using GLFW;
using OpenTKTutorial;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GLFWMonitor = GLFW.Monitor;
using GLFWWindow = GLFW.Window;
using GLFWErrorCode = GLFW.ErrorCode;
using System.Windows.Threading;

namespace WPFGLSandBox
{
    public class MiniEngine
    {
        private readonly string _basePath = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\";
        private readonly string _contentPath;
        private Renderer _renderer;
        private Task _runTask;
        private CancellationTokenSource _tokenSrc;
        private Texture _linkTexture;
        private Dispatcher _dispatcher;
        private GLFWWindow _glfwWindow;


        public MiniEngine(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            
            Glfw.Init();//Dont forget to call Glfw.Terminate();
            Glfw.SetErrorCallback(GLFWErrorCallback);

            Glfw.WindowHint(Hint.Visible, true);
            Glfw.WindowHint(Hint.ContextVersionMajor, 4);
            Glfw.WindowHint(Hint.ContextVersionMinor, 0);


            _glfwWindow = Glfw.CreateWindow(800, 600, "GLFW", GLFWMonitor.None, GLFW.Window.None);
            Glfw.MakeContextCurrent(_glfwWindow);
            Glfw.SwapInterval(1);

            GL.LoadBindings(new MyBindings());

            _contentPath = $@"{_basePath}Content\";
            _renderer = new Renderer(300, 300);
            _tokenSrc = new CancellationTokenSource();
            _runTask = new Task(EngineLoop, _tokenSrc.Token);
            _linkTexture = new Texture($@"{_contentPath}Graphics\Link.png")
            {
                X = 150,
                Y = 150
            };
        }


        public void Run()
        {
            _runTask.Start();
        }


        public void EngineLoop()
        {
            while (!_tokenSrc.IsCancellationRequested)
            {
                Thread.Sleep(500);

                _dispatcher.Invoke(() =>
                {
                    GL.Clear(ClearBufferMask.ColorBufferBit);

                    _renderer.Render(_linkTexture);

                    Glfw.SwapBuffers(_glfwWindow);
                });
            }
        }


        private void GLFWErrorCallback(GLFWErrorCode code, IntPtr ptr)
        {

        }
    }
}
