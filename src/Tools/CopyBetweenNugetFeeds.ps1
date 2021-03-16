Param(
    [string]$version,
    [string]$server, 
    [string]$internalServer, 
    [string]$apiKey
)


### Configuration
$packages = @(
    [pscustomobject]@{ Package = "DotVVM.Core"; Directory = "DotVVM.Core"; Type = "standard" },
    [pscustomobject]@{ Package = "DotVVM"; Directory = "DotVVM.Framework" ; Type = "standard" },
    [pscustomobject]@{ Package = "DotVVM.Owin"; Directory = "DotVVM.Framework.Hosting.Owin"; Type = "standard" },
    [pscustomobject]@{ Package = "DotVVM.AspNetCore"; Directory = "DotVVM.Framework.Hosting.AspNetCore" ; Type = "standard" },
    [pscustomobject]@{ Package = "DotVVM.CommandLine"; Directory = "DotVVM.CommandLine"; Type = "tool" },
    [pscustomobject]@{ Package = "DotVVM.Templates"; Directory = "Templates" ; Type = "template" },
    [pscustomobject]@{ Package = "DotVVM.Api.Swashbuckle.AspNetCore"; Directory = "DotVVM.Framework.Api.Swashbuckle.AspNetCore"; Type = "standard" },
    [pscustomobject]@{ Package = "DotVVM.Api.Swashbuckle.Owin"; Directory = "DotVVM.Framework.Api.Swashbuckle.Owin"; Type = "standard" }
)


## Standard packages
foreach ($package in $packages) {

    $packageId = $package.Package
    Write-Host "Downloading $packageId ($version)"

    # standard package
    if ($package.Type -eq "standard") {
        & .\tools\nuget.exe install $packageId -OutputDirectory .\tools\packages -version $version -DirectDownload -NoCache -DependencyVersion Ignore -source $internalServer
        $nupkgFile = dir -s ./tools/packages/*.nupkg | Select -First 1
        Write-Host "Downloaded package located on '$nupkgFile'"
    }
    # standard package
    if ($package.Type -eq "tool") {
        ## dotnet tools
        dotnet tool install DotVVM.CommandLine --tool-path ./tools/packages --version 3.0.0-preview03-final
        $nupkgFile = dir -s ./tools/packages/*.nupkg | Select -First 1
        Write-Host "Downloaded package located on '$nupkgFile'"
    }
    # dotnet templates
    if ($package.Type -eq "template") {
        dotnet new --install "$packageId::$version" --force --nuget-source $internalServer
        $nupkgFile = dir $env:USERPROFILE\.templateengine\dotnetcli\ -s | where { $_.Name -eq "$packageId.$version.nupkg" } | Select -First 1
        Write-Host "Downloaded package located on '$nupkgFile'"
    }
    if ($nupkgFile) {
        # upload 
        Write-Host "Uploading package..."
        & .\tools\nuget.exe push $nupkgFile -source $server -apiKey $apiKey
        Write-Host "Package uploaded to $server."
    }

    if ( Test-Path -Path ./tools/packages ) {
        Remove-Item -Recurse -Force ./tools/packages
    }
}