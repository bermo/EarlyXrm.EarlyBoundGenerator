using Microsoft.Crm.Services.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using ModelBuilder;
using NSubstitute;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;

namespace EarlyXrm.EarlyBoundGenerator.UnitTests
{
    public abstract class UnitTestBase
    {
        protected IServiceProvider serviceProvider;
        protected IBuildConfiguration Builder;

        protected IOrganizationMetadata organizationMetadata;
        protected ICodeWriterFilterService filterService;

        protected EntityMetadata testParent;
        protected EntityMetadata test;
        protected EntityMetadata testChild;

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

        protected List<EntityMetadata> entities = new List<EntityMetadata>();

        protected void SetupEntities()
        {
            testParent = new EntityMetadata
            {
                LogicalName = "ee_testparent",
                DisplayName = "Test Parent".AsLabel()
            }
            .Set(x => x.PrimaryIdAttribute, "ee_testparentid")
            .AddAttribute(filterService,
                new UniqueIdentifierAttributeMetadata { LogicalName = "ee_testparentid", DisplayName = "Id".AsLabel() },
                new StringAttributeMetadata { LogicalName = "ee_name", DisplayName = "Name".AsLabel() })
            .AddOneToMany(filterService,
                new OneToManyRelationshipMetadata { ReferencedEntity = "ee_test", ReferencedAttribute = "ee_testid", SchemaName = "ee_testparent_tests" }
            );

            test = new EntityMetadata
            {
                LogicalName = "ee_test",
                DisplayName = "Test".AsLabel()
            }
            .Set(x => x.PrimaryIdAttribute, "ee_testid")
            .AddAttribute(filterService,
                new UniqueIdentifierAttributeMetadata { LogicalName = "ee_testid", DisplayName = "Id".AsLabel() },
                new StringAttributeMetadata { LogicalName = "ee_name", DisplayName = "Name".AsLabel() },
                new LookupAttributeMetadata { LogicalName = "ee_testparentid", DisplayName = "Test Parent".AsLabel() })
            .AddOneToMany(filterService,
                new OneToManyRelationshipMetadata
                {
                    ReferencingEntity = "ee_testchild",
                    ReferencingAttribute = "ee_testid",
                    SchemaName = "ee_test_testchilds"
                })
            .AddManyToOne(filterService,
                new OneToManyRelationshipMetadata
                {
                    ReferencedEntity = "ee_testparent",
                    ReferencedAttribute = "ee_testparentid",
                    ReferencingAttribute = "ee_testparentid",
                    SchemaName = "ee_testparent_tests"
                });

            testChild = new EntityMetadata
            {
                LogicalName = "ee_testchild",
                DisplayName = "Test Child".AsLabel(),
                DisplayCollectionName = "Test Children".AsLabel()
            }
            .Set(x => x.PrimaryIdAttribute, "ee_testchildid")
            .AddAttribute(filterService,
                new UniqueIdentifierAttributeMetadata { LogicalName = "ee_testchildid", DisplayName = "Id".AsLabel() },
                new StringAttributeMetadata { LogicalName = "ee_name", DisplayName = "Name".AsLabel() },
                new LookupAttributeMetadata { LogicalName = "ee_testid", DisplayName = "Test".AsLabel() })
            .AddManyToOne(filterService,
                new OneToManyRelationshipMetadata
                {
                    ReferencingEntity = "ee_test",
                    ReferencingAttribute = "ee_testid",
                    SchemaName = "ee_test_testchilds"
                }
            );

            entities.AddRange(new[] { testParent, test, testChild });

            organizationMetadata.Entities.Returns(entities.ToArray());
        }
    }
}