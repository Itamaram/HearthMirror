using System;
using Castle.DynamicProxy;
using HearthMirror.Mono;

namespace HearthMirror.Proxy
{
    public class ProxyFactory
    {
        private readonly ProxyGenerator generator = new ProxyGenerator();

        private readonly Lazy<Mirror> mirror;

        public ProxyFactory(string process)
        {
            mirror = new Lazy<Mirror>(() => new Mirror(process));
        }

        public Mirror Mirror => mirror.Value;

        public T GetClass<T>() where T : class
        {
            var proc = mirror.Value.Proc;

            if (proc == null)
                return default;

            if (proc.HasExited)
                mirror.Value.Clean();

            mirror.Value.View?.ClearCache();

            var prop = typeof(T).Name.Remove(typeof(T).Name.Length - "Class".Length);

            return generator.CreateClassProxy<T>(new MonoClassInterceptor(mirror.Value.Root[prop], this));
        }

        public object GetItem(MonoItem mi)
            => generator.CreateClassProxy(mi.GetInternalType(), new MonoItemInterceptor(mi, this));
    }
}