using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace MemoARCenter.Client.Pages
{
    public partial class QRCodeComponent : ComponentBase
    {
        [Parameter]
        public string QRCodeURL { get; set; }

        [Parameter]
        public string QRCodeImageData { get; set; }

        private bool _isJsInitialized = false;


        public async Task SaveCode()
        {
            if (_isJsInitialized)
            {
                await JSRuntime.InvokeVoidAsync("siteJs.saveQrCodeImage", QRCodeImageData, "QRCode.png");
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _isJsInitialized = true;
            }
        }

    }
}
