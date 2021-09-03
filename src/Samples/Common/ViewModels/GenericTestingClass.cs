using DotVVM.Framework.ViewModel;
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

    public class TestingClass
    {
        [AllowStaticCommand]
        public static string GenericStaticFunction<TArg1, TArg2>()
            where TArg1 : new()
            where TArg2 : new()
        {
            var arg1 = new TArg1();
            var arg2 = new TArg2();
            return $"Hello from static generic method arg1:{arg1.ToString()} arg2:{arg2.ToString()}";
        }

        [AllowStaticCommand]
        public string GenericInstanceFunction<TArg1, TArg2>()
            where TArg1 : new()
            where TArg2 : new()
        {
            var arg1 = new TArg1();
            var arg2 = new TArg2();
            return $"Hello from instance generic method arg1:{arg1.ToString()} arg2:{arg2.ToString()}";
        }
    }

    public class GenericCommandDemo
    {
        [Bind(Direction.ServerToClient)]
        public string Output { get; set; }
        [Bind(Direction.ServerToClient)]
        public string StaticOutput => StaticProperty;

        public static string StaticProperty { get; set; } = string.Empty;
        public string StaticCommandOutput { get; set; }

        public void GenericInstanceFunction<TArg1, TArg2>()
            where TArg1 : new()
            where TArg2 : new()
        {
            Output = GenericInstanceFunctionWithReturnValue<TArg1, TArg2>();
        }

        [AllowStaticCommand]
        public string GenericInstanceFunctionWithReturnValue<TArg1, TArg2>()
            where TArg1 : new()
            where TArg2 : new()
        {
            var arg1 = new TArg1();
            var arg2 = new TArg2();
            return $"Hello from instance generic command arg1:{arg1} arg2:{arg2}";
        }

        public static void GenericStaticFunction<TArg1, TArg2>()
            where TArg1 : new()
            where TArg2 : new()
        {
            StaticProperty = GenericStaticFunctionWithReturnValue<TArg1, TArg2>();
        }

        [AllowStaticCommand]
        public static string GenericStaticFunctionWithReturnValue<TArg1, TArg2>()
            where TArg1 : new()
            where TArg2 : new()
        {
            var arg1 = new TArg1();
            var arg2 = new TArg2();
            return $"Hello from static generic command arg1:{arg1} arg2:{arg2}";
        }
    }

    public class TestParameter
    {
        public override string ToString()
        {
            return "Hallo from generic parameter.";
        }
    }
}
