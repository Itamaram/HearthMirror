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
            if (type.IsPrimitive)
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

            var mi = (MonoItem)obj;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var items = (object[])mi["_items"];
                var size = (int)mi["_size"];

                var list = (IList) Activator.CreateInstance(type);

                var gt = type.GenericTypeArguments[0];

                for (var i = 0; i < size; i++)
                    list.Add(items[i].Deserialise(gt));

                return list;
            }

            //todo dictionary

            var result = Activator.CreateInstance(type);

            foreach (var prop in type.GetProperties())
                prop.SetValue(result, Deserialise(mi[prop.GetPropertyName()], prop.PropertyType));

            return result;
        }

        private static string GetPropertyName(this PropertyInfo prop)
        {
            // todo backing field?
            return prop.GetCustomAttribute<SourceNameAttribute>()?.Name ?? prop.Name;
        }

        public static T Deserialise<T>(this MonoItem item) => (T)item.Deserialise(typeof(T));
    }
}