param([String]$key)

& del dotvvm.*.nupkg -y

& ..\nuget.exe pack dotvvm.nuspec

$file = dir dotvvm.*.nupkg
$file = $file.FullName

& ..\nuget.exe push $file -ApiKey $key