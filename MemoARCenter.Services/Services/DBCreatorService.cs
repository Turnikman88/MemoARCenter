using MemoARCenter.Services.Contracts;
using MemoARCenter.Services.Helpers;
using MemoARCenter.Services.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace MemoARCenter.Services.Services
{
    public class DBCreatorService : IDBCreator
    {
        private readonly IImageEdit _is;
        private readonly IVideoEdit _vs;

        public DBCreatorService(IImageEdit iss, IVideoEdit vs)
        {
            _is = iss;
            _vs = vs;
        }

        public async Task Test()
        {

            string inputFile = "C:\\Users\\Jora\\Documents\\SpomenAR\\Server\\LatestTests\\horo.mp4";
            string outputFile = "C:\\Users\\Jora\\Documents\\SpomenAR\\Server\\LatestTests\\output_code.mp4";

            // Target frame dimensions (example: 287x500)
            int frameWidth = 500;
            int frameHeight = 287;
            double frameAspectRatio = (double)frameWidth / frameHeight;

            // Get video info
            var mediaInfo = await FFmpeg.GetMediaInfo(inputFile);
            var videoStream = mediaInfo.VideoStreams.First();

            // Original video dimensions
            int originalWidth = videoStream.Width;
            int originalHeight = videoStream.Height;
            double videoAspectRatio = (double)originalWidth / originalHeight;

            // Initialize scaling and padding variables
            int scaledWidth = originalWidth;
            int scaledHeight = originalHeight;
            int padWidth = frameWidth;
            int padHeight = frameHeight;
            int padX = 0;
            int padY = 0;

            var conversion = FFmpeg.Conversions.New().SetOutput(outputFile).SetOverwriteOutput(true);

            if (videoStream.Codec == "hevc")
            {
                conversion
                    .SetPreset(ConversionPreset.Slow)
                    .SetPixelFormat(PixelFormat.yuv420p)
//                    .SetOutputFormat(Format.h264)
                    .SetVideoBitrate(2000000)
                    .SetAudioBitrate(128000);
            }

            // Determine the case and calculate dimensions
            if (videoAspectRatio > frameAspectRatio)
            {
                // Case: Video is wider than the frame
                padWidth = originalWidth;
                padHeight = (int)(originalWidth / frameAspectRatio);
                padY = (padHeight - originalHeight) / 2;
            }
            else if (videoAspectRatio < frameAspectRatio)
            {
                // Case: Video is taller than the frame
                padHeight = originalHeight;
                padWidth = (int)(originalHeight * frameAspectRatio);
                padX = (padWidth - originalWidth) / 2;
            }


            // FFmpeg scaling and padding command
            conversion 
                .AddStream(videoStream)
                //.AddStream(mediaInfo.AudioStreams.First())

                .AddParameter($"-vf pad={padWidth}:{padHeight}:{padX}:{padY}:black");

            // Start the conversion
            try
            {
                await conversion.Start();

            }
            catch (Exception e)
            {

                throw;
            }
        }

        public async Task ProcessZipAndResizeImages(string sourceZipPath, string targetZipPath)
        {
            string tempExtractFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            try
            {
                Directory.CreateDirectory(tempExtractFolder);
                ZipFile.ExtractToDirectory(sourceZipPath, tempExtractFolder);

                string entryName = string.Empty;

                using (var targetZip = ZipFile.Open(targetZipPath, ZipArchiveMode.Create))
                {
                    var imageMetadataList = new List<TargetFile>();

                    var dict = CreateCustomFileInfoModel(Directory.GetFiles(tempExtractFolder));

                    foreach (var fileInfo in dict)
                    {
                        ImageInfoDTO imageInfo = CreateImageEntry(targetZip, imageMetadataList, fileInfo);
                        await CreateVideoEntry(targetZip, fileInfo, imageInfo);
                    }

                    var jsonContent = JsonSerializer.Serialize(imageMetadataList, new JsonSerializerOptions { WriteIndented = true });

                    var jsonEntry = targetZip.CreateEntry("targets.json");
                    using (var jsonEntryStream = jsonEntry.Open())
                    using (var writer = new StreamWriter(jsonEntryStream))
                    {
                        writer.Write(jsonContent);
                    }
                }
            }
            catch (Exception e)
            {

            }
            finally
            {
                if (Directory.Exists(tempExtractFolder))
                {
                    Directory.Delete(tempExtractFolder, true);
                }
                File.Delete(sourceZipPath);
            }

            void CreateEntry(string fileName, string ext, ZipArchive targetZip, byte[] byteArr)
            {
                var entryName = fileName + ext;

                var entry = targetZip.CreateEntry(entryName);

                using (var entryStream = entry.Open())
                {
                    entryStream.Write(byteArr, 0, byteArr.Length);
                }
            }

            ImageInfoDTO CreateImageEntry(ZipArchive targetZip, List<TargetFile> imageMetadataList, KeyValuePair<string, CustomFileInfoDTO> fileInfo)
            {
                var imageInfo = _is.ResizeImage(fileInfo.Value.ImageFileCompletePath, 500, 500, 100);

                imageMetadataList.Add(new TargetFile
                {
                    Name = fileInfo.Key,
                    Extension = ".jpg"
                });

                CreateEntry(fileInfo.Key, fileInfo.Value.ImageExtension, targetZip, imageInfo.ImageBytes);
                return imageInfo;
            }

            async Task CreateVideoEntry(ZipArchive targetZip, KeyValuePair<string, CustomFileInfoDTO> fileInfo, ImageInfoDTO imageInfo)
            {
                var videoInfo = await _vs.ReduceVideoSizeAndBitrateAsync(fileInfo.Value.VideoFileCompletePath, 1000, imageInfo.Width, imageInfo.Height);

                CreateEntry(fileInfo.Key, fileInfo.Value.VideoExtension, targetZip, videoInfo.VideoBytes);
            }
        }

        //ToDo: may need optimization and error handling
        private Dictionary<string, CustomFileInfoDTO> CreateCustomFileInfoModel(string[] paths)
        {
            var dirPath = Path.GetDirectoryName(paths.First());

            var dict = new Dictionary<string, CustomFileInfoDTO>();

            var fileNameNoExt = string.Empty;

            var customFileInfoDTO = new CustomFileInfoDTO();

            foreach (var path in paths)
            {
                fileNameNoExt = Path.GetFileNameWithoutExtension(path);

                if (dict.TryGetValue(fileNameNoExt, out customFileInfoDTO))
                {
                    SetExtensions(path);
                }
                else
                {
                    customFileInfoDTO = new CustomFileInfoDTO(dirPath, fileNameNoExt);
                    SetExtensions(path);    
                    dict.Add(fileNameNoExt, customFileInfoDTO);
                }
                    
            }

            return dict;

            void SetExtensions(string path)
            {
                customFileInfoDTO.ImageExtension = Helper.GetValidExtension(Path.GetExtension(path), ImageEditService.ValidImageExtensions);
                customFileInfoDTO.VideoExtension = Helper.GetValidExtension(Path.GetExtension(path), VideoEditService.ValidVideoExtensions);
            }
        }
    }
}
