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
    public class EntitiesCodeFilteringServiceUnitTests : UnitTestBase
    {
        private Dictionary<string, string> parameters;
        private EntitiesCodeFilteringService sut;
        private IOrganizationMetadata organizationMetadata;
        private IMetadataProviderService metadataProviderService;
        private ICodeWriterFilterService codeWriterFilterService;

        [TestInitialize]
        public void TestInitialise()
        {
            metadataProviderService = Substitute.For<IMetadataProviderService>();
            organizationMetadata = Substitute.For<IOrganizationMetadata>();
            metadataProviderService.LoadMetadata().Returns(organizationMetadata);
            serviceProvider.GetService(typeof(IMetadataProviderService)).Returns(metadataProviderService);

            codeWriterFilterService = Substitute.For<ICodeWriterFilterService>();
            codeWriterFilterService.GenerateAttribute(Arg.Any<AttributeMetadata>(), serviceProvider).Returns(true);
            parameters = new Dictionary<string, string> {};

            sut = new EntitiesCodeFilteringService(codeWriterFilterService, parameters);
        }

        [TestMethod]
        public void GenerateEntity()
        {
            var id = Guid.NewGuid();
            organizationMetadata.Entities.Returns(new[] {
                new EntityMetadata { LogicalName = "ee_test", MetadataId = id, DisplayName = new Label("Test", 1033) }
            });

            var entityMetadata = new EntityMetadata { LogicalName = "ee_test" };
            SolutionHelper.organisationService.RetrieveMultiple(Arg.Any<QueryExpression>())
                .Returns(new EntityCollection(new List<Entity> { new Entity("solutioncomponent") { Attributes = { {"objectid", id } } } }));

            var result = sut.GenerateEntity(entityMetadata, serviceProvider);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void GenerateAttribute()
        {
            var id = Guid.NewGuid();
            var testId = Guid.NewGuid();

            var attributeMetadata = new StringAttributeMetadata { 
                LogicalName = "ee_testid",
                MetadataId = testId,
                DisplayName = new Label("Test Id", 1033) 
            }.Set(x => x.EntityLogicalName, "ee_test");

            organizationMetadata.Entities.Returns(new[] {
                new EntityMetadata { LogicalName = "ee_test", MetadataId = id, DisplayName = new Label("Test", 1033) }
                    .Set(x => x.Attributes, new[] { attributeMetadata })
            });

            SolutionHelper.organisationService.RetrieveMultiple(Arg.Any<QueryExpression>())
                .Returns(new EntityCollection(new List<Entity> { 
                    new Entity("solutioncomponent") { Attributes = { { "objectid", id }, { "componenttype", 1 } } },
                    new Entity("solutioncomponent") { Attributes = { { "objectid", testId }, { "componenttype", 2 } } }
                }));

            var result = sut.GenerateAttribute(attributeMetadata, serviceProvider);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void GenerateRelationshipReturnsFalse()
        {
            codeWriterFilterService.GenerateRelationship(Arg.Any<RelationshipMetadataBase>(), Arg.Any<EntityMetadata>(), serviceProvider).Returns(true);

            var id = Guid.NewGuid();
            var testId = Guid.NewGuid();
            organizationMetadata.Entities.Returns(new[] {
                new EntityMetadata { LogicalName = "ee_test", MetadataId = id }
            });

            var relationshipMetadata = new OneToManyRelationshipMetadata
            {
                ReferencedEntity = "ee_test",
                ReferencingEntity = "ee_test",
                ReferencingAttribute = "ee_val"
            };
            organizationMetadata.Entities.Returns(new[] {
                new EntityMetadata { LogicalName = "ee_test", MetadataId = id, DisplayName = new Label("Test", 1033) }
            });

            SolutionHelper.organisationService.RetrieveMultiple(Arg.Any<QueryExpression>())
                .Returns(new EntityCollection(new List<Entity> {
                    new Entity("solutioncomponent") { Attributes = { { "objectid", id }, { "componenttype", 1 } } },
                    new Entity("solutioncomponent") { Attributes = { { "objectid", testId }, { "componenttype", 2 } } }
                }));

            var result = sut.GenerateRelationship(relationshipMetadata, new EntityMetadata(), serviceProvider);

            Assert.IsFalse(result);
        }
    }
}