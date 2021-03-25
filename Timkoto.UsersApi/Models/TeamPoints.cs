namespace Timkoto.UsersApi.Models
{
    public class TeamPoints
    {
        public long OperatorId { get; set; }

        public long UserId { get; set; }

        public long AgentId { get; set; }

        public long PlayerTeamId { get; set; }

        public decimal TotalPoints { get; set; }

        public int TeamRank { get; set; }

        public decimal Prize { get; set; }

        public decimal Points { get; set; }
    }
}
