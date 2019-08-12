using NUnit.Framework;
using System;
namespace ProxyFactory.Test
{
    public class ProxyTestClassWithInterface
    {
        private TestProxy<TestClassWithInterface, ITestInterface> TestProxy =>
            TestProxy<TestClassWithInterface, ITestInterface>.Instance;

        [Test]
        public void TestMethodCall()
        {
            var test = TestProxy.NewInstance(ProxyFactory.ProxyType.Interfaces);

            test.TestMethodOne();

            Assert.Pass();
        }

        [Test]
        public void TestMethodCallWithParam()
        {
            var test = TestProxy.NewInstance(ProxyFactory.ProxyType.Interfaces);

            test.TestMethodTwo("Test String");

            Assert.Pass();
        }

        [Test]
        public void TestMethodWithReturnCall()
        {
            var test = TestProxy.NewInstance(ProxyFactory.ProxyType.Interfaces);

            var value = test.TestMethodThree();

            Assert.AreEqual("In TestClassWithInterface, method TestMethodThree", value);
            Assert.Pass();
        }
        [Test]
        public void TestMethodWithParamAndReturnCall()
        {
            var test = TestProxy.NewInstance(ProxyFactory.ProxyType.Interfaces);

            var value = test.TestMethodFour("Test String");

            Assert.AreEqual("In TestClassWithInterface, method TestMethodFour", value);
            Assert.Pass();
        }
    }
}
