using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using XiaoyaNLP.Helper;

namespace XiaoyaNLP.Encoding
{
    public static class EncodingDetector
    {
        public static string GetEncoding(string filePath)
        {
            var detector = new UniversalDetector(null);
            using (var stream = File.OpenRead(filePath))
            {
                byte[] DetectBuff = new byte[4096];
                while (stream.Read(DetectBuff, 0, DetectBuff.Length) > 0 && !detector.IsDone())
                {
                    detector.HandleData(DetectBuff, 0, DetectBuff.Length);
                }
                detector.DataEnd();
            }
            if (detector.GetDetectedCharset() == null)
            {
                return "UTF-8";
            }
            return detector.GetDetectedCharset();
        }


    }
}
