using Amazon.Lambda.Core;
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
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.ResponseCompression;
using Timkoto.Data.Services;
using Timkoto.Data.Services.Interfaces;
using Timkoto.UsersApi.Authorization;
using Timkoto.UsersApi.Authorization.Interfaces;
using Timkoto.UsersApi.BaseClasses;
using Timkoto.UsersApi.Extensions;
using Timkoto.UsersApi.Infrastructure;
using Timkoto.UsersApi.Infrastructure.Interfaces;
using Timkoto.UsersApi.Services;
using Timkoto.UsersApi.Services.Interfaces;

namespace Timkoto.UsersApi
{
    public class Startup
    {
        public static ILambdaContext LambdaContext { get; set; }

        public static IConfiguration Configuration { get; private set; }

        public static IServiceProvider ServiceProvider { get; set; }

        private readonly bool _isProd;

        public Startup(IConfiguration configuration)
        {
            var environment = Environment.GetEnvironmentVariable("Environment") ?? "Development";
            _isProd = environment == "Production";
            
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

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("TimkotoWrite");
           
            services.AddControllers()
                .AddNewtonsoftJson(options =>
                    options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter()));

            services.AddControllersWithViews().AddNewtonsoftJson();

            //hide in prod
            services.AddSwaggerGen(_ =>
            {
                _.SwaggerDoc("v1", new OpenApiInfo {Title = "TimKoTo API", Version = "v1"});
                _.ResolveConflictingActions(__ => __.First());

                _.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
                {
                    Description = "Enter your Api Key below:",
                    Name = "x-api-key",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                _.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "ApiKey"
                            },
                        },
                        new List<string>()
                    }
                });
            });

            services.AddNHibernate(connectionString);

            services.AddTransient<IPersistService, PersistService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IRegistrationCodeService, RegistrationCodeService>();
            services.AddTransient<IAgentService, AgentService>();
            services.AddTransient<IPlayerService, PlayerService>();
            services.AddTransient<IHttpService, HttpService>();
            services.AddTransient<ICognitoUserStore, CognitoUserStore>();
            services.AddTransient<IOperatorService, OperatorService>();
            services.AddTransient<IContestService, ContestService>();
            services.AddTransient<IRapidNbaStatistics, RapidNbaStatistics>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<ITransactionService, TransactionService>();
            services.AddSingleton<IAppConfig>(_ => new AppConfig(_isProd));
            services.AddTransient<IVerifier, Verifier>();
            services.AddTransient<ILogger, Logger>();
            services.AddTransient<IOfficialNbaStatistics, OfficialNbaStatistics>();
            
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes =
                    ResponseCompressionDefaults.MimeTypes.Concat(
                        new[] { "image/svg+xml" });
            });

            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseResponseCompression();

            //hide in prod
            app.UseSwagger(c =>
            {
                c.RouteTemplate = "swagger/{documentName}/swagger.json";
            });

            app.UseSwaggerUI(_ =>
            {
                var swaggerJsonBasePath = string.IsNullOrWhiteSpace(_.RoutePrefix) ? "." : "..";
                _.SwaggerEndpoint($"{swaggerJsonBasePath}/swagger/v1/swagger.json", "TimKoTo API V1");
            });

            //var origins = _isProd ? "https://timkoto.com" : "http://localhost:3000";
            var origins = _isProd ? "https://timkoto.com" : "*";

            app.UseCors(_ => _.WithOrigins(origins)
                .AllowAnyMethod()
                .AllowAnyHeader()
                //.AllowCredentials()
                .Build()
            );

            app.UseHttpsRedirection();
            
            app.UseRouting();
            app.UseAuthorization();
            app.UseAuthentication();
     
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            ServiceProvider = app.ApplicationServices;
        }

        private bool IsOriginAllowed(string origin)
        {
            if (!_isProd && origin.Contains("localhost")) return true;
            if (!_isProd && origin == "https://dev.timkoto.com") return true;
            if (_isProd && (origin == "https://timkoto.com" || origin == "https://www.timkoto.com")) return true;

            return false;
        }
    }
}
