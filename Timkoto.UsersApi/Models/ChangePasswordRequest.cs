namespace Timkoto.UsersApi.Models
{
    public class ChangePasswordRequest
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public string Code{ get; set; }
    }
}
