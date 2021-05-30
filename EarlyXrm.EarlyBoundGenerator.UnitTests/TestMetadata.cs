using Microsoft.Crm.Services.Utility;
using Microsoft.Xrm.Sdk.Metadata;
using System.Collections.Generic;

namespace EarlyXrm.EarlyBoundGenerator.UnitTests
{
    public class TestMetadata : List<EntityMetadata>
    {
        private ICodeWriterFilterService filterService;

        public TestMetadata(ICodeWriterFilterService filterService)
        {
            this.filterService = filterService;

            AddRange(SetupEntities());
        }

        protected List<EntityMetadata> entities = new List<EntityMetadata>();

        public EntityMetadata TestParent;
        public EntityMetadata Test;
        public EntityMetadata TestChild;

        protected List<EntityMetadata> SetupEntities()
        {
            TestParent = new EntityMetadata
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

            Test = new EntityMetadata
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

            TestChild = new EntityMetadata
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

            entities.AddRange(new[] { TestParent, Test, TestChild });

            return entities;
        }
    }
}