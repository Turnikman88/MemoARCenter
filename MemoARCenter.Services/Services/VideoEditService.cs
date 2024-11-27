using MemoARCenter.Services.Contracts;
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

        public async Task<VideoInfoDTO> ReduceVideoSizeAndBitrateAsync(string inputPath, int targetBitrateKbps, int targetWidth = 1280, int targetHeight = 720)
        {

            string outputPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() 
                + Path.GetExtension(inputPath));

            string argumentsInput = $"\"{inputPath}\" ";
            string argumentsVF = $"-vf \"scale=w=if(gte(iw/ih\\,{targetWidth / targetHeight})\\,{targetWidth}\\,-1):h=if(gte(iw/ih\\,{targetWidth / targetHeight})\\,-1\\,{targetHeight}),pad={targetWidth}:{targetHeight}:({targetWidth}-iw)/2:({targetHeight}-ih)/2:color=black\" ";
            string argumentsQuality = $"-b:v {targetBitrateKbps}k -b:a 128k -bufsize 2000k -c:v libx264 ";
            string argumentsOutput = $"\"{outputPath}\"";


            var arguments = string.Concat(argumentsInput, argumentsVF, argumentsQuality, argumentsOutput);
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/K \"C:\\ffmpeg\\bin\\ffmpeg.exe -i {arguments}", 
                    UseShellExecute = true, 
                    CreateNoWindow = false 
                };

                process.Start();

                process.WaitForExit();

            }

            byte[] videoBytes = await File.ReadAllBytesAsync(outputPath);

            File.Delete(outputPath);

            return new VideoInfoDTO (videoBytes); 
        }

        public bool IsVideoFile(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return ValidVideoExtensions.Contains(extension);
        }


       

    }
}
