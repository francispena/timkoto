﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Timkoto.UsersApi.Models;

namespace Timkoto.UsersApi.Services.Interfaces
{
    public interface IContestService
    {
        Task<GenericResponse> GetGames(long contestId, List<string> messages);

        Task<GenericResponse> GetPlayers(long contestId, List<string> messages);

        Task<GenericResponse> SubmitLineUp(LineUpRequest request, List<string> messages);

        Task<string> SetPrizes(List<string> messages);

        Task<GenericResponse> PrizePool(long operatorId, List<string> messages);

        Task<bool> RankTeams(List<string> messages);

        Task<bool> BroadcastRanks(List<string> messages);

        Task<GenericResponse> TeamRanks(long operatorId, List<string> messages);

        Task<GenericResponse> TeamHistoryRanks(long operatorId, string gameDate, List<string> messages);

        Task<List<ContestPrizePool>> ComputePrizePool(long operatorId, long contestId,
            List<ContestPrizePool> contestPrizePool);

        Task<string> SetPrizesInTransaction(List<string> messages);

        Task<string> CreateContest(int offsetDays, List<string> messages);

        Task<GenericResponse> GetContest(long contestId, List<string> messages);

        Task<GenericResponse> GetPlayersForUpdate(long contestId, long playerTeamId, List<string> messages);
    }
}