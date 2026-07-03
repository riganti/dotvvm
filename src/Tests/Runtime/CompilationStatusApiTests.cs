using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class CompilationStatusApiTests
    {
        [TestMethod]
        public async Task GetStatusResponse_Success_Returns200WithoutBody()
        {
            var service = new FakeViewCompilationService(compileResult: true, failedFiles: []);

            var result = await DotvvmCompilationStatusApi.GetStatusResponse(service);

            Assert.AreEqual(200, result.StatusCode);
            Assert.IsNull(result.ResponseBody);
        }

        [TestMethod]
        public async Task GetStatusResponse_Failure_Returns500WithJsonBody()
        {
            var failedFiles = ImmutableArray.Create(new DotHtmlFileInfo("/Views/Failing.dothtml"));
            var service = new FakeViewCompilationService(compileResult: false, failedFiles);

            var result = await DotvvmCompilationStatusApi.GetStatusResponse(service);

            Assert.AreEqual(500, result.StatusCode);
            Assert.IsNotNull(result.ResponseBody);
            StringAssert.Contains(result.ResponseBody, "/Views/Failing.dothtml");
        }

        private sealed class FakeViewCompilationService : IDotvvmViewCompilationService
        {
            private readonly bool compileResult;
            private readonly ImmutableArray<DotHtmlFileInfo> failedFiles;

            public FakeViewCompilationService(bool compileResult, ImmutableArray<DotHtmlFileInfo> failedFiles)
            {
                this.compileResult = compileResult;
                this.failedFiles = failedFiles;
            }

            public ImmutableArray<DotHtmlFileInfo> GetFilesWithFailedCompilation() => failedFiles;
            public ImmutableArray<DotHtmlFileInfo> GetMasterPages() => [];
            public ImmutableArray<DotHtmlFileInfo> GetControls() => [];
            public ImmutableArray<DotHtmlFileInfo> GetRoutes() => [];
            public bool BuildView(DotHtmlFileInfo file, out DotHtmlFileInfo? masterPage)
            {
                masterPage = null;
                return true;
            }
            public Task<bool> CompileAll(bool buildInParallel = true, bool forceRecompile = false) => Task.FromResult(compileResult);
            public void RegisterCompiledView(string filePath, ViewCompiler.ControlBuilderDescriptor? descriptor, Exception? exception) { }
        }
    }
}
