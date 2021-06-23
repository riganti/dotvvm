@echo building dot net core packages
dotnet restore ../../DotVVM.CommandLine
dotnet build ../../DotVVM.CommandLine

dotnet restore ../../DotVVM.Core
dotnet build ../../DotVVM.Core

dotnet restore ../../DotVVM.Framework
dotnet build ../../DotVVM.Framework

dotnet restore ../../DotVVM.Framework.Hosting.AspNetCore
dotnet build ../../DotVVM.Framework.Hosting.AspNetCore

dotnet restore ../../DotVVM.Framework.Tests
dotnet build ../../DotVVM.Framework.Tests

dotnet restore ../../DotVVM.Framework.Tools.SeleniumGenerator
dotnet build ../../DotVVM.Framework.Tools.SeleniumGenerator

dotnet restore ../../DotVVM.Samples.BasicSamples.AspNetCore
dotnet build ../../DotVVM.Samples.BasicSamples.AspNetCore

dotnet restore ../../DotVVM.Samples.Common
dotnet build ../../DotVVM.Samples.Common


dotnet restore ../../DotVVM.Compiler.Light
dotnet build ../../DotVVM.Compiler.Light
