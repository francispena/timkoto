﻿namespace Timkoto.UsersApi.Models
{
    public class AddUserRequest
    {
        public string Email { get; set; }

        public string UserName { get; set; }

        public string PhoneNumber { get; set; }

        public string Password { get; set; }

        public string RegistrationCode{ get; set; }
    }
}
