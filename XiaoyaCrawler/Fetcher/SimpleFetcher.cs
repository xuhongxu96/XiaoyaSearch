using OpenQA.Selenium.PhantomJS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using XiaoyaStore.Data.Model;
using XiaoyaStore.Helper;
using XiaoyaStore.Store;
using XiaoyaCrawler.Config;
using XiaoyaLogger;
using XiaoyaFileParser;

namespace XiaoyaCrawler.Fetcher
{
    public class SimpleFetcher : IFetcher
    {
        protected CrawlerConfig mConfig;
        protected RuntimeLogger mLogger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="directory">Directory to save fetched web files</param>
        public SimpleFetcher(CrawlerConfig config)
        {
            mLogger = new RuntimeLogger(Path.Combine(config.LogDirectory, "Crawler.Log"));
            if (!Directory.Exists(config.FetchDirectory))
            {
                Directory.CreateDirectory(config.FetchDirectory);
            }

            mConfig = config;
        }

        /// <summary>
        /// Fetch the web content in the specific url
        /// </summary>
        /// <param name="url">Url in which to fetch the content</param>
        /// <returns>Local url to downloaded content</returns>
        public async Task<UrlFile> FetchAsync(string url)
        {
            if (!url.Contains(":"))
            {
                return null;
            }

            var protocol = url.Substring(0, url.IndexOf(":"));
            if (protocol != "http" && protocol != "https")
            {
                return null;
            }

            // Target path to save downloaded web file
            var path = Path.Combine(mConfig.FetchDirectory, UrlHelper.UrlToFileName(url));
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 10);
                var response = await client.GetAsync(url);
                var statusCode = response.StatusCode;
                var type = response.Content.Headers.ContentType;

                if (statusCode != HttpStatusCode.Accepted
                    && statusCode != HttpStatusCode.OK)
                {
                    mLogger.Log(nameof(SimpleFetcher), "Status: " + statusCode + " " + url);
                    return null;
                }

                if (type == null)
                {
                    mLogger.Log(nameof(SimpleFetcher), "Unknown MIME type: " + url);
                    return null;
                }

                if (mConfig.UsePhantomJS && type.MediaType == "text/html")
                {
                    // If config is set to use PhantomJS and web content type is HTML, 
                    // use PhantomJS to fetch real web page content
                    File.WriteAllText(path, 
                        FetchPageContentByPhantomJS(url, mConfig.PhantomJSDriverPath));
                }
                else
                {
                    // Otherwise, directly save it if supported by parser
                    if (UniversalFileParser.IsSupported(type.MediaType))
                    {
                        using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                        stream = File.Create(path))
                        {
                            await contentStream.CopyToAsync(stream);
                        }
                    }
                    else
                    {
                        return null;
                    }
                }

                return new UrlFile
                {
                    Url = url,
                    FilePath = path,
                    Charset = type.CharSet,
                    MimeType = type.MediaType,
                    FileHash = HashHelper.GetFileMd5(path),
                };
            }
        }

        private static string FetchPageContentByPhantomJS(string url, string phantomJsDriverPath)
        {
            var driver = new PhantomJSDriver(phantomJsDriverPath);
            try
            {
                driver.Navigate().GoToUrl(url);
                return driver.PageSource;
            }
            finally
            {
                driver.Close();
                driver.Quit();
            }
        }
    }
}
