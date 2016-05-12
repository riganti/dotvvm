param([String]$key)

$file = dir ../dotvvm.*.nupkg
$file = $file.FullName

& ..\..\..\Tools\nuget.exe push $file -ApiKey $key