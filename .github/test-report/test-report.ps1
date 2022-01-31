# Heavily inspired by https://github.com/zyborg/dotnet-tests-report
# and https://github.com/NasAmin/trx-parser.

if (-not (Get-Module -ListAvailable GitHubActions)) {
    Install-Module GitHubActions -Force
}
Import-Module GitHubActions

$trxPath = Get-ActionInput trx-path -Required
$reportName = Get-ActionInput report-name -Required
$reportTitle = Get-ActionInput report-title -Required
$githubToken = Get-ActionInput github-token -Required

$tmpDir = [System.IO.Path]::Combine($PWD, '_TMP')
New-Item -Name $tmpDir -ItemType Directory -Force -ErrorAction Ignore
Write-ActionInfo "Resolved tmpDir as '$tmpDir'"

function Build-MarkdownReport {
    $script:report_name = $inputs.report_name
    $script:report_title = $inputs.report_title
    $script:trx_xsl_path = $inputs.trx_xsl_path

    if (-not $script:report_name) {
        $script:report_name = "TEST_RESULTS_$([datetime]::Now.ToString('yyyyMMdd_hhmmss'))"
    }
    if (-not $report_title) {
        $script:report_title = $report_name
    }

    $script:test_report_path = Join-Path $tmpDir test-results.md
    $trx2mdParams = @{
        trxFile   = $script:test_results_path
        mdFile    = $script:test_report_path
        xslParams = @{
            reportTitle = $script:report_title
        }
    }
    if ($script:trx_xsl_path) {
        $script:trx_xsl_path = "$(Resolve-Path $script:trx_xsl_path)"
        Write-ActionInfo "Override TRX XSL Path Provided"
        Write-ActionInfo "  resolved as: $($script:trx_xsl_path)"

        if (Test-Path $script:trx_xsl_path) {
            ## If XSL path is provided and exists, override the default
            $trx2mdParams.xslFile = $script:trx_xsl_path
        }
        else {
            Write-ActionWarning "Could not find TRX XSL at resolved path; IGNORING"
        }
    }
    & "$PSScriptRoot/trx2md.ps1" @trx2mdParams -Verbose

}

function Publish-ToCheckRun {
    param(
        [string]$reportData
    )

    Write-ActionInfo "Publishing Report to GH Workflow"

    $ghToken = $inputs.github_token
    $ctx = Get-ActionContext
    $repo = Get-ActionRepo
    $repoFullName = "$($repo.Owner)/$($repo.Repo)"

    Write-ActionInfo "Resolving REF"
    $ref = $ctx.Sha
    if ($ctx.EventName -eq 'pull_request') {
        Write-ActionInfo "Resolving PR REF"
        $ref = $ctx.Payload.pull_request.head.sha
        if (-not $ref) {
            Write-ActionInfo "Resolving PR REF as AFTER"
            $ref = $ctx.Payload.after
        }
    }
    if (-not $ref) {
        Write-ActionError "Failed to resolve REF"
        exit 1
    }
    Write-ActionInfo "Resolved REF as $ref"
    Write-ActionInfo "Resolve Repo Full Name as $repoFullName"

    Write-ActionInfo "Adding Check Run"
    $conclusion = 'neutral'

    # Set check status based on test result outcome.
    if ($inputs.set_check_status_from_test_outcome) {

        Write-ActionInfo "Mapping check status to test outcome..."

        if ($testResult.ResultSummary_outcome -eq "Failed") {

            Write-ActionWarning "Found failing tests"
            $conclusion = 'failure'
        }
        elseif ($testResult.ResultSummary_outcome -eq "Completed") {

            Write-ActionInfo "All tests passed"
            $conclusion = 'success'
        }
    }

    $url = "https://api.github.com/repos/$repoFullName/check-runs"
    $hdr = @{
        Accept = 'application/vnd.github.antiope-preview+json'
        Authorization = "token $ghToken"
    }
    $bdy = @{
        name       = $report_name
        head_sha   = $ref
        status     = 'completed'
        conclusion = $conclusion
        output     = @{
            title   = $report_title
            summary = "This run completed at ``$([datetime]::Now)``"
            text    = $reportData
        }
    }
    Invoke-WebRequest -Headers $hdr $url -Method Post -Body ($bdy | ConvertTo-Json)
}

Set-ActionOutput -Name test_results_path -Value $test_results_path

Write-ActionInfo "Compiling Test Result object"
$testResultXml = Select-Xml -Path $test_results_path -XPath /
$testResult = [psobject]::new()
$testResultXml.Node.TestRun.Attributes | % { $testResult |
    Add-Member -MemberType NoteProperty -Name "TestRun_$($_.Name)" -Value $_.Value }
$testResultXml.Node.TestRun.Times.Attributes | % { $testResult |
    Add-Member -MemberType NoteProperty -Name "Times_$($_.Name)" -Value $_.Value }
$testResultXml.Node.TestRun.ResultSummary.Attributes | % { $testResult |
    Add-Member -MemberType NoteProperty -Name "ResultSummary_$($_.Name)" -Value $_.Value }
$testResultXml.Node.TestRun.ResultSummary.Counters.Attributes | % { $testResult |
    Add-Member -MemberType NoteProperty -Name "Counters_$($_.Name)" -Value $_.Value }
Write-ActionInfo "$($testResult|Out-Default)"

$result_clixml_path = Join-Path $tmpDir dotnet-test-result.clixml
Export-Clixml -InputObject $testResult -Path $result_clixml_path

Set-ActionOutput -Name result_clixml_path -Value $result_clixml_path
Set-ActionOutput -Name result_value -Value ($testResult.ResultSummary_outcome)
Set-ActionOutput -Name total_count -Value ($testResult.Counters_total)
Set-ActionOutput -Name passed_count -Value ($testResult.Counters_passed)
Set-ActionOutput -Name failed_count -Value ($testResult.Counters_failed)

Write-ActionInfo "Generating Markdown Report from TRX file"
Build-MarkdownReport
$reportData = [System.IO.File]::ReadAllText($test_report_path)

if ($inputs.skip_check_run -ne $true) {
    Publish-ToCheckRun -ReportData $reportData
}
if ($inputs.gist_name -and $inputs.gist_token) {
    Publish-ToGist -ReportData $reportData
}
