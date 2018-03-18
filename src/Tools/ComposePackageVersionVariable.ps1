param([string]$versionCore, [string]$prereleaseVersion, [string]$buildNumber, [bool]$useBuildNumber, [bool]$isPreviewFinal, [string]$additionalSuffix, [bool]$isMainFinalVersion )


## Final MAIN Version (example: 1.1.4) 
if($isMainFinalVersion){
Write-Host "Adding or updation variable PackageVersion: $versionCore"
Write-Host "##vso[task.setvariable variable=PackageVersion]$versionCore"
exit
}

## sufix
$_prereleaseVersion = $_buildNumber  = $_final  = $_additionalSuffix = "";

if($prereleaseVersion){
    $_prereleaseVersion = "-$prereleaseVersion";
}

if($isPreviewFinal)
{
    $_final = "-final";
}else {

    if($useBuildNumber){
        $_buildNumber = "-$buildNumber";
    }
}

if($isPreviewFinal)
{
    $_final = "-final";
}

if($additionalSuffix){
    $_additionalSuffix = "-$additionalSuffix";
}
$packageVersion = "$versionCore$_prereleaseVersion$_buildNumber$_final$_additionalSuffix"
Write-Host "Adding or updation variable PackageVersion: $packageVersion"
Write-Host "##vso[task.setvariable variable=PackageVersion]$packageVersion"

## versionCore-prereleaseVersion-buildNumber
## 1.0.0-preview-24153515

## versionCore-prereleaseVersion-final
## 1.0.0-preview-24153515


## versionCore-prereleaseVersion
## 1.0.0-preview

## versionCore
## 1.0.0

