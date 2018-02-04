using AngleSharp.Parser.Html;
using OpenQA.Selenium.PhantomJS;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XiaoyaCommon.Helper
{
    public static class HtmlHelper
    {
        public static async Task<IList<string>> GetUrlsAsync(string html, string url)
        {
            var result = new List<string>();

            var parser = new HtmlParser();
            var document = await parser.ParseAsync(html);
            var links = document.GetElementsByTagName("a");
            foreach (var link in links)
            {
                var href = link.GetAttribute("href");
                var absoluteUrl = new Uri(new Uri(url), href);
                result.Add(absoluteUrl.ToString());
            }

            return result;
        }
    }
}
