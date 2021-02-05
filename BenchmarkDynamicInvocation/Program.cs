using ConsoleTables;
using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BenchmarkDynamicInvocation
{
    public class Foo
    {
        public int Bar(int a, int b, bool c)
        {
            return a + (c ? b : 0);
        }
    }

    public record Results(string Name, TimeSpan FirstCall, TimeSpan NextCalls);

    public class Program
    {
        private const int Iterations = 10000000;
        private static readonly Type ClassType = typeof(Foo);
        private static readonly MethodInfo Method = ClassType.GetMethod(nameof(Foo.Bar));

        public static void Main(string[] args)
        {
            Console.WriteLine($"building dataset on {Iterations} iterations...");

            var results = new[]
                {
                    DirectCall(),
                    MethodInfoInvoke(),
                    DynamicCastInvoke(),
                    DelegateDynamicInvokeInvoke(),
                    FuncCall()
                }.OrderBy(r => r.NextCalls)
                .ThenBy(r => r.NextCalls);

            ConsoleTable.From<Results>(results).Write();

            Console.WriteLine("press a key to exit..."); 
            Console.ReadLine();
        }

        private static Results DirectCall()
        {
            var foo = new Foo();
            
            var firstCallTimer = Stopwatch.StartNew();
            foo.Bar(1, 2, false);
            firstCallTimer.Stop();

            var nextCallsTimer = Stopwatch.StartNew();
            for (var i = 0; i < Iterations; i++)
            {
                foo.Bar(1, 2, false);
            }
            nextCallsTimer.Stop();

            return new Results("direct", firstCallTimer.Elapsed, nextCallsTimer.Elapsed);
        }

        private static Results MethodInfoInvoke()
        {
            object foo = new Foo();

            var firstCallTimer = Stopwatch.StartNew();
            Method.Invoke(foo, new[] { (object)1, (object)2, (object)false });
            firstCallTimer.Stop();

            var nextCallsTimer = Stopwatch.StartNew();
            for (var i = 0; i < Iterations; i++)
            {
                Method.Invoke(foo, new[] { (object)1, (object)2, (object)false });
            }
            nextCallsTimer.Stop();

            return new Results("MethodInfo", firstCallTimer.Elapsed, nextCallsTimer.Elapsed);
        }

        private static Results DynamicCastInvoke()
        {
            object foo = new Foo();
            dynamic dynamicFoo = foo as dynamic;

            var firstCallTimer = Stopwatch.StartNew();
            dynamicFoo.Bar(1, 2, false);
            firstCallTimer.Stop();

            var nextCallsTimer = Stopwatch.StartNew();
            for (var i = 0; i < Iterations; i++)
            {
                dynamicFoo.Bar(1, 2, false);
            }
            nextCallsTimer.Stop();

            return new Results("dynamic cast", firstCallTimer.Elapsed, nextCallsTimer.Elapsed);
        }

        private static Results DelegateDynamicInvokeInvoke()
        {
            object foo = new Foo();

            var delegateType = Expression.GetDelegateType(typeof(Foo), typeof(int), typeof(int), typeof(bool), typeof(int));
            var @delegate = Delegate.CreateDelegate(delegateType, Method);
            
            var firstCallTimer = Stopwatch.StartNew();
            @delegate.DynamicInvoke(new[] { foo, (object)1, (object)2, (object)false });
            firstCallTimer.Stop();

            var nextCallsTimer = Stopwatch.StartNew();
            for (var i = 0; i < Iterations; i++)
            {
                @delegate.DynamicInvoke(new[] { (object)foo, (object)1, (object)2, (object)false });
            }
            nextCallsTimer.Stop();

            return new Results("delegate", firstCallTimer.Elapsed, nextCallsTimer.Elapsed);
        }

        private static Results FuncCall()
        {
            Foo foo = new();

            var delegateType = Expression.GetDelegateType(typeof(Foo), typeof(int), typeof(int), typeof(bool), typeof(int));
            var func = (Func<Foo, int, int, bool, int>)Delegate.CreateDelegate(delegateType, Method);

            var firstCallTimer = Stopwatch.StartNew();
            func(foo, 1, 2, false);
            firstCallTimer.Stop();

            var nextCallsTimer = Stopwatch.StartNew();
            for (var i = 0; i < Iterations; i++)
            {
                func(foo, 1, 2, false);
            }
            nextCallsTimer.Stop();

            return new Results("Func", firstCallTimer.Elapsed, nextCallsTimer.Elapsed);
        }
    }
}
