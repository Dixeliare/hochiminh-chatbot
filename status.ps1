# HCM Chatbot - Status Check (Windows PowerShell)
# This script checks the status of all HCM Chatbot services

Write-Host "📊 HCM Chatbot System Status" -ForegroundColor Cyan
Write-Host "=============================" -ForegroundColor Cyan
Write-Host ""

# Function to check service status
function Get-ServiceStatus {
    param(
        [string]$ServiceName,
        [int]$Port,
        [string]$HealthUrl
    )

    Write-Host "🔍 Checking $ServiceName (Port $Port)..." -ForegroundColor Yellow

    # Check if port is in use
    $connection = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue
    if (-not $connection) {
        Write-Host "   ❌ Status: NOT RUNNING" -ForegroundColor Red
        Write-Host ""
        return
    }

    # Get process info
    $process = Get-Process -Id $connection[0].OwningProcess -ErrorAction SilentlyContinue
    if ($process) {
        Write-Host "   ✅ Status: RUNNING" -ForegroundColor Green
        Write-Host "   📋 PID: $($process.Id)" -ForegroundColor Gray
        Write-Host "   💾 Memory: $([math]::Round($process.WorkingSet64 / 1MB, 2)) MB" -ForegroundColor Gray
        Write-Host "   ⏱️  CPU Time: $($process.TotalProcessorTime.ToString('hh\:mm\:ss'))" -ForegroundColor Gray
    }

    # Check health endpoint
    try {
        $response = Invoke-WebRequest -Uri $HealthUrl -TimeoutSec 2 -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            Write-Host "   🏥 Health: OK" -ForegroundColor Green
        }
    }
    catch {
        Write-Host "   ⚠️  Health: CHECK FAILED" -ForegroundColor Yellow
    }

    Write-Host ""
}

# Check prerequisites
Write-Host "🔧 Prerequisites:" -ForegroundColor Cyan

# Check .NET
if (Get-Command dotnet -ErrorAction SilentlyContinue) {
    $dotnetVersion = (dotnet --version)
    Write-Host "   ✅ .NET: $dotnetVersion" -ForegroundColor Green
}
else {
    Write-Host "   ❌ .NET: NOT INSTALLED" -ForegroundColor Red
}

# Check Python
if (Get-Command python -ErrorAction SilentlyContinue) {
    $pythonVersion = (python --version)
    Write-Host "   ✅ Python: $pythonVersion" -ForegroundColor Green
}
else {
    Write-Host "   ❌ Python: NOT INSTALLED" -ForegroundColor Red
}

# Check PostgreSQL
try {
    $pgConnection = Test-NetConnection -ComputerName localhost -Port 5432 -WarningAction SilentlyContinue -InformationLevel Quiet
    if ($pgConnection) {
        Write-Host "   ✅ PostgreSQL: RUNNING (Port 5432)" -ForegroundColor Green
    }
    else {
        Write-Host "   ❌ PostgreSQL: NOT RUNNING" -ForegroundColor Red
    }
}
catch {
    Write-Host "   ❌ PostgreSQL: NOT RUNNING" -ForegroundColor Red
}

Write-Host ""
Write-Host "🚀 Services:" -ForegroundColor Cyan
Write-Host ""

# Check each service
Get-ServiceStatus -ServiceName ".NET API" -Port 9000 -HealthUrl "http://localhost:9000/health"
Get-ServiceStatus -ServiceName "Python AI" -Port 8000 -HealthUrl "http://localhost:8000/health"
Get-ServiceStatus -ServiceName "Frontend" -Port 3000 -HealthUrl "http://localhost:3000"

# Summary
Write-Host "📍 URLs:" -ForegroundColor Cyan
Write-Host "   🌐 Frontend:  http://localhost:3000/welcome.html" -ForegroundColor White
Write-Host "   🔗 API Docs:  http://localhost:9000/swagger" -ForegroundColor White
Write-Host "   🤖 AI Docs:   http://localhost:8000/docs" -ForegroundColor White
Write-Host ""

# Check log files
if (Test-Path "logs") {
    Write-Host "📝 Recent Logs:" -ForegroundColor Cyan
    Get-ChildItem "logs\*.log" -ErrorAction SilentlyContinue | ForEach-Object {
        $lastWrite = $_.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")
        $size = [math]::Round($_.Length / 1KB, 2)
        Write-Host "   📄 $($_.Name) - ${size} KB (Last: $lastWrite)" -ForegroundColor Gray
    }
    Write-Host ""
}
