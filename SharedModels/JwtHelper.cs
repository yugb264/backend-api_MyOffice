using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

public static class JwtHelper
{
    public static string GenerateToken(object payload)
    {
        var secret = Environment.GetEnvironmentVariable("JWT_SIGNING_SECRET")
            ?? throw new InvalidOperationException("JWT_SIGNING_SECRET environment variable is not configured.");

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secret)
        );

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256
        );

        var handler = new JwtSecurityTokenHandler();

        var token = handler.CreateJwtSecurityToken(
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        var tokenString = handler.WriteToken(token);

        return tokenString;
    }
}
