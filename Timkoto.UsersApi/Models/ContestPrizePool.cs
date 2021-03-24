namespace Timkoto.UsersApi.Models
{
    public class ContestPrizePool
    {
        public long Id { get; set; }

        public string DisplayRank { get; set; }

        public int FromRank { get; set; }

        public int ToRank { get; set; }

        public decimal Prize { get; set; }
    }
}
