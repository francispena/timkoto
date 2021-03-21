﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Timkoto.UsersApi.Models;

namespace Timkoto.UsersApi.Services.Interfaces
{
    public interface IContestService
    {
        Task<GenericResponse> GetGames(string gameDate, List<string> messages);

        Task<GenericResponse> GetPlayers(string gameDate, List<string> messages);

        Task<GenericResponse> SubmitLineUp(LineUpRequest request, List<string> messages);

        Task<bool> RankTeams(List<string> messages);

        Task<GenericResponse> PrizePool(List<string> messages);
    }
}