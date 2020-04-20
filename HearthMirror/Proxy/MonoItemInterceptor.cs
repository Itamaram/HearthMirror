using System;
using Castle.DynamicProxy;
using HearthMirror.Mono;

namespace HearthMirror.Proxy
{
    public class MonoItemInterceptor :IInterceptor
    {
        private readonly MonoItem mi;

        public MonoItemInterceptor(MonoItem mi)
        {
            this.mi = mi;
        }

        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();

            // todo try catch
            var val = mi[invocation.Method.Name.Substring(4)];

            if (val is MonoItem m)
                invocation.ReturnValue = Generator.GetItem(m, invocation.TargetType);
            else
                invocation.ReturnValue = val;
        }
    }

    // todo separate classes into two separate dto objects
    // one for the static members, and another for instances

    public class MonoClassInterceptor : IInterceptor
    {
        private readonly MonoClass mc;

        public MonoClassInterceptor(MonoClass mc)
        {
            this.mc = mc;
        }

        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();

            // todo try catch
            var val = mc[invocation.Method.Name.Substring(4)];

            if (val is MonoItem m)
                invocation.ReturnValue = Generator.GetItem(m, invocation.TargetType);
            else
                invocation.ReturnValue = val;
        }
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

