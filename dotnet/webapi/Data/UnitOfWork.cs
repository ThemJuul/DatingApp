using WebApi.Interfaces;

namespace WebApi.Data;

public class UnitOfWork(DataContext dataContext, IUserRepository userRepository, ILikesRepository likesRepository, IMessageRepository messageRepository, IPhotoRepository photosRepository) : IUnitOfWork
{
    public IUserRepository UserRepository => userRepository;

    public ILikesRepository LikesRepository => likesRepository;

    public IMessageRepository MessageRepository => messageRepository;

    public IPhotoRepository PhotosRepository => photosRepository;

    public async Task<bool> Complete()
    {
        return await dataContext.SaveChangesAsync() > 0;
    }

    public bool HasChanges()
    {
        return dataContext.ChangeTracker.HasChanges();
    }
}
