using Microsoft.Crm.Services.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using NSubstitute;
using System;
using System.Collections.Generic;

namespace EarlyXrm.EarlyBoundGenerator.UnitTests
{
    [TestClass]
    public class CodeNamingServiceUnitTests : UnitTestBase
    {
        private Dictionary<string, string> parameters;
        private CodeNamingService sut;
        private IOrganizationMetadata organizationMetadata;
        private IMetadataProviderService metadataProviderService;
        private INamingService namingService;

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

            sut = new CodeNamingService(namingService, parameters);

        }

        [TestMethod]
        public void GetNameForEntity()
        {
            
            var em = new EntityMetadata { DisplayName = new Label("Blah", 1433) };
            var serviceProvider = Substitute.For<IServiceProvider>();
            var result = sut.GetNameForEntity(em, serviceProvider);

            Assert.AreEqual("Blah", result);
        }

        [TestMethod]
        public void GetNameForAttribute_AddsRefOnTheEndOfLookup()
        {
            var attMetadata = new LookupAttributeMetadata { LogicalName = "ee_testpropid", DisplayName = new Label("TestProp", 1033) };
            var metadata = new EntityMetadata { LogicalName = "ee_test" }
                    .Set(x => x.Attributes, new AttributeMetadata[] {attMetadata});
            
            var output = sut.GetNameForAttribute(metadata, attMetadata, serviceProvider);

            Assert.AreEqual("TestPropRef", output);
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
                ReferencedEntity = "ee_testprop", ReferencedAttribute = "ee_testid"
            };
            var attMetadata = new StringAttributeMetadata { LogicalName = "ee_testid", DisplayName = new Label("Test Id", 1033) };
            var metadata = new EntityMetadata { LogicalName = "ee_test" }
                    .Set(x => x.Attributes, new AttributeMetadata[] { attMetadata })
                    .Set(x => x.ManyToOneRelationships, new OneToManyRelationshipMetadata[]
                    {
                        relationship,
                        new OneToManyRelationshipMetadata { ReferencingAttribute = "ee_testid" }
                    });

            var output = sut.GetNameForRelationship(metadata, relationship, null, serviceProvider);

            Assert.AreEqual("TestIdTestProp", output);
        }

        [TestMethod]
        public void GetNameForRelationship_Reflexive()
        {
            organizationMetadata.Entities.Returns(new[] {
                new EntityMetadata { LogicalName = "ee_test", DisplayName = new Label("Test", 1033), DisplayCollectionName = new Label("Tests", 1033)}
            });

            var relationship = new OneToManyRelationshipMetadata
            {
                ReferencingEntity = "ee_test",
                ReferencingAttribute = "ee_testid",
                ReferencedEntity = "ee_test",
                ReferencedAttribute = "ee_testid"
            };
            var attMetadata = new StringAttributeMetadata { LogicalName = "ee_testid", DisplayName = new Label("Test Id", 1033) };
            var metadata = new EntityMetadata { LogicalName = "ee_test" }
                    .Set(x => x.Attributes, new AttributeMetadata[] { attMetadata })
                    .Set(x => x.ManyToOneRelationships, new OneToManyRelationshipMetadata[]
                    {
                        relationship,
                        new OneToManyRelationshipMetadata { ReferencingAttribute = "ee_testid" }
                    });

            var output = sut.GetNameForRelationship(metadata, relationship, EntityRole.Referenced, serviceProvider);

            Assert.AreEqual("TestIdTests", output);
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
    }
}
