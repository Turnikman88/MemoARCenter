using MemoARCenter.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoARCenter.Services.Contracts
{
    public interface IImageEdit
    {
        ImageInfoDTO ResizeImage(string imagePath, int maxWidth, int maxHeight, int maxSizeKB);
        bool IsImageFile(string filePath);
    }
}
