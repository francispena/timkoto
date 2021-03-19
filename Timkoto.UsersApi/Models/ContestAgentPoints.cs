namespace Timkoto.UsersApi.Models
{
    public class ContestAgentPoints
    {
        public long ContestId { get; set; }

        public long OperatorId { get; set; }

        public string Email { get; set; }

        public string UserName { get; set; }

        public long AgentId { get; set; }
        
        public decimal Collectible { get; set; }

        public decimal Commission { get; set; }

        public decimal Prize { get; set; }
    }
}
