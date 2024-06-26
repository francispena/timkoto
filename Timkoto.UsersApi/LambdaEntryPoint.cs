using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Runtime.Internal.Util;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using Timkoto.UsersApi.Services.Interfaces;
using Timkoto.Data.Services.Interfaces;
using Timkoto.Data.Repositories;
using Timkoto.Data.Enumerations;

namespace Timkoto.UsersApi
{
    /// <summary>
    /// This class extends from APIGatewayProxyFunction which contains the method FunctionHandlerAsync which is the 
    /// actual Lambda function entry point. The Lambda handler field should be set to
    /// 
    /// Timkoto::Timkoto.LambdaEntryPoint::FunctionHandlerAsync
    /// </summary>
    public class LambdaEntryPoint :

        // The base class must be set to match the AWS service invoking the Lambda function. If not Amazon.Lambda.AspNetCoreServer
        // will fail to convert the incoming request correctly into a valid ASP.NET Core request.
        //
        // API Gateway REST API                         -> Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
        // API Gateway HTTP API payload version 1.0     -> Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
        // API Gateway HTTP API payload version 2.0     -> Amazon.Lambda.AspNetCoreServer.APIGatewayHttpApiV2ProxyFunction
        // Application Load Balancer                    -> Amazon.Lambda.AspNetCoreServer.ApplicationLoadBalancerFunction
        // 
        // Note: When using the AWS::Serverless::Function resource with an event type of "HttpApi" then payload version 2.0
        // will be the default and you must make Amazon.Lambda.AspNetCoreServer.APIGatewayHttpApiV2ProxyFunction the base class.

        Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
        {

        private IServiceProvider _serviceProvider;

        private IContestService _contestService;

        private IPersistService _persistService;

        /// <summary>
        /// The builder has configuration, logging and Amazon API Gateway already configured. The startup class
        /// needs to be configured in this method using the UseStartup<>() method.
        /// </summary>
        /// <param name="builder"></param>
        protected override void Init(IWebHostBuilder builder)
        {
            builder
                .UseStartup<Startup>();

        }

        /// <summary>
        /// Use this override to customize the services registered with the IHostBuilder. 
        /// 
        /// It is recommended not to call ConfigureWebHostDefaults to configure the IWebHostBuilder inside this method.
        /// Instead customize the IWebHostBuilder in the Init(IWebHostBuilder) overload.
        /// </summary>
        /// <param name="builder"></param>
        protected override void Init(IHostBuilder builder)
        {
        }

        public override async Task<APIGatewayProxyResponse> FunctionHandlerAsync(APIGatewayProxyRequest request, ILambdaContext lambdaContext)
        {
            _serviceProvider = Startup.ServiceProvider;
            _contestService = _serviceProvider.GetService<IContestService>();
            _persistService = _serviceProvider.GetService<IPersistService>();

            if (request.Resource == "GetLiveStatsRapid")
            {
                var currentHour = DateTime.UtcNow.Hour;
                if (currentHour >= 4 && currentHour <= 6)
                {
                    await GetLiveStatsNba(lambdaContext);
                    return new APIGatewayProxyResponse { StatusCode = 200 };
                }
                else
                {
                    await GetLiveStatsRapid(lambdaContext);
                }
                
                var contest = await _persistService.FindOne<Contest>(_ => _.ContestState == ContestState.Ongoing);
                if (contest == null)
                {
                    return new APIGatewayProxyResponse { StatusCode = 200 };
                }

                var allContestGames = await _persistService.FindMany<Game>(_ => _.ContestId == contest.Id);
                if (allContestGames.Any())
                {
                    var allComplete = allContestGames.All(_ => _.Finished == true);
                    if (allContestGames.Any() && allComplete)
                    {
                        await GetLiveStatsNba(lambdaContext);
                    }
                }
                return new APIGatewayProxyResponse {StatusCode = 200};
            }

            if (request.Resource == "GetLiveStatsNba")
            {
                await GetLiveStatsNba(lambdaContext);
                return new APIGatewayProxyResponse { StatusCode = 200 };
            }

            if (request.Resource == "WarmingLambda")
            {
                int.TryParse(request.Body, out var concurrencyCount);

                if (concurrencyCount > 1)
                {
                    var client = new AmazonLambdaClient();
                    await client.InvokeAsync(new Amazon.Lambda.Model.InvokeRequest
                    {
                        FunctionName = lambdaContext.FunctionName,
                        InvocationType = InvocationType.RequestResponse,
                        Payload = JsonConvert.SerializeObject(new APIGatewayProxyRequest
                        {
                            Resource = request.Resource,
                            Body = (concurrencyCount - 1).ToString()
                        })
                    });
                }

                return new APIGatewayProxyResponse { };
            }

            Startup.LambdaContext = lambdaContext;
            return await base.FunctionHandlerAsync(request, lambdaContext);
        }

        private async Task GetLiveStatsNba(ILambdaContext lambdaContext)
        {
            var officialNbaStatistics = _serviceProvider.GetService<IOfficialNbaStatistics>();

            var getLiveStats = await officialNbaStatistics.GetLiveStats(new List<string>());
            lambdaContext.Logger.Log($"Get Live Stats Result - {getLiveStats }");

            var rankTeams = await _contestService.RankTeams(new List<string>());

            lambdaContext.Logger.Log($"Rank Teams Result - {rankTeams}");

            //process if all games completed
            var contest = await _persistService.FindOne<Contest>(_ => _.ContestState == ContestState.Ongoing);
            if (contest == null)
            {
                return;
            }

            var allContestGames = await _persistService.FindMany<OfficialNbaSchedules>(_ => _.GameDate == contest.GameDate);
            if (allContestGames.Any())
            {
                var allComplete = allContestGames.All(_ => _.Finished == true);
                if (allContestGames.Any() && allComplete)
                {
                    var messages = new List<string>();
                    await _contestService.SetPrizes(messages);
                    await _contestService.SetPrizesInTransaction(messages);
                    //await _contestService.CreateContest(0, messages);
                }
            }
        }

        private async Task GetLiveStatsRapid(ILambdaContext lambdaContext)
        {
            var rapidNbaStatistics = _serviceProvider.GetService<IRapidNbaStatistics>();

            var getLiveStats = await rapidNbaStatistics.GetLiveStats2(new List<string>());
            lambdaContext.Logger.Log($"Get Final Stats Result - {getLiveStats }");

            var rankTeams = await _contestService.RankTeams(new List<string>());
            lambdaContext.Logger.Log($"Rank Teams Result - {rankTeams}");
        }
    }
}
