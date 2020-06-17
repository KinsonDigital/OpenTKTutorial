using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Desktop;
using OpenToolkit.Windowing.Common.Input;
using OpenToolkit.Graphics.OpenGL4;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NETColor = System.Drawing.Color;
using OpenToolkit.Mathematics;
using System.Drawing;

namespace OpenTKTutorial
{
    /*References to batch redering
     * Cherno: https://www.youtube.com/watch?v=bw6JsLnx5Jg&list=PLlrATfBNZ98foTJPJ_Ev03o2oq3-GGOS2&index=31&t=0s
     */
    public class Game : GameWindow
    {
        #region Private Fields
        private ShaderProgram _shader;
        private bool _isShuttingDown;
        private double _elapsedTime;
        private Renderer _renderer;
        private Vector2 _linkPosition;
        private readonly Dictionary<int, ITexture> _texturePool = new Dictionary<int, ITexture>();
        private Entity _backgroundEntity;
        private readonly List<AtlasEntity> _linkEntities = new List<AtlasEntity>();
        private readonly int _atlasID;
        private readonly Dictionary<string, AtlasSubRect> _atlasSubRects;
        private Entity _linkEntity;
        #endregion

        //TODO: Need to finish the custom batching process including setting the total batch size in the shaders
        //TODO: Need to add color to the vertex buffer and update its data

        #region Constructors
        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            string name = "Hello World";

            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);
            GL.DebugMessageCallback(DebugCallback, Marshal.StringToHGlobalAnsi(name));

            _shader = new ShaderProgram("shader.vert", "shader.frag");
            _renderer = new Renderer(_shader, nativeWindowSettings.Size.X, nativeWindowSettings.Size.Y);

            var backgroundTexture = ContentLoader.CreateTexture("dungeon.png");
            _texturePool.Add(backgroundTexture.ID, backgroundTexture);

            var mainAtlasTexture = ContentLoader.CreateTexture("main-atlas.png");
            _texturePool.Add(mainAtlasTexture.ID, mainAtlasTexture);

            _atlasID = mainAtlasTexture.ID;

            //Load the atlas sub rectangle data
            _atlasSubRects = ContentLoader.LoadAtlasData("atlas-data.json");

            _backgroundEntity = new Entity(backgroundTexture.ID);

            _linkPosition = new Vector2(0, 0);

            var random = new Random();

            for (int i = 0; i < 48; i++)
            {
                var newEntity = new AtlasEntity(mainAtlasTexture.ID, _atlasSubRects["link"]) 
                {
                    Position = new Vector2(random.Next(0, 1020), random.Next(0, 800)),
                    TintColor = Color.FromArgb(
                        255,
                        random.Next(0, 255),
                        random.Next(0, 255),
                        random.Next(0, 255))
                };

                _linkEntities.Add(newEntity);
            }
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


            var totalTime = 4000;

            //Use easing functions to gradually change texture values
            var alphaResult = (int)EasingFunctions.EaseOutBounce(_elapsedTime * 1000, 0, 255, totalTime);

            alphaResult = alphaResult > 255 ? 255 : alphaResult;

            //_linkTexture.X = (float)EasingFunctions.EaseOutBounce(_elapsedTime * 1000, 200, 400, totalTime);

            //_linkTexture.TintColor = NETColor.FromArgb(alphaResult,
            //                                           _linkTexture.TintColor.R,
            //                                           _linkTexture.TintColor.G,
            //                                           _linkTexture.TintColor.B);

            //_linkTexture.Angle = (float)EasingFunctions.EaseOutBounce(_elapsedTime * 1000, 0, 360, totalTime);

            //_linkTexture.Size = (float)EasingFunctions.EaseOutBounce(_elapsedTime * 1000, 0.5f, 0.5f, totalTime);

            //If the total time for the easing functions
            //to finish has expired, reset everything.
            _elapsedTime = _elapsedTime * 1000 > totalTime
                ? 0
                : _elapsedTime += args.Time;

            base.OnUpdateFrame(args);
        }


        private List<double> _perfTimes = new List<double>();
        private Stopwatch _timer = new Stopwatch();


        protected override void OnRenderFrame(FrameEventArgs args)
        {
            if (_isShuttingDown)
                return;

            GL.Clear(ClearBufferMask.ColorBufferBit);

            _timer.Start();

            Render();

            _timer.Stop();

            _perfTimes.Add(_timer.Elapsed.TotalMilliseconds);

            if (_perfTimes.Count >= 1000)
            {
                var averageResult = _perfTimes.Average();
                Debugger.Break();
            }

            _timer.Reset();

            SwapBuffers();

            base.OnRenderFrame(args);
        }


        private void Render()
        {
            _renderer.Begin();

            var backgroundTexture = _texturePool[_backgroundEntity.TextureID];

            _renderer.Render(
                backgroundTexture,
                new Rectangle(0, 0, backgroundTexture.Width, backgroundTexture.Height),
                new Rectangle(0, 0, backgroundTexture.Width, backgroundTexture.Height),
                1,
                0,
                NETColor.White);

            var atlasTexture = _texturePool[_atlasID];

            for (int i = 0; i < _linkEntities.Count; i++)
            {
                var destRect = new Rectangle((int)_linkEntities[i].Position.X, (int)_linkEntities[i].Position.Y, atlasTexture.Width, atlasTexture.Height);

                _renderer.Render(atlasTexture, _linkEntities[i].AtlasSubRect.ToRectangle(), destRect, 1, 0, NETColor.White);
            }

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
            base.OnClosing(e);
        }


        protected override void OnUnload()
        {
            _isShuttingDown = true;
            base.OnUnload();
        }
        #endregion


        private void DebugCallback(DebugSource src, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            var errorMessage = Marshal.PtrToStringAnsi(message);

            errorMessage += errorMessage;
            errorMessage += $"\n\tSrc: {src}";
            errorMessage += $"\n\tType: {type}";
            errorMessage += $"\n\tID: {id}";
            errorMessage += $"\n\tSeverity: {severity}";
            errorMessage += $"\n\tLength: {length}";
            errorMessage += $"\n\tUser Param: {Marshal.PtrToStringAnsi(userParam)}";

            if (severity != DebugSeverity.DebugSeverityNotification)
            {
                throw new Exception(errorMessage);
            }
        }
    }
}
