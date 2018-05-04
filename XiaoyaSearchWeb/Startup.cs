using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using XiaoyaStore.Data;

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

            var options = new DbContextOptionsBuilder<XiaoyaSearchContext>()
                                .UseSqlServer("Data Source=IR-PC;Initial Catalog=XiaoyaSearch;Integrated Security=True")
                                .Options;
            EngineOptions.IndexStatStore = new XiaoyaStore.Store.IndexStatStore(options);
            EngineOptions.InvertedIndexStore = new XiaoyaStore.Store.InvertedIndexStore(options);
            EngineOptions.UrlFileStore = new XiaoyaStore.Store.UrlFileStore(options);
            EngineOptions.SearchEngine = new XiaoyaSearch.SearchEngine(new XiaoyaSearch.Config.SearchEngineConfig
            {
                UrlFileStore = EngineOptions.UrlFileStore,
                IndexStatStore = EngineOptions.IndexStatStore,
                InvertedIndexStore = EngineOptions.InvertedIndexStore,
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
