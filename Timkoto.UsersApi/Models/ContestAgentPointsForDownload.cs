namespace Timkoto.UsersApi.Models
{
    public class ContestAgentPointsForDownload
    {
        public string GameDate { get; set; }

        public string OperatorName { get; set; }

        public string AgentName { get; set; }

        public decimal Collection { get; set; }

        public decimal Remit { get; set; }

        public decimal Commission { get; set; }

        public decimal Prize { get; set; }
    }
}
