using System;
using System.Collections.Concurrent;
using Castle.DynamicProxy;
using HearthMirror.Mono;

namespace HearthMirror.Proxy
{

    public abstract class MonoInterceptor : IInterceptor
    {
        private readonly ProxyFactory proxy;

        protected MonoInterceptor(ProxyFactory proxy)
        {
            this.proxy = proxy;
        }

        protected abstract object GetPropertyValue(string s);

        private readonly ConcurrentDictionary<string, object> cache
            = new ConcurrentDictionary<string, object>();

        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();

            invocation.ReturnValue = cache.GetOrAdd(GetPropertyName(invocation), name =>
            {
                var type = invocation.Method.ReturnType;

                try
                {
                    return GetValue(name, type);
                }
                catch
                {
                    proxy.Mirror.Clean();

                    try
                    {
                        return GetValue(name, type);
                    }
                    catch
                    {
                        return default;
                    }
                }
            });
        }

        private static string GetPropertyName(IInvocation invocation)
        {
            // todo invocation.Method.GetCustomAttribute<SourceNameAttribute>().Name
            return invocation.Method.Name.Substring(4);
        }

        private object GetValue(string prop, Type type)
            => new ObjectValueExtractor(proxy).GetValue(GetPropertyValue(prop), type);
    }

    public class MonoItemInterceptor : MonoInterceptor
    {
        private readonly MonoItem mi;

        public MonoItemInterceptor(MonoItem mi, ProxyFactory proxy) : base(proxy)
        {
            this.mi = mi;
        }

        protected override object GetPropertyValue(string s) => mi[s];
    }

    public class MonoClassInterceptor : MonoInterceptor
    {
        private readonly MonoClass mc;

        public MonoClassInterceptor(MonoClass mc, ProxyFactory proxy) : base(proxy)
        {
            this.mc = mc;
        }

        protected override object GetPropertyValue(string s) => mc[s];
    }
}

