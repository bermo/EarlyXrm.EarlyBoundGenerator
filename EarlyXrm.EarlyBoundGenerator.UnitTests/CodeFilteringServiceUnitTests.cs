using EarlyBoundTypes;
using Microsoft.Crm.Services.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using ModelBuilder;
using NSubstitute;
using System;
using System.Collections.Generic;

namespace EarlyXrm.EarlyBoundGenerator.UnitTests
{
    [TestClass]
    public class CodeFilteringServiceUnitTests : UnitTestBase
    {
        private Dictionary<string, string> parameters;
        private CodeFilteringService sut;
        private IMetadataProviderService metadataProviderService;

        [TestInitialize]
        public void TestInitialise()
        {
            metadataProviderService = Substitute.For<IMetadataProviderService>();
            organizationMetadata = Substitute.For<IOrganizationMetadata>();
            metadataProviderService.LoadMetadata().Returns(organizationMetadata);
            serviceProvider.GetService(typeof(IMetadataProviderService)).Returns(metadataProviderService);

            filterService.GenerateAttribute(Arg.Any<AttributeMetadata>(), serviceProvider).Returns(true);
            parameters = new Dictionary<string, string> {};

            sut = new CodeFilteringService(filterService, parameters);
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
        public void GenerateAttributes()
        {
            var entityMetadata = new EntityMetadata { LogicalName = "ee_test", MetadataId = Guid.NewGuid(), DisplayName = new Label("Test", 1033) };           
            var stringAttributeMetadata = new StringAttributeMetadata { 
                LogicalName = "ee_teststring",
                MetadataId = Guid.NewGuid(),
                DisplayName = new Label("Test String", 1033) 
            }.Set(x => x.EntityLogicalName, entityMetadata.LogicalName);
            var picklistAttributeMetadata = new PicklistAttributeMetadata
            {
                LogicalName = "ee_testpicklist",
                MetadataId = Guid.NewGuid(),
                DisplayName = new Label("Test Picklist", 1033)
            }.Set(x => x.EntityLogicalName, entityMetadata.LogicalName);
            var stateAttributeMetadata = new StateAttributeMetadata
            {
                LogicalName = "ee_teststate",
                MetadataId = Guid.NewGuid(),
                DisplayName = new Label("Test State", 1033)
            }.Set(x => x.EntityLogicalName, entityMetadata.LogicalName);
            organizationMetadata.Entities.Returns(new[] {
                entityMetadata.Set(x => x.Attributes, new AttributeMetadata [] { stringAttributeMetadata, picklistAttributeMetadata, stateAttributeMetadata })
            });

            var f = Builder.Create<SolutionComponent>();
            f.Set(x => x.Regarding, entityMetadata.MetadataId);

            SolutionHelper.organisationService.RetrieveMultiple(Arg.Any<QueryExpression>())
                .Returns(new EntityCollection(new List<Entity> {
                    Builder.Create<SolutionComponent>().Set(x => x.Regarding, entityMetadata.MetadataId).Set(x => x.ObjectTypeCode, ComponentType.Entity),
                    Builder.Create<SolutionComponent>().Set(x => x.Regarding, stringAttributeMetadata.MetadataId).Set(x => x.ObjectTypeCode,ComponentType.Attribute),
                    Builder.Create<SolutionComponent>().Set(x => x.Regarding, picklistAttributeMetadata.MetadataId).Set(x => x.ObjectTypeCode,ComponentType.Attribute),
                    Builder.Create<SolutionComponent>().Set(x => x.Regarding, stateAttributeMetadata.MetadataId).Set(x => x.ObjectTypeCode,ComponentType.Attribute),
                }));

            Assert.IsTrue(sut.GenerateAttribute(stringAttributeMetadata, serviceProvider));
            Assert.IsTrue(sut.GenerateAttribute(picklistAttributeMetadata, serviceProvider));
            Assert.IsTrue(sut.GenerateAttribute(stateAttributeMetadata, serviceProvider));
        }

        [TestMethod]
        public void GenerateRelationshipReturnsFalse()
        {
            filterService.GenerateRelationship(Arg.Any<RelationshipMetadataBase>(), Arg.Any<EntityMetadata>(), serviceProvider).Returns(true);

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
                    new Entity("solutioncomponent") { Id = Guid.NewGuid(), Attributes = { { "objectid", id }, { "componenttype", 1 } } },
                    new Entity("solutioncomponent") { Id = Guid.NewGuid(), Attributes = { { "objectid", attId }, { "name", "ee_testval" }, { "componenttype", 2 } } }
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
                MetadataId = attId,
                OptionSet = optionSetMetadata
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
                    new Entity("solutioncomponent") { Id = Guid.NewGuid(), Attributes = { { "objectid", id }, { "componenttype", 1 } } },
                    new Entity("solutioncomponent") { Id = Guid.NewGuid(), Attributes = { { "objectid", attId }, { "name", "ee_testval" }, { "componenttype", 2 } } }
                }));

            var result = sut.GenerateOptionSet(optionSetMetadata, serviceProvider);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void GeneratePicklistAttribute()
        {
            var id = Guid.NewGuid();
            var testId = Guid.NewGuid();

            var attributeMetadata = new PicklistAttributeMetadata
            {
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
                    Builder.Create<SolutionComponent>().Set(x => x.Regarding, id).Set(x => x.ObjectTypeCode, ComponentType.Entity),
                    Builder.Create<SolutionComponent>().Set(x => x.Regarding, testId).Set(x => x.ObjectTypeCode, ComponentType.Attribute),
                }));

            var result = sut.GenerateAttribute(attributeMetadata, serviceProvider);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void GenerateEntity2()
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
                .Returns(new EntityCollection(new List<Entity> { }));

            var result = sut.GenerateEntity(entityMetadata, serviceProvider);

            Assert.IsFalse(result);
        }
    }
}