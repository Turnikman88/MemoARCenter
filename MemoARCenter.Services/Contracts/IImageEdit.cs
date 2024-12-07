using MemoARCenter.Helpers.Models.DTOs;

namespace MemoARCenter.Services.Contracts
{
    public interface IImageEdit
    {
        ImageInfoDTO ResizeImage(string imagePath, int maxWidth, int maxHeight, int maxSizeKB);
        bool IsImageFile(string filePath);
    }
}
