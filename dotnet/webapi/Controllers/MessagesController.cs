using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.Entities;
using WebApi.Extensions;
using WebApi.Helpers;
using WebApi.Interfaces;

namespace WebApi.Controllers;

[Authorize]
public class MessagesController(IMessageRepository messageRepository, IUserRepository userRepository, IMapper mapper) : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        var username = User.GetUsername();

        if (username == createMessageDto.RecipientUsername.ToLower())
        {
            return BadRequest("You cannot message yourself.");
        }

        var sender = await userRepository.GetUserByUsernameAsync(username);
        var recipient = await userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

        if (recipient == null || sender == null)
        {
            return BadRequest("Cannot send message at this time.");
        }

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content
        };

        messageRepository.AddMessage(message);

        if (await messageRepository.SaveAllAsync())
        {
            return Ok(mapper.Map<MessageDto>(message));
        }

        return BadRequest("Failed to save message.");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery]MessageParams messageParams)
    {
        messageParams.Username = User.GetUsername();

        var messages = await messageRepository.GetAllMessagesForUserAsync(messageParams);

        Response.AddPaginationHeader(messages);

        return Ok(messages);
    }

    [HttpGet("thread/{username}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
    {
        var currentUsername = User.GetUsername();

        return Ok(await messageRepository.GetMessageThreadAsync(currentUsername, username));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        var username = User.GetUsername();

        var message = await messageRepository.GetMessageAsync(id);

        if (message == null)
        {
            return BadRequest("Cannot delete this message.");
        }

        bool userIsMessageSender = message.SenderUsername == username;
        bool userIsMessageRecipient = message.RecipientUsername == username;

        if (userIsMessageSender == false && userIsMessageRecipient == false)
        {
            return Forbid();
        }

        if (userIsMessageSender)
        {
            message.SenderDeleted = true;
        }

        if (userIsMessageRecipient)
        {
            message.RecipientDeleted = true;
        }

        if (message is {SenderDeleted: true, RecipientDeleted: true})
        {
            messageRepository.DeleteMessage(message);
        }

        if (await messageRepository.SaveAllAsync())
        {
            return Ok();
        }

        return BadRequest("Problem deleting the message.");
    }
}
