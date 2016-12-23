param([String]$version)

if ($version.Contains("-")) {
	$versionWithoutPre = $version.Substring(0, $version.IndexOf("-"))
} 
else {
	$versionWithoutPre = $version
}

$nuspecPath = ".\DotVVM.DynamicData.nuspec"
$assemblyInfoPath = "..\DotVVM.Framework.Controls.DynamicData\Properties\AssemblyInfo.cs"

# determine latest installed dotvvm version
[xml]$file = get-content ..\DotVVM.Framework.Controls.DynamicData\packages.config
foreach($elm in  $file.GetElementsByTagName("package"))
{
    $id = $elm.Attributes["id"];
    if($id.Value -like "DotVVM")
    { 
        $dotvvmVersion = $elm.Attributes["version"].Value;
        if($dotvvmVersion.Contains("-"))
        {
            $dotvvmVersionWithoutPre = $dotvvmVersion.Substring(0, $dotvvmVersion.IndexOf("-"))
        }
        else{
            $dotvvmVersionWithoutPre = $dotvvmVersion;
        }
    }
}

echo "Registering DotVVM Version $dotvvmVersion";

# change the nuspec
$nuspec = [System.IO.File]::ReadAllText($nuspecPath, [System.Text.Encoding]::UTF8)
$nuspec = [System.Text.RegularExpressions.Regex]::Replace($nuspec, "\<version\>([^""]*)\</version\>", "<version>" + $version + "</version>")
$nuspec = [System.Text.RegularExpressions.Regex]::Replace($nuspec, "\<dependency id=""DotVVM"" version=""([^""]*)"" /\>", "<dependency id=""DotVVM"" version=""" + $dotvvmVersion + """ />")
[System.IO.File]::WriteAllText($nuspecPath, $nuspec, [System.Text.Encoding]::UTF8)


# change the AssemblyInfo project file
$assemblyInfo = [System.IO.File]::ReadAllText($assemblyInfoPath, [System.Text.Encoding]::UTF8)
$assemblyInfo = [System.Text.RegularExpressions.Regex]::Replace($assemblyInfo, "\[assembly: AssemblyVersion\(""([^""]*)""\)\]", "[assembly: AssemblyVersion(""" + $versionWithoutPre + """)]")
$assemblyInfo = [System.Text.RegularExpressions.Regex]::Replace($assemblyInfo, "\[assembly: AssemblyFileVersion\(""([^""]*)""\)]", "[assembly: AssemblyFileVersion(""" + $versionWithoutPre + """)]")
[System.IO.File]::WriteAllText($assemblyInfoPath, $assemblyInfo, [System.Text.Encoding]::UTF8)
