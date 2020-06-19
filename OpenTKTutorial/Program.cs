// <copyright file="Program.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace OpenTKTutorial
{
    using OpenToolkit.Mathematics;
    using OpenToolkit.Windowing.Desktop;

    public static class Program
    {
        public static void Main()
        {
            var gameWinSettings = new GameWindowSettings();
            var nativeWinSettings = new NativeWindowSettings
            {
                Size = new Vector2i(1020, 800),
            };

            var game = new Game(gameWinSettings, nativeWinSettings);

            game.Run();

            game.Dispose();
        }
    }
}
