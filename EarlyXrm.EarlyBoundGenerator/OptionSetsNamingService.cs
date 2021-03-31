using Microsoft.Crm.Services.Utility;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EarlyXrm.EarlyBoundGenerator
{
    public class OptionSetsNamingService : INamingService
    {
        private INamingService DefaultNamingService { get; set; }

        public OptionSetsNamingService(INamingService namingService)
        {
            this.Debug();
            DefaultNamingService = namingService;
        }

        public string GetNameForOptionSet(EntityMetadata entityMetadata, OptionSetMetadataBase optionSetMetadata, IServiceProvider services)
        {
            return optionSetMetadata.Name;
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

        private static string EnsureValidIdentifier(string name)
        {
            var pattern = @"^[A-Za-z_][A-Za-z0-9_]*$";

            if (!Regex.IsMatch(name, pattern))
                name = string.Format("_{0}", name);

            return name;
        }

        public string GetNameForAttribute(EntityMetadata entityMetadata, AttributeMetadata attributeMetadata, IServiceProvider services)
        {
            return DefaultNamingService.GetNameForAttribute(entityMetadata, attributeMetadata, services);
        }

        public string GetNameForEntity(EntityMetadata entityMetadata, IServiceProvider services)
        {
            return DefaultNamingService.GetNameForEntity(entityMetadata, services);
        }

        public string GetNameForEntitySet(EntityMetadata entityMetadata, IServiceProvider services)
        {
            return DefaultNamingService.GetNameForEntitySet(entityMetadata, services);
        }

        public string GetNameForMessagePair(SdkMessagePair messagePair, IServiceProvider services)
        {
            return DefaultNamingService.GetNameForMessagePair(messagePair, services);
        }

        public string GetNameForRelationship(EntityMetadata entityMetadata, RelationshipMetadataBase relationshipMetadata, EntityRole? reflexiveRole, IServiceProvider services)
        {
            return DefaultNamingService.GetNameForRelationship(entityMetadata, relationshipMetadata, reflexiveRole, services);
        }

        public string GetNameForRequestField(SdkMessageRequest request, SdkMessageRequestField requestField, IServiceProvider services)
        {
            return DefaultNamingService.GetNameForRequestField(request, requestField, services);
        }

        public string GetNameForResponseField(SdkMessageResponse response, SdkMessageResponseField responseField, IServiceProvider services)
        {
            return DefaultNamingService.GetNameForResponseField(response, responseField, services);
        }

        public string GetNameForServiceContext(IServiceProvider services)
        {
            return DefaultNamingService.GetNameForServiceContext(services);
        }
    }
}