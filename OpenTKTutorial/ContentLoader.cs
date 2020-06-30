// <copyright file="ContentLoader.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace OpenTKTutorial
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text.Json;
    using Silk.NET.OpenGL;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    public static class ContentLoader
    {
        private static readonly string AppPathDir = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\";

        public static ITexture[] CreateTextures(GL gl, string fileName, uint count)
        {
            var contentDir = $@"{AppPathDir}Content\";
            var graphicsContent = $@"{contentDir}Graphics\";

            var result = new List<Texture>();

            var (pixelData, width, height) = LoadImageData($"{graphicsContent}{fileName}");

            for (var i = 0; i < count; i++)
            {
                result.Add(new Texture(gl, pixelData, (uint)width, (uint)height, $"{Path.GetFileNameWithoutExtension(fileName)}-{i}"));
            }

            return result.ToArray();
        }

        public static ITexture CreateTexture(GL gl, string fileName)
        {
            var textures = CreateTextures(gl, fileName, 1);

            return textures[0];
        }

        public static Dictionary<string, AtlasSubRect> LoadAtlasData(string fileName)
        {
            var result = new Dictionary<string, AtlasSubRect>();

            var contentDir = $@"{AppPathDir}Content\";
            var graphicsContent = $@"{contentDir}Graphics\";

            var rawData = File.ReadAllText($"{graphicsContent}{fileName}");

            var rectItems = JsonSerializer.Deserialize<AtlasSubRect[]>(rawData);

            foreach (var item in rectItems)
            {
                result.Add(item.Name, item);
            }

            return result;
        }

        private static (byte[] pixelData, int width, int height) LoadImageData(string texturePath)
        {
            // Load the image
            var image = (Image<Rgba32>)Image.Load(texturePath);

            // ImageSharp loads from the top-left pixel, whereas OpenGL loads from the bottom-left, causing the texture to be flipped vertically.
            // This will correct that, making the texture display properly.
            image.Mutate(x => x.Flip(FlipMode.Vertical));

            // Get an array of the pixels, in ImageSharp's internal format.
            var tempPixels = new List<Rgba32>();

            for (var i = 0; i < image.Height; i++)
            {
                var rowPixels = image.GetPixelRowSpan(i).ToArray();

                tempPixels.AddRange(image.GetPixelRowSpan(i).ToArray());
            }

            // Convert ImageSharp's format into a byte array, so we can use it with OpenGL.
            var pixels = new List<byte>();

            foreach (var p in tempPixels)
            {
                pixels.Add(p.R);
                pixels.Add(p.G);
                pixels.Add(p.B);
                pixels.Add(p.A);
            }

            return (pixels.ToArray(), image.Width, image.Height);
        }
    }
}
