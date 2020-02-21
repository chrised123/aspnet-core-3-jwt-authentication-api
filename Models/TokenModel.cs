using System;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
    public class TokenModel
    {
        public string Token { get; set; }

        public string Expires { get; set; }

        public string RefreshToken { get; set; }

        public string RefreshTokenExpires { get; set; }
    }
}