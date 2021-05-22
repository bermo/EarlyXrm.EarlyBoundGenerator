using Microsoft.Crm.Services.Utility;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace EarlyXrm.EarlyBoundGenerator
{
    public class CodeNamingService : INamingService
    {
        private INamingService DefaultNamingService { get; set; }
        private readonly bool useDisplayNames;

        public CodeNamingService(INamingService namingService, IDictionary<string, string> parameters)
        {
            this.Debug();
            bool.TryParse(parameters["UseDisplayNames"], out useDisplayNames);
            DefaultNamingService = namingService;
        }

        string reservedEntities = "Entity";

        private string EntityNamingLogic(EntityMetadata entityMetadata)
        {
            return entityMetadata.DisplayName();
        }

        public string GetNameForEntity(EntityMetadata entityMetadata, IServiceProvider services)
        {
            var entityName = DefaultNamingService.GetNameForEntity(entityMetadata, services);

            if (useDisplayNames)
            {
                var metaData = services.LoadMetadata();

                entityName = EntityNamingLogic(entityMetadata);

                var reservedDups = entityName == reservedEntities ? 1 : 0;
                var dups = metaData.Entities.Where(x => x.DisplayName() == entityName);

                entityName = MakeUnique(entityName, dups, x => x.LogicalName, entityMetadata.LogicalName, reservedDups);
            }

            this.Debug(entityName, entityMetadata.LogicalName);

            return entityName;
        }

        private string[] reservedAttributes = new[] { 
            nameof(EarlyBoundTypes.Solution.EntityLogicalName), nameof(EarlyBoundTypes.Solution.EntityLogicalCollectionName), 
            nameof(EarlyBoundTypes.Solution.EntitySetName), nameof(EarlyBoundTypes.Solution.LogicalName), 
            nameof(Entity.KeyAttributes), nameof(Entity.Attributes), nameof(Entity.EntityState), 
            nameof(Entity.ExtensionData), nameof(Entity.FormattedValues), nameof(Entity.RelatedEntities), 
            nameof(Entity.RowVersion), nameof(Entity.LazyFileAttributeKey), nameof(Entity.LazyFileAttributeValue), 
            nameof(Entity.LazyFileSizeAttributeKey), nameof(Entity.LazyFileSizeAttributeValue),
        };

        private string AttributeNamingLogic(EntityMetadata entityMetadata, AttributeMetadata attributeMetadata)
        {
            return attributeMetadata.DisplayName();
        }

        public string GetNameForAttribute(EntityMetadata entityMetadata, AttributeMetadata attributeMetadata, IServiceProvider services)
        {
            var attributeName = DefaultNamingService.GetNameForAttribute(entityMetadata, attributeMetadata, services);

            if (useDisplayNames)
            {
                attributeName = AttributeNamingLogic(entityMetadata, attributeMetadata);

                var entDups = entityMetadata.DisplayName() == attributeName ? 1 : 0;
                var reservedDups = reservedAttributes.Any(x => x == attributeName) ? 1 : 0;

                var attDups = entityMetadata.Attributes.Where(x => x.DisplayName() == attributeName);

                attributeName = MakeUnique(attributeName, attDups, x => x.LogicalName, attributeMetadata.LogicalName, entDups, reservedDups);
            }

            this.Debug(attributeName, entityMetadata.LogicalName, attributeMetadata.LogicalName);

            return EnsureValidIdentifier(attributeName);
        }

        public string GetNameForRelationship(EntityMetadata entityMetadata, RelationshipMetadataBase relationshipMetadata, EntityRole? reflexiveRole, IServiceProvider services)
        {
            var tmr = new Stopwatch();tmr.Start();

            var metaData = services.LoadMetadata();

            var returnValue = DefaultNamingService.GetNameForRelationship(entityMetadata, relationshipMetadata, reflexiveRole, services);

            if (useDisplayNames)
            {
                var filteringService = (ICodeWriterFilterService)services.GetService(typeof(ICodeWriterFilterService));

                Func<OneToManyRelationshipMetadata, string> one2ManyNamingLogic = o2m =>
                {
                    var other = metaData.Entities.FirstOrDefault(x => x.LogicalName == o2m.ReferencingEntity);
                    var attribute = other.Attributes?.FirstOrDefault(x => x.LogicalName == o2m.ReferencingAttribute);

                    if (string.IsNullOrWhiteSpace(attribute.DisplayName()))
                        attribute = other.Attributes.FirstOrDefault(x => x.AttributeOf == o2m.ReferencingAttribute && o2m.ReferencingAttribute.Equals(x.SchemaName + "name", StringComparison.OrdinalIgnoreCase));

                    if (attribute == null)
                        return other.DisplayCollectionName?.DisplayName() ?? other.SchemaName;

                    return (attribute.DisplayName() ?? attribute.SchemaName) + "_" + (other.DisplayCollectionName?.DisplayName() ?? other.SchemaName);
                };

                Func<OneToManyRelationshipMetadata, string> many2OneNamingLogic = (m2o) =>
                {
                    var attribute = entityMetadata.Attributes.FirstOrDefault(x => x.LogicalName == m2o.ReferencingAttribute);
                    var name = attribute.DisplayName();
                    var other = metaData.Entities.FirstOrDefault(x => x.LogicalName == m2o.ReferencedEntity);
                    name += "_" + other.DisplayName();
                    return name;
                };

                EntityMetadata otherMeta;
                if (relationshipMetadata.RelationshipType == RelationshipType.OneToManyRelationship)
                {
                    var one2many = relationshipMetadata as OneToManyRelationshipMetadata;

                    if (entityMetadata.LogicalName == one2many.ReferencingEntity && reflexiveRole != EntityRole.Referenced) // many to one
                    {
                        var name = many2OneNamingLogic(one2many);

                        var entDups = entityMetadata.DisplayName() == name ? 1 : 0;
                        var attDups = entityMetadata.Attributes
                                        .Where(x => filteringService.GenerateAttribute(x, services))
                                        .Count(x => x.DisplayName() == name);
                        var manyDups = entityMetadata.ManyToOneRelationships
                                        .Where(x => filteringService.GenerateRelationship(x, metaData.Entities.FirstOrDefault(y => y.LogicalName == x.ReferencedEntity), services))
                                        .Where(x => many2OneNamingLogic(x) == name);

                        returnValue = MakeUnique(name, manyDups, x => x.SchemaName, one2many.SchemaName, entDups, attDups);
                    }
                    else // one to many
                    {
                        var name = one2ManyNamingLogic(one2many);
                        name = string.IsNullOrWhiteSpace(name) ? one2many.SchemaName : name;

                        var entDups = entityMetadata.DisplayName() == name ? 1 : 0;
                        var attDups = entityMetadata.Attributes
                                        .Where(x => filteringService.GenerateAttribute(x, services))
                                        .Count(x => x.DisplayName() == name);
                        var m2oDups = entityMetadata.ManyToOneRelationships?
                                        .Count(x => many2OneNamingLogic(x) == name) ?? 0;
                        var o2mDups = entityMetadata.OneToManyRelationships
                                        .Where(x => filteringService.GenerateRelationship(x, metaData.Entities.FirstOrDefault(y => y.LogicalName == x.ReferencingEntity), services))
                                        .Where(x => one2ManyNamingLogic(x) == name);

                        returnValue = MakeUnique(name, o2mDups, x => x.SchemaName, one2many.SchemaName, entDups, attDups, m2oDups);
                    }
                }
                else // many to many
                {
                    Func<ManyToManyRelationshipMetadata, string> many2ManyNamingLogic = m2m =>
                    {
                        var otherLogicalName = entityMetadata.LogicalName == m2m.Entity1LogicalName ? m2m.Entity2LogicalName : m2m.Entity1LogicalName;
                        otherMeta = metaData.Entities.FirstOrDefault(x => x.LogicalName == otherLogicalName);
                        return otherMeta.DisplayCollectionName.DisplayName();
                    };

                    var many2many = relationshipMetadata as ManyToManyRelationshipMetadata;

                    returnValue = many2ManyNamingLogic(many2many);

                    if (reflexiveRole != null)
                        returnValue = $"{reflexiveRole.Value}{returnValue}";

                    var attDups = entityMetadata.Attributes
                                        .Where(x => filteringService.GenerateAttribute(x, services))
                                        .Count(x => x.DisplayName() == returnValue);
                    var m2mDups = entityMetadata.ManyToManyRelationships
                                        .Where(x => filteringService.GenerateRelationship(x, metaData.Entities.FirstOrDefault(y => y.LogicalName == (entityMetadata.LogicalName == x.Entity1LogicalName ? x.Entity2LogicalName : x.Entity1LogicalName)), services))
                                        .Where(x => many2ManyNamingLogic(x) == returnValue);

                    returnValue = MakeUnique(returnValue, m2mDups, x => x.SchemaName, many2many.SchemaName, attDups);
                }
            }

            this.Debug(returnValue, entityMetadata.LogicalName, relationshipMetadata.SchemaName, reflexiveRole?.ToString() ?? "null");

            tmr.Stop();

            if (tmr.ElapsedMilliseconds > 200)
                Console.WriteLine($"{tmr.ElapsedMilliseconds}: GetNameForRelationship({entityMetadata.LogicalName} {relationshipMetadata.SchemaName}{reflexiveRole}");

            return returnValue;
        }

        [ExcludeFromCodeCoverage]
        public string GetNameForEntitySet(EntityMetadata entityMetadata, IServiceProvider services)
        {
            return DefaultNamingService.GetNameForEntitySet(entityMetadata, services);
        }

        [ExcludeFromCodeCoverage]
        public string GetNameForMessagePair(SdkMessagePair messagePair, IServiceProvider services)
        {
            return DefaultNamingService.GetNameForMessagePair(messagePair, services);
        }

        [ExcludeFromCodeCoverage]
        public string GetNameForRequestField(SdkMessageRequest request, Microsoft.Crm.Services.Utility.SdkMessageRequestField requestField, IServiceProvider services)
        {
            return DefaultNamingService.GetNameForRequestField(request, requestField, services);
        }

        [ExcludeFromCodeCoverage]
        public string GetNameForResponseField(SdkMessageResponse response, SdkMessageResponseField responseField, IServiceProvider services)
        {
            return DefaultNamingService.GetNameForResponseField(response, responseField, services);
        }

        [ExcludeFromCodeCoverage]
        public string GetNameForServiceContext(IServiceProvider services)
        {
            return DefaultNamingService.GetNameForServiceContext(services);
        }

        public string GetNameForOptionSet(EntityMetadata entityMetadata, OptionSetMetadataBase optionSetMetadata, IServiceProvider services)
        {
            var name = optionSetMetadata.Name;

            if (useDisplayNames)
            {
                var metadata = services.LoadMetadata();

                var optionSet = metadata.OptionSets.FirstOrDefault(x => x.Name == optionSetMetadata.Name) as OptionSetMetadata;      

                if (optionSet != null) // global
                {
                    name = optionSet?.DisplayName();

                    var entDups = metadata.Entities.Count(x => x.DisplayName() == name);
                    var matches = metadata.OptionSets.Where(x => x.DisplayName() == name);

                    name = MakeUnique(name, matches, x => x.Name, optionSetMetadata.Name, entDups);
                }
                else // not global
                { 
                    var enumAttributeMetadata = entityMetadata.Attributes.OfType<EnumAttributeMetadata>().FirstOrDefault(x => x?.OptionSet?.Name == optionSetMetadata.Name);

                    optionSet = enumAttributeMetadata?.OptionSet;

                    if (optionSet != null)
                    {
                        var entName = entityMetadata.DisplayName();

                        var entMatches = metadata.Entities.Where(x => x.DisplayName() == entName);

                        entName = MakeUnique(entName, entMatches, x => x.LogicalName, entityMetadata.LogicalName);

                        name = entName + "_" + optionSet?.DisplayName();

                        var matches = metadata.Entities.Where(x => x.DisplayName() == entityMetadata.DisplayName())
                                        .SelectMany(x => x?.Attributes?.OfType<EnumAttributeMetadata>()?.Where(y => y.OptionSet.DisplayName() == optionSet?.DisplayName()));

                        name = MakeUnique(name, matches, x => x.OptionSet.Name, optionSetMetadata.Name);
                    }
                }
            }

            this.Debug(name, entityMetadata?.LogicalName ?? "GLOBAL", optionSetMetadata.Name);

            return name;
        }

        private static string EnsureValidIdentifier(string name)
        {
            if (name == null)
            {
                return "_";
            }

            var pattern = @"^[A-Za-z_][A-Za-z0-9_]*$";

            if (!Regex.IsMatch(name, pattern))
                name = string.Format("_{0}", name);

            return name;
        }

        public string GetNameForOption(OptionSetMetadataBase optionSetMetadata, OptionMetadata optionMetadata, IServiceProvider services)
        {
            var optionName = optionMetadata.DisplayName();

            optionName = EnsureValidIdentifier(optionName);

            var optionSet = optionSetMetadata as OptionSetMetadata;
            var matches = optionSet.Options.Where(x => EnsureValidIdentifier(x.DisplayName()) == optionName);

            optionName = MakeUnique(optionName, matches, x => x.Value, optionMetadata.Value);
            
            return optionName;
        }

        private string MakeUnique<T, U>(string name, IEnumerable<T> duplicates, Func<T, U> uniqueFunction, U uniqueData, params int[] dupCounts)
        {
            if (duplicates.Count() > 1 || dupCounts.Any(x => x > 0))
            {
                var index = duplicates.OrderBy(uniqueFunction).ToList().FindIndex(x => uniqueFunction(x).Equals(uniqueData)) + dupCounts.Sum() + 1;
                if (index > 1)
                    name += index.ToString();
            }

            return name;
        }
    }
}