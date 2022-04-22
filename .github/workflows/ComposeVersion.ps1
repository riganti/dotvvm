param(
    [Parameter(Mandatory)][string]$major,
    [Parameter(Mandatory)][string]$minor,
    [Parameter(Mandatory)][string]$patch,
    [string]$preview,
    [bool]$isFinalPreview = $false,
    [Parameter(Mandatory)][string]$buildNumber,
    [Parameter(Mandatory)][bool]$useBuildNumber,
    [string]$additionalSuffix)

if ($isFinalPreview -and -not ($preview)) {
    throw "The preview version must be set if a release is a final preview!"
}

$major = [int]::Parse($major)
$minor = [int]::Parse($minor)
$patch = [int]::Parse($patch)
$preview = [int]::Parse($preview)
$preview = ([string]$preview).PadLeft(2, '0');

$versionCore = "$major.$minor.$patch"
$_previewSuffix = $_buildNumberSuffix = $_finalSuffix = $_additionalSuffix = "";

if ($preview) {
    $_previewSuffix = "-preview$preview";
}

if ($useBuildNumber) {
    $_buildNumberSuffix = "-$buildNumber";
}

if ($isFinalPreview) {
    $_finalSuffix = "-final";
}

if ($additionalSuffix) {
    $_additionalSuffix = "-$additionalSuffix";
}
$packageVersion = "$versionCore$_previewSuffix$_buildNumberSuffix$_finalSuffix$_additionalSuffix"
return $packageVersion

## versionCore-prereleaseVersion-buildNumber
## 1.0.0-preview-24153515

## versionCore-prereleaseVersion-final
## 1.0.0-preview-24153515-final

## versionCore-prereleaseVersion
## 1.0.0-preview

## versionCore
## 1.0.0
