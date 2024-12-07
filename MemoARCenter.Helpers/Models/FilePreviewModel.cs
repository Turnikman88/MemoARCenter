namespace MemoARCenter.Helpers.Models
{
    public class FilePreviewModel
    {
        public string Name { get; set; }
        public string VideoName { get; set; }
        public string DataUrl { get; set; }
        public string? AssociatedVideoUrl { get; set; }
        public bool IsVideoLoaded { get; set; }
    }
}
