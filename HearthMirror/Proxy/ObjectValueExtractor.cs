using System;
using System.Collections;
using System.Collections.Generic;
using HearthMirror.Mono;

namespace HearthMirror.Proxy
{
    public class ObjectValueExtractor
    {
        private readonly ProxyFactory proxy;

        public ObjectValueExtractor(ProxyFactory proxy)
        {
            this.proxy = proxy;
        }

        public object GetValue(object raw, Type type)
        {
            if (raw == default)
                return default;
            
            if (type.IsPrimitive || type == typeof(string))
                return raw;

            if (type.IsEnum)
                return Enum.ToObject(type, (int)raw);
            
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (raw.GetType() == type.GenericTypeArguments[0])
                    return raw;

                var mi = (MonoItem) raw;
                return (bool) mi["has_value"] ? mi["value"] : null;
            }

            if (type.IsArray)
            {
                var gt = type.GetElementType();

                var src = (object[])raw;
                var dst = Array.CreateInstance(gt, src.Length);

                for (var i = 0; i < src.Length; i++)
                    dst.SetValue(GetValue(src[i], gt), i);

                return dst;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var mi = (MonoItem) raw;

                var items = (object[])mi["_items"];
                var size = (int)mi["_size"];

                var list = (IList)Activator.CreateInstance(type); 

                for (var i = 0; i < size; i++)
                    list.Add(GetValue(items[i], type.GenericTypeArguments[0]));

                return list;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var mi = (MonoItem) raw;

                var gt = type.GenericTypeArguments;

                var dst = (IDictionary) Activator.CreateInstance(type);

                var count = (int) mi["count"];
                var entries = (object[]) mi["entries"];

                for (var i = 0; i < count; i++)
                {
                    var entry = (MonoItem) entries[i];

                    if ((int) entry["hashCode"] >= 0)
                        dst.Add(GetValue(entry["key"], gt[0]), GetValue(entry["value"], gt[1]));
                }

                return dst;
            }

            if (raw is MonoItem m)
                return proxy.GetItem(m);

            // TODO handle this more gracefully, potentially make this extensible?
            // Looks like a missed chance for an open/close pattern
            throw new ArgumentException();
        }
    }
}