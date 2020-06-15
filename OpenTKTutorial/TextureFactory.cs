using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NETColor = System.Drawing.Color;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;

namespace OpenTKTutorial
{
    public static class TextureFactory
    {
        private readonly static string _appPathDir = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\";
        private static string _contentDir;
        private static string _graphicsContent;
        private const int WINDOW_WIDTH = 1020;
        private const int WINDOW_HEIGHT = 800;


        public static Texture[] CreateTextures(string fileName, uint count)
        {
            var random = new Random();
            _contentDir = $@"{_appPathDir}Content\";
            _graphicsContent = $@"{_contentDir}Graphics\";


            var result = new List<Texture>();

            var (pixelData, width, height) = LoadImageData($"{_graphicsContent}{fileName}");

            for (int i = 0; i < count; i++)
            {
                result.Add(new Texture(pixelData, width, height, $"{Path.GetFileNameWithoutExtension(fileName)}-{i}")
                {
                    X = random.Next(0, WINDOW_WIDTH),
                    Y = random.Next(0, WINDOW_HEIGHT),
                    TintColor = NETColor.FromArgb(255, 255, 255, 255)
                });
            }


            return result.ToArray();
        }



        private static (byte[] pixelData, int width, int height) LoadImageData(string texturePath)
        {
            //Load the image
            var image = (Image<Rgba32>)Image.Load(texturePath);

            //ImageSharp loads from the top-left pixel, whereas OpenGL loads from the bottom-left, causing the texture to be flipped vertically.
            //This will correct that, making the texture display properly.
            image.Mutate(x => x.Flip(FlipMode.Vertical));

            //Get an array of the pixels, in ImageSharp's internal format.
            var tempPixels = new List<Rgba32>();

            for (int i = 0; i < image.Height; i++)
            {
                var rowPixels = image.GetPixelRowSpan(i).ToArray();

                tempPixels.AddRange(image.GetPixelRowSpan(i).ToArray());
            }

            //Convert ImageSharp's format into a byte array, so we can use it with OpenGL.
            List<byte> pixels = new List<byte>();

            foreach (Rgba32 p in tempPixels)
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
