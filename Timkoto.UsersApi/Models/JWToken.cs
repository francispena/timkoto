using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Timkoto.UsersApi.Models
{
    public class JWToken
    {
        public string IdToken { get; set; }
        
        public string AccessToken { get; set; }
        
        public string RefreshToken { get; set; }
    }
}
