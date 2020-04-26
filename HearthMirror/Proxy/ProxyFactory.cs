using Castle.DynamicProxy;
using HearthMirror.Mono;

namespace HearthMirror.Proxy
{
    public static class ProxyFactory
    {
        private static readonly ProxyGenerator Instance = new ProxyGenerator();

        public static T WrapClass<T>(MonoClass mc) where T : class
            => Instance.CreateClassProxy<T>(new MonoClassInterceptor(mc));

        public static object WrapItem(MonoItem mi)
            => Instance.CreateClassProxy(mi.GetInternalType(), new MonoItemInterceptor(mi));
    }
}