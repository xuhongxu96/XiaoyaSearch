using System;

namespace XiaoyaCommon.Helper
{
    public static class UrlHelper
    {
        public static string UrlToFileName(string url)
        {
            return HashHelper.GetStringMd5(url) 
                + new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds().ToString();
        }
    }
}
