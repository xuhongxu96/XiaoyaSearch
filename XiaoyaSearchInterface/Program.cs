using System;
using System.Linq;
using XiaoyaCommon.ArgumentParser;
using XiaoyaNLP.Helper;
using XiaoyaSearch;
using XiaoyaSearch.Config;
using XiaoyaStore.Store;

namespace XiaoyaSearchInterface
{
    class Program
    {
        static void Main(string[] args)
        {
            var arguments = Parser.ParseArguments<SearchArguments>(args);

            var config = new SearchEngineConfig
            {
                UrlFileStore = new UrlFileStore(),
                InvertedIndexStore = new InvertedIndexStore(),
                PostingListStore = new PostingListStore(),
                LogDirectory = arguments.LogDir,
            };

            var engine = new SearchEngine(config);
            var urlFileStore = config.UrlFileStore;

            while (true)
            {
                Console.WriteLine("Search:");
                var query = Console.ReadLine();

                var results = engine.Search(query);

                var count = 0;

                foreach (var result in results)
                {
                    var urlFile = urlFileStore.GetUrlFile(result.UrlFileId);

                    Console.WriteLine("{0}: {1} ({2}, {3})", result.UrlFileId, urlFile.Url, result.Score, result.ProScore);

                    if (result.WordPositions == null)
                    {
                        Console.WriteLine("  " + urlFile.TextContent.Substring(0, 50).Replace("\r", "").Replace("\n", "  "));
                    }
                    else
                    {
                        var orderPos = result.WordPositions.OrderBy(o => o.Position);
                        var minWordPos = orderPos.First();
                        var minPos = (int) Math.Max(minWordPos.Position - 50, 0);
                        var maxPos = orderPos.Last();

                        var content = urlFile.TextContent;

                        Console.WriteLine("  "
                            + content.Substring(minPos,
                            Math.Min((int) maxPos.Position - minPos + maxPos.Word.Length + 50, content.Length - minPos))
                            .Replace("\r", "").Replace("\n", "  "));
                    }

                    Console.WriteLine("");

                    count++;

                    if (count % 10 == 0)
                    {
                        Console.WriteLine("Input N to View Next Page, Otherwise Exit.");
                        var cmd = Console.ReadLine();
                        if (cmd != "N")
                        {
                            break;
                        }
                    }
                }

                Console.WriteLine();
            }

        }
    }
}
