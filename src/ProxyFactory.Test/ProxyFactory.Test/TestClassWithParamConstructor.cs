using System;

namespace ProxyFactory.Test
{
    public class TestClassWithParamConstructor
    {
        public TestClassWithParamConstructor(string paramConstructor)
        {
            Console.WriteLine($"paramConstructor: {paramConstructor}");
        }

        public virtual void TestFunctionOne()
        {
            Console.WriteLine($"In {nameof(TestFunctionOne)}");
        }

        public virtual void TestFunctionTwo(string test)
        {
            Console.WriteLine($"In {nameof(TestFunctionTwo)}, {nameof(test)} = {test}");
        }

        public virtual string TestFunctionThree()
        {
            Console.WriteLine($"In {nameof(TestFunctionThree)}");

            return $"From {nameof(TestFunctionThree)}";
        }

        public virtual string TestFunctionFour(string test)
        {
            Console.WriteLine($"In {nameof(TestFunctionFour)}, {nameof(test)} = {test}");

            return $"From {nameof(TestFunctionFour)}";
        }
    }
}
