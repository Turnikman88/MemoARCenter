using Microsoft.AspNetCore.Components;

namespace MemoARCenter.Client.Pages
{
    public partial class Home : ComponentBase
    {
        [Inject]
        private NavigationManager NavigationManager { get; set; }

        private void NavigateToUpload()
        {
            NavigationManager?.NavigateTo("/file-upload");
        }
    }
}
