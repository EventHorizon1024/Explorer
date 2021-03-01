using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Explorer.JaegerHttpQuery.MappingProfiles;
using Explorer.Storage;
using Explorer.Storage.Elasticsearch;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace Explorer.JaegerHttpQuery
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
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "Explorer.JaegerHttpQuery", Version = "v1"});
            });
            services.AddExplorerStorage(
                options => options.UseElasticsearch(opt => 
                    Configuration.Bind("Elasticsearch", opt)));

            services.AddSpaStaticFiles(options => options.RootPath = "wwwroot");

            services.AddAutoMapper(options => options.AddProfile<ExplorerQueryProfile>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSpaStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Explorer.JaegerHttpQuery v1"));
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("/index.html");
            });
        }
    }
}