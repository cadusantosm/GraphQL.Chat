using Marten;

namespace GraphQL.Chat.Application.Services.DocumentStore
{
    public class DocumentStoreResolver:IDocumentStoreResolver
    {
        public DocumentStoreResolver(IDocumentStore documentStore)
        {
            DocumentStore = documentStore;
        }

        public IDocumentStore DocumentStore { get; }

        private static void Configure(string connectionString, StoreOptions opts)
        {
            opts.Connection(connectionString);
            opts.AutoCreateSchemaObjects = AutoCreate.CreateOnly;
        }
    }
}