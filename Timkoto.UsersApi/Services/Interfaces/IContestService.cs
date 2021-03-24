using System.Collections.Generic;
using System.Threading.Tasks;
using Timkoto.UsersApi.Models;

namespace Timkoto.UsersApi.Services.Interfaces
{
    public interface IContestService
    {
        Task<GenericResponse> GetGames(long contestId, List<string> messages);

        Task<GenericResponse> GetPlayers(long contestId, List<string> messages);

        Task<GenericResponse> SubmitLineUp(LineUpRequest request, List<string> messages);

        Task<bool> RankAndSetPrizes(List<string> messages);

        Task<GenericResponse> PrizePool(long operatorId, List<string> messages);

        Task<bool> RankTeams(List<string> messages);

        Task<bool> BroadcastRanks(List<string> messages);

        Task<GenericResponse> TeamRanks(long operatorId, List<string> messages);
    }
}