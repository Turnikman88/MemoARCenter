using Microsoft.AspNetCore.Components;

namespace MemoARCenter.Components.Pages
{
    public partial class DownloadPage : ComponentBase
    {
        [Parameter]
        public string Base64Parameter { get; set; }

        [Parameter]
        public string AlbumName { get; set; }

        private string MemoarUrl => $"memoar://getr?base64url={Base64Parameter}&folderName={AlbumName}";
    }
}
