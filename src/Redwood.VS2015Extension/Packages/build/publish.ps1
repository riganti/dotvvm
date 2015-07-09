$file = dir ../dotvvm.*.nupkg
$file = $file.FullName

& ..\..\..\Tools\nuget.exe push $file 8cf42ba4-b31b-40c0-b456-432886121659