using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
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

            typeof(T).GetProperty(pi.Name).SetValue(t, val);

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

        public static Label AsLabel(this string name)
        {
            return new Label(name, 1033);
        }
    }
}