using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Crm.Services.Utility;
using Microsoft.Xrm.Sdk.Metadata;

namespace EarlyXrm.EarlyBoundGenerator
{
    public class EntitiesCodeFilteringService : ICodeWriterFilterService
    {
        private readonly ICodeWriterFilterService _defaultService;

        public EntitiesCodeFilteringService(ICodeWriterFilterService defaultService, IDictionary<string, string> parameters)
        {
            _defaultService = defaultService;

            this.Debug();
            
            foreach(var param in parameters)
                $"Key:{param.Key} Value:{param.Value}".Debug();
        }

        public bool GenerateEntity(EntityMetadata entityMetadata, IServiceProvider services)
        {
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

        [ExcludeFromCodeCoverage]
        public bool GenerateOptionSet(OptionSetMetadataBase optionSetMetadata, IServiceProvider services)
        {
            return false;
        }

        [ExcludeFromCodeCoverage]
        public bool GenerateOption(OptionMetadata optionMetadata, IServiceProvider services)
        {
            return false;
        }
    }
}