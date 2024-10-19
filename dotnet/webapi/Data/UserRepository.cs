using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using WebApi.DTOs;
using WebApi.Entities;
using WebApi.Interfaces;

namespace WebApi.Data;

public class UserRepository(DataContext dataContext, IMapper mapper) : IUserRepository
{
    public async Task<IEnumerable<MemberDto>> GetAllMembersAsync()
    {
        return await dataContext.Users
            .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await dataContext.Users
            .Include(x => x.Photos)
            .ToListAsync();
    }

    public async Task<MemberDto?> GetMemberByUsernameAsync(string username)
    {
        return await dataContext.Users
            .Where(x => x.UserName == username)
            .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await dataContext.Users.FindAsync(id);
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await dataContext.Users
            .Include(x => x.Photos)
            .SingleOrDefaultAsync(x => x.UserName == username);
    }

    public async Task<bool> SaveAllAsync()
    {
        return await dataContext.SaveChangesAsync() > 0;
    }

    public void Update(User user)
    {
        dataContext.Entry(user).State = EntityState.Modified;
    }
}
