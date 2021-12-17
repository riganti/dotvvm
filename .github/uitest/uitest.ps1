param(
    [string] $Root,
    [string] $Config,
    [string] $SamplesProfile = "seleniumconfig.owin.chrome.json",
    [string] $SamplesPort = "5407",
    [string] $SamplesPortApi = "61453")

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

$packagesDir = "$Root\src\packages\"
$testResultsDir = "$Root\artifacts\test\"
$samplesDir = "$Root\src\Samples\Tests\Tests\"
$ciDir = "$Root\ci\windows\"

# set the codepage to UTF-8
chcp 65001

Write-Host "ROOT=$Root"
Write-Host "CONFIGURATION=$Config"
Write-Host "TEST_RESULTS_DIR=$testResultsDir"
Write-Host "SAMPLES_DIR=$samplesDir"
Write-Host "SAMPLES_PROFILE=$SamplesProfile"
Write-Host "SAMPLES_PORT=$SamplesPort"
Write-Host "SAMPLES_PORT_API=$SamplesPortApi"

# ================
# helper functions
# ================

function Run-Command {
    param (
        [string][parameter(Position=0)]$Name,
        [scriptblock][parameter(Position=1)]$Command
    )
    Write-Host "::group::$Name"
    Write-Host -ForegroundColor Blue "$Command".Trim()
    Invoke-Command $Command
    Write-Host "::endgroup::"
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

# seleniumconfig.json needs to be copied before the build of the sln
$profilePath="$samplesDir\Profiles\$SamplesProfile"

if (-Not(Test-Path -PathType Leaf -Path $profilePath)) {
    Write-Host -ForegroundColor Red "Profile '$profilePath' doesn't exist."
    exit 1
}
Copy-Item -Force "$profilePath" "$samplesDir\seleniumconfig.json"

Ensure-Command "Build samples" {
    msbuild "$Root\src\Samples\Owin\DotVVM.Samples.BasicSamples.Owin.csproj" -v:m `
        -p:PublishProfile="$Root\ci\windows\GenericPublish.pubxml" `
        -p:DeployOnBuild=true `
        -p:Configuration="$Config" `
        -p:SourceLinkCreate=true
    msbuild "$Root\src\Samples\Api.Owin\DotVVM.Samples.BasicSamples.Api.Owin.csproj" -v:m `
        -p:PublishProfile="$Root\ci\windows\GenericPublish.pubxml" `
        -p:DeployOnBuild=true `
        -p:Configuration="$Config" `
        -p:SourceLinkCreate=true
}

Ensure-Command "Configure IIS" {
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
        "$Root\src\Samples\Common" `
        "$Root\artifacts"
}

Ensure-Command "Run UI tests" {
    $uiTestProcess = Start-Process -PassThru -NoNewWindow -FilePath "dotnet.exe" -ArgumentList `
        "test", `
        "$samplesDir", `
        "--configuration", `
        "$Config", `
        "--no-restore",`
        "--logger", `
        "trx;LogFileName=ui-test-results.trx", `
        "--results-directory", `
        "$testResultsDir"

    Wait-Process -Id "$($uiTestProcess.Id)"

    Clean-UITest
}
