﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.Entities;
using WebApi.Extensions;
using WebApi.Helpers;
using WebApi.Interfaces;

namespace WebApi.Controllers;

[Authorize]
public class UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
    {
        userParams.CurrentUsername = User.GetUsername();
        var users = await unitOfWork.UserRepository.GetAllMembersAsync(userParams);

        Response.AddPaginationHeader(users);

        return Ok(users);
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        var currentUser = User.GetUsername();

        var user = await unitOfWork.UserRepository.GetMemberByUsernameAsync(username, currentUser.ToLower() == username.ToLower());

        if (user == null)
        {
            return NotFound();
        }

        return user;
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null)
        {
            return BadRequest("Could not find user.");
        }

        mapper.Map(memberUpdateDto, user);

        if (await unitOfWork.Complete())
        {
            return NoContent();
        }

        return BadRequest("Failed to update the user.");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null)
        {
            return BadRequest("Cannot update user.");
        }

        var result = await photoService.AddPhotoAsync(file);

        if (result.Error != null)
        {
            return BadRequest(result.Error.Message);
        }

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };

        user.Photos.Add(photo);

        if (await unitOfWork.Complete())
        {
            return CreatedAtAction(nameof(GetUser),
                new { username = user.UserName }, mapper.Map<PhotoDto>(photo));
        }

        return BadRequest("Problem adding photo.");
    }

    [HttpPut("set-main-photo/{photoId:int}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null)
        {
            return BadRequest("Could not find user.");
        }

        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

        if (photo == null || photo.IsMain || !photo.IsApproved)
        {
            return BadRequest("Cannot use this as main photo.");
        }

        var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);

        if (currentMain != null)
        {
            currentMain.IsMain = false;
        }

        photo.IsMain = true;

        if (await unitOfWork.Complete())
        {
            return NoContent();
        }

        return BadRequest("Problem setting main photo");
    }

    [HttpDelete("delete-photo/{photoId:int}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null)
        {
            return BadRequest("Could not find user.");
        }

        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

        if (photo == null || photo.IsMain)
        {
            return BadRequest("This photo cannot be deleted.");
        }

        //this is due to the seeding of users with random images not on Cloudinary
        if (photo.PublicId != null)
        {
            var result = await photoService.DeletePhotoAsync(photo.PublicId);

            if (result.Error != null)
            {
                return BadRequest(result.Error.Message);
            }
        }

        user.Photos.Remove(photo);

        if (await unitOfWork.Complete())
        {
            return Ok();
        }

        return BadRequest("Problem deleting photo.");
    }
}
