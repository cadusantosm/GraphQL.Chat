using Marten;

namespace GraphQL.Chat.Application.Services.DocumentStore
{
    public interface IDocumentStoreResolver
    {
        IDocumentStore DocumentStore { get; }
    }
}