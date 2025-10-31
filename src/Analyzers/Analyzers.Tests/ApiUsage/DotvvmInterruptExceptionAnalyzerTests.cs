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
            await VerifyCS.VerifyAnalyzerAsync(@"
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
    }",

            VerifyCS.Diagnostic(DotvvmInterruptExceptionAnalyzer.DoNotCatchDotvvmInterruptException)
                .WithLocation(0).WithArguments("ReturnFileAsync"));
        }

        [Fact]
        public async Task Test_Warning_CatchAllException_RedirectToUrl()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
    }",

            VerifyCS.Diagnostic(DotvvmInterruptExceptionAnalyzer.DoNotCatchDotvvmInterruptException)
                .WithLocation(0).WithArguments("RedirectToUrl"));
        }

        [Fact]
        public async Task Test_Warning_CatchAllException_RedirectToRoute()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
    }",

            VerifyCS.Diagnostic(DotvvmInterruptExceptionAnalyzer.DoNotCatchDotvvmInterruptException)
                .WithLocation(0).WithArguments("RedirectToRoute"));
        }

        [Fact]
        public async Task Test_Warning_CatchAllException_RedirectToLocalUrl()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
    }",

            VerifyCS.Diagnostic(DotvvmInterruptExceptionAnalyzer.DoNotCatchDotvvmInterruptException)
                .WithLocation(0).WithArguments("RedirectToLocalUrl"));
        }

        [Fact]
        public async Task Test_Warning_CatchAllException_RedirectToUrlPermanent()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
    }",

            VerifyCS.Diagnostic(DotvvmInterruptExceptionAnalyzer.DoNotCatchDotvvmInterruptException)
                .WithLocation(0).WithArguments("RedirectToUrlPermanent"));
        }

        [Fact]
        public async Task Test_Warning_CatchAllException_RedirectToRoutePermanent()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
    }",

            VerifyCS.Diagnostic(DotvvmInterruptExceptionAnalyzer.DoNotCatchDotvvmInterruptException)
                .WithLocation(0).WithArguments("RedirectToRoutePermanent"));
        }

        [Fact]
        public async Task Test_Warning_CatchAllException_ReturnFile_Obsolete()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
    }",

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
            await VerifyCS.VerifyAnalyzerAsync(@"
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
    }",

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
    }
}
