using System;
using System.Threading.Tasks;
using DotVVM.Framework.Interop.DotnetWasm;

namespace DotVVM.Samples.BasicSamples.CSharpClient;

public class TestCsharpModule
{
    private readonly IViewModuleContext context;

    public TestCsharpModule(IViewModuleContext context)
    {
        this.context = context;
    }

    public void Hello()
    {
        Console.WriteLine("Hello world");
    }

    public int TestViewModelAccess()
    {
        var vm = context.GetViewModelSnapshot<TestViewModelShadow>();
        return vm.Value;
    }

    public void PatchViewModel(int newValue)
    {
        context.PatchViewModel(new { Value = newValue });
    }

    public async Task CallNamedCommand(int value)
    {
        await context.InvokeNamedCommandAsync("TestCommand", value);
    }

}

public class TestViewModelShadow
{
    public int Value { get; set; }
}
