using DotVVM.Framework.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class Sample42ViewModel
    {
        public string Name { get; set; } = "Deep Thought";
        public string Greeting { get; set; }

        [StaticCommandCallable]
        public static string GetGreeting(string name)
        {
            return "Hello " + name + "!";
        }
    }
}