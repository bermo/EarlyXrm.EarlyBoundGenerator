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
using System.Linq.Expressions;
using System.Reflection;

namespace EarlyXrm.EarlyBoundGenerator.UnitTests
{
    [TestClass]
    public class EntitiesCodeCustomistationServiceUnitTests
    {
        private Dictionary<string, string> parameters;
        private IServiceProvider serviceProvider;
        private IOrganizationMetadata organizationMetadata;

        [TestInitialize]
        public void TestInitialise()
        {
            typeof(SolutionHelper)
                .GetField("organisationMetadata", BindingFlags.Static | BindingFlags.NonPublic)
                .SetValue(null, null);

            serviceProvider = Substitute.For<IServiceProvider>();
            var metadataProviderService = Substitute.For<IMetadataProviderService>();
            organizationMetadata = Substitute.For<IOrganizationMetadata>();
            metadataProviderService.LoadMetadata().Returns(organizationMetadata);
            serviceProvider.GetService(typeof(IMetadataProviderService)).Returns(metadataProviderService);

            parameters = new Dictionary<string, string> {
                { "UseDisplayNames".ToUpper(), false.ToString() },
                { "Instrument".ToUpper(), false.ToString() }
            };
        }

        [TestMethod]
        public void CommentsAreRemovedFromConstructorAsExpected()
        {
            organizationMetadata.Entities.Returns(new[] {
                new EntityMetadata { LogicalName = "ee_test" }
            });

            var sut = new EntitiesCodeCustomistationService(parameters);
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
        public void WhenParameterIsSet_EnumAttributeUsesDisplayName()
        {
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

            parameters["UseDisplayNames".ToUpper()] = true.ToString();
            var sut = new EntitiesCodeCustomistationService(parameters);
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
                                        Name = "Colour",
                                        CustomAttributes = { Build<AttributeLogicalNameAttribute>("ee_colour") },
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
            Assert.AreEqual("Test_Colour?", property.Type.BaseType);
            var getStatement = property.GetStatements.Cast<CodeSnippetStatement>().First();
            Assert.IsTrue(getStatement.Value.Contains("return (Test_Colour?)GetAttributeValue<OptionSetValue>(\"ee_colour\")?.Value;"));
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

            var sut = new EntitiesCodeCustomistationService(parameters);
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

            var sut = new EntitiesCodeCustomistationService(parameters);
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

            //var testProp = ns.Types.OfType<CodeTypeDeclaration>().First(x => x.Name == "TestProp");
        }

        private CodeAttributeDeclaration Build<T>(params string [] args) where T : Attribute
        {
            var cad = new CodeAttributeDeclaration
            {
                Name = typeof(T).Name,
                Arguments = {}
            };
            foreach (var arg in args)
            {
                cad.Arguments.Add(new CodeAttributeArgument
                {
                    Value = new CodePrimitiveExpression
                    {
                        Value = arg
                    }
                });
            }
            return cad;
        }
    }

    public static class Extensions
    {
        public static T Set<T, U>(this T t, Expression<Func<T, U>> prop, U val)
        {

            var me = prop.Body as MemberExpression;
            var pi = me.Member as PropertyInfo;

            typeof(T).GetProperty(pi.Name).SetValue(t, val);

            return t;
        }
    }
}