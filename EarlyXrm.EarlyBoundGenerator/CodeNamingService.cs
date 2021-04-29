using Microsoft.Crm.Services.Utility;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
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

        public string GetNameForEntity(EntityMetadata entityMetadata, IServiceProvider services)
        {
            var entityName = DefaultNamingService.GetNameForEntity(entityMetadata, services);

            if (useDisplayNames)
            {
                var dn = entityMetadata?.DisplayName();
                if (!string.IsNullOrWhiteSpace(dn))
                {
                    entityName = dn;

                    var metaData = services.LoadMetadata();
                    var dups = metaData.Entities.Where(x => x.DisplayName() == entityName).OrderBy(x => x.LogicalName).ToList();

                    if(dups.Count() > 1)
                    {
                        var index = dups.FindIndex(x => x.LogicalName == entityMetadata.LogicalName) + 1;
                        if (index > 1)
                            entityName += index.ToString();
                    }
                }
            }

            this.Debug(entityName, entityMetadata.LogicalName);

            return entityName;
        }

        public string GetNameForAttribute(EntityMetadata entityMetadata, AttributeMetadata attributeMetadata, IServiceProvider services)
        {
            var attributeName = DefaultNamingService.GetNameForAttribute(entityMetadata, attributeMetadata, services);

            if (useDisplayNames)
            {
                attributeName = UniqueDisplayName(entityMetadata, attributeMetadata, services);
            }

            this.Debug(attributeName, entityMetadata.LogicalName, attributeMetadata.LogicalName);

            return attributeName;
        }

        private string UniqueDisplayName(EntityMetadata entityMetadata, AttributeMetadata attributeMetadata, IServiceProvider services)
        {
            string attributeName = attributeMetadata.DisplayName();
            var matches = entityMetadata.Attributes.Where(x => x.DisplayName() == attributeName).OrderBy(x => x.LogicalName).ToList();

            if (entityMetadata.DisplayName() == attributeName)
            {
                matches.Insert(0, new AttributeMetadata { LogicalName = "" });
            }

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

            if (attributeName == "EntityLogicalName")
            {
                attributeName += "2";
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

                    if (one2many.ReferencedEntity == one2many.ReferencingEntity) // parent child
                    {
                        var att = entityMetadata.Attributes.FirstOrDefault(x => x.LogicalName == one2many.ReferencingAttribute);

                        returnValue = att.DisplayName();

                        if (reflexiveRole == EntityRole.Referenced)
                        {
                            otherMeta = metaData.Entities.FirstOrDefault(x => x.LogicalName == one2many.ReferencingEntity);
                            returnValue += otherMeta.DisplayCollectionName.DisplayName();
                        }
                    }
                    else if (entityMetadata.LogicalName == one2many.ReferencingEntity) // many to one
                    {
                        var att = entityMetadata.Attributes.FirstOrDefault(x => x.LogicalName == one2many.ReferencingAttribute);
                        var ent = att.DisplayName();
                        var dups = entityMetadata.ManyToOneRelationships.Where(x => x.ReferencingAttribute == one2many.ReferencingAttribute);

                        if (dups.Count() > 1)
                        {
                            otherMeta = metaData.Entities.FirstOrDefault(x => x.LogicalName == one2many.ReferencedEntity); // one2many.ReferencingAttribute);
                            ent += otherMeta.DisplayName();
                        }

                        var nameDups = entityMetadata.ManyToOneRelationships.Where(x => entityMetadata.Attributes.Any(y => y.DisplayName() == ent)).OrderByDescending(x => x.SchemaName).ToList();
                        if (nameDups.Count() > 1)
                        {
                            var index = nameDups.FindIndex(x => x.SchemaName == one2many.SchemaName) + 1;
                            if (index > 1)
                                ent += index.ToString();
                        }

                        returnValue = ent;
                    }
                    else // one to many
                    {
                        otherMeta = metaData.Entities.FirstOrDefault(x => x.LogicalName == one2many.ReferencingEntity);

                        var ent = otherMeta.DisplayCollectionName.DisplayName();
                        var oneToManys = entityMetadata.OneToManyRelationships.Where(x => x.ReferencingEntity == one2many.ReferencingEntity);

                        if (oneToManys.Count() > 1)
                        {
                            var at = otherMeta.Attributes.FirstOrDefault(x => x.LogicalName == one2many.ReferencingAttribute);
                            ent += at.DisplayName();
                        }

                        if (!string.IsNullOrWhiteSpace(ent))
                        {
                            var nameDups = entityMetadata.OneToManyRelationships
                                .Where(x => 
                                x.ReferencingEntity == one2many.ReferencingEntity && 
                                metaData.Entities.Any(y => y.LogicalName == one2many.ReferencingEntity && y.Attributes.Any(z => y.DisplayCollectionName.DisplayName() + z.DisplayName() == ent)))
                                .OrderBy(x => x.SchemaName).ToList();
                            if (nameDups.Count() > 1)
                            {
                                var index = nameDups.FindIndex(x => x.SchemaName == one2many.SchemaName) + 1;
                                if (index > 1)
                                    ent += index.ToString();
                            }

                            returnValue = ent;
                        }
                    }
                }
                else // many to many
                {
                    var many2many = relationshipMetadata as ManyToManyRelationshipMetadata;
                    var the = entityMetadata.LogicalName == many2many.Entity1LogicalName ? many2many.Entity1LogicalName : many2many.Entity2LogicalName;
                    var other = entityMetadata.LogicalName == many2many.Entity1LogicalName ? many2many.Entity2LogicalName : many2many.Entity1LogicalName;
                    otherMeta = metaData.Entities.FirstOrDefault(x => x.LogicalName == other);

                    returnValue = otherMeta.DisplayCollectionName.DisplayName();

                    var oneToManysAtts = entityMetadata.OneToManyRelationships
                        .Where(x => x.ReferencingEntity == other);
                    var start = oneToManysAtts.Count();

                    var manyToManys = entityMetadata.ManyToManyRelationships
                    .Where(x => (x.Entity1LogicalName == the || x.Entity2LogicalName == the) && otherMeta.DisplayCollectionName.DisplayName() == returnValue)
                    //.Where(x => x.Entity1LogicalName == many2many.Entity1LogicalName && 
                    //            x.Entity2LogicalName == many2many.Entity2LogicalName)
                    .OrderBy(x => x.SchemaName).ToList();

                    var mmIndex = manyToManys.FindIndex(x => x.SchemaName == many2many.SchemaName);
                    var comIndex = start + mmIndex;
                    if (comIndex > 0)
                    {
                        returnValue += comIndex.ToString();
                    }
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
                name = optionSet?.DisplayName();

                if (optionSet == null)
                {
                    var enumAttributeMetadata = metadata.Entities.SelectMany(x => x?.Attributes?.OfType<EnumAttributeMetadata>()?.Where(y => y?.OptionSet?.Name == optionSetMetadata.Name) ?? Array.Empty<EnumAttributeMetadata>())?.FirstOrDefault();
                    optionSet = enumAttributeMetadata?.OptionSet;

                    if (optionSet != null)
                    {
                        var ent = metadata.Entities.First(x => x.LogicalName == enumAttributeMetadata.EntityLogicalName);
                        name = ent.DisplayName() + "_" + optionSet?.DisplayName();

                        var matches = metadata.Entities.SelectMany(x => x?.Attributes?.OfType<EnumAttributeMetadata>()?.Where(y => (x.DisplayName() + "_" + y.OptionSet.DisplayName()) == name)).OrderBy(x => x.OptionSet.MetadataId).ToList();

                        if (matches.Count() > 1)
                        {
                            var index = matches.FindIndex(x => x.OptionSet.MetadataId == optionSetMetadata.MetadataId) + 1;
                            if (index > 1)
                                name += index.ToString();
                        }
                    }
                }
            }

            return name;
        }

        private static string EnsureValidIdentifier(string name)
        {
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
            var matches = optionSet.Options.Where(x => EnsureValidIdentifier(x.DisplayName()) == optionName).OrderBy(x => x.Value);
            if (matches.Count() > 1)
            {
                var index = matches.ToList().FindIndex(x => x.Value == optionMetadata.Value) + 1;
                if (index > 1)
                    optionName += index.ToString();
            }

            return optionName;
        }
    }
}