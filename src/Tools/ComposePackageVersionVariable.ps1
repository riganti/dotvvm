param([string]$versionCore, [string]$suffix, [string]$buildNumber, [bool]$useBuildNumber, [bool]$isFinal, [string]$postSuffix )



## sufix
$_sufix = $_buildNumber  = $_final  = $_postSuffix = "";

if($suffix){
    $_sufix = "-$suffix";
}

if($useBuildNumber){
    $_buildNumber = "-$buildNumber";
}

if($isFinal)
{
    $_final = "-final";
}

if($postSuffix){
    $_postSuffix = "-$postSuffix";
}
$packageVersion = "$versionCore$_sufix$_buildNumber$_final$_postSuffix"
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

