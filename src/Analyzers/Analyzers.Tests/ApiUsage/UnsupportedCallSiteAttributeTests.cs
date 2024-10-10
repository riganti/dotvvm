using System.Threading.Tasks;
using DotVVM.Analyzers.ApiUsage;
using Xunit;
using VerifyCS = DotVVM.Analyzers.Tests.CSharpAnalyzerVerifier<
    DotVVM.Analyzers.ApiUsage.UnsupportedCallSiteAttributeAnalyzer>;

namespace DotVVM.Analyzers.Tests.ApiUsage
{
    public class UnsupportedCallSiteAttributeTests
    {
        [Fact]
        public async Task Test_NoDiagnostics_InvokeMethod_WithoutUnsupportedCallSiteAttribute()
        {
            var test = @"
    using System;
    using System.IO;

    namespace ConsoleApplication1
    {
        public class RegularClass
        {
            public void Target()
            {

            }

            public void CallSite()
            {
                Target();
            }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task Test_Warning_InvokeMethod_WithUnsupportedCallSiteAttribute()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
    using System;
    using System.IO;
    using DotVVM.Framework.CodeAnalysis;

    namespace ConsoleApplication1
    {
        public class RegularClass
        {
            [UnsupportedCallSite(CallSiteType.ServerSide)]
            public void Target()
            {

            }

            public void CallSite()
            {
                {|#0:Target()|};
            }
        }
    }",

            VerifyCS.Diagnostic(UnsupportedCallSiteAttributeAnalyzer.DoNotInvokeMethodFromUnsupportedCallSite)
                .WithLocation(0).WithArguments("Target", string.Empty));
        }

        [Fact]
        public async Task Test_Warning_InvokeMethod_WithUnsupportedCallSiteAttribute_WithReason()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
    using System;
    using System.IO;
    using DotVVM.Framework.CodeAnalysis;

    namespace ConsoleApplication1
    {
        public class RegularClass
        {
            [UnsupportedCallSite(CallSiteType.ServerSide, ""REASON"")]
            public void Target()
            {

            }

            public void CallSite()
            {
                {|#0:Target()|};
            }
        }
    }",

            VerifyCS.Diagnostic(UnsupportedCallSiteAttributeAnalyzer.DoNotInvokeMethodFromUnsupportedCallSite)
                .WithLocation(0).WithArguments("Target", "due to: \"REASON\""));
        }

        [Fact]
        public async Task Test_Warning_InvokeMethod_WithUnsupportedCallSiteAttribute_InLambda()
        {
            await VerifyCS.VerifyAnalyzerAsync("""
                using System;
                using System.IO;
                using DotVVM.Framework.CodeAnalysis;

                namespace ConsoleApplication1
                {
                    public class RegularClass
                    {
                        [UnsupportedCallSite(CallSiteType.ServerSide)]
                        public void Target()
                        {

                        }

                        public void CallSite()
                        {
                            Action fn = () => {|#0:Target()|};
                        }
                    }
                }
                """,

                VerifyCS.Diagnostic(UnsupportedCallSiteAttributeAnalyzer.DoNotInvokeMethodFromUnsupportedCallSite)
                    .WithLocation(0).WithArguments("Target", string.Empty)
            );
        }

        [Fact]
        public async Task Test_Warning_InvokeMethod_WithUnsupportedCallSiteAttribute_InLinqExpression()
        {
            // supressed in Linq.Expressions
            await VerifyCS.VerifyAnalyzerAsync("""
                using System;
                using System.IO;
                using System.Linq.Expressions;
                using DotVVM.Framework.CodeAnalysis;

                namespace ConsoleApplication1
                {
                    public class RegularClass
                    {
                        [UnsupportedCallSite(CallSiteType.ServerSide)]
                        public int Target()
                        {
                            return 0;
                        }

                        public void CallSite()
                        {
                            Expression<Action> fn = () => Target();
                            Expression<Func<int, int>> fn2 = arg => Target();
                        }
                    }
                }
                """,
                expected: []
            );
        }
    }
}
