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

function Get-Number {
    param(
        [Parameter(Position = 0)]
        [string]$arg,

        [Parameter(Position = 1)]
        [string]$desc = "required"
    )
    $argInt = 0
    if (-not([int]::TryParse($arg, [ref]$argInt))) {
        throw "Failed to parse '$arg' as a $desc version."
    }
    return [string]$argInt
}

$major = Get-Number $major "major"
$minor = Get-Number $minor "minor"
$patch = Get-Number $patch "patch"

$versionCore = "$major.$minor.$patch"
$_previewSuffix = $_buildNumberSuffix = $_finalSuffix = $_additionalSuffix = "";

if ($preview) {
    $preview = ([string](Get-Number $preview "preview")).PadLeft(2, '0')
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
