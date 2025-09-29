#!/bin/bash

# Script restart các services cho HCM Chatbot
# Sử dụng: ./restart-services.sh [api|ai|all]

set -e  # Dừng script nếu có lỗi

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to kill processes on specific ports
kill_port() {
    local port=$1
    local service_name=$2

    print_status "Đang tìm processes trên port $port..."

    # Tìm và kill processes trên port
    local pids=$(lsof -ti:$port 2>/dev/null || true)

    if [ -n "$pids" ]; then
        print_warning "Đang kill $service_name processes (PIDs: $pids)..."
        echo $pids | xargs kill -9 2>/dev/null || true
        sleep 2
        print_success "$service_name processes đã được dừng"
    else
        print_status "Không có $service_name process nào đang chạy trên port $port"
    fi
}

# Function to start .NET API
start_api() {
    print_status "Đang khởi động .NET API server..."
    cd /Users/techmax/Documents/hcm-chatbot/dotnet-api/hcm-chatbot-api

    # Build project trước khi start
    print_status "Building .NET project..."
    dotnet build Web_API/Web_API.csproj

    # Start API server in background
    print_status "Starting API server trên port 9000..."
    nohup dotnet run --project Web_API/Web_API.csproj --urls="http://localhost:9000" > api.log 2>&1 &

    # Wait a moment for startup
    sleep 3

    # Check if API is running
    if curl -s http://localhost:9000/health >/dev/null 2>&1 || lsof -ti:9000 >/dev/null 2>&1; then
        print_success ".NET API server đã khởi động thành công trên port 9000"
        print_status "Logs: tail -f /Users/techmax/Documents/hcm-chatbot/dotnet-api/hcm-chatbot-api/api.log"
    else
        print_error "Không thể khởi động .NET API server"
        return 1
    fi
}

# Function to start Python AI service
start_ai() {
    print_status "Đang khởi động Python AI service..."
    cd /Users/techmax/Documents/hcm-chatbot/backend

    # Activate virtual environment và start service
    print_status "Activating Python virtual environment..."
    source venv/bin/activate

    print_status "Starting AI service trên port 8000..."
    nohup python -m uvicorn app.main:app --host 0.0.0.0 --port 8000 --reload > ai.log 2>&1 &

    # Wait a moment for startup
    sleep 3

    # Check if AI service is running
    if curl -s http://localhost:8000/health >/dev/null 2>&1 || lsof -ti:8000 >/dev/null 2>&1; then
        print_success "Python AI service đã khởi động thành công trên port 8000"
        print_status "Logs: tail -f /Users/techmax/Documents/hcm-chatbot/backend/ai.log"
    else
        print_error "Không thể khởi động Python AI service"
        return 1
    fi
}

# Function to restart API
restart_api() {
    print_status "=== RESTARTING .NET API ==="
    kill_port 9000 ".NET API"
    start_api
}

# Function to restart AI
restart_ai() {
    print_status "=== RESTARTING PYTHON AI ==="
    kill_port 8000 "Python AI"
    start_ai
}

# Function to restart all services
restart_all() {
    print_status "=== RESTARTING ALL SERVICES ==="

    # Stop all services
    kill_port 9000 ".NET API"
    kill_port 8000 "Python AI"

    print_status "Chờ 3 giây để đảm bảo ports được giải phóng..."
    sleep 3

    # Start all services
    start_ai &
    sleep 2  # Start AI first, then API
    start_api &

    wait  # Wait for both background processes

    print_success "=== TẤT CẢ SERVICES ĐÃ ĐƯỢC RESTART ==="
    print_status "API Status: http://localhost:9000"
    print_status "AI Status: http://localhost:8000"
}

# Function to show status
show_status() {
    print_status "=== SERVICE STATUS ==="

    # Check .NET API
    if lsof -ti:9000 >/dev/null 2>&1; then
        print_success ".NET API: ✓ Running on port 9000"
    else
        print_warning ".NET API: ✗ Not running"
    fi

    # Check Python AI
    if lsof -ti:8000 >/dev/null 2>&1; then
        print_success "Python AI: ✓ Running on port 8000"
    else
        print_warning "Python AI: ✗ Not running"
    fi
}

# Function to show help
show_help() {
    echo "HCM Chatbot Service Management Script"
    echo ""
    echo "Usage: $0 [COMMAND]"
    echo ""
    echo "Commands:"
    echo "  api     Restart .NET API service only"
    echo "  ai      Restart Python AI service only"
    echo "  all     Restart all services (default)"
    echo "  status  Show current service status"
    echo "  stop    Stop all services"
    echo "  help    Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0              # Restart all services"
    echo "  $0 api          # Restart only API"
    echo "  $0 ai           # Restart only AI service"
    echo "  $0 status       # Check service status"
}

# Function to stop all services
stop_all() {
    print_status "=== STOPPING ALL SERVICES ==="
    kill_port 9000 ".NET API"
    kill_port 8000 "Python AI"
    print_success "Tất cả services đã được dừng"
}

# Main script logic
case "${1:-all}" in
    "api")
        restart_api
        ;;
    "ai")
        restart_ai
        ;;
    "all")
        restart_all
        ;;
    "status")
        show_status
        ;;
    "stop")
        stop_all
        ;;
    "help"|"-h"|"--help")
        show_help
        ;;
    *)
        print_error "Unknown command: $1"
        show_help
        exit 1
        ;;
esac