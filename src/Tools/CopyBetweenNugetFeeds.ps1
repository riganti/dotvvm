Param(
    [string]$version,
    [string]$server, 
    [string]$internalServer,    
    [string]$internalSnupkgServer,
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

$webClient = New-Object System.Net.WebClient
## Standard packages
foreach ($package in $packages) {

    $packageId = $package.Package    

    Write-Host "Downloading $packageId ($version)"

    # standard package
    if ($package.Type -eq "standard") {
        & .\tools\nuget.exe install $packageId -OutputDirectory .\tools\packages -version $version -DirectDownload -NoCache -DependencyVersion Ignore -source $internalServer
        $nupkgFile = dir -s ./tools/packages/$packageId.$version.nupkg | Select -First 1
        Write-Host "Downloaded package located on '$nupkgFile'"
    }
    # standard package
    if ($package.Type -eq "tool") {
        ## dotnet tools
        dotnet tool install $packageId --tool-path ./tools/packages --version $version
        $nupkgFile = dir -s ./tools/packages/*/$packageId.$version.nupkg | Select -First 1
        Write-Host "Downloaded tool located on '$nupkgFile'"
    }
    # dotnet templates
    if ($package.Type -eq "template") {
        dotnet new --install "$packageId::$version" --force --nuget-source $internalServer
        $nupkgFile = dir $env:USERPROFILE\.templateengine\dotnetcli\ -s | where { $_.Name -eq "$packageId.$version.nupkg" } | select { $_.FullName } | Select -First 1
        Write-Host "Downloaded template located on '$nupkgFile'"
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
}
