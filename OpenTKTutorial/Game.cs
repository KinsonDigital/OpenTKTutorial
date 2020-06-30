// <copyright file="Game.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace OpenTKTutorial
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using OpenToolkit.Graphics.OpenGL4;
    using OpenToolkit.Mathematics;
    using OpenToolkit.Windowing.Common;
    using OpenToolkit.Windowing.Common.Input;
    using OpenToolkit.Windowing.Desktop;
    using NETColor = System.Drawing.Color;

    /// <summary>
    /// The main game window.
    /// </summary>
    public class Game : GameWindow
    {
        private readonly SpriteBatch spriteBatch;
        private readonly Dictionary<int, ITexture> texturePool = new Dictionary<int, ITexture>();
        private readonly List<AtlasEntity> linkEntities = new List<AtlasEntity>();
        private readonly int atlasID;
        private readonly int linkTextureID;
        private readonly Dictionary<string, AtlasSubRect> atlasSubRects;
        private readonly int totalEntities = 10;
        private bool isShuttingDown;
        private double elapsedTime;
        private int backgroundTextureID;

        // TODO: Need to finish the custom batching process including setting the total batch size in the shaders
        // TODO: Need to add color to the vertex buffer and update its data
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Exception message only used inside method.")]
        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            if (nativeWindowSettings is null)
                throw new ArgumentNullException(nameof(nativeWindowSettings), "The argument must not be null");

            var name = "Hello World";

            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);
            GL.DebugMessageCallback(DebugCallback, Marshal.StringToHGlobalAnsi(name));

            this.spriteBatch = new SpriteBatch(nativeWindowSettings.Size.X, nativeWindowSettings.Size.Y);

            var backgroundTexture = ContentLoader.CreateTexture("dungeon.png");
            this.texturePool.Add(backgroundTexture.ID, backgroundTexture);
            this.backgroundTextureID = backgroundTexture.ID;

            var linkTexture = ContentLoader.CreateTexture("link.png");
            this.texturePool.Add(linkTexture.ID, linkTexture);
            this.linkTextureID = linkTexture.ID;

            //var mainAtlasTexture = ContentLoader.CreateTexture("main-atlas.png");
            //this.texturePool.Add(mainAtlasTexture.ID, mainAtlasTexture);
            //this.atlasID = mainAtlasTexture.ID;

            // Load the atlas sub rectangle data
            this.atlasSubRects = ContentLoader.LoadAtlasData("atlas-data.json");

            //var random = new Random();

            //for (var i = 0; i < this.totalEntities; i++)
            //{
            //    var newEntity = new AtlasEntity(mainAtlasTexture.ID, this.atlasSubRects["link"])
            //    {
            //        Position = new Vector2(random.Next(50, 970), random.Next(120, 680)),
            //        TintColor = Color.FromArgb(
            //            255,
            //            random.Next(0, 255),
            //            random.Next(0, 255),
            //            random.Next(0, 255)),
            //    };

            //    this.linkEntities.Add(newEntity);
            //}
        }

        public Vector2 ScreenCenter => new Vector2(this.Size.X / 2f, this.Size.Y / 2);

        protected override void OnLoad()
        {
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            if (this.isShuttingDown)
                return;

            //if (KeyboardState.IsKeyDown(Key.Escape))
            //    Close();

            //var totalTime = 4000;

            //// Use easing functions to gradually change texture values
            //var alphaResult = (int)EasingFunctions.EaseOutBounce(this.elapsedTime * 1000, 0, 255, totalTime);

            //alphaResult = alphaResult > 255 ? 255 : alphaResult;

            //for (var i = 0; i < this.linkEntities.Count; i++)
            //{
            //    this.linkEntities[i].Position = new Vector2((float)EasingFunctions.EaseOutBounce(this.elapsedTime * 1000, 100, 800, totalTime), this.linkEntities[i].Position.Y);

            //    this.linkEntities[i].TintColor = NETColor.FromArgb(
            //        alphaResult,
            //        this.linkEntities[i].TintColor.R,
            //        this.linkEntities[i].TintColor.G,
            //        this.linkEntities[i].TintColor.B);

            //    this.linkEntities[i].Angle = (float)EasingFunctions.EaseOutBounce(this.elapsedTime * 1000, 0, 360, totalTime);
            //    this.linkEntities[i].Size = (float)EasingFunctions.EaseOutBounce(this.elapsedTime * 1000, 0.2f, 0.7f, totalTime);
            //}

            //// If the total time for the easing functions
            //// to finish has expired, reset everything.
            //this.elapsedTime = this.elapsedTime * 1000 > totalTime
            //    ? 0
            //    : this.elapsedTime += args.Time;

            base.OnUpdateFrame(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            if (this.isShuttingDown)
                return;

            GL.Clear(ClearBufferMask.ColorBufferBit);

            Render();

            SwapBuffers();

            base.OnRenderFrame(args);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            // TODO: Setup renderer to updates its render surface width and height
            GL.Viewport(0, 0, Size.X, Size.Y);

            base.OnResize(e);
        }

        protected override void OnClosing(CancelEventArgs e) => base.OnClosing(e);

        protected override void OnUnload()
        {
            this.isShuttingDown = true;
            base.OnUnload();
        }

        private void Render()
        {
            this.spriteBatch.Begin();

            var backgroundTexture = this.texturePool[this.backgroundTextureID];
            var backgroundSrcRect = new Rectangle()
            {
                X = 0,
                Y = 0,
                Width = this.texturePool[this.backgroundTextureID].Width,
                Height = this.texturePool[this.backgroundTextureID].Height,
            };

            var backgroundDestRect = new Rectangle()
            {
                X = (int)ScreenCenter.X,
                Y = (int)ScreenCenter.Y,
                Width = this.texturePool[this.backgroundTextureID].Width,
                Height = this.texturePool[this.backgroundTextureID].Height,
            };

            this.spriteBatch.Render(backgroundTexture, backgroundSrcRect, backgroundDestRect, 1, 0, NETColor.White);

            var linkTexture = this.texturePool[this.linkTextureID];

            var linkSrcRect = new Rectangle()
            {
                X = 0,
                Y = 0,
                Width = this.texturePool[this.linkTextureID].Width,
                Height = this.texturePool[this.linkTextureID].Height,
            };

            var linkDestRect = new Rectangle()
            {
                X = 400,
                Y = 400,
                Width = this.texturePool[this.linkTextureID].Width,
                Height = this.texturePool[this.linkTextureID].Height,
            };

            this.spriteBatch.Render(linkTexture, linkSrcRect, linkDestRect, 1, 0, NETColor.White);

            //var atlasTexture = this.texturePool[this.atlasID];

            //for (var i = 0; i < this.linkEntities.Count; i++)
            //{
            //    var destRect = new Rectangle((int)this.linkEntities[i].Position.X, (int)this.linkEntities[i].Position.Y, atlasTexture.Width, atlasTexture.Height);

            //    this.spriteBatch.Render(
            //        atlasTexture,
            //        this.linkEntities[i].AtlasSubRect.ToRectangle(),
            //        destRect,
            //        this.linkEntities[i].Size,
            //        this.linkEntities[i].Angle,
            //        this.linkEntities[i].TintColor);
            //    break;
            //}

            this.spriteBatch.End();
        }

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
