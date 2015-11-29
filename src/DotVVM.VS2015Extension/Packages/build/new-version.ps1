param([String]$version)

$versionWithoutPre = $version
if ($versionWithoutPre.Contains("-")) {
	$versionWithoutPre = $versionWithoutPre.Substring(0, $versionWithoutPre.IndexOf("-"))
}

$templatePath = Resolve-Path ".\..\..\..\DotVVM.VS2015Extension.ProjectTemplate\DotvvmProject.vstemplate"
$nuspecPath = Resolve-Path ".\..\DotVVM\DotVVM.nuspec"
$manifestPath = Resolve-Path ".\..\..\..\DotVVM.VS2015Extension\source.extension.vsixmanifest"
$projectPath = Resolve-Path ".\..\..\..\DotVVM.VS2015Extension\DotVVM.VS2015Extension.csproj"
$assemblyInfoPath =  Resolve-Path ".\..\..\..\DotVVM.Framework\Properties\AssemblyInfo.cs"
$assemblyInfoPath2 = Resolve-Path ".\..\..\..\DotVVM.VS2015Extension\Properties\AssemblyInfo.cs"


# change the nuspec
$nuspec = [System.IO.File]::ReadAllText($nuspecPath, [System.Text.Encoding]::UTF8)
$nuspec = [System.Text.RegularExpressions.Regex]::Replace($nuspec, "\<version\>([^""]*)\</version\>", "<version>" + $version + "</version>")
[System.IO.File]::WriteAllText($nuspecPath, $nuspec, [System.Text.Encoding]::UTF8)

# change the project template
$template = [System.IO.File]::ReadAllText($templatePath, [System.Text.Encoding]::UTF8)
$template = [System.Text.RegularExpressions.Regex]::Replace($template, "\<package id=""DotVVM"" version=""([^""]*)"" /\>", "<package id=""DotVVM"" version=""" + $version + """ />")
[System.IO.File]::WriteAllText($templatePath, $template, [System.Text.Encoding]::UTF8)

# change the vsix manifest
$manifest = [System.IO.File]::ReadAllText($manifestPath, [System.Text.Encoding]::UTF8)
$manifest = [System.Text.RegularExpressions.Regex]::Replace($manifest, "DotVVM.([^""]*).nupkg", "DotVVM." + $version + ".nupkg")
[System.IO.File]::WriteAllText($manifestPath, $manifest, [System.Text.Encoding]::UTF8)

# change the vsix project file
$project = [System.IO.File]::ReadAllText($projectPath, [System.Text.Encoding]::UTF8)
$project = [System.Text.RegularExpressions.Regex]::Replace($project, "DotVVM.([^*""]*).nupkg", "DotVVM." + $version + ".nupkg")
[System.IO.File]::WriteAllText($projectPath, $project, [System.Text.Encoding]::UTF8)

# change the AssemblyInfo project file
$assemblyInfo = [System.IO.File]::ReadAllText($assemblyInfoPath, [System.Text.Encoding]::UTF8)
$assemblyInfo = [System.Text.RegularExpressions.Regex]::Replace($assemblyInfo, "\[assembly: AssemblyVersion\(""([^""]*)""\)\]", "[assembly: AssemblyVersion(""" + $versionWithoutPre + """)]")
$assemblyInfo = [System.Text.RegularExpressions.Regex]::Replace($assemblyInfo, "\[assembly: AssemblyFileVersion\(""([^""]*)""\)]", "[assembly: AssemblyFileVersion(""" + $versionWithoutPre + """)]")
[System.IO.File]::WriteAllText($assemblyInfoPath, $assemblyInfo, [System.Text.Encoding]::UTF8)

# change the AssemblyInfo project file
$assemblyInfo = [System.IO.File]::ReadAllText($assemblyInfoPath2, [System.Text.Encoding]::UTF8)
$assemblyInfo = [System.Text.RegularExpressions.Regex]::Replace($assemblyInfo, "\[assembly: AssemblyVersion\(""([^""]*)""\)\]", "[assembly: AssemblyVersion(""" + $versionWithoutPre + """)]")
$assemblyInfo = [System.Text.RegularExpressions.Regex]::Replace($assemblyInfo, "\[assembly: AssemblyFileVersion\(""([^""]*)""\)\]", "[assembly: AssemblyFileVersion(""" + $versionWithoutPre + """)]")
[System.IO.File]::WriteAllText($assemblyInfoPath2, $assemblyInfo, [System.Text.Encoding]::UTF8)
