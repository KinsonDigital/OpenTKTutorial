﻿using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Desktop;
using OpenToolkit.Graphics.OpenGL4;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System;

namespace OpenTKTutorial
{
    class Program
    {
        private static int _indexBufferID;
        private static int _vertexBufferID;
        private static int _vertexArrayID;

        static void Main(string[] args)
        {
            var gameWinSettings = new GameWindowSettings();
            var nativeWinSettings = new NativeWindowSettings();
            nativeWinSettings.Size = new Vector2i(1020, 800);


            var game = new Game(gameWinSettings, nativeWinSettings);

            game.Run();

            game.Dispose();
        }
    }
}
