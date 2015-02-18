namespace Gitle.Model.Helpers
{
    using System;
    using System.Collections.Generic;
    using NHibernate;
    using NHibernate.Metadata;

    public static class NHibernateMetadataHelper
    {
         public static bool IsMapped<T>(ISessionFactory sessionFactory, string propertyName)
         {
             Type type = typeof (T);
             IClassMetadata meta = sessionFactory.GetClassMetadata(type);

             var propertyNames = new List<string>(meta.PropertyNames);

             return propertyNames.Contains(propertyName);
         }
    }
}