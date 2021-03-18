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
using System.Linq;
using Amazon.Lambda.Core;
using Timkoto.Data.Services;
using Timkoto.Data.Services.Interfaces;
using Timkoto.UsersApi.Authorization;
using Timkoto.UsersApi.Authorization.Interfaces;
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
            services.AddTransient<ITransactionService, TransactionService>();
            services.AddTransient<IPlayerService, PlayerService>();
            services.AddTransient<IHttpService, HttpService>();
            services.AddTransient<ICognitoUserStore, CognitoUserStore>();
            services.AddTransient<IOperatorService, OperatorService>();
            services.AddTransient<IContestService, ContestService>();
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

            app.UseCors(_ => _.WithOrigins("*")
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseHttpsRedirection();
            
            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthentication();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
