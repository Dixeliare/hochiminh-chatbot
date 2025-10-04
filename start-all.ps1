# HCM Chatbot - Start All Services (Windows PowerShell)
# This script starts all three components of the HCM Chatbot system

Write-Host "🇻🇳 Starting HCM Chatbot System..." -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan

# Function to check if a port is in use
function Test-Port {
    param([int]$Port)

    $connection = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue
    if ($connection) {
        Write-Host "⚠️  Port $Port is already in use!" -ForegroundColor Yellow
        return $false
    }
    return $true
}

# Function to start a service in background
function Start-Service {
    param(
        [string]$ServiceName,
        [string]$WorkingDir,
        [string]$Command,
        [int]$Port,
        [string]$LogFile
    )

    Write-Host "🚀 Starting $ServiceName on port $Port..." -ForegroundColor Green

    # Check if port is available
    if (-not (Test-Port -Port $Port)) {
        Write-Host "❌ Cannot start $ServiceName - port $Port is busy" -ForegroundColor Red
        return $false
    }

    # Create logs directory if it doesn't exist
    if (-not (Test-Path "logs")) {
        New-Item -ItemType Directory -Path "logs" | Out-Null
    }

    # Start the service
    $process = Start-Process -FilePath "powershell.exe" `
        -ArgumentList "-NoExit", "-Command", "cd '$WorkingDir'; $Command" `
        -WindowStyle Minimized `
        -PassThru

    # Save PID for later cleanup
    $process.Id | Out-File "logs\$($ServiceName.ToLower()).pid"

    Write-Host "✅ $ServiceName started (PID: $($process.Id))" -ForegroundColor Green
    return $true
}

# Function to wait for service to be ready
function Wait-ForService {
    param(
        [string]$ServiceName,
        [string]$Url,
        [int]$MaxAttempts = 30
    )

    Write-Host "⏳ Waiting for $ServiceName to be ready..." -ForegroundColor Yellow

    for ($i = 1; $i -le $MaxAttempts; $i++) {
        try {
            $response = Invoke-WebRequest -Uri $Url -TimeoutSec 2 -ErrorAction SilentlyContinue
            if ($response.StatusCode -eq 200) {
                Write-Host "✅ $ServiceName is ready!" -ForegroundColor Green
                return $true
            }
        }
        catch {
            # Service not ready yet
        }

        Write-Host "   Attempt $i/$MaxAttempts..." -ForegroundColor Gray
        Start-Sleep -Seconds 2
    }

    Write-Host "❌ $ServiceName failed to start within timeout" -ForegroundColor Red
    return $false
}

# Check prerequisites
Write-Host "🔍 Checking prerequisites..." -ForegroundColor Cyan

# Check if .NET is installed
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "❌ .NET is not installed. Please install .NET 8.0 or later." -ForegroundColor Red
    Write-Host "   Download from: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    exit 1
}

# Check if Python is installed
if (-not (Get-Command python -ErrorAction SilentlyContinue)) {
    Write-Host "❌ Python is not installed. Please install Python 3.8 or later." -ForegroundColor Red
    Write-Host "   Download from: https://www.python.org/downloads/" -ForegroundColor Yellow
    exit 1
}

# Check if PostgreSQL is running
try {
    $pgConnection = Test-NetConnection -ComputerName localhost -Port 5432 -WarningAction SilentlyContinue
    if (-not $pgConnection.TcpTestSucceeded) {
        Write-Host "❌ PostgreSQL is not running. Please start PostgreSQL first." -ForegroundColor Red
        Write-Host "   Run: pg_ctl -D 'C:\Program Files\PostgreSQL\16\data' start" -ForegroundColor Yellow
        exit 1
    }
}
catch {
    Write-Host "❌ Cannot check PostgreSQL status. Please ensure PostgreSQL is running." -ForegroundColor Red
    exit 1
}

Write-Host "✅ All prerequisites are met!" -ForegroundColor Green
Write-Host ""

# Start services
Write-Host "🚀 Starting services..." -ForegroundColor Cyan
Write-Host ""

# 1. Start .NET API
$dotnetDir = Join-Path $PSScriptRoot "dotnet-api\hcm-chatbot-api"
if (Start-Service -ServiceName "NET_API" -WorkingDir $dotnetDir -Command "dotnet run --project Web_API\Web_API.csproj --urls http://localhost:9000" -Port 9000 -LogFile "dotnet-api.log") {
    Start-Sleep -Seconds 5
    if (-not (Wait-ForService -ServiceName ".NET API" -Url "http://localhost:9000/health")) {
        Write-Host "❌ Failed to start .NET API" -ForegroundColor Red
        exit 1
    }
    Write-Host ""
}
else {
    exit 1
}

# 2. Start Python AI Backend
$backendDir = Join-Path $PSScriptRoot "backend"
if (Start-Service -ServiceName "PYTHON_AI" -WorkingDir $backendDir -Command "venv\Scripts\activate; python -m uvicorn app.main:app --host 0.0.0.0 --port 8000" -Port 8000 -LogFile "python-ai.log") {
    Start-Sleep -Seconds 5
    if (-not (Wait-ForService -ServiceName "Python AI" -Url "http://localhost:8000/health")) {
        Write-Host "❌ Failed to start Python AI" -ForegroundColor Red
        exit 1
    }
    Write-Host ""
}
else {
    exit 1
}

# 3. Start Frontend
$frontendDir = Join-Path $PSScriptRoot "frontend"
if (Start-Service -ServiceName "FRONTEND" -WorkingDir $frontendDir -Command "python -m http.server 3000" -Port 3000 -LogFile "frontend.log") {
    Start-Sleep -Seconds 3
    if (-not (Wait-ForService -ServiceName "Frontend" -Url "http://localhost:3000")) {
        Write-Host "❌ Failed to start Frontend" -ForegroundColor Red
        exit 1
    }
    Write-Host ""
}
else {
    exit 1
}

# Success message
Write-Host "🎉 HCM Chatbot System Started Successfully!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""
Write-Host "📍 Service URLs:" -ForegroundColor Cyan
Write-Host "   🌐 Frontend:    http://localhost:3000/welcome.html" -ForegroundColor White
Write-Host "   🔗 .NET API:    http://localhost:9000/swagger" -ForegroundColor White
Write-Host "   🤖 Python AI:   http://localhost:8000/docs" -ForegroundColor White
Write-Host ""
Write-Host "👤 Admin Account:" -ForegroundColor Cyan
Write-Host "   Username: admin" -ForegroundColor White
Write-Host "   Password: admin123" -ForegroundColor White
Write-Host ""
Write-Host "📋 Commands:" -ForegroundColor Cyan
Write-Host "   🛑 Stop all:    .\stop-all.ps1" -ForegroundColor White
Write-Host "   📊 Status:      .\status.ps1" -ForegroundColor White
Write-Host "   📝 Logs:        Get-Content logs\*.log -Tail 50" -ForegroundColor White
Write-Host ""
Write-Host "🎯 Quick Start:" -ForegroundColor Cyan
Write-Host "   1. Open: http://localhost:3000/welcome.html" -ForegroundColor White
Write-Host "   2. Click 'Đăng nhập' or 'Đăng ký'" -ForegroundColor White
Write-Host "   3. Start chatting about Hồ Chí Minh's thoughts!" -ForegroundColor White
Write-Host ""
Write-Host "✨ Enjoy using HCM Chatbot! 🇻🇳" -ForegroundColor Green
