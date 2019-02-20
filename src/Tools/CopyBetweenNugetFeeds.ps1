Param(
    [string]$version,
    [string]$server, 
    [string]$internalServer, 
    [string]$apiKey
)


### Configuration

### Configuration

$packages = @(
	[pscustomobject]@{ Package = "DotVVM.Tracing.ApplicationInsights"; Directory = "DotVVM.Tracing.ApplicationInsights" }
	[pscustomobject]@{ Package = "DotVVM.Tracing.ApplicationInsights.AspNetCore"; Directory = "DotVVM.Tracing.ApplicationInsights.AspNetCore" },
	[pscustomobject]@{ Package = "DotVVM.Tracing.ApplicationInsights.Owin"; Directory = "DotVVM.Tracing.ApplicationInsights.Owin" },
	[pscustomobject]@{ Package = "DotVVM.Tracing.MiniProfiler.AspNetCore"; Directory = "DotVVM.Tracing.MiniProfiler.AspNetCore" },
	[pscustomobject]@{ Package = "DotVVM.Tracing.MiniProfiler.Owin"; Directory = "DotVVM.Tracing.MiniProfiler.Owin" }
)


foreach($package in $packages){

    $packageId = $package.Package
    $webClient = New-Object System.Net.WebClient
    $url = "$internalServer/package/" + $packageId + "/" + $version
    $nupkgFile = Join-Path $PSScriptRoot ($packageId + "." + $version + ".nupkg")

    Write-Host "Downloading from $url"
    $webClient.DownloadFile($url, $nupkgFile)
    Write-Host "Package downloaded from '$internalServer'."

    Write-Host "Uploading package..."
    & .\Tools\nuget.exe push $nupkgFile -source $server -apiKey $apiKey
    Write-Host "Package uploaded to $server."

    Remove-Item $nupkgFile
}
