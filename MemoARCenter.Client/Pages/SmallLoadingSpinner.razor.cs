using Microsoft.AspNetCore.Components;

namespace MemoARCenter.Client.Pages
{
    public partial class SmallLoadingSpinner : ComponentBase
    {
        [Parameter]
        public bool IsVisible { get; set; }

        [Parameter]
        public string? LoadingMessage { get; set; }

        public void Show(string? message = null)
        {
            LoadingMessage = message;
            IsVisible = true;
            StateHasChanged();
        }

        public void Hide()
        {
            IsVisible = false;
            StateHasChanged();
        }
    }
}
