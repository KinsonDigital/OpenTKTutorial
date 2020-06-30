// <copyright file="Game.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace OpenTKTutorial
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Numerics;
    using System.Runtime.InteropServices;
    using Silk.NET.OpenGL;
    using Silk.NET.Windowing;
    using Silk.NET.Windowing.Common;
    using NETColor = System.Drawing.Color;

    /// <summary>
    /// The main game window.
    /// </summary>
    public class Game
    {
        private SpriteBatch spriteBatch;
        private readonly Dictionary<uint, ITexture> texturePool = new Dictionary<uint, ITexture>();
        private readonly List<AtlasEntity> linkEntities = new List<AtlasEntity>();
        private readonly int atlasID;
        private uint linkTextureID;
        private Dictionary<string, AtlasSubRect> atlasSubRects;
        private readonly int totalEntities = 10;
        private bool isShuttingDown;
        private double elapsedTime;
        private uint backgroundTextureID;
        private GL gl;
        private IWindow window;


        // TODO: Need to finish the custom batching process including setting the total batch size in the shaders
        // TODO: Need to add color to the vertex buffer and update its data
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Exception message only used inside method.")]
        public unsafe Game()
        {
            this.window = Window.Create(WindowOptions.Default);
            this.window.Load += Window_Load;
            this.window.Update += Window_Update;
            this.window.Render += Window_Render;
            this.window.Resize += Window_Resize;
            this.window.Closing += Window_Closing;
        }

        public void Run()
        {
            this.window.Run();
        }

        private unsafe void Window_Load()
        {
            this.gl = GL.GetApi(this.window);

            this.gl.Enable(EnableCap.DebugOutput);
            this.gl.Enable(EnableCap.DebugOutputSynchronous);
            this.gl.DebugMessageCallback(DebugCallback, null);

            this.spriteBatch = new SpriteBatch(this.gl, 1020, 800);

            var backgroundTexture = ContentLoader.CreateTexture(this.gl, "dungeon.png");
            this.texturePool.Add(backgroundTexture.ID, backgroundTexture);
            this.backgroundTextureID = backgroundTexture.ID;

            var linkTexture = ContentLoader.CreateTexture(this.gl, "link.png");
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

        private void Window_Resize(Size obj)
        {
            this.gl.Viewport(obj);
        }

        private void Window_Update(double obj)
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
        }

        private void Window_Render(double obj)
        {
            if (this.isShuttingDown)
                return;

            this.gl.Clear((uint)ClearBufferMask.ColorBufferBit);

            Render();
        }

        public Vector2 ScreenCenter => new Vector2(1020 / 2f, 800 / 2);


        private void Window_Closing()
        {
            this.isShuttingDown = true;
        }

        private void Render()
        {
            this.spriteBatch.Begin();

            var backgroundTexture = this.texturePool[this.backgroundTextureID];
            var backgroundSrcRect = new Rectangle()
            {
                X = 0,
                Y = 0,
                Width = (int)this.texturePool[this.backgroundTextureID].Width,
                Height = (int)this.texturePool[this.backgroundTextureID].Height,
            };

            var backgroundDestRect = new Rectangle()
            {
                X = (int)ScreenCenter.X,
                Y = (int)ScreenCenter.Y,
                Width = (int)this.texturePool[this.backgroundTextureID].Width,
                Height = (int)this.texturePool[this.backgroundTextureID].Height,
            };

            this.spriteBatch.Render(backgroundTexture, backgroundSrcRect, backgroundDestRect, 1, 0, NETColor.White);

            var linkTexture = this.texturePool[this.linkTextureID];

            var linkSrcRect = new Rectangle()
            {
                X = 0,
                Y = 0,
                Width = (int)this.texturePool[this.linkTextureID].Width,
                Height = (int)this.texturePool[this.linkTextureID].Height,
            };

            var linkDestRect = new Rectangle()
            {
                X = 400,
                Y = 400,
                Width = (int)this.texturePool[this.linkTextureID].Width,
                Height = (int)this.texturePool[this.linkTextureID].Height,
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

        private void DebugCallback(GLEnum src, GLEnum type, int id, GLEnum severity, int length, IntPtr message, IntPtr userParam)
        {
            var errorMessage = Marshal.PtrToStringAnsi(message);

            errorMessage += errorMessage;
            errorMessage += $"\n\tSrc: {src}";
            errorMessage += $"\n\tType: {type}";
            errorMessage += $"\n\tID: {id}";
            errorMessage += $"\n\tSeverity: {severity}";
            errorMessage += $"\n\tLength: {length}";
            errorMessage += $"\n\tUser Param: {Marshal.PtrToStringAnsi(userParam)}";

            if (severity != GLEnum.DebugSeverityNotification)
            {
                throw new Exception(errorMessage);
            }
        }
    }
}
