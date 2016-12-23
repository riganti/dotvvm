param([String]$version, [String]$apiKey, [String]$server, [String]$branchName, [String]$repoUrl)

function CleanOldGeneratedPackages() {
	foreach ($package in $packages) {
		del .\$($package.Directory)\bin\debug\*.nupkg -ErrorAction SilentlyContinue
	}
}

function SetVersion() {
  	foreach ($package in $packages) {
		$filePath = ".\$($package.Directory)\$($package.Directory).csproj"
		$file = [System.IO.File]::ReadAllText($filePath, [System.Text.Encoding]::UTF8)
		$file = [System.Text.RegularExpressions.Regex]::Replace($file, "\<VersionPrefix\>([^<]+)\</VersionPrefix\>", "<VersionPrefix>" + $version + "</VersionPrefix>")
		$file = [System.Text.RegularExpressions.Regex]::Replace($file, "\<PackageVersion\>([^<]+)\</PackageVersion\>", "<PackageVersion>" + $version + "</PackageVersion>")
		[System.IO.File]::WriteAllText($filePath, $file, [System.Text.Encoding]::UTF8)
		
		$filePath = ".\$($package.Directory)\Properties\AssemblyInfo.cs"
		$file = [System.IO.File]::ReadAllText($filePath, [System.Text.Encoding]::UTF8)
		$file = [System.Text.RegularExpressions.Regex]::Replace($file, "\[assembly: AssemblyVersion\(""([^""]+)""\)\]", "[assembly: AssemblyVersion(""" + $versionWithoutPre + """)]")
		$file = [System.Text.RegularExpressions.Regex]::Replace($file, "\[assembly: AssemblyFileVersion\(""([^""]+)""\)]", "[assembly: AssemblyFileVersion(""" + $versionWithoutPre + """)]")
		[System.IO.File]::WriteAllText($filePath, $file, [System.Text.Encoding]::UTF8)
	}  
}

function BuildPackages() {
	foreach ($package in $packages) {
		cd .\$($package.Directory)
		& dotnet restore --source http://nuget.riganti.cz:8080/nuget --source https://nuget.org/api/v2/
		& dotnet pack
		cd ..
	}
}

function PushPackages() {
	foreach ($package in $packages) {
		& .\Tools\nuget.exe push .\$($package.Directory)\bin\debug\$($package.Package).$version.nupkg -source $server -apiKey $apiKey
	}
}

function GitCheckout() {
	& git checkout $branchName 2>&1
	& git -c http.sslVerify=false pull $repoUrl 2>&1
}

function GitPush() {
	& git commit -am "NuGet package version $version"
	& git rebase HEAD $branchName
	& git -c http.sslVerify=false push $repoUrl $branchName 2>&1
}


$packages = @(
	[pscustomobject]@{ Package = "DotVVM.DynamicData"; Directory = "DotVVM.Framework.Controls.DynamicData" }
)

$versionWithoutPre = $version
if ($versionWithoutPre.Contains("-")) {
	$versionWithoutPre = $versionWithoutPre.Substring(0, $versionWithoutPre.IndexOf("-"))
}

CleanOldGeneratedPackages;
GitCheckout;
SetVersion;
BuildPackages;
PushPackages;
GitPush;