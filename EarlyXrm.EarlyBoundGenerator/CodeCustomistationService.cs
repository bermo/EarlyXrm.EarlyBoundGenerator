using Microsoft.Crm.Services.Utility;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EarlyXrm.EarlyBoundGenerator
{
    public class CodeCustomistationService : ICustomizeCodeDomService
    {
        private readonly bool UseDisplayNames;
        private readonly bool Instrument;
        private readonly bool AddSetters;

        public CodeCustomistationService(IDictionary<string, string> parameters)
        {
            bool.TryParse(parameters[nameof(UseDisplayNames)], out UseDisplayNames);
            bool.TryParse(parameters[nameof(Instrument)], out Instrument);
            bool.TryParse(parameters[nameof(AddSetters)], out AddSetters);
            this.Debug();
        }

        public static string tabs = new string('\t', 4);

        public void CustomizeCodeDom(CodeCompileUnit codeCompileUnit, IServiceProvider services)
        {
            var metadata = services.LoadMetadata();

            var codeNamespace = codeCompileUnit.Namespaces[0];
            var imports = codeNamespace.Imports;
            imports.AddRange(new[] {
                new CodeNamespaceImport { Namespace = "System" },
                new CodeNamespaceImport { Namespace = "System.Linq" },
                new CodeNamespaceImport { Namespace = "Microsoft.Xrm.Sdk" },
                new CodeNamespaceImport { Namespace = "System.Runtime.Serialization" },
                new CodeNamespaceImport { Namespace = "Microsoft.Xrm.Sdk.Client" },
                new CodeNamespaceImport { Namespace = "System.Collections.Generic" },
                new CodeNamespaceImport { Namespace = "System.ComponentModel" },
                new CodeNamespaceImport { Namespace = "System.Diagnostics.CodeAnalysis" }
            });

            foreach (var entityMetadata in metadata.Entities)
            {
                var entityClass = codeNamespace.Types.Cast<CodeTypeDeclaration>().FirstOrDefault(x => GetAttributeValues<EntityLogicalNameAttribute>(x.CustomAttributes)
                                                    ?.FirstOrDefault() == entityMetadata.LogicalName);
                if (entityClass == null)
                    continue;

                var interfaceCodeType = new CodeTypeDeclaration
                {
                    Name = "I" + entityClass.Name,
                    IsPartial = true,
                    IsInterface = true
                };

                if (Instrument)
                {
                    codeNamespace.Types.Add(interfaceCodeType);
                }

                entityClass.Comments.Clear();
                entityClass.BaseTypes.Clear();
                entityClass.BaseTypes.Add(new CodeTypeReference("EarlyEntity"));
                if (Instrument)
                {
                    entityClass.BaseTypes.Add(new CodeTypeReference("I" + entityClass.Name));
                }

                entityClass.CustomAttributes.Add(new CodeAttributeDeclaration("ExcludeFromCodeCoverage"));

                for (var i = entityClass.Members.Count - 1; i >= 0; i--)
                {
                    var codeTypeMember = entityClass.Members[i];
                    var codeConstructor = codeTypeMember as CodeConstructor;
                    if (codeConstructor != null)
                    {
                        codeConstructor.Comments.Clear();
                        continue;
                    }

                    var codeMethod = codeTypeMember as CodeMemberMethod;
                    if (codeMethod != null)
                    {
                        if (codeMethod.Name == "OnPropertyChanging" || codeMethod.Name == "OnPropertyChanged")
                            entityClass.Members.RemoveAt(i);
                        continue;
                    }

                    var codeEvent = codeTypeMember as CodeMemberEvent;
                    if (codeEvent != null)
                    {
                        entityClass.Members.RemoveAt(i);
                        continue;
                    }

                    var codeMemberField = codeTypeMember as CodeMemberField;
                    if (codeMemberField != null)
                    {
                        if (codeMemberField.Name == "EntityTypeCode")
                        {
                            entityClass.Members.RemoveAt(i);
                        }
                        continue;
                    }

                    var codeMemberProperty = codeTypeMember as CodeMemberProperty;
                    if (codeMemberProperty == null)
                        continue;

                    codeMemberProperty.Comments.Clear();

                    foreach (CodeAttributeDeclaration att in codeTypeMember.CustomAttributes)
                    {
                        att.Name = att.Name.Replace("Microsoft.Xrm.Sdk.", "");
                    }

                    codeTypeMember.Comments.Clear();

                    var interfaceProp = new CodeMemberProperty
                    {
                        Name = codeMemberProperty.Name,
                        HasSet = false,
                        HasGet = true
                    };

                    var type = codeMemberProperty.Type;

                    if (type.BaseType == "System.Guid") codeMemberProperty.Type = new CodeTypeReference("Guid");
                    if (type.BaseType.StartsWith("Microsoft.Xrm.Sdk.")) codeMemberProperty.Type = new CodeTypeReference(codeMemberProperty.Type.BaseType.Replace("Microsoft.Xrm.Sdk.", ""));
                    if (type.BaseType.StartsWith(codeNamespace.Name + "."))
                    {
                        interfaceProp.Type = new CodeTypeReference(codeMemberProperty.Type.BaseType.Replace(codeNamespace.Name + ".", "I"));
                        codeMemberProperty.Type = new CodeTypeReference(codeMemberProperty.Type.BaseType.Replace(codeNamespace.Name + ".", ""));

                        if (Instrument)
                        {
                            entityClass.Members.Add(new CodeMemberProperty
                            {
                                PrivateImplementationType = new CodeTypeReference($"I{entityClass.Name}"),
                                Name = codeMemberProperty.Name,
                                Type = interfaceProp.Type,
                                GetStatements = { new CodeSnippetStatement($"\t\t\t\treturn {codeMemberProperty.Name};") }
                            });
                        }
                    }

                    if (type.BaseType.StartsWith("System.Nullable`1"))
                    {
                        if (type.TypeArguments[0].BaseType == "System.Int32") codeMemberProperty.Type = new CodeTypeReference("int?");
                        if (type.TypeArguments[0].BaseType == "System.Int64") codeMemberProperty.Type = new CodeTypeReference("long?");
                        if (type.TypeArguments[0].BaseType == "System.Boolean") codeMemberProperty.Type = new CodeTypeReference("bool?");
                        if (type.TypeArguments[0].BaseType == "System.DateTime") codeMemberProperty.Type = new CodeTypeReference("DateTime?");
                        if (type.TypeArguments[0].BaseType == "System.Double") codeMemberProperty.Type = new CodeTypeReference("double?");
                        if (type.TypeArguments[0].BaseType == "System.Decimal") codeMemberProperty.Type = new CodeTypeReference("decimal?");
                        if (type.TypeArguments[0].BaseType == "System.Guid")
                        {
                            codeMemberProperty.Type = new CodeTypeReference("Guid?");
                            var attributeLogicalNameAttribute = codeMemberProperty.CustomAttributes.Cast<CodeAttributeDeclaration>().FirstOrDefault(x => x.Name.EndsWith("AttributeLogicalNameAttribute"));
                            if (attributeLogicalNameAttribute != null)
                            {
                                var ctr = attributeLogicalNameAttribute.Arguments.Cast<CodeAttributeArgument>().First().Value as CodePrimitiveExpression;
                                var logicalName = ctr.Value.ToString();
                                var att = entityMetadata.Attributes.FirstOrDefault(x => x.LogicalName == logicalName);
                                if (att.IsPrimaryId == true)
                                {
                                    entityClass.Members.RemoveAt(i);
                                    continue;
                                }
                            }
                        }
                    }

                    if (type.BaseType == "Microsoft.Xrm.Sdk.OptionSetValue" || type.BaseType == "System.Object")
                    {
                        var enumAtt = entityMetadata.Attributes.FirstOrDefault(x => (UseDisplayNames ? x.DisplayName() : x.SchemaName) == codeMemberProperty.Name &&
                                new[] { AttributeTypeCode.Picklist, AttributeTypeCode.Status, AttributeTypeCode.State }.Any(y => y == x.AttributeType)) as EnumAttributeMetadata;

                        if (enumAtt != null)
                        {
                            var optionsSetName = "";
                            if (!UseDisplayNames)
                                optionsSetName = enumAtt.OptionSet.Name;
                            else if (enumAtt.OptionSet.IsGlobal ?? false)
                                optionsSetName = enumAtt.OptionSet.DisplayName();
                            else
                                optionsSetName = $"{entityMetadata?.DisplayName()}_{enumAtt.OptionSet.DisplayName()}";

                            var crt = new CodeTypeReference(optionsSetName + "?");
                            interfaceProp.Type = crt;
                        }
                    }

                    if (type.BaseType.StartsWith("System.Collections.Generic."))
                    {
                        if (type.TypeArguments[0].BaseType.StartsWith("Microsoft.Xrm.Sdk."))
                        {
                            codeMemberProperty.Type = new CodeTypeReference(type.BaseType.Replace("System.Collections.Generic.", ""),
                                new CodeTypeReference(type.TypeArguments[0].BaseType.Replace("Microsoft.Xrm.Sdk.", "")));

                            interfaceProp.Type = new CodeTypeReference(type.BaseType.Replace("System.Collections.Generic.", ""),
                                new CodeTypeReference("I" + type.TypeArguments[0].BaseType.Replace("Microsoft.Xrm.Sdk.", "")));
                        }
                    }

                    if (type.BaseType.StartsWith("System.Collections.Generic.IEnumerable"))
                    {
                        var baseType = type.TypeArguments[0].BaseType.Replace(codeNamespace.Name + ".", "");
                        codeMemberProperty.Type = new CodeTypeReference(type.BaseType.Replace("System.Collections.Generic.", ""), new CodeTypeReference($"{baseType}"));

                        if (Instrument)
                        {
                            interfaceProp.Type = new CodeTypeReference(type.BaseType.Replace("System.Collections.Generic.", ""), new CodeTypeReference($"I{baseType}"));

                            entityClass.Members.Add(new CodeMemberProperty
                            {
                                PrivateImplementationType = new CodeTypeReference($"I{entityClass.Name}"),
                                Name = codeMemberProperty.Name,
                                Type = interfaceProp.Type,
                                GetStatements = { new CodeSnippetStatement($"\t\t\t\treturn {codeMemberProperty.Name}.Cast<I{baseType}>();") }
                            });
                        }
                    }

                    if (Instrument)
                    {
                        if (interfaceProp.Type.BaseType == "System.Void")
                        {
                            interfaceProp.Type = codeMemberProperty.Type;
                        }

                        interfaceCodeType.Members.Add(interfaceProp);
                    }
                }

                var logicalNames = new CodeTypeDeclaration("LogicalNames") { CustomAttributes = { new CodeAttributeDeclaration("DataContract") } };
                entityClass.Members.Add(logicalNames);

                var relationships = new CodeTypeDeclaration("Relationships") { CustomAttributes = { new CodeAttributeDeclaration("DataContract") } };
                entityClass.Members.Add(relationships);

                var props = entityClass.Members.Cast<CodeTypeMember>().OfType<CodeMemberProperty>();
                foreach (var prop in props)
                {
                    var logicalName = GetAttributeValues<AttributeLogicalNameAttribute>(prop.CustomAttributes)?.FirstOrDefault();
                    if (logicalName != null)
                        logicalNames.Members.Add(new CodeSnippetTypeMember($"{new string('\t', 3)}public static string {prop.Name} = \"{logicalName}\";"));

                    var relationshipSchema = GetAttributeValues<RelationshipSchemaNameAttribute>(prop.CustomAttributes)?.FirstOrDefault();
                    if (relationshipSchema != null)
                        relationships.Members.Add(new CodeSnippetTypeMember($"{new string('\t', 3)}public static string {prop.Name} = \"{relationshipSchema}\";"));

                    if (logicalName != null && relationshipSchema == null)
                    {
                        if (entityMetadata.Attributes == null)
                            continue;

                        var propertyLogicalName = GetAttributeValues<AttributeLogicalNameAttribute>(prop.CustomAttributes).First();
                        var enumAtt = entityMetadata.Attributes.FirstOrDefault(x => x.LogicalName == propertyLogicalName &&
                                new[] { AttributeTypeCode.Picklist, AttributeTypeCode.Status, AttributeTypeCode.State }.Any(y => y == x.AttributeType)) as EnumAttributeMetadata;

                        if (enumAtt != null)
                        {
                            var optionsSetName = "";
                            if (!UseDisplayNames)
                            {
                                optionsSetName = enumAtt.OptionSet.Name;
                            }
                            else if (enumAtt.OptionSet.IsGlobal ?? false)
                            {
                                optionsSetName = enumAtt.OptionSet.DisplayName();
                            }
                            else
                            {
                                optionsSetName = $"{entityMetadata?.DisplayName()}_{enumAtt.OptionSet.DisplayName()}";
                            }

                            prop.Type = new CodeTypeReference(optionsSetName + "?");
                            prop.GetStatements.Clear();

                            prop.GetStatements.Add(new CodeSnippetStatement($"{tabs}return ({optionsSetName}?)GetAttributeValue<OptionSetValue>(\"{enumAtt.LogicalName}\")?.Value;"));

                            if (prop.HasSet || AddSetters)
                            {
                                prop.SetStatements.Clear();
                                prop.SetStatements.AddRange(new[] {
                                    new CodeSnippetStatement($"{tabs}SetAttributeValue(\"{logicalName}\", nameof({prop.Name}), value.HasValue ? new OptionSetValue((int)value.Value) : null);"),
                                });
                            }

                            continue;
                        }

                        var baseType = prop.Type.BaseType;
                        baseType = baseType.Replace("System.Byte", "byte");
                        baseType = baseType.Replace("System.String", "string");

                        if (prop.Type.TypeArguments.Count > 0)
                        {
                            baseType = baseType.Replace("`1", "<" + prop.Type.TypeArguments[0].BaseType + ">");
                        }

                        if (prop.Type.ArrayRank != 0)
                            baseType += "[]";

                        prop.GetStatements.Clear();

                        var method = "AttributeValue";
                        if (prop.Type.BaseType.StartsWith("IEnumerable")) 
                            method += "s";

                        if (prop.Name == "Id")
                        {
                            prop.Attributes = MemberAttributes.New | MemberAttributes.Public;
                            prop.GetStatements.Add(new CodeSnippetStatement($"{tabs}return base.Id != default ? base.Id : Get{method}<{baseType}>(\"{logicalName}\");"));
                        }
                        else
                        {
                            var genericType = prop.Type.TypeArguments.Count > 0 ? prop.Type.TypeArguments[0].BaseType : baseType;
                            prop.GetStatements.Add(new CodeSnippetStatement($"{tabs}return Get{method}<{genericType}>(\"{logicalName}\");"));
                        }

                        if (prop.HasSet || AddSetters)
                        {
                            prop.SetStatements.Clear();
                            prop.SetStatements.Add(new CodeSnippetStatement($"{tabs}Set{method}(\"{logicalName}\", nameof({prop.Name}), value);"));

                            if (prop.Name == "Id")
                                prop.SetStatements.Add(new CodeSnippetStatement($"{tabs}base.Id = value;"));
                        }

                        continue;
                    }

                    var codeAttributes = GetCodeAttributeArguments<RelationshipSchemaNameAttribute>(prop.CustomAttributes);

                    string entityRole = string.Empty;
                    if (codeAttributes != null && codeAttributes.Count() > 1)
                    {
                        var erAttribute = codeAttributes.Skip(1).First();
                        var role = erAttribute.Value as CodeFieldReferenceExpression;
                        entityRole = $", EntityRole.{role.FieldName}";
                        erAttribute.Value = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("EntityRole"), role.FieldName);
                    }

                    if (logicalName == null && relationshipSchema != null)
                    {
                        var baseType = prop.Type.TypeArguments[0].BaseType.Replace(codeNamespace.Name + ".", "");
                        prop.Type = new CodeTypeReference(prop.Type.BaseType.Replace("System.Collections.Generic.", ""), new CodeTypeReference($"{baseType}"));
                        prop.GetStatements.Clear();
                        prop.GetStatements.Add(new CodeSnippetStatement($"{tabs}return GetRelatedEntities<{baseType}>(\"{relationshipSchema}\"{entityRole});"));

                        if (prop.HasSet)
                        {
                            prop.SetStatements.Clear();
                            prop.SetStatements.Add(new CodeSnippetStatement($"{tabs}SetRelatedEntities<{baseType}>(\"{relationshipSchema}\", nameof({prop.Name}), value{entityRole});"));
                        }

                        continue;
                    }

                    if (logicalName != null && relationshipSchema != null)
                    {
                        var baseType = prop.Type.BaseType.Replace(codeNamespace.Name + ".", "");
                        prop.Type = new CodeTypeReference(baseType);
                        prop.GetStatements.Clear();
                        prop.GetStatements.Add(new CodeSnippetStatement($"{tabs}return GetRelatedEntity<{baseType}>(\"{relationshipSchema}\"{entityRole});"));

                        if (prop.HasSet)
                        {
                            prop.SetStatements.Clear();
                            prop.SetStatements.Add(new CodeSnippetStatement($"{tabs}SetRelatedEntity<{baseType}>(\"{relationshipSchema}\", nameof({prop.Name}), value{entityRole});"));
                        }

                        continue;
                    }
                }
            }

            foreach (CodeNamespace ns in codeCompileUnit.Namespaces)
            {
                foreach (CodeTypeDeclaration type in ns.Types)
                {
                    for (var i = type.CustomAttributes.Count - 1; i >= 0; i--)
                    {
                        var catt = type.CustomAttributes[i];
                        catt.Name = catt.Name.Replace("System.Runtime.Serialization.", "").Replace("DataContractAttribute", "DataContract");
                        catt.Name = catt.Name.Replace("Microsoft.Xrm.Sdk.Client.", "");
                        if (catt.Name.Contains("GeneratedCodeAttribute"))
                            type.CustomAttributes.RemoveAt(i);
                    }

                    if (type.IsEnum == false)
                        continue;

                    var optionSet = metadata.OptionSets.FirstOrDefault(x => x.DisplayName() == type.Name) as OptionSetMetadata;
                    EnumAttributeMetadata enumAttributeMetadata = null;

                    if (optionSet == null)
                    {
                        var enumAttributeMetadatas = metadata.Entities.SelectMany(x => x?.Attributes?.OfType<EnumAttributeMetadata>()?.Where(y => x.DisplayName() + "_" + y.OptionSet.DisplayName() == type.Name) ?? Array.Empty<EnumAttributeMetadata>()); //?.FirstOrDefault();
                        enumAttributeMetadata = enumAttributeMetadatas.FirstOrDefault();
                        
                        optionSet = enumAttributeMetadata?.OptionSet;

                        if (optionSet == null)
                        {
                            continue;
                        }
                    }

                    foreach (var field in type.Members.Cast<CodeTypeMember>().OfType<CodeMemberField>())
                    {
                        var codeExpression = field.InitExpression as CodePrimitiveExpression;
                        if (codeExpression == null)
                            continue;

                        var val = (int)codeExpression.Value;

                        foreach (CodeAttributeDeclaration att in field.CustomAttributes)
                            att.Name = att.Name.Replace("System.Runtime.Serialization.", "").Replace("EnumMemberAttribute", "EnumMember");

                        var option = optionSet.Options.FirstOrDefault(x => x.Value.Value == val);
                        if (option == null)
                            continue;

                        var status = option as StatusOptionMetadata;
                        if (status != null && enumAttributeMetadata.LogicalName == "statuscode")
                        {
                            var parentEnt = metadata.Entities.First(x => x.LogicalName == enumAttributeMetadata.EntityLogicalName);
                            var stateOptionSet = parentEnt.Attributes.OfType<StateAttributeMetadata>().FirstOrDefault()?.OptionSet;
                            if (stateOptionSet == null)
                            {
                                continue;
                            }

                            var stateOption = stateOptionSet.Options.FirstOrDefault(x => x.Value.Value == status.State.Value);

                            var state = UseDisplayNames ? parentEnt.DisplayName() + "_" + stateOptionSet.DisplayName() : stateOptionSet.Name;

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

            // re-order alphabetical
            for (var i = 0; i < codeCompileUnit.Namespaces.Count; ++i)
            {
                var types = codeCompileUnit.Namespaces[i].Types;

                var typesCopy = new CodeTypeDeclaration[codeCompileUnit.Namespaces[i].Types.Count];
                codeCompileUnit.Namespaces[i].Types.CopyTo(typesCopy, 0);
                var orderedTypes = typesCopy.OrderBy(x => x.Name).ToArray();

                for (var j = 0; j < codeCompileUnit.Namespaces[i].Types.Count;)
                    codeCompileUnit.Namespaces[i].Types.RemoveAt(j);

                codeCompileUnit.Namespaces[i].Types.AddRange(orderedTypes);
            }

            //// move around
            //for (var i = 0; i < codeCompileUnit.Namespaces.Count; ++i)
            //{
            //    var types = codeCompileUnit.Namespaces[i].Types;

            //    for (var j = types.Count - 1; j >= 0; j--)
            //    {
            //        var type = types[j];

            //        if (!type.IsEnum)
            //        {
            //            continue;
            //        }

            //        var segments = type.Name.Split('_');

            //        if (segments.Count() <= 1)
            //            continue;

            //        Console.WriteLine($"T {type.Name}");
            //        var parent = types.Cast<CodeTypeDeclaration>().FirstOrDefault(x => x.IsClass && x.Name == segments[0]);

            //        if (parent != null)
            //        {
            //            type.Name = string.Join("_", segments.Skip(1));
            //            parent.Members.Add(type);

            //            types.Remove(type);
            //        }

            //    }
            //}

            var tab = new string('\t', 1);
            var twoTabs = new string('\t', 2);

            var codeType = new CodeTypeDeclaration("EarlyEntity")
            {
                TypeAttributes = TypeAttributes.Abstract | TypeAttributes.Public,
                CustomAttributes = {
                    new CodeAttributeDeclaration("DataContract"),
                    new CodeAttributeDeclaration("ExcludeFromCodeCoverage")
                }
            };
            codeType.BaseTypes.AddRange(new[]{
                new CodeTypeReference("Entity"),
                new CodeTypeReference("INotifyPropertyChanging"),
                new CodeTypeReference("INotifyPropertyChanged"),
            });

            codeType.Members.Add(new CodeSnippetTypeMember(twoTabs +
        @"public EarlyEntity(string entityLogicalName) : base(entityLogicalName) { }"));

            codeType.Members.AddRange(new[]{
                new CodeSnippetTypeMember(twoTabs +
        @"public event PropertyChangedEventHandler PropertyChanged;"),
                new CodeSnippetTypeMember(twoTabs +
        @"public event PropertyChangingEventHandler PropertyChanging;"),
                new CodeSnippetTypeMember(twoTabs +
        @"protected void OnPropertyChanged(string propertyName)
        {
            if ((PropertyChanged != null))
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }"),
                new CodeSnippetTypeMember(twoTabs +
        @"protected void OnPropertyChanging(string propertyName)
        {
            if ((PropertyChanging != null))
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }")
            });

            codeType.Members.AddRange(new[]{
                new CodeSnippetTypeMember(twoTabs +
        @"public IEnumerable<T> GetAttributeValues<T>(string attributeLogicalName) where T : Entity
	    {
		    return base.GetAttributeValue<EntityCollection>(attributeLogicalName)?.Entities?.Cast<T>();
	    }"),
                new CodeSnippetTypeMember(twoTabs +
        @"protected void SetAttributeValues<T>(string logicalName, string attributePropertyName, IEnumerable<T> value)  where T : Entity
        {
            SetAttributeValue(logicalName, attributePropertyName, new EntityCollection(new List<Entity>(value)));
        }"),
                new CodeSnippetTypeMember(twoTabs +
        @"protected void SetAttributeValue(string logicalName, string attributePropertyName, object value)
        {
            OnPropertyChanging(attributePropertyName);
            base.SetAttributeValue(logicalName, value);
            OnPropertyChanged(attributePropertyName);
        }"),
                new CodeSnippetTypeMember(twoTabs +
        @"protected new T GetRelatedEntity<T>(string relationshipSchemaName, EntityRole? primaryEntityRole = null) where T : Entity
        {
            return base.GetRelatedEntity<T>(relationshipSchemaName, primaryEntityRole);
        }"),
                new CodeSnippetTypeMember(twoTabs +
        @"protected void SetRelatedEntity<T>(string relationshipSchemaName, string attributePropertyName, T entity, EntityRole? primaryEntityRole = null) where T : Entity
        {
            OnPropertyChanging(attributePropertyName);
            base.SetRelatedEntity(relationshipSchemaName, primaryEntityRole, entity);
            OnPropertyChanged(attributePropertyName);
        }"),
                new CodeSnippetTypeMember(twoTabs +
        @"protected new IEnumerable<T> GetRelatedEntities<T>(string relationshipSchemaName, EntityRole? primaryEntityRole = null) where T : Entity
        {
            return base.GetRelatedEntities<T>(relationshipSchemaName, primaryEntityRole);
        }"),
                new CodeSnippetTypeMember(twoTabs +
        @"protected void SetRelatedEntities<T>(string relationshipSchemaName, string attributePropertyName, IEnumerable<T> entities, EntityRole? primaryEntityRole = null) where T : Entity
        {
            OnPropertyChanging(attributePropertyName);
            base.SetRelatedEntities(relationshipSchemaName, primaryEntityRole, entities);
            OnPropertyChanged(attributePropertyName);
        }"),
            });
            codeNamespace.Types.Add(codeType);
        }

        private IEnumerable<string> GetAttributeValues<T>(CodeAttributeDeclarationCollection customAttributes) where T : Attribute
        {
            var attribute = customAttributes.Cast<CodeAttributeDeclaration>().FirstOrDefault(y => y.Name.EndsWith(typeof(T).Name));
            if (attribute != null)
            {
                var ctr = attribute?.Arguments.Cast<CodeAttributeArgument>()?.Select(x => x.Value as CodePrimitiveExpression);
                return ctr?.Select(x => x.Value.ToString());
            }

            return null;
        }

        private IEnumerable<CodeAttributeArgument> GetCodeAttributeArguments<T>(CodeAttributeDeclarationCollection customAttributes) where T : Attribute
        {
            var attribute = customAttributes.Cast<CodeAttributeDeclaration>().FirstOrDefault(y => y.Name.EndsWith(typeof(T).Name));
            if (attribute == null)
                return null;

            return attribute?.Arguments.Cast<CodeAttributeArgument>();
        }
    }
}