namespace MemoARCenter.Services.Contracts
{
    public interface IDBCreator
    {
        Task ProcessZipAndResizeImages(string sourceZipPath, string targetZipPath);
    }
}
