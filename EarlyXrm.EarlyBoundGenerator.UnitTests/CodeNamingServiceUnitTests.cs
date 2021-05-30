using Microsoft.Crm.Services.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;

namespace EarlyXrm.EarlyBoundGenerator.UnitTests
{
    [TestClass]
    public class CodeNamingServiceUnitTests : UnitTestBase
    {
        private Dictionary<string, string> parameters;
        private CodeNamingService sut;
        
        private IMetadataProviderService metadataProviderService;
        private INamingService namingService;

        private TestMetadata testMetadata;

        [TestInitialize]
        public void TestInitialise()
        {
            metadataProviderService = Substitute.For<IMetadataProviderService>();
            organizationMetadata = Substitute.For<IOrganizationMetadata>();
            metadataProviderService.LoadMetadata().Returns(organizationMetadata);
            serviceProvider.GetService(typeof(IMetadataProviderService)).Returns(metadataProviderService);

            namingService = Substitute.For<INamingService>();
            parameters = new Dictionary<string, string> {
                { "UseDisplayNames", true.ToString() }
            };

            testMetadata = new TestMetadata(filterService);

            organizationMetadata.Entities.Returns(testMetadata.ToArray());

            sut = new CodeNamingService(namingService, parameters);
        }

        [TestMethod]
        public void GetNameForEntity()
        {
            organizationMetadata.Entities.Returns(new[] {
                new EntityMetadata { LogicalName = "ee_blah", DisplayName = new Label("Blah", 1033) }
            });

            var em = new EntityMetadata { DisplayName = new Label("Blah", 1433) };
            var result = sut.GetNameForEntity(em, serviceProvider);

            Assert.AreEqual("Blah", result);
        }

        [TestMethod]
        public void GetNameForEntityWithDup()
        {
            organizationMetadata.Entities.Returns(new[] {
                new EntityMetadata { LogicalName = "ee_blah", DisplayName = new Label("Blah", 1033) },
                new EntityMetadata { LogicalName = "ee_blah2", DisplayName = new Label("Blah", 1033) }
            });

            var em = new EntityMetadata { LogicalName = "ee_blah2", DisplayName = new Label("Blah", 1433) };
            var result = sut.GetNameForEntity(em, serviceProvider);

            Assert.AreEqual("Blah2", result);
        }

        [TestMethod]
        public void GetNameForAttribute_AddsRefOnTheEndOfLookup()
        {
            var attMetadata = new LookupAttributeMetadata { LogicalName = "ee_testpropid", DisplayName = new Label("TestProp", 1033) };
            var metadata = new EntityMetadata { LogicalName = "ee_test" }
                    .Set(x => x.Attributes, new AttributeMetadata[] {attMetadata});
            
            var output = sut.GetNameForAttribute(metadata, attMetadata, serviceProvider);

            Assert.AreEqual("TestProp", output);
        }

        [TestMethod]
        public void GetNameForAttribute_AddsIndexToDupAttributeLabel()
        {
            var attMetadata = new StringAttributeMetadata { LogicalName = "ee_value1", DisplayName = new Label("Value", 1033) };
            var metadata = new EntityMetadata { LogicalName = "ee_test" }
                    .Set(x => x.Attributes, new AttributeMetadata[] { 
                        new StringAttributeMetadata { LogicalName = "ee_value", DisplayName = new Label("Value", 1033) },
                        attMetadata});

            var output = sut.GetNameForAttribute(metadata, attMetadata, serviceProvider);

            Assert.AreEqual("Value2", output);
        }

        [TestMethod]
        public void GetNameForRelationship_LooksUpReferencingEntityAttribute()
        {
            organizationMetadata.Entities.Returns(new[] {
                new EntityMetadata { LogicalName = "ee_testprop", DisplayName = new Label("Test Prop", 1033) }
            });

            var relationship = new OneToManyRelationshipMetadata { 
                ReferencingEntity = "ee_test", ReferencingAttribute = "ee_testid",
                ReferencedEntity = "ee_testprop"
            };
            var attMetadata = new StringAttributeMetadata { LogicalName = "ee_testid", DisplayName = new Label("Test Id", 1033) };
            var metadata = new EntityMetadata { LogicalName = "ee_test" }
                    .Set(x => x.Attributes, new AttributeMetadata[] { attMetadata })
                    .Set(x => x.ManyToOneRelationships, new OneToManyRelationshipMetadata[]
                    {
                        relationship,
                        new OneToManyRelationshipMetadata { ReferencingEntity = "ee_testprop", ReferencingAttribute = "ee_testid" }
                    });

            var output = sut.GetNameForRelationship(metadata, relationship, null, serviceProvider);

            Assert.AreEqual("TestId_TestProp", output);
        }

        [TestMethod]
        public void GetNameForRelationship_OneToMany()
        {
            organizationMetadata.Entities.Returns(new[] {
                new EntityMetadata { LogicalName = "ee_testprop",
                DisplayCollectionName = new Label("Test Props", 1033) }
                .Set(x=> x.Attributes, new AttributeMetadata[]
                {
                    new StringAttributeMetadata{ 
                        LogicalName = "ee_testid", 
                        DisplayName = new Label("Test Id", 1033)
                    }
                })
            });

            var relationship = new OneToManyRelationshipMetadata
            {
                ReferencingEntity = "ee_testprop",
                ReferencingAttribute = "ee_testid",
                ReferencedEntity = "ee_test",
                SchemaName = "ee_two"
            };
            var attMetadata = new StringAttributeMetadata { LogicalName = "ee_testid", DisplayName = new Label("Test Id", 1033) };
            var metadata = new EntityMetadata { LogicalName = "ee_test" }
                    .Set(x => x.Attributes, new AttributeMetadata[] { attMetadata })
                    .Set(x => x.OneToManyRelationships, new OneToManyRelationshipMetadata[]
                    {
                        relationship,
                        new OneToManyRelationshipMetadata { ReferencingEntity = "ee_testprop", ReferencingAttribute = "ee_testid", SchemaName= "ee_one" }
                    });

            var output = sut.GetNameForRelationship(metadata, relationship, null, serviceProvider);

            Assert.AreEqual("TestId_TestProps", output);
        }

        [TestMethod]
        public void GetNameForRelationship_Reflexive()
        {
            var attMetadata = new StringAttributeMetadata { LogicalName = "ee_testid", DisplayName = new Label("Test Id", 1033) };
            var em = new EntityMetadata
            {
                LogicalName = "ee_test",
                DisplayName = new Label("Test", 1033),
                DisplayCollectionName = new Label("Tests", 1033)
            };
            em.Set(x => x.Attributes, new [] {
                attMetadata
            });
            organizationMetadata.Entities.Returns(new[] {em});

            var relationship = new OneToManyRelationshipMetadata
            {
                ReferencingEntity = "ee_test",
                ReferencingAttribute = "ee_testid",
                ReferencedEntity = "ee_test",
                ReferencedAttribute = "ee_testid"
            };
            var metadata = new EntityMetadata { LogicalName = "ee_test" }
                    .Set(x => x.Attributes, new AttributeMetadata[] { attMetadata })
                    .Set(x => x.OneToManyRelationships, new OneToManyRelationshipMetadata[]
                    {
                        relationship,
                        new OneToManyRelationshipMetadata { ReferencingAttribute = "ee_testid" }
                    });

            var output = sut.GetNameForRelationship(metadata, relationship, EntityRole.Referenced, serviceProvider);

            Assert.AreEqual("TestId_Tests", output);
        }

        [TestMethod]
        public void GetNameForAttribute_WhenNotUsingDisplayNames_ReturnsDefault()
        {
            namingService.GetNameForAttribute(Arg.Any<EntityMetadata>(), Arg.Any<AttributeMetadata>(), serviceProvider)
                .Returns("Name");
            parameters = new Dictionary<string, string> {
                { "UseDisplayNames", false.ToString() }
            };

            sut = new CodeNamingService(namingService, parameters);

            var result = sut.GetNameForAttribute(new EntityMetadata(), new AttributeMetadata(), serviceProvider);
            Assert.AreEqual(result, "Name");
        }

        [TestMethod]
        public void GetNameForOptionSet_ReturnsOptionsetName()
        {
            var optionSet = new OptionSetMetadata { Name = "OptionsetName" };
            organizationMetadata.OptionSets.Returns(new[] {optionSet});

            var result = sut.GetNameForOptionSet(new EntityMetadata(), optionSet, serviceProvider);

            Assert.AreEqual(result, "OptionsetName");
        }

        [TestMethod]
        public void GetNameForOption_AdjustsOptionName()
        {
            var option = new OptionMetadata
            {
                Label = new Label("5TestValue", 1033),
                Value = 6
            };
            var optionSet = new OptionSetMetadata()
                .Set(x => x.Options, new OptionMetadataCollection(new List<OptionMetadata> {
                    new OptionMetadata { Label = new Label("5TestValue", 1033), Value = 5 },
                    option
                }));


            var result = sut.GetNameForOption(optionSet, option, serviceProvider);

            Assert.AreEqual(result, "_5TestValue2");
        }

        [TestMethod]
        public void GetNameForRelationship_ManyToMany()
        {
            organizationMetadata.Entities.Returns(new[] {
                new EntityMetadata { LogicalName = "ee_testprop",
                DisplayCollectionName = new Label("Test Props", 1033) }
                .Set(x=> x.Attributes, new AttributeMetadata[]
                {
                    new StringAttributeMetadata{
                        LogicalName = "ee_testid",
                        DisplayName = "Test Id".AsLabel()
                    }
                })
            });

            var relationship = new OneToManyRelationshipMetadata
            {
                ReferencingEntity = "ee_testprop",
                ReferencingAttribute = "ee_testid",
                ReferencedEntity = "ee_test",
                SchemaName = "ee_two"
            };
            var many2Many = new ManyToManyRelationshipMetadata
            {
                Entity1LogicalName = "ee_test",
                Entity2LogicalName = "ee_testprop",
                SchemaName = "ee_testProp_association"
            };
            var attMetadata = new StringAttributeMetadata { LogicalName = "ee_testid", DisplayName = "Test Id".AsLabel() };
            var metadata = new EntityMetadata { LogicalName = "ee_test" }
                    .Set(x => x.Attributes, new AttributeMetadata[] { attMetadata })
                    .Set(x => x.OneToManyRelationships, new OneToManyRelationshipMetadata[]
                    {
                        relationship,
                        new OneToManyRelationshipMetadata { ReferencingEntity = "ee_testprop", ReferencingAttribute = "ee_testid", SchemaName= "ee_one" }
                    })
                    .Set(x => x.ManyToManyRelationships, new[] { many2Many });

            var output = sut.GetNameForRelationship(metadata, many2Many, null, serviceProvider);

            Assert.AreEqual("TestProps", output);
        }

        [TestMethod]
        public void GetNameForRelationship_OneToMany_()
        {
            //SetupEntities();
            var rel = testMetadata.Test.OneToManyRelationships.First(x => x.SchemaName == "ee_test_testchilds");

            var output = sut.GetNameForRelationship(testMetadata.Test, rel, null, serviceProvider);

            Assert.AreEqual("Test_TestChildren", output);
        }

        [TestMethod]
        public void GetNameForLookupAttribute()
        {
            var att = testMetadata.Test.Attributes.First(x => x.LogicalName == "ee_testparentid");

            var output = sut.GetNameForAttribute(testMetadata.Test, att, serviceProvider);

            Assert.AreEqual("TestParent", output);
        }

        [TestMethod]
        public void GetNameForAttribute_DuplicateDisplayNameAppends2()
        {
            testMetadata.Test.AddAttribute(new StringAttributeMetadata { DisplayName = "Name".AsLabel(), LogicalName = "ee_name1" });
            var att = testMetadata.Test.Attributes.First(x => x.LogicalName == "ee_name1");

            var output = sut.GetNameForAttribute(testMetadata.Test, att, serviceProvider);

            Assert.AreEqual("Name2", output);
        }

        [TestMethod]
        public void GetNameForRelationship_Duplicates()
        {
            testMetadata.Test.AddAttribute(filterService, new StringAttributeMetadata { 
                DisplayName = "Test Parent_Test Parent".AsLabel(), 
                LogicalName = "ee_testparent" });
            var rel = testMetadata.Test.ManyToOneRelationships.First(x => x.SchemaName == "ee_testparent_tests");

            var output = sut.GetNameForRelationship(testMetadata.Test, rel, null, serviceProvider);

            Assert.AreEqual("TestParent_TestParent2", output);
        }

        [TestMethod]
        public void GetNameForRelationship_AttributeAndManyToOneDups()
        {
            testMetadata.Test.AddAttribute(filterService, new StringAttributeMetadata { 
                DisplayName = "Test Parent_Test Parent".AsLabel(),
                LogicalName = "ee_testparent" });
            testMetadata.TestParent.AddAttribute(filterService, new AttributeMetadata { 
                DisplayName = "Test Parent".AsLabel(), 
                LogicalName = "ee_testid2" });
            testMetadata.Test.AddManyToOne(filterService, new OneToManyRelationshipMetadata {
                ReferencedEntity = "ee_testparent",
                ReferencedAttribute = "ee_testid2",
                ReferencingAttribute = "ee_testparentid",
                SchemaName = "ee_testparent_tests2"
            });
            var rel = testMetadata.Test.ManyToOneRelationships.First(x => x.SchemaName == "ee_testparent_tests2");

            var output = sut.GetNameForRelationship(testMetadata.Test, rel, null, serviceProvider);

            Assert.AreEqual("TestParent_TestParent3", output);
        }

        [TestMethod]
        public void GetNameForRelationship_AttributeAndOneToManyDupsClash()
        {
            testMetadata.Test.AddAttribute(filterService, new StringAttributeMetadata
            {
                DisplayName = "Test Parent_Test Parent".AsLabel(),
                LogicalName = "ee_testparent"
            });
            testMetadata.TestParent.AddAttribute(filterService, new AttributeMetadata
            {
                DisplayName = "Test Parent".AsLabel(),
                LogicalName = "ee_testid2"
            });
            testMetadata.Test.AddManyToOne(filterService, new OneToManyRelationshipMetadata
            {
                ReferencedEntity = "ee_testparent",
                ReferencedAttribute = "ee_testid2",
                ReferencingAttribute = "ee_testparentid",
                SchemaName = "ee_testparent_tests2"
            });
            testMetadata.TestParent.DisplayCollectionName = "TestParent".AsLabel();
            testMetadata.TestParent.AddAttribute(filterService, new AttributeMetadata
            {
                DisplayName = "Test Parent".AsLabel(),
                LogicalName = "ee_testid3"
            });
            testMetadata.Test.AddOneToMany(filterService, new OneToManyRelationshipMetadata
            {
                ReferencingEntity = "ee_testparent",
                ReferencingAttribute = "ee_testid3",
                SchemaName = "ee_test_testparent3"
            });
            var rel = testMetadata.Test.OneToManyRelationships.First(x => x.SchemaName == "ee_test_testparent3");

            var output = sut.GetNameForRelationship(testMetadata.Test, rel, null, serviceProvider);

            Assert.AreEqual("TestParent_TestParent4", output);
        }

        [TestMethod]
        public void GetNameForRelationship_ManyToOneWithDupEntityName()
        {
            var rel = testMetadata.Test.ManyToOneRelationships.First(x => x.SchemaName == "ee_testparent_tests");

            var output = sut.GetNameForRelationship(testMetadata.Test, rel, null, serviceProvider);

            Assert.AreEqual("TestParent_TestParent", output);
        }

        [TestMethod]
        public void GetNameForRelationship_ManyToOneWithDupAttribute()
        {
            var rel = testMetadata.Test.ManyToOneRelationships.First(x => x.SchemaName == "ee_testparent_tests");
            testMetadata.Test.AddAttribute(new StringAttributeMetadata { DisplayName = "Test Parent".AsLabel() });

            var output = sut.GetNameForRelationship(testMetadata.Test, rel, null, serviceProvider);

            Assert.AreEqual("TestParent_TestParent", output);
        }
    }
}