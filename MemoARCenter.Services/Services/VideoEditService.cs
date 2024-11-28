using MemoARCenter.Services.Contracts;
using MemoARCenter.Services.Helpers;
using MemoARCenter.Services.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace MemoARCenter.Services.Services
{
    public class VideoEditService : IVideoEdit
    {
        public static readonly List<string> ValidVideoExtensions = new List<string>() { ".mp4", ".avi", ".mov", ".mkv", ".wmv", ".flv" };

        //public async Task<VideoInfoDTO> ReduceVideoSizeAndBitrateAsync(string inputPath, int targetBitrateKbps, int targetWidth = 1280, int targetHeight = 720)
        //{

        //    string outputPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() 
        //        + Path.GetExtension(inputPath));

        //    string argumentsInput = $"\"{inputPath}\" ";
        //    string argumentsVF = $"-vf \"scale=w=if(gte(iw/ih\\,{targetWidth / targetHeight})\\,{targetWidth}\\,-1):h=if(gte(iw/ih\\,{targetWidth / targetHeight})\\,-1\\,{targetHeight}),pad={targetWidth}:{targetHeight}:({targetWidth}-iw)/2:({targetHeight}-ih)/2:color=black\" ";
        //    string argumentsQuality = $"-b:v {targetBitrateKbps}k -b:a 128k -bufsize 2000k -c:v libx264 ";
        //    string argumentsOutput = $"\"{outputPath}\"";


        //    var arguments = string.Concat(argumentsInput, argumentsVF, argumentsQuality, argumentsOutput);
        //    using (var process = new Process())
        //    {
        //        process.StartInfo = new ProcessStartInfo
        //        {
        //            FileName = "cmd.exe",
        //            Arguments = $"/K \"C:\\ffmpeg\\bin\\ffmpeg.exe -i {arguments}", 
        //            UseShellExecute = true, 
        //            CreateNoWindow = false 
        //        };

        //        process.Start();

        //        process.WaitForExit();

        //    }

        //    byte[] videoBytes = await File.ReadAllBytesAsync(outputPath);

        //    File.Delete(outputPath);

        //    return new VideoInfoDTO (videoBytes); 
        //}

        public async Task<VideoInfoDTO> ReduceVideoSizeAndBitrateAsync(string inputPath, int targetBitrateKbps, int targetWidth = 1280, int targetHeight = 720)
        {
            string outputPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()
                 + Path.GetExtension(inputPath));

            var conversion = FFmpeg.Conversions.New()
                .SetOutput(outputPath)
                .SetOverwriteOutput(true);

            var mediaInfo = await FFmpeg.GetMediaInfo(inputPath);
            var videoStream = mediaInfo.VideoStreams.First();

            conversion.AddStream(videoStream);

            CalculatePadding(targetWidth, targetHeight, conversion, videoStream);


            conversion.ChangeBitRateAndFormat(videoStream.Bitrate) //ToDo: add logic here
                .AddAudioStream(mediaInfo);

            await conversion.Start();

            byte[] videoBytes = await File.ReadAllBytesAsync(outputPath);

            File.Delete(outputPath);

            return new VideoInfoDTO(videoBytes);

            static void CalculatePadding(int targetWidth, int targetHeight, IConversion conversion, IVideoStream videoStream)
            {
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


       

    }
}
