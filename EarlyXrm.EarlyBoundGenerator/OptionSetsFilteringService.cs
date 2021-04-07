using Microsoft.Crm.Services.Utility;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EarlyXrm.EarlyBoundGenerator
{
    public class OptionSetsFilteringService : ICodeWriterFilterService
    {
        private Dictionary<string, bool> GeneratedOptionSets { get; set; } = new Dictionary<string, bool>();
        private ICodeWriterFilterService DefaultService { get; set; }

        public OptionSetsFilteringService(ICodeWriterFilterService defaultService)
        {
            this.Debug();
            DefaultService = defaultService;
        }

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

        public bool GenerateAttribute(AttributeMetadata attributeMetadata, IServiceProvider services)
        {
            if (attributeMetadata.AttributeType != AttributeTypeCode.Picklist &&
                attributeMetadata.AttributeType != AttributeTypeCode.State &&
                attributeMetadata.AttributeType != AttributeTypeCode.Status)
                return false;

            return true;
        }

        public bool GenerateEntity(EntityMetadata entityMetadata, IServiceProvider services)
        {
            var solutionEntities = services.LoadSolutionEntities();
            return solutionEntities.Any(x => x.LogicalName == entityMetadata.LogicalName);
        }

        [ExcludeFromCodeCoverage]
        public bool GenerateRelationship(RelationshipMetadataBase relationshipMetadata, EntityMetadata otherEntityMetadata, IServiceProvider services)
        {
            return false;
        }

        [ExcludeFromCodeCoverage]
        public bool GenerateServiceContext(IServiceProvider services)
        {
            return false;
        }

        [ExcludeFromCodeCoverage]
        public bool GenerateOption(OptionMetadata optionMetadata, IServiceProvider services)
        {
            return DefaultService.GenerateOption(optionMetadata, services);
        }
    }
}