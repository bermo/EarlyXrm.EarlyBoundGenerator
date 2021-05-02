using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Crm.Services.Utility;
using Microsoft.Xrm.Sdk.Metadata;

namespace EarlyXrm.EarlyBoundGenerator
{
    public class CodeFilteringService : ICodeWriterFilterService
    {
        private readonly ICodeWriterFilterService _defaultService;

        public CodeFilteringService(ICodeWriterFilterService defaultService, IDictionary<string, string> parameters)
        {
            _defaultService = defaultService;

            this.Debug();
            
            foreach(var param in parameters)
                $"Key:{param.Key} Value:{param.Value}".Debug();
        }

        public bool GenerateEntity(EntityMetadata entityMetadata, IServiceProvider services)
        {
            if (entityMetadata.LogicalName == "entity")
                return false;

            if (entityMetadata.LogicalName.StartsWith("new_system_donotuseentity_"))
                return false;

            var solutionEntities = services.LoadSolutionEntities();

            var generate = solutionEntities.Any(x => x.LogicalName == entityMetadata.LogicalName);

            this.Debug(generate, entityMetadata.LogicalName);

            return generate;
        }

        public bool GenerateAttribute(AttributeMetadata attributeMetadata, IServiceProvider services)
        {
            var solutionEntities = services.LoadSolutionEntities();

            var generate = false;
            if (attributeMetadata.AttributeType == AttributeTypeCode.Uniqueidentifier && attributeMetadata.IsPrimaryId == true)
                generate = true;
            else if (attributeMetadata.AttributeOf != null && attributeMetadata.GetType() != typeof(ImageAttributeMetadata))
                generate = false;
            else if (attributeMetadata.AttributeType == AttributeTypeCode.Uniqueidentifier)
                generate = true;
            else if (solutionEntities.Any(x => x.LogicalName == attributeMetadata.EntityLogicalName && x.IncludedFields.Any(y => y.LogicalName == attributeMetadata.LogicalName)))
                generate = _defaultService.GenerateAttribute(attributeMetadata, services);

            this.Debug(generate, attributeMetadata.EntityLogicalName, attributeMetadata.LogicalName);

            return generate;
        }

        public bool GenerateRelationship(RelationshipMetadataBase relationshipMetadata, EntityMetadata otherEntityMetadata, IServiceProvider services)
        {
            var solutionEntities = services.LoadSolutionEntities();

            var generate = _defaultService.GenerateRelationship(relationshipMetadata, otherEntityMetadata, services);

            if (generate && relationshipMetadata.RelationshipType == RelationshipType.OneToManyRelationship)
            {
                var o2m = relationshipMetadata as OneToManyRelationshipMetadata;
                if (o2m.ReferencedEntity == o2m.ReferencingEntity)
                {
                    var entity = solutionEntities.FirstOrDefault(x => x.LogicalName == o2m.ReferencedEntity);
                    if (entity == null || !entity.IncludedFields.Any(x => x.LogicalName == o2m.ReferencingAttribute))
                        generate = false;
                }
            }

            this.Debug(generate, relationshipMetadata.SchemaName, otherEntityMetadata.LogicalName);

            return generate;
        }

        [ExcludeFromCodeCoverage]
        public bool GenerateServiceContext(IServiceProvider services)
        {
            return _defaultService.GenerateServiceContext(services);
        }

        private Dictionary<string, bool> GeneratedOptionSets { get; set; } = new Dictionary<string, bool>();

        public bool GenerateOptionSet(OptionSetMetadataBase optionSetMetadata, IServiceProvider services)
        {
            var solutionEntities = services.LoadSolutionEntities();

            if (optionSetMetadata.IsGlobal == true)
            {
                if (!solutionEntities.Any(x => x.IncludedFields.Any(y => y.OptionSetName != null && y.OptionSetName == optionSetMetadata.Name)))
                    return false;

                var name = optionSetMetadata?.Name;
                if (!string.IsNullOrWhiteSpace(name) && !GeneratedOptionSets.ContainsKey(name))
                {
                    GeneratedOptionSets[name] = true;
                    return true;
                }

                return false;
            }

            if (optionSetMetadata.IsCustomOptionSet ?? false)
                return true;

            if (optionSetMetadata.OptionSetType == OptionSetType.State || optionSetMetadata.OptionSetType == OptionSetType.Status)
                return true;

            if (solutionEntities.Any(x => x.IncludedFields.Any(y => y.OptionSetName != null && y.OptionSetName == optionSetMetadata.Name)))
                return true;

            return false;
        }

        [ExcludeFromCodeCoverage]
        public bool GenerateOption(OptionMetadata optionMetadata, IServiceProvider services)
        {
            return _defaultService.GenerateOption(optionMetadata, services);
        }
    }
}