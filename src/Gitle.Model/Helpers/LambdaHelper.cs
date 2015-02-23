namespace Gitle.Model.Helpers
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class LambdaHelper
    {
        public static LambdaExpression GenerateSelector<T>(string propertyName, out Type resultType) where T : class
        {
            // Create a parameter to pass into the Lambda expression (Entity => Entity.OrderByField).
            var parameter = Expression.Parameter(typeof(T), "Entity");
            //  create the selector part, but support child properties
            PropertyInfo property;
            Expression propertyAccess;
            if (propertyName.Contains('.'))
            {
                // support to be sorted on child fields.
                var childProperties = propertyName.Split('.');
                property = typeof(T).GetProperty(childProperties[0]);
                propertyAccess = Expression.MakeMemberAccess(parameter, property);
                for (var i = 1; i < childProperties.Length; i++)
                {
                    var tempProp = property;
                    property = tempProp.PropertyType.GetProperty(childProperties[i]) ??
                               tempProp.PropertyType.GetInterfaces().Select(x => x.GetProperty(childProperties[i])).
                                                                                            FirstOrDefault(x => x != null);
                    propertyAccess = Expression.MakeMemberAccess(propertyAccess, property);
                }
            }
            else
            {
                property = typeof(T).GetProperty(propertyName);
                propertyAccess = Expression.MakeMemberAccess(parameter, property);
            }
            resultType = property.PropertyType;
            return Expression.Lambda(propertyAccess, parameter);
        }
    }
}