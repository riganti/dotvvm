param([String]$outputDirectory)

Set-Location $PSScriptRoot
Set-Location ..\DotVVM.Framework
npm install

npm install yarn -g
yarn add jest-junit -g

Set-Location $PSScriptRoot
Set-Location ..\DotVVM.Framework\Resources\Scripts\tests

yarn jest --ci --reporters="jest-junit"

Set-Location $PSScriptRoot
Set-Location ..\DotVVM.Framework

Copy-Item .\junit.xml $outputDirectory\js-tests-results.xml
Remove-Item .\junit.xml

Set-Location $PSScriptRoot