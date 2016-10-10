param([String]$key)

# build DotVVM.Core
remove-item dotvvm.*.nupkg -Force
& ..\nuget.exe pack dotvvm.core.nuspec

# publish DotVVM.Core
$file = dir dotvvm.*.nupkg
$file = $file.FullName
& ..\nuget.exe push $file -ApiKey $key



# build DotVVM
remove-item dotvvm.*.nupkg -Force
& ..\nuget.exe pack dotvvm.nuspec

# publish DotVVM
$file = dir dotvvm.*.nupkg
$file = $file.FullName
& ..\nuget.exe push $file -ApiKey $key




# build DotVVM.AspNetCore
remove-item dotvvm.*.nupkg -Force
& ..\nuget.exe pack dotvvm.AspNetCore.nuspec

# publish DotVVM.AspNetCore
$file = dir dotvvm.*.nupkg
$file = $file.FullName
& ..\nuget.exe push $file -ApiKey $key 



# build DotVVM.Owin
remove-item dotvvm.*.nupkg -Force
& ..\nuget.exe pack dotvvm.Owin.nuspec

# publish DotVVM.Owin
$file = dir dotvvm.*.nupkg
$file = $file.FullName
& ..\nuget.exe push $file -ApiKey $key 