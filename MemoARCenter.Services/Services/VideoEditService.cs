using MemoARCenter.Helpers;
using MemoARCenter.Helpers.Models.DTOs;
using MemoARCenter.Helpers.Models.System;
using MemoARCenter.Services.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xabe.FFmpeg;

namespace MemoARCenter.Services.Services
{
    public class VideoEditService : IVideoEdit
    {
        private readonly AppSettings _settings;
        private readonly ILogger<VideoEditService> _log;
        public static List<string> ValidVideoExtensions = new List<string>();

        public VideoEditService(IOptions<AppSettings> settings, ILogger<VideoEditService> log)
        {
            _settings = settings.Value;
            _log = log;

            SetConfigs();
        }

        public async Task<VideoInfoDTO> ReduceVideoSizeAndBitrateAsync(string inputPath, int targetBitrateKbps, int targetWidth = 1280, int targetHeight = 720)
        {
            _log.LogDebug("Inside ReduceVideoSizeAndBitrateAsync");

            var outputPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()
                 + Path.GetExtension(inputPath));

            var conversion = FFmpeg.Conversions.New()
                .SetOutput(outputPath)
                .SetOverwriteOutput(true);

            _log.LogDebug("Getting media info");

            var mediaInfo = await FFmpeg.GetMediaInfo(inputPath);

            var videoStream = mediaInfo.VideoStreams.First();

            _log.LogDebug("Adding video stream");

            conversion.AddStream(videoStream);

            CalculatePadding(targetWidth, targetHeight, conversion, videoStream);

            conversion.ChangeBitRateAndFormat(videoStream.Bitrate) //ToDo: add logic here
                .AddAudioStream(mediaInfo);

            await conversion.Start();

            var videoBytes = await File.ReadAllBytesAsync(outputPath);

            File.Delete(outputPath);

            return new VideoInfoDTO(videoBytes);

            void CalculatePadding(int targetWidth, int targetHeight, IConversion conversion, IVideoStream videoStream)
            {
                _log.LogDebug("Calculationg padding");

                double frameAspectRatio = (double)targetWidth / targetHeight;

                int originalWidth = videoStream.Width;
                int originalHeight = videoStream.Height;
                double videoAspectRatio = (double)originalWidth / originalHeight;

                int scaledWidth = originalWidth;
                int scaledHeight = originalHeight;
                int padWidth = targetWidth;
                int padHeight = targetHeight;
                int padX = 0;
                int padY = 0;

                if (Math.Abs(frameAspectRatio - videoAspectRatio) < 0.1)
                {
                    return;
                }
                else if (videoAspectRatio > frameAspectRatio)
                {
                    padWidth = originalWidth;
                    padHeight = (int)(originalWidth / frameAspectRatio);
                    padY = (padHeight - originalHeight) / 2;
                }
                else if (videoAspectRatio < frameAspectRatio)
                {
                    padHeight = originalHeight;
                    padWidth = (int)(originalHeight * frameAspectRatio);
                    padX = (padWidth - originalWidth) / 2;
                }

                conversion.AddParameter($"-vf pad={padWidth}:{padHeight}:{padX}:{padY}:black");
            }
        }

        public bool IsVideoFile(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return ValidVideoExtensions.Contains(extension);
        }

        private void SetConfigs()
        {
            ValidVideoExtensions = _settings.Video.ValidVideoExtensions;
        }
    }
}
