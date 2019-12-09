using EMS.Shared.Model;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EMS.Api.Factory
{
    public static class TokenFactory
    {
        private readonly static JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        public static string Create(TokenConfigOption option, string name, IEnumerable<string> userRoles, IEnumerable<Claim> userClaims, int expireHours = 1)
        {
            var signingCredential = GetSigningCreadential(option.SecretKey);
            var claims = GetClaims(name, userRoles, userClaims);
            var expireOn = DateTime.Now.AddHours(expireHours);
            var token = GenerateToken(option.Issuer, option.Audience, claims, signingCredential, expireOn);
            return token;
        }

        #region Utility Methods
        private static SigningCredentials GetSigningCreadential(string secret)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var signingCredential = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            return signingCredential;
        }

        private static List<Claim> GetClaims(string name, IEnumerable<string> userRoles, IEnumerable<Claim> userClaims)
        {
            var claims = new List<Claim>(userClaims);

            claims.Add(new Claim(ClaimTypes.Name, name));
            claims.Add(new Claim(ClaimTypes.Email, name));

            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }

        private static string GenerateToken(string issuer, string audience, IEnumerable<Claim> claims, SigningCredentials signingCredential, DateTime expireOn)
        {
            var securityToken = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expireOn,
                signingCredentials: signingCredential
            );
            return tokenHandler.WriteToken(securityToken); ;
        }
        #endregion

    }
}
