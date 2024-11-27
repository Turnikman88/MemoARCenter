using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoARCenter.Services.Models
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
