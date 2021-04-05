using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Tooling.Connector;
using System.Globalization;
using Microsoft.Crm.Services.Utility;
using System.Diagnostics;
using EarlyBoundSolutionGenerator;

namespace EarlyXrm.EarlyBoundGenerator
{
    public static class SolutionHelper
    {
        private static bool debugMode;
        private static IOrganizationMetadata organisationMetadata;
        private static IServiceProvider services;
        private static string solutionName;
        private static IEnumerable<IncludedEntity> solutionEntities;
        private static Dictionary<string, List<string>> skip = new Dictionary<string, List<string>>();
        private static Dictionary<string, List<string>> extra = new Dictionary<string, List<string>>();

        public static IOrganizationService organisationService;

        static SolutionHelper()
        {
            bool.TryParse(GetParameter("DebugMode"?.ToUpper()), out debugMode);

            var extra = GetParameter("extra");
            if (extra != null)
            {
                var entities = extra.Split(';').Where(x => !string.IsNullOrWhiteSpace(x));
                var metaInclude = entities.Select(x => x.Split(':')).ToDictionary(x => x.First().Trim(), x => x.Skip(1).FirstOrDefault()?.Split(',')
                                    .Select(y => y.Trim()).ToList());
                SolutionHelper.extra = metaInclude;
            }

            var skip = GetParameter("skip");
            if (skip != null)
            {
                var entities = skip.Split(';').Where(x => !string.IsNullOrWhiteSpace(x));
                var meta2Include = entities.Select(x => x.Split(':'))
                                    .GroupBy(
                                        x => x.First().Trim(),
                                        x => x.Skip(1).FirstOrDefault()
                                    );
                var metaInclude = meta2Include.ToDictionary(x => x.Key, x => x?.SelectMany(y => y?.Split(',')?.Select(z => z?.Trim()) ?? new List<string>()).ToList() );
                SolutionHelper.skip = metaInclude;
            }
        }

        public static IOrganizationMetadata LoadMetadata(this IServiceProvider services)
        {
            if (SolutionHelper.services == null)
                SolutionHelper.services = services;

            if (organisationMetadata != null)
                return organisationMetadata;

            "IMetadataProviderService.LoadMetadata Start".Debug();

            var metadataProviderService = (IMetadataProviderService)services.GetService(typeof(IMetadataProviderService));
            organisationMetadata = metadataProviderService.LoadMetadata();

            "IMetadataProviderService.LoadMetadata End".Debug();

            return organisationMetadata;
        }

        public static IEnumerable<IncludedEntity> LoadSolutionEntities(this IServiceProvider services)
        {
            services.LoadMetadata();

            if (solutionEntities == null)
                solutionEntities = GetSolutionEntities();

            return solutionEntities;
        }

        private static string GetParameter(string key)
        {
            var args = Environment.GetCommandLineArgs();
            var arg = args.FirstOrDefault(x => x.ToLower().StartsWith("/" + key.ToLower()));

            if (arg != null)
            {
                var colonPostion = arg.IndexOf(":");
                return arg.Substring(colonPostion + 1).Trim(new[] { '"' });
            }

            return null;
        }

        private static IEnumerable<IncludedEntity> GetSolutionEntities()
        {
            var includedEntities = new List<IncludedEntity>();
            var solutionComponents = GetSolutionComponents(componenttype.Entity, componenttype.Attribute);

            var skipGlobal = skip.ContainsKey("*") ? skip["*"] : null;

            foreach (var entity in organisationMetadata.Entities)
            {
                var solutionComponent = solutionComponents.FirstOrDefault(x => x.ObjectId.Value == entity.MetadataId.Value);

                var skipFields = skip.ContainsKey(entity.LogicalName) ? skip[entity.LogicalName] : null;
                var fullSkip = skip.ContainsKey(entity.LogicalName) && (!skipFields?.Any() ?? true);

                if (fullSkip || !extra.ContainsKey(entity.LogicalName) && solutionComponent == null)
                    continue;

                var extraFields = extra.ContainsKey(entity.LogicalName) ? extra[entity.LogicalName] : null;

                var includedEntity = new IncludedEntity { LogicalName = entity.LogicalName };

                foreach (var attribute in entity?.Attributes ?? new AttributeMetadata[0])
                {
                    if ((!skipFields?.Any(y => y == attribute.LogicalName) ?? true) &&
                        (!skipGlobal?.Any(y => y == attribute.LogicalName) ?? true) &&
                        (
                            entity.IsCustomEntity == true ||
                            solutionComponent != null && solutionComponent.RootComponentBehavior == solutioncomponent_rootcomponentbehavior.IncludeSubcomponents ||
                            (extraFields?.Any(y => y == attribute.LogicalName) ?? extra.ContainsKey(entity.LogicalName)) ||
                            solutionComponents.Any(y => y.ObjectId.HasValue && attribute.MetadataId.HasValue && y.ObjectId.Value == attribute.MetadataId.Value)
                        )
                    )
                    {
                        includedEntity.IncludedFields.Add(new IncludedField
                        {
                            LogicalName = attribute.LogicalName,
                            OptionSetName = (attribute as PicklistAttributeMetadata)?.OptionSet?.Name
                        });
                    }
                }

                includedEntities.Add(includedEntity);
            }

            return includedEntities;
        }

        private static IEnumerable<SolutionComponent> GetSolutionComponents(params componenttype[] componentTypes)
        {
            solutionName = GetParameter("solutionname");

            var types = componentTypes.Select(x => (int)x).ToArray();
            var query = new QueryExpression(SolutionComponent.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(
                    nameof(SolutionComponent.ObjectId).ToLower(), 
                    nameof(SolutionComponent.RootComponentBehavior).ToLower()
                ),
                Criteria = { 
                    Conditions = { 
                        new ConditionExpression(nameof(SolutionComponent.ComponentType).ToLower(), ConditionOperator.In, types) 
                    } 
                },
                LinkEntities =
                {
                    new LinkEntity()
                    {
                        LinkFromEntityName = SolutionComponent.EntityLogicalName,
                        LinkFromAttributeName = nameof(SolutionComponent.SolutionId).ToLower(),
                        LinkToEntityName = Solution.EntityLogicalName,
                        LinkToAttributeName = nameof(Solution.SolutionId).ToLower(),
                        JoinOperator = JoinOperator.Inner,
                        LinkCriteria = { 
                            Conditions = { 
                                new ConditionExpression(nameof(Solution.UniqueName).ToLower(), ConditionOperator.Equal, solutionName) 
                            } 
                        }
                    }
                }
            };

            $"{nameof(SolutionHelper.GetSolutionComponents)} Start".Debug();

            IEnumerable<SolutionComponent> result;

            if (organisationService == null)
                organisationService = new CrmServiceClient(GetParameter("connectionstring"));
            
            result = organisationService.RetrieveMultiple(query).Entities.Select(x => x.ToEntity<SolutionComponent>());

            $"{nameof(SolutionHelper.GetSolutionComponents)} End".Debug();

            return result;
        }

        public static string DisplayName(this MetadataBase metadataBase)
        {
            var displayName = "";
            var attribute = metadataBase as AttributeMetadata;
            if (attribute != null)
            {
                displayName = attribute.DisplayName.DisplayName();

                if (displayName == null)
                {
                    return attribute.SchemaName;
                }
            }

            var entity = metadataBase as EntityMetadata;
            if (entity != null)
            {
                return entity.DisplayName.DisplayName();
            }

            var option = metadataBase as OptionMetadata;
            if (option != null)
            {
                displayName = option.Label.DisplayName();
            }

            var optionset = metadataBase as OptionSetMetadata;
            if (optionset != null)
            {
                displayName = optionset.DisplayName.DisplayName();

                if (displayName == null)
                {
                    var lookup = new Dictionary<string, string>{
                        { "organization_dateformatcode", "DateFormatCode" },
                        { "organization_plugintracelogsetting", "PluginTraceLogSetting" },
                        { "flipswitch_options", "FlipSwitchOptions" }
                    };

                    if (lookup.ContainsKey(optionset.Name))
                        return lookup[optionset.Name];

                    return optionset.Name;
                }
            }

            return displayName;
        }

        public static string DisplayName(this Label label)
        {
            if (label == null)
                return null;

            var description = label?.LocalizedLabels?.FirstOrDefault()?.Label ?? null;
            if (description != null)
            {
                if (description.Contains(" "))
                    description = new CultureInfo("en-AU", false).TextInfo.ToTitleCase(description);

                description = description
                                .Replace(" ", "")
                                .Replace("(", "")
                                .Replace(")", "")
                                .Replace("/", "_")
                                .Replace(":", "_")
                                .Replace(".", "")
                                .Replace("-", "")
                                .Replace("'", "")
                                .Replace(",", "")
                                .Replace("%", "");

                return description;
            }

            return null;
        }

        private static string Signature(this StackTrace stackTrace, params string[] parameters)
        {
            var callingMethod = stackTrace.GetFrame(1).GetMethod();
            var joinedParameters = string.Join(", ", parameters);

            var declaringType = callingMethod.DeclaringType;
            var declaringTypeName = declaringType.IsGenericType ? "" : declaringType.Name;

            var callingMethodName = callingMethod.IsGenericMethod ? "" : callingMethod.Name;

            if (callingMethod.Name == ".ctor")
                callingMethodName = "ctor";

            return $"{declaringTypeName}.{callingMethodName}({joinedParameters})";
        }

        public static void Debug(this object value)
        {
            if (debugMode)
                Console.WriteLine($"{DateTime.Now} {new StackTrace().Signature()}");
        }

        public static void Debug(this object value, object returnValue, params string[] parameters)
        {
            if (debugMode)
                Console.WriteLine($"{DateTime.Now} {returnValue} {new StackTrace().Signature(parameters)}");
        }

        public static void Debug(this string value)
        {
            if (debugMode)
                Console.WriteLine($"{DateTime.Now} {new StackTrace().Signature()} {value}");
        }
    }
}