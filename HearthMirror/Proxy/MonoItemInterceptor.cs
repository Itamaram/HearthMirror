using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy;
using HearthMirror.Mono;

namespace HearthMirror.Proxy
{
    public abstract class MonoInterceptor : IInterceptor
    {
        protected abstract object GetPropertyValue(string s);

        private readonly ConcurrentDictionary<string, object> cache
            = new ConcurrentDictionary<string, object>();

        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();

            invocation.ReturnValue = cache.GetOrAdd(invocation.Method.Name, _ =>
            {
                var val = GetPropertyValue(invocation);
                return GetValue(val, invocation.Method.ReturnType);
            });
        }

        private object GetPropertyValue(IInvocation invocation)
        {
            try
            {
                // todo invocation.Method.GetCustomAttribute<SourceNameAttribute>().Name
                return GetPropertyValue(invocation.Method.Name.Substring(4));
            }
            catch
            {
                return default;
            }
        }

        private static object GetValue(object raw, Type type)
        {
            if (raw == default)
                return default;
            
            if (IsUnwrappedType(type))
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

                //if (IsUnwrappedType(gt))
                //    return raw;

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
                return Generator.GetItem(m, m.GetInternalType()); // todo don't user target, find corresponding type instead

            throw new ArgumentException();
        }

        private static bool IsUnwrappedType(Type t) => t.IsPrimitive || t == typeof(string);
    }

    public class MonoItemInterceptor : MonoInterceptor
    {
        private readonly MonoItem mi;

        public MonoItemInterceptor(MonoItem mi)
        {
            this.mi = mi;
        }

        protected override object GetPropertyValue(string s) => mi[s];
    }

    // todo separate classes into two separate dto objects
    // one for the static members, and another for instances

    public class MonoClassInterceptor : MonoInterceptor
    {
        private readonly MonoClass mc;

        public MonoClassInterceptor(MonoClass mc)
        {
            this.mc = mc;
        }

        protected override object GetPropertyValue(string s) => mc[s];
    }

    public static class Generator
    {
        private static readonly ProxyGenerator Instance = new ProxyGenerator();

        public static T GetClass<T>(MonoClass mc) where T : class
            => Instance.CreateClassProxy<T>(new MonoClassInterceptor(mc));

        public static object GetItem(MonoItem mi, Type t)
            => Instance.CreateClassProxy(t, new MonoItemInterceptor(mi));
    }

    public static class TypeMap
    {
        private const string Namespace = "UnityModels";

        private static readonly IReadOnlyDictionary<string, Type> Types =
            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.FullName.StartsWith(Namespace))
                .ToDictionary(t => t.FullName.Substring(Namespace.Length + 1));

        public static Type GetInternalType(this MonoItem mi) =>
            Types.TryGetValue(mi.Class.FullName, out var type)
                ? type
                : throw new Exception($"External type {mi.Class.FullName} was not mapped internally");
    }
}

