﻿namespace WebApi.Interfaces;

public interface IUnitOfWork
{
    IUserRepository UserRepository { get; }
    ILikesRepository LikesRepository { get; }
    IMessageRepository MessageRepository { get; }
    IPhotoRepository PhotosRepository { get; }
    Task<bool> Complete();
    bool HasChanges();
}
