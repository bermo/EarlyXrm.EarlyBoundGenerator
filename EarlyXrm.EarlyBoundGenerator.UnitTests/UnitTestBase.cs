using Microsoft.Crm.Services.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using ModelBuilder;
using NSubstitute;
using System;
using System.CodeDom;
using System.Reflection;

namespace EarlyXrm.EarlyBoundGenerator.UnitTests
{
    public abstract class UnitTestBase
    {
        protected IServiceProvider serviceProvider;
        protected IBuildConfiguration Builder;

        protected IOrganizationMetadata organizationMetadata;
        protected ICodeWriterFilterService filterService;

        [TestInitialize]
        public void Initialise()
        {
            Builder = Model.UsingModule<DynamicsModule>();

            typeof(SolutionHelper)
                .GetField("organisationMetadata", BindingFlags.Static | BindingFlags.NonPublic)
                .SetValue(null, null);

            typeof(SolutionHelper)
                .GetField("solutionEntities", BindingFlags.Static | BindingFlags.NonPublic)
                .SetValue(null, null);

            SolutionHelper.organisationService = Substitute.For<IOrganizationService>();

            serviceProvider = Substitute.For<IServiceProvider>();

            filterService = Substitute.For<ICodeWriterFilterService>();
            serviceProvider.GetService(typeof(ICodeWriterFilterService)).Returns(filterService);
        }

        protected CodeAttributeDeclaration Build<T>(params string[] args) where T : Attribute
        {
            var cad = new CodeAttributeDeclaration
            {
                Name = typeof(T).Name,
                Arguments = { }
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

        protected CodeAttributeDeclaration Build<T>(params CodeExpression[] args) where T : Attribute
        {
            var cad = new CodeAttributeDeclaration
            {
                Name = typeof(T).Name,
                Arguments = { }
            };
            foreach (var arg in args)
            {
                cad.Arguments.Add(new CodeAttributeArgument(arg));
            }
            return cad;
        }
    }
}