using MemoARCenter.Helpers.Models.System;
using MemoARCenter.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class FileUploadController : ControllerBase
{
    private readonly IDBCreator _dc;
    private readonly IWebHostEnvironment _env;
    private readonly IQRCode _qr;
    private readonly AppSettings _settings;
    private readonly ILogger<FileUploadController> _log;

    private string _host = string.Empty;

    public FileUploadController(IDBCreator dbCreatorService, IWebHostEnvironment env, IQRCode qr, IOptions<AppSettings> settings, ILogger<FileUploadController> log)
    {
        _dc = dbCreatorService;
        _env = env;
        _qr = qr;
        _settings = settings.Value;
        _log = log;
        SetConfig();
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile([FromQuery] string albumName, IFormFile file)
    {
        _log.LogInformation("Inside upload method");

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
        var result = await _dc.ProcessZipAndResizeImages(filePath, targetPath);


        var fileUrl = $"{Request.Scheme}://{Request.Host}/api/fileupload/download/{guidFileName}";

        var base64Parameter = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileUrl));

        var qrCodeURL = $"{_host}/download-page/{base64Parameter}/{albumName}";
        var image = _qr.GenerateQrCode(qrCodeURL);

        return new ObjectResult(new { QRCode = image, QRCodeURL = qrCodeURL, Message = result.Message })
        {
            StatusCode = result.StatusCode
        };
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

    private void SetConfig()
    {
        _host = _settings.Host;
    }

}
