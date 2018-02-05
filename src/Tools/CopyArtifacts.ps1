param([String]$searchFolder, [String]$artifactsFolder)


$files  = Get-ChildItem   "*.pdb"  -Recurse | where { !$_.Directory.FullName -notlike "\obj\" } | where { $_.Name -like "*dotvvm*" }   |   where { $_.Directory.FullName -notlike "\packages\" }   |  where { $_.Name -notlike "*Riganti.selenium*" }
$files2  = Get-ChildItem   "*.dll"  -Recurse | where { $_.Directory.FullName -notlike "\obj\" }  | where { $_.Name -like "*dotvvm*" }   |   where { $_.Directory.FullName -notlike "\packages\" }   |  where { $_.Name -notlike "*Riganti.selenium*" }
$files3  = Get-ChildItem   "*.nupkg"  -Recurse | where { $_.Directory.FullName -notlike "\obj\" }  | where { $_.Name -like "*dotvvm*" }   |   where { $_.Directory.FullName -notlike "\packages\" }   |  where { $_.Name -notlike "*Riganti.selenium*" }


foreach($file in $files){

 Copy-Item $file -Destination $artifactsFolder

}

foreach($file in $files2){

 Copy-Item $file -Destination $artifactsFolder

}
foreach($file in $files3){

 Copy-Item $file -Destination $artifactsFolder

}