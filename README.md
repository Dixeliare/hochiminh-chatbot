# 🇻🇳 HCM Chatbot - Tư Tưởng Hồ Chí Minh

> Chatbot AI thông minh về tư tưởng và di sản của Chủ tịch Hồ Chí Minh

![Vietnam Flag](https://img.shields.io/badge/🇻🇳-Vietnam-red?style=for-the-badge)
![Python](https://img.shields.io/badge/Python-3.8+-blue?style=for-the-badge&logo=python)
![FastAPI](https://img.shields.io/badge/FastAPI-0.104+-green?style=for-the-badge&logo=fastapi)
![JavaScript](https://img.shields.io/badge/JavaScript-ES6+-yellow?style=for-the-badge&logo=javascript)

## 📖 Mô tả dự án

HCM Chatbot là một ứng dụng AI tiên tiến được xây dựng để trao đổi và học hỏi về tư tưởng của Chủ tịch Hồ Chí Minh. Dự án sử dụng công nghệ RAG (Retrieval-Augmented Generation) kết hợp với Gemini AI để cung cấp câu trả lời chính xác và có nguồn gốc.

### ✨ Tính năng chính

- 🤖 **AI Chat thông minh** với RAG system
- 📚 **Knowledge Base** về tư tưởng Hồ Chí Minh
- 🔍 **Source Citation** cho mọi câu trả lời
- 📱 **Responsive Design** trên mọi thiết bị
- 🎨 **UI/UX hiện đại** với theme cờ Việt Nam
- ⚡ **Real-time Chat** với typing indicators
- 📊 **Confidence Score** cho độ tin cậy
- 🔄 **Auto Health Check** cho backend

## 🏗️ Kiến trúc hệ thống

```
hcm-chatbot/
├── backend/                 # FastAPI Backend
│   ├── app/
│   │   ├── main.py         # API endpoints
│   │   └── services/       # Business logic
│   │       ├── enhanced_rag_service.py
│   │       ├── vector_store.py
│   │       └── web_data_collector.py
│   ├── requirements.txt    # Python dependencies
│   ├── .env.example       # Environment template
│   └── venv/              # Virtual environment
├── frontend/               # Modern Web UI
│   ├── index.html         # Main interface
│   ├── styles.css         # Modern styling
│   ├── script.js          # Chat functionality
│   └── README.md          # Frontend docs
├── data/                  # Data storage
├── scripts/               # Automation scripts
└── README.md              # This file
```

## 🚀 Cài đặt nhanh

### Yêu cầu hệ thống

- **Python** 3.8+
- **Node.js** (tùy chọn, cho live server)
- **Git**
- **API Keys**: Gemini AI, Pinecone

### 1️⃣ Clone dự án

```bash
git clone https://github.com/username/hcm-chatbot.git
cd hcm-chatbot
```

### 2️⃣ Cài đặt Backend

```bash
# Tạo virtual environment
cd backend
python -m venv venv

# Kích hoạt virtual environment
# Trên macOS/Linux:
source venv/bin/activate
# Trên Windows:
# venv\Scripts\activate

# Cài đặt dependencies
pip install -r requirements.txt

# Cấu hình environment
cp .env.example .env
# Chỉnh sửa .env với API keys của bạn
```

### 3️⃣ Cấu hình API Keys

Mở file `.env` và thêm API keys:

```env
GEMINI_API_KEY=your_gemini_api_key_here
PINECONE_API_KEY=your_pinecone_api_key_here
```

**Cách lấy API Keys:**
- **Gemini AI**: https://ai.google.dev/
- **Pinecone**: https://www.pinecone.io/

## 🎮 Cách sử dụng

### Chạy bằng Scripts tự động

Chúng tôi đã tạo các script tiện lợi để bạn dễ dàng sử dụng:

```bash
# Khởi động toàn bộ hệ thống
./scripts/start.sh

# Chỉ chạy backend
./scripts/start-backend.sh

# Chỉ chạy frontend
./scripts/start-frontend.sh

# Dừng hệ thống
./scripts/stop.sh
```

### Chạy thủ công

#### Backend (Terminal 1)

```bash
cd backend
source venv/bin/activate
python -m uvicorn app.main:app --host 0.0.0.0 --port 8000 --reload
```

#### Frontend (Terminal 2)

```bash
cd frontend

# Cách 1: Mở trực tiếp trong browser
open index.html

# Cách 2: Sử dụng Python HTTP Server
python -m http.server 3000

# Cách 3: Sử dụng Live Server (VS Code extension)
# Right-click index.html -> "Open with Live Server"
```

### 🌐 Truy cập ứng dụng

- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:8000
- **API Docs**: http://localhost:8000/docs
- **Health Check**: http://localhost:8000/health

## 📱 Hướng dẫn sử dụng

### 💬 Chat với Bot

1. **Mở giao diện** tại http://localhost:3000
2. **Chờ kết nối** - status sẽ hiện "Đã kết nối" màu xanh
3. **Nhập câu hỏi** về tư tưởng Hồ Chí Minh
4. **Xem kết quả** với sources và confidence score

### 🎯 Gợi ý câu hỏi

Thử các câu hỏi mẫu:

- "Tư tưởng Hồ Chí Minh về độc lập dân tộc là gì?"
- "Quan điểm của Bác Hồ về đạo đức cách mạng?"
- "Tư tưởng Hồ Chí Minh về giáo dục và văn hóa?"
- "Tầm nhìn của Bác Hồ về một Việt Nam thống nhất?"

### 🔍 Hiểu kết quả

Mỗi câu trả lời bao gồm:
- **Answer**: Nội dung trả lời chính
- **Sources**: Nguồn tham khảo từ knowledge base
- **Confidence**: Độ tin cậy từ 0-100%
- **Last Updated**: Thời gian cập nhật dữ liệu

## 🛠️ Scripts tự động

### `/scripts/start.sh` - Khởi động hệ thống

```bash
#!/bin/bash
echo "🚀 Starting HCM Chatbot System..."

# Check if backend directory exists
if [ ! -d "backend" ]; then
    echo "❌ Backend directory not found!"
    exit 1
fi

# Start backend
echo "📡 Starting backend server..."
cd backend

# Check if virtual environment exists
if [ ! -d "venv" ]; then
    echo "🔧 Creating virtual environment..."
    python -m venv venv
    source venv/bin/activate
    pip install -r requirements.txt
else
    source venv/bin/activate
fi

# Check if .env exists
if [ ! -f ".env" ]; then
    echo "⚠️  .env file not found! Copying from .env.example..."
    cp .env.example .env
    echo "📝 Please edit .env file with your API keys"
    read -p "Press Enter after editing .env file..."
fi

# Start backend in background
nohup python -m uvicorn app.main:app --host 0.0.0.0 --port 8000 --reload > ../logs/backend.log 2>&1 &
BACKEND_PID=$!
echo $BACKEND_PID > ../logs/backend.pid

echo "✅ Backend started (PID: $BACKEND_PID)"
echo "📊 Backend API: http://localhost:8000"
echo "📚 API Docs: http://localhost:8000/docs"

# Wait for backend to start
sleep 5

# Start frontend
cd ../frontend
echo "🎨 Starting frontend server..."

# Check if Python HTTP server is available
if command -v python3 &> /dev/null; then
    nohup python3 -m http.server 3000 > ../logs/frontend.log 2>&1 &
    FRONTEND_PID=$!
    echo $FRONTEND_PID > ../logs/frontend.pid
    echo "✅ Frontend started (PID: $FRONTEND_PID)"
    echo "🌐 Frontend: http://localhost:3000"
else
    echo "⚠️  Python not found. Please open frontend/index.html manually"
fi

echo ""
echo "🎉 HCM Chatbot is ready!"
echo "🌐 Open: http://localhost:3000"
echo "🛑 To stop: ./scripts/stop.sh"
```

### `/scripts/stop.sh` - Dừng hệ thống

```bash
#!/bin/bash
echo "🛑 Stopping HCM Chatbot System..."

# Create logs directory if it doesn't exist
mkdir -p logs

# Stop backend
if [ -f "logs/backend.pid" ]; then
    BACKEND_PID=$(cat logs/backend.pid)
    if ps -p $BACKEND_PID > /dev/null; then
        kill $BACKEND_PID
        echo "✅ Backend stopped (PID: $BACKEND_PID)"
    else
        echo "⚠️  Backend process not found"
    fi
    rm -f logs/backend.pid
fi

# Stop frontend
if [ -f "logs/frontend.pid" ]; then
    FRONTEND_PID=$(cat logs/frontend.pid)
    if ps -p $FRONTEND_PID > /dev/null; then
        kill $FRONTEND_PID
        echo "✅ Frontend stopped (PID: $FRONTEND_PID)"
    else
        echo "⚠️  Frontend process not found"
    fi
    rm -f logs/frontend.pid
fi

# Kill any remaining uvicorn processes
pkill -f "uvicorn app.main:app" 2>/dev/null
pkill -f "python.*http.server.*3000" 2>/dev/null

echo "🏁 All services stopped!"
```

## 🧪 Testing & Development

### Kiểm tra API

```bash
# Health check
curl http://localhost:8000/health

# Test chat endpoint
curl -X POST "http://localhost:8000/chat" \
     -H "Content-Type: application/json" \
     -d '{"question": "Tư tưởng Hồ Chí Minh về độc lập dân tộc là gì?"}'
```

### Development Mode

```bash
# Backend với auto-reload
cd backend
source venv/bin/activate
uvicorn app.main:app --reload --host 0.0.0.0 --port 8000

# Frontend với live reload (VS Code)
# Install Live Server extension
# Right-click frontend/index.html -> "Open with Live Server"
```

## 🔧 Troubleshooting

### Lỗi thường gặp

#### 1. Backend không khởi động

```bash
# Kiểm tra Python version
python --version  # Cần >= 3.8

# Kiểm tra virtual environment
cd backend
source venv/bin/activate
pip list

# Reinstall dependencies
pip install -r requirements.txt --force-reinstall
```

#### 2. Lỗi API Keys

```bash
# Kiểm tra file .env
cat backend/.env

# Test API keys
python -c "
import os
from dotenv import load_dotenv
load_dotenv()
print('Gemini:', os.getenv('GEMINI_API_KEY')[:10] + '...')
print('Pinecone:', os.getenv('PINECONE_API_KEY')[:10] + '...')
"
```

#### 3. Frontend không kết nối Backend

- Kiểm tra backend đã chạy: http://localhost:8000/health
- Kiểm tra CORS settings trong `backend/app/main.py`
- Mở Developer Tools (F12) để xem lỗi JavaScript

#### 4. Port đã được sử dụng

```bash
# Kiểm tra port 8000
lsof -i :8000

# Kill process sử dụng port
kill -9 $(lsof -t -i:8000)

# Hoặc đổi port trong scripts
```

### Logs và Debugging

```bash
# Xem logs backend
tail -f logs/backend.log

# Xem logs frontend
tail -f logs/frontend.log

# Check processes
ps aux | grep -E "(uvicorn|python.*http.server)"
```

## 📊 API Documentation

### Endpoints

#### `GET /`
- **Mô tả**: Thông tin API
- **Response**: `{"message": "Enhanced HCM Thought Chatbot API", "version": "2.0.0", "status": "running"}`

#### `GET /health`
- **Mô tả**: Kiểm tra sức khỏe hệ thống
- **Response**: `{"status": "healthy", "stats": {...}}`

#### `POST /chat`
- **Mô tả**: Chat với AI
- **Request Body**:
  ```json
  {
    "question": "Câu hỏi của bạn"
  }
  ```
- **Response**:
  ```json
  {
    "answer": "Câu trả lời từ AI",
    "sources": ["Nguồn 1", "Nguồn 2"],
    "confidence": 85,
    "last_updated": "2024-01-01T00:00:00"
  }
  ```

## 🤝 Đóng góp

### Cách đóng góp

1. **Fork** project
2. **Clone** về máy local
3. **Tạo branch** mới: `git checkout -b feature/amazing-feature`
4. **Commit** changes: `git commit -m 'Add amazing feature'`
5. **Push** lên branch: `git push origin feature/amazing-feature`
6. **Tạo Pull Request**

### Coding Standards

- **Python**: PEP 8, type hints
- **JavaScript**: ES6+, modern syntax
- **CSS**: BEM methodology
- **Git**: Conventional commits

## 📄 License

Dự án này được phát hành dưới [MIT License](LICENSE).

## 👨‍💻 Tác giả

- **Developer**: Đào Xuân Long
- **Email**: daoxuanlong492004@gmail.com
- **GitHub**: Dixeliare

## 🙏 Acknowledgments

- **Chủ tịch Hồ Chí Minh** - Nguồn cảm hứng và kiến thức
- **Google Gemini AI** - AI technology
- **Pinecone** - Vector database
- **FastAPI** - Modern web framework
- **Cộng đồng Vietnam Developer** - Support và feedback

---

<div align="center">

**🇻🇳 Made with ❤️ for Vietnam 🇻🇳**

*"Không có gì quý hơn độc lập tự do"*

</div>
