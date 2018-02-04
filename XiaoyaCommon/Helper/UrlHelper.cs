using System;

namespace XiaoyaCommon.Helper
{
    public static class UrlHelper
    {
        public static string UrlToFileName(string url)
        {
            return HashHelper.GetMd5Hash(url) 
                + new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds().ToString();
        }
    }
}
