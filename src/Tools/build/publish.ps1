param([String]$key)

# build DotVVM.Core
remove-item dotvvm.*.nupkg -Force
& ..\nuget.exe pack dotvvm.core.nuspec

# publish DotVVM.Core
$file = dir dotvvm.*.nupkg
$file = $file.FileName
& ..\nuget.exe push $file -ApiKey $key

# build DotVVM
remove-item dotvvm.*.nupkg -Force
& ..\nuget.exe pack dotvvm.nuspec

# publish DotVVM
$file = dir dotvvm.*.nupkg
$file = $file.FileName
& ..\nuget.exe push $file -ApiKey $key