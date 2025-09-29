#!/bin/bash

# Script khởi động tất cả services cho HCM Chatbot
# Sử dụng: ./start-all.sh

set -e

# Colors
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m'

print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_status "=== KHỞI ĐỘNG HCM CHATBOT SERVICES ==="

# Start Python AI Service
print_status "Đang khởi động Python AI service..."
cd /Users/techmax/Documents/hcm-chatbot/backend

if [ ! -d "venv" ]; then
    print_warning "Virtual environment không tồn tại, đang tạo..."
    python3 -m venv venv
    source venv/bin/activate
    pip install -r requirements.txt
else
    source venv/bin/activate
fi

print_status "Starting AI service on port 8000..."
nohup python -m uvicorn app.main:app --host 0.0.0.0 --port 8000 --reload > ai.log 2>&1 &

# Wait for AI service to start
sleep 3

# Start .NET API
print_status "Đang khởi động .NET API..."
cd /Users/techmax/Documents/hcm-chatbot/dotnet-api/hcm-chatbot-api

print_status "Building .NET project..."
dotnet build Web_API/Web_API.csproj

print_status "Starting API service on port 9000..."
nohup dotnet run --project Web_API/Web_API.csproj --urls="http://localhost:9000" > api.log 2>&1 &

# Wait for services to fully start
sleep 5

# Check services
print_status "Checking services..."

if lsof -ti:8000 >/dev/null 2>&1; then
    print_success "✓ Python AI service đang chạy trên port 8000"
else
    print_warning "✗ Python AI service không chạy được"
fi

if lsof -ti:9000 >/dev/null 2>&1; then
    print_success "✓ .NET API service đang chạy trên port 9000"
else
    print_warning "✗ .NET API service không chạy được"
fi

print_success "=== KHỞI ĐỘNG HOÀN TẤT ==="
print_status "Frontend: Mở file chat.html hoặc admin.html trong browser"
print_status "API Logs: tail -f /Users/techmax/Documents/hcm-chatbot/dotnet-api/hcm-chatbot-api/api.log"
print_status "AI Logs: tail -f /Users/techmax/Documents/hcm-chatbot/backend/ai.log"
print_status "Để restart services: ./restart-services.sh"
