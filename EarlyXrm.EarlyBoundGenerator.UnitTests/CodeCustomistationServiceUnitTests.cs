using Microsoft.Crm.Services.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;
using NSubstitute;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;

namespace EarlyXrm.EarlyBoundGenerator.UnitTests
{
    [TestClass]
    public class CodeCustomistationServiceUnitTests : UnitTestBase
    {
        private Dictionary<string, string> parameters;
        private IOrganizationMetadata organizationMetadata;

        [TestInitialize]
        public void TestInitialise()
        {
            var metadataProviderService = Substitute.For<IMetadataProviderService>();
            organizationMetadata = Substitute.For<IOrganizationMetadata>();
            metadataProviderService.LoadMetadata().Returns(organizationMetadata);
            serviceProvider.GetService(typeof(IMetadataProviderService)).Returns(metadataProviderService);

            parameters = new Dictionary<string, string> {
                { "UseDisplayNames", false.ToString() },
                { "Instrument", false.ToString() },
                { "AddSetters", false.ToString() },
                { "NestNonGlobalEnums", true.ToString() },
                { "GenerateConstants", true.ToString() }
            };
        }

        [TestMethod]
        public void CommentsAreRemovedFromConstructorAsExpected()
        {
            organizationMetadata.Entities.Returns(new[] {
                new EntityMetadata { LogicalName = "ee_test" }
            });

            var sut = new CodeCustomistationService(parameters);
            var codeCompileUnit = new CodeCompileUnit
            {
                Namespaces = {
                    new CodeNamespace("EarlyTest")
                    {
                        Types = {
                            new CodeTypeDeclaration {
                                Name = "Test",
                                CustomAttributes = { Build<EntityLogicalNameAttribute>("ee_test") },
                                Members = {
                                    new CodeConstructor {
                                        Comments = {
                                            new CodeCommentStatement("Comment")
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            sut.CustomizeCodeDom(codeCompileUnit, serviceProvider);

            var ns = codeCompileUnit.Namespaces.Cast<CodeNamespace>().First();
            var @class = ns.Types.OfType<CodeTypeDeclaration>().First();
            var constructor = @class.Members.OfType<CodeConstructor>().First();
            Assert.IsTrue(constructor.Comments.Cast<CodeCommentStatement>().SequenceEqual(new CodeCommentStatement[] { }));
        }

        [TestMethod]
        public void ChangingMethodsAreRemovedAsExpected()
        {
            organizationMetadata.Entities.Returns(new[] {
                new EntityMetadata { LogicalName = "ee_test" }
            });

            var sut = new CodeCustomistationService(parameters);
            var codeCompileUnit = new CodeCompileUnit
            {
                Namespaces = {
                    new CodeNamespace("EarlyTest")
                    {
                        Types = {
                            new CodeTypeDeclaration {
                                Name = "Test",
                                CustomAttributes = { Build<EntityLogicalNameAttribute>("ee_test") },
                                Members = {
                                    new CodeMemberMethod
                                    {
                                        Name = "OnPropertyChanging"
                                    },
                                    new CodeMemberMethod
                                    {
                                        Name = "OnPropertyChanged"
                                    }
                                }
                            }
                        }
                    }
                }
            };
            sut.CustomizeCodeDom(codeCompileUnit, serviceProvider);

            var ns = codeCompileUnit.Namespaces.Cast<CodeNamespace>().First();
            var @class = ns.Types.OfType<CodeTypeDeclaration>().First();
            Assert.IsFalse(@class.Members.OfType<CodeMemberMethod>().Any());
        }

        [TestMethod]
        public void EventsAreRemovedAsExpected()
        {
            organizationMetadata.Entities.Returns(new[] {
                new EntityMetadata { LogicalName = "ee_test" }
            });

            var sut = new CodeCustomistationService(parameters);
            var codeCompileUnit = new CodeCompileUnit
            {
                Namespaces = {
                    new CodeNamespace("EarlyTest")
                    {
                        Types = {
                            new CodeTypeDeclaration {
                                Name = "Test",
                                CustomAttributes = { Build<EntityLogicalNameAttribute>("ee_test") },
                                Members = {
                                    new CodeMemberEvent()
                                }
                            }
                        }
                    }
                }
            };
            sut.CustomizeCodeDom(codeCompileUnit, serviceProvider);

            var ns = codeCompileUnit.Namespaces.Cast<CodeNamespace>().First();
            var @class = ns.Types.OfType<CodeTypeDeclaration>().First();
            Assert.IsFalse(@class.Members.OfType<CodeMemberEvent>().Any());
        }

        [TestMethod]
        public void FieldsAreRemovedAsExpected()
        {
            organizationMetadata.Entities.Returns(new[] {
                new EntityMetadata { LogicalName = "ee_test" }
            });

            var sut = new CodeCustomistationService(parameters);
            var codeCompileUnit = new CodeCompileUnit
            {
                Namespaces = {
                    new CodeNamespace("EarlyTest")
                    {
                        Types = {
                            new CodeTypeDeclaration {
                                Name = "Test",
                                CustomAttributes = { Build<EntityLogicalNameAttribute>("ee_test") },
                                Members = {
                                    new CodeMemberField{ Name = "EntityTypeCode" }
                                }
                            }
                        }
                    }
                }
            };
            sut.CustomizeCodeDom(codeCompileUnit, serviceProvider);

            var ns = codeCompileUnit.Namespaces.Cast<CodeNamespace>().First();
            var @class = ns.Types.OfType<CodeTypeDeclaration>().First();
            Assert.IsFalse(@class.Members.OfType<CodeMemberField>().Any());
        }

        [TestMethod]
        public void CodeTypeMemberBaseIsNotProcessed()
        {
            organizationMetadata.Entities.Returns(new[] {
                new EntityMetadata { LogicalName = "ee_test" }
            });

            var sut = new CodeCustomistationService(parameters);
            var codeCompileUnit = new CodeCompileUnit
            {
                Namespaces = {
                    new CodeNamespace("EarlyTest")
                    {
                        Types = {
                            new CodeTypeDeclaration {
                                Name = "Test",
                                CustomAttributes = { Build<EntityLogicalNameAttribute>("ee_test") },
                                Members = {
                                    new CodeTypeMember()
                                }
                            }
                        }
                    }
                }
            };
            sut.CustomizeCodeDom(codeCompileUnit, serviceProvider);

            var ns = codeCompileUnit.Namespaces.Cast<CodeNamespace>().First();
            var @class = ns.Types.OfType<CodeTypeDeclaration>().First();
            Assert.IsTrue(@class.Members.OfType<CodeTypeMember>().Any());
        }

        [TestMethod]
        public void WhenParameterIsSet_EnumAttributeUsesDisplayName()
        {
            INamingService namingService = Substitute.For<INamingService>();
            serviceProvider.GetService(typeof(INamingService)).Returns(namingService);
            organizationMetadata.Entities.Returns(new[] {
                new EntityMetadata { LogicalName = "ee_test", DisplayName = new Label("Test", 1033) }
                    .Set(x => x.Attributes, new[] {
                        new PicklistAttributeMetadata { LogicalName = "ee_colour", DisplayName = new Label("Colour", 1033),
                            OptionSet = new OptionSetMetadata {
                                Name = "Colour",
                                IsGlobal = false,
                                DisplayName = new Label("Colour", 1033),
                                Options = { 
                                    new OptionMetadata() 
                                } 
                            }
                        }
                    })
            });
            parameters["UseDisplayNames"] = true.ToString();
            var sut = new CodeCustomistationService(parameters);
            var codeCompileUnit = new CodeCompileUnit
            {
                Namespaces = {
                    new CodeNamespace("EarlyTest")
                    {
                        Types = {
                            new CodeTypeDeclaration {
                                CustomAttributes = { Build<EntityLogicalNameAttribute>("ee_test") },
                                Members = {
                                    new CodeMemberProperty
                                    {
                                        HasSet = true,
                                        Name = "Colour",
                                        CustomAttributes = { Build<AttributeLogicalNameAttribute>("ee_colour") },
                                    }
                                }
                            },
                            new CodeTypeDeclaration
                            {
                                IsEnum = true,
                                Name = "Test_Colour",
                                Members = {}
                            }
                        }
                    }
                }
            };
            namingService.GetNameForOptionSet(Arg.Any<EntityMetadata>(), Arg.Any<OptionSetMetadataBase>(), serviceProvider)
                .Returns("Test_Colour");
            sut.CustomizeCodeDom(codeCompileUnit, serviceProvider);

            var ns = codeCompileUnit.Namespaces.Cast<CodeNamespace>().First();
            var @class = ns.Types.OfType<CodeTypeDeclaration>().First();
            var property = @class.Members.OfType<CodeMemberProperty>().First();
            Assert.AreEqual("Enums.Colour?", property.Type.BaseType);
            var getStatement = property.GetStatements.Cast<CodeSnippetStatement>().First();
            Assert.IsTrue(getStatement.Value.Contains("return (Enums.Colour?)GetAttributeValue<OptionSetValue>(\"ee_colour\")?.Value;"));
        }

        [TestMethod]
        public void ParentChildRelationshipBuiltAsExpected()
        {
            var entity = new EntityMetadata { 
                LogicalName = "ee_test", 
                DisplayName = new Label("Test", 1033)
            }
            .Set(x => x.ManyToManyRelationships, Array.Empty<ManyToManyRelationshipMetadata>());
            var manyToOne = new OneToManyRelationshipMetadata
            {
                ReferencedEntity = entity.LogicalName
            };
            organizationMetadata.Entities.Returns(new[] {
                entity.Set(x => x.OneToManyRelationships, new[] { manyToOne })
            });
            parameters["UseDisplayNames"] = true.ToString();
            var sut = new CodeCustomistationService(parameters);
            var codeCompileUnit = new CodeCompileUnit
            {
                Namespaces = {
                    new CodeNamespace("EarlyTest")
                    {
                        Types = {
                            new CodeTypeDeclaration {
                                CustomAttributes = { Build<EntityLogicalNameAttribute>("ee_test") },
                                Members = {
                                    new CodeMemberProperty
                                    {
                                        HasSet = true,
                                        Type = new CodeTypeReference("System.Collections.Generic.IEnumerable",new CodeTypeReference("TestProp")),
                                        Name = "Relationship",
                                        CustomAttributes = { Build<RelationshipSchemaNameAttribute>(
                                            new CodePrimitiveExpression("ee_relationship"),
                                            new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("EntityRole"), EntityRole.Referenced.ToString() ))
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            sut.CustomizeCodeDom(codeCompileUnit, serviceProvider);

            var ns = codeCompileUnit.Namespaces.Cast<CodeNamespace>().First();
            var @class = ns.Types.OfType<CodeTypeDeclaration>().First();
            var property = @class.Members.OfType<CodeMemberProperty>().First();
            Assert.AreEqual("IEnumerable`1", property.Type.BaseType);
            Assert.AreEqual("TestProp", property.Type.TypeArguments.Cast<CodeTypeReference>().First().BaseType);
            Assert.AreEqual("Referenced", (property.CustomAttributes.Cast<CodeAttributeDeclaration>().First().Arguments.Cast<CodeAttributeArgument>().Last().Value as CodeFieldReferenceExpression).FieldName);
            var getStatement = property.GetStatements.Cast<CodeSnippetStatement>().First();
            Assert.IsTrue(getStatement.Value.Contains("EntityRole.Referenced"));
        }

        [TestMethod]
        public void EntityIsModifiedAsExpected()
        {
            organizationMetadata.Entities.Returns(new[] {
                new EntityMetadata { LogicalName = "ee_test" }
                    .Set(x => x.Attributes, new[] {
                        new StringAttributeMetadata { LogicalName = "ee_testid", DisplayName = new Label("Test Id", 1033) }
                    })
            });

            var sut = new CodeCustomistationService(parameters);
            var codeCompileUnit = new CodeCompileUnit
            {
                Namespaces = {
                    new CodeNamespace("EarlyTest")
                    {
                        Types = {
                            new CodeTypeDeclaration {
                                CustomAttributes = { Build<EntityLogicalNameAttribute>("ee_test") },
                                Members = {
                                    new CodeMemberProperty
                                    {
                                        Name = "ee_testid",
                                        CustomAttributes = { Build<AttributeLogicalNameAttribute>("ee_testid") },
                                        Type = new CodeTypeReference("System.Guid"),
                                        Comments = {
                                            new CodeCommentStatement("Comment")
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            sut.CustomizeCodeDom(codeCompileUnit, serviceProvider);

            var ns = codeCompileUnit.Namespaces.Cast<CodeNamespace>().First();
            var @class = ns.Types.OfType<CodeTypeDeclaration>().First();
            var property = @class.Members.OfType<CodeMemberProperty>().First();
            Assert.AreEqual("ee_testid", property.Name);
            Assert.AreEqual("Guid", property.Type.BaseType);
            Assert.IsTrue(!ns.Comments.Cast<CodeCommentStatement>().Any());
        }

        [TestMethod]
        public void EntityPointingToAnotherEntityIsBuiltExpected()
        {
            organizationMetadata.Entities.Returns(new[] {
                new EntityMetadata { LogicalName = "ee_test" }
                    .Set(x => x.Attributes, new AttributeMetadata[] {
                        new StringAttributeMetadata { LogicalName = "ee_testid", DisplayName = new Label("Test Id", 1033) },
                        new LookupAttributeMetadata { LogicalName = "ee_testpropid", DisplayName = new Label("Test Prop Id", 1033) }
                    })
            });

            var sut = new CodeCustomistationService(parameters);
            var codeCompileUnit = new CodeCompileUnit
            {
                Namespaces = {
                    new CodeNamespace("EarlyTest")
                    {
                        Types = {
                            new CodeTypeDeclaration {
                                Name = "Test",
                                CustomAttributes = { Build<EntityLogicalNameAttribute>("ee_test") },
                                Members = {
                                    new CodeMemberProperty
                                    {
                                        Name = "TestProp", HasSet = true,
                                        CustomAttributes = { Build<AttributeLogicalNameAttribute>("ee_testpropid") },
                                        Type = new CodeTypeReference("EarlyTest.TestProp"),
                                    }
                                }
                            }
                        }
                    }
                }
            };
            sut.CustomizeCodeDom(codeCompileUnit, serviceProvider);

            var ns = codeCompileUnit.Namespaces.Cast<CodeNamespace>().First();
            var test = ns.Types.OfType<CodeTypeDeclaration>().First(x => x.Name == "Test");
            var property = test.Members.OfType<CodeMemberProperty>().First();
            Assert.AreEqual("TestProp", property.Name);
            Assert.AreEqual("TestProp", property.Type.BaseType);
        }

        [TestMethod]
        public void NullableAttributeIsBuiltExpected()
        {
            organizationMetadata.Entities.Returns(new[] { new EntityMetadata { LogicalName = "ee_test" }});

            var sut = new CodeCustomistationService(parameters);
            var codeCompileUnit = new CodeCompileUnit
            {
                Namespaces = {
                    new CodeNamespace("EarlyTest")
                    {
                        Types = {
                            new CodeTypeDeclaration {
                                Name = "Test",
                                CustomAttributes = { Build<EntityLogicalNameAttribute>("ee_test") },
                                Members = {
                                    new CodeMemberProperty
                                    {
                                        Type = new CodeTypeReference("System.String")
                                    },
                                    new CodeMemberProperty
                                    {
                                        Type = new CodeTypeReference("System.Nullable`1", new CodeTypeReference("System.Int32"))
                                    },
                                    new CodeMemberProperty
                                    {
                                        Type = new CodeTypeReference("System.Nullable`1", new CodeTypeReference("System.Int64"))
                                    },
                                    new CodeMemberProperty
                                    {
                                        Type = new CodeTypeReference("System.Nullable`1", new CodeTypeReference("System.Boolean"))
                                    },
                                    new CodeMemberProperty
                                    {
                                        Type = new CodeTypeReference("System.Nullable`1", new CodeTypeReference("System.DateTime"))
                                    },
                                    new CodeMemberProperty
                                    {
                                        Type = new CodeTypeReference("System.Nullable`1", new CodeTypeReference("System.Double"))
                                    },
                                    new CodeMemberProperty
                                    {
                                        Type = new CodeTypeReference("System.Nullable`1", new CodeTypeReference("System.Decimal"))
                                    },
                                    new CodeMemberProperty
                                    {
                                        Type = new CodeTypeReference("System.Nullable`1", new CodeTypeReference("System.Guid"))
                                    }
                                }
                            }
                        }
                    }
                }
            };
            sut.CustomizeCodeDom(codeCompileUnit, serviceProvider);

            var ns = codeCompileUnit.Namespaces.Cast<CodeNamespace>().First();
            var test = ns.Types.OfType<CodeTypeDeclaration>().First(x => x.Name == "Test");
            var properties = test.Members.OfType<CodeMemberProperty>().Select(x => x.Type.BaseType);
            Assert.IsTrue(properties.SequenceEqual(new[] { "System.String", "int?", "long?", "bool?", "DateTime?", "double?", "decimal?", "Guid?" }));
        }

        [TestMethod]
        public void GenericCollectionOfXrmSdk()
        {
            organizationMetadata.Entities.Returns(new[] { new EntityMetadata { LogicalName = "ee_test" } });

            var sut = new CodeCustomistationService(parameters);
            var codeCompileUnit = new CodeCompileUnit
            {
                Namespaces = {
                    new CodeNamespace("EarlyTest")
                    {
                        Types = {
                            new CodeTypeDeclaration {
                                Name = "Test",
                                CustomAttributes = { Build<EntityLogicalNameAttribute>("ee_test") },
                                Members = {
                                    new CodeMemberProperty
                                    {
                                        Type = new CodeTypeReference("System.Collections.Generic.List", new CodeTypeReference("Microsoft.Xrm.Sdk.Blah"))
                                    }
                                }
                            }
                        }
                    }
                }
            };
            sut.CustomizeCodeDom(codeCompileUnit, serviceProvider);

            var ns = codeCompileUnit.Namespaces.Cast<CodeNamespace>().First();
            var test = ns.Types.OfType<CodeTypeDeclaration>().First(x => x.Name == "Test");
            var member = test.Members.OfType<CodeMemberProperty>().First();
            Assert.AreEqual("List`1", member.Type.BaseType);
            Assert.AreEqual("Blah", member.Type.TypeArguments[0].BaseType);
        }

        [TestMethod]
        public void RemovesExtraDefaultIdAttribute()
        {
            organizationMetadata.Entities.Returns(new[] {
                new EntityMetadata { LogicalName = "ee_test" }
                    .Set(x => x.Attributes, new AttributeMetadata[] {
                        new UniqueIdentifierAttributeMetadata { LogicalName = "ee_testid" }.Set(x => x.IsPrimaryId, true)
                    })
            });

            var sut = new CodeCustomistationService(parameters);
            var codeCompileUnit = new CodeCompileUnit
            {
                Namespaces = {
                    new CodeNamespace("EarlyTest")
                    {
                        Types = {
                            new CodeTypeDeclaration {
                                Name = "Test",
                                CustomAttributes = { Build<EntityLogicalNameAttribute>("ee_test") },
                                Members = {
                                    new CodeMemberProperty
                                    {
                                        CustomAttributes = { Build<AttributeLogicalNameAttribute>("ee_testid") },
                                        Type = new CodeTypeReference("System.Nullable`1", new CodeTypeReference("System.Guid"))
                                    }
                                }
                            }
                        }
                    }
                }
            };
            sut.CustomizeCodeDom(codeCompileUnit, serviceProvider);

            var ns = codeCompileUnit.Namespaces.Cast<CodeNamespace>().First();
            var test = ns.Types.OfType<CodeTypeDeclaration>().First(x => x.Name == "Test");
            Assert.IsFalse(test.Members.OfType<CodeMemberProperty>().Any());
        }

        [TestMethod]
        public void CustomizeCodeDom()
        {
            INamingService namingService = Substitute.For<INamingService>();
            serviceProvider.GetService(typeof(INamingService)).Returns(namingService);
            organizationMetadata.OptionSets.Returns(new OptionSetMetadataBase[] {
                new OptionSetMetadata { 
                    Name = "Test_B", DisplayName = new Label("Test B", 1033),
                    Options =
                    {
                        new OptionMetadata(new Label("Opt", 1033), 1)
                    }
                }
            });
            organizationMetadata.Entities.Returns(new[] {
                new EntityMetadata {
                    LogicalName = "ee_test",
                    DisplayName = new Label("Test", 1033)
                }
                .Set(x => x.Attributes, new AttributeMetadata []
                    {
                        new StateAttributeMetadata {
                            LogicalName = "statuscode",
                            OptionSet = new OptionSetMetadata{
                                Name = "B",
                                DisplayName = new Label("B", 1033),
                                Options =
                                {
                                    new StatusOptionMetadata{ State = 1, Value = 1 }
                                }
                            }
                        }.Set(x => x.EntityLogicalName, "ee_test")
                    })
            });

            var codeCompileUnit = new CodeCompileUnit
            {
                AssemblyCustomAttributes = {
                    new CodeAttributeDeclaration(new CodeTypeReference("Microsoft.Xrm.Sdk.Client.ProxyTypesAssemblyAttribute"))
                },
                Namespaces =
                {
                    new CodeNamespace("Test"){
                        Types =
                        {
                            new CodeTypeDeclaration { 
                                CustomAttributes = { Build<EntityLogicalNameAttribute>("ee_test") },
                                Members =
                                {
                                    new CodeMemberProperty
                                    {
                                        CustomAttributes = { Build<AttributeLogicalNameAttribute>("statuscode") }
                                    }
                                }
                            },
                            new CodeTypeDeclaration { IsEnum = false, Name = "C" },
                            new CodeTypeDeclaration { IsEnum = true, Name = "Test_B",
                                Members =
                                {
                                    new CodeMemberField()
                                    {
                                        Name = "Val",
                                        InitExpression = new CodePrimitiveExpression{ Value = 1 }
                                    }
                                }
                            },
                            new CodeTypeDeclaration { IsEnum = true, Name = "A" }
                        }
                    }
                }
            };
            namingService.GetNameForOption(Arg.Any<OptionSetMetadataBase>(), Arg.Any<OptionMetadata>(), serviceProvider)
                .Returns("Blah");
            namingService.GetNameForOptionSet(Arg.Any<EntityMetadata>(), Arg.Any<OptionSetMetadataBase>(), serviceProvider)
                .Returns("Test_B");

            var sut = new CodeCustomistationService(parameters);
            sut.CustomizeCodeDom(codeCompileUnit, serviceProvider);

            var ns = codeCompileUnit.Namespaces.Cast<CodeNamespace>().First();
            var types = ns.Types.OfType<CodeTypeDeclaration>();

            var aType = types.First(x => x.Name == "A");
            Assert.IsNotNull(aType);

            var bType = types.First(x => x.Name == "Test_B");
            var valMember = bType.Members.Cast<CodeTypeMember>().First(x => x.Name == "Val");
            var customAtt = valMember.CustomAttributes.Cast<CodeAttributeDeclaration>().FirstOrDefault(x => x.Name == "AmbientValue");
            var firstArg = customAtt.Arguments.Cast<CodeAttributeArgument>().First().Value as CodeFieldReferenceExpression;
            Assert.AreEqual("Test_B", (firstArg.TargetObject as CodeTypeReferenceExpression).Type.BaseType);
            Assert.AreEqual("Blah", firstArg.FieldName);
        }
    }
}