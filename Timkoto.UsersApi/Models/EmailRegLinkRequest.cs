namespace Timkoto.UsersApi.Models
{
    public class EmailRegLinkRequest
    {
        public string EmailAddress { get; set; }

        public string Link { get; set; }

        public long UserId { get; set; }

    }
}
