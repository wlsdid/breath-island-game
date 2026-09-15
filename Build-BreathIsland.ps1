[CmdletBinding()]
param([switch]$SkipTests, [switch]$ReleaseIl2Cpp)
$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.5.4f1\Editor\Unity.exe'
$buildDirectory = Join-Path $projectRoot 'Builds'
if (-not (Test-Path -LiteralPath $unity)) { throw "Unity 6000.5.4f1 was not found at $unity" }
New-Item -ItemType Directory -Force $buildDirectory | Out-Null

function Invoke-UnityBatch {
    param([string[]]$Arguments, [string]$LogPath)
    Remove-Item -LiteralPath $LogPath -Force -ErrorAction SilentlyContinue
    # Quote paths for Windows Start-Process and wait for the Editor to release the project.
    $quotedArguments = $Arguments | ForEach-Object { '"' + $_ + '"' }
    $process = Start-Process -FilePath $unity -ArgumentList $quotedArguments -WindowStyle Hidden -PassThru
    $deadline = (Get-Date).AddMinutes(20)
    while (-not $process.WaitForExit(500)) {
        if ((Get-Date) -gt $deadline) {
            $process.Kill()
            throw "Unity operation timed out. See $LogPath"
        }
    }
    if ($process.ExitCode -ne 0) { throw "Unity exited with code $($process.ExitCode). See $LogPath" }
    if (-not (Test-Path -LiteralPath $LogPath)) { throw "Unity did not produce $LogPath" }
    if (Select-String -LiteralPath $LogPath -Pattern 'No valid Unity Editor license|error CS\d+|Aborting batchmode|BuildFailedException' -Quiet) {
        throw "Unity reported a license, compile or build error. See $LogPath"
    }
}

$setupLog = Join-Path $buildDirectory 'localization-setup.log'
Invoke-UnityBatch -Arguments @('-batchmode', '-nographics', '-quit', '-projectPath', $projectRoot,
    '-executeMethod', 'BreathIsland.Editor.LocalizationSetup.EnsureConfigured', '-logFile', $setupLog) -LogPath $setupLog

if (-not $SkipTests) {
    foreach ($testPlatform in @('EditMode', 'PlayMode')) {
        $testResults = Join-Path $buildDirectory ($testPlatform.ToLowerInvariant() + '-results.xml')
        $testLog = Join-Path $buildDirectory ($testPlatform.ToLowerInvariant() + '-tests.log')
        Remove-Item -LiteralPath $testResults -Force -ErrorAction SilentlyContinue
        # The runner owns shutdown. IMGUI needs a real Game View, which batch mode does not repaint.
        $testArguments = @('-projectPath', $projectRoot, '-runTests', '-testPlatform', $testPlatform,
            '-testResults', $testResults, '-logFile', $testLog)
        if ($testPlatform -eq 'EditMode') { $testArguments = @('-batchmode') + $testArguments }
        Invoke-UnityBatch -Arguments $testArguments -LogPath $testLog
        if (-not (Test-Path -LiteralPath $testResults)) { throw "$testPlatform did not produce results. See $testLog" }
        [xml]$testReport = Get-Content -LiteralPath $testResults -Raw
        if ($testReport.'test-run'.result -ne 'Passed' -or [int]$testReport.'test-run'.failed -ne 0 -or [int]$testReport.'test-run'.total -eq 0) {
            throw "$testPlatform tests failed or did not run. See $testResults"
        }
    }
}
$previousBackend = $env:BREATH_ISLAND_BACKEND
$env:BREATH_ISLAND_BACKEND = if ($ReleaseIl2Cpp) { 'IL2CPP' } else { 'MONO' }
$buildLog = Join-Path $buildDirectory 'build.log'
try {
    Invoke-UnityBatch -Arguments @('-batchmode', '-nographics', '-quit', '-projectPath', $projectRoot,
        '-executeMethod', 'BreathIsland.Editor.CommercialBuild.BuildWindowsCI', '-logFile', $buildLog) -LogPath $buildLog
} finally {
    $env:BREATH_ISLAND_BACKEND = $previousBackend
}
$executable = Join-Path $buildDirectory 'Windows\BreathIsland.exe'
if (-not (Test-Path -LiteralPath $executable)) { throw "Build executable is missing: $executable" }
Get-Item -LiteralPath $executable | Select-Object FullName, Length, LastWriteTime
