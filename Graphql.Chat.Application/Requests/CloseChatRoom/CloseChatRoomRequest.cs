using System;
using GraphQL.Chat.Application.Model;
using MediatR;

namespace GraphQL.Chat.Application.Requests.CloseChatRoom
{
    public class CloseChatRoomRequest:IRequest<ChatRoom?>
    {
        public Guid ChatRoomId { get; set; }
    }
}