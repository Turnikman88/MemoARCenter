using Microsoft.AspNetCore.Components;

namespace MemoARCenter.Client.Pages
{
    public partial class LoadingOverlay : ComponentBase
    {
        [Parameter]
        public bool IsVisible { get; set; }

        [Parameter]
        public string? LoadingMessage { get; set; }

        public void Show(string? message = null)
        {
            IsVisible = true;
            LoadingMessage = message;
            StateHasChanged();
        }

        public void Hide()
        {
            IsVisible = false;
            StateHasChanged();
        }
    }
}
