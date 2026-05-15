using System.IdentityModel.Tokens.Jwt;

public class TokenService
{
    public (string name, string email, string oid) GetUser(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            return ("Unknown User", "unknown", "unknown");

        var token = authHeader.Substring("Bearer ".Length).Trim();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var name = jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
        var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;
        var oid = jwtToken.Claims.FirstOrDefault(c => c.Type == "oid")?.Value;

        return (name ?? "Unknown User", email ?? "unknown", oid ?? "unknown");
    }
}