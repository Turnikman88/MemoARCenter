using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoARCenter.Services.Helpers
{
    public static class Helper
    {
        public static string GetValidExtension(string extension, List<string> extsToCheck)
        {
            return extsToCheck.Contains(extension) ? extension : null;
        }
    }
}
