﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Timkoto.Data.Repositories;
using Timkoto.Data.Services.Interfaces;
using Timkoto.UsersApi.Enumerations;
using Timkoto.UsersApi.Models;
using Timkoto.UsersApi.Services.Interfaces;

namespace Timkoto.UsersApi.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly IPersistService _persistService;

        public PlayerService(IPersistService persistService)
        {
            _persistService = persistService;
        }

        public async Task<ResponseBase> GetPlayer(long userId, Guid traceId, List<string> messages)
        {
            GetPlayerResponse getPlayersResult;

            var player =
                await _persistService.FindOne<User>(_ => _.Id == userId);

            if (player == null)
            {
                getPlayersResult =
                    GetPlayerResponse.Create(false, traceId, HttpStatusCode.Forbidden,  GetPlayerResult.NoPlayerFound);
                
                return getPlayersResult;
            }

            var playerWallet =
                await _persistService.FindLast<Transaction>(_ => _.UserId == userId, _ => _.CreateDateTime);

            getPlayersResult =
                GetPlayerResponse.Create(true, traceId, HttpStatusCode.OK, GetPlayerResult.PlayerFound);
            
            getPlayersResult.Data = new { Player = player, Wallet = playerWallet  };

            
            return getPlayersResult;
        }
    }
}
