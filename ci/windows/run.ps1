param(
    [switch] $noNpmBuild = $false,
    [switch] $noSlnRestore = $false,
    [switch] $noSlnBuild = $false,
    [switch] $noUnitTests = $false,
    [switch] $noUITests = $false)

$root = $env:DOTVVM_ROOT
if ($null -eq $root) {
    $root = "$PWD"
}
$env:DOTVVM_ROOT = $root

$configuration = $env:CONFIGURATION
if ($null -eq $configuration) {
    $configuration = "Release"
}

$sln = "$root\ci\windows\Windows.sln"
$packagesDir = "$root\src\packages\"
$testResultsDir = "$root\artifacts\test\"

Write-Host "ROOT=$ROOT"
Write-Host "CONFIGURATION=$CONFIGURATION"

if ($noNpmBuild -ne $true) {
    Write-Host "--------------------------------"
    Write-Host "npm build"
    Write-Host "--------------------------------"
    Set-Location $root\src\DotVVM.Framework
    npm ci --cache $root\.npm --prefer-offline
    npm run build
    if ($LASTEXITCODE -ne 0) {
        Write-Host "npm build failed"
        exit 1
    }
}


if ($noSlnRestore -ne $true) {
    Write-Host "--------------------------------"
    Write-Host "sln restore"
    Write-Host "--------------------------------"
    Set-Location $root
    & "$root\src\Tools\NuGet.exe" restore $sln -PackagesDirectory $packagesDir
    dotnet restore $sln --packages $packagesDir
    if ($LASTEXITCODE -ne 0) {
        Write-Host "nuget restore failed"
        exit 1
    }
}

if ($noSlnBuild -ne $true) {
    Write-Host "--------------------------------"
    Write-Host "sln build"
    Write-Host "--------------------------------"
    msbuild $sln -v:m `
        -p:PublishProfile=$root\ci\windows\GenericPublish.pubxml `
        -p:DeployOnBuild=true `
        -p:Configuration=$configuration `
        -p:SourceLinkCreate=true
    if ($LASTEXITCODE -ne 0) {
        Write-Host "dotnet build failed"
        exit 1
    }
}

if ($noUnitTests -ne $true) {
    Write-Host "--------------------------------"
    Write-Host "unit tests"
    Write-Host "--------------------------------"
    dotnet test src/DotVVM.Framework.Tests `
        --no-build `
        --configuration $configuration `
        --logger trx `
        --results-directory $testResultsDir `
        --collect "Code Coverage"
}

function Clean-UITest {
    Stop-Process -Force -Name chrome -ErrorAction SilentlyContinue
    Stop-Process -Force -Name chromedriver -ErrorAction SilentlyContinue
    Remove-IISSite -Confirm:$false -Name dotvvm.owin
    Remove-IISSite -Confirm:$false -Name dotvvm.owin.api
}

if ($noUITests -ne $true) {
    Write-Host "--------------------------------"
    Write-Host "UI tests"
    Write-Host "--------------------------------"

    Import-Module IISAdministration
    Clean-UITest

    icacls $root\artifacts\ /grant "IIS_IUSRS:(OI)(CI)F"

    New-IISSite -Name dotvvm.owin `
        -PhysicalPath $root\artifacts\DotVVM.Samples.BasicSamples.Owin `
        -BindingInformation "*:5407:"

    New-IISSite -Name dotvvm.owin.api `
        -PhysicalPath $root\artifacts\DotVVM.Samples.BasicSamples.Api.Owin `
        -BindingInformation "*:61453:"

    Copy-Item -Force -Recurse `
        $root\src\DotVVM.Samples.Common `
        $root\artifacts

    Copy-Item -Force `
        $root\src\DotVVM.Samples.Tests\Profiles\seleniumconfig.owin.chrome.json `
        $root\src\DotVVM.Samples.Tests\seleniumconfig.json

    dotnet test $root\src\DotVVM.Samples.Tests `
        --configuration $configuration `
        --logger trx `
        --results-directory $testResultsDir

    Clean-UITest

    Get-Process
}
