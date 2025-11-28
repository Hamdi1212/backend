using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Checklist.Models; // make sure this matches your User model namespace

namespace Checklist.Helpers
{
    public class JwtHelper
    {
        private readonly IConfiguration _config;

        public JwtHelper(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateJwtToken(User user)
        {
            // Get JWT settings from appsettings.json
            var jwtKey = _config["Jwt:Key"];
            var jwtIssuer = _config["Jwt:Issuer"];
            var jwtAudience = _config["Jwt:Audience"];

            // Ensure key is at least 32 characters (256 bits)
            if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
            {
                throw new ArgumentException("JWT key must be at least 32 characters long.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create claims (info stored inside token)
            var claims = new[]
            {
    new Claim(JwtRegisteredClaimNames.Sub, user.Username),
    new Claim("userId", user.Id.ToString()), // ← ADD THIS LINE - User's GUID
    new Claim(ClaimTypes.Role, user.Role ?? ""), //  Correct claim type for role
    new Claim("fullname", user.FullName ?? ""),
    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
};


            // Create the token
            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(3), // token valid for 3 hours
                signingCredentials: creds
            );

            // Return the JWT as string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
