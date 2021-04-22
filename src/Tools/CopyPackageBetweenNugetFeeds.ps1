Param(
    [string]$version,
    [string]$server, 
    [string]$internalServer,
    [string]$internalSnupkgServer,
    [string]$apiKey,
    [string]$packageId
)

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

    # snupkg management
    $snupkgUrl = "file://$internalSnupkgServer/snupkg/"
    $snupkgFile = Join-Path $PSScriptRoot ($packageId + "." + $version + ".snupkg")

    try {
      $webClient.DownloadFile($snupkgUrl, $snupkgFile)
      $snupkgDownloaded = $true;
    }catch {
      Write-Host "No snupkg package found!"
      $snupkgDownloaded = $false;
   }        
    
    if ($snupkgDownloaded -eq $true){
        Write-Host "Uploading snupkg package..."        
        & .\Tools\nuget.exe push $snupkgFile -source $server -apiKey $apiKey
        Remove-Item $nupkgFile    
        try {Remove-Item $snupkgFile}catch {            
            Write-Host "Unable to cleanup snupkg..."
        }
    }