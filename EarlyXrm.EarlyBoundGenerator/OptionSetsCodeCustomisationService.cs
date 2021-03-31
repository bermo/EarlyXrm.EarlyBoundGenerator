using Microsoft.Crm.Services.Utility;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;

namespace EarlyXrm.EarlyBoundGenerator
{
    public class OptionSetsCodeCustomisationService : ICustomizeCodeDomService
    {
        private readonly bool useDisplayNames;

        public OptionSetsCodeCustomisationService(IDictionary<string, string> parameters)
        {
            this.Debug();
            bool.TryParse(parameters["UseDisplayNames".ToUpper()], out useDisplayNames);
        }

        public void CustomizeCodeDom(CodeCompileUnit codeUnit, IServiceProvider services)
        {
            var metadata = services.LoadMetadata();

            foreach (CodeAttributeDeclaration attribute in codeUnit.AssemblyCustomAttributes)
            {
                if (attribute.AttributeType.BaseType == "Microsoft.Xrm.Sdk.Client.ProxyTypesAssemblyAttribute")
                {
                    codeUnit.AssemblyCustomAttributes.Remove(attribute);
                    break;
                }
            }

            for (var i = 0; i < codeUnit.Namespaces.Count; ++i)
            {
                var types = codeUnit.Namespaces[i].Types;

                for (var j = 0; j < types.Count;)
                {
                    if (!types[j].IsEnum)
                        types.RemoveAt(j);
                    else
                        j += 1;
                }

                var typesCopy = new CodeTypeDeclaration[codeUnit.Namespaces[i].Types.Count];
                codeUnit.Namespaces[i].Types.CopyTo(typesCopy, 0);
                var orderedTypes = typesCopy.OrderBy(x => x.Name);

                for (var j = 0; j < codeUnit.Namespaces[i].Types.Count;)
                    codeUnit.Namespaces[i].Types.RemoveAt(j);

                codeUnit.Namespaces[i].Types.AddRange(orderedTypes.ToArray());
            }

            foreach (CodeNamespace ns in codeUnit.Namespaces)
            {
                ns.Imports.Add(new CodeNamespaceImport("System.Runtime.Serialization"));
                ns.Imports.Add(new CodeNamespaceImport("System.ComponentModel"));

                foreach (CodeTypeDeclaration type in ns.Types)
                {
                    for (var i = type.CustomAttributes.Count - 1; i >= 0; i--)
                    {
                        var catt = type.CustomAttributes[i];
                        catt.Name = catt.Name.Replace("System.Runtime.Serialization.", "").Replace("DataContractAttribute", "DataContract");
                        if (catt.Name.Contains("GeneratedCodeAttribute"))
                            type.CustomAttributes.RemoveAt(i);
                    }

                    var optionSet = metadata.OptionSets.FirstOrDefault(x => x.Name == type.Name) as OptionSetMetadata;
                    var displayName = optionSet?.DisplayName();
                    EnumAttributeMetadata enumAttributeMetadata = null;

                    if (optionSet == null)
                    {
                        enumAttributeMetadata = metadata.Entities.SelectMany(x => x.Attributes.OfType<EnumAttributeMetadata>().Where(y => y?.OptionSet?.Name == type.Name)).FirstOrDefault();
                        optionSet = enumAttributeMetadata?.OptionSet;

                        if (optionSet == null)
                            continue;

                        displayName = metadata.Entities.First(x => x.LogicalName == enumAttributeMetadata.EntityLogicalName).DisplayName() + "_" + optionSet?.DisplayName();
                    }
                    
                    foreach (var field in type.Members.Cast<CodeTypeMember>().OfType<CodeMemberField>())
                    {
                        var codeExpression = field.InitExpression as CodePrimitiveExpression;
                        var val = (int)codeExpression.Value;
                        
                        foreach (CodeAttributeDeclaration att in field.CustomAttributes)
                            att.Name = att.Name.Replace("System.Runtime.Serialization.", "").Replace("EnumMemberAttribute", "EnumMember");

                        if (optionSet != null)
                        {
                            var option = optionSet.Options.FirstOrDefault(x => x.Value.Value == val);
                            if (option != null)
                            {
                                var status = option as StatusOptionMetadata;
                                if (status != null)
                                {
                                    var stateOptionSet = metadata.Entities.SelectMany(x => x.Attributes.OfType<StateAttributeMetadata>().Where(y => y?.OptionSet?.Name == type.Name.Replace("_statuscode","_statecode"))).FirstOrDefault()?.OptionSet;
                                    var stateOption = stateOptionSet.Options.FirstOrDefault(x => x.Value.Value == status.State.Value);

                                    var state = useDisplayNames ? metadata.Entities.First(x => x.LogicalName == enumAttributeMetadata.EntityLogicalName).DisplayName() + "_" + stateOptionSet.DisplayName() : stateOptionSet.Name;

                                    var namingService = (INamingService)services.GetService(typeof(INamingService));

                                    var name = namingService.GetNameForOption(stateOptionSet, stateOption, services);
                                    field.CustomAttributes.Insert(0, new CodeAttributeDeclaration("AmbientValue", new CodeAttributeArgument(
                                        new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(state), name)
                                    )));
                                }

                                field.CustomAttributes.Insert(0, new CodeAttributeDeclaration("Description", new CodeAttributeArgument(new CodePrimitiveExpression(option.Label?.LocalizedLabels?.FirstOrDefault()?.Label))));
                            }
                        }
                    }

                    if (useDisplayNames)
                    {
                        type.Name = displayName;
                    }
                }
            }
        }
    }
}