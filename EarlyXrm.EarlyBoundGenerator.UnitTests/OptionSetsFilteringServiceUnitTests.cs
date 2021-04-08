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
        public void GenerateGlobalOptionSetNoMatch()
        {
            var optionSetMetadata = new OptionSetMetadata
            {
                IsGlobal = true,
                OptionSetType = OptionSetType.Picklist,
                Name = "ee_TestOpt"
            };
            var id = Guid.NewGuid();
            var attId = Guid.NewGuid();
            var attributeMetadata = new PicklistAttributeMetadata
            {
                MetadataId = attId,
                OptionSet = optionSetMetadata
            }.Set(x => x.EntityLogicalName, "ee_test");

            organizationMetadata.Entities.Returns(new[] {
                new EntityMetadata {
                    LogicalName = "ee_test",  MetadataId = id,
                }
                .Set(x => x.Attributes, new[] { attributeMetadata })
            });

            SolutionHelper.organisationService.RetrieveMultiple(Arg.Any<QueryExpression>())
                .Returns(new EntityCollection(new List<Entity> {
                    new Entity("solutioncomponent") { Attributes = { { "objectid", id }, { "componenttype", 1 } } }
                }));

            var result = sut.GenerateOptionSet(optionSetMetadata, serviceProvider);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GenerateGlobalOptionSet()
        {
            var optionSetMetadata = new OptionSetMetadata
            {
                IsGlobal = true,
                OptionSetType = OptionSetType.Picklist,
                Name = "ee_TestOpt"
            };
            var id = Guid.NewGuid();
            var attId = Guid.NewGuid();
            var attributeMetadata = new PicklistAttributeMetadata
            {
                MetadataId = attId, OptionSet = optionSetMetadata
            }.Set(x => x.EntityLogicalName, "ee_test");

            organizationMetadata.Entities.Returns(new[] {
                new EntityMetadata { 
                    LogicalName = "ee_test",  MetadataId = id, 
                }
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

        [TestMethod]
        public void GenerateGlobalOptionSetAlreadyGenerated()
        {
            var optionSetMetadata = new OptionSetMetadata
            {
                IsGlobal = true,
                OptionSetType = OptionSetType.Picklist,
                Name = "ee_TestOpt"
            };
            var id = Guid.NewGuid();
            var attId = Guid.NewGuid();
            var attributeMetadata = new PicklistAttributeMetadata
            {
                MetadataId = attId, OptionSet = optionSetMetadata
            }.Set(x => x.EntityLogicalName, "ee_test");

            organizationMetadata.Entities.Returns(new[] {
                new EntityMetadata { LogicalName = "ee_test", MetadataId = id}
                .Set(x => x.Attributes, new[] { attributeMetadata })
            });

            SolutionHelper.organisationService.RetrieveMultiple(Arg.Any<QueryExpression>())
                .Returns(new EntityCollection(new List<Entity> {
                    new Entity("solutioncomponent") { Attributes = { { "objectid", id }, { "componenttype", 1 } } },
                    new Entity("solutioncomponent") { Attributes = { { "objectid", attId }, { "name", "ee_testval" }, { "componenttype", 2 } } }
                }));

            sut.GenerateOptionSet(optionSetMetadata, serviceProvider);

            var result = sut.GenerateOptionSet(optionSetMetadata, serviceProvider);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GenerateCustomOptionSet()
        {
            var optionSetMetadata = new OptionSetMetadata
            {
                IsGlobal = false,
                IsCustomOptionSet = true,
                OptionSetType = OptionSetType.Picklist
            };

            organizationMetadata.Entities.Returns(new EntityMetadata[0]);

            SolutionHelper.organisationService.RetrieveMultiple(Arg.Any<QueryExpression>())
                .Returns(new EntityCollection(new List<Entity>(0)));

            var result = sut.GenerateOptionSet(optionSetMetadata, serviceProvider);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void GenerateStateOptionSet()
        {
            var optionSetMetadata = new OptionSetMetadata
            {
                IsGlobal = false,
                IsCustomOptionSet = false,
                OptionSetType = OptionSetType.State
            };

            organizationMetadata.Entities.Returns(new EntityMetadata[0]);

            SolutionHelper.organisationService.RetrieveMultiple(Arg.Any<QueryExpression>())
                .Returns(new EntityCollection(new List<Entity>(0)));

            var result = sut.GenerateOptionSet(optionSetMetadata, serviceProvider);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void GenerateStatusOptionSet()
        {
            var optionSetMetadata = new OptionSetMetadata
            {
                IsGlobal = false,
                IsCustomOptionSet = false,
                OptionSetType = OptionSetType.Status
            };

            organizationMetadata.Entities.Returns(new EntityMetadata[0]);

            SolutionHelper.organisationService.RetrieveMultiple(Arg.Any<QueryExpression>())
                .Returns(new EntityCollection(new List<Entity>(0)));

            var result = sut.GenerateOptionSet(optionSetMetadata, serviceProvider);

            Assert.IsTrue(result);
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

        [TestMethod]
        public void GeneratePicklistAttribute()
        {
            var result = sut.GenerateAttribute(new PicklistAttributeMetadata(), serviceProvider);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void GenerateStateAttribute()
        {
            var result = sut.GenerateAttribute(new StateAttributeMetadata(), serviceProvider);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void GenerateStatusAttribute()
        {
            var result = sut.GenerateAttribute(new StatusAttributeMetadata(), serviceProvider);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void GenerateStringAttribute()
        {
            var result = sut.GenerateAttribute(new StringAttributeMetadata(), serviceProvider);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GenerateEntity()
        {
            var id = Guid.NewGuid();
            var entityMetadata = new EntityMetadata { LogicalName = "ee_test", MetadataId = id };

            organizationMetadata.Entities.Returns(new[] { entityMetadata });

            SolutionHelper.organisationService.RetrieveMultiple(Arg.Any<QueryExpression>())
                .Returns(new EntityCollection(new List<Entity> {
                    new Entity("solutioncomponent") { Attributes = { { "objectid", id }, { "componenttype", 1 } } }
                }));

            var result = sut.GenerateEntity(entityMetadata, serviceProvider);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void GenerateEntityMissing()
        {
            var id = Guid.NewGuid();
            var entityMetadata = new EntityMetadata { LogicalName = "ee_test", MetadataId = id };

            organizationMetadata.Entities.Returns(new[] { entityMetadata });

            SolutionHelper.organisationService.RetrieveMultiple(Arg.Any<QueryExpression>())
                .Returns(new EntityCollection(new List<Entity> {}));

            var result = sut.GenerateEntity(entityMetadata, serviceProvider);

            Assert.IsFalse(result);
        }
    }
}