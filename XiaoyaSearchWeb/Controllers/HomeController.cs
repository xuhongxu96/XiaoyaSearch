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

        public async Task<IActionResult> Search(string query)
        {
            var searchResults = new WebSearchResult
            {
                Query = query.Trim(),
            };

            await Task.Run(() =>
            {
                var results = EngineOptions.SearchEngine.Search(query);

                foreach (var result in results)
                {
                    var urlFile = EngineOptions.UrlFileStore.GetUrlFile(result.UrlFileId);

                    var searchResultItem = new SearchResultItem
                    {
                        Title = urlFile.Title,
                        Url = urlFile.Url,
                        PublishDate = DateTime.FromBinary((long) urlFile.PublishDate),
                        Score = result.Score.Value,
                        ProScore = result.ProScore.Value,
                        ScoreDebugInfo = result.Score.ToString(),
                        ProScoreDebugInfo = result.ProScore.ToString(),
                    };

                    if (result.WordPositions == null)
                    {
                        searchResultItem.Details = urlFile.TextContent
                            .Substring(0, Math.Min(50, urlFile.TextContent.Length)).Replace("\r", "").Replace("\n", "  ");
                    }
                    else
                    {
                        var orderPos = result.WordPositions.OrderBy(o => o.Position);
                        var minWordPos = orderPos.First();
                        var minPos = (int) Math.Max(minWordPos.Position - 50, 0);
                        var maxPos = orderPos.Last();

                        var content = urlFile.TextContent;

                        searchResultItem.Details = content.Substring((int) minPos,
                            Math.Min((int) maxPos.Position - minPos + maxPos.Word.Length + 50, content.Length - minPos))
                            .Replace("\r", "").Replace("\n", "  ");
                    }

                    searchResults.Items.Add(searchResultItem);
                }
            });

            return View("Index", searchResults);

        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
