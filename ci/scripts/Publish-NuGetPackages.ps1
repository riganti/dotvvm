param(
    [string][parameter(Mandatory = $true)]$root,
    [string][parameter(Mandatory = $true)]$version,
    [string][parameter(Mandatory = $true)]$internalFeed,
    [string][parameter(Mandatory = $true)]$internalFeedUser,
    [string][parameter(Mandatory = $true)]$internalFeedPat,
    [string]$signatureType = "DotNetFoundation",
    [string]$dnfUser,
    [string]$dnfSecret,
    [string]$rigantiUrl,
    [string]$rigantiClientId,
    [string]$rigantiTenantId,
    [string]$rigantiSecret,
    [string]$rigantiCertificate
)

$root = Resolve-Path "$root"

if ("$signatureType" -eq "DotNetFoundation") {
    if (([string]::IsNullOrEmpty($dnfUser) -or [string]::IsNullOrEmpty($dnfSecret))) {
        throw "-dnfUser and -dnfSecret are required when signing using signclient"
    }
} elseif ("$signatureType" -eq "Riganti") {
    if ([string]::IsNullOrEmpty($rigantiUrl) `
        -or [string]::IsNullOrEmpty($rigantiClientId) `
        -or [string]::IsNullOrEmpty($rigantiTenantId) `
        -or [string]::IsNullOrEmpty($rigantiSecret) `
        -or [string]::IsNullOrEmpty($rigantiCertificate)) {
        throw "-rigantiUrl, -rigantiClientId, -rigantiTenantId, -rigantiSecret, and -rigantiCertificate are required when signing using NuGetKeyVaultSignTool"
    }
} else {
    throw "$signatureType is not a valid signature type"
}

function Get-TemplateProjects {
    return @(
        [PSCustomObject]@{
            Name = "DotVVM.Templates"
            Path = "src/Templates/DotVVM.Templates.nuspec"
        }
    )
}

function Build-PublicProjectPackages {
    $packages = . "$PSScriptRoot/Get-PublicProjects.ps1"
    foreach ($package in $packages) {
        Write-Host "::group::$($package.Name)"
        $dir = Join-Path "$root" "$($package.Path)"
        try {
            dotnet build --nologo -c Release --no-restore --no-incremental -p:DOTVVM_ROOT="$root" -p:DOTVVM_VERSION="$version" "$dir"
            dotnet pack -c Release --no-build -p:DOTVVM_ROOT="$root" -p:DOTVVM_VERSION="$version" "$dir"
        }
        finally {
            Write-Host "::endgroup::"
        }
    }
}

function Build-TemplatePackages {
    $packages = Get-TemplateProjects
    foreach ($package in $packages) {
        Write-Host "::group::$($package.Name)"
        $path = Join-Path "$root" "$($package.Path)"
        try {
            $file = [System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)
            $file = [System.Text.RegularExpressions.Regex]::Replace($file, "\<version\>([^<]+)\</version\>", "<version>" + $version + "</version>")
            [System.IO.File]::WriteAllText($path, $file, [System.Text.Encoding]::UTF8)

            nuget pack $path -outputdirectory "$root/artifacts/packages" | Out-Host
        }
        finally {
            Write-Host "::endgroup::"
        }
    }
}

function Remove-AllPackages {
    Remove-Item -Recurse -Force (Join-Path "$root" "artifacts/packages") -ErrorAction SilentlyContinue
}

function Set-AllPackageSignatures {
    $oldCwd = Get-Location
    Set-Location "$root/src"

    try {
        foreach ($package in (Get-Item "$root/artifacts/packages/*.nupkg")) {
            $packageName = [System.IO.Path]::GetFileNameWithoutExtension($package);

            if ($signatureType -eq "DotNetFoundation") {
                dotnet signclient sign `
                    --baseDirectory "$root/artifacts/packages" `
                    --input "$package" `
                    --config "$root/signconfig.json" `
                    --user "$dnfUser" `
                    --secret "$dnfSecret" `
                    --name "$packageName" `
                    --description "$("$packageName" + " " + $env:DOTVVM_VERSION)" `
                    --descriptionUrl "https://github.com/riganti/dotvvm"
            }
            elseif ($signatureType -eq "Riganti") {
                dotnet NuGetKeyVaultSignTool sign `
                    --file-digest "sha256" `
                    --timestamp-rfc3161 "http://timestamp.digicert.com" `
                    --timestamp-digest "sha256" `
                    --azure-key-vault-url "$rigantiUrl" `
                    --azure-key-vault-client-id "$rigantiClientId" `
                    --azure-key-vault-tenant-id "$rigantiTenantId" `
                    --azure-key-vault-client-secret "$rigantiSecret" `
                    --azure-key-vault-certificate "$rigantiCertificate" `
                    "$package"
            }
            else {
                throw "$signatureType is not a valid signature type"
            }
        }
    }
    finally {
        Set-Location "$oldCwd"
    }
}

function Publish-AllPackages {
    dotnet nuget add source `
        --username "$internalFeedUser" `
        --password "$internalFeedPat" `
        --store-password-in-clear-text `
        --name internal `
        "$internalFeed"

    foreach ($package in (Get-Item "$root/artifacts/packages/*.nupkg")) {
        dotnet nuget push --api-key az --source internal "$package"
    }
}

Remove-AllPackages
Build-PublicProjectPackages
Build-TemplatePackages
Set-AllPackageSignatures
Publish-AllPackages
