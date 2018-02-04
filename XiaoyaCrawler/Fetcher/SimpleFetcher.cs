using OpenQA.Selenium.PhantomJS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using XiaoyaCommon.Data.Crawler.Model;
using XiaoyaCommon.Helper;
using XiaoyaCommon.Store;
using XiaoyaCrawler.Config;
using XiaoyaLogger;

namespace XiaoyaCrawler.Fetcher
{
    public class SimpleFetcher : IFetcher
    {
        /// <summary>
        /// Directory to save fetched web files
        /// </summary>
        protected string mDirectory;
        /// <summary>
        /// Store for url and file path pairs
        /// </summary>
        protected IUrlFileStore mStore;
        /// <summary>
        /// Logger
        /// </summary>
        protected RuntimeLogger mLogger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="directory">Directory to save fetched web files</param>
        public SimpleFetcher(CrawlerConfig config)
        {
            mDirectory = config.FetchDirectory;
            mStore = config.UrlFileStore;
            mLogger = new RuntimeLogger(Path.Combine(config.LogDirectory, "SimpleFetcher.Log"));
            if (!Directory.Exists(mDirectory))
            {
                Directory.CreateDirectory(mDirectory);
            }
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

            mLogger.Log(nameof(SimpleFetcher), "Fetching URL: " + url);
            // Target path to save downloaded web file
            var path = Path.Combine(mDirectory, UrlHelper.UrlToFileName(url));
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 10);
                var response = await client.GetAsync(url);
                var type = response.Content.Headers.ContentType;
                if (type.MediaType == "text/html")
                {
                    // If it is HTML, use PhantomJS to fetch real web page content
                    File.WriteAllText(path, FetchPageContent(url));
                }
                else
                {
                    // Otherwise, directly save it
                    using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                        stream = new FileStream(path, FileMode.Create, FileAccess.Write))
                    {
                        await contentStream.CopyToAsync(stream);
                    }
                }
                mLogger.Log(nameof(SimpleFetcher), string.Format("Fetched URL: {0} to {1}", url, path));
                return await mStore.SaveAsync(new UrlFile
                {
                    Url = url,
                    FilePath = path,
                    Charset = type.CharSet,
                    MimeType = type.MediaType,
                });
            }
        }

        private static string FetchPageContent(string url)
        {
            var driver = new PhantomJSDriver(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
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
