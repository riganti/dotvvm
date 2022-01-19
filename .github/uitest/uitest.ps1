param(
    [string] $root,
    [string] $config,
    [string] $samplesProfile = "seleniumconfig.owin.chrome.json",
    [string] $samplesOwinPort = "5407",
    [string] $samplesApiOwinPort = "61453",
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
SamplesOwinPort: $samplesOwinPort
SamplesApiOwinPort: $samplesApiOwinPort
TrxName: $trxName
"@

$testResultsDir = "$root\artifacts\test\"
$testDir = "$root\src\Samples\Tests\Tests\"
$profilePath = "$testDir\Profiles\$samplesProfile"
$samplesOwinName = "dotvvm.owin"
$samplesOwinPath = "$root\artifacts\DotVVM.Samples.BasicSamples.Owin"
$samplesApiOwinName = "dotvvm.owin.api"
$samplesApiOwinPath = "$root\artifacts\DotVVM.Samples.BasicSamples.Api.Owin"

function Invoke-Cmds {
    param (
        [string][parameter(Position = 0)]$name,
        [scriptblock][parameter(Position = 1)]$command
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
        [string][parameter(Position = 0)]$name,
        [scriptblock][parameter(Position = 1)]$command
    )
    $ErrorActionPreference = "Stop"
    if (!(Invoke-Cmds $name $command)) {
        throw "$name failed"
    }
}

function Publish-Sample {
    param ([string][parameter(Position = 0)]$path)
    Invoke-RequiredCmds "Publish sample '$path'" {
        $msBuildProcess = Start-Process -PassThru -NoNewWindow -FilePath "msbuild.exe" -ArgumentList `
            "$path", `
            "-v:m", `
            "-noLogo", `
            "-p:PublishProfile=$root\.github\uitest\GenericPublish.pubxml", `
            "-p:DeployOnBuild=true", `
            "-p:Configuration=$config", `
            "-p:SourceLinkCreate=true"
        Wait-Process -InputObject $msBuildProcess
        if ($msBuildProcess.ExitCode -ne 0) {
            throw "MSBuild failed with exit code $($msBuildProcess.ExitCode)."
        }
    }
}

function Start-Sample {
    param (
        [string][parameter(Position = 0)]$sampleName,
        [string][parameter(Position = 1)]$path,
        [int][parameter(Position = 2)]$port
    )
    Invoke-RequiredCmds "Start sample '$sampleName'" {
        Remove-IISSite -Confirm:$false -Name $sampleName -ErrorAction SilentlyContinue

        icacls "$root\artifacts\" /grant "IIS_IUSRS:(OI)(CI)F"

        New-IISSite -Name "$sampleName" `
            -PhysicalPath "$path" `
            -BindingInformation "*:${port}:"

        # ensure IIS created the site
        while ($true) {
            $state = (Get-IISSite -Name $sampleName).State
            if ($state -eq "Started") {
                break
            }
            elseif ([string]::IsNullOrEmpty($state)) {
                continue
            }
            else {
                throw "Site '${sampleName}' could not be started. State: '${state}'."
            }
        }
    }
}

function Stop-Sample {
    param ([string][parameter(Position = 0)]$sampleName)
    Invoke-Cmds "Stop sample '$sampleName'" {
        $ErrorActionPreference = "SilentlyContinue"
        Stop-Process -Force -Name chrome -ErrorAction SilentlyContinue
        Stop-Process -Force -Name chromedriver -ErrorAction SilentlyContinue
        Stop-Process -Force -Name firefox -ErrorAction SilentlyContinue
        Stop-Process -Force -Name geckodriver -ErrorAction SilentlyContinue
        Remove-IISSite -Confirm:$false -Name $sampleName -ErrorAction SilentlyContinue
    }
}

function Test-Sample {
    param (
        [string][parameter(Position = 0)]$sampleName,
        [int][parameter(Position = 1)]$port
    )
    Invoke-RequiredCmds "Test the front page of '${sampleName}'" {
        # ensure the site runs and can serve the front page
        while ($true) {
            $request = Invoke-WebRequest "http://localhost:${port}" -ErrorAction SilentlyContinue
            $httpStatus = $request.StatusCode
            if ($httpStatus -eq 200) {
                break
            }
            elseif ($httpStatus -eq 500) {
                throw "Site '${sampleName}' returned 500 Internal Server Error."
            }
        }
    }
}

try {
    # this is needed because of IIS
    Invoke-RequiredCmds "Check if Administrator" {
        $user = [Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()
        if (!($user.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator))) {
            throw "Please run this script as an Administrator."
        }
    }

    Invoke-RequiredCmds "Configure IIS" {
        if ($PSVersionTable.PSVersion.Major -gt 5) {
            Import-Module -Name IISAdministration -UseWindowsPowerShell
        }
        else {
            Import-Module -Name IISAdministration
        }
    }

    Invoke-RequiredCmds "Copy profile" {
        if (-Not(Test-Path -PathType Leaf -Path $profilePath)) {
            throw "Profile '$profilePath' doesn't exist."
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

    Start-Sample $samplesOwinName $samplesOwinPath $samplesOwinPort
    Test-Sample $samplesOwinName $samplesOwinPort
    Start-Sample $samplesApiOwinName $samplesApiOwinPath $samplesApiOwinPort

    Invoke-RequiredCmds "Run UI tests" {
        $uiTestProcess = Start-Process -PassThru -NoNewWindow -FilePath "dotnet.exe" -ArgumentList `
            "test", `
            "$testDir", `
            "--configuration", `
            "$config", `
            "--no-restore", `
            "--logger", `
            "trx;LogFileName=$TrxName", `
            "--results-directory", `
            "$testResultsDir"
        Wait-Process -InputObject $uiTestProcess
        if ($uiTestProcess.ExitCode -ne 0) {
            Write-Host "The test process returned $($uiTestProcess.ExitCode). Ignoring."
        }
    }

}
finally {
    Test-Sample $samplesOwinName $samplesOwinPort
    Stop-Sample $samplesOwinName
    Stop-Sample $samplesApiOwinName
}

