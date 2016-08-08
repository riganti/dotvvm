param([String]$key, [String]$server = "")

del ./DotVVM.DynamicData.*.nupkg

& ..\Tools\nuget.exe pack "./DotVVM.DynamicData.nuspec"

$file = dir ./DotVVM.DynamicData.*.nupkg
$file = $file.Name

if ($server -eq "") {
	& ..\Tools\nuget.exe push $file $key
}
else {
	& ..\Tools\nuget.exe push $file $key -Source $server
}