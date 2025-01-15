using WebApi.DTOs;
using WebApi.Entities;
using WebApi.Helpers;

namespace WebApi.Interfaces;

public interface IUserRepository
{
    void Update(User user);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByUsernameAsync(string name);
    Task<PagedList<MemberDto>> GetAllMembersAsync(UserParams userParams);
    Task<MemberDto?> GetMemberByUsernameAsync(string username);
}
