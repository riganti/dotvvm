[Reflection.Assembly]::LoadWithPartialName('System.IO.Compression')

$zipfile = 'C:\dev\dotvvm\src\DotVVM.Framework\bin\Release\DotVVM.2.0.0-preview02-26468.symbols.zip'
#$files   = 'some.file', 'other.file', ...

try 
{

$stream = New-Object IO.FileStream($zipfile, [IO.FileMode]::Open)
$mode   = [IO.Compression.ZipArchiveMode]::Update
$zip    = New-Object IO.Compression.ZipArchive($stream, $mode)

Write-Host "Entries:"

($zip.Entries | ? { ($_.FullName.EndsWith('.dll') -or  $_.FullName.EndsWith('.exe')  -or  $_.FullName.EndsWith('.xml')) -and ( !$_.FullName.Contains('[Content_Types].xml') ) }) | % { $_.Delete() }

}
finally{
    $zip.Dispose()
    $stream.Close()
    $stream.Dispose()

}