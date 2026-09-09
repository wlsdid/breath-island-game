[CmdletBinding()]
param(
    [switch]$SkipTests
)

$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.5.4f1\Editor\Unity.exe'
$buildDirectory = Join-Path $projectRoot 'Builds'
$testResults = Join-Path $buildDirectory 'editmode-results.xml'
$testLog = Join-Path $buildDirectory 'tests.log'
$buildLog = Join-Path $buildDirectory 'build.log'

if (-not (Test-Path -LiteralPath $unity)) {
    throw "Unity 6000.5.4f1 was not found at $unity"
}

New-Item -ItemType Directory -Force $buildDirectory | Out-Null

if (-not $SkipTests) {
    & $unity -batchmode -nographics -quit -projectPath $projectRoot -runTests -testPlatform EditMode -testResults $testResults -logFile $testLog
    if ($LASTEXITCODE -ne 0) {
        throw "EditMode tests failed. See $testLog"
    }
}

& $unity -batchmode -nographics -quit -projectPath $projectRoot -executeMethod BreathIsland.Editor.CommercialBuild.BuildWindowsCI -logFile $buildLog
if ($LASTEXITCODE -ne 0) {
    throw "Windows build failed. See $buildLog"
}

$executable = Join-Path $projectRoot 'Builds\Windows\BreathIsland.exe'
if (-not (Test-Path -LiteralPath $executable)) {
    throw "Build reported success but executable is missing: $executable"
}

Get-Item -LiteralPath $executable | Select-Object FullName, Length, LastWriteTime

