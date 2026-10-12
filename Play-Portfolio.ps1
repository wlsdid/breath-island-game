[CmdletBinding()]
param([ValidateSet('village','sea','home','gather')][string]$Scene='village', [switch]$PrepareOnly)
$ErrorActionPreference='Stop'
$executable=Join-Path $PSScriptRoot 'Builds/Windows/BreathIsland.exe'
$seeds=Join-Path $PSScriptRoot 'Documentation/Portfolio/Seeds'
if (!(Test-Path -LiteralPath $executable)) { throw 'Build the Windows player with Build-BreathIsland.ps1 first.' }
# Every run creates a fresh session; no real save or previous demo is replaced.
$sessionRoot=Join-Path $PSScriptRoot ('Builds/Portfolio/Sessions/'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $sessionRoot -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $seeds ($Scene+'.json')) -Destination (Join-Path $sessionRoot 'autosave.json')
$scenes=@('sea','gather','home')
for($i=0;$i -lt 3;$i++) { Copy-Item -LiteralPath (Join-Path $seeds ($scenes[$i]+'.json')) -Destination (Join-Path $sessionRoot ('save_'+$i+'.json')) }
$exeRelative='../../../Windows/BreathIsland.exe'
$resume=@"
@echo off
setlocal
set "BREATH_ISLAND_SAVE_ROOT=%~dp0"
start "" "%~dp0$exeRelative" -screen-fullscreen 0 -screen-width 1280 -screen-height 720 -logFile "%~dp0Player.log"
endlocal
"@
[IO.File]::WriteAllText((Join-Path $sessionRoot 'Resume.cmd'),$resume,[Text.Encoding]::ASCII)
Write-Output "Isolated demo session: $sessionRoot"
Write-Output 'Slots 1/2/3: sea / gathering / home. Resume.cmd continues this session after exit.'
if (!$PrepareOnly) {
    $previous=$env:BREATH_ISLAND_SAVE_ROOT
    try {
        $env:BREATH_ISLAND_SAVE_ROOT=$sessionRoot
        Start-Process -FilePath $executable -ArgumentList @('-screen-fullscreen','0','-screen-width','1280','-screen-height','720','-logFile',('"'+(Join-Path $sessionRoot 'Player.log')+'"')) -WindowStyle Normal
    } finally { $env:BREATH_ISLAND_SAVE_ROOT=$previous }
}
