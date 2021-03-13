using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Timkoto.Data.Repositories;
using Timkoto.Data.Services.Interfaces;
using Timkoto.UsersApi.Enumerations;
using Timkoto.UsersApi.Models;
using Timkoto.UsersApi.Services.Interfaces;

namespace Timkoto.UsersApi.Services
{
    public class AgentService : IAgentService
    {
        private readonly IPersistService _persistService;

        public AgentService(IPersistService persistService)
        {
            _persistService = persistService;
        }

        
    }
}
