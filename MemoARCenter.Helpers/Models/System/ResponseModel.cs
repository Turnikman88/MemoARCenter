using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
