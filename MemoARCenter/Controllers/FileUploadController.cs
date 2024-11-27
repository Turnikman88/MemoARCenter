using MemoARCenter.Services.Contracts;
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


    public FileUploadController(IDBCreator dbCreatorService, IWebHostEnvironment env, IQRCode qr)
    {
        _dc = dbCreatorService;
        _env = env;
        _qr = qr;
    }

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

        var qrCodeURL = $"{Request.Scheme}://{Request.Host}/download-page/{base64Parameter}";
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

    [HttpGet("test")]
    public IActionResult Test()
    {
        _dc.Test();

        return Ok();
    }

}
