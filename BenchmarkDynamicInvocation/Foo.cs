namespace BenchmarkDynamicInvocation
{
    public class Foo
    {
        public int Bar(int a, int b, bool c)
        {
            return a + (c ? b : 0);
        }
    }
}
