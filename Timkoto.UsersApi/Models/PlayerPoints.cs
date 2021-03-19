namespace Timkoto.UsersApi.Models
{
    public class PlayerPoints
    {
        public long PlayerId { get; set; }

        public long AgentId { get; set; }

        public long OperatorId { get; set; }

        public string UserName { get; set; }

        public string TeamName { get; set; }

        public int TeamRank { get; set; }

        public decimal Score { get; set; }

        public decimal AgentCommission { get; set; }

        public decimal Amount { get; set; }

        public decimal Prize { get; set; }
    }
}
