param([String]$websiteName, [String]$websitePath, [int16]$websitePort)

Import-Module -Name WebAdministration
$website = Get-Website -Name $websiteName

if (!$website) { 
    Write-Host "Creating website";
    $poolName = $websiteName + "Pool";
    
    ##create directory
    $testDir = (Test-Path C:\www\selenium.utils.tests -PathType Container);
    if($testDir.ToString() -eq "False"){
        mkdir $websitePath;
    }
    try{
        New-WebAppPool -Name $poolName;
    }
    Catch{

    }
    New-Website -Name $websiteName -ApplicationPool $poolName -PhysicalPath $websitePath -Port $websitePort
    

} else {
    Write-Host "Website already exists."

}
