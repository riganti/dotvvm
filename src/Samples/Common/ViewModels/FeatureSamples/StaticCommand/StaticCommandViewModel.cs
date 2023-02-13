using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Binding;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.StaticCommand
{
    public class StaticCommandViewModel : DotvvmViewModelBase
    {
        public string Name { get; set; } = "Deep Thought";
        public string Greeting { get; set; }
        public StaticCommandTestObject Child { get; set; }

        [AllowStaticCommand]
        public static string GetGreeting(string name)
        {
            var ms = new ArgumentModelState();
            ms.AddArgumentError(nameof(name), "Error!!!");
            ms.FailOnInvalidModelState();

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

    public interface IGreetingComputationService : IGreetingComputationServiceBase,
        IAsyncGreetingComputationServiceBase, IObjectServiceBase
    {
    }

    public interface IObjectServiceBase
    {
        [AllowStaticCommand]
        StaticCommandTestObject GetObject();

        [AllowStaticCommand]
        StaticCommandTestObject GetNull();

    }

    public class StaticCommandTestObject
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }


    public class HelloGreetingComputationService : IGreetingComputationService
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

        public StaticCommandTestObject GetObject() => new StaticCommandTestObject()
            {Name = GetGreeting("DotVVM"), Value = 1};

        public StaticCommandTestObject GetNull() => null;
    }

    public  class StaticCommandTestMethods
    {
        [AllowStaticCommand]
        public static StaticCommandTestObject GetNull()
        {
            return null;
        }
    }

}
