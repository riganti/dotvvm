Param(
    [string]$version,
    [string]$server, 
    [string]$internalServer,
    [string]$internalSnupkgServer,
    [string]$apiKey,
    [string]$packageId,
)

    $packageId = $package.Package
    $webClient = New-Object System.Net.WebClient
    $url = "$internalServer/package/" + $packageId + "/" + $version
    $snupkgUrl = "$internalServer/package/" + $packageId + "/" + $version
    $nupkgFile = Join-Path $PSScriptRoot ($packageId + "." + $version + ".nupkg")
    $snupkgurl = "$internalSnupkgServer/snupkg/" + $nupkgFile

    $snupkgFile = Join-Path $PSScriptRoot ($packageId + "." + $version + ".snupkg")

    Write-Host "Downloading from $url"
    $webClient.DownloadFile($url, $nupkgFile)
    try {
        $webClient.DownloadFile($snupkgUrl, $snupkgFile)
    }catch { Write-Host "No snupkg package found!"}
    Write-Host "Package downloaded from '$internalServer'."

    Write-Host "Uploading package..."
    & .\Tools\nuget.exe push $nupkgFile -source $server -apiKey $apiKey
    & .\Tools\nuget.exe push $snupkgFile -source $server -apiKey $apiKey
    Write-Host "Package uploaded to $server."

    Remove-Item $nupkgFile    
    try {Remove-Item $snupkgFile}catch{ Write-Host "Unable to delete snupkg!"}
