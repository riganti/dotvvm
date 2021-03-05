Param(
    [string]$version,
    [string]$server, 
    [string]$internalServer, 
    [string]$apiKey
)


### Configuration

$packages = @(
	[pscustomobject]@{ Package = "DotVVM.Core"; Directory = "DotVVM.Core" },
	[pscustomobject]@{ Package = "DotVVM"; Directory = "DotVVM.Framework" },
	[pscustomobject]@{ Package = "DotVVM.Owin"; Directory = "DotVVM.Framework.Hosting.Owin" },
	[pscustomobject]@{ Package = "DotVVM.AspNetCore"; Directory = "DotVVM.Framework.Hosting.AspNetCore" },
	[pscustomobject]@{ Package = "DotVVM.CommandLine"; Directory = "DotVVM.CommandLine" },
	[pscustomobject]@{ Package = "DotVVM.Compiler.Light"; Directory = "DotVVM.Compiler.Light" },
	[pscustomobject]@{ Package = "DotVVM.Templates"; Directory = "Templates" },
    [pscustomobject]@{ Package = "DotVVM.Api.Swashbuckle.AspNetCore"; Directory = "DotVVM.Framework.Api.Swashbuckle.AspNetCore" },
	[pscustomobject]@{ Package = "DotVVM.Api.Swashbuckle.Owin"; Directory = "DotVVM.Framework.Api.Swashbuckle.Owin" }
)

foreach($package in $packages){

    $packageId = $package.Package
    $webClient = New-Object System.Net.WebClient
    $url = "$internalServer/package/" + $packageId + "/" + $version
    $nupkgFile = Join-Path $PSScriptRoot ($packageId + "." + $version + ".nupkg")
    $snupkgFile = Join-Path $PSScriptRoot ($packageId + "." + $version + ".snupkg")

    Write-Host "Downloading from $url"
    $webClient.DownloadFile($url, $nupkgFile)
    try {
        $webClient.DownloadFile($url, $snupkgFile)
    }catch {echo "No snupkg package found!"}
    Write-Host "Package downloaded from '$internalServer'."

    Write-Host "Uploading package..."
    & .\Tools\nuget.exe push $nupkgFile -source $server -apiKey $apiKey
    & .\Tools\nuget.exe push $snupkgFile -source $server -apiKey $apiKey
    Write-Host "Package uploaded to $server."

    Remove-Item $nupkgFile    
    try {Remove-Item $snupkgFile}catch {}
}
