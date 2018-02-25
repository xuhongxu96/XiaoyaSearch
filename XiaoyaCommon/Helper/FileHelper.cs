using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XiaoyaStore.Helper
{
    public static class FileHelper
    {
        public static bool FilesAreEqual(string path1, string path2)
        {
            return File.ReadAllBytes(path1).SequenceEqual(File.ReadAllBytes(path2));
        }
    }
}
