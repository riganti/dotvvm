using System.Text;

namespace DotVVM.Tool
{
    public static class Shims
    {
        public const string GeneratorNotice = "NOTICE: This file has been generated automatically.";

        public const string DotvvmDirectory = ".dotvvm";
        public const string Netcoreapp = "netcoreapp3.1";
        public const string CompilerPackage = "DotVVM.Compiler";
        public const string CompilerShimProgramFile = "Compiler.cs";
        public const string CompilerShimProjectFile = "Compiler.csproj";

        public static string GetCompilerShimProject(
            string project,
            string dotvvmVersion,
            string programFile = CompilerShimProgramFile,
            string? compilerReference = null)
        {
            var sb = new StringBuilder();
            sb.Append(
$@"<Project Sdk=""Microsoft.NET.Sdk"">

  <!-- {GeneratorNotice} -->

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>{Netcoreapp}</TargetFramework>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
  </PropertyGroup>

  <ItemGroup>
    <Compile Include=""{programFile}"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""{project}"" />
  </ItemGroup>");
            if (compilerReference is null)
            {
                sb.Append(
$@"
  <ItemGroup>
    <PackageReference Include=""{CompilerPackage}"" Version=""{dotvvmVersion}"" />
  </ItemGroup>
");
            }
            else
            {
                sb.Append(
$@"
  <ItemGroup>
    <ProjectReference Include=""{compilerReference}"" />
  </ItemGroup>
");
            }

            sb.Append(
$@"
</Project>
");
            return sb.ToString();
        }

        public static string GetCompilerShimProgram()
        {
            // TODO: Add a new entry point to the Compiler.
            return
$@"
namespace DotVVM.Compiler.Shim
{{
    public static class Program
    {{
        public static int Main(string[] args)
        {{
            return DotVVM.Compiler.Program.Main(args);
        }}
    }}
}}
";
        }
    }
}
