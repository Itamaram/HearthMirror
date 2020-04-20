using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy;
using HearthMirror.Mono;

namespace HearthMirror.Proxy
{
    public abstract class MonoInterceptor : IInterceptor
    {
        protected abstract object GetPropertyValue(string s);

        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();

            var val = GetPropertyValue(invocation);

            invocation.ReturnValue = GetValue(val, invocation.Method.ReturnType);
        }

        private object GetPropertyValue(IInvocation invocation)
        {
            try
            {
                return GetPropertyValue(invocation.Method.Name.Substring(4));
            }
            catch
            {
                return default;
            }
        }

        private static object GetValue(object raw, Type target)
        {
            if (raw == default || IsUnwrappedType(target))
                return raw;

            if (raw is MonoItem m)
                return Generator.GetItem(m, target);

            if (target.IsArray)
            {
                var gt = target.GetElementType();

                if (IsUnwrappedType(gt))
                    return raw;

                var src = (object[])raw;
                var dst = Array.CreateInstance(gt, src.Length);

                for (var i = 0; i < src.Length; i++)
                    dst.SetValue(GetValue(src[i], gt), i);

                return dst;
            }

            if (target.IsGenericType && target.GetGenericTypeDefinition() == typeof(List<>))
            {
                var gt = target.GenericTypeArguments[0];

                if (IsUnwrappedType(gt))
                    return raw;

                var src = (IList) raw;
                var dst = (IList)Activator.CreateInstance(target);

                foreach (var item in src)
                    dst.Add(GetValue(item, gt));

                return dst;
            }

            
            if (target.IsGenericType && target.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                if (target.GenericTypeArguments.All(IsUnwrappedType))
                    return raw;

                var src = (IDictionary) raw;
                var dst = (IDictionary) Activator.CreateInstance(target);

                foreach (DictionaryEntry entry in src)
                    dst[GetValue(entry.Key, target.GenericTypeArguments[0])]
                        = GetValue(entry.Value, target.GenericTypeArguments[1]);

                return dst;
            }

            throw new ArgumentException();
        }

        private static bool IsUnwrappedType(Type t) => t.IsPrimitive || t.IsEnum || t == typeof(string);
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
}

