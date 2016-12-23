param([String]$key, [String]$server = "")

del ./DotVVM.DynamicData.*.nupkg

& .\nuget.exe pack "./DotVVM.DynamicData.nuspec"

$file = dir ./DotVVM.DynamicData.*.nupkg
$file = $file.Name

if ($server -eq "") {
	& .\nuget.exe push $file $key
}
else {
	& .\nuget.exe push $file $key -Source $server
}