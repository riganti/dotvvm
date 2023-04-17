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
                var ams = new ArgumentModelState();
                ams.AddArgumentError(""non-exiting-arg"", ""Error"");
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
                var ams = new ArgumentModelState();
                ams.AddArgumentError({|#0:""arg1""|}, ""Error"");
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
                var ams = new ArgumentModelState();
                ams.AddArgumentError({|#0:nameof(arg1)|}, ""Error"");
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
                var ams = new ArgumentModelState();
                ams.AddArgumentError(argName, ""Error"");
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
                var ams = new ArgumentModelState();
                ams.AddArgumentError(GetArgName(), ""Error"");
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
                var ams = new ArgumentModelState();
                ams.AddArgumentError({|#0:""non-exiting-arg""|}, ""Error"");
            }
        }
    }",

            VerifyCS.Diagnostic(AddArgumentErrorAnalyzer.ReferenceOnlyArgumentsIncludedInStaticCommandInvocation)
                .WithLocation(0).WithArguments("non-exiting-arg"));
        }
    }
}
