using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Runtime;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Timkoto.Data.Repositories;
using Timkoto.Data.Services;
using Timkoto.Data.Services.Interfaces;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Timkoto.WebSocket
{
    public class Functions
    {
        private readonly IPersistService _persistService;

        private static readonly DbSession _dbSession = new DbSession();

        /// <summary>
        /// Factory func to create the AmazonApiGatewayManagementApiClient. This is needed to created per endpoint of the a connection. It is a factory to make it easy for tests
        /// to moq the creation.
        /// </summary>
        Func<string, IAmazonApiGatewayManagementApi> ApiGatewayManagementApiClientFactory { get; }

        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public Functions()
        {
            var sessionFactory = _dbSession.GetSessionFactory();
            _persistService = new PersistService(sessionFactory);

            ApiGatewayManagementApiClientFactory = (Func<string, AmazonApiGatewayManagementApiClient>)((endpoint) =>
            {
                return new AmazonApiGatewayManagementApiClient(new AmazonApiGatewayManagementApiConfig
                {
                    ServiceURL = endpoint
                });
            });
        }

        /// <summary>
        /// Constructor used for testing allow tests to pass in moq versions of the service clients.
        /// </summary>
        /// <param name="apiGatewayManagementApiClientFactory">The API gateway management API client factory.</param>
        public Functions(Func<string, IAmazonApiGatewayManagementApi> apiGatewayManagementApiClientFactory)
        {
            ApiGatewayManagementApiClientFactory = apiGatewayManagementApiClientFactory;
        }

        public async Task<APIGatewayProxyResponse> OnConnectHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                var connectionId = request.RequestContext.ConnectionId;
                context.Logger.LogLine($"ConnectionId: {connectionId}");

                var operatorId = String.Empty;
                var hasOperatorId = request.QueryStringParameters?.TryGetValue("operatorId", out operatorId);

                if (hasOperatorId.HasValue && hasOperatorId.Value && !string.IsNullOrWhiteSpace(operatorId))
                {
                    var connection = new WsConnection
                    {
                        ConnectionId = connectionId,
                        OperatorId = operatorId
                    };
                    await _persistService.Save(connection);
                }

                //var connection = new WsConnection
                //{
                //    ConnectionId = connectionId,
                //    OperatorId = "1"
                //};
                //await _persistService.Save(connection);

                return new APIGatewayProxyResponse
                {
                    StatusCode = 200,
                    Body = "Connected."
                };
            }
            catch (Exception e)
            {
                context.Logger.LogLine("Error connecting: " + e.Message);
                context.Logger.LogLine(e.StackTrace);
                return new APIGatewayProxyResponse
                {
                    StatusCode = 500,
                    Body = $"Failed to connect: {e.Message}"
                };
            }
        }

        public async Task<APIGatewayProxyResponse> SendMessageHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                var domainName = request.RequestContext.DomainName;
                var stage = request.RequestContext.Stage;
                var endpoint = $"https://{domainName}/{stage}";
                context.Logger.LogLine($"API Gateway management endpoint: {endpoint}");

                var message = JsonDocument.Parse(request.Body);
                context.Logger.LogLine($"request.Body: {request.Body}");

                if (request.QueryStringParameters != null)
                {
                    context.Logger.LogLine($"request.QueryStringParameters: {JsonConvert.SerializeObject(request.QueryStringParameters)}");
                }

                if (!message.RootElement.TryGetProperty("data", out var dataProperty))
                {
                    context.Logger.LogLine("Failed to find data element in JSON document");
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest
                    };
                }

                var data = dataProperty.GetString();
                var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

                var apiClient = ApiGatewayManagementApiClientFactory(endpoint);

                var operatorId = string.Empty;
                var hasOperatorId = request.QueryStringParameters?.TryGetValue("operatorId", out operatorId);
                if (string.IsNullOrWhiteSpace(operatorId))
                {
                    return null;
                }

                var count = 0;
                var connections = await _persistService.FindMany<WsConnection>(_ => _.Id > 0 && _.OperatorId == operatorId);
                //var connections = await _persistService.FindMany<WsConnection>(_ => _.Id > 0);
                foreach (var connection in connections)
                {
                    var postConnectionRequest = new PostToConnectionRequest
                    {
                        ConnectionId = connection.ConnectionId,
                        Data = stream
                    };

                    try
                    {
                        context.Logger.LogLine($"Post to connection {count}: {postConnectionRequest.ConnectionId}");
                        stream.Position = 0;
                        await apiClient.PostToConnectionAsync(postConnectionRequest);
                        count++;
                    }
                    catch (AmazonServiceException e)
                    {
                        if (e.StatusCode == HttpStatusCode.Gone)
                        {
                            context.Logger.LogLine($"Deleting gone connection: {postConnectionRequest.ConnectionId}");
                            await _persistService.ExecuteSql($"delete from wsConnection where connectionId = '{postConnectionRequest.ConnectionId}';");
                        }
                        else
                        {
                            context.Logger.LogLine($"Error posting message to {postConnectionRequest.ConnectionId}: {e.Message}");
                            context.Logger.LogLine(e.StackTrace);
                        }
                    }
                }

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = "Data sent to " + count + " connection" + (count == 1 ? "" : "s")
                };
            }
            catch (Exception e)
            {
                context.Logger.LogLine("Error disconnecting: " + e.Message);
                context.Logger.LogLine(e.StackTrace);
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Body = $"Failed to send message: {e.Message}"
                };
            }
        }

        public async Task<APIGatewayProxyResponse> OnDisconnectHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                var connectionId = request.RequestContext.ConnectionId;
                context.Logger.LogLine($"ConnectionId: {connectionId}");

                await _persistService.ExecuteSql($"delete from wsConnection where connectionId = '{connectionId}';");

                return new APIGatewayProxyResponse
                {
                    StatusCode = 200,
                    Body = "Disconnected."
                };
            }
            catch (Exception e)
            {
                context.Logger.LogLine("Error disconnecting: " + e.Message);
                context.Logger.LogLine(e.StackTrace);
                return new APIGatewayProxyResponse
                {
                    StatusCode = 500,
                    Body = $"Failed to disconnect: {e.Message}"
                };
            }
        }
    }
}