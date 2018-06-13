using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace XiaoyaSearchWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            EngineOptions.PostingListStore = new XiaoyaStore.Store.PostingListStore();
            EngineOptions.InvertedIndexStore = new XiaoyaStore.Store.InvertedIndexStore();
            EngineOptions.UrlFileStore = new XiaoyaStore.Store.UrlFileStore();
            EngineOptions.SearchEngine = new XiaoyaSearch.SearchEngine(new XiaoyaSearch.Config.SearchEngineConfig
            {
                UrlFileStore = EngineOptions.UrlFileStore,
                PostingListStore = EngineOptions.PostingListStore,
                InvertedIndexStore = EngineOptions.InvertedIndexStore,
                LogDirectory = @"D:\XiaoyaSearch\Logs"
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
