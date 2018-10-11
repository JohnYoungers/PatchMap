using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFCoreAspNetCore.Data;
using EFCoreAspNetCore.Web.Filters;
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

namespace EFCoreAspNetCore.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            Application.InitializeServices(services, Configuration);

            services.AddMvc(options =>
            {
                options.Filters.Add(new ExposeHeadersResultFilter());
                options.Filters.Add(new VersionResultFilter());
                options.Filters.Add(new ODataResultFilter());
                options.Filters.Add(new ItemNotFoundExceptionFilter());
                options.Filters.Add(new JsonPatchParseExceptionFilter());
                options.Filters.Add(new PatchCommandResultFilter());
            }).AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            });

            services.AddOData();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            Application.FinalizeInitialization(app.ApplicationServices);

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvc(routeBuilder =>
            {
                routeBuilder.EnableDependencyInjection();
            });
        }
    }
}
