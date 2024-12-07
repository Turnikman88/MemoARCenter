using MemoARCenter.Services.Contracts;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using SkiaSharp.QrCode;
namespace MemoARCenter.Services.Services
{
    public class QRCodeService : IQRCode
    {
        private readonly ILogger<QRCodeService> _log;

        public QRCodeService(ILogger<QRCodeService> log)
        {
            _log = log;
        }

        public string GenerateQrCode(string url)
        {
            _log.LogDebug("inside GenerateQrCode");

            var generator = new QRCodeGenerator();

            var qrCodeData = generator.CreateQrCode(
                url,
                ECCLevel.M,
                false,
                false,
                QRCodeGenerator.EciMode.Default,
                -1,
                4
            );

            using (var surface = SKSurface.Create(new SKImageInfo(256, 256)))
            {
                var canvas = surface.Canvas;
                canvas.Clear(SKColors.White);

                var rect = new SKRect(0, 0, 256, 256);

                var renderer = new QRCodeRenderer();
                renderer.Render(
                    canvas,
                    rect,
                    qrCodeData,
                    SKColors.Black,
                    SKColors.White,
                    null
                );

                using (var ms = new MemoryStream())
                {
                    surface.Snapshot().Encode(SKEncodedImageFormat.Png, 100).SaveTo(ms);
                    var base64Image = Convert.ToBase64String(ms.ToArray());
                    return $"data:image/png;base64,{base64Image}";
                }
            }
        }
    }
}

