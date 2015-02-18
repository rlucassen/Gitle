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
            ParameterExpression parameter = Expression.Parameter(typeof(T), "Entity");
            //  create the selector part, but support child properties
            PropertyInfo property;
            Expression propertyAccess;
            if (propertyName.Contains('.'))
            {
                // support to be sorted on child fields.
                string[] childProperties = propertyName.Split('.');
                property = typeof(T).GetProperty(childProperties[0]);
                propertyAccess = Expression.MakeMemberAccess(parameter, property);
                for (int i = 1; i < childProperties.Length; i++)
                {
                    PropertyInfo tempProp = property;
                    property = tempProp.PropertyType.GetProperty(childProperties[i], BindingFlags.FlattenHierarchy | BindingFlags.Instance);
                    if (property == null)
                        property =
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