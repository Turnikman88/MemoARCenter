namespace MemoARCenter.Helpers.Models.DTOs
{
    public class VideoInfoDTO
    {
        public VideoInfoDTO(byte[]? videoBytes)
        {
            VideoBytes = videoBytes;

        }
        public byte[]? VideoBytes { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
