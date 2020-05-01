using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics;
using GraphQL.Chat.Application.Model;
using GraphQL.Chat.Application.Requests.CloseChatRoom;
using GraphQL.Chat.Application.Services.DocumentStore;
using Marten;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GraphQL.Chat.Application.Requests.OpenChatRoom
{
    public class OpenChatRoomRequestHandler : IRequestHandler<OpenChatRoomRequest, ChatRoom>
    {
        private readonly IDocumentStoreResolver _documentStoreResolver;
        private readonly ILogger<OpenChatRoomRequest> _logger;
        private readonly IMediator _mediator;
        private readonly IMetrics _metrics;

        public OpenChatRoomRequestHandler(IDocumentStoreResolver documentStoreResolver,
            ILogger<OpenChatRoomRequest> logger, IMediator mediator, IMetrics metrics)
        {
            _documentStoreResolver = documentStoreResolver;
            _logger = logger;
            _mediator = mediator;
            _metrics = metrics;
        }

        public async Task<ChatRoom> Handle(OpenChatRoomRequest request, CancellationToken cancellationToken)
        {
            try
            {
                using var session = _documentStoreResolver.DocumentStore.LightweightSession();

                var existingChatRooms = await session.Query<ChatRoom>()
                    .Where(r => r.ClosingTimestamp == null &&
                                (r.UserId == request.UserId))
                    .ToListAsync(token: cancellationToken);

                var roomClosureTasks = existingChatRooms.Select(r =>
                    _mediator.Send(new CloseChatRoomRequest {ChatRoomId = r.ChatRoomId}, cancellationToken));

                await Task.WhenAll(roomClosureTasks);

                var chatRoom = new ChatRoom
                {
                    ChatRoomId = Guid.NewGuid(),
                    UserId = request.UserId,
                    OpeningTimestamp = DateTimeOffset.Now
                };

                session.Store(chatRoom);
                await session.SaveChangesAsync(cancellationToken);

                _metrics.Measure.Meter.Mark(MetricsOptions.ROOM_OPEN);

                return chatRoom;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error to open chat room to user: {UserId}", request.UserId);
                throw;
            }
        }
    }
}