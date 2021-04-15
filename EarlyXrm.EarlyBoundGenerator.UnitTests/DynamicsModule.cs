using Microsoft.Xrm.Sdk;
using ModelBuilder;
using ModelBuilder.TypeCreators;
using System.Collections.Generic;
using System.Reflection;

namespace EarlyXrm.EarlyBoundGenerator.UnitTests
{
    public class DynamicsModule : IConfigurationModule
    {
        public void Configure(IBuildConfiguration configuration)
        {
            configuration

                .UpdateTypeCreator<EnumerableTypeCreator>(x => { x.MinCount = 1; x.MaxCount = 5; })

                .AddIgnoreRule(x => x.PropertyType == typeof(EntityReference))
                //.AddIgnoreRule(x => x.PropertyType == typeof(IEnumerable<>))
                .AddIgnoreRule(x => x.GetCustomAttribute<AttributeLogicalNameAttribute>()?.LogicalName == "statecode")
                .AddIgnoreRule(x => x.GetCustomAttribute<AttributeLogicalNameAttribute>()?.LogicalName == "statuscode")

                //.AddIgnoreRule(x => x.Name == nameof(Entity.LogicalName))
                .AddIgnoreRule<Entity>(x => x.LogicalName)
                .AddIgnoreRule(x => x.Name == nameof(Entity.EntityState))
                .AddIgnoreRule(x => x.Name == nameof(Entity.KeyAttributes))
                .AddIgnoreRule(x => x.Name == nameof(Entity.LazyFileAttributeKey))
                .AddIgnoreRule(x => x.Name == nameof(Entity.LazyFileAttributeValue))
                .AddIgnoreRule(x => x.Name == nameof(Entity.LazyFileSizeAttributeKey))
                .AddIgnoreRule(x => x.Name == nameof(Entity.LazyFileSizeAttributeValue))
                .AddIgnoreRule(x => x.Name == nameof(Entity.RowVersion))
                .AddIgnoreRule(x => x.Name == nameof(Entity.ExtensionData))
                //.AddIgnoreRule(x => x.Name == nameof(Entity.Attributes))
                .AddIgnoreRule<Entity>(x => x.Attributes)
                .AddIgnoreRule(x => x.Name == nameof(Entity.LazyFileSizeAttributeValue));
        }
    }
}
