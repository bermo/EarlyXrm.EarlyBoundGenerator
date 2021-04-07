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
    }
}