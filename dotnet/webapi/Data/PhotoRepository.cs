using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using WebApi.DTOs;
using WebApi.Entities;
using WebApi.Interfaces;

namespace WebApi.Data;

public class PhotoRepository(DataContext dataContext, IMapper mapper) : IPhotoRepository
{
    public async Task<List<PhotoForModerationDto>> GetPhotosForModeration()
    {
        return await dataContext.Photos
            .IgnoreQueryFilters()
            .Where(x => !x.IsApproved)
            .ProjectTo<PhotoForModerationDto>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<Photo?> GetPhotoForApprovalByIdAsync(int photoId)
    {
        return await dataContext.Photos
            .IgnoreQueryFilters()
            .Where(x => x.Id == photoId)
            .Include(x => x.User)
                .ThenInclude(x => x.Photos)
            .SingleOrDefaultAsync();
    }

    public async Task<Photo?> GetPhotoById(int photoId)
    {
        return await dataContext.Photos.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == photoId);
    }

    public void RemovePhoto(Photo photoToDelete)
    {
        dataContext.Photos.Remove(photoToDelete);
    }

    public async Task<Photo?> GetUserMainPhoto(string username)
    {
        return await dataContext.Photos.Where(x => x.User.NormalizedUserName == username.ToUpper() && x.IsMain).SingleOrDefaultAsync();
    }
}
