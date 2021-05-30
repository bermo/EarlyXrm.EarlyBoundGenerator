using EarlyBoundTypes;
using Microsoft.Crm.Services.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EarlyXrm.EarlyBoundGenerator.UnitTests
{
    [TestClass]
    public class SolutionHelperUnitTests
    {
        private IOrganizationService orgService;
        private IOrganizationMetadata organizationMetadata;

        [TestInitialize]
        public void TestInitialise()
        {
            typeof(SolutionHelper)
                .NullStaticField(nameof(SolutionHelper.organisationService))
                .NullStaticField(nameof(SolutionHelper.solutionEntities))
                .NullStaticField(nameof(SolutionHelper.commandlineArgs));

            organizationMetadata = Substitute.For<IOrganizationMetadata>();
            orgService = Substitute.For<IOrganizationService>();
            SolutionHelper.organisationService = orgService;
        }

        [TestMethod]
        public void GetSolutionEntities_IncludesEntity()
        {
            var id = Guid.NewGuid();
            
            organizationMetadata.Entities.Returns(new[] {
                new EntityMetadata { LogicalName = "ee_test", MetadataId = id }
            });
            SolutionHelper.organisationMetadata = organizationMetadata;
            orgService.RetrieveMultiple(Arg.Any<QueryBase>())
                .Returns(new EntityCollection ( new List<Entity> { 
                    new SolutionComponent {   }.Set(x => x.Regarding, id)
                } ));
            SolutionHelper.commandlineArgs = new[] { "/extra:ee_test" };
            SolutionHelper.SetExtra();

            var ents = SolutionHelper.GetSolutionEntities();

            Assert.IsNotNull(ents.First(x => x.LogicalName == "ee_test"));
        }

        [TestMethod]
        public void GetSolutionEntities_SkipsEntity()
        {
            var id = Guid.NewGuid();

            organizationMetadata.Entities.Returns(new[] {
                new EntityMetadata { LogicalName = "ee_test", MetadataId = id }
            });
            SolutionHelper.organisationMetadata = organizationMetadata;
            orgService.RetrieveMultiple(Arg.Any<QueryBase>())
                .Returns(new EntityCollection(new List<Entity> {
                    new SolutionComponent {   }.Set(x => x.Regarding, id)
                }));
            SolutionHelper.commandlineArgs = new[] { "/skip:ee_test" };
            SolutionHelper.SetSkip();

            var ents = SolutionHelper.GetSolutionEntities();

            Assert.IsNull(ents.FirstOrDefault(x => x.LogicalName == "ee_test"));
        }

        [TestMethod]
        public void StackTrace_AppendsParameters()
        {
            var result = SolutionHelper.Signature(new StackTrace(), "one");

            Assert.AreEqual("RuntimeMethodHandle.InvokeMethod(one)", result);
        }
    }
}