namespace MemoARCenter.Helpers.Models.DTOs
{
    public class ImageInfoDTO
    {
        public ImageInfoDTO(byte[]? imageBytes, int width, int height)
        {
            ImageBytes = imageBytes;
            Width = width;
            Height = height;
        }
        public byte[]? ImageBytes { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
