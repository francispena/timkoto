namespace Timkoto.UsersApi.Models
{
    public class UpdateUserRequest
    {
        public long Id { get; set; }

        public string UserName { get; set; }

        public string PhoneNumber { get; set; }
    }
}
