param(
    [string][parameter(Mandatory = $true)]$root,
    [string][parameter(Mandatory = $true)]$nuGetOrgApiKey,
    [string]$internalNuGetFeedName = "riganti",
    [string]$include = "*",
    [string]$exclude = "",
    [string]$version = ""
)

if (-not (Test-Path "$root")) {
    throw "The '$root' directory must exist."
}

$root = Resolve-Path "$root"

$packagesDir = Join-Path "$root" "./artifacts/packages"

if (Test-Path "$packagesDir") {
    Remove-Item -Recurse "$packagesDir"
}
New-Item -ItemType Directory -Force "$packagesDir"

$packages = . "$PSScriptRoot/Get-PublicProjects.ps1"

$filteredPackages = $packages
if ("$include" -ne "*") {
    $filteredPackages = $filteredPackages `
        | Where-Object { $_.Name -match "$include" }
}

if (-not ([string]::IsNullOrWhiteSpace($exclude))) {
    $filteredPackages = $filteredPackages `
        | Where-Object { $_.Name -notmatch "$exclude" }
}

if ([string]::IsNullOrWhiteSpace($version)) {
    $latestVersion = nuget list "DotVVM" -NonInteractive -PreRelease -Source "$internalNuGetFeedName" `
        | Select-String -Pattern "^DotVVM ([\d\w\.-]+)$" `
        | Select-Object -First 1
    $version = $latestVersion.Matches.Groups[1].Value
    Write-Host "Version '$version' selected."
}

$packagesConfig = @"
<?xml version="1.0" encoding="utf-8"?>
<packages>
    $($filteredPackages `
        | Foreach-Object { return "<package id=""$($_.Name)"" version=""$version"" />" } `
        | Join-String -Separator "`n")
</packages>
"@

$packagesConfig | Out-File (Join-Path "$packagesDir" "packages.config")

Write-Host "::group::Downloading NuGet packages from internal feed"

$oldCwd = Get-Location
Set-Location "$packagesDir"

try {
    nuget restore `
        -DirectDownload `
        -NonInteractive `
        -NoCache `
        -PackageSaveMode nupkg `
        -PackagesDirectory "$packagesDir" `
        -Source "$internalNuGetFeedName"
} finally {
    Set-Location "$oldCwd"
    Write-Host "::endgroup::"
}

Write-Host "::group::Pushing packages to NuGet.org"
try {
    foreach ($package in (Get-ChildItem "$packagesDir/**/*.nupkg")) {
        nuget push "$($package.FullName)" `
            -Source "nuget.org" `
            -ApiKey "$nuGetOrgApiKey" `
            -NonInteractive
    }
} finally {
    Write-Host "::endgroup::"
}
