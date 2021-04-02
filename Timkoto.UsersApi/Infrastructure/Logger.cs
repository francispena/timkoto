using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Timkoto.UsersApi.Enumerations;
using Timkoto.UsersApi.Extensions;
using Timkoto.UsersApi.Infrastructure.Interfaces;
using Timkoto.UsersApi.Models;

namespace Timkoto.UsersApi.Infrastructure
{
    public class Logger : ILogger
    {
        private readonly ILambdaContext _lambdaContext;

        public Logger()
        {
            _lambdaContext = Startup.LambdaContext;
        }

        public void Log(string header, List<string> messages, LogType logType)
        {
            if (_lambdaContext?.Logger == null)
            {
                return;
            }

            try
            {
                _ = Task.Run(() =>
                {
                    messages.AddWithTimeStamp("APPLICATION_LOGS");
                    messages.AddWithTimeStamp($"LogType-{logType}");

                    var indexedMessages = messages.Select((x, i) => new
                    {
                        item = $"Step {i + 1}. {x}"
                    }).ToList();

                    messages.Clear();
                    indexedMessages.ForEach(_ => messages.Add(_.item));

                    var serializedMessage = JsonConvert.SerializeObject(indexedMessages);

                    _lambdaContext.Logger.Log(serializedMessage);
                });
            }
            catch 
            {
                //dont blow up the logger
            }
        } 
    }
}
