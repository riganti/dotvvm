using System.Threading.Tasks;
using DotVVM.Analyzers.ApiUsage;
using Xunit;
using VerifyCS = DotVVM.Analyzers.Tests.CSharpAnalyzerVerifier<
    DotVVM.Analyzers.ApiUsage.AddArgumentErrorAnalyzer>;

namespace DotVVM.Analyzers.Tests.ApiUsage
{
    public class AddArgumentErrorTests
    {
        [Fact]
        public async Task Test_NoDiagnostics_RegularMethod_Argument_AddArgumentErrorOnDifferentType()
        {
            var test = @"
    using System;
    using System.IO;

    namespace ConsoleApplication1
    {
        public class RegularClass
        {
            public void AddArgumentError(string arg, string message)
            {

            }

            public void CallSite(int arg1, int arg2)
            {
                AddArgumentError(""non-existing-arg"", ""Error"");
            }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task Test_NoDiagnostics_RegularMethod_Argument_AddArgumentError()
        {
            var test = @"
    using System;
    using System.IO;
    using DotVVM.Framework.Hosting;

    namespace ConsoleApplication1
    {
        public class RegularClass
        {
            public void CallSite(int arg1, int arg2)
            {
                var scms = new StaticCommandModelState();
                scms.AddArgumentError(""non-exiting-arg"", ""Error"");
            }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task Test_NoDiagnostics_StaticCommandMethod_Argument_AddArgumentError_CorrectLiteral()
        {
            var test = @"
    using System;
    using System.IO;
    using DotVVM.Framework.Hosting;
    using DotVVM.Framework.ViewModel;

    namespace ConsoleApplication1
    {
        public class RegularClass
        {
            [AllowStaticCommand]
            public void CallSite(int arg1, int arg2)
            {
                var scms = new StaticCommandModelState();
                scms.AddArgumentError({|#0:""arg1""|}, ""Error"");
            }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task Test_NoDiagnostics_StaticCommandMethod_Argument_AddArgumentError_LocalVariable()
        {
            var test = @"
    using System;
    using System.IO;
    using DotVVM.Framework.Hosting;
    using DotVVM.Framework.ViewModel;

    namespace ConsoleApplication1
    {
        public class RegularClass
        {
            [AllowStaticCommand]
            public void CallSite(int arg1, int arg2)
            {
                var scms = new StaticCommandModelState();
                var argName = Console.ReadLine();
                scms.AddArgumentError({|#0:argName|}, ""Error"");
            }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }


        [Fact]
        public async Task Test_NoDiagnostics_StaticCommandMethod_Argument_AddArgumentError_CorrectNameof()
        {
            var test = @"
    using System;
    using System.IO;
    using DotVVM.Framework.Hosting;
    using DotVVM.Framework.ViewModel;

    namespace ConsoleApplication1
    {
        public class RegularClass
        {
            [AllowStaticCommand]
            public void CallSite(int arg1, int arg2)
            {
                var scms = new StaticCommandModelState();
                scms.AddArgumentError({|#0:nameof(arg1)|}, ""Error"");
            }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task Test_NoDiagnostics_StaticCommandMethod_Argument_AddArgumentError_CorrectConstant()
        {
            var test = @"
    using System;
    using System.IO;
    using DotVVM.Framework.Hosting;
    using DotVVM.Framework.ViewModel;

    namespace ConsoleApplication1
    {
        public class RegularClass
        {
            private const string argName = ""arg1"";

            [AllowStaticCommand]
            public void CallSite(int arg1, int arg2)
            {
                var scms = new StaticCommandModelState();
                scms.AddArgumentError(argName, ""Error"");
            }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task Test_NoDiagnostics_StaticCommandMethod_Argument_AddArgumentError_NotConstexpr()
        {
            var test = @"
    using System;
    using System.IO;
    using DotVVM.Framework.Hosting;
    using DotVVM.Framework.ViewModel;

    namespace ConsoleApplication1
    {
        public class RegularClass
        {
            private const string argName = ""arg1"";

            public string GetArgName()
                => argName;

            [AllowStaticCommand]
            public void CallSite(int arg1, int arg2)
            {
                var scms = new StaticCommandModelState();
                scms.AddArgumentError(GetArgName(), ""Error"");
            }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task Test_Warning_StaticCommandMethod_Argument_AddArgumentError_WrongVariableReference()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
    using System;
    using System.IO;
    using DotVVM.Framework.Hosting;
    using DotVVM.Framework.ViewModel;

    namespace ConsoleApplication1
    {
        public class RegularClass
        {
            [AllowStaticCommand]
            public void CallSite(int arg1, int arg2)
            {
                var scms = new StaticCommandModelState();
                scms.AddArgumentError({|#0:""non-exiting-arg""|}, ""Error"");
            }
        }
    }",

            VerifyCS.Diagnostic(AddArgumentErrorAnalyzer.ReferenceOnlyArgumentsIncludedInStaticCommandInvocation)
                .WithLocation(0).WithArguments("non-exiting-arg"));
        }

        [Fact]
        public async Task Test_NoDiagnostics_StaticCommandMethod_Argument_AddArgumentError_CorrectPropertyPathLambda_SimpleObject()
        {
            var test = @"
    using System;
    using System.IO;
    using DotVVM.Framework.Hosting;
    using DotVVM.Framework.ViewModel;

    namespace ConsoleApplication1
    {
        public class RegularClass
        {
            [AllowStaticCommand]
            public void CallSite(int arg1, int arg2)
            {
                var scms = new StaticCommandModelState();
                {|#0:scms.AddArgumentError(() => arg1, ""Error"")|};
            }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task Test_NoDiagnostics_StaticCommandMethod_Argument_AddArgumentError_CorrectPropertyPathLambda_ComplexObject()
        {
            var test = @"
    using System;
    using System.IO;
    using DotVVM.Framework.Hosting;
    using DotVVM.Framework.ViewModel;

    namespace ConsoleApplication1
    {
        public class RegularClass
        {
            public class TestClass
            {
                public string Property { get; set; }
            }

            [AllowStaticCommand]
            public void CallSite(TestClass arg1, int arg2)
            {
                var scms = new StaticCommandModelState();
                {|#0:scms.AddArgumentError(() => arg1.Property, ""Error"")|};
            }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task Test_Warning_StaticCommandMethod_Argument_AddArgumentError_ReferenceInvalidLocalVariable_SimpleObject()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
    using System;
    using System.IO;
    using DotVVM.Framework.Hosting;
    using DotVVM.Framework.ViewModel;

    namespace ConsoleApplication1
    {
        public class RegularClass
        {
            public class TestClass
            {
                public string Property { get; set; }
            }

            [AllowStaticCommand]
            public void CallSite(TestClass arg1, int arg2)
            {
                var myVariable = 123;
                var scms = new StaticCommandModelState();
                scms.AddArgumentError({|#0:() => myVariable|}, ""Error"");
            }
        }
    }",

            VerifyCS.Diagnostic(AddArgumentErrorAnalyzer.ReferenceOnlyArgumentsIncludedInStaticCommandInvocation)
                .WithLocation(0).WithArguments("() => myVariable"));
        }

        [Fact]
        public async Task Test_Warning_StaticCommandMethod_Argument_AddArgumentError_ReferenceInvalidLocalVariable_ComplexObject()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
    using System;
    using System.IO;
    using DotVVM.Framework.Hosting;
    using DotVVM.Framework.ViewModel;

    namespace ConsoleApplication1
    {
        public class RegularClass
        {
            public class TestClass
            {
                public string Property { get; set; }
            }

            [AllowStaticCommand]
            public void CallSite(TestClass arg1, int arg2)
            {
                var scms = new StaticCommandModelState();
                scms.AddArgumentError({|#0:() => scms.Errors|}, ""Error"");
            }
        }
    }",

            VerifyCS.Diagnostic(AddArgumentErrorAnalyzer.ReferenceOnlyArgumentsIncludedInStaticCommandInvocation)
                .WithLocation(0).WithArguments("() => scms.Errors"));
        }
    }
}
