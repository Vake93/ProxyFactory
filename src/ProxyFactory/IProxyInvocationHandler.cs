namespace ProxyFactory
{
	public interface IProxyInvocationHandler<T>
        where T : class
    {
        void Invoked(string methodName);
    }
}
