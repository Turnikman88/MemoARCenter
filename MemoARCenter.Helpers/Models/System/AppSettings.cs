namespace MemoARCenter.Helpers.Models.System
{
    public class AppSettings
    {
        public string Host { get; set; }
        public int HttpRequestTimeoutMinutes { get; set; }
        public int MaxBytesAllowedTraffic { get; set; }
        public ImageSettings Image { get; set; }
        public VideoSettings Video { get; set; }
        public ZipSettings Archive { get; set; }
    }

    public class ImageSettings
    {
        public int TargetSizeBytes { get; set; }
        public int ImageQuality { get; set; }
        public List<string> ValidImageExtensions { get; set; }
    }

    public class VideoSettings
    {
        public List<string> ValidVideoExtensions { get; set; }
    }

    public class ZipSettings
    {
        public List<string> ValidZipExtensions { get; set; }
        public int MaxFileSize { get; set; }
    }
}
