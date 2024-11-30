using MemoARCenter.Services.Contracts;
using MemoARCenter.Services.Models;
using MemoARCenter.Services.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkiaSharp;
using System.IO;
using System.IO.Compression;
using System.Net.Mime;
using System.Reflection.Metadata;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class FileUploadController : ControllerBase
{
    private readonly IDBCreator _dc;
    private readonly IWebHostEnvironment _env;
    private readonly IQRCode _qr;
    private readonly IConfiguration _config;


    public FileUploadController(IDBCreator dbCreatorService, IWebHostEnvironment env, IQRCode qr, IConfiguration config)
    {
        _dc = dbCreatorService;
        _env = env;
        _qr = qr;
        _config = config;
    }
    /*
        [HttpPost("bulkupload")]
        public async Task<IActionResult> UploadFiles()
        {
            if (!Request.HasFormContentType)
            {
                return BadRequest("Unsupported content type.");
            }

            var form = Request.Form;
            var uploadedImages = new List<FilePreviewModel>();
            var uploadedVideos = new List<FilePreviewModel>();

            try
            {
                foreach (var formFile in form.Files)
                {
                    if (formFile.Length > 0)
                    {
                        using var memoryStream = new MemoryStream();
                        await formFile.CopyToAsync(memoryStream);

                        var fileData = new FilePreviewModel
                        {
                            Name = formFile.FileName,
                            ContentType = formFile.ContentType,
                            Content = memoryStream.ToArray()
                        };

                        // Classify as image or video based on MIME type
                        if (fileData.ContentType.StartsWith("image"))
                        {
                            uploadedImages.Add(fileData);
                        }
                        else if (fileData.ContentType.StartsWith("video"))
                        {
                            uploadedVideos.Add(fileData);
                        }
                    }
                }

                // Match images and videos (you may need a custom matching logic based on naming convention, etc.)
                var pairedFiles = new List<ImageVideoPair>();
                foreach (var image in uploadedImages)
                {
                    var matchingVideo = uploadedVideos.FirstOrDefault(video =>
                        Path.GetFileNameWithoutExtension(video.Name) == Path.GetFileNameWithoutExtension(image.Name));

                    if (matchingVideo != null)
                    {
                        pairedFiles.Add(new ImageVideoPair
                        {
                            Image = image,
                            Video = matchingVideo
                        });

                        // Remove paired video to avoid duplicate pairing
                        uploadedVideos.Remove(matchingVideo);
                    }
                }

                // Save paired files to storage or database
                foreach (var pair in pairedFiles)
                {
                    // Save image
                    await SaveFileAsync(pair.Image, "images");

                    // Save video
                    await SaveFileAsync(pair.Video, "videos");
                }

                return Ok(new { Message = "Files uploaded successfully!", PairsUploaded = pairedFiles.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }*/

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { Message = "File not provided or empty." });
        }

        var uploadPath = Path.Combine(_env.ContentRootPath, "UploadedFiles");
        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        var uniqueFileName = $"{Guid.NewGuid()}_0g_{file.FileName}";
        var filePath = Path.Combine(uploadPath, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var guidFileName = $"{Guid.NewGuid().ToString()}_DB.zip";

        var targetPath = Path.Combine(uploadPath, guidFileName);
        await _dc.ProcessZipAndResizeImages(filePath, targetPath);


        var fileUrl = $"{Request.Scheme}://{Request.Host}/api/fileupload/download/{guidFileName}";

        string base64Parameter = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileUrl));

        var host = _config["AppSettings:Host"];

        var qrCodeURL = $"{host}/download-page/{base64Parameter}";
        var image = _qr.GenerateQrCode(qrCodeURL);
      
        return Ok(new {QRCode = image, QRCodeURL = qrCodeURL});
    }

    [HttpGet("download/{fileName}")]
    public IActionResult DownloadFile(string fileName)
    {
        var filePath = Path.Combine(_env.ContentRootPath, "UploadedFiles", fileName);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound(new { Message = "File not found." });
        }

        var contentType = GetContentType(filePath);

        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        return File(fileBytes, contentType, "DB.zip");
    }

    private string GetContentType(string filePath)
    {
        var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(filePath, out var contentType))
        {
            contentType = "application/octet-stream"; 
        }
        return contentType;
    }

    private async Task SaveFileAsync(FilePreviewModel file, string folder)
    {
        var path = Path.Combine("wwwroot", folder, file.Name);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await System.IO.File.WriteAllBytesAsync(path, file.Content);
    }

    [HttpGet("test")]
    public IActionResult Test()
    {
        //_dc.Test();

        return Ok();
    }

}
