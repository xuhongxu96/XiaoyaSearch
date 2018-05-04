using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using XiaoyaSearchWeb.Models;
using System.Text;

namespace XiaoyaSearchWeb.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public async Task<string> Search(string query)
        {
            var stringBuilder = new StringBuilder();

            await Task.Run(() =>
            {
                var results = EngineOptions.SearchEngine.Search(query);
                var count = 0;

                foreach (var result in results)
                {
                    var urlFile = EngineOptions.UrlFileStore.LoadById(result.UrlFileId);

                    stringBuilder.AppendFormat("{0}: {1} ({2}, {3})\n", result.UrlFileId, urlFile.Url, result.Score, result.ProScore);
                    stringBuilder.AppendLine(urlFile.Title);

                    if (result.WordPositions == null)
                    {
                        stringBuilder.AppendLine("  " + urlFile.TextContent.Substring(0, 50).Replace("\r", "").Replace("\n", "  "));
                    }
                    else
                    {
                        var orderPos = result.WordPositions.OrderBy(o => o.Position);
                        var minWordPos = orderPos.First();
                        var minPos = Math.Max(minWordPos.Position - 50, 0);
                        var maxPos = orderPos.Last();

                        var content = urlFile.TextContent;

                        stringBuilder.AppendLine("  "
                            + content.Substring(minPos,
                            Math.Min(maxPos.Position - minPos + maxPos.Word.Length + 50, content.Length - minPos))
                            .Replace("\r", "").Replace("\n", "  "));
                    }

                    stringBuilder.AppendLine("\n");

                    count++;
                }
            });

            return stringBuilder.ToString();

        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
