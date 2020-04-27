using System.Collections.Concurrent;
using Castle.DynamicProxy;
using HearthMirror.Mono;

namespace HearthMirror.Proxy
{
    //public interface ProxyNode
    //{
    //    ProxyFactory ProxyFactory { get; }
    //    void Refresh();
    //    object GetProperty(string name);
    //}

    //public class Root : ProxyNode
    //{
    //    public Root(ProxyFactory proxy)
    //    {
    //        ProxyFactory = proxy;
    //    }

    //    public ProxyFactory ProxyFactory { get; }

    //    public void Refresh()
    //    {
    //        ProxyFactory.Mirror.Clean();
    //    }

    //    public object GetProperty(string name)
    //    {
    //        var proc = ProxyFactory.Mirror.Proc;

    //        if (proc == null)
    //            return default;

    //        if (proc.HasExited)
    //            ProxyFactory.Mirror.Clean();

    //        ProxyFactory.Mirror.View?.ClearCache();

    //        return ProxyFactory.Mirror.Root[name];
    //    }
    //}

    //public class Child<T> : ProxyNode
    //{
    //    private readonly ProxyNode parent;
    //    private readonly string self;
    //    private readonly Func<T, string, object> get;

    //    public Child(ProxyNode parent, string name, Func<T, string, object> get)
    //    {
    //        this.parent = parent;
    //        self = name;
    //        this.get = get;
    //        item = GetItemValue();
    //    }

    //    private T item;

    //    private T GetItemValue() => (T)parent.GetProperty(self);

    //    public ProxyFactory ProxyFactory => parent.ProxyFactory;

    //    public void Refresh()
    //    {
    //        parent.Refresh();
    //        item = GetItemValue();
    //    }

    //    public object GetProperty(string name) => get(item, name);
    //}

    //public class MonoNodeInterceptor : IInterceptor
    //{
    //    private readonly ProxyNode node;

    //    public MonoNodeInterceptor(ProxyNode node)
    //    {
    //        this.node = node;
    //    }

    //    private readonly ConcurrentDictionary<string, object> cache
    //        = new ConcurrentDictionary<string, object>();

    //    public void Intercept(IInvocation invocation)
    //    {
    //        invocation.Proceed();

    //        invocation.ReturnValue = cache.GetOrAdd(GetPropName(invocation), name =>
    //        {
    //            var val = GetPropertyValue(name);

    //            var extracted = ObjectValueExtractor.GetValue(val, invocation.Method.ReturnType);

    //            if (extracted is MonoItem mi)
    //                return node.ProxyFactory.GetItem(mi, new Child<MonoItem>(node, name, (x, s) => x[s]));

    //            return extracted;
    //        });
    //    }

    //    private static string GetPropName(IInvocation invocation)
    //    {
    //        // todo invocation.Method.GetCustomAttribute<SourceNameAttribute>().Name
    //        return invocation.Method.Name.Substring(4);
    //    }

    //    private object GetPropertyValue(string name)
    //    {
    //        try
    //        {
    //            return node.GetProperty(name);
    //        }
    //        catch
    //        {
    //            cache.Clear();
    //            node.Refresh();

    //            try
    //            {
    //                return node.GetProperty(name);
    //            }
    //            catch
    //            {
    //                return default;
    //            }
    //        }
    //    }
    //}

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
               var val = GetPropertyValueInternal(name);
               return new ObjectValueExtractor(proxy).GetValue(val, invocation.Method.ReturnType);
           });
        }

        private static string GetPropertyName(IInvocation invocation)
        {
            // todo invocation.Method.GetCustomAttribute<SourceNameAttribute>().Name
            return invocation.Method.Name.Substring(4);
        }

        private object GetPropertyValueInternal(string property)
        {
            try
            {
                return GetPropertyValue(property);
            }
            catch
            {
                proxy.Mirror.Clean();

                try
                {
                    return GetPropertyValue(property);
                }
                catch
                {
                    return default;
                }
            }
        }

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

