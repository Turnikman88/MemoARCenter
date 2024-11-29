using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoARCenter.Services.Models
{
    public class FilePreviewModel
    {
        public string Name { get; set; } = string.Empty;
        public string DataUrl { get; set; } = string.Empty;
        public byte[] Content { get; set; } = Array.Empty<byte>(); 
        public string ContentType { get; set; } = string.Empty; 

    }
}
