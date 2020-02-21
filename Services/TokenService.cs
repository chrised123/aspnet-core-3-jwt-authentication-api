using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models;

namespace WebApi.Services
{
    public interface ITokenService
    {
        TokenModel Generate(User user);
        TokenModel Generate(string token, string refreshToken);
        bool ValidateToken(string oldToken);
        
    }
    public class TokenService : ITokenService
    {

        private readonly AppSettings _appSettings;

        public TokenService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public TokenModel Generate(User user) {

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] 
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddSeconds(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            TokenModel AccessToken = new TokenModel();
            AccessToken.Token = tokenHandler.WriteToken(token);
            var expiryDate = tokenDescriptor.Expires ?? DateTime.Now;
            AccessToken.Expires =  expiryDate.ToUniversalTime().Subtract(
                                        new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                        ).TotalMilliseconds.ToString();
            AccessToken.RefreshToken = GenerateRefreshToken();
            AccessToken.RefreshTokenExpires = DateTime.Now.AddMinutes(30).ToUniversalTime().Subtract(
                                        new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                        ).TotalMilliseconds.ToString();

            return AccessToken;
        }

        public TokenModel Generate(string token, string refreshToken) {
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;

            if(string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refreshToken)){
                throw new SecurityTokenException("Token and Refreshtoken is expected");
            }
            var principal = tokenHandler.ValidateToken(token, GetValidationParameters(), out securityToken);

            var userId = principal.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new SecurityTokenException($"Missing claim: {ClaimTypes.Name}!");
            } else{
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[] 
                    {
                        new Claim(ClaimTypes.Name, userId)
                    }),
                    Expires = DateTime.UtcNow.AddSeconds(30),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var newToken = tokenHandler.CreateToken(tokenDescriptor);
                TokenModel AccessToken = new TokenModel();
                AccessToken.Token = tokenHandler.WriteToken(newToken);
                var expiryDate = tokenDescriptor.Expires ?? DateTime.Now;
                AccessToken.Expires =  expiryDate.ToUniversalTime().Subtract(
                                            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                            ).TotalMilliseconds.ToString();
                AccessToken.RefreshToken = GenerateRefreshToken();
                AccessToken.RefreshTokenExpires = DateTime.Now.AddMinutes(30).ToUniversalTime().Subtract(
                                            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                            ).TotalMilliseconds.ToString();

                return AccessToken;
            }

            
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public bool ValidateToken(string oldToken) {

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetValidationParameters();
            SecurityToken validatedToken;
            IPrincipal principal = tokenHandler.ValidateToken(oldToken, validationParameters, out validatedToken);
            return true;
        }

        private TokenValidationParameters GetValidationParameters()
        {

            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            return new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = System.TimeSpan.Zero
            };
        }
    }
}