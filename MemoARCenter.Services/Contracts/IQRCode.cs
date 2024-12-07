namespace MemoARCenter.Services.Contracts
{
    public interface IQRCode
    {
        string GenerateQrCode(string url);
    }
}
