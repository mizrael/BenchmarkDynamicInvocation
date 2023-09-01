using BenchmarkDotNet.Attributes;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace BenchmarkDynamicInvocation
{
    public class Benchmarks
    {
        private static readonly Type ClassType = typeof(Foo);
        private static readonly MethodInfo MethodToCall = ClassType.GetMethod(nameof(Foo.Bar));
        private static readonly object[] MethodArguments = new[] { 1, 2, (object)false };

        private static Type DelegateType = Expression.GetDelegateType(typeof(Foo), typeof(int), typeof(int), typeof(bool), typeof(int));
        private static Delegate DelegateToCall = Delegate.CreateDelegate(DelegateType, MethodToCall);
        private static Func<Foo, int, int, bool, int> FuncToCall = (Func<Foo, int, int, bool, int>)Delegate.CreateDelegate(DelegateType, MethodToCall);

        private Foo _fooInstance;
        private object[] _delegateArguments;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _fooInstance = new Foo();
            _delegateArguments = new[] { _fooInstance, 1, 2, (object)false };
        }

        [Benchmark(Baseline = true)]
        public int DirectCall()
        {
            Foo instance = new();
            return instance.Bar(1, 2, false);
        }

        [Benchmark()]
        public int DirectCall_CachedInstance()
        {
            return _fooInstance.Bar(1, 2, false);
        }

        [Benchmark]
        public int MethodInfoInvoke_NonCachedArgs()
        {
            Foo instance = new();
            return (int)MethodToCall.Invoke(instance, new[] { 1, 2, (object)false });
        }

        [Benchmark]
        public int MethodInfoInvoke_CachedArgs()
        {
            return (int)MethodToCall.Invoke(_fooInstance, MethodArguments);
        }

        [Benchmark]
        public int DynamicCastInvoke_NonCachedInstance()
        {
            dynamic dynamicFoo = new Foo();
            return dynamicFoo.Bar(1, 2, false);
        }

        [Benchmark]
        public int DynamicCastInvoke_CachedInstance()
        {
            dynamic dynamicFoo = _fooInstance;
            return dynamicFoo.Bar(1, 2, false);
        }

        [Benchmark]
        public int DelegateDynamicInvoke_NonCachedArgs()
        {
            var args = new[] { new Foo(), 1, 2, (object)false };
            return (int)DelegateToCall.DynamicInvoke(args);
        }

        [Benchmark]
        public int DelegateDynamicInvoke_CachedArgs()
        {
            return (int)DelegateToCall.DynamicInvoke(_delegateArguments);
        }

        [Benchmark]
        public int FuncCall_NonCachedInstance()
        {
            var foo = new Foo();
            return FuncToCall(foo, 1, 2, false);
        }

        [Benchmark]
        public int FuncCall_CachedInstance()
        {
            return FuncToCall(_fooInstance, 1, 2, false);
        }
    }
}