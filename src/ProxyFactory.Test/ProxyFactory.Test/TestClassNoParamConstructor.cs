using System;

namespace ProxyFactory.Test
{
    public class TestClassNoParamConstructor
    {
        public virtual void TestFunctionOne()
        {
            Console.WriteLine($"In {nameof(TestFunctionOne)}");
        }

        public virtual void TestFunctionTwo(string test)
        {
            Console.WriteLine($"In {nameof(TestFunctionOne)}, {nameof(test)} = {test}");
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

        public virtual void TestFunctionFive()
        {
            Console.WriteLine($"Method {nameof(TestFunctionFive)} executed");
        }

        public virtual void TestFunctionSix(string text)
        {
            Console.WriteLine($"Method {nameof(TestFunctionFive)} executed, {nameof(text)} = {text}");
        }
    }
}
