param(
    [string]$targetDirectory,
    [string][parameter(Mandatory = $true)]$registry,
    [string][parameter(Mandatory = $false)]$pat,
    [string][parameter(Mandatory = $false)]$authToken,
    [string][parameter(Mandatory = $false)]$username,
    [string][parameter(Mandatory = $false)]$email
)

$oldCwd = Get-Location

if (-not ([string]::IsNullOrWhiteSpace("$targetDirectory"))) {
    if (-not (Test-Path -PathType Container "$targetDirectory")) {
        throw "Target directory '$targetDirectory' does not exist."
    }
    Set-Location $targetDirectory
    $location = "project"
} else {
    $location = "user"
}

try {
    $feed = "$registry".Trim("https:");
    npm config set --location $location "registry=$registry"
    if ($username) {
        npm config set --location $location "${feed}:username=$username"
    }
    if ($email) {
        npm config set --location $location "${feed}:email=$email"
    }
    if ($pat) {
        $password = [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes("$pat"));
        npm config set --location $location "${feed}:_password=$password"
    }
    if ($authToken) {
        npm config set --location $location "${feed}:_authToken=$authToken"
    }
}
finally {
    Set-Location "$oldCwd"
}
