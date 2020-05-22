using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Desktop;
using GLFW;
using System.Runtime.InteropServices;
using System;

namespace OpenTKTutorial
{
    class Program
    {
        private const int GL_COLOR_BUFFER_BIT = 0x00004000;

        private delegate void glClearColorHandler(float r, float g, float b, float a);
        private delegate void glClearHandler(int mask);

        private static glClearColorHandler glClearColor;
        private static glClearHandler glClear;
        private static Random rand;


        static void Main(string[] args)
        {
            // Set some common hints for the OpenGL profile creation
            Glfw.WindowHint(Hint.ClientApi, ClientApi.OpenGL);
            Glfw.WindowHint(Hint.ContextVersionMajor, 3);
            Glfw.WindowHint(Hint.ContextVersionMinor, 3);
            Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);
            Glfw.WindowHint(Hint.Doublebuffer, true);
            Glfw.WindowHint(Hint.Decorated, true);

            rand = new Random();

            var window = Glfw.CreateWindow(800, 600, "GLFW", Monitor.None, Window.None);
            Glfw.MakeContextCurrent(window);

            Glfw.SwapInterval(1);

            // Find center position based on window and monitor sizes
            var screenSize = Glfw.PrimaryMonitor.WorkArea;
            var x = (screenSize.Width - 800) / 2;
            var y = (screenSize.Height - 600) / 2;
            Glfw.SetWindowPosition(window, x, y);

            // Set a key callback
            Glfw.SetKeyCallback(window, KeyCallback);


            glClearColor = Marshal.GetDelegateForFunctionPointer<glClearColorHandler>(Glfw.GetProcAddress("glClearColor"));
            glClear = Marshal.GetDelegateForFunctionPointer<glClearHandler>(Glfw.GetProcAddress("glClear"));


            var tick = 0L;
            ChangeRandomColor();

            while (!Glfw.WindowShouldClose(window))
            {
                // Poll for OS events and swap front/back buffers
                Glfw.PollEvents();
                Glfw.SwapBuffers(window);

                // Change background color to something random every 60 draws
                if (tick++ % 60 == 0)
                    ChangeRandomColor();

                // Clear the buffer to the set color
                glClear(GL_COLOR_BUFFER_BIT);
            }

            return;
            var gameWinSettings = new GameWindowSettings();
            var nativeWinSettings = new NativeWindowSettings();
            nativeWinSettings.Size = new Vector2i(1020, 800);


            var game = new Game(gameWinSettings, nativeWinSettings);

            game.Run();

            game.Dispose();
        }


        private static void ChangeRandomColor()
        {
            var r = (float)rand.NextDouble();
            var g = (float)rand.NextDouble();
            var b = (float)rand.NextDouble();
            glClearColor(r, g, b, 1.0f);
        }

        private static void KeyCallback(Window window, Keys key, int scancode, InputState state, ModifierKeys mods)
        {
            switch (key)
            {
                case Keys.Escape:
                    Glfw.SetWindowShouldClose(window, true);
                    break;
            }
        }
    }
}
