using MemoARCenter.Helpers.Models.System;

namespace MemoARCenter.Services.Contracts
{
    public interface IDBCreator
    {
        Task<ResponseModel> ProcessZipAndResizeImages(string sourceZipPath, string targetZipPath);
    }
}
