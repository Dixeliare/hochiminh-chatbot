# HCM Chatbot - Stop All Services (Windows PowerShell)
# This script stops all running HCM Chatbot services

Write-Host "🛑 Stopping HCM Chatbot System..." -ForegroundColor Red
Write-Host "==================================" -ForegroundColor Red

# Function to stop service by PID file
function Stop-ServiceByPid {
    param(
        [string]$ServiceName,
        [string]$PidFile
    )

    if (Test-Path $PidFile) {
        $pid = Get-Content $PidFile
        try {
            $process = Get-Process -Id $pid -ErrorAction SilentlyContinue
            if ($process) {
                Stop-Process -Id $pid -Force
                Write-Host "✅ Stopped $ServiceName (PID: $pid)" -ForegroundColor Green
            }
            else {
                Write-Host "⚠️  $ServiceName process not found (PID: $pid)" -ForegroundColor Yellow
            }
        }
        catch {
            Write-Host "⚠️  Error stopping $ServiceName : $_" -ForegroundColor Yellow
        }
        Remove-Item $PidFile -ErrorAction SilentlyContinue
    }
    else {
        Write-Host "ℹ️  No PID file for $ServiceName" -ForegroundColor Gray
    }
}

# Function to kill process by port
function Stop-ProcessByPort {
    param(
        [string]$ServiceName,
        [int]$Port
    )

    $connections = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue
    if ($connections) {
        foreach ($conn in $connections) {
            $process = Get-Process -Id $conn.OwningProcess -ErrorAction SilentlyContinue
            if ($process) {
                Stop-Process -Id $process.Id -Force
                Write-Host "✅ Killed $ServiceName on port $Port (PID: $($process.Id))" -ForegroundColor Green
            }
        }
    }
    else {
        Write-Host "ℹ️  No process running on port $Port" -ForegroundColor Gray
    }
}

# Stop services by PID files
Write-Host "🔍 Stopping services by PID files..." -ForegroundColor Cyan
Stop-ServiceByPid -ServiceName "Frontend" -PidFile "logs\frontend.pid"
Stop-ServiceByPid -ServiceName "Python AI" -PidFile "logs\python_ai.pid"
Stop-ServiceByPid -ServiceName ".NET API" -PidFile "logs\net_api.pid"

Write-Host ""
Write-Host "🔍 Checking and killing processes by ports..." -ForegroundColor Cyan

# Also check ports and kill if needed
Stop-ProcessByPort -ServiceName "Frontend" -Port 3000
Stop-ProcessByPort -ServiceName "Python AI" -Port 8000
Stop-ProcessByPort -ServiceName ".NET API" -Port 9000

Write-Host ""
Write-Host "✅ All services stopped!" -ForegroundColor Green
Write-Host ""
Write-Host "💡 To start again, run: .\start-all.ps1" -ForegroundColor Cyan
