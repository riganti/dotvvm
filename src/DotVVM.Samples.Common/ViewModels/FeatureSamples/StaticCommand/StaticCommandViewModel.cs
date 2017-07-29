using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Binding;
using System.Threading.Tasks;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.StaticCommand
{
    public class StaticCommandViewModel : DotvvmViewModelBase
    {
        public string Name { get; set; } = "Deep Thought";
        public string Greeting { get; set; }

        [AllowStaticCommand]
        public static string GetGreeting(string name)
        {
            return "Hello " + name + "!";
        }
    }

    public interface IGreetingComputationServiceBase
    {
        [AllowStaticCommand]
        string GetGreeting(string name);
    }
    public interface IAsyncGreetingComputationServiceBase
    {
        [AllowStaticCommand]
        Task<string> GetGreetingAsync(string name);
    }
    
    public interface IGreetingComputationService: IGreetingComputationServiceBase, IAsyncGreetingComputationServiceBase
    {
    }

    public class HelloGreetingComputationService: IGreetingComputationService
    {
        public string GetGreeting(string name)
        {
            return "Hello " + name + "!";
        }

        public async Task<string> GetGreetingAsync(string name)
        {
            await Task.Delay(50);
            return GetGreeting(name);
        }

    }
}
