using System;
using GraphQL.Chat.Application.Model;
using MediatR;

namespace GraphQL.Chat.Application.Requests.OpenChatRoom
{
    public class OpenChatRoomRequest:IRequest<ChatRoom>
    {
        public Guid UserId { get; set; }
    }
}