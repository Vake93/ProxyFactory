using System;

namespace ProxyFactory.Test
{
    public interface ITestInterface
    {
        void TestMethodOne();

        void TestMethodTwo(string test);

        string TestMethodThree();

        string TestMethodFour(string test);

        void TestMethodFive();

        string TestMethodSix(string test);
    }

    public sealed class TestClassWithInterface : ITestInterface
    {
        public void TestMethodOne()
        {
            Console.WriteLine($"In {nameof(TestClassWithInterface)}, method {nameof(TestMethodOne)}");
        }

        public void TestMethodTwo(string test)
        {
            Console.WriteLine($"In {nameof(TestClassWithInterface)}, method {nameof(TestMethodTwo)}");
        }

        public string TestMethodThree()
        {
            return $"In {nameof(TestClassWithInterface)}, method {nameof(TestMethodThree)}";
        }

        public string TestMethodFour(string test)
        {
            return $"In {nameof(TestClassWithInterface)}, method {nameof(TestMethodFour)}";
        }

        public void TestMethodFive()
        {
            Console.WriteLine($"In {nameof(TestClassWithInterface)}, method {nameof(TestMethodFive)}");
        }

        public string TestMethodSix(string test)
        {
            return $"In {nameof(TestClassWithInterface)}, method {nameof(TestMethodSix)}";
        }
    }
}
