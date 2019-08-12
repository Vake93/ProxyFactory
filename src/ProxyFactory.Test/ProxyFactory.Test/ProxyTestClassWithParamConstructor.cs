using NUnit.Framework;

namespace ProxyFactory.Test
{
    class ProxyTestClassWithParamConstructor
    {
        private TestProxy<TestClassWithParamConstructor> TestProxy =>
            TestProxy<TestClassWithParamConstructor>.Instance;

        [Test]
        public void TestMethodCall()
        {
            var test = TestProxy.NewInstance("Test");

            test.TestFunctionOne();

            Assert.Pass();
        }

        [Test]
        public void TestMethodWithParamCall()
        {
            var test = TestProxy.NewInstance("Test");

            test.TestFunctionTwo("Test String");

            Assert.Pass();
        }

        [Test]
        public void TestMethodWithReturnCall()
        {
            var test = TestProxy.NewInstance("Test");

            var value = test.TestFunctionThree();

            Assert.AreEqual("From TestFunctionThree", value);
            Assert.Pass();
        }
        [Test]
        public void TestMethodWithParamAndReturnCall()
        {
            var test = TestProxy.NewInstance("Test");

            var value = test.TestFunctionFour("Test String");

            Assert.AreEqual("From TestFunctionFour", value);
            Assert.Pass();
        }
    }
}
