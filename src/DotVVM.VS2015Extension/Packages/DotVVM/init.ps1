param($installPath, $toolsPath, $package)
 
foreach ($_ in Get-Module | ?{$_.Name -eq 'DotvvmModule'})
{
    Remove-Module 'DotvvmModule'
}
 
Import-Module (Join-Path $toolsPath DotvvmModule.psm1)