param($installPath, $toolsPath, $package, $project)

#generate dotvvm.json with security keys if it does not exists
$fileName = (Join-Path $project.Properties.Item("FullPath").Value "dotvvm.json")
if (-not [System.IO.File]::Exists($fileName)) {
  Generate-DotvvmSecurityKeys >$fileName
  $project.ProjectItems.AddFromFile($fileName)
}