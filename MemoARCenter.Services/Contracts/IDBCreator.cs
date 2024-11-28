using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoARCenter.Services.Contracts
{
    public interface IDBCreator
    {
        Task ProcessZipAndResizeImages(string sourceZipPath, string targetZipPath);
    }
}
