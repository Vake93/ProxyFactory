using System;

namespace ProxyFactory.Test
{
    public class TestProxy<T, K> : IProxyInvocationHandler
        where T : class, K
        where K : class
    {
        public static TestProxy<T, K> Instance { get; }

        static TestProxy()
        {
            Instance = new TestProxy<T, K>();
        }

        private TestProxy()
        {
        }

        public K NewInstance(ProxyFactory.ProxyType proxyType, params object[] args)
        {
            var proxy = ProxyFactory.Instance
                .Create<T, K>(this, proxyType, args);

            return proxy;
        }

        public void Invoked(string methodName)
        {
            if (methodName.EndsWith("Five") || methodName.EndsWith("Six"))
            {
                throw new UnauthorizedAccessException();
            }

            Console.WriteLine($"Method {methodName} invoked");
        }
    }
}
