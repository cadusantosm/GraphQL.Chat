using System;

namespace GraphQL.Chat.Application.Model
{
    public class ChatRoom
    {
        public Guid ChatRoomId { get; set; }
        public Guid UserId { get; set; }
        public DateTimeOffset OpeningTimestamp { get; set; }
        public DateTimeOffset? ClosingTimestamp { get; set; }
    }
}