using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NHibernate;
using Org.BouncyCastle.Asn1.X509;
using Timkoto.Data.Services;
using Timkoto.Data.Services.Interfaces;
using Timkoto.UsersApi.Extensions;
using Timkoto.UsersApi.Services;
using Timkoto.UsersApi.Services.Interfaces;

namespace Timkoto.UsersApi
{
    public class Startup
    {
        public const string AppS3BucketKey = "AppS3Bucket";

        public Startup(IConfiguration configuration)
        {
            //Configuration = configuration;
            var environment = Environment.GetEnvironmentVariable("Environment") ?? "Development";

            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            JsonConvert.DefaultSettings = () =>
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new StringEnumConverter {NamingStrategy = new CamelCaseNamingStrategy()});
                return settings;
            };
        }

        public static IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            
            var connectionString = Configuration.GetConnectionString("TimkotoWrite");

            services.AddControllers();

            services.AddSwaggerGen(_ =>
            {
                _.SwaggerDoc("v1", new OpenApiInfo {Title = "TimKoTo API", Version = "v1"});
                _.ResolveConflictingActions(__ => __.First());
            });
                
            // Add S3 to the ASP.NET Core dependency injection framework.
            //services.AddAWSService<Amazon.S3.IAmazonS3>();
            
            services.AddNHibernate(connectionString);

            services.AddTransient<IPersistService, PersistService>();
            services.AddTransient<IUserService, UserService>();

            //services.AddSingleton(typeof(DbManager));
            //services.AddTransient<ISessionFactory>(_ =>
            //{
            //    var dbManager = new DbManager();
            //    return dbManager.SessionFactory;
            //});

            //services.AddTransient<ISession>(_ =>
            //{
            //    var dbManager = new DbManager();
            //    return dbManager.SessionFactory.OpenSession();
            //});
            //Test Change for Github

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger(c =>
            {
                c.RouteTemplate = "swagger/{documentName}/swagger.json";
            });

            app.UseSwaggerUI(_ =>
            {
                var swaggerJsonBasePath = string.IsNullOrWhiteSpace(_.RoutePrefix) ? "." : "..";
                _.SwaggerEndpoint(url: $"{swaggerJsonBasePath}/swagger/v1/swagger.json", name: "TimKoTo API V1");
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
