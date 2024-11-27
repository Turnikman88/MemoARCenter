using MemoARCenter.Services.Contracts;
using MemoARCenter.Services.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MemoARCenter.Services.Services
{
    public class ImageEditService : IImageEdit
    {
        private const int _targetSizeInBytes = 100 * 1024;
        private int _quality = 90;
        public static readonly List<string> ValidImageExtensions = new List<string>(){ ".jpg", ".jpeg", ".png", ".bmp", ".gif" };

        public ImageInfoDTO ResizeImage(string imagePath, int maxWidth, int maxHeight, int maxSizeKB)
        {
            using var originalBitmap = SKBitmap.Decode(imagePath);

            int width, height;
            CalculateDimentions();

            using var resizedBitmap = originalBitmap.Resize(new SKImageInfo(width, height), SKFilterQuality.Medium);

            using var resizedImage = SKImage.FromBitmap(resizedBitmap);
            using var outputStream = new MemoryStream();

            resizedImage.Encode(SKEncodedImageFormat.Jpeg, 100).SaveTo(outputStream);


            while (outputStream.Length > _targetSizeInBytes)
            {
                if (_quality <= 15) 
                {
                    break;
                }

                outputStream.SetLength(0); 
                resizedImage.Encode(SKEncodedImageFormat.Jpeg, _quality).SaveTo(outputStream);

                _quality -= 5; 
            }

            var result = new ImageInfoDTO(outputStream.ToArray(), width, height); 

            return result;

            void CalculateDimentions()
            {
                var aspectRatio = (float)originalBitmap.Width / originalBitmap.Height;
                width = maxWidth;
                height = (int)(maxWidth / aspectRatio);

                if (height > maxHeight)
                {
                    height = maxHeight;
                    width = (int)(maxHeight * aspectRatio);
                }
            }
        }

        public bool IsImageFile(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return ValidImageExtensions.Contains(extension);
        }
    }
}
