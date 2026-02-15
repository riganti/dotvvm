using System.Threading.Tasks;
using DotVVM.Analyzers.ApiUsage;
using Xunit;
using VerifyCS = DotVVM.Analyzers.Tests.CSharpAnalyzerVerifier<
    DotVVM.Analyzers.ApiUsage.DotvvmInterruptExceptionAnalyzer>;

namespace DotVVM.Analyzers.Tests.ApiUsage
{
    public class DotvvmInterruptExceptionAnalyzerTests
    {
        [Fact]
        public async Task Test_NoDiagnostics_NoTryCatch()
        {
            var test = @"
    using System.Threading.Tasks;
    using DotVVM.Framework.Hosting;

    namespace ConsoleApplication1
    {
        public class TestClass
        {
            public async Task OnDownloadFile(IDotvvmRequestContext context)
            {
                var bytes = new byte[] { 1, 2, 3 };
                await context.ReturnFileAsync(bytes, ""file.csv"", ""application/octet-stream"");
            }
        }
    }";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task Test_NoDiagnostics_WithProperRethrow()
        {
            var test = @"
    using System;
    using System.Threading.Tasks;
    using DotVVM.Framework.Hosting;

    namespace ConsoleApplication1
    {
        public class TestClass
        {
            public async Task OnDownloadFile(IDotvvmRequestContext context)
            {
                try
                {
                    var bytes = new byte[] { 1, 2, 3 };
                    await context.ReturnFileAsync(bytes, ""file.csv"", ""application/octet-stream"");
                }
                catch (DotvvmInterruptRequestExecutionException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    // Handle other exceptions
                }
            }
        }
    }";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task Test_Warning_CatchAllException_ReturnFileAsync()
        {
            var test = @"
    using System;
    using System.Threading.Tasks;
    using DotVVM.Framework.Hosting;

    namespace ConsoleApplication1
    {
        public class TestClass
        {
            public async Task OnDownloadFile(IDotvvmRequestContext context)
            {
                try
                {
                    var bytes = new byte[] { 1, 2, 3 };
                    await {|#0:context.ReturnFileAsync(bytes, ""file.csv"", ""application/octet-stream"")|};
                }
                catch (System.Exception ex)
                {
                    // Handle exceptions
                }
            }
        }
    }";
            await VerifyCS.VerifyAnalyzerAsync(test, 
                VerifyCS.Diagnostic(DotvvmInterruptExceptionAnalyzer.DoNotCatchDotvvmInterruptException)
                    .WithLocation(0).WithArguments("ReturnFileAsync"));
        }

        [Fact]
        public async Task Test_Warning_CatchAllException_RedirectToUrl()
        {
            var test = @"
    using System;
    using DotVVM.Framework.Hosting;

    namespace ConsoleApplication1
    {
        public class TestClass
        {
            public void OnRedirect(IDotvvmRequestContext context)
            {
                try
                {
                    {|#0:context.RedirectToUrl(""/home"")|};
                }
                catch (Exception ex)
                {
                    // Handle exceptions
                }
            }
        }
    }";
            await VerifyCS.VerifyAnalyzerAsync(test, 
                VerifyCS.Diagnostic(DotvvmInterruptExceptionAnalyzer.DoNotCatchDotvvmInterruptException)
                    .WithLocation(0).WithArguments("RedirectToUrl"));
        }

        [Fact]
        public async Task Test_Warning_CatchAllException_RedirectToRoute()
        {
            var test = @"
    using System;
    using DotVVM.Framework.Hosting;

    namespace ConsoleApplication1
    {
        public class TestClass
        {
            public void OnRedirect(IDotvvmRequestContext context)
            {
                try
                {
                    {|#0:context.RedirectToRoute(""Default"")|};
                }
                catch (Exception)
                {
                    // Handle exceptions
                }
            }
        }
    }";
            await VerifyCS.VerifyAnalyzerAsync(test,
                VerifyCS.Diagnostic(DotvvmInterruptExceptionAnalyzer.DoNotCatchDotvvmInterruptException)
                    .WithLocation(0).WithArguments("RedirectToRoute"));
        }

        [Fact]
        public async Task Test_Warning_CatchAllException_RedirectToLocalUrl()
        {
            var test = @"
    using System;
    using DotVVM.Framework.Hosting;

    namespace ConsoleApplication1
    {
        public class TestClass
        {
            public void OnRedirect(IDotvvmRequestContext context)
            {
                try
                {
                    {|#0:context.RedirectToLocalUrl(""/home"")|};
                }
                catch (Exception ex)
                {
                    // Log exception
                }
            }
        }
    }";
            await VerifyCS.VerifyAnalyzerAsync(test, 
                VerifyCS.Diagnostic(DotvvmInterruptExceptionAnalyzer.DoNotCatchDotvvmInterruptException)
                    .WithLocation(0).WithArguments("RedirectToLocalUrl"));
        }

        [Fact]
        public async Task Test_Warning_CatchAllException_RedirectToUrlPermanent()
        {
            var test = @"
    using System;
    using DotVVM.Framework.Hosting;

    namespace ConsoleApplication1
    {
        public class TestClass
        {
            public void OnRedirect(IDotvvmRequestContext context)
            {
                try
                {
                    {|#0:context.RedirectToUrlPermanent(""/home"")|};
                }
                catch (Exception ex)
                {
                    // Handle exception
                }
            }
        }
    }";
            await VerifyCS.VerifyAnalyzerAsync(test,
                VerifyCS.Diagnostic(DotvvmInterruptExceptionAnalyzer.DoNotCatchDotvvmInterruptException)
                    .WithLocation(0).WithArguments("RedirectToUrlPermanent"));
        }

        [Fact]
        public async Task Test_Warning_CatchAllException_RedirectToRoutePermanent()
        {
            var test = @"
    using System;
    using DotVVM.Framework.Hosting;

    namespace ConsoleApplication1
    {
        public class TestClass
        {
            public void OnRedirect(IDotvvmRequestContext context)
            {
                try
                {
                    {|#0:context.RedirectToRoutePermanent(""Default"")|};
                }
                catch (Exception ex)
                {
                    // Handle exception
                }
            }
        }
    }";
            await VerifyCS.VerifyAnalyzerAsync(test, 
                VerifyCS.Diagnostic(DotvvmInterruptExceptionAnalyzer.DoNotCatchDotvvmInterruptException)
                    .WithLocation(0).WithArguments("RedirectToRoutePermanent"));
        }

        [Fact]
        public async Task Test_Warning_CatchAllException_ReturnFile_Obsolete()
        {
            var test = @"
    using System;
    using DotVVM.Framework.Hosting;

    namespace ConsoleApplication1
    {
        public class TestClass
        {
            public void OnDownloadFile(IDotvvmRequestContext context)
            {
                try
                {
                    var bytes = new byte[] { 1, 2, 3 };
#pragma warning disable CS0618
                    {|#0:context.ReturnFile(bytes, ""file.csv"", ""application/octet-stream"")|};
#pragma warning restore CS0618
                }
                catch (Exception ex)
                {
                    // Handle exceptions
                }
            }
        }
    }";
            await VerifyCS.VerifyAnalyzerAsync(test,
                VerifyCS.Diagnostic(DotvvmInterruptExceptionAnalyzer.DoNotCatchDotvvmInterruptException)
                    .WithLocation(0).WithArguments("ReturnFile"));
        }

        [Fact]
        public async Task Test_NoDiagnostics_CatchSpecificException()
        {
            var test = @"
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using DotVVM.Framework.Hosting;

    namespace ConsoleApplication1
    {
        public class TestClass
        {
            public async Task OnDownloadFile(IDotvvmRequestContext context)
            {
                try
                {
                    var bytes = new byte[] { 1, 2, 3 };
                    await context.ReturnFileAsync(bytes, ""file.csv"", ""application/octet-stream"");
                }
                catch (IOException ex)
                {
                    // Handle IO exceptions
                }
            }
        }
    }";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task Test_Warning_NestedTryCatch()
        {
            var test = @"
    using System;
    using System.Threading.Tasks;
    using DotVVM.Framework.Hosting;

    namespace ConsoleApplication1
    {
        public class TestClass
        {
            public async Task OnDownloadFile(IDotvvmRequestContext context)
            {
                try
                {
                    try
                    {
                        var bytes = new byte[] { 1, 2, 3 };
                        await {|#0:context.ReturnFileAsync(bytes, ""file.csv"", ""application/octet-stream"")|};
                    }
                    catch (Exception ex)
                    {
                        // Inner catch
                    }
                }
                catch (Exception ex)
                {
                    // Outer catch
                }
            }
        }
    }";
            await VerifyCS.VerifyAnalyzerAsync(test, 
                VerifyCS.Diagnostic(DotvvmInterruptExceptionAnalyzer.DoNotCatchDotvvmInterruptException)
                    .WithLocation(0).WithArguments("ReturnFileAsync"));
        }

        [Fact]
        public async Task Test_NoDiagnostics_MethodNotOnContext()
        {
            var test = @"
    using System;
    using System.Threading.Tasks;

    namespace ConsoleApplication1
    {
        public class TestClass
        {
            public async Task ReturnFileAsync(byte[] bytes, string fileName, string mimeType)
            {
                // Custom method, not the DotVVM one
            }

            public async Task OnDownloadFile()
            {
                try
                {
                    var bytes = new byte[] { 1, 2, 3 };
                    await ReturnFileAsync(bytes, ""file.csv"", ""application/octet-stream"");
                }
                catch (Exception ex)
                {
                    // Handle exceptions - this is fine
                }
            }
        }
    }";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task Test_NoDiagnostics_RethrowInsideIfStatement()
        {
            var test = @"
    using System;
    using System.Threading.Tasks;
    using DotVVM.Framework.Hosting;

    namespace ConsoleApplication1
    {
        public class TestClass
        {
            public async Task OnDownloadFile(IDotvvmRequestContext context)
            {
                try
                {
                    var bytes = new byte[] { 1, 2, 3 };
                    await context.ReturnFileAsync(bytes, ""file.csv"", ""application/octet-stream"");
                }
                catch (DotvvmInterruptRequestExecutionException ex)
                {
                    if (true)
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    // Handle other exceptions
                }
            }
        }
    }";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task Test_NoDiagnostics_WhenClauseExcludingInterruptException()
        {
            var test = @"
    using System;
    using System.Threading.Tasks;
    using DotVVM.Framework.Hosting;

    namespace ConsoleApplication1
    {
        public class TestClass
        {
            public async Task OnDownloadFile(IDotvvmRequestContext context)
            {
                try
                {
                    var bytes = new byte[] { 1, 2, 3 };
                    await context.ReturnFileAsync(bytes, ""file.csv"", ""application/octet-stream"");
                }
                catch (Exception ex) when (ex is not DotvvmInterruptRequestExecutionException)
                {
                    // Handle other exceptions only
                }
            }
        }
    }";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task Test_NoDiagnostics_NestedTryBlocks_BothProperlyHandled()
        {
            var test = @"
    using System;
    using System.Threading.Tasks;
    using DotVVM.Framework.Hosting;

    namespace ConsoleApplication1
    {
        public class TestClass
        {
            public async Task OnDownloadFile(IDotvvmRequestContext context)
            {
                try
                {
                    try
                    {
                        var bytes = new byte[] { 1, 2, 3 };
                        await context.ReturnFileAsync(bytes, ""file.csv"", ""application/octet-stream"");
                    }
                    catch (DotvvmInterruptRequestExecutionException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        // Handle other exceptions in inner try
                    }
                }
                catch (DotvvmInterruptRequestExecutionException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    // Handle other exceptions in outer try
                }
            }
        }
    }";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task Test_Warning_NestedTryBlocks_OuterNotProperlyHandled()
        {
            var test = @"
    using System;
    using System.Threading.Tasks;
    using DotVVM.Framework.Hosting;

    namespace ConsoleApplication1
    {
        public class TestClass
        {
            public async Task OnDownloadFile(IDotvvmRequestContext context)
            {
                try
                {
                    try
                    {
                        var bytes = new byte[] { 1, 2, 3 };
                        await {|#0:context.ReturnFileAsync(bytes, ""file.csv"", ""application/octet-stream"")|};
                    }
                    catch (DotvvmInterruptRequestExecutionException)
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    // Outer catch doesn't properly handle the rethrown exception
                }
            }
        }
    }";
            await VerifyCS.VerifyAnalyzerAsync(test,
                VerifyCS.Diagnostic(DotvvmInterruptExceptionAnalyzer.DoNotCatchDotvvmInterruptException)
                    .WithLocation(0).WithArguments("ReturnFileAsync"));
        }

        [Fact]
        public async Task Test_Warning_NestedTryBlocks_InnerNotProperlyHandled()
        {
            var test = @"
    using System;
    using System.Threading.Tasks;
    using DotVVM.Framework.Hosting;

    namespace ConsoleApplication1
    {
        public class TestClass
        {
            public async Task OnDownloadFile(IDotvvmRequestContext context)
            {
                try
                {
                    try
                    {
                        var bytes = new byte[] { 1, 2, 3 };
                        await {|#0:context.ReturnFileAsync(bytes, ""file.csv"", ""application/octet-stream"")|};
                    }
                    catch (Exception ex)
                    {
                        // Inner catch doesn't properly handle
                    }
                }
                catch (DotvvmInterruptRequestExecutionException)
                {
                    throw;
                }
            }
        }
    }";
            await VerifyCS.VerifyAnalyzerAsync(test,
                VerifyCS.Diagnostic(DotvvmInterruptExceptionAnalyzer.DoNotCatchDotvvmInterruptException)
                    .WithLocation(0).WithArguments("ReturnFileAsync"));
        }

        [Fact]
        public async Task Test_Warning_BareCatchBlock_NoRethrow()
        {
            var test = @"
    using System.Threading.Tasks;
    using DotVVM.Framework.Hosting;

    namespace ConsoleApplication1
    {
        public class TestClass
        {
            public async Task OnDownloadFile(IDotvvmRequestContext context)
            {
                try
                {
                    var bytes = new byte[] { 1, 2, 3 };
                    await {|#0:context.ReturnFileAsync(bytes, ""file.csv"", ""application/octet-stream"")|};
                }
                catch
                {
                    // Bare catch without rethrow - this is BAD
                }
            }
        }
    }";
            await VerifyCS.VerifyAnalyzerAsync(test,
                VerifyCS.Diagnostic(DotvvmInterruptExceptionAnalyzer.DoNotCatchDotvvmInterruptException)
                    .WithLocation(0).WithArguments("ReturnFileAsync"));
        }

        [Fact]
        public async Task Test_NoDiagnostics_CatchAllExceptions_Rethrow()
        {
            var test = @"
    using System;
    using System.Threading.Tasks;
    using DotVVM.Framework.Hosting;

    namespace ConsoleApplication1
    {
        public class TestClass
        {
            public async Task OnDownloadFile(IDotvvmRequestContext context)
            {
                try
                {
                    var bytes = new byte[] { 1, 2, 3 };
                    await {|#0:context.ReturnFileAsync(bytes, ""file.csv"", ""application/octet-stream"")|};
                }
                catch (Exception)
                {
                    throw;      // rethrow is fine
                }
            }
        }
    }";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task Test_Warning_BareCatchBlock_WithRethrow()
        {
            // Bare catch blocks with rethrow are now allowed
            var test = @"
    using System.Threading.Tasks;
    using DotVVM.Framework.Hosting;

    namespace ConsoleApplication1
    {
        public class TestClass
        {
            public async Task OnDownloadFile(IDotvvmRequestContext context)
            {
                try
                {
                    var bytes = new byte[] { 1, 2, 3 };
                    await context.ReturnFileAsync(bytes, ""file.csv"", ""application/octet-stream"");
                }
                catch
                {
                    throw;
                }
            }
        }
    }";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task Test_Warning_BareCatchBlock_WithRethrow_NonAsync()
        {
            // Bare catch blocks with rethrow are now allowed
            var test = @"
    using DotVVM.Framework.Hosting;

    namespace ConsoleApplication1
    {
        public class TestClass
        {
            public void OnRedirect(IDotvvmRequestContext context)
            {
                try
                {
                    context.RedirectToUrl(""/home"");
                }
                catch
                {
                    throw;
                }
            }
        }
    }";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }
    }
}
