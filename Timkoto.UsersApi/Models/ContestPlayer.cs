namespace Timkoto.UsersApi.Models
{
    public class ContestPlayer
    {
        public string PlayerId { get; set; }

        public string PlayerName { get; set; }

        public string Jersey { get; set; }

        public string Team { get; set; }

        public string Position { get; set; }

        public decimal Salary { get; set; }

        public decimal Fppg { get; set; }

        public virtual bool Selected { get; set; }
    }
}
