using MemoARCenter.Helpers;
using MemoARCenter.Helpers.Models;
using MemoARCenter.Helpers.Models.DTOs;
using MemoARCenter.Helpers.Models.System;
using MemoARCenter.Services.Contracts;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Text.Json;

namespace MemoARCenter.Services.Services
{
    public class DBCreatorService : IDBCreator
    {
        private readonly IImageEdit _is;
        private readonly IVideoEdit _vs;
        private readonly ILogger<DBCreatorService> _log;
        public DBCreatorService(IImageEdit iss, IVideoEdit vs, ILogger<DBCreatorService> log)
        {
            _is = iss;
            _vs = vs;
            _log = log;
        }

        public async Task<ResponseModel> ProcessZipAndResizeImages(string sourceZipPath, string targetZipPath)
        {
            _log.LogDebug("Inside ProcessZipAndResizeImages");

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

                    if (dict == null)
                    {
                        return new ResponseModel(false, 422, "The zip contains invalid files. Use only images and videos");
                    }

                    foreach (var fileInfo in dict)
                    {
                        _log.LogDebug($"Creating target for {fileInfo.Key}");
                        if (string.IsNullOrEmpty(fileInfo.Value.ImageExtension) ||
                            string.IsNullOrEmpty(fileInfo.Value.VideoExtension))
                        {
                            continue;
                        }

                        ImageInfoDTO imageInfo = CreateImageEntry(targetZip, imageMetadataList, fileInfo);
                        await CreateVideoEntry(targetZip, fileInfo, imageInfo);
                    }

                    _log.LogDebug("Creating targets.json");

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
                return new ResponseModel(false, 422, e.Message);
            }
            finally
            {
                _log.LogDebug("Clear files");

                if (Directory.Exists(tempExtractFolder))
                {
                    Directory.Delete(tempExtractFolder, true);
                }
                File.Delete(sourceZipPath);
            }

            return new ResponseModel(true, 200, "OK");

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
                    Extension = ".jpeg"
                });

                CreateEntry(fileInfo.Key, ".jpeg", targetZip, imageInfo.ImageBytes);
                return imageInfo;
            }

            async Task CreateVideoEntry(ZipArchive targetZip, KeyValuePair<string, CustomFileInfoDTO> fileInfo, ImageInfoDTO imageInfo)
            {
                var videoInfo = await _vs.ReduceVideoSizeAndBitrateAsync(fileInfo.Value.VideoFileCompletePath, 1000, imageInfo.Width, imageInfo.Height);

                CreateEntry(fileInfo.Key, ".mp4", targetZip, videoInfo.VideoBytes);
            }
        }

        private Dictionary<string, CustomFileInfoDTO> CreateCustomFileInfoModel(string[] paths)
        {
            _log.LogDebug("Creating custom file info model");

            var dirPath = Path.GetDirectoryName(paths.First());

            var dict = new Dictionary<string, CustomFileInfoDTO>();

            var fileNameNoExt = string.Empty;

            var customFileInfoDTO = new CustomFileInfoDTO();
            var ext = string.Empty;

            foreach (var path in paths)
            {
                ext = Path.GetExtension(path);
                if (Helper.GetValidExtension(ext, ImageEditService.ValidImageExtensions) == null
                    && Helper.GetValidExtension(ext, VideoEditService.ValidVideoExtensions) == null)
                {
                    return default;
                }

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
