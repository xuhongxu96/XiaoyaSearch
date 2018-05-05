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
using System.Text.RegularExpressions;

namespace XiaoyaCrawler.Fetcher
{
    public class SimpleFetcher : IFetcher
    {
        private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.181 Safari/537.36";
        private readonly Regex Html4CharsetRegex = new Regex(@"<\s*meta\s*.*?\s*content\s*=\s*""\s*text/html\s*;\s*charset\s*=\s*([^\s""]{3,30})\s*""\s*", RegexOptions.Compiled);
        private readonly Regex Html5CharsetRegex = new Regex(@"<\s*meta\s*charset\s*=\s*""\s*([^\s""]{3,30})\s*""\s*>", RegexOptions.Compiled);

        protected CrawlerConfig mConfig;
        protected RuntimeLogger mLogger;
        protected HttpClient mClientWithProxy, mClientWithoutProxy;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="directory">Directory to save fetched web files</param>
        public SimpleFetcher(CrawlerConfig config)
        {
            mLogger = new RuntimeLogger(Path.Combine(config.LogDirectory, "Crawler.Log"));
            if (Directory.Exists(config.FetchDirectory))
            {
                Directory.Delete(config.FetchDirectory, true);
            }
            Directory.CreateDirectory(config.FetchDirectory);

            mConfig = config;

            var handler = new HttpClientHandler
            {
                UseProxy = false,
            };

            mClientWithoutProxy = new HttpClient(handler);
            mClientWithProxy = new HttpClient();

            mClientWithoutProxy.DefaultRequestHeaders.Connection.Add("keep-alive");
            mClientWithoutProxy.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
            mClientWithoutProxy.DefaultRequestHeaders.Accept.ParseAdd("*/*");
            mClientWithoutProxy.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en,zh-CN;q=0.9,zh;q=0.8,zh-TW;q=0.7,de;q=0.6,ru;q=0.5");
            mClientWithoutProxy.Timeout = new TimeSpan(0, 0, 8);

            mClientWithProxy.DefaultRequestHeaders.Connection.Add("keep-alive");
            mClientWithProxy.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
            mClientWithoutProxy.DefaultRequestHeaders.Accept.ParseAdd("*/*");
            mClientWithoutProxy.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en,zh-CN;q=0.9,zh;q=0.8,zh-TW;q=0.7,de;q=0.6,ru;q=0.5");
            mClientWithProxy.Timeout = new TimeSpan(0, 0, 30);
        }


        private string DetectEncoding(string filePath, string defaultEncoding)
        {
            string detectedCharset = null;

            int lineCount = 0;
            bool isHtml = false;

            foreach (var line in File.ReadLines(filePath))
            {
                var lineContent = line.ToLower().Trim();

                // skip empty lines
                if (lineContent == "")
                {
                    continue;
                }

                // 10 lines but no <html>, give up
                if (lineCount++ > 10 && !isHtml)
                {
                    break;
                }
                // found <html>
                if (!isHtml && lineContent.Contains("<html"))
                {
                    isHtml = true;
                }
                // Arrived <body>, give up
                if (lineContent.Contains("<body"))
                {
                    break;
                }

                // Already detected <html>, then found <meta>
                if (isHtml)
                {
                    var match = Html4CharsetRegex.Match(lineContent);
                    if (match.Success && match.Groups.Count == 2)
                    {
                        detectedCharset = match.Groups[1].Value;
                        break;
                    }

                    match = Html5CharsetRegex.Match(lineContent);
                    if (match.Success && match.Groups.Count == 2)
                    {
                        detectedCharset = match.Groups[1].Value;
                        break;
                    }
                }
            }

            var autoDetectedCharset = EncodingDetector.GetEncoding(filePath);

            if (detectedCharset == null || autoDetectedCharset == "UTF-8")
            {
                detectedCharset = autoDetectedCharset;
                if (detectedCharset == null)
                {
                    detectedCharset = defaultEncoding;
                }
            }

            return detectedCharset.ToUpper();
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

            var client = mConfig.NotUseProxyUrlRegex.IsMatch(url) ? mClientWithoutProxy : mClientWithProxy;

            var response = await client.GetAsync(url);
            var statusCode = response.StatusCode;
            var contentType = response.Content.Headers.ContentType;

            if (statusCode != HttpStatusCode.Accepted
                && statusCode != HttpStatusCode.OK)
            {
                mLogger.Log(nameof(SimpleFetcher), "Status: " + statusCode + " " + url);
                throw new IOException(statusCode.ToString() + ": " + url);
            }

            if (mConfig.UsePhantomJS && (contentType == null || contentType.MediaType == "text/html"))
            {
                // If config is set to use PhantomJS and web content type is HTML, 
                // use PhantomJS to fetch real web page content
                File.WriteAllText(filePath,
                    FetchPageContentByPhantomJS(url, mConfig.PhantomJSDriverPath));
            }
            else
            {
                // Otherwise, directly save it if supported by parser
                if (contentType == null || UniversalFileParser.IsSupported(contentType.MediaType))
                {
                    using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                    stream = File.Create(filePath))
                    {
                        await contentStream.CopyToAsync(stream);
                    }
                }
                else
                {
                    throw new NotSupportedException("Not supported media type: " + contentType?.MediaType);
                }
            }

            #region Detect Content MIME Type
            var detectedContentType = MimeHelper.GetContentType(filePath);
            if ((detectedContentType == null
                || detectedContentType == "application/octet-stream"
                || detectedContentType == "inode/x-empty")
                && contentType != null)
            {
                detectedContentType = contentType.MediaType;
            }

            if (detectedContentType == null)
            {
                File.Delete(filePath);
                mLogger.Log(nameof(SimpleFetcher), "Unknown MIME type: " + url);
                return null;
            }

            if (!UniversalFileParser.IsSupported(detectedContentType))
            {
                File.Delete(filePath);
                mLogger.Log(nameof(SimpleFetcher), $"Deleted Not-Supported MIME type ({detectedContentType}): {url}");
                return null;
            }
            #endregion

            var detectedCharset = DetectEncoding(filePath, contentType.CharSet);

            return new UrlFile
            {
                Url = url,
                FilePath = filePath,
                Charset = detectedCharset,
                MimeType = detectedContentType,
                FileHash = HashHelper.GetFileMd5(filePath),
            };
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
