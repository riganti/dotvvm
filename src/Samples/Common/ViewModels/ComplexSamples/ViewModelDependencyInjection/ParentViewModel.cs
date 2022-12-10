using System;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ComplexSamples.ViewModelDependencyInjection;

public class ParentViewModel : DotvvmViewModelBase
{
    public ChildViewModel ChildViewModel { get; set; }

    public ParentViewModel(ChildViewModel childViewModel)
    {
        this.ChildViewModel = childViewModel;
    }

    public override Task Init()
    {
        if (Context is null)
        {
            throw new Exception($"{nameof(Context)} is null in {nameof(Init)} method of {nameof(ParentViewModel)}.");
        }

        return base.Init();
    }

    public override Task Load()
    {
        if (Context is null)
        {
            throw new Exception($"{nameof(Context)} is null in {nameof(Load)} method of {nameof(ParentViewModel)}.");
        }

        return base.Load();
    }

    public void DoSomething()
    {
    }
}
