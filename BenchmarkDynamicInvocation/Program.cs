using BenchmarkDotNet.Running;

namespace BenchmarkDynamicInvocation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<Benchmarks>();
        }
    }
}