using System;

namespace EventPro.API.Models
{
    public class UserToken
    {
        public string UserName { get; set; }
        public int UserId { get; set; }
        public string Role { get; set; }
        public string Issuer { get; set; }
        public DateTime ValidTo { get; set; }
    }
}
