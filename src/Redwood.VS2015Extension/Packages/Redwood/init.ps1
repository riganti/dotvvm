param($installPath, $toolsPath, $package)
 
foreach ($_ in Get-Module | ?{$_.Name -eq 'RedwoodModule'})
{
    Remove-Module 'RedwoodModule'
}
 
Import-Module (Join-Path $toolsPath RedwoodModule.psm1)