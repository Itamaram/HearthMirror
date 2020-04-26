using System.Collections.Concurrent;
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
                return ObjectValueExtractor.GetValue(val, invocation.Method.ReturnType);
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

    public class MonoClassInterceptor : MonoInterceptor
    {
        private readonly MonoClass mc;

        public MonoClassInterceptor(MonoClass mc)
        {
            this.mc = mc;
        }

        protected override object GetPropertyValue(string s) => mc[s];
    }
}

