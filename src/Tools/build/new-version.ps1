param([String]$version)

$versionWithoutPre = $version
if ($versionWithoutPre.Contains("-")) {
	$versionWithoutPre = $versionWithoutPre.Substring(0, $versionWithoutPre.IndexOf("-"))
}

$nuspecPath = Resolve-Path ".\DotVVM.nuspec"
$nuspecPath2 = Resolve-Path ".\DotVVM.Core.nuspec"
$assemblyInfoPath = Resolve-Path ".\..\..\DotVVM.Framework\Properties\AssemblyInfo.cs"
$assemblyInfoPath2 = Resolve-Path ".\..\..\DotVVM.Core\Properties\AssemblyInfo.cs"

# change the nuspec
$nuspec = [System.IO.File]::ReadAllText($nuspecPath, [System.Text.Encoding]::UTF8)
$nuspec = [System.Text.RegularExpressions.Regex]::Replace($nuspec, "\<version\>([^""]*)\</version\>", "<version>" + $version + "</version>")
$nuspec = [System.Text.RegularExpressions.Regex]::Replace($nuspec, "\<dependency id=""DotVVM.Core"" version=""([^""]*)"" /\>", "<dependency id=""DotVVM.Core"" version=""" + $version + """ />")
[System.IO.File]::WriteAllText($nuspecPath, $nuspec, [System.Text.Encoding]::UTF8)

# change the nuspec
$nuspec2 = [System.IO.File]::ReadAllText($nuspecPath2, [System.Text.Encoding]::UTF8)
$nuspec2 = [System.Text.RegularExpressions.Regex]::Replace($nuspec2, "\<version\>([^""]*)\</version\>", "<version>" + $version + "</version>")
[System.IO.File]::WriteAllText($nuspecPath2, $nuspec2, [System.Text.Encoding]::UTF8)

# change the AssemblyInfo project file
$assemblyInfo = [System.IO.File]::ReadAllText($assemblyInfoPath, [System.Text.Encoding]::UTF8)
$assemblyInfo = [System.Text.RegularExpressions.Regex]::Replace($assemblyInfo, "\[assembly: AssemblyVersion\(""([^""]*)""\)\]", "[assembly: AssemblyVersion(""" + $versionWithoutPre + """)]")
$assemblyInfo = [System.Text.RegularExpressions.Regex]::Replace($assemblyInfo, "\[assembly: AssemblyFileVersion\(""([^""]*)""\)]", "[assembly: AssemblyFileVersion(""" + $versionWithoutPre + """)]")
[System.IO.File]::WriteAllText($assemblyInfoPath, $assemblyInfo, [System.Text.Encoding]::UTF8)

# change the AssemblyInfo project file
$assemblyInfo2 = [System.IO.File]::ReadAllText($assemblyInfoPath2, [System.Text.Encoding]::UTF8)
$assemblyInfo2 = [System.Text.RegularExpressions.Regex]::Replace($assemblyInfo2, "\[assembly: AssemblyVersion\(""([^""]*)""\)\]", "[assembly: AssemblyVersion(""" + $versionWithoutPre + """)]")
$assemblyInfo2 = [System.Text.RegularExpressions.Regex]::Replace($assemblyInfo2, "\[assembly: AssemblyFileVersion\(""([^""]*)""\)]", "[assembly: AssemblyFileVersion(""" + $versionWithoutPre + """)]")
[System.IO.File]::WriteAllText($assemblyInfoPath2, $assemblyInfo2, [System.Text.Encoding]::UTF8)