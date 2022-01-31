# Author: Eugene Bekker
# https://github.com/zyborg/dotnet-tests-report
# Licensed under MIT license

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$trxFile,
    [string]$mdFile=$null,
    [string]$xslFile=$null,
    [hashtable]$xslParams=$null
)

if ($trxFile -notmatch '^[/\\]') {
    $trxFile = [System.IO.Path]::Combine($PWD, $trxFile)
    Write-Verbose "Resolving TRX file relative to current directory: $trxFile"
}

if (-not $mdFile) {
    $mdFile = $trxFile
    if ([System.IO.Path]::GetExtension($trxFile) -ieq '.trx') {
        $mdFile = $trxFile -ireplace '.trx$',''
    }
    $mdFile += '.md'
    Write-Verbose "Resolving default MD file: $mdFile"
}
elseif ($mdFile -notmatch '^[/\\]') {
    $mdFile = [System.IO.Path]::Combine($PWD, $mdFile)
    Write-Verbose "Resolving MD file relative to current directory: $mdFile"
}

if (-not $xslFile) {
    $xslFile = "$PSScriptRoot/trx2md.xsl"
    Write-Verbose "Resolving default XSL file: $xslFile"
}
elseif ($xslFile -notmatch '^[/\\]') {
    $xslFile = [System.IO.Path]::Combine($PWD, $xslFile)
    Write-Verbose "Resolving XSL file relative to current directory: $xslFile"
}

class TrxFn {
    [double]DiffSeconds([datetime]$from, [datetime]$till) {
        return ($till - $from).TotalSeconds
    }
}


if (-not $script:xslt) {
    $script:urlr = [System.Xml.XmlUrlResolver]::new()
    $script:opts = [System.Xml.Xsl.XsltSettings]::new()
    #$script:opts.EnableScript = $true
    $script:xslt = [System.Xml.Xsl.XslCompiledTransform]::new()
    try {
        $script:xslt.Load($xslFile, $script:opts, $script:urlr)
    }
    catch {
        Write-Error $Error[0]
        return
    }
    Write-Verbose "Loaded XSL transformer"
}

$script:list = [System.Xml.Xsl.XsltArgumentList]::new()
$script:list.AddExtensionObject("urn:trxfn", [TrxFn]::new())
if ($xslParams) {
    foreach ($xp in $xslParams.GetEnumerator()) {
        $script:list.AddParam($xp.Key, [string]::Empty, $xp.Value)
    }
}
$script:wrtr = [System.IO.StreamWriter]::new($mdFile)
try {
    Write-Verbose "Transforming TRX to MD"
    $script:xslt.Transform(
        [string]$trxFile,
        [System.Xml.Xsl.XsltArgumentList]$script:list,
        [System.IO.TextWriter]$script:wrtr)
}
finally {
    $script:wrtr.Dispose()
}
