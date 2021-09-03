Param(
    [string]$version,
    [string]$server, 
    [string]$internalServer, 
    [string]$apiKey
)


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
	
    # get package
	& .\tools\nuget.exe install $packageId -OutputDirectory .\tools\packages -version $version -DirectDownload -NoCache -DependencyVersion Ignore -source $internalServer
	$nupkgFile = dir -s ./tools/packages/$packageId.$version.nupkg | Select -First 1
	Write-Host "Downloaded package located on '$nupkgFile'"
		
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