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
		& dotnet restore
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
	& git checkout $branchName
	& git -c http.sslVerify=false pull $repoUrl
}

function GitPush() {
	& git commit -am "NuGet package version $version"
	& git rebase HEAD $branchName
	& git -c http.sslVerify=false push $repoUrl $branchName
}


$packages = @(
	[pscustomobject]@{ Package = "DotVVM.Core"; Directory = "DotVVM.Core" },
	[pscustomobject]@{ Package = "DotVVM"; Directory = "DotVVM.Framework" },
	[pscustomobject]@{ Package = "DotVVM.Owin"; Directory = "DotVVM.Framework.Hosting.Owin" },
	[pscustomobject]@{ Package = "DotVVM.AspNetCore"; Directory = "DotVVM.Framework.Hosting.AspNetCore" }
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