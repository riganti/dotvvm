$key = Read-Host "Enter NuGet key"
$file = dir ../dotvvm.*.nupkg
$file = $file.FullName

& ..\..\..\Tools\nuget.exe push $file $key