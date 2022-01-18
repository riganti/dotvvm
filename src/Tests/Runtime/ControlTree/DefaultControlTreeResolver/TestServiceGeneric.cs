namespace DotVVM.Framework.Tests.Runtime.ControlTree.DefaultControlTreeResolver
{
    public class TestServiceGeneric<T, Q>
        where Q : new()
        where T : new()
    {
        public void TestCall() { }
        public Q TestCallWithReturn() { return new Q(); }
        public T TestCallWithReturnAndParameters(int a, string b) { return new T(); }
    }
}
