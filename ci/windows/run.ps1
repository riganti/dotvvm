param(
    [string] $Root,
    [string] $Config,
    [string] $SamplesProfile = "seleniumconfig.owin.chrome.json",
    [string] $SamplesPort = "5407",
    [string] $SamplesPortApi = "61453",
    [switch] $Clean = $false,
    [switch] $NoNpmBuild = $false,
    [switch] $NoSlnRestore = $false,
    [switch] $NoSlnBuild = $false,
    [switch] $NoUnitTests = $false,
    [switch] $NoJSTests = $false,
    [switch] $NoUITests = $false)

# ==================
# config var setting
# ==================

$Root = $env:DOTVVM_ROOT
if ([string]::IsNullOrEmpty($Root)) {
    $Root = "$PWD"
}
$env:DOTVVM_ROOT = $Root

$Config = $env:CONFIGURATION
if ([string]::IsNullOrEmpty($Config)) {
    $Config = "Release"
}

$sln = "$Root\ci\windows\Windows.sln"
$packagesDir = "$Root\src\packages\"
$testResultsDir = "$Root\artifacts\test\"
$samplesDir = "$Root\src\DotVVM.Samples.Tests\"
$ciDir = "$Root\ci\windows\"

Write-Host "ROOT=$Root"
Write-Host "SLN=$sln"
Write-Host "CONFIGURATION=$Config"
Write-Host "TEST_RESULTS_DIR=$testResultsDir"
Write-Host "SAMPLES_DIR=$samplesDir"
Write-Host "SAMPLES_PROFILE=$SamplesProfile"
Write-Host "SAMPLES_PORT=$SamplesPort"
Write-Host "SAMPLES_PORT_API=$SamplesPortApi"

# ================
# helper functions
# ================

function Write-Header {
    param (
        [string][parameter(Position=0)]$Text
    )
    Write-Host -ForegroundColor Yellow "--------------------------------"
    Write-Host -BackgroundColor Yellow -ForegroundColor Black $Text
    Write-Host -ForegroundColor Yellow "--------------------------------"
}

function Run-Command {
    param (
        [string][parameter(Position=0)]$Name,
        [scriptblock][parameter(Position=1)]$Command
    )
    Write-Header $Name
    Write-Host -ForegroundColor Blue "$Command".Trim()
    Invoke-Command $Command
}

function Ensure-Command {
    param (
        [string][parameter(Position=0)]$Name,
        [scriptblock][parameter(Position=1)]$Command
    )
    Run-Command $Name $Command
    if ($LASTEXITCODE -ne 0) {
        Write-Host -ForegroundColor Red "$Name failed"
        exit 1
    }
}

function Clean-UITest {
    Stop-Process -Force -Name chrome -ErrorAction SilentlyContinue
    Stop-Process -Force -Name chromedriver -ErrorAction SilentlyContinue
    Remove-IISSite -Confirm:$false -Name dotvvm.owin
    Remove-IISSite -Confirm:$false -Name dotvvm.owin.api
}

# =============================
# actual continuous integration
# =============================

if ($Clean -eq $true) {
    Ensure-Command "clean" {
        git clean -dfx
    }
}

if ($NoNpmBuild -ne $true) {
    Ensure-Command "npm build" {
        Set-Location $Root\src\DotVVM.Framework
        npm ci --cache $Root\.npm --prefer-offline
        npm run build
    }
}

if ($NoSlnRestore -ne $true) {
    Ensure-Command "sln restore" {
        Set-Location $Root
        & "$ciDir\NuGet.exe" restore $sln -PackagesDirectory $packagesDir -ConfigFile "$ciDir\NuGet.config"
        dotnet restore $sln --packages $packagesDir --configfile "$ciDir\NuGet.config"
    }
}

# seleniumconfig.json needs to be copied before the build of the sln
if ($NoUITests -ne $true) {
    $profilePath="$samplesDir\Profiles\$SamplesProfile"

    if (-Not(Test-Path -PathType Leaf -Path $profilePath)) {
        Write-Host -ForegroundColor Red "Profile '$profilePath' doesn't exist."
        exit 1
    }
    Copy-Item -Force "$profilePath" "$samplesDir\seleniumconfig.json"
}

if ($NoSlnBuild -ne $true) {
    Ensure-Command "sln build" {
        msbuild $sln -v:m `
            -p:PublishProfile="$Root\ci\windows\GenericPublish.pubxml" `
            -p:DeployOnBuild=true `
            -p:Configuration="$Config" `
            -p:SourceLinkCreate=true
    }
}

if ($NoUnitTests -ne $true) {
    Run-Command "unit tests" {
        dotnet test src/DotVVM.Framework.Tests `
            --no-build `
            --configuration "$Config" `
            --logger "trx;LogFileName=unit-test-results.trx" `
            --results-directory "$testResultsDir" `
            --collect "Code Coverage"
    }
}

if ($NoJSTests -ne $true) {
    Run-Command "JS tests" {
        Set-Location "$Root\src\DotVVM.Framework"
        npx jest --ci --reporters="jest-junit"
        Copy-Item junit.xml "$testResultsDir\js-test-results.xml"
        Set-Location "$Root"
    }
}

if ($NoUITests -ne $true) {
    Run-Command "UI tests" {
        Import-Module IISAdministration
        Clean-UITest

        icacls "$Root\artifacts\" /grant "IIS_IUSRS:(OI)(CI)F"

        New-IISSite -Name "dotvvm.owin" `
            -PhysicalPath "$Root\artifacts\DotVVM.Samples.BasicSamples.Owin" `
            -BindingInformation "*:${SamplesPort}:"

        New-IISSite -Name "dotvvm.owin.api" `
            -PhysicalPath "$Root\artifacts\DotVVM.Samples.BasicSamples.Api.Owin" `
            -BindingInformation "*:${SamplesPortApi}:"

        Copy-Item -Force -Recurse `
            "$Root\src\DotVVM.Samples.Common" `
            "$Root\artifacts"

        $uiTestProcess = Start-Process -PassThru -NoNewWindow -FilePath "dotnet.exe" `
            -ArgumentList "test","$samplesDir","--configuration","$Config","--no-build",`
                "--logger","trx;LogFileName=ui-test-results.trx","--results-directory","$testResultsDir"

        Wait-Process -Id "$($uiTestProcess.Id)"

        Clean-UITest
    }
}

Get-Process
