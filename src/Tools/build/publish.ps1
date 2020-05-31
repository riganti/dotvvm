param([String]$version, [String]$apiKey, [String]$server, [String]$branchName, [String]$repoUrl, [String]$nugetRestoreAltSource = "", [bool]$pushTag, [String]$configuration, [String]$apiKeyInternal, [String]$internalServer, [String]$signUser = "", [String]$signSecret = "", $signConfigPath = "")
$currentDirectory = $PWD

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

function RestoreSignClient() {
    & dotnet tool restore | Out-Host
}

function SetVersion() {
    Write-Host "Setting version: $version ..."
    Write-Host "Current directory: $currentDirectory"  
    foreach ($package in $packages) {

        Write-Host --------------------------------

        $filePath = Join-Path $currentDirectory ".\$($package.Directory)\$($package.Directory).csproj" -Resolve
            Write-Host "Updating $filePath" 
    
    
        $file = [System.IO.File]::ReadAllText($filePath, [System.Text.Encoding]::UTF8)
        $file = [System.Text.RegularExpressions.Regex]::Replace($file, "\<VersionPrefix\>([^<]+)\</VersionPrefix\>", "<VersionPrefix>" + $version + "</VersionPrefix>")
        $file = [System.Text.RegularExpressions.Regex]::Replace($file, "\<PackageVersion\>([^<]+)\</PackageVersion\>", "<PackageVersion>" + $version + "</PackageVersion>")
        [System.IO.File]::WriteAllText($filePath, $file, [System.Text.Encoding]::UTF8)
                
        $filePath = Join-Path $currentDirectory ".\$($package.Directory)\Properties\AssemblyInfo.cs" 
        if(Test-Path $filePath) {
            Write-Host "Updating $filePath" 
            $file = [System.IO.File]::ReadAllText($filePath, [System.Text.Encoding]::UTF8)
            $file = [System.Text.RegularExpressions.Regex]::Replace($file, "\[assembly: AssemblyVersion\(""([^""]+)""\)\]", "[assembly: AssemblyVersion(""" + $versionWithoutPre + """)]")
            $file = [System.Text.RegularExpressions.Regex]::Replace($file, "\[assembly: AssemblyFileVersion\(""([^""]+)""\)]", "[assembly: AssemblyFileVersion(""" + $versionWithoutPre + """)]")
            [System.IO.File]::WriteAllText($filePath, $file, [System.Text.Encoding]::UTF8)
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
            & dotnet restore --source $nugetRestoreAltSource --source https://nuget.org/api/v2/ | Out-Host
        }
        Write-Host "Packing project in directory $PWD"
        
        & dotnet pack -c $configuration --include-symbols  | Out-Host
        cd ..
    }
}

function SignPackages() {
    if ($signUser -ne "") {        
        Write-Host "Signing packages ..."
        foreach ($package in $packages) {
            $baseDir = Join-Path $currentDirectory ".\$($package.Directory)\bin\$configuration\" 
            & dotnet signclient sign --baseDirectory "$baseDir" --input *.nupkg --config "$signConfigPath" --user "$signUser" --secret "$signSecret" --name "$($package.Package)" --description "$($package.Package + " " + $version)" --descriptionUrl "https://github.com/riganti/dotvvm" | Out-Host
        }
    }
}

function PushPackages() {
    Write-Host "Pushing packages ..."
    foreach ($package in $packages) {
        & .\Tools\nuget.exe push .\$($package.Directory)\bin\$configuration\$($package.Package).$version.symbols.nupkg -source $server -apiKey $apiKey | Out-Host
    }
}

function BuildTemplates() {
    Write-Host "Building templates ..."
    del .\Templates\*.nupkg  -ErrorAction SilentlyContinue
    
    $filePath = ".\Templates\DotVVM.Templates.nuspec"
    $file = [System.IO.File]::ReadAllText($filePath, [System.Text.Encoding]::UTF8)
    $file = [System.Text.RegularExpressions.Regex]::Replace($file, "\<version\>([^<]+)\</version\>", "<version>" + $version + "</version>")
    [System.IO.File]::WriteAllText($filePath, $file, [System.Text.Encoding]::UTF8)
    
    & .\Tools\nuget.exe pack .\Templates\DotVVM.Templates.nuspec -outputdirectory .\Templates | Out-Host
}

function SignTemplates() {
    Write-Host "Signing templates ..."
    if ($signUser -ne "") {
        $baseDir = Join-Path $currentDirectory ".\Templates\" 
        & dotnet signclient sign --baseDirectory "$baseDir" --input *.nupkg --config "$signConfigPath" --user "$signUser" --secret "$signSecret" --name "DotVVM.Templates" --description "DotVVM.Templates $version" --descriptionUrl "https://github.com/riganti/dotvvm" | Out-Host
    }
}

function PublishTemplates() {
    Write-Host "Publishing templates ..."
    & .\Tools\nuget.exe push .\Templates\DotVVM.Templates.$version.nupkg -source $server -apiKey $apiKey | Out-Host 
}

function GitCheckout() {
    invoke-git checkout $branchName
    invoke-git -c http.sslVerify=false pull $repoUrl $branchName
}

function GitPush() {

    invoke-git config --global user.email "rigantiteamcity"
    invoke-git config --global user.name "Riganti Team City"
    if ($pushTag) {
            invoke-git tag "v$($version)" HEAD
    }
    invoke-git commit -am "NuGet package version $version"
    invoke-git rebase HEAD $branchName
    invoke-git push --follow-tags $repoUrl $branchName
}



### Configuration

$packages = @(
    [pscustomobject]@{ Package = "DotVVM.Core"; Directory = "DotVVM.Core" },
    [pscustomobject]@{ Package = "DotVVM"; Directory = "DotVVM.Framework" },
    [pscustomobject]@{ Package = "DotVVM.Owin"; Directory = "DotVVM.Framework.Hosting.Owin" },
    [pscustomobject]@{ Package = "DotVVM.AspNetCore"; Directory = "DotVVM.Framework.Hosting.AspNetCore" },
    [pscustomobject]@{ Package = "DotVVM.CommandLine"; Directory = "DotVVM.CommandLine" },
    [pscustomobject]@{ Package = "DotVVM.Tools.StartupPerf"; Directory = "DotVVM.Framework.StartupPerfTests" },
    [pscustomobject]@{ Package = "DotVVM.Compiler.Light"; Directory = "DotVVM.Compiler.Light" },
    [pscustomobject]@{ Package = "DotVVM.Api.Swashbuckle.AspNetCore"; Directory = "DotVVM.Framework.Api.Swashbuckle.AspNetCore" },
    [pscustomobject]@{ Package = "DotVVM.Api.Swashbuckle.Owin"; Directory = "DotVVM.Framework.Api.Swashbuckle.Owin" }
)


### Publish Workflow

$versionWithoutPre = $version
if ($versionWithoutPre.Contains("-")) {
    $versionWithoutPre = $versionWithoutPre.Substring(0, $versionWithoutPre.IndexOf("-"))
}

CleanOldGeneratedPackages;
RestoreSignClient;
GitCheckout;
SetVersion;
BuildPackages;
SignPackages;
PushPackages;

BuildTemplates;
SignTemplates;
PublishTemplates;

GitPush;