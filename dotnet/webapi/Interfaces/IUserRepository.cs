using WebApi.DTOs;
using WebApi.Entities;

namespace WebApi.Interfaces;

public interface IUserRepository
{
    void Update(User user);
    Task<bool> SaveAllAsync();
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByUsernameAsync(string name);
    Task<IEnumerable<MemberDto>> GetAllMembersAsync();
    Task<MemberDto?> GetMemberByUsernameAsync(string username);
}
