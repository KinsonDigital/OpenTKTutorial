using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Desktop;
using System.Runtime.InteropServices.ComTypes;

namespace OpenTKTutorial
{
    public class Program
    {
        public static void Main(string[] args)
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
