[CmdletBinding()]
param(
    [switch]$SkipTests,
    [switch]$ReleaseIl2Cpp
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

function Invoke-UnityBatch {
    param(
        [string[]]$Arguments,
        [string]$LogPath,
        [string]$SuccessPattern
    )

    Remove-Item -LiteralPath $LogPath -Force -ErrorAction SilentlyContinue
    Start-Process -FilePath $unity -ArgumentList $Arguments -WindowStyle Hidden | Out-Null
    $deadline = (Get-Date).AddMinutes(20)
    do {
        Start-Sleep -Milliseconds 500
        if (Test-Path -LiteralPath $LogPath) {
            $tail = Get-Content -LiteralPath $LogPath -Tail 80 -ErrorAction SilentlyContinue
            if ($tail -match $SuccessPattern) { return }
            if ($tail -match 'Aborting batchmode|Compilation failed|BuildFailedException|Error building Player') {
                throw "Unity batch operation failed. See $LogPath"
            }
        }
    } while ((Get-Date) -lt $deadline)

    throw "Unity batch operation timed out. See $LogPath"
}

if (-not $SkipTests) {
    # Unity Test Framework exits the Editor after the run. Passing -quit here can
    # terminate Unity 6 before the test runner creates its result file.
    Remove-Item -LiteralPath $testResults -Force -ErrorAction SilentlyContinue
    Invoke-UnityBatch -Arguments @('-batchmode', '-nographics', '-projectPath', $projectRoot, '-runTests', '-testPlatform', 'EditMode', '-testResults', $testResults, '-logFile', $testLog) -LogPath $testLog -SuccessPattern 'Test run completed\. Exiting with code 0'
    if (-not (Test-Path -LiteralPath $testResults)) {
        throw "EditMode tests failed. See $testLog"
    }
    [xml]$testReport = Get-Content -LiteralPath $testResults
    if ($testReport.'test-run'.result -ne 'Passed' -or [int]$testReport.'test-run'.failed -ne 0) {
        throw "EditMode tests failed. See $testResults"
    }
}

if ($ReleaseIl2Cpp) {
    $env:BREATH_ISLAND_BACKEND = 'IL2CPP'
} else {
    $env:BREATH_ISLAND_BACKEND = 'MONO'
}

try {
    Invoke-UnityBatch -Arguments @('-batchmode', '-nographics', '-quit', '-projectPath', $projectRoot, '-executeMethod', 'BreathIsland.Editor.CommercialBuild.BuildWindowsCI', '-logFile', $buildLog) -LogPath $buildLog -SuccessPattern 'Exiting batchmode successfully now!'
} finally {
    Remove-Item Env:BREATH_ISLAND_BACKEND -ErrorAction SilentlyContinue
}

$executable = Join-Path $projectRoot 'Builds\Windows\BreathIsland.exe'
if (-not (Test-Path -LiteralPath $executable)) {
    throw "Build reported success but executable is missing: $executable"
}

Get-Item -LiteralPath $executable | Select-Object FullName, Length, LastWriteTime
