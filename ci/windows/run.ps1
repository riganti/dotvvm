$root = $env:DOTVVM_ROOT
if ($null -eq $root) {
    $root = "$PWD"
}
$env:DOTVVM_ROOT = $root

$configuration = $env:CONFIGURATION
if ($null -eq $configuration) {
    $configuration = "Release"
}

Write-Host "ROOT=$ROOT"
Write-Host "CONFIGURATION=$CONFIGURATION"

Set-Location $root\src\DotVVM.Framework
npm ci --cache $root\.npm --prefer-offline
npm run build
if ($LASTEXITCODE -ne 0) {
    Write-Host "npm build failed"
    exit 1
}

$sln = "$root\ci\windows\Windows.sln"
$nuget = "$root\src\packages\"
Set-Location $root
nuget restore $sln -PackagesDirectory $nuget
dotnet restore $sln --packages $nuget
if ($LASTEXITCODE -ne 0) {
    Write-Host "nuget restore failed"
    exit 1
}

msbuild $sln -v:m `
    -p:PublishProfile=$root\ci\windows\GenericPublish.pubxml `
    -p:DeployOnBuild=true `
    -p:Configuration=$configuration
if ($LASTEXITCODE -ne 0) {
    Write-Host "dotnet build failed"
    exit 1
}

Import-Module IISAdministration -UseWindowsPowerShell

icacls $root/artifacts/ /grant "IIS_IUSRS:(OI)(CI)F"
New-IISSite -Name dotvvm.owin `
    -PhysicalPath $root\artifacts\DotVVM.Samples.BasicSamples.Owin `
    -BindingInformation "*:5407:"

New-IISSite -Name dotvvm.owin.api `
    -PhysicalPath $root\artifacts\DotVVM.Samples.BasicSamples.Api.Owin `
    -BindingInformation "*:5002:"

Copy-Item -Recurse `
    $root\src\DotVVM.Samples.BasicSamples.Owin `
    $root\artifacts

Copy-Item `
    $root\src\DotVVM.Samples.Tests\Profiles\seleniumconfig.owin.chrome.json `
    $root\src\DotVVM.Samples.Tests\seleniumconfig.json

dotnet test $root\src\DotVVM.Samples.Tests `
    --configuration $configuration `
    --logger trx `
    --results-directory $root\artifacts\test

Stop-Process -Name chrome
Stop-Process -Name chromedriver

Remove-IISSite -Force -Name dotvvm.owin
Remove-IISSite -Force -Name dotvvm.owin.api
