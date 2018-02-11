param([string]$versionCore, [string]$prereleaseVersion, [string]$buildNumber, [bool]$useBuildNumber, [bool]$isFinal, [string]$additionalSuffix )



## sufix
$_sufix = $_buildNumber  = $_final  = $_additionalSuffix = "";

if($prereleaseVersion){
    $_prereleaseVersion = "-$prereleaseVersion";
}

if($isFinal)
{
    $_final = "-final";
}else {

    if($useBuildNumber){
        $_buildNumber = "-$buildNumber";
    }
}

if($isFinal)
{
    $_final = "-final";
}

if($additionalSuffix){
    $_additionalSuffix = "-$additionalSuffix";
}
$packageVersion = "$versionCore$_prereleaseVersion$_buildNumber$_final$_additionalSuffix"
Write-Host "Adding or updation variable PackageVersion: $packageVersion"
Write-Host "##vso[task.setvariable variable=PackageVersion]$packageVersion"

## versionCore-suffix-buildNumber
## 1.0.0-preview-24153515

## versionCore-suffix-final
## 1.0.0-preview-24153515


## versionCore-suffix
## 1.0.0-preview

## versionCore
## 1.0.0

