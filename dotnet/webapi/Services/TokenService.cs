using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApi.Entities;
using WebApi.Interfaces;

namespace WebApi.Services;

public class TokenService(IConfiguration configuration, UserManager<User> userManager) : ITokenService
{
    public async Task<string> CreateToken(User user)
    {
        var tokenKey = configuration["TokenKey"] ?? throw new Exception("Cannot access token key from appsettings.");
        
        if(tokenKey.Length < 64)
        {
            throw new Exception("Your token key needs to be longer.");
        }

        if (user.UserName == null)
        {
            throw new Exception("No username for user");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var claims = new List<Claim>
        {
            new (ClaimTypes.NameIdentifier, user.Id.ToString()),
            new (ClaimTypes.Name, user.UserName)
        };

        var roles = await userManager.GetRolesAsync(user);

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
