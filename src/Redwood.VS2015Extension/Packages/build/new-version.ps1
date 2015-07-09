$templatePath = "..\..\..\Redwood.VS2015Extension.ProjectTemplate\RedwoodProject.vstemplate"
$nuspecPath = "..\Redwood\DotVVM.nuspec"
$manifestPath = "..\..\..\Redwood.VS2015Extension\source.extension.vsixmanifest"
$projectPath = "..\..\..\Redwood.VS2015Extension\Redwood.VS2015Extension.csproj"


$version = Read-Host "New version"

# change the nuspec
$nuspec = [System.IO.File]::ReadAllText($nuspecPath, [System.Text.Encoding]::UTF8)
$nuspec = [System.Text.RegularExpressions.Regex]::Replace($nuspec, "\<version\>([^""]+)\</version\>", "<version>" + $version + "</version>")
[System.IO.File]::WriteAllText($nuspecPath, $nuspec, [System.Text.Encoding]::UTF8)

# change the project template
$template = [System.IO.File]::ReadAllText($templatePath, [System.Text.Encoding]::UTF8)
$template = [System.Text.RegularExpressions.Regex]::Replace($template, "\<package id=""DotVVM"" version=""([^""]+)"" /\>", "<package id=""DotVVM"" version=""" + $version + """ />")
[System.IO.File]::WriteAllText($templatePath, $template, [System.Text.Encoding]::UTF8)

# change the vsix manifest
$manifest = [System.IO.File]::ReadAllText($manifestPath, [System.Text.Encoding]::UTF8)
$manifest = [System.Text.RegularExpressions.Regex]::Replace($manifest, "DotVVM.([^""]+).nupkg", "DotVVM." + $version + ".nupkg")
[System.IO.File]::WriteAllText($manifestPath, $manifest, [System.Text.Encoding]::UTF8)

# change the vsix project file
$project = [System.IO.File]::ReadAllText($projectPath, [System.Text.Encoding]::UTF8)
$project = [System.Text.RegularExpressions.Regex]::Replace($project, "DotVVM.([^*""]+).nupkg", "DotVVM." + $version + ".nupkg")
[System.IO.File]::WriteAllText($projectPath, $project, [System.Text.Encoding]::UTF8)
