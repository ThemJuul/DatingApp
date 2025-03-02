using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.DTOs;
using WebApi.Entities;
using WebApi.Interfaces;

namespace WebApi.Controllers;

public class AdminController(UserManager<User> userManager, IUnitOfWork unitOfWork, IPhotoService photoService) : BaseApiController
{
    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUsersWithRoles()
    {
        var users = await userManager.Users
            .OrderBy(x => x.UserName)
            .Select(x => new
            {
                x.Id,
                Username = x.UserName,
                Roles = x.UserRoles.Select(r => r.Role.Name).ToList()
            })
            .ToListAsync();

        return Ok(users);
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("edit-roles/{username}")]
    public async Task<ActionResult> EditRoles(string username, string roles)
    {
        if (string.IsNullOrEmpty(roles))
        {
            return BadRequest("You must select at least one role.");
        }

        var selectedRoles = roles.Split(",").ToArray();
        var user = await userManager.FindByNameAsync(username);

        if (user == null)
        {
            return BadRequest("User not found.");
        }

        var userRoles = await userManager.GetRolesAsync(user);

        var result = await userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

        if (!result.Succeeded)
        {
            return BadRequest("Failed to add to roles.");
        }

        result = await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

        if (!result.Succeeded)
        {
            return BadRequest("Failed to remove from roles.");
        }

        return Ok(await userManager.GetRolesAsync(user));
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
    public async Task<ActionResult<IEnumerable<PhotoForModerationDto>>> GetPhotosForModeration()
    {
        return await unitOfWork.PhotosRepository.GetPhotosForModeration();
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("approve-photo/{photoId:int}")]
    public async Task<ActionResult> ApprovePhoto(int photoId)
    {
        var photoToApprove = await unitOfWork.PhotosRepository.GetPhotoForApprovalByIdAsync(photoId);

        if (photoToApprove == null)
        {
            return BadRequest("Failed to approve photo. No photo found.");
        }

        var setPhotoAsMain = !photoToApprove.User.Photos.Any(x => x.IsMain);

        photoToApprove.IsApproved = true;
        photoToApprove.IsMain = setPhotoAsMain;

        if (await unitOfWork.Complete())
        {
            return NoContent();
        }

        return BadRequest("Failed to approve photo.");
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("reject-photo/{photoId:int}")]
    public async Task<ActionResult> RejectPhoto(int photoId)
    {
        var photoToDelete = await unitOfWork.PhotosRepository.GetPhotoById(photoId);

        if (photoToDelete == null)
        {
            return BadRequest("Failed to delete photo. Could not find it.");
        }

        if (photoToDelete.PublicId != null)
        {
            await photoService.DeletePhotoAsync(photoToDelete.PublicId);
        }

        unitOfWork.PhotosRepository.RemovePhoto(photoToDelete);

        if (await unitOfWork.Complete())
        {
            return NoContent();
        }

        return BadRequest("Failed to delete photo.");
    }
}
