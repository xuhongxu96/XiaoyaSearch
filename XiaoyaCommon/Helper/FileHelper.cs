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
            try
            {
                return File.ReadAllBytes(path1).SequenceEqual(File.ReadAllBytes(path2));
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
