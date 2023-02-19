param(
    [string]$targetDirectory,
    [string][parameter(Mandatory = $true)]$registry,
    [string][parameter(Mandatory = $true)]$pat,
    [string][parameter(Mandatory = $true)]$username,
    [string][parameter(Mandatory = $true)]$email
)

$oldCwd = Get-Location

if (-not ([string]::IsNullOrWhiteSpace("$targetDirectory"))) {
    if (-not (Test-Path -PathType Container "$targetDirectory")) {
        throw "Target directory '$targetDirectory' does not exist."
    }
    Set-Location $targetDirectory
}

try {
    $password = [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes("$internalNpmRegistryPat"));
    $feed = "$internalNpmRegistry".Trim("https:");
    npm set --location project `
        "always-auth=true" `
        "registry=$internalNpmRegistry" `
        "${feed}:username=$internalNpmRegistryUsername" `
        "${feed}:email=$internalNpmRegistryEmail" `
        "${feed}:_password=$password"
}
finally {
    Set-Location "$oldCwd"
}
