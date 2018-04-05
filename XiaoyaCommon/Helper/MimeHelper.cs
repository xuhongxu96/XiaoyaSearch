using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO;
using HeyRed.Mime;

namespace XiaoyaCommon.Helper
{
    public class MimeHelper
    {
        public static string GetContentType(string filePath)
        {
            var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
            return MimeGuesser.GuessMimeType(absolutePath);
        }
    }

}