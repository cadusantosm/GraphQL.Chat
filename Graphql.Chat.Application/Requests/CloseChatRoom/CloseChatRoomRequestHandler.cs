using System;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics;
using GraphQL.Chat.Application.Model;
using GraphQL.Chat.Application.Services.DocumentStore;
using Marten;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GraphQL.Chat.Application.Requests.CloseChatRoom
{
    public class CloseChatRoomRequestHandler : IRequestHandler<CloseChatRoomRequest, ChatRoom?>
    {
        private readonly IDocumentStoreResolver _documentStoreResolver;
        private readonly ILogger<CloseChatRoomRequest> _logger;
        private readonly IMetrics _metrics;

        public CloseChatRoomRequestHandler(IDocumentStoreResolver documentStoreResolver, IMetrics metrics)
        {
            _documentStoreResolver = documentStoreResolver;
            _metrics = metrics;
        }

        public async Task<ChatRoom?> Handle(CloseChatRoomRequest request, CancellationToken cancellationToken)
        {
            try
            {
                using var session = _documentStoreResolver.DocumentStore.LightweightSession();

                var chatRoom = await session
                    .Query<ChatRoom>()
                    .FirstOrDefaultAsync(r => r.ChatRoomId == request.ChatRoomId, token: cancellationToken);

                if (chatRoom == null || chatRoom.ClosingTimestamp != null)
                {
                    return chatRoom;
                }

                chatRoom.ClosingTimestamp = DateTimeOffset.UtcNow;

                session.Update(chatRoom);
                await session.SaveChangesAsync(cancellationToken);

                _metrics.Measure.Meter.Mark(MetricsOptions.ROOM_CLOSE);

                return chatRoom;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error to close chat room:{ChatRoomId}", request.ChatRoomId);
                throw;
            }
        }
    }
}