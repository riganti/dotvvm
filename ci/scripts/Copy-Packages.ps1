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

if (-not (Test-Path -PathType Container "$packagesDir")) {
    New-Item -ItemType Directory -Force "$packagesDir"
}

$packages = . "$PSScriptRoot/Get-PublicProjects.ps1"

if ([string]::IsNullOrWhiteSpace($version)) {
    $latestVersion = nuget list "DotVVM" -NonInteractive -PreRelease -Source riganti `
        | Select-String -Pattern "^DotVVM ([\d\w\.-]+)$" `
        | Select-Object -First 1
    $version = $latestVersion.Matches.Groups[1].Value
    Write-Host "Version '$version' selected."
}

Write-Host "::group::Downloading NuGet packages from internal feed"
try {
    $filteredPackages = $packages
    if ("$include" -ne "*") {
        $filteredPackages = $filteredPackages `
            | Where-Object { $_.Name -match "$include" }
    }

    if (-not ([string]::IsNullOrWhiteSpace($exclude))) {
        $filteredPackages = $filteredPackages `
            | Where-Object { $_.Name -notmatch "$exclude" }
    }

    foreach ($package in $filteredPackages) {
        if ($package.Type -eq "tool") {
            dotnet tool install "$($package.Name)" `
                --tool-path "$packagesDir" `
                --version "$version" `
                --add-source "$internalNuGetFeedName"
        } elseif ($package.Type -eq "template") {
            dotnet new install "$($package.Name)::$version" `
                --force `
                --nuget-source "$internalNuGetFeedName"
            New-Item -ItemType Directory "$packagesDir/$($package.Name)"
            Copy-Item "$env:USERPROFILE/.templateengine/dotnetcli/$($package.Name).$version.nupkg" "$packagesDir/$($package.Name)"
        } else {
            nuget install "$($package.Name)" `
                -DirectDownload `
                -NonInteractive `
                -DependencyVersion Ignore `
                -NoCache `
                -PackageSaveMode nupkg `
                -OutputDirectory "$packagesDir" `
                -Version "$version" `
                -Source "$internalNuGetFeedName"
        }
    }
} finally {
    Write-Host "::endgroup::"
}

Write-Host "::group::Pushing packages to NuGet.org"
try {
    foreach ($package in (Get-ChildItem "$packagesDir")) {
        nuget push "$($package.FullName)" `
            -Source "nuget.org" `
            -ApiKey "$nuGetOrgApiKey" `
            -NonInteractive
    }
} finally {
    Write-Host "::endgroup::"
}
