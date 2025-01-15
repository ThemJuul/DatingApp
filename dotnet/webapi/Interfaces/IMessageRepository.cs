using WebApi.DTOs;
using WebApi.Entities;
using WebApi.Helpers;

namespace WebApi.Interfaces;

public interface IMessageRepository
{
    void AddMessage(Message message);
    void DeleteMessage(Message message);
    Task<Message?> GetMessageAsync(int id);
    Task<PagedList<MessageDto>> GetAllMessagesForUserAsync(MessageParams messageParams);
    Task<IEnumerable<MessageDto>> GetMessageThreadAsync(string currentUsername, string recipientUsername);
    void AddGroup(Group group);
    void RemoveConnection(Connection connection);
    Task<Connection?> GetConnectionAsync(string connectionId);
    Task<Group?> GetMessageGroupAsync(string groupName);
    Task<Group?> GetGroupForConnection(string connectionId);
}
