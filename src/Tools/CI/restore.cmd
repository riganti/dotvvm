@echo restoring dot net core packages

cd ../DotVVM.CommandLine
dotnet restore

cd ../DotVVM.Compiler.Light
dotnet restore

cd ../DotVVM.Core
dotnet restore

cd ../DotVVM.Framework
dotnet restore

cd ../DotVVM.Framework.Hosting.AspNetCore
dotnet restore

cd ../DotVVM.Framework.Hosting.Owin
dotnet restore

cd ../DotVVM.Framework.PerfTests
dotnet restore

cd ../DotVVM.Framework.Testing
dotnet restore

cd ../DotVVM.Framework.Tests
dotnet restore

cd ../DotVVM.Framework.Tools.SeleniumGenerator
dotnet restore

cd ../DotVVM.Framework.Testing.SeleniumHelpers
dotnet restore

cd ../DotVVM.Samples.BasicSamples.AspNetCore
dotnet restore

cd ../DotVVM.Samples.Common
dotnet restore

cd ../DotVVM.Samples.Tests
dotnet restore

cd ../DotVVM.Tracing.ApplicationInsights
dotnet restore

cd ../DotVVM.Tracing.ApplicationInsights.Owin
dotnet restore

cd ../DotVVM.Tracing.ApplicationInsights.AspNetCore
dotnet restore

cd ../DotVVM.Samples.ApplicationInsights.AspNetCore
dotnet restore

cd ../DotVVM.Tracing.MiniProfiler.Owin
dotnet restore

cd ../DotVVM.Tracing.MiniProfiler.AspNetCore
dotnet restore

cd ../DotVVM.Samples.MiniProfiler.AspNetCore
dotnet restore
