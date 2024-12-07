using MemoARCenter.Helpers;
using MemoARCenter.Helpers.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace MemoARCenter.Client.Pages
{
    public partial class FileUpload : ComponentBase
    {
        private string AlbumName { get; set; } = string.Empty;

        private LoadingOverlay? _loadingOverlay;
        private SmallLoadingSpinner? _loadingSpinner;
        private List<FilePreviewModel> _uploadedImages = new();
        private string? _uploadStatus;
        private string _qrCodeImageData = string.Empty;
        private string _qrCodeURL = string.Empty;
        private bool _isIphone;
        private bool _IsElementEnabled => (!string.IsNullOrEmpty(AlbumName) && _uploadedImages.Any(x => !string.IsNullOrEmpty(x.AssociatedVideoUrl)));
        private bool _isImagesLoading;
        private string _host = string.Empty;
        private int _maxAllowedVideoSize;
        private int _maxAllowedImageSize;


        [Inject]
        public IConfiguration Configuration { get; set; }

        public FileUpload()
        {
        }
        private async Task HandleImageSelected(InputFileChangeEventArgs e)
        {
            _isImagesLoading = true;
            _uploadedImages.Clear();

            var semaphore = new SemaphoreSlim(_isIphone ? 1 : 5); // Limit to 5 concurrent tasks or 1 for apple

            var tasks = e.GetMultipleFiles(25)
                .Where(file => file != null && file.Size > 0 && file.ContentType.StartsWith("image"))
                .Select(async file =>
                {
                    await semaphore.WaitAsync();

                    try
                    {
                        using var stream = file.OpenReadStream(maxAllowedSize: _maxAllowedImageSize);
                        using var memoryStream = new MemoryStream();
                        await stream.CopyToAsync(memoryStream);
                        var fileData = memoryStream.ToArray();

                        // Pass the raw file data to JavaScript for generating the object URL
                        var objectUrl = await JSRuntime.InvokeAsync<string>("siteJs.createObjectUrl", new
                        {
                            arrayBuffer = fileData,
                            type = file.ContentType
                        });

                        return new FilePreviewModel
                        {
                            Name = file.Name,
                            DataUrl = objectUrl,
                            IsVideoLoaded = true
                        };
                    }
                    finally
                    {
                        semaphore.Release();
                        _isImagesLoading = false;
                    }
                });

            var previews = await Task.WhenAll(tasks);

            _uploadedImages.AddRange(previews);
        }

        private async Task HandleVideoSelected(InputFileChangeEventArgs e, FilePreviewModel image)
        {
            image.IsVideoLoaded = false;

            var videoFile = e.File;

            if (videoFile == null || videoFile.Size == 0 || !videoFile.ContentType.StartsWith("video"))
            {
                return;
            }

            using var stream = videoFile.OpenReadStream(maxAllowedSize: _maxAllowedVideoSize);
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var fileData = memoryStream.ToArray();

            var videoUrl = await JSRuntime.InvokeAsync<string>("siteJs.createObjectUrl", new
            {
                arrayBuffer = fileData,
                type = videoFile.ContentType
            });

            image.VideoName = videoFile.Name;
            image.AssociatedVideoUrl = videoUrl;
            image.IsVideoLoaded = false;
        }

        private async Task UploadImagesAndVideos()
        {
            if (_uploadedImages.Count == 0)
            {
                _uploadStatus = "No images selected.";
                return;
            }

            try
            {
                using var zipStream = new MemoryStream();
                using (var archive = new System.IO.Compression.ZipArchive(zipStream, System.IO.Compression.ZipArchiveMode.Create, true))
                {
                    int fileIndex = 1;

                    foreach (var image in _uploadedImages)
                    {
                        if (string.IsNullOrEmpty(image.AssociatedVideoUrl))
                        {
                            // Skip images without associated videos
                            continue;
                        }

                        // Add the image to the ZIP archive
                        var imageData = await JSRuntime.InvokeAsync<byte[]>("siteJs.getBlobData", image.DataUrl);
                        var imageExtension = Path.GetExtension(image.Name) ?? ".jpeg";
                        var imageEntry = archive.CreateEntry($"{fileIndex}{imageExtension}", System.IO.Compression.CompressionLevel.Fastest);

                        using (var entryStream = imageEntry.Open())
                        {
                            await entryStream.WriteAsync(imageData, 0, imageData.Length);
                        }

                        // Add the video to the ZIP archive
                        var videoData = await JSRuntime.InvokeAsync<byte[]>("siteJs.getBlobData", image.AssociatedVideoUrl);
                        var videoExtension = Path.GetExtension(image.VideoName) ?? ".mp4";
                        var videoEntry = archive.CreateEntry($"{fileIndex}{videoExtension}", System.IO.Compression.CompressionLevel.Fastest);

                        using (var entryStream = videoEntry.Open())
                        {
                            await entryStream.WriteAsync(videoData, 0, videoData.Length);
                        }

                        fileIndex++;
                    }
                }

                // Reset the stream position before sending it
                zipStream.Position = 0;

                HttpResponseMessage response;
                try
                {
                    using var httpClient = new HttpClient();
                    httpClient.Timeout = TimeSpan.FromMinutes(int.Parse(Configuration["HttpRequestTimeoutMinutes"]));

                    using var content = new MultipartFormDataContent();

                    // Add the ZIP file to the form
                    var zipContent = new StreamContent(zipStream);
                    zipContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/zip");
                    content.Add(zipContent, "file", "uploaded_files.zip");


                    // Send the POST request
                    var url = $"{_host}/api/fileupload/upload?albumName={Helper.EncodeToBase64(AlbumName)}";


                    AlbumName = string.Empty;
                    _uploadedImages.Clear();
                    StateHasChanged();
                    _loadingSpinner.Show();
                    response = await httpClient.PostAsync(url, content);

                }
                catch (Exception ex)
                {
                    _loadingSpinner.Hide();
                    _uploadStatus = "Network error while uploading.";
                    return;
                }

                _loadingSpinner.Hide();

                var responseContent = await response?.Content?.ReadAsStringAsync() ?? string.Empty;

                var responseObject = System.Text.Json.JsonDocument.Parse(responseContent);

                if (response.IsSuccessStatusCode)
                {
                    if (responseObject.RootElement.TryGetProperty("qrCode", out var qrCodeElement))
                    {
                        _qrCodeImageData = qrCodeElement.GetString();
                        _uploadStatus = "File uploaded successfully!";
                    }
                    else
                    {
                        _uploadStatus = "File uploaded, but QR code not received.";
                    }

                    if (responseObject.RootElement.TryGetProperty("qrCodeURL", out var qrCodeURLElement))
                    {
                        _qrCodeURL = qrCodeURLElement.GetString();
                        _uploadStatus = "File uploaded successfully!";
                    }
                    else
                    {
                        _uploadStatus = "File uploaded, but QR code URL not received.";
                    }
                }
                else
                {
                    if (responseObject.RootElement.TryGetProperty("message", out var message))
                    {
                        _uploadStatus = message.GetString();
                    }
                    else
                    {
                        _uploadStatus = $"Upload failed. Status Code: {response.StatusCode}";
                    }
                }
            }
            catch (Exception ex)
            {
                _uploadStatus = $"Error: {ex.Message}";
            }
        }

        private void MarkVideoAsLoaded(FilePreviewModel image)
        {
            image.IsVideoLoaded = true;
        }

        private async Task TriggerVideoInputClick(string imageName)
        {
            var inputId = $"video-input-{imageName}";
            await JSRuntime.InvokeVoidAsync("triggerInputClick", inputId);
        }

        private void SetConfig()
        {
            _host = Configuration["Host"];
            _maxAllowedVideoSize = int.Parse(Configuration["MaxAllowedVideoSize"]);
            _maxAllowedImageSize = int.Parse(Configuration["MaxAllowedImageSize"]);
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                SetConfig();

                _isIphone = await JSRuntime.InvokeAsync<bool>("siteJs.isIphone");
                _loadingOverlay?.Hide();
                StateHasChanged();
                await JSRuntime.InvokeVoidAsync("resetFileInput", "file-input");
            }
        }
    }
}
