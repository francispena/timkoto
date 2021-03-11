using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using Timkoto.Data.Services;
using Timkoto.Data.Services.Interfaces;
using Timkoto.UsersApi.Extensions;
using Timkoto.UsersApi.Services;
using Timkoto.UsersApi.Services.Interfaces;

namespace Timkoto.UsersApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
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
                settings.Converters.Add(new StringEnumConverter { NamingStrategy = new CamelCaseNamingStrategy() });
                return settings;
            };
        }

        public static IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("TimkotoWrite");

            services.AddControllers()
                .AddNewtonsoftJson(options =>
                    options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter()));

            services.AddControllersWithViews().AddNewtonsoftJson();

            services.AddSwaggerGen(_ =>
            {
                _.SwaggerDoc("v1", new OpenApiInfo {Title = "TimKoTo API", Version = "v1"});
                _.ResolveConflictingActions(__ => __.First());
            });

            services.AddNHibernate(connectionString);

            services.AddTransient<IPersistService, PersistService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IRegistrationCodeService, RegistrationCodeService>();
            services.AddTransient<IAgentService, AgentService>();
            
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
