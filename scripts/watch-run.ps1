<#
Kills any running instance of the Presentation project (if detected by command line)
and starts `dotnet watch run` for the Presentation project.

Usage (PowerShell):
  .\scripts\watch-run.ps1
#>

try {
    $scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
} catch {
    $scriptRoot = Get-Location
}

$projDir = Resolve-Path (Join-Path $scriptRoot '..\OnlineLearningPlatform.Presentation')
$csproj = Join-Path $projDir 'OnlineLearningPlatform.Presentation.csproj'

Write-Host "Project dir: $projDir"
Write-Host "csproj: $csproj"

# Find dotnet processes that reference the project path and kill them
$candidates = Get-CimInstance Win32_Process | Where-Object {
    $_.CommandLine -and ($_.CommandLine -like "*$($projDir.Path)*")
}

if ($candidates) {
    foreach ($p in $candidates) {
        Write-Host "Stopping process $($p.ProcessId) - $($p.Name)" -ForegroundColor Yellow
        try {
            Stop-Process -Id $p.ProcessId -Force -ErrorAction Stop
        } catch {
            Write-Host "Failed to stop PID $($p.ProcessId): $_" -ForegroundColor Red
        }
    }
    Start-Sleep -Milliseconds 500
}

Write-Host "Starting dotnet watch run..." -ForegroundColor Green
dotnet watch --project $csproj run
