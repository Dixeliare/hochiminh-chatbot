# HCM Chatbot Service Management Scripts

## Scripts Available

### 1. `start-all.sh` - Khởi động tất cả services
Khởi động cả Python AI service và .NET API từ đầu.

```bash
./start-all.sh
```

**Chức năng:**
- Kiểm tra và tạo Python virtual environment nếu cần
- Build .NET project
- Khởi động Python AI service trên port 8000
- Khởi động .NET API trên port 9000
- Tạo log files: `api.log` và `ai.log`

### 2. `restart-services.sh` - Quản lý services
Script chính để restart và quản lý services.

#### Commands:

```bash
# Restart tất cả services (mặc định)
./restart-services.sh
./restart-services.sh all

# Restart chỉ .NET API
./restart-services.sh api

# Restart chỉ Python AI service
./restart-services.sh ai

# Kiểm tra trạng thái services
./restart-services.sh status

# Dừng tất cả services
./restart-services.sh stop

# Xem hướng dẫn
./restart-services.sh help
```

## Features

### ✅ Tự động kill processes trên ports
- Sử dụng `lsof` để tìm và kill processes
- An toàn với error handling

### ✅ Colored output
- Xanh: Thành công ✓
- Vàng: Cảnh báo ⚠️
- Đỏ: Lỗi ❌
- Xanh dương: Thông tin ℹ️

### ✅ Health checks
- Kiểm tra ports có đang được sử dụng không
- Verify services khởi động thành công

### ✅ Background processes
- Services chạy dưới nền với `nohup`
- Log files để debug

### ✅ Build automation
- Tự động build .NET project trước khi start
- Activate Python virtual environment

## Log Files

```bash
# Xem .NET API logs
tail -f /Users/techmax/Documents/hcm-chatbot/dotnet-api/hcm-chatbot-api/api.log

# Xem Python AI logs
tail -f /Users/techmax/Documents/hcm-chatbot/backend/ai.log
```

## Typical Usage

### Development workflow:
```bash
# Lần đầu khởi động
./start-all.sh

# Khi có thay đổi code, restart service cụ thể
./restart-services.sh api      # Sau khi sửa .NET code
./restart-services.sh ai       # Sau khi sửa Python code

# Kiểm tra trạng thái
./restart-services.sh status

# Restart tất cả khi cần
./restart-services.sh all
```

### Debug problems:
```bash
# Dừng tất cả
./restart-services.sh stop

# Kiểm tra logs
tail -f api.log
tail -f ai.log

# Khởi động lại
./start-all.sh
```

## Ports

- **Python AI Service**: `http://localhost:8000`
- **.NET API Service**: `http://localhost:9000`
- **Frontend**: Mở `chat.html` hoặc `admin.html` trong browser

## Troubleshooting

### "Address already in use"
```bash
# Sử dụng script để kill processes
./restart-services.sh stop
./start-all.sh
```

### Services không start được
```bash
# Kiểm tra logs
tail -f api.log
tail -f ai.log

# Kiểm tra dependencies
cd backend && source venv/bin/activate && pip install -r requirements.txt
cd dotnet-api/hcm-chatbot-api && dotnet restore
```

### Port conflicts
```bash
# Kiểm tra ports đang sử dụng
lsof -i :8000
lsof -i :9000

# Kill specific processes
kill -9 <PID>
```