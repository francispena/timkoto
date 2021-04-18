namespace Timkoto.UsersApi.Models
{
    public class OfficialPlayerStats
    {
        public string PlayerId { get; set; }

        public int Points { get; set; }

        public int ReboundsTotal { get; set; }

        public int Steals { get; set; }

        public int Assists { get; set; }

        public int Blocks { get; set; }

        public int TurnOvers { get; set; }

        public string TeamId { get; set; }
    }
}
