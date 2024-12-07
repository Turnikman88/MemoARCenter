namespace MemoARCenter.Helpers.Models.System
{
    public class ResponseModel
    {
        public ResponseModel()
        {

        }

        public ResponseModel(bool isOk, int statusCode, string message)
        {
            IsOk = isOk;
            StatusCode = statusCode;
            Message = message;
        }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public bool IsOk { get; set; }
    }
}
