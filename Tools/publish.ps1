param([String]$version, [String]$apiKey, [String]$server, [String]$branchName, [String]$repoUrl, [String]$nugetRestoreAltSource = "", [bool]$pushTag, [String]$configuration, [String]$apiKeyInternal, [String]$internalServer)
$curretDirectory = $PWD

### Helper Functions

function Invoke-Git {
<#
.Synopsis
Wrapper function that deals with Powershell's peculiar error output when Git uses the error stream.
.Example
Invoke-Git ThrowError
$LASTEXITCODE
#>
    [CmdletBinding()]
    param(
        [parameter(ValueFromRemainingArguments=$true)]
        [string[]]$Arguments
    )

    & {
        [CmdletBinding()]
        param(
            [parameter(ValueFromRemainingArguments=$true)]
            [string[]]$InnerArgs
        )
        git.exe $InnerArgs 2>&1
    } -ErrorAction SilentlyContinue -ErrorVariable fail @Arguments

    if ($fail) {
        $fail.Exception
    }
}

function CleanOldGeneratedPackages() {
    Write-Host "Cleaning old versions of nupkg ..."
	foreach ($package in $packages) {
		del .\$($package.Directory)\bin\$configuration\*.nupkg -ErrorAction SilentlyContinue
	}
}


function SetVersion() {
	Write-Host "Setting version: $version ..."
	Write-Host "Current directory: $curretDirectory"  
	foreach ($package in $packages) {

        Write-Host --------------------------------

		$filePath = Join-Path $curretDirectory ".\$($package.Directory)\$($package.Directory).csproj" -Resolve
	        Write-Host "Updating $filePath" 
	
	
        $file = [System.IO.File]::ReadAllText($filePath, [System.Text.Encoding]::UTF8)
		$file = [System.Text.RegularExpressions.Regex]::Replace($file, "\<VersionPrefix\>([^<]+)\</VersionPrefix\>", "<VersionPrefix>" + $version + "</VersionPrefix>")
		$file = [System.Text.RegularExpressions.Regex]::Replace($file, "\<PackageVersion\>([^<]+)\</PackageVersion\>", "<PackageVersion>" + $version + "</PackageVersion>")
		[System.IO.File]::WriteAllText($filePath, $file, [System.Text.Encoding]::UTF8)
				
		$filePath = Join-Path $curretDirectory ".\$($package.Directory)\Properties\AssemblyInfo.cs" 
        if(Test-Path $filePath) {
	        Write-Host "Updating $filePath" 
            $file = [System.IO.File]::ReadAllText($filePath, [System.Text.Encoding]::UTF8)
            $file = [System.Text.RegularExpressions.Regex]::Replace($file, "\[assembly: AssemblyVersion\(""([^""]+)""\)\]", "[assembly: AssemblyVersion(""" + $versionWithoutPre + """)]")
            $file = [System.Text.RegularExpressions.Regex]::Replace($file, "\[assembly: AssemblyFileVersion\(""([^""]+)""\)]", "[assembly: AssemblyFileVersion(""" + $versionWithoutPre + """)]")
			[System.IO.File]::WriteAllText($filePath, $file, [System.Text.Encoding]::UTF8) | Write-Host
        }
	}  
}

function BuildPackages() {
    Write-Host "Build started"
	foreach ($package in $packages) {
		cd .\$($package.Directory)
        Write-Host "Building in directory $PWD"

		if ($nugetRestoreAltSource -eq "") {
			& dotnet restore  | Out-Host
		}
		else {
			& dotnet restore --source $nugetRestoreAltSource --source https://nuget.org/api/v2/  | Write-Host
		}
        Write-Host "Packing project in directory $PWD"
		
		& dotnet pack -c $configuration --include-symbols  | Out-Host
		cd ..
	}
}

function PushPackages() {
    Write-Host "Pushing packages ..."
	foreach ($package in $packages) {
		& .\Tools\nuget.exe push .\$($package.Directory)\bin\$configuration\$($package.Package).$version.symbols.nupkg -source $server -apiKey $apiKey | Out-Host
	}
}

function GitCheckout() {
	invoke-git checkout $branchName
	invoke-git -c http.sslVerify=false pull $repoUrl $branchName
}

function GitPush() {
	if ($pushTag) {
			invoke-git tag "v$($version)" HEAD
	}
	invoke-git commit -am "NuGet package version $version"
	invoke-git rebase HEAD $branchName
	invoke-git push --follow-tags $repoUrl $branchName
}



### Configuration

$packages = @(
	[pscustomobject]@{ Package = "DotVVM.Diagnostics.StatusPage"; Directory = "DotVVM.Diagnostics.StatusPage" }
)



### Publish Workflow

$versionWithoutPre = $version
if ($versionWithoutPre.Contains("-")) {
	$versionWithoutPre = $versionWithoutPre.Substring(0, $versionWithoutPre.IndexOf("-"))
}

CleanOldGeneratedPackages;
#GitCheckout;
SetVersion;
BuildPackages;
PushPackages;
#GitPush;