using WebApi.Entities;

namespace WebApi.Interfaces;

public interface ITokenService
{
    Task<string> CreateToken(User user);
}
