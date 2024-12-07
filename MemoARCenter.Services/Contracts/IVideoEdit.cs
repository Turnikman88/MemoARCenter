using MemoARCenter.Helpers.Models.DTOs;

namespace MemoARCenter.Services.Contracts
{
    public interface IVideoEdit
    {
        bool IsVideoFile(string filePath);
        Task<VideoInfoDTO> ReduceVideoSizeAndBitrateAsync(string inputPath, int targetBitrateKbps, int targetWidth = 1280, int targetHeight = 720);
    }
}
