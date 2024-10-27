using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using WebApi.Data;
using WebApi.Entities;
using WebApi.DTOs;
using WebApi.Interfaces;
using WebApi.Extensions;
using AutoMapper;

namespace WebApi.Controllers;

public class AccountController(DataContext dataContext, ITokenService tokenService, IUserRepository userRepository, IMapper mapper) : BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> RegisterUser(RegisterUserDto registerUserDto)
    {
        if(await UserExists(registerUserDto.Username))
        {
            return BadRequest("Username is taken.");
        }

        using var hmac = new HMACSHA512();

        var user = mapper.Map<User>(registerUserDto);

        user.UserName = registerUserDto.Username.ToLower();
        user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerUserDto.Password));
        user.PasswordSalt = hmac.Key;

        //var user = new User
        //{
        //    UserName = registerUserDto.Username.ToLower(),
        //    PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerUserDto.Password)),
        //    PasswordSalt = hmac.Key
        //};

        dataContext.Users.Add(user);
        await dataContext.SaveChangesAsync();

        return new UserDto
        {
            Username = user.UserName,
            Token = tokenService.CreateToken(user),
            KnownAs = user.KnownAs
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await userRepository.GetUserByUsernameAsync(loginDto.Username.ToLower());

        if(user is null)
        {
            return Unauthorized("Invalid username! or password.");
        }

        using var hmac = new HMACSHA512(user.PasswordSalt);

        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i])
            {
                return Unauthorized("Invalid username or password!.");
            }
        }

        return new UserDto
        {
            Username = user.UserName,
            KnownAs = user.KnownAs,
            Token = tokenService.CreateToken(user),
            PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url
        };
    }

    private async Task<bool> UserExists(string username)
    {
        return await dataContext.Users.AnyAsync(user => user.UserName.ToLower() == username.ToLower());
    }
}
