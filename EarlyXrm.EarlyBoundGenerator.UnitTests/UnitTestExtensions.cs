using Microsoft.Crm.Services.Utility;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using NSubstitute;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace EarlyXrm.EarlyBoundGenerator.UnitTests
{
    public static class UnitTestExtensions
    {
        public static T Set<T, U>(this T t, Expression<Func<T, U>> prop, U val)
        {
            var me = prop.Body as MemberExpression;
            var pi = me.Member as PropertyInfo;

            var property = typeof(T).GetProperty(pi.Name);
            if (property.GetSetMethod(true) == null)
            {
                if (typeof(Entity).IsAssignableFrom(typeof(T)))
                {
                    var att = property.GetCustomAttribute<AttributeLogicalNameAttribute>().LogicalName;

                    (t as Entity).Attributes[att] = val;
                }
            }
            else
            {
                property.SetValue(t, val);
            }

            return t;
        }

        public static Type NullStaticField(this Type t, string field)
        {
            t
                .GetField(field, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                .SetValue(null, null);
            return t;
        }

        public static EntityMetadata Setup(
            this EntityMetadata ent,
            AttributeMetadata[] attributes,
            OneToManyRelationshipMetadata[] oneToManyRelationships,
            OneToManyRelationshipMetadata[] manyToOneRelationships
        )
        {
            foreach (var att in attributes ?? new AttributeMetadata[0])
                att.Set(x => x.EntityLogicalName, ent.LogicalName);

            foreach (var one2Many in oneToManyRelationships ?? new OneToManyRelationshipMetadata[0])
            {
                one2Many.Set(x => x.RelationshipType, RelationshipType.OneToManyRelationship);
                one2Many.ReferencedAttribute = ent.PrimaryIdAttribute;
                one2Many.ReferencedEntity = ent.LogicalName;
            }

            foreach (var many2One in manyToOneRelationships ?? new OneToManyRelationshipMetadata[0])
            {
                many2One.Set(x => x.RelationshipType, RelationshipType.OneToManyRelationship);
                many2One.ReferencingEntity = ent.LogicalName;
            };

            ent.Set(x => x.Attributes, attributes)
                .Set(x => x.OneToManyRelationships, oneToManyRelationships)
                .Set(x => x.ManyToOneRelationships, manyToOneRelationships);

            return ent;
        }

        public static EntityMetadata AddAttribute(
            this EntityMetadata entityMetadata,
            params AttributeMetadata[] attributes
        )
        {
            return entityMetadata.AddAttribute(null, attributes);
        }

        public static EntityMetadata AddAttribute(
            this EntityMetadata entityMetadata,
            ICodeWriterFilterService filterService,
            params AttributeMetadata[] attributes
        )
        {
            foreach (var att in attributes)
            {
                att.Set(x => x.EntityLogicalName, entityMetadata.LogicalName);

                if(filterService != null)
                    filterService.GenerateAttribute(att, Arg.Any<IServiceProvider>())
                        .Returns(true);
            }

            var existing = entityMetadata.Attributes ?? Array.Empty<AttributeMetadata>();
            entityMetadata.Set(x => x.Attributes, existing.Union(attributes).ToArray());
            return entityMetadata;
        }

        public static EntityMetadata AddOneToMany(
            this EntityMetadata entityMetadata,
            ICodeWriterFilterService filterService,
            params OneToManyRelationshipMetadata[] oneToManyRelationshipMetadata
        )
        {
            foreach (var one2Many in oneToManyRelationshipMetadata)
            {
                one2Many.Set(x => x.RelationshipType, RelationshipType.OneToManyRelationship);
                one2Many.ReferencedAttribute = entityMetadata.PrimaryIdAttribute;
                one2Many.ReferencedEntity = entityMetadata.LogicalName;

                if (filterService != null)
                    filterService.GenerateRelationship(one2Many, Arg.Is<EntityMetadata>(x => x.LogicalName == one2Many.ReferencingEntity), Arg.Any<IServiceProvider>())
                        .Returns(true);
            }
            var existing = entityMetadata.OneToManyRelationships ?? Array.Empty<OneToManyRelationshipMetadata>();
            entityMetadata.Set(x => x.OneToManyRelationships, existing.Union(oneToManyRelationshipMetadata).ToArray());
            return entityMetadata;
        }

        public static EntityMetadata AddManyToOne(
            this EntityMetadata entityMetadata,
            ICodeWriterFilterService filterService,
            params OneToManyRelationshipMetadata[] manyToOneRelationshipMetadata
        )
        {
            foreach (var many2One in manyToOneRelationshipMetadata)
            {
                many2One.Set(x => x.RelationshipType, RelationshipType.OneToManyRelationship);
                many2One.ReferencingEntity = entityMetadata.LogicalName;

                if (filterService != null)
                    filterService.GenerateRelationship(many2One, Arg.Is<EntityMetadata>(x => x.LogicalName == many2One.ReferencedEntity), Arg.Any<IServiceProvider>())
                        .Returns(true);
            };

            var existing = entityMetadata.ManyToOneRelationships ?? Array.Empty<OneToManyRelationshipMetadata>();
            entityMetadata.Set(x => x.ManyToOneRelationships, existing.Union(manyToOneRelationshipMetadata).ToArray());
            return entityMetadata;
        }

        public static EntityMetadata AddManyToMany(
            this EntityMetadata entityMetadata,
            ICodeWriterFilterService filterService,
            params ManyToManyRelationshipMetadata[] manyToManyRelationshipMetadata
        )
        {
            foreach (var many2Many in manyToManyRelationshipMetadata)
            {
                many2Many.Set(x => x.RelationshipType, RelationshipType.ManyToManyRelationship);
                if (many2Many.Entity1LogicalName == null)
                {
                    many2Many.Entity1IntersectAttribute = entityMetadata.PrimaryIdAttribute;
                    many2Many.Entity1LogicalName = entityMetadata.LogicalName;
                }
                else
                {
                    many2Many.Entity2IntersectAttribute = entityMetadata.PrimaryIdAttribute;
                    many2Many.Entity2LogicalName = entityMetadata.LogicalName;
                }

                if (filterService != null)
                    filterService.GenerateRelationship(many2Many, Arg.Any<EntityMetadata>(), Arg.Any<IServiceProvider>())
                        .Returns(true);
            }
            var existing = entityMetadata.ManyToManyRelationships ?? Array.Empty<ManyToManyRelationshipMetadata>();
            entityMetadata.Set(x => x.ManyToManyRelationships, existing.Union(manyToManyRelationshipMetadata).ToArray());
            return entityMetadata;
        }

        public static Label AsLabel(this string name)
        {
            return new Label(name, 1033);
        }
    }
}