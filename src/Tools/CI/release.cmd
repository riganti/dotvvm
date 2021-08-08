@echo building dot net core packages
dotnet restore ../../DotVVM.CommandLine
dotnet build ../../DotVVM.CommandLine -c Release

dotnet restore ../../DotVVM.Core
dotnet build ../../DotVVM.Core -c Release

dotnet restore ../../DotVVM.Framework
dotnet build ../../DotVVM.Framework -c Release

dotnet restore ../../DotVVM.Framework.Hosting.AspNetCore
dotnet build ../../DotVVM.Framework.Hosting.AspNetCore -c Release

dotnet restore ../../DotVVM.Framework.Testing
dotnet build ../../DotVVM.Framework.Testing -c Release

dotnet restore ../../DotVVM.Framework.Tests
dotnet build ../../DotVVM.Framework.Tests -c Release

dotnet restore ../../DotVVM.Framework.Tools.SeleniumGenerator
dotnet build ../../DotVVM.Framework.Tools.SeleniumGenerator -c Release

dotnet restore ../../DotVVM.Samples.BasicSamples.AspNetCore
dotnet build ../../DotVVM.Samples.BasicSamples.AspNetCore -c Release

dotnet restore ../../DotVVM.Samples.Common
dotnet build ../../DotVVM.Samples.Common -c Release


dotnet restore ../../DotVVM.Compiler.Light -c Release
dotnet build ../../DotVVM.Compiler.Light -c Release
