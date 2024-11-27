using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoARCenter.Services.Models
{
    internal class CustomFileInfoDTO
    {
        public CustomFileInfoDTO()
        {
            
        }
        public CustomFileInfoDTO(string dirPath, string fileNameNoExt, string imgExt = "", string vidExt = "")
        {
            DirectoryPath = dirPath;
            FileNameNoExtension = fileNameNoExt;
            ImageExtension = imgExt;
            VideoExtension = vidExt;
        }
        public string DirectoryPath { get; set; }

        private string _imageExtension;
        public string ImageExtension
        {
            get { return _imageExtension; }
            set 
            {
                if (value == null)
                {
                    return;
                }

                _imageExtension = value;
            }
        }

        private string _videoExtension;
        public string VideoExtension
        {
            get { return _videoExtension; }
            set
            {
                if (value == null)
                {
                    return;
                }

                _videoExtension = value;
            }
        }

        public string FileNameNoExtension { get; set; }

        public string ImageFileCompletePath => Path.Combine(DirectoryPath, FileNameNoExtension) + ImageExtension;
        public string VideoFileCompletePath => Path.Combine(DirectoryPath, FileNameNoExtension) + VideoExtension;

    }
}
