# HCM Chatbot - Production Deployment Script (Windows PowerShell)
# This script deploys the entire HCM Chatbot system using Docker

Write-Host "🚀 HCM Chatbot - Production Deployment" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Check if Docker is installed
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Host "❌ Docker is not installed. Please install Docker Desktop first." -ForegroundColor Red
    Write-Host "   Visit: https://docs.docker.com/desktop/install/windows-install/" -ForegroundColor Yellow
    exit 1
}

# Check if Docker Compose is available
try {
    docker compose version | Out-Null
}
catch {
    Write-Host "❌ Docker Compose is not available. Please ensure Docker Desktop is running." -ForegroundColor Red
    exit 1
}

# Check if .env.production exists
if (-not (Test-Path .env.production)) {
    Write-Host "⚠️  .env.production not found!" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Creating .env.production from example..." -ForegroundColor Yellow
    Copy-Item .env.production.example .env.production
    Write-Host ""
    Write-Host "⚠️  IMPORTANT: Edit .env.production and fill in your values:" -ForegroundColor Yellow
    Write-Host "   - POSTGRES_PASSWORD" -ForegroundColor White
    Write-Host "   - GEMINI_API_KEY" -ForegroundColor White
    Write-Host "   - JWT_SECRET_KEY" -ForegroundColor White
    Write-Host "   - FRONTEND_URL" -ForegroundColor White
    Write-Host ""
    Write-Host "After editing, run this script again." -ForegroundColor Yellow
    exit 1
}

# Load environment variables
Get-Content .env.production | ForEach-Object {
    if ($_ -match '^([^=]+)=(.*)$') {
        $key = $matches[1]
        $value = $matches[2]
        [Environment]::SetEnvironmentVariable($key, $value, "Process")
    }
}

Write-Host "✅ Environment variables loaded" -ForegroundColor Green
Write-Host ""

# Display configuration
Write-Host "📋 Deployment Configuration:" -ForegroundColor Cyan
$frontendUrl = $env:FRONTEND_URL
if (-not $frontendUrl) { $frontendUrl = "http://localhost" }
Write-Host "   - Frontend URL: $frontendUrl" -ForegroundColor White
Write-Host "   - Database: PostgreSQL 16" -ForegroundColor White
$geminiPreview = $env:GEMINI_API_KEY
if ($geminiPreview) { $geminiPreview = $geminiPreview.Substring(0, [Math]::Min(10, $geminiPreview.Length)) + "..." }
Write-Host "   - Gemini API: $geminiPreview" -ForegroundColor White
Write-Host ""

# Ask for confirmation
$confirmation = Read-Host "Continue with deployment? (y/n)"
if ($confirmation -ne 'y' -and $confirmation -ne 'Y') {
    Write-Host "Deployment cancelled." -ForegroundColor Yellow
    exit 0
}

# Stop existing containers
Write-Host ""
Write-Host "🛑 Stopping existing containers..." -ForegroundColor Yellow
docker compose down 2>$null
Write-Host ""

# Build images
Write-Host "🔨 Building Docker images..." -ForegroundColor Cyan
docker compose build --no-cache
Write-Host ""

# Start services
Write-Host "🚀 Starting services..." -ForegroundColor Green
docker compose --env-file .env.production up -d
Write-Host ""

# Wait for services to be ready
Write-Host "⏳ Waiting for services to be ready..." -ForegroundColor Yellow
Write-Host ""

$maxAttempts = 30
$attempt = 1

while ($attempt -le $maxAttempts) {
    Write-Host "   Checking health... (Attempt $attempt/$maxAttempts)" -ForegroundColor Gray

    # Check if containers are running
    $runningContainers = docker compose ps --filter "status=running" -q

    if ($runningContainers) {
        # Check health endpoints
        try {
            $frontend = Invoke-WebRequest -Uri "http://localhost/" -TimeoutSec 2 -UseBasicParsing -ErrorAction SilentlyContinue
            $api = Invoke-WebRequest -Uri "http://localhost:9000/health" -TimeoutSec 2 -UseBasicParsing -ErrorAction SilentlyContinue
            $ai = Invoke-WebRequest -Uri "http://localhost:8000/health" -TimeoutSec 2 -UseBasicParsing -ErrorAction SilentlyContinue

            if ($frontend -and $api -and $ai) {
                Write-Host ""
                Write-Host "✅ All services are healthy!" -ForegroundColor Green
                break
            }
        }
        catch {
            # Services not ready yet
        }
    }

    if ($attempt -eq $maxAttempts) {
        Write-Host ""
        Write-Host "❌ Services failed to start. Check logs with:" -ForegroundColor Red
        Write-Host "   docker compose logs" -ForegroundColor Yellow
        exit 1
    }

    Start-Sleep -Seconds 3
    $attempt++
}

Write-Host ""
Write-Host "🎉 Deployment Successful!" -ForegroundColor Green
Write-Host "========================" -ForegroundColor Green
Write-Host ""
Write-Host "📍 Service URLs:" -ForegroundColor Cyan
Write-Host "   🌐 Frontend:    http://localhost/" -ForegroundColor White
Write-Host "   🔗 API Docs:    http://localhost:9000/swagger" -ForegroundColor White
Write-Host "   🤖 AI Docs:     http://localhost:8000/docs" -ForegroundColor White
Write-Host "   💚 Health:      http://localhost:9000/health" -ForegroundColor White
Write-Host ""
Write-Host "👤 Admin Account:" -ForegroundColor Cyan
Write-Host "   Username: admin" -ForegroundColor White
Write-Host "   Password: admin123" -ForegroundColor White
Write-Host ""
Write-Host "📋 Useful Commands:" -ForegroundColor Cyan
Write-Host "   View logs:       docker compose logs -f" -ForegroundColor White
Write-Host "   Stop services:   docker compose down" -ForegroundColor White
Write-Host "   Restart:         docker compose restart" -ForegroundColor White
Write-Host "   View status:     docker compose ps" -ForegroundColor White
Write-Host ""
Write-Host "🔒 Security Reminder:" -ForegroundColor Yellow
Write-Host "   - Change admin password after first login" -ForegroundColor White
Write-Host "   - Set up SSL/TLS for production" -ForegroundColor White
Write-Host "   - Configure firewall rules" -ForegroundColor White
Write-Host "   - Backup database regularly" -ForegroundColor White
Write-Host ""
Write-Host "✨ HCM Chatbot is now live! 🇻🇳" -ForegroundColor Green
