using Microsoft.Crm.Services.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using NSubstitute;
using System.Collections.Generic;

namespace EarlyXrm.EarlyBoundGenerator.UnitTests
{
    [TestClass]
    public class OptionSetsNamingServiceUnitTests : UnitTestBase
    {
        private INamingService namingService;

        private OptionSetsNamingService sut;

        [TestInitialize]
        public void TestInitialise()
        {
            namingService = Substitute.For<INamingService>();

            sut = new OptionSetsNamingService(namingService);
        }

        [TestMethod]
        public void GetNameForAttribute_ReturnsDefault()
        {
            namingService.GetNameForAttribute(Arg.Any<EntityMetadata>(), Arg.Any<AttributeMetadata>(), serviceProvider)
                .Returns("Name");

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