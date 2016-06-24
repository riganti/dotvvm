param([String]$key, [String]$source)

& del dotvvm.*.nupkg -y

& ..\nuget.exe pack dotvvm.nuspec

$file = dir dotvvm.*.nupkg
$file = $file.FullName

& ..\nuget.exe push $file -ApiKey $key -Source $source 