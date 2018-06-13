using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace XiaoyaSearchWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args.Skip(1).ToArray())
                .UseUrls(new string[]
                {
                    "http://*:" + (args.Length > 0 ? args[0] : "8080"),
                })
                .UseStartup<Startup>()
                .Build();
    }
}
