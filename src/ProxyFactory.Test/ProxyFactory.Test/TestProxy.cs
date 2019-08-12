using System;

namespace ProxyFactory.Test
{
    public class TestProxy<T> : IProxyInvocationHandler<T>
        where T : class
    {
        public static TestProxy<T> Instance { get; }

        static TestProxy()
        {
            Instance = new TestProxy<T>();
        }

        private TestProxy()
        {
        }

        public T NewInstance(params object[] args)
        {
            var proxy = ProxyFactory.Instance
                .Create(this, args);

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
