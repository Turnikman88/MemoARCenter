using MemoARCenter.Helpers.Models.DTOs;
using MemoARCenter.Helpers.Models.System;
using MemoARCenter.Services.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkiaSharp;

namespace MemoARCenter.Services.Services
{
    public class ImageEditService : IImageEdit
    {
        private readonly AppSettings _settings;
        private readonly ILogger<ImageEditService> _log;

        private int _targetSizeInBytes = 50 * 1024;
        private int _quality = 90;

        public static List<string> ValidImageExtensions = new List<string>();

        public ImageEditService(IOptions<AppSettings> settings, ILogger<ImageEditService> log)
        {
            _settings = settings.Value;
            _log = log;

            SetConfigs();
        }

        public ImageInfoDTO ResizeImage(string imagePath, int maxWidth, int maxHeight, int maxSizeKB)
        {
            _log.LogDebug("Start resizing image");

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
                    _log.LogDebug("Quality is less than 15 - stopping");

                    break;
                }

                outputStream.SetLength(0);
                resizedImage.Encode(SKEncodedImageFormat.Jpeg, _quality).SaveTo(outputStream);

                _quality -= 5;
                _log.LogDebug($"Quality is down to {_quality}");
            }

            var result = new ImageInfoDTO(outputStream.ToArray(), width, height);

            _log.LogDebug("Image generation finished");

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

        private void SetConfigs()
        {
            ValidImageExtensions = _settings.Image.ValidImageExtensions;
            _targetSizeInBytes = _settings.Image.TargetSizeBytes;
            _quality = _settings.Image.ImageQuality;
        }
    }
}
