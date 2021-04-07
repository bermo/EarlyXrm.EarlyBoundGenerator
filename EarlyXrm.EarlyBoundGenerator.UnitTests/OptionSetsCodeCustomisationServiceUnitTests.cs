using Microsoft.Crm.Services.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using NSubstitute;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarlyXrm.EarlyBoundGenerator.UnitTests
{
    [TestClass]
    public class OptionSetsCodeCustomisationServiceUnitTests : UnitTestBase
    {
        private IOrganizationMetadata organizationMetadata;
        private Dictionary<string, string> parameters;
        private INamingService namingService;

        private OptionSetsCodeCustomisationService sut;

        [TestInitialize]
        public void TestInitialise()
        {
            var metadataProviderService = Substitute.For<IMetadataProviderService>();
            organizationMetadata = Substitute.For<IOrganizationMetadata>();
            metadataProviderService.LoadMetadata().Returns(organizationMetadata);
            serviceProvider.GetService(typeof(IMetadataProviderService)).Returns(metadataProviderService);
            namingService = Substitute.For<INamingService>();
            serviceProvider.GetService(typeof(INamingService)).Returns(namingService);

            parameters = new Dictionary<string, string> {
                { "UseDisplayNames".ToUpper(), true.ToString() }
            };

            sut = new OptionSetsCodeCustomisationService(parameters);
        }

        [TestMethod]
        public void CustomizeCodeDom()
        {
            organizationMetadata.OptionSets.Returns(new OptionSetMetadataBase[] {
                new OptionSetMetadata { Name = "A", DisplayName = new Label("A", 1033) }
            });
            organizationMetadata.Entities.Returns(new[] {
                new EntityMetadata { 
                    LogicalName = "ee_test",
                    DisplayName = new Label("Test", 1033)
                }.Set(x => x.Attributes, new EnumAttributeMetadata []
                    {
                        new StateAttributeMetadata {
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
                            new CodeTypeDeclaration { IsEnum = false, Name = "C" },
                            new CodeTypeDeclaration { IsEnum = true, Name = "B",
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

            sut.CustomizeCodeDom(codeCompileUnit, serviceProvider);

            var ns = codeCompileUnit.Namespaces.Cast<CodeNamespace>().First();
            var types = ns.Types.OfType<CodeTypeDeclaration>();

            var firstType = types.First();
            Assert.AreEqual("A", firstType.Name);

            var lastType = types.Last();
            Assert.AreEqual("Test_B", lastType.Name);
            var valMember = lastType.Members.Cast<CodeTypeMember>().First(x => x.Name == "Val");
            var customAtt = valMember.CustomAttributes.Cast<CodeAttributeDeclaration>().FirstOrDefault(x => x.Name == "AmbientValue");
            var firstArg = customAtt.Arguments.Cast<CodeAttributeArgument>().First().Value as CodeFieldReferenceExpression;
            Assert.AreEqual("Test_B", (firstArg.TargetObject as CodeTypeReferenceExpression).Type.BaseType);
            Assert.AreEqual("Blah", firstArg.FieldName);
        }
    }
}