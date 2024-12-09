using MemoARCenter.Client.Pages;
using MemoARCenter.Helpers;
using MemoARCenter.Helpers.Models.System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;

namespace MemoARCenter.Components.Pages
{
    public partial class ZipUpload : ComponentBase
    {
        [Inject]
        private ILogger<ZipUpload> _log { get; set; }

        [Inject]
        private IOptions<AppSettings> _settings { get; set; }

        private string AlbumName { get; set; } = string.Empty;
        private LoadingOverlay? _loadingOverlay;
        private SmallLoadingSpinner? _loadingSpinner;

        private IBrowserFile? _selectedFile;
        private string _statusMessage = "No file selected.";
        private bool _isFileSelected = false;
        private string _qrCodeImageData = string.Empty;
        private string _qrCodeURL = string.Empty;
        private string _host = string.Empty;
        private int _maxZipFileSize;
        private List<string> _zipExtensions = new List<string>();

        private void HandleFileChange(InputFileChangeEventArgs e)
        {
            _qrCodeImageData = string.Empty;

            _log.LogDebug("Zip file was changed");

            _selectedFile = e.File;

            if (_selectedFile != null)
            {
                var fileExtension = Path.GetExtension(_selectedFile.Name).ToLowerInvariant();

                if (_zipExtensions.Any(x => x == fileExtension))
                {
                    _statusMessage = $"Selected file: {_selectedFile.Name}";
                    _isFileSelected = true;
                }
                else
                {
                    _statusMessage = $"Please select {string.Join(", ", _zipExtensions)} file.";
                    _isFileSelected = false;
                }
            }
        }

        private async Task UploadFile()
        {
            _qrCodeImageData = string.Empty;

            if (_selectedFile == null)
            {
                _statusMessage = "No file selected to upload.";
                return;
            }

            try
            {
                _loadingSpinner.Show();
                _log.LogDebug("Zip file upload started");

                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromMinutes(_settings.Value.HttpRequestTimeoutMinutes);

                var content = new MultipartFormDataContent();

                var fileStream = _selectedFile.OpenReadStream(_maxZipFileSize);
                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(_selectedFile.ContentType);
                content.Add(streamContent, "file", _selectedFile.Name);

                var url = $"{_host}/api/fileupload/upload?albumName={Helper.EncodeToBase64(AlbumName)}";

                ClearSelection();
                var response = await httpClient.PostAsync(url, content);

                var responseContent = await response?.Content?.ReadAsStringAsync() ?? string.Empty;

                var responseObject = System.Text.Json.JsonDocument.Parse(responseContent);

                _loadingSpinner.Hide();

                if (response.IsSuccessStatusCode)
                {

                    if (responseObject.RootElement.TryGetProperty("qrCode", out var qrCodeElement))
                    {
                        _qrCodeImageData = qrCodeElement.GetString();
                        _statusMessage = "File uploaded successfully!";
                    }
                    else
                    {
                        _statusMessage = "File uploaded, but QR code not received.";
                    }

                    if (responseObject.RootElement.TryGetProperty("qrCodeURL", out var qrCodeURLElement))
                    {
                        _qrCodeURL = qrCodeURLElement.GetString();
                        _statusMessage = "File uploaded successfully!";
                    }
                    else
                    {
                        _statusMessage = "File uploaded, but QR code URL not received.";
                    }
                }
                else
                {
                    if (responseObject.RootElement.TryGetProperty("message", out var message))
                    {
                        _statusMessage = message.GetString();
                    }
                    else
                    {
                        _statusMessage = $"Failed to upload file. Status code: {response.StatusCode}";
                    }
                }
            }
            catch (Exception ex)
            {
                _loadingSpinner.Hide();

                _statusMessage = $"Error uploading file: {ex.Message}";
            }
        }

        private void ClearSelection()
        {
            _isFileSelected = false;
            AlbumName = string.Empty;
            StateHasChanged();
        }

        private void SetConfigs()
        {
            _host = _settings.Value.Host;
            _maxZipFileSize = _settings.Value.Archive.MaxFileSize;
            _zipExtensions = _settings.Value.Archive.ValidZipExtensions;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                SetConfigs();
                _loadingOverlay?.Hide();
                StateHasChanged();
            }
        }
    }
}
