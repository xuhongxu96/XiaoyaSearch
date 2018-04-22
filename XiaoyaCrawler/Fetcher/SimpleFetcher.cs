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
using XiaoyaCommon.Helper;
using XiaoyaNLP.Encoding;

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
            var uri = new Uri(url);

            if (uri.Scheme != "http" && uri.Scheme != "https")
            {
                throw new NotSupportedException("Not supported Uri Scheme: " + uri.Scheme);
            }

            // Target path to save downloaded web file
            var filePath = Path.Combine(mConfig.FetchDirectory, UrlHelper.UrlToFileName(url));
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
                    throw new IOException(statusCode.ToString() + ": " + url);
                }


                if (mConfig.UsePhantomJS && (type == null || type.MediaType == "text/html"))
                {
                    // If config is set to use PhantomJS and web content type is HTML, 
                    // use PhantomJS to fetch real web page content
                    File.WriteAllText(filePath,
                        FetchPageContentByPhantomJS(url, mConfig.PhantomJSDriverPath));
                }
                else
                {
                    // Otherwise, directly save it if supported by parser
                    if (type == null || UniversalFileParser.IsSupported(type.MediaType))
                    {
                        using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                        stream = File.Create(filePath))
                        {
                            await contentStream.CopyToAsync(stream);
                        }
                    }
                    else
                    {
                        throw new NotSupportedException("Not supported media type: " + type?.MediaType);
                    }
                }

                var contentType = MimeHelper.GetContentType(filePath);
                if (contentType == null && type != null)
                {
                    contentType = type.MediaType;
                }

                if (contentType == null)
                {
                    mLogger.Log(nameof(SimpleFetcher), "Unknown MIME type: " + url);
                    return null;
                }

                if (!UniversalFileParser.IsSupported(contentType))
                {
                    File.Delete(filePath);
                    mLogger.Log(nameof(SimpleFetcher), $"Deleted Not-Supported MIME type ({contentType}): {url}");
                    return null;
                }

                var charset = EncodingDetector.GetEncoding(filePath);
                if (charset == null)
                {
                    charset = type.CharSet;
                }

                return new UrlFile
                {
                    Url = url,
                    FilePath = filePath,
                    Charset = charset,
                    MimeType = contentType,
                    FileHash = HashHelper.GetFileMd5(filePath),
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
