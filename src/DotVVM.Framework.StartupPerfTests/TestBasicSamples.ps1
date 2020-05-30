# Prepare temp content directory
$temp = [System.IO.Path]::GetTempPath()
if (Test-Path $temp/DotVVM.Samples.Common) {
  rmdir $temp/DotVVM.Samples.Common -Recurse -Force
}
mkdir $temp/DotVVM.Samples.Common | out-null
copy ../DotVVM.Samples.Common/Content -Destination $temp/DotVVM.Samples.Common -Recurse -Force
copy ../DotVVM.Samples.Common/Scripts -Destination $temp/DotVVM.Samples.Common -Recurse -Force
copy ../DotVVM.Samples.Common/Views -Destination $temp/DotVVM.Samples.Common -Recurse -Force
copy ../DotVVM.Samples.Common/sampleConfig.json -Destination $temp/DotVVM.Samples.Common -Force

# Run OWIN tests
./bin/Debug/netcoreapp3.0/DotVVM.Framework.StartupPerfTests.exe ../DotVVM.Samples.BasicSamples.Owin/DotVVM.Samples.BasicSamples.Owin.csproj -t owin -v -r 5

# Run ASP.NET Core tests
./bin/Debug/netcoreapp3.0/DotVVM.Framework.StartupPerfTests.exe ../DotVVM.Samples.BasicSamples.AspNetCoreLatest/DotVVM.Samples.BasicSamples.AspNetCoreLatest.csproj -t aspnetcore -v -r 5