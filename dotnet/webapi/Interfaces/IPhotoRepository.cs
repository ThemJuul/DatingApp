using WebApi.DTOs;
using WebApi.Entities;

namespace WebApi.Interfaces;

public interface IPhotoRepository
{
    Task<List<PhotoForModerationDto>> GetPhotosForModeration();
    Task<Photo?> GetPhotoForApprovalByIdAsync(int photoId);
    void RemovePhoto(Photo photoToDelete);
    Task<Photo?> GetPhotoById(int photoId);
    Task<Photo?> GetUserMainPhoto(string username);
}
