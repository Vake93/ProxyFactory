using NUnit.Framework;
using System;

namespace ProxyFactory.Test
{
    public class ProxyTestClassNoParamConstructor
    {
        private TestProxy<TestClassNoParamConstructor, TestClassNoParamConstructor> TestProxy =>
            TestProxy<TestClassNoParamConstructor, TestClassNoParamConstructor>.Instance;

        [Test]
        public void TestMethodCall()
        {
            var test = TestProxy.NewInstance(ProxyFactory.ProxyType.Inheritance);

            test.TestFunctionOne();

            test = TestProxy.NewInstance(ProxyFactory.ProxyType.Inheritance);

            test.TestFunctionOne();

            Assert.Pass();
        }

        [Test]
        public void TestMethodWithParamCall()
        {
            var test = TestProxy.NewInstance(ProxyFactory.ProxyType.Inheritance);

            test.TestFunctionTwo("Test String");

            Assert.Pass();
        }

        [Test]
        public void TestMethodWithReturnCall()
        {
            var test = TestProxy.NewInstance(ProxyFactory.ProxyType.Inheritance);

            var value = test.TestFunctionThree();

            Assert.AreEqual("From TestFunctionThree", value);

            Assert.Pass();
        }
        [Test]
        public void TestMethodWithParamAndReturnCall()
        {
            var test = TestProxy.NewInstance(ProxyFactory.ProxyType.Inheritance);

            var value = test.TestFunctionFour("Test String");

            Assert.AreEqual("From TestFunctionFour", value);
            Assert.Pass();
        }

        [Test]
        public void TestMethodCallException()
        {
            var test = TestProxy.NewInstance(ProxyFactory.ProxyType.Inheritance);

            Assert.Throws(typeof(UnauthorizedAccessException), () => test.TestFunctionFive());

            Assert.Pass();
        }

        [Test]
        public void TestMethodWithParamCallException()
        {
            var test = TestProxy.NewInstance(ProxyFactory.ProxyType.Inheritance);

            Assert.Throws(typeof(UnauthorizedAccessException), () => test.TestFunctionSix("Test"));

            Assert.Pass();
        }
    }
}