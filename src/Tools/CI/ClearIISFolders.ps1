param([string]$rootIisFolder,[string]$folder)
$folders = ("$rootIisFolder\dotvvm.owin.api","$rootIisFolder\dotvvm.core.api", "$rootIisFolder\dotvvm.$folder")

foreach($f in $folders){ if($f){ new-item -path $f -name app_offline.htm -itemtype file -value "Deployment"}}  
foreach($f in $folders){ if($f){ Get-ChildItem -Path $f | Select -ExpandProperty FullName | Where {!$_.endswith('\aspnet_client')} | Remove-Item -Recurse -Force }}
foreach($f in $folders){ if($f){ new-item -path $f -name app_offline.htm -itemtype file -value "Deployment"}}
