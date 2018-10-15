using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFCoreAspNetCore.Data;
using EFCoreAspNetCore.Web.Filters;
using EFCoreAspNetCore.Web.Swashbuckle;
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
using Swashbuckle.AspNetCore.Swagger;

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

            services.AddOData();

            services.AddMvc(options =>
            {
                options.Filters.Add(new ExposeHeadersResultFilter());
                options.Filters.Add(new VersionResultFilter());
                options.Filters.Add(new ODataResultFilter());
                options.Filters.Add(new ItemNotFoundExceptionFilter());
                options.Filters.Add(new JsonPatchParseExceptionFilter());
                options.Filters.Add(new PatchCommandResultFilter());

                // We're not using the standard OData output, and it breaks swagger
                options.OutputFormatters.RemoveType<Microsoft.AspNet.OData.Formatter.ODataOutputFormatter>();
            }).AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            });

            services.AddSwaggerGen(c =>
            {
                c.OperationFilter<ODataOperationFilter>();
                c.OperationFilter<SummaryByFunctionNameOperationFilter>();
                c.OperationFilter<PatchCommandResultOperationFilter>();
                c.DescribeAllEnumsAsStrings();
                c.SwaggerDoc("v1", new Info { Title = "Example ASPNetCore + EFCore API", Version = "v1" });
            });
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

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Example ASPNetCore + EFCore API V1");
            });
        }
    }
}
