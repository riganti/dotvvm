Param(
    [string]$version,
    [string]$server, 
    [string]$internalServer,    
    [string]$internalSnupkgServer,
    [string]$apiKey
)


### Configuration
$packages = @(
    [pscustomobject]@{ Package = "DotVVM.Core"; Directory = "Framework/Core"; Type = "standard" },
    [pscustomobject]@{ Package = "DotVVM"; Directory = "Framework/Framework" ; Type = "standard" },
    [pscustomobject]@{ Package = "DotVVM.Owin"; Directory = "Framework/Hosting.Owin"; Type = "standard" },
    [pscustomobject]@{ Package = "DotVVM.AspNetCore"; Directory = "Framework/Hosting.AspNetCore" ; Type = "standard" },
    [pscustomobject]@{ Package = "DotVVM.Testing"; Directory = "Framework/Testing" ; Type = "standard" },
    [pscustomobject]@{ Package = "DotVVM.CommandLine"; Directory = "Tools/CommandLine"; Type = "tool" },
    [pscustomobject]@{ Package = "DotVVM.Templates"; Directory = "Templates" ; Type = "template" },
    [pscustomobject]@{ Package = "DotVVM.Api.Swashbuckle.AspNetCore"; Directory = "Api/Swashbuckle.AspNetCore"; Type = "standard" },
    [pscustomobject]@{ Package = "DotVVM.Api.Swashbuckle.Owin"; Directory = "Api/Swashbuckle.Owin"; Type = "standard" },
    [pscustomobject]@{ Package = "DotVVM.HotReload"; Directory = "Tools/HotReload/Common"; Type = "standard" },
    [pscustomobject]@{ Package = "DotVVM.HotReload.AspNetCore"; Directory = "Tools/HotReload/AspNetCore"; Type = "standard" },
    [pscustomobject]@{ Package = "DotVVM.HotReload.Owin"; Directory = "Tools/HotReload/Owin"; Type = "standard" },
    [pscustomobject]@{ Package = "DotVVM.DynamicData"; Directory = "DynamicData/DynamicData"; Type = "standard" },
    [pscustomobject]@{ Package = "DotVVM.DynamicData.Annotations"; Directory = "DynamicData/Annotations"; Type = "standard" },
    [pscustomobject]@{ Package = "DotVVM.Tracing.ApplicationInsights"; Directory = "Tracing/ApplicationInsights"; Type = "standard" },
    [pscustomobject]@{ Package = "DotVVM.Tracing.ApplicationInsights.AspNetCore"; Directory = "Tracing/ApplicationInsights.AspNetCore"; Type = "standard" },
    [pscustomobject]@{ Package = "DotVVM.Tracing.ApplicationInsights.Owin"; Directory = "Tracing/ApplicationInsights.Owin"; Type = "standard" }
    [pscustomobject]@{ Package = "DotVVM.Tracing.MiniProfiler.AspNetCore"; Directory = "Tracing/MiniProfiler.AspNetCore"; Type = "standard" },
    [pscustomobject]@{ Package = "DotVVM.Tracing.MiniProfiler.Owin"; Directory = "Tracing/MiniProfiler.Owin"; Type = "standard" }
)

Write-Host "Current directory: $PWD"

$webClient = New-Object System.Net.WebClient
## Standard packages
foreach ($package in $packages) {

    $packageId = $package.Package    

    Write-Host "Downloading $packageId ($version)"

    # standard package
    if ($package.Type -eq "standard") {
        & ../ci/scripts/nuget.exe install $packageId -OutputDirectory .\tools\packages -version $version -DirectDownload -NoCache -DependencyVersion Ignore -source $internalServer | Out-Host
        $nupkgFile = dir -s ./tools/packages/$packageId.$version.nupkg | Select -First 1
        Write-Host "Downloaded package located on '$nupkgFile'"
    }
    # standard package
    if ($package.Type -eq "tool") {
        ## dotnet tools
        dotnet tool install $packageId --tool-path ./tools/packages --version $version --add-source $internalServer | Out-Host
        $nupkgFile = dir -s ./tools/packages/*/$packageId.$version.nupkg | Select -First 1
        Write-Host "Downloaded tool located on '$nupkgFile'"
    }
    # dotnet templates
    if ($package.Type -eq "template") {
        dotnet new --install "$packageId::$version" --force --nuget-source $internalServer | Out-Host
        $nupkgFile = dir -s $env:USERPROFILE/.templateengine/dotnetcli -filter $packageId.$version.nupkg | Select -First 1
        Write-Host "Downloaded template located on '$nupkgFile'"
    }
    
    if ($nupkgFile) {
        # upload 
        Write-Host "Uploading package..."
        & ../ci/scripts/nuget.exe push $nupkgFile -source $server -apiKey $apiKey | Out-Host
        Write-Host "Package uploaded to $server."
    }
    if (Test-Path -Path ./tools/packages) {
        Remove-Item -Recurse -Force ./tools/packages
    }

    # snupkg management
    
        $snupkgUrl = "file://$internalSnupkgServer/snupkg/"+$packageId + "." + $version + ".snupkg"
    $snupkgFile = Join-Path $PSScriptRoot ($packageId + "." + $version + ".snupkg")

    try {
      $webClient.DownloadFile($snupkgUrl, $snupkgFile)
      $snupkgDownloaded = $true;
    } catch {
      Write-Host "No snupkg package found!"
      $snupkgDownloaded = $false;
    }        
    
    if ($snupkgDownloaded -eq $true){
        Write-Host "Uploading snupkg package..."        
        & ../ci/scripts/nuget.exe push $snupkgFile -source $server -apiKey $apiKey | Out-Host
		Write-Host "Uploaded snupkg package." 
        try {
			Remove-Item $snupkgFile
		} catch {            
            Write-Host "Unable to cleanup snupkg..."
        }
    }
}
