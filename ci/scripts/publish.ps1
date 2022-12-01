param(
    [String]$version,
    [String]$apiKey,
    [String]$server,
    [String]$branchName,
    [String]$repoUrl,
    [String]$nugetRestoreAltSource = "",
    [bool]$pushTag,
    [String]$configuration,
    [String]$apiKeyInternal,
    [String]$internalServer,
    [String]$signUser = "",
    [String]$signSecret = "",
    $signConfigPath = "")
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
        [parameter(ValueFromRemainingArguments = $true)]
        [string[]]$Arguments
    )

    & {
        [CmdletBinding()]
        param(
            [parameter(ValueFromRemainingArguments = $true)]
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
        del .\$($package.Directory)\bin\$configuration\*.snupkg -ErrorAction SilentlyContinue
    }
}

function RestoreSignClient() {
    & dotnet tool restore | Out-Host
}

function BuildPackages() {
    Write-Host "Build started"
    $originDirecotry = $PWD
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

        & dotnet pack -p:version=$version -p:ContinuousIntegrationBuild=true -c $configuration | Out-Host
        cd $originDirecotry
    }
}

function SignPackages() {
    if ($signUser -ne "") {
        Write-Host "Signing packages ..."
        foreach ($package in $packages) {
            $baseDir = Join-Path $currentDirectory ".\$($package.Directory)\bin\$configuration\"
            Write-Host "Signing $($package.Package + " " + $version) (Base dir: $baseDir)"
            & dotnet signclient sign --baseDirectory "$baseDir" --input *.nupkg  --config "$signConfigPath" --user "$signUser" --secret "$signSecret" --name "$($package.Package)" --description "$($package.Package + " " + $version)" --descriptionUrl "https://github.com/riganti/dotvvm" | Out-Host
        }
    }
}

function PushPackages() {
    Write-Host "Pushing packages ..."
    foreach ($package in $packages) {
        & ../ci/scripts/nuget.exe push .\$($package.Directory)\bin\$configuration\$($package.Package).$version.nupkg -source $server -apiKey $apiKey | Out-Host
        & ../ci/scripts/nuget.exe push .\$($package.Directory)\bin\$configuration\$($package.Package).$version.snupkg -source $server -apiKey $apiKey | Out-Host
    }
}

function BuildTemplates() {
    cd $currentDirectory

    Write-Host "Building templates ..."
    del .\Templates\*.nupkg  -ErrorAction SilentlyContinue

    $filePath = ".\Templates\DotVVM.Templates.nuspec"
    $file = [System.IO.File]::ReadAllText($filePath, [System.Text.Encoding]::UTF8)
    $file = [System.Text.RegularExpressions.Regex]::Replace($file, "\<version\>([^<]+)\</version\>", "<version>" + $version + "</version>")
    [System.IO.File]::WriteAllText($filePath, $file, [System.Text.Encoding]::UTF8)

    & ../ci/scripts/nuget.exe pack .\Templates\DotVVM.Templates.nuspec -outputdirectory .\Templates | Out-Host
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
    & ../ci/scripts/nuget.exe push .\Templates\DotVVM.Templates.$version.nupkg -source $server -apiKey $apiKey | Out-Host
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
    invoke-git push --follow-tags $repoUrl $branchName
}



### Configuration

$packages = @(
    [pscustomobject]@{ Package = "DotVVM.Core"; Directory = "Framework/Core" },
    [pscustomobject]@{ Package = "DotVVM"; Directory = "Framework/Framework" },
    [pscustomobject]@{ Package = "DotVVM.Owin"; Directory = "Framework/Hosting.Owin" },
    [pscustomobject]@{ Package = "DotVVM.AspNetCore"; Directory = "Framework/Hosting.AspNetCore" },
    [pscustomobject]@{ Package = "DotVVM.CommandLine"; Directory = "Tools/CommandLine" },
    [pscustomobject]@{ Package = "DotVVM.Tools.StartupPerf"; Directory = "Tools/StartupPerfTester" },
    [pscustomobject]@{ Package = "DotVVM.Api.Swashbuckle.AspNetCore"; Directory = "Api/Swashbuckle.AspNetCore" },
    [pscustomobject]@{ Package = "DotVVM.Api.Swashbuckle.Owin"; Directory = "Api/Swashbuckle.Owin" },
    [pscustomobject]@{ Package = "DotVVM.HotReload"; Directory = "Tools/HotReload/Common" },
    [pscustomobject]@{ Package = "DotVVM.HotReload.AspNetCore"; Directory = "Tools/HotReload/AspNetCore" },
    [pscustomobject]@{ Package = "DotVVM.HotReload.Owin"; Directory = "Tools/HotReload/Owin" }
    [pscustomobject]@{ Package = "DotVVM.Testing"; Directory = "Framework/Testing" },
    [pscustomobject]@{ Package = "DotVVM.DynamicData"; Directory = "DynamicData/DynamicData" },
    [pscustomobject]@{ Package = "DotVVM.DynamicData.Annotations"; Directory = "DynamicData/Annotations" },
    [pscustomobject]@{ Package = "DotVVM.AutoUI"; Directory = "AutoUI/Core" },
    [pscustomobject]@{ Package = "DotVVM.AutoUI.Annotations"; Directory = "AutoUI/Annotations" },
    [pscustomobject]@{ Package = "DotVVM.Tracing.ApplicationInsights"; Directory = "Tracing/ApplicationInsights" },
    [pscustomobject]@{ Package = "DotVVM.Tracing.ApplicationInsights.AspNetCore"; Directory = "Tracing/ApplicationInsights.AspNetCore" },
    [pscustomobject]@{ Package = "DotVVM.Tracing.ApplicationInsights.Owin"; Directory = "Tracing/ApplicationInsights.Owin" }
    [pscustomobject]@{ Package = "DotVVM.Tracing.MiniProfiler.AspNetCore"; Directory = "Tracing/MiniProfiler.AspNetCore" },
    [pscustomobject]@{ Package = "DotVVM.Tracing.MiniProfiler.Owin"; Directory = "Tracing/MiniProfiler.Owin" }
)


### Publish Workflow

$versionWithoutPre = $version
if ($versionWithoutPre.Contains("-")) {
    $versionWithoutPre = $versionWithoutPre.Substring(0, $versionWithoutPre.IndexOf("-"))
}

if ($branchName.StartsWith("refs/heads/") -eq $true) {
    $branchName = $branchName.Substring("refs/heads/".Length)
}

CleanOldGeneratedPackages;
RestoreSignClient;
GitCheckout;
BuildPackages;

SignPackages;
PushPackages;

BuildTemplates;
SignTemplates;
PublishTemplates;

GitPush;
