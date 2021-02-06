using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace BenchmarkDynamicInvocation
{

    public class MethodCalls
    {
        private static readonly Type ClassType = typeof(Foo);
        private static readonly MethodInfo Method = ClassType.GetMethod(nameof(Foo.Bar));

        [Benchmark(Baseline = true)]
        public int DirectCall()
        {
            var foo = new Foo();
            
            return foo.Bar(1, 2, false);
        }

        [Benchmark]
        public int MethodInfoInvoke()
        {
            object foo = new Foo();

            return (int)Method.Invoke(foo, new[] { (object)1, (object)2, (object)false });
        }

        [Benchmark]
        public int DynamicCastInvoke()
        {
            object foo = new Foo();
            dynamic dynamicFoo = foo as dynamic;

            return dynamicFoo.Bar(1, 2, false);
        }

        [Benchmark]
        public int DelegateDynamicInvoke()
        {
            object foo = new Foo();

            var delegateType = Expression.GetDelegateType(typeof(Foo), typeof(int), typeof(int), typeof(bool), typeof(int));
            var @delegate = Delegate.CreateDelegate(delegateType, Method);
            
            var firstCallTimer = Stopwatch.StartNew();
            
            return (int)@delegate.DynamicInvoke(new[] { foo, (object)1, (object)2, (object)false });
        }

        [Benchmark]
        public int FuncCall()
        {
            Foo foo = new();

            var delegateType = Expression.GetDelegateType(typeof(Foo), typeof(int), typeof(int), typeof(bool), typeof(int));
            var func = (Func<Foo, int, int, bool, int>)Delegate.CreateDelegate(delegateType, Method);

            return func(foo, 1, 2, false);
        }
    }
}
