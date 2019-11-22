Param(
    [string]$key,
    [string]$value,
    [string]$configPath = '../DotVVM.Samples.Common/sampleConfig.json'
)

$json = Get-Content $configPath -raw | ConvertFrom-Json
$json.appSettings.$key = $value
$json | ConvertTo-Json -depth 32 | set-content $configPath