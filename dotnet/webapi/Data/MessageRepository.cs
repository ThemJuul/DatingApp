using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using WebApi.DTOs;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Interfaces;

namespace WebApi.Data;

public class MessageRepository(DataContext dataContext, IMapper mapper) : IMessageRepository
{
    public void AddMessage(Message message)
    {
        dataContext.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
        dataContext.Messages.Remove(message);
    }

    public async Task<PagedList<MessageDto>> GetAllMessagesForUserAsync(MessageParams messageParams)
    {
        var query = dataContext.Messages
            .OrderByDescending(x => x.MessageSent)
            .AsQueryable();

        query = messageParams.Container switch
        {
            "Inbox" => query.Where(x => x.Recipient.UserName == messageParams.Username && x.RecipientDeleted == false),
            "Outbox" => query.Where(x => x.Sender.UserName == messageParams.Username && x.SenderDeleted == false),
            _ => query.Where(x => x.Recipient.UserName == messageParams.Username && x.DateRead == null && x.RecipientDeleted == false) //unread messages
        };

        var messages = query.ProjectTo<MessageDto>(mapper.ConfigurationProvider);

        return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
    }

    public async Task<Message?> GetMessageAsync(int id)
    {
        return await dataContext.Messages.FindAsync(id);
    }

    public async Task<IEnumerable<MessageDto>> GetMessageThreadAsync(string currentUsername, string recipientUsername)
    {
        var messages = await dataContext.Messages
            .Include(x => x.Sender)
                .ThenInclude(x => x.Photos)
            .Include(x => x.Recipient)
                .ThenInclude(x => x.Photos)
            .Where(x => x.RecipientUsername == currentUsername && x.RecipientDeleted == false && x.SenderUsername == recipientUsername ||
                    x.SenderUsername == currentUsername && x.SenderDeleted == false && x.RecipientUsername == recipientUsername)
            .OrderBy(x => x.MessageSent)
            .ToListAsync();

        var unreadMessages = messages.Where(x => x.DateRead == null && x.RecipientUsername == currentUsername).ToList();

        if (unreadMessages.Count != 0)
        {
            foreach (var message in unreadMessages)
            {
                message.DateRead = DateTime.UtcNow;
            }

            await dataContext.SaveChangesAsync();
        }

        return mapper.Map<IEnumerable<MessageDto>>(messages);
    }

    public async Task<bool> SaveAllAsync()
    {
        return await dataContext.SaveChangesAsync() > 0;
    }
}
