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
    $password = [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes("$pat"));
    $feed = "$registry".Trim("https:");
    npm config set --location project "registry=$registry"
    npm config set --location project "${feed}:username=$username"
    npm config set --location project "${feed}:email=$email"
    npm config set --location project "${feed}:_password=$password"
}
finally {
    Set-Location "$oldCwd"
}
