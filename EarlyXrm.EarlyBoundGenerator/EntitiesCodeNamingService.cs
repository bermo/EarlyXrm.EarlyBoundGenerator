using Microsoft.Crm.Services.Utility;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EarlyXrm.EarlyBoundGenerator
{
    public class EntitiesCodeNamingService : INamingService
    {
        private INamingService DefaultNamingService { get; set; }
        private readonly bool useDisplayNames;

        public EntitiesCodeNamingService(INamingService namingService, IDictionary<string, string> parameters)
        {
            this.Debug();
            bool.TryParse(parameters["UseDisplayNames"?.ToUpper()], out useDisplayNames);
            DefaultNamingService = namingService;
        }

        public string GetNameForEntity(EntityMetadata entityMetadata, IServiceProvider services)
        {
            var entityName = DefaultNamingService.GetNameForEntity(entityMetadata, services);

            if (useDisplayNames)
            {
                var dn = entityMetadata?.DisplayName();
                if (!string.IsNullOrWhiteSpace(dn))
                    entityName = dn;
            }

            this.Debug(entityName, entityMetadata.LogicalName);

            return entityName;
        }

        public string GetNameForAttribute(EntityMetadata entityMetadata, AttributeMetadata attributeMetadata, IServiceProvider services)
        {
            var attributeName = DefaultNamingService.GetNameForAttribute(entityMetadata, attributeMetadata, services);

            if (useDisplayNames)
            {
                attributeName = attributeMetadata.DisplayName();

                var matches = entityMetadata.Attributes.Where(x => x.DisplayName() == attributeName).OrderBy(x => x.LogicalName);
                if (matches.Count() > 1)
                {
                    var index = matches.ToList().FindIndex(x => x.LogicalName == attributeMetadata.LogicalName) + 1;
                    if (index > 1)
                        attributeName += index.ToString();
                }

                if (string.IsNullOrWhiteSpace(attributeName) || attributeMetadata.AttributeType == AttributeTypeCode.Uniqueidentifier)
                    attributeName = DefaultNamingService.GetNameForAttribute(entityMetadata, attributeMetadata, services);

                if (attributeMetadata.AttributeType == AttributeTypeCode.Lookup || attributeMetadata.AttributeType == AttributeTypeCode.Customer)
                {
                    attributeName += "Ref";
                }
            }

            this.Debug(attributeName, entityMetadata.LogicalName, attributeMetadata.LogicalName);

            return attributeName;
        }

        public string GetNameForRelationship(EntityMetadata entityMetadata, RelationshipMetadataBase relationshipMetadata, EntityRole? reflexiveRole, IServiceProvider services)
        {
            var metaData = services.LoadMetadata();

            var returnValue = DefaultNamingService.GetNameForRelationship(entityMetadata, relationshipMetadata, reflexiveRole, services);

            if (useDisplayNames)
            {
                EntityMetadata otherMeta;
                if (relationshipMetadata.RelationshipType == RelationshipType.OneToManyRelationship)
                {
                    var one2many = relationshipMetadata as OneToManyRelationshipMetadata;

                    if (one2many.ReferencedEntity == one2many.ReferencingEntity)
                    {
                        var att = entityMetadata.Attributes.FirstOrDefault(x => x.LogicalName == one2many.ReferencingAttribute);

                        returnValue = att.DisplayName();

                        if (reflexiveRole == EntityRole.Referenced)
                        {
                            otherMeta = metaData.Entities.FirstOrDefault(x => x.LogicalName == one2many.ReferencingEntity);
                            returnValue += otherMeta.DisplayCollectionName.DisplayName();
                        }
                    }
                    else if (entityMetadata.LogicalName == one2many.ReferencingEntity)
                    {
                        var att = entityMetadata.Attributes.FirstOrDefault(x => x.LogicalName == one2many.ReferencingAttribute);
                        var ent = att.DisplayName();
                        var dups = entityMetadata.ManyToOneRelationships.Where(x => x.ReferencingAttribute == one2many.ReferencingAttribute);

                        if (dups.Count() > 1)
                        {
                            otherMeta = metaData.Entities.FirstOrDefault(x => x.LogicalName == one2many.ReferencedEntity);
                            ent += otherMeta.DisplayName();
                        }

                        returnValue = ent;
                    }
                    else
                    {
                        otherMeta = metaData.Entities.FirstOrDefault(x => x.LogicalName == one2many.ReferencingEntity);

                        var ent = otherMeta.DisplayCollectionName.DisplayName();
                        var oneToManys = entityMetadata.OneToManyRelationships.Where(x => x.ReferencingEntity == one2many.ReferencingEntity);

                        if (oneToManys.Count() > 1)
                        {
                            var at = otherMeta.Attributes.FirstOrDefault(x => x.LogicalName == one2many.ReferencingAttribute);
                            ent += at.DisplayName();
                        }

                        returnValue = ent;
                    }
                }
                else
                {
                    var many2many = relationshipMetadata as ManyToManyRelationshipMetadata;
                    otherMeta = metaData.Entities.FirstOrDefault(x => x.LogicalName == (entityMetadata.LogicalName == many2many.Entity1LogicalName ? many2many.Entity2LogicalName : many2many.Entity1LogicalName));

                    returnValue = otherMeta.DisplayCollectionName.DisplayName();
                }
            }

            this.Debug(returnValue, entityMetadata.LogicalName, relationshipMetadata.SchemaName, reflexiveRole?.ToString() ?? "null");

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
        public string GetNameForRequestField(SdkMessageRequest request, SdkMessageRequestField requestField, IServiceProvider services)
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

        [ExcludeFromCodeCoverage]
        public string GetNameForOptionSet(EntityMetadata entityMetadata, OptionSetMetadataBase optionSetMetadata, IServiceProvider services)
        {
            return DefaultNamingService.GetNameForOptionSet(entityMetadata, optionSetMetadata, services);
        }

        [ExcludeFromCodeCoverage]
        public string GetNameForOption(OptionSetMetadataBase optionSetMetadata, OptionMetadata optionMetadata, IServiceProvider services)
        {
            return DefaultNamingService.GetNameForOption(optionSetMetadata, optionMetadata, services);
        }
    }
}