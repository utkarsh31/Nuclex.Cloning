using System.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using Nuclex.Cloning.Attributes;

namespace Nuclex.Cloning.Helpers
{
    /// <summary>Contains helper methods for the cloners</summary>
    internal static class ClonerHelpers
    {
        /// <summary>
        ///     Returns all the fields of a type, working around a weird reflection issue
        ///     where explicitly declared fields in base classes are returned, but not
        ///     automatic property backing fields.
        /// </summary>
        /// <param name="type">Type whose fields will be returned</param>
        /// <param name="bindingFlags">Binding flags to use when querying the fields</param>
        /// <returns>All of the type's fields, including its base types</returns>
        public static FieldInfo[] GetFieldInfosIncludingBaseClasses(
            Type type, BindingFlags bindingFlags
            )
        {
            var fields = type.GetWriteableFields(bindingFlags);

            // If this class doesn't have a base, don't waste any time
            if (type.BaseType == typeof (object))
            {
                return fields.ToArray();
            }

            // Otherwise, collect all types up to the furthest base class
            var fieldInfoList = new List<FieldInfo>(fields);
            while (type.BaseType != typeof (object))
            {
                type = type.BaseType;
                fields = type.GetWriteableFields(bindingFlags);

                // Look for fields we do not have listed yet and merge them into the main list
                foreach (var field in fields)
                {
                    var found = fieldInfoList.Any(t => (t.DeclaringType == field.DeclaringType) && (t.Name == field.Name));

                    if (!found)
                    {
                        fieldInfoList.Add(field);
                    }
                }
            }

            return fieldInfoList.ToArray();
        }

        private static FieldInfo[] GetWriteableFields(this Type type, BindingFlags bindingFlags)
        {
            return type.GetFields(bindingFlags).Where(i => !i.IsInitOnly && !i.HasAttribute<CloneIgnore>()).ToArray();
        }

        public static bool HasAttribute<TAttribute>(this FieldInfo type) where TAttribute : Attribute
        {
            var att = type.GetCustomAttributes(typeof (TAttribute), true).FirstOrDefault() as TAttribute;

            return att != null;
        }

        public static TValue GetAttributePropertyValue<TAttribute, TValue>(this FieldInfo type, string propertyName) where TAttribute : Attribute
        {
            var att = type.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute;

            if (att != null)
            {
                var sourceType = att.GetType();
                var value = sourceType.GetProperty(propertyName).GetValue(att, null);
                if (value != null)
                {
                    return (TValue) value;
                }
            }

            return default(TValue);
        }
    }
}