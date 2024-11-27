using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoARCenter.Services.Models
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
