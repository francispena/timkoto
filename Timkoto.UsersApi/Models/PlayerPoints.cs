namespace Timkoto.UsersApi.Models
{
    public class PlayerPoints
    {
        public long ContestId { get; set; }

        public long OperatorId { get; set; }

        public long AgentId { get; set; }

        public string Email { get; set; }

        public string UserName { get; set; }
        
        public decimal Amount { get; set; }

        public decimal AgentCommission { get; set; }

        public decimal Prize { get; set; }
    }
}
