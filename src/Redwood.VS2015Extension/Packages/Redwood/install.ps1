param($installPath, $toolsPath, $package, $project)

#generate redwood.json with security keys if it does not exists
$fileName = (Join-Path $project.Properties.Item("FullPath").Value "redwood.json")
if (-not [System.IO.File]::Exists($fileName)) {
  Generate-RedwoodSecurityKeys >$fileName
  $project.ProjectItems.AddFromFile($fileName)
}