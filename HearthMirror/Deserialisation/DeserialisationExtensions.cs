using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HearthMirror.Mono;

namespace HearthMirror.Deserialisation
{
    public static class DeserialisationExtensions
    {
        public static object Deserialise(this object obj, Type type)
        {
            if (obj == null)
                return null;

            if (type.IsPrimitive || type == typeof(string) || type == typeof(MonoItem) || type == typeof(object))
                return obj;

            if (type.IsArray && type.GetElementType().IsPrimitive)
            {
                // todo confirm if this is needed, or if I can pass thru
                var src = (object[])obj;
                var dst = Array.CreateInstance(type.GetElementType(), src.Length);
                Array.Copy(src, dst, src.Length);
                return dst;
            }

            if (type.IsArray)
            {
                var src = (object[])obj;
                var dst = Array.CreateInstance(type.GetElementType(), src.Length);

                for (var i = 0; i < src.Length; i++)
                    dst.SetValue(Deserialise(src[i], type.GetElementType()), i);

                return dst;
            }

            if (type.IsEnum)
                return Enum.ToObject(type, (int)obj);

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (obj.GetType() == type.GenericTypeArguments[0])
                    return obj;
                
                var mi = (MonoItem)obj;
                return (bool) mi["has_value"] ? mi["value"] : null;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var mi = (MonoItem)obj;

                var items = (object[])mi["_items"];
                var size = (int)mi["_size"];

                var list = (IList)Activator.CreateInstance(type);

                var gt = type.GenericTypeArguments[0];

                for (var i = 0; i < size; i++)
                    list.Add(items[i].Deserialise(gt));

                return list;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var mi = (MonoItem)obj;

                var dict = (IDictionary) Activator.CreateInstance(type);
                var count = (int) mi["count"];
                var entries = (object[]) mi["entries"];

                for (var i = 0; i < count; i++)
                {
                    var entry = entries[i].Deserialise<DictionaryEntry>();
                    if (entry.HashCode >= 0)
                        dict.Add(entry.Key.Deserialise(type.GenericTypeArguments[0]), entry.Value.Deserialise(type.GenericTypeArguments[1]));
                }

                return dict;
            }

            return PropertywiseDeserialisation(obj, type);
        }

        private static object PropertywiseDeserialisation(object obj, Type type)
        {
            var result = Activator.CreateInstance(type);
            var mi = (MonoItem)obj;

            foreach (var prop in type.GetProperties())
                prop.SetValue(result, mi[prop.GetPropertyName()].Deserialise(prop.PropertyType));

            return result;
        }

        private static string GetPropertyName(this PropertyInfo prop)
        {
            // todo backing field?
            return prop.GetCustomAttribute<SourceNameAttribute>()?.Name ?? prop.Name;
        }

        public static T Deserialise<T>(this object item) => (T)item.Deserialise(typeof(T));

        public static T Deserialise<T>(this MonoItem mi, string property)
            => mi[property].Deserialise<T>();

        public static T Deserialise<T>(this MonoClass mc, string property)
            => mc[property].Deserialise<T>();

        public static T Deserialise<T>(this MonoClass mc)
        {
            var result = Activator.CreateInstance(typeof(T));

            foreach (var prop in typeof(T).GetProperties())
                prop.SetValue(result, mc[prop.GetPropertyName()].Deserialise(prop.PropertyType));

            return (T)result;
        }

        private class DictionaryEntry
        {
            [SourceName("hashCode")]
            public int HashCode { get; set; }

            [SourceName("key")]
            public object Key { get; set; }

            [SourceName("value")]
            public object Value { get; set; }
        }
    }
}