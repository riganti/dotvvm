Param(
    [string]$selectedProfile,
    [string]$configPath = '../DotVVM.Samples.Common/sampleConfig.json'
)
if ([System.String]::IsNullOrEmpty($selectedProfile) -eq $true) {
  $selectedProfile = "Default"
}

$json = Get-Content $configPath -raw | ConvertFrom-Json
$json.activeProfile = $selectedProfile
$json | ConvertTo-Json -depth 32 | set-content $configPath