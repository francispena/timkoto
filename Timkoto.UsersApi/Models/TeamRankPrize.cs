namespace Timkoto.UsersApi.Models
{
    public class TeamRankPrize
    {
        public string UserName { get; set; }

        public string TeamName { get; set; }

        public decimal Score { get; set; }

        public int TeamRank { get; set; }

        public decimal Prize { get; set; }
    }
}
