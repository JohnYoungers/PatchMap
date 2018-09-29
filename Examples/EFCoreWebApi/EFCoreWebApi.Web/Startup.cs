using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFCoreWebApi.Data;
using EFCoreWebApi.Web.Filters;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;

namespace EFCoreWebApi.Web
{
    public class Startup
    {
        public static readonly LoggerFactory SqlLogger = new LoggerFactory(new[] { new DebugLoggerProvider((cat, level) => cat == DbLoggerCategory.Database.Command.Name && level == LogLevel.Information) });

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.Filters.Add(new ExposeHeadersResultFilter());
                options.Filters.Add(new VersionResultFilter());
                options.Filters.Add(new ODataResultFilter());
                options.Filters.Add(new ItemNotFoundExceptionFilter());
            }).AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            });

            services.AddOData();

            services.AddDbContext<ExampleContext>(options => options
                .UseLoggerFactory(SqlLogger)
                .UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=example;Trusted_Connection=True;ConnectRetryCount=0")
            );
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvc(routeBuilder =>
            {
                routeBuilder.EnableDependencyInjection();
            });

            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<ExampleContext>();
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }
        }
    }
}
