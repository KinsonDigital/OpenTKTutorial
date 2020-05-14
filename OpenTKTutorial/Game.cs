using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Desktop;
using OpenToolkit.Windowing.Common.Input;
using OpenToolkit.Graphics.OpenGL4;
using System.IO;
using System.Reflection;
using NETColor = System.Drawing.Color;

namespace OpenTKTutorial
{
    public class Game : GameWindow
    {
        #region Private Fields
        private readonly string _appPathDir = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\";
        private readonly string _contentDir;
        private readonly string _graphicsContent;
        private Renderer _renderer;
        private Texture _linkTexture;
        private readonly Texture _backgroundTexture;
        private double _elapsedTime;
        #endregion


        #region Constructors
        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            _contentDir = $@"{_appPathDir}Content\";
            _graphicsContent = $@"{_contentDir}Graphics\";
            _renderer = new Renderer(Size.X, Size.Y);
            _linkTexture = new Texture($"{_graphicsContent}Link.png", _renderer.Shader)
            {
                X = 200,
                Y = Size.Y / 2,
                Angle = 0,
                TintColor = NETColor.FromArgb(125, 255, 255, 255)
            };

            _backgroundTexture = new Texture($"{_graphicsContent}Dungeon.png", _renderer.Shader)
            {
                X = Size.X / 2,
                Y = Size.Y / 2
            };
        }
        #endregion


        #region Protected Methods
        protected override void OnLoad()
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        }


        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            if (KeyboardState.IsKeyDown(Key.Escape))
                Close();

            var totalTime = 4000;

            //Use easing functions to gradually change texture values
            var alphaResult = (int)EasingFunctions.EaseOutBounce(_elapsedTime * 1000, 0, 255, totalTime);

            alphaResult = alphaResult > 255 ? 255 : alphaResult;

            _linkTexture.X = (float)EasingFunctions.EaseOutBounce(_elapsedTime * 1000, 200, 400, totalTime);

            _linkTexture.TintColor = NETColor.FromArgb(alphaResult,
                                                       _linkTexture.TintColor.R,
                                                       _linkTexture.TintColor.G,
                                                       _linkTexture.TintColor.B);

            _linkTexture.Angle = (float)EasingFunctions.EaseOutBounce(_elapsedTime * 1000, 0, 360, totalTime);

            _linkTexture.Size = (float)EasingFunctions.EaseOutBounce(_elapsedTime * 1000, 0.5f, 0.5f, totalTime);

            //If the total time for the easing functions
            //to finish has expired, reset everything.
            _elapsedTime = _elapsedTime * 1000 > totalTime
                ? 0
                : _elapsedTime += args.Time;

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
            _linkTexture.Dispose();
            _backgroundTexture.Dispose();
            _renderer.Dispose();
            base.OnUnload();
        }
        #endregion
    }
}
