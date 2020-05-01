using System.Linq;
using System.Reflection;
using HotChocolate;
using HotChocolate.Types;

namespace Graphql.Chat.Api.Util
{
    public static class SchemaBuilderExtensions
    {
        public static ISchemaBuilder AddAllLocalTypes(this ISchemaBuilder builder)
        {
            var types = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(x => x.BaseType?.IsGenericType == true &&
                            (x.BaseType.GetGenericTypeDefinition() == typeof(ObjectType<>) ||
                             x.BaseType.GetGenericTypeDefinition() == typeof(InterfaceType<>) ||
                             x.BaseType.GetGenericTypeDefinition() == typeof(UnionType<>) ||
                             x.BaseType.GetGenericTypeDefinition() == typeof(InputObjectType<>) ||
                             x.BaseType.GetGenericTypeDefinition() == typeof(EnumType<>)));

            foreach (var type in types)
            {
                builder.AddType(type);
            }

            return builder;
        }
    }
}