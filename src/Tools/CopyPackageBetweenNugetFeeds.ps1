Param(
    [string]$version,
    [string]$server, 
    [string]$internalServer, 
    [string]$apiKey,
    [string]$packageId
)

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
