using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.Interfaces;

namespace WebApi.Controllers;

//[Authorize]
public class UsersController(IUserRepository userRepository, IMapper mapper) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
    {
        var users = await userRepository.GetAllMembersAsync();

        return Ok(users);
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        var user = await userRepository.GetMemberByUsernameAsync(username);

        if (user == null)
        {
            return NotFound();
        }

        return user;
    }
}
