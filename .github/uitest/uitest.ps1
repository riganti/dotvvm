param(
    [string] $root,
    [string] $config,
    [string] $samplesProfile = "seleniumconfig.owin.chrome.json",
    [string] $samplesPort = "5407",
    [string] $samplesPortApi = "61453",
    [string] $trxName = "ui-test-results.trx")

# set the codepage to UTF-8
chcp 65001

$root = $env:DOTVVM_ROOT
if ([string]::IsNullOrEmpty($root)) {
    $root = "$PWD"
}
$env:DOTVVM_ROOT = $root

$config = $env:CONFIGURATION
if ([string]::IsNullOrEmpty($config)) {
    $config = "Release"
}

Write-Host -ForegroundColor Blue @"
Root: $root
Config: $config
SamplesProfile: $samplesProfile
samplesPort: $samplesPort
samplesPortApi: $samplesPortApi
TrxName: $trxName
"@

$testResultsDir = "$root\artifacts\test\"
$testDir = "$root\src\Samples\Tests\Tests\"
$profilePath="$testDir\Profiles\$samplesProfile"

function Invoke-Cmds {
    param (
        [string][parameter(Position=0)]$name,
        [scriptblock][parameter(Position=1)]$command
    )
    Write-Host "::group::$name"
    Write-Host -ForegroundColor Blue "$command".Trim()

    $LASTEXITCODE = 0
    Invoke-Command $command | Write-Host
    $Result = ($LASTEXITCODE -eq 0) -and $?

    Write-Host "::endgroup::"
    return $Result
}

function Invoke-RequiredCmds {
    param (
        [string][parameter(Position=0)]$name,
        [scriptblock][parameter(Position=1)]$command
    )
    $ErrorActionPreference = "Stop"
    if (!(Invoke-Cmds $name $command)) {
        Write-Error "$name failed"
        exit 1
    }
}

function Publish-Sample {
    param ([string][parameter(Position=0)]$path)
    Invoke-RequiredCmds "Publish sample '$path'" {
        $msBuildProcess = Start-Process -PassThru -NoNewWindow -FilePath "msbuild.exe" -ArgumentList `
            "$path", `
            "-v:m",`
            "-noLogo",`
            "-p:PublishProfile=$root\ci\windows\GenericPublish.pubxml", `
            "-p:DeployOnBuild=true", `
            "-p:Configuration=$config", `
            "-p:SourceLinkCreate=true"
        Wait-Process -Id "$($msBuildProcess.Id)"
        if ($msBuildProcess.ExitCode -ne 0) {
            throw "MSBuild failed."
        }
    }
}

function Start-Sample {
    param (
        [string][parameter(Position=0)]$sampleName,
        [string][parameter(Position=1)]$path,
        [int][parameter(Position=2)]$port
    )
    Invoke-RequiredCmds "Start sample '$sampleName'" {
        Remove-IISSite -Confirm:$false -Name $sampleName  -ErrorAction SilentlyContinue

        icacls "$root\artifacts\" /grant "IIS_IUSRS:(OI)(CI)F"

        New-IISSite -Name "$sampleName" `
            -PhysicalPath "$path" `
            -BindingInformation "*:${port}:"
    }
}

function Stop-Sample {
    param ([string][parameter(Position=0)]$name)
    Invoke-RequiredCmds "Stop sample '$name'" {
        Stop-Process -Force -Name chrome -ErrorAction SilentlyContinue
        Stop-Process -Force -Name chromedriver -ErrorAction SilentlyContinue
        # Stop-Process -Force -Name firefox -ErrorAction SilentlyContinue
        # Stop-Process -Force -Name geckodriver -ErrorAction SilentlyContinue
        Remove-IISSite -Confirm:$false -Name $name -ErrorAction SilentlyContinue
    }
}

Invoke-RequiredCmds "Copy profile" {
    if (-Not(Test-Path -PathType Leaf -Path $profilePath)) {
        Write-Host -ForegroundColor Red "Profile '$profilePath' doesn't exist."
        exit 1
    }
    Copy-Item -Force "$profilePath" "$testDir\seleniumconfig.json"
}

Publish-Sample "$root\src\Samples\Owin\DotVVM.Samples.BasicSamples.Owin.csproj"
Publish-Sample "$root\src\Samples\Api.Owin\DotVVM.Samples.BasicSamples.Api.Owin.csproj"

Invoke-RequiredCmds "Copy common" {
    Copy-Item -Force -Recurse `
        "$root\src\Samples\Common" `
        "$root\artifacts"
}

Invoke-RequiredCmds "Configure IIS" {
    Import-Module IISAdministration
}

$samplesOwinName = "dotvvm.owin"
$samplesOwinPath = "$root\artifacts\DotVVM.Samples.BasicSamples.Owin"

$samplesApiOwinName = "dotvvm.owin.api"
$samplesApiOwinPath = "$root\artifacts\DotVVM.Samples.BasicSamples.Api.Owin"

Stop-Sample $samplesOwinName
Stop-Sample $samplesApiOwinName
Start-Sample $samplesOwinName $samplesOwinPath $samplesPort
Start-Sample $samplesApiOwinName $samplesApiOwinPath $samplesApiPort

Invoke-RequiredCmds "Run UI tests" {
    $uiTestProcess = Start-Process -PassThru -NoNewWindow -FilePath "dotnet.exe" -ArgumentList `
        "test", `
        "$testDir", `
        "--configuration", `
        "$config", `
        "--no-restore",`
        "--logger", `
        "trx;LogFileName=$TrxName", `
        "--results-directory", `
        "$testResultsDir"

    Wait-Process -Id "$($uiTestProcess.Id)"
}

Stop-Sample $samplesOwinName
Stop-Sample $samplesApiOwinName
