# Heavily inspired by https://github.com/zyborg/dotnet-tests-report
# and https://github.com/NasAmin/trx-parser.

# NB: Action inputs are not obtained using the GitHubActions PS module,
#     because we use a composite action.
param(
    [string] $trxPath = "tmp/test.trx",
    [string] $reportName,
    [string] $reportTitle,
    [string] $githubToken)

if (-not (Get-Module -ListAvailable GitHubActions)) {
    Install-Module GitHubActions -Force
}
Import-Module GitHubActions

$tmpDir = Join-Path (Get-Location) "tmp"
New-Item -Path $tmpDir -ItemType Directory -Force -ErrorAction Ignore
if (!(Test-Path -Path $tmpDir -PathType Container)) {
    throw "Could not create a temporary directory."
}

if (-not $reportName) {
    $reportName = "tests-$([datetime]::Now.ToString('yyyy-MM-ddThh-mm-ss'))"
}
if (-not $reportTitle) {
    $reportTitle = $reportName
}

$reportPath = Join-Path $tmpDir "$reportName.md"

Write-ActionInfo "Temporary directory: '$tmpDir'"
Write-ActionInfo "Test results path: '$trxPath'"
Write-ActionInfo "Report name: '$reportName'"
Write-ActionInfo "Report title: '$reportTitle'"
Write-ActionInfo "Report path: '$reportPath'"

function Build-MarkdownReport {
    $trx2mdParams = @{
        trxFile   = $trxPath
        mdFile    = $reportPath
        xslParams = @{
            reportTitle = $reportTitle
        }
    }
    & "$PSScriptRoot/trx2md.ps1" @trx2mdParams -Verbose
}

function Get-ReportText {
    $reportText =
    if ($reportText.Length -gt 65535 ) {
        $tooLongError = "...`nThe test report is too long to display.`n"
        $reportText = $reportText.Substring(0, [System.Math]::Min($reportText.Length, 65535 - $tooLongError.Length)) `
            + $tooLongError
        Write-ActionWarning "Report is $($reportText.Length) characters long. Shortening to 65535."
    }
    return $reportText;
}

function Publish-ToCheckRun {
    param(
        [string]$reportText
    )

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
        Authorization = "token $githubToken"
    }

    $bdy = @{
        name       = $reportName
        head_sha   = $ref
        status     = 'completed'
        conclusion = $conclusion
        output     = @{
            title   = $reportTitle
            summary = "This run completed at ``$([datetime]::Now)``"
            text    = $reportText
        }
    }
    Invoke-WebRequest -Headers $hdr $url -Method Post -Body ($bdy | ConvertTo-Json)
}

$trxNamespace = @{
    trx = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"
}
$notPassedTests = Select-Xml -Path $trxPath -Namespace $trxNamespace -XPath "//trx:UnitTestResult[@outcome!='Passed']"
if ($notPassedTests.Length -eq 0) {
    Write-ActionInfo "All tests have passed. No report needed."
    exit 0
}

Write-ActionInfo "Generating Markdown Report from TRX file"
Build-MarkdownReport

if (-not $githubToken) {
    Write-Warning "GitHub token is missing. Skipping upload to GitHub."
} else {
    $reportText = Get-ReportText
    Publish-ToCheckRun -reportText $reportText
}

