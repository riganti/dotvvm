param([String]$version)

$versionWithoutPre = $version
if ($versionWithoutPre.Contains("-")) {
	$versionWithoutPre = $versionWithoutPre.Substring(0, $versionWithoutPre.IndexOf("-"))
}

## nuspecs
$nuspecPath = Resolve-Path ".\DotVVM.nuspec"
$nuspecPath2 = Resolve-Path ".\DotVVM.Core.nuspec"
$nuspecPath3 = Resolve-Path ".\DotVVM.Owin.nuspec"
$nuspecPath4 = Resolve-Path ".\DotVVM.AspNetCore.nuspec"

## project.json files
$jsonPath1 = Resolve-Path ".\..\..\DotVVM.Framework\project.json"
$jsonPath2 = Resolve-Path ".\..\..\DotVVM.Framework.Hosting.AspNetCore\project.json"
$jsonPath3 = Resolve-Path ".\..\..\DotVVM.Core\project.json"


## assembly info cs files
$assemblyInfoPath = Resolve-Path ".\..\..\DotVVM.Framework.Hosting.Owin\Properties\AssemblyInfo.cs"


# change the AssemblyInfo project file
$assemblyInfo = [System.IO.File]::ReadAllText($assemblyInfoPath, [System.Text.Encoding]::UTF8)
$assemblyInfo = [System.Text.RegularExpressions.Regex]::Replace($assemblyInfo, "\[assembly: AssemblyVersion\(""([^""]*)""\)\]", "[assembly: AssemblyVersion(""" + $versionWithoutPre + """)]")
$assemblyInfo = [System.Text.RegularExpressions.Regex]::Replace($assemblyInfo, "\[assembly: AssemblyFileVersion\(""([^""]*)""\)]", "[assembly: AssemblyFileVersion(""" + $versionWithoutPre + """)]")
[System.IO.File]::WriteAllText($assemblyInfoPath, $assemblyInfo, [System.Text.Encoding]::UTF8)


# change the nuspec
$nuspec = [System.IO.File]::ReadAllText($nuspecPath, [System.Text.Encoding]::UTF8)
$nuspec = [System.Text.RegularExpressions.Regex]::Replace($nuspec, "\<version\>([^""]*)\</version\>", "<version>" + $version + "</version>")
$nuspec = [System.Text.RegularExpressions.Regex]::Replace($nuspec, "\<dependency id=""DotVVM.Core"" version=""([^""]*)"" /\>", "<dependency id=""DotVVM.Core"" version=""" + $version + """ />")
[System.IO.File]::WriteAllText($nuspecPath, $nuspec, [System.Text.Encoding]::UTF8)

# change the nuspec
$nuspec2 = [System.IO.File]::ReadAllText($nuspecPath2, [System.Text.Encoding]::UTF8)
$nuspec2 = [System.Text.RegularExpressions.Regex]::Replace($nuspec2, "\<version\>([^""]*)\</version\>", "<version>" + $version + "</version>")
[System.IO.File]::WriteAllText($nuspecPath2, $nuspec2, [System.Text.Encoding]::UTF8)


# change the nuspec
$nuspec3 = [System.IO.File]::ReadAllText($nuspecPath3, [System.Text.Encoding]::UTF8)
$nuspec3 = [System.Text.RegularExpressions.Regex]::Replace($nuspec3, "\<version\>([^""]*)\</version\>", "<version>" + $version + "</version>")
[System.IO.File]::WriteAllText($nuspecPath3, $nuspec3, [System.Text.Encoding]::UTF8)


# change the nuspec
$nuspec4 = [System.IO.File]::ReadAllText($nuspecPath4, [System.Text.Encoding]::UTF8)
$nuspec4 = [System.Text.RegularExpressions.Regex]::Replace($nuspec4, "\<version\>([^""]*)\</version\>", "<version>" + $version + "</version>")
[System.IO.File]::WriteAllText($nuspecPath4, $nuspec4, [System.Text.Encoding]::UTF8)



$json1 = [System.IO.File]::ReadAllText($jsonPath1, [System.Text.Encoding]::UTF8)
$json1 = [System.Text.RegularExpressions.Regex]::Replace($json1, """version""(\s*):(\s*)""([^""]*)""", """version"": """ + $version + """")
[System.IO.File]::WriteAllText($jsonPath1, $json1, [System.Text.Encoding]::UTF8)



$json2 = [System.IO.File]::ReadAllText($jsonPath2, [System.Text.Encoding]::UTF8)
$json2 = [System.Text.RegularExpressions.Regex]::Replace($json2, """version""(\s*):(\s*)""([^""]*)""", """version"": """ + $version + """")
[System.IO.File]::WriteAllText($jsonPath2, $json2, [System.Text.Encoding]::UTF8)



$json3 = [System.IO.File]::ReadAllText($jsonPath3, [System.Text.Encoding]::UTF8)
$json3 = [System.Text.RegularExpressions.Regex]::Replace($json3, """version""(\s*):(\s*)""([^""]*)""", """version"": """ + $version + """")
[System.IO.File]::WriteAllText($jsonPath3, $json3, [System.Text.Encoding]::UTF8)

