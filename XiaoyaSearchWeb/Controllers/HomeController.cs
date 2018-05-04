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
            var searchResults = new List<SearchResultItem>();

            await Task.Run(() =>
            {
                var results = EngineOptions.SearchEngine.Search(query);

                foreach (var result in results)
                {
                    var urlFile = EngineOptions.UrlFileStore.LoadById(result.UrlFileId);

                    var searchResultItem = new SearchResultItem();
                    searchResultItem.Title = urlFile.Title;
                    searchResultItem.Url = urlFile.Url;
                    searchResultItem.Score = result.Score;
                    searchResultItem.ProScore = result.ProScore;

                    if (result.WordPositions == null)
                    {
                        searchResultItem.Details = urlFile.TextContent.Substring(0, 50).Replace("\r", "").Replace("\n", "  ");
                    }
                    else
                    {
                        var orderPos = result.WordPositions.OrderBy(o => o.Position);
                        var minWordPos = orderPos.First();
                        var minPos = Math.Max(minWordPos.Position - 50, 0);
                        var maxPos = orderPos.Last();

                        var content = urlFile.TextContent;

                        searchResultItem.Details = content.Substring(minPos,
                            Math.Min(maxPos.Position - minPos + maxPos.Word.Length + 50, content.Length - minPos))
                            .Replace("\r", "").Replace("\n", "  ");
                    }

                    searchResults.Add(searchResultItem);
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
