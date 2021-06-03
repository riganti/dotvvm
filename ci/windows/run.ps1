$root = $env:DOTVVM_ROOT ?? "$PWD"
$env:DOTVVM_ROOT = $root
$configuration = $env:CONFIGURATION ?? "Release"
Write-Host "ROOT=$ROOT"
Write-Host "CONFIGURATION=$CONFIGURATION"

cd $root\src\DotVVM.Framework `
    && npm ci --cache $root\.npm --prefer-offline `
    && npm run build

if (-Not($?)) {
    Write-Host "npm build failed"
    exit 1
}

$sln = "$root\ci\windows\Windows.sln"
$nuget = "$root\src\packages\"
cd $root `
    && nuget restore $sln -PackagesDirectory $nuget `
    && dotnet restore $sln --packages $nuget `

if (-Not($?)) {
    Write-Host "nuget restore failed"
    exit 1
}

msbuild $sln -v:m `
    -p:PublishProfile=$root\ci\windows\GenericPublish.pubxml `
    -p:DeployOnBuild=true `
    -p:Configuration=$configuration

if (-Not($?)) {
    Write-Host "dotnet build failed"
    exit 1
}

# Import-Module IISAdministration -UseWindowsPowerShell

# icacls $root/artifacts/ /grant "IIS_IUSRS:(OI)(CI)F"
New-IISSite -Name dotvvm.owin `
    -PhysicalPath $root\artifacts\DotVVM.Samples.BasicSamples.Owin `
    -BindingInformation "localhost:5407"

# New-IISSite -Name dotvvm.owin.api `
#     -PhysicalPath $root\artifacts\DotVVM.Samples.BasicSamples.Api.Owin `
#     -BindingInformation "localhost:5002"

# Copy-Item `
#     $root\src\DotVVM.Samples.Tests\Profiles\seleniumconfig.owin.chrome.json `
#     $root\src\DotVVM.Samples.Tests\seleniumconfig.json

# dotnet test $root\src\DotVVM.Samples.Tests `
#     --configuration $configuration `
#     --logger trx `
#     --results-directory $root\artifacts\test; `
#     icm { Stop-Process -Name chrome; Stop-Process -Name chromedriver }

# Remove-IISSite -Name dotvvm.owin
# Remove-IISSite -Name dotvvm.owin.api
