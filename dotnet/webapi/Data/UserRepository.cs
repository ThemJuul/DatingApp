using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using WebApi.DTOs;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Interfaces;

namespace WebApi.Data;

public class UserRepository(DataContext dataContext, IMapper mapper) : IUserRepository
{
    public async Task<PagedList<MemberDto>> GetAllMembersAsync(UserParams userParams)
    {
        var query = dataContext.Users.AsQueryable();

        query = query.Where(x => x.UserName != userParams.CurrentUsername);

        if(userParams.Gender != null)
        {
            query = query.Where(x => x.Gender == userParams.Gender);
        }

        var minimumDateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1));
        var maxDateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));

        query = query.Where(x => x.DateOfBirth >= minimumDateOfBirth && x.DateOfBirth <= maxDateOfBirth);

        query = userParams.OrderBy switch
        {
            "created" => query.OrderByDescending(x => x.Created),
            _ => query.OrderByDescending(x => x.LastActive)
        };

        return await PagedList<MemberDto>.CreateAsync(query.ProjectTo<MemberDto>(mapper.ConfigurationProvider),
            userParams.PageNumber, userParams.PageSize);
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
            .Where(x => x.NormalizedUserName == username.ToUpper())
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
