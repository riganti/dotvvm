using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class GenericTestingClass<TArg1, TArg2>
        where TArg1 : new()
        where TArg2 : new()
    {
        public static string Arg1 { get; } = new TArg1().ToString();
        public static string Arg2 { get; } = new TArg2().ToString();
    }

    public class TestParameter
    {
        public override string ToString()
        {
            return "Hallo from generic parameter.";
        }
    }
}
