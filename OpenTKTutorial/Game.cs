using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Desktop;
using OpenToolkit.Windowing.Common.Input;
using OpenToolkit.Graphics.OpenGL4;
using System.IO;
using System.Reflection;
using NETColor = System.Drawing.Color;
using System.ComponentModel;

namespace OpenTKTutorial
{
    /*References to batch redering
     * Cherno: https://www.youtube.com/watch?v=bw6JsLnx5Jg&list=PLlrATfBNZ98foTJPJ_Ev03o2oq3-GGOS2&index=31&t=0s
     */
    public class Game : GameWindow
    {
        #region Private Fields
        private readonly string _appPathDir = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\";
        private readonly string _contentDir;
        private readonly string _graphicsContent;
        private Texture _linkTexture;
        private Texture _backgroundTexture;
        private Renderer _renderer;
        private bool _isShuttingDown;
        private double _elapsedTime;
        #endregion


        #region Constructors
        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            var totalBytes = VertexDataAnalyzer.GetTotalBytesForStruct(typeof(VertexData));

            _contentDir = $@"{_appPathDir}Content\";
            _graphicsContent = $@"{_contentDir}Graphics\";

            _renderer = new Renderer(nativeWindowSettings.Size.X, nativeWindowSettings.Size.Y);


            _backgroundTexture = new Texture($"{_graphicsContent}dungeon.png", _renderer.Shader.ProgramId)
            {
                X = Size.X / 2,
                Y = Size.Y / 2,
                TextureSlot = 0
            };

            _linkTexture = new Texture($"{_graphicsContent}Link.png", _renderer.Shader.ProgramId)
            {
                X = Size.X / 2,
                Y = Size.Y / 2,
                Angle = 0,
                TintColor = NETColor.FromArgb(255, 0, 0, 255),
                TextureSlot = 1
            };
        }
        #endregion


        #region Protected Methods
        protected override void OnLoad()
        {
            
        }


        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            if (_isShuttingDown)
                return;

            if (KeyboardState.IsKeyDown(Key.Escape))
                Close();


            //var totalTime = 4000;

            ////Use easing functions to gradually change texture values
            //var alphaResult = (int)EasingFunctions.EaseOutBounce(_elapsedTime * 1000, 0, 255, totalTime);

            //alphaResult = alphaResult > 255 ? 255 : alphaResult;

            //_linkTexture.X = (float)EasingFunctions.EaseOutBounce(_elapsedTime * 1000, 200, 400, totalTime);

            //_linkTexture.TintColor = NETColor.FromArgb(alphaResult,
            //                                           _linkTexture.TintColor.R,
            //                                           _linkTexture.TintColor.G,
            //                                           _linkTexture.TintColor.B);

            //_linkTexture.Angle = (float)EasingFunctions.EaseOutBounce(_elapsedTime * 1000, 0, 360, totalTime);

            //_linkTexture.Size = (float)EasingFunctions.EaseOutBounce(_elapsedTime * 1000, 0.5f, 0.5f, totalTime);

            ////If the total time for the easing functions
            ////to finish has expired, reset everything.
            //_elapsedTime = _elapsedTime * 1000 > totalTime
            //    ? 0
            //    : _elapsedTime += args.Time;

            base.OnUpdateFrame(args);
        }
         

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            if (_isShuttingDown)
                return;

            GL.Clear(ClearBufferMask.ColorBufferBit);


            //RenderOLD();
            RenderNEW();

            SwapBuffers();

            base.OnRenderFrame(args);
        }


        private void RenderNEW()
        {
            _renderer.Begin();
            _renderer.Render(_backgroundTexture);
            _renderer.Render(_linkTexture);
            _renderer.End();
        }


        protected override void OnResize(ResizeEventArgs e)
        {
            //TODO: Setup renderer to updates its render surface width and height
            GL.Viewport(0, 0, Size.X, Size.Y);

            base.OnResize(e);
        }


        protected override void OnClosing(CancelEventArgs e)
        {
            _linkTexture.Dispose();
            _backgroundTexture.Dispose();
            _renderer.Dispose();
            base.OnClosing(e);
        }


        protected override void OnUnload()
        {
            _isShuttingDown = true;
            base.OnUnload();
        }
        #endregion
    }
}
