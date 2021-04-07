using Microsoft.Crm.Services.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using NSubstitute;
using System;
using System.Collections.Generic;

namespace EarlyXrm.EarlyBoundGenerator.UnitTests
{
    [TestClass]
    public class OptionSetsFilteringServiceUnitTests : UnitTestBase
    {
        private OptionSetsFilteringService sut;
        private ICodeWriterFilterService codeWriterFilterService;
        private IOrganizationMetadata organizationMetadata;
        private IMetadataProviderService metadataProviderService;

        [TestInitialize]
        public void TestInitialise()
        {
            metadataProviderService = Substitute.For<IMetadataProviderService>();
            organizationMetadata = Substitute.For<IOrganizationMetadata>();
            metadataProviderService.LoadMetadata().Returns(organizationMetadata);
            serviceProvider.GetService(typeof(IMetadataProviderService)).Returns(metadataProviderService);
            codeWriterFilterService = Substitute.For<ICodeWriterFilterService>();

            sut = new OptionSetsFilteringService(codeWriterFilterService);
        }

        [TestMethod]
        public void GenerateOptionSet()
        {
            var optionSetMetadata = new OptionSetMetadata
            {
                IsGlobal = false,
                IsCustomOptionSet = false,
                OptionSetType = OptionSetType.Picklist,
                Name = "ee_TestOpt"
            };
            var id = Guid.NewGuid();
            var attId = Guid.NewGuid();
            var attributeMetadata = new PicklistAttributeMetadata
            {
                LogicalName = "ee_testval",
                MetadataId = attId,
                DisplayName = new Label("Test Val", 1033),
                OptionSet = optionSetMetadata
            }.Set(x => x.EntityLogicalName, "ee_test");
            
            organizationMetadata.Entities.Returns(new[] {
                new EntityMetadata { LogicalName = "ee_test", MetadataId = id, DisplayName = new Label("Test", 1033) }
                    .Set(x => x.Attributes, new[] { attributeMetadata })
            });

            SolutionHelper.organisationService.RetrieveMultiple(Arg.Any<QueryExpression>())
                .Returns(new EntityCollection(new List<Entity> {
                    new Entity("solutioncomponent") { Attributes = { { "objectid", id }, { "componenttype", 1 } } },
                    new Entity("solutioncomponent") { Attributes = { { "objectid", attId }, { "name", "ee_testval" }, { "componenttype", 2 } } }
                }));

            var result = sut.GenerateOptionSet(optionSetMetadata, serviceProvider);

            Assert.IsTrue(result);
        }
    }
}