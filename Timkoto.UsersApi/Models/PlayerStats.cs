namespace Timkoto.UsersApi.Models
{
    public class PlayerStats
    {
        public string PlayerName { get; set; }

        public string TeamName { get; set; }

        public decimal Points { get; set; }

        public decimal Rebounds { get; set; }

        public decimal Assists { get; set; }

        public decimal Steals { get; set; }

        public decimal Blocks { get; set; }

        public decimal TurnOvers { get; set; }

        public decimal TotalPoints { get; set; }
    }
}
