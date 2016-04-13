namespace Gitle.Model.Helpers
{
    using System.Collections;
    using System.ComponentModel;
    using System.Reflection;
    using System;
    using System.Collections.Generic;

    public static class EnumHelper
    {
        ///<summary>
        /// Maakt van een enumeratie een lijst
        ///</summary>
        ///<typeparam name="T"></typeparam>
        ///<returns></returns>
        ///<exception cref="ArgumentException"></exception>
        public static List<T> EnumToList<T>()
        {
            var enumType = typeof(T);

            // Can't use type constraints on value types, so have to do check like this
            if (enumType.BaseType != typeof(Enum))
                throw new ArgumentException("T must be of type System.Enum");

            var enumValArray = Enum.GetValues(enumType);

            var enumValList = new List<T>(enumValArray.Length);

            foreach (int val in enumValArray)
            {
                enumValList.Add((T)Enum.Parse(enumType, val.ToString()));
            }

            return enumValList;
        }

        public static IList ToList(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            var list = new ArrayList();
            var enumValues = Enum.GetValues(type);

            foreach (Enum value in enumValues)
            {
                if(!string.IsNullOrEmpty(value.GetDescription()))
                    list.Add(new { value = value.GetHashCode(), text = value.GetDescription(), name = value });
            }

            return list;
        }

        public static IDictionary<int, String> ToDictionary(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            IDictionary<int, String> dict = new Dictionary<int, String>();
            var enumValues = Enum.GetValues(type);

            foreach (Enum value in enumValues)
            {
                dict[value.GetHashCode()] = value.GetDescription();
            }

            return dict;
        }

        public static string GetDescription(this object value)
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            if (name != null)
            {
                var field = type.GetField(name);
                if (field != null)
                {
                    var attr =
                           Attribute.GetCustomAttribute(field,
                             typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null)
                    {
                        return attr.Description;
                    }
                }
            }
            return null;
        }

    }
}