namespace Timkoto.UsersApi.Models
{
    public class PlayerTeamHistory
    {
        public long Id { get; set; }

        public long ContestId { get; set; }

        public string GameDate { get; set; }

        public string TeamName { get; set; }

        public decimal  Score { get; set; }

        public int TeamRank { get; set; }

        public decimal Prize { get; set; }
        
    }
}
