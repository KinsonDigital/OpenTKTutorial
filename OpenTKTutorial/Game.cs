﻿using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Desktop;
using OpenToolkit.Windowing.Common.Input;
using OpenToolkit.Graphics.OpenGL4;
using System.IO;
using System.Reflection;
using NETColor = System.Drawing.Color;
using System.ComponentModel;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenToolkit.Mathematics;

namespace OpenTKTutorial
{
    /*References to batch redering
     * Cherno: https://www.youtube.com/watch?v=bw6JsLnx5Jg&list=PLlrATfBNZ98foTJPJ_Ev03o2oq3-GGOS2&index=31&t=0s
     */
    public class Game : GameWindow
    {
        #region Private Fields
        private Texture _redLink;
        private Texture _greenLink;
        private Texture _blueLink;
        private Texture _backgroundTexture;
        private Renderer _renderer;
        private bool _isShuttingDown;
        private double _elapsedTime;
        private Texture[] _textures;
        #endregion


        #region Constructors
        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            string name = "Hello World";

            GPU.Instance.ViewPortWidth = nativeWindowSettings.Size.X;
            GPU.Instance.ViewPortHeight = nativeWindowSettings.Size.Y;

            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);
            GL.DebugMessageCallback(DebugCallback, Marshal.StringToHGlobalAnsi(name));

            _renderer = new Renderer(nativeWindowSettings.Size.X, nativeWindowSettings.Size.Y);

            //48 = 1.5 batches = 2 draw calls for batch rendering
            _textures = TextureFactory.CreateTextures("Link.png", 1864);
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


        private List<double> _perfResults = new List<double>();
        private Stopwatch _timer = new Stopwatch();

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            if (_isShuttingDown)
                return;

            GL.Clear(ClearBufferMask.ColorBufferBit);

            _timer.Start();

            Render();

            _timer.Stop();

            _perfResults.Add(_timer.Elapsed.TotalMilliseconds);

            if (_perfResults.Count >= 1000)
            {
                var averageResult = _perfResults.Average();
                Debugger.Break();
            }

            _timer.Reset();

            SwapBuffers();

            base.OnRenderFrame(args);
        }


        private void Render()
        {
            _renderer.Begin();

            foreach (var texture in _textures)
            {
                _renderer.Render(texture);
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
            _redLink?.Dispose();
            _greenLink?.Dispose();
            _blueLink?.Dispose();

            _backgroundTexture?.Dispose();
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
