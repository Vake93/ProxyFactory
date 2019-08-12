using NUnit.Framework;
using System;

namespace ProxyFactory.Test
{
    public class ProxyTestClassNoParamConstructor
    {
        private TestProxy<TestClassNoParamConstructor> TestProxy =>
            TestProxy<TestClassNoParamConstructor>.Instance;

        [Test]
        public void TestMethodCall()
        {
            var test = TestProxy.NewInstance();

            test.TestFunctionOne();

            Assert.Pass();
        }

        [Test]
        public void TestMethodWithParamCall()
        {
            var test = TestProxy.NewInstance();

            test.TestFunctionTwo("Test String");

            Assert.Pass();
        }

        [Test]
        public void TestMethodWithReturnCall()
        {
            var test = TestProxy.NewInstance();

            var value = test.TestFunctionThree();

            Assert.AreEqual("From TestFunctionThree", value);

            Assert.Pass();
        }
        [Test]
        public void TestMethodWithParamAndReturnCall()
        {
            var test = TestProxy.NewInstance();

            var value = test.TestFunctionFour("Test String");

            Assert.AreEqual("From TestFunctionFour", value);
            Assert.Pass();
        }

        [Test]
        public void TestMethodCallException()
        {
            var test = TestProxy.NewInstance();

            Assert.Throws(typeof(UnauthorizedAccessException), () => test.TestFunctionFive());

            Assert.Pass();
        }

        [Test]
        public void TestMethodWithParamCallException()
        {
            var test = TestProxy.NewInstance();

            Assert.Throws(typeof(UnauthorizedAccessException), () => test.TestFunctionSix("Test"));

            Assert.Pass();
        }
    }
}