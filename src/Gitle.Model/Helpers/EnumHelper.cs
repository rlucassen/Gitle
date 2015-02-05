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
            Type enumType = typeof(T);

            // Can't use type constraints on value types, so have to do check like this
            if (enumType.BaseType != typeof(Enum))
                throw new ArgumentException("T must be of type System.Enum");

            Array enumValArray = Enum.GetValues(enumType);

            List<T> enumValList = new List<T>(enumValArray.Length);

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

            ArrayList list = new ArrayList();
            Array enumValues = Enum.GetValues(type);

            foreach (Enum value in enumValues)
            {
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
            Array enumValues = Enum.GetValues(type);

            foreach (Enum value in enumValues)
            {
                dict[value.GetHashCode()] = value.GetDescription();
            }

            return dict;
        }

        public static string GetDescription(this object value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attr =
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