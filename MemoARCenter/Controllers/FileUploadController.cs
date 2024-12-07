using MemoARCenter.Services.Contracts;
using MemoARCenter.Services.Models;
using MemoARCenter.Services.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog.Core;
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
    private readonly ILogger<FileUploadController> _log;

    public FileUploadController(IDBCreator dbCreatorService, IWebHostEnvironment env, IQRCode qr, IConfiguration config, ILogger<FileUploadController> log)
    {
        _dc = dbCreatorService;
        _env = env;
        _qr = qr;
        _config = config;
        _log = log;
    }

    [HttpGet]
    public IActionResult Test()
    {
        Serilog.Log.Logger.Error("teeeeesr");
        _log.LogInformation("are beeeeeeeeeeeeeeee");
        return Ok();
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile([FromQuery] string albumName, IFormFile file)
    {
        _log.LogInformation("Inside upload method");

        // Validate album name
        if (string.IsNullOrEmpty(albumName))
        {
            return BadRequest("Album name is required.");
        }

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

        var base64Parameter = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileUrl));
        var host = _config["AppSettings:Host"];

        var qrCodeURL = $"{host}/download-page/{base64Parameter}/{albumName}";
        var image = _qr.GenerateQrCode(qrCodeURL);
      
        return Ok(new {QRCode = image, QRCodeURL = qrCodeURL});
    }

    [HttpGet("download/{fileName}")]
    public IActionResult DownloadFile(string fileName)
    {
        _log.LogInformation("Inside download method");

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
}
