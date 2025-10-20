# 🖼️ Hướng dẫn Setup Google Custom Search API cho Image Search

## 📋 Tổng quan

Chức năng tìm kiếm ảnh sử dụng **Google Custom Search API** để tìm ảnh thật trên Google Images. Khi user hỏi "cho tôi ảnh Bác Hồ ở Pháp", chatbot sẽ tìm và hiển thị ảnh thật từ Google.

## 🔑 Bước 1: Lấy Google Custom Search API Key

### 1.1. Tạo Google Cloud Project

1. Truy cập: https://console.cloud.google.com/
2. Đăng nhập bằng tài khoản Google
3. Click "Create Project" hoặc chọn project có sẵn
4. Đặt tên project (VD: "HCM-Chatbot-Image-Search")
5. Click "Create"

### 1.2. Bật Custom Search API

1. Trong Google Cloud Console, vào **APIs & Services** > **Library**
2. Tìm kiếm: "Custom Search API"
3. Click vào "Custom Search API"
4. Click nút **"Enable"**

### 1.3. Tạo API Credentials

1. Vào **APIs & Services** > **Credentials**
2. Click **"+ Create Credentials"** > **"API Key"**
3. Copy API Key (dạng: `AIzaSyXXXXXXXXXXXXXXXXXXXXXXXXX`)
4. (Tùy chọn) Click "Restrict Key" để bảo mật:
   - **API restrictions**: Chọn "Custom Search API"
   - **Application restrictions**: Chọn "None" (cho development) hoặc "IP addresses" (cho production)
5. Click "Save"

## 🔍 Bước 2: Tạo Custom Search Engine

### 2.1. Tạo Search Engine

1. Truy cập: https://programmablesearchengine.google.com/
2. Click **"Add"** hoặc **"Get Started"**
3. Điền thông tin:
   - **Search engine name**: "HCM Image Search"
   - **What to search**: Chọn **"Search the entire web"**
   - **Image search**: Bật **ON** (quan trọng!)
   - **SafeSearch**: Bật **ON** (lọc nội dung nhạy cảm)
4. Click **"Create"**

### 2.2. Lấy Search Engine ID

1. Sau khi tạo xong, click vào search engine vừa tạo
2. Vào tab **"Overview"** hoặc **"Setup"**
3. Tìm **"Search engine ID"** (cx parameter)
4. Copy Search Engine ID (dạng: `a1234567890abcdef`)

## 🔐 Bước 3: Cấu hình Backend

### 3.1. Tạo file .env trong backend/

```bash
cd backend
touch .env  # hoặc tạo file mới nếu chưa có
```

### 3.2. Thêm API credentials vào .env

Mở file `backend/.env` và thêm:

```bash
# Gemini API (đã có sẵn)
GEMINI_API_KEY=your_gemini_api_key_here

# Google Custom Search API (mới thêm)
GOOGLE_CUSTOM_SEARCH_API_KEY=AIzaSyXXXXXXXXXXXXXXXXXXXXXXXXX
GOOGLE_SEARCH_ENGINE_ID=a1234567890abcdef
```

**Lưu ý:**
- Thay `AIzaSyXXXXXXXXXXXXXXXXXXXXXXXXX` bằng API Key từ Bước 1.3
- Thay `a1234567890abcdef` bằng Search Engine ID từ Bước 2.2

## 🧪 Bước 4: Test chức năng

### 4.1. Khởi động lại Backend

```bash
# macOS / Linux
cd backend
source venv/bin/activate
uvicorn app.main:app --reload --host 0.0.0.0 --port 8000

# Windows
cd backend
.\venv\Scripts\activate
uvicorn app.main:app --reload --host 0.0.0.0 --port 8000
```

### 4.2. Test trực tiếp Python API

```bash
curl -X POST "http://localhost:8000/search-image" \
  -H "Content-Type: application/json" \
  -d '{
    "query": "Ho Chi Minh in France",
    "num_results": 5
  }'
```

### 4.3. Test qua Frontend

1. Khởi động toàn bộ hệ thống: `./start-all.sh`
2. Truy cập: http://localhost:3000/chat.html
3. Đăng nhập
4. Gửi tin nhắn: **"Cho tôi ảnh Bác Hồ ở Pháp"**
5. Chatbot sẽ hiển thị gallery ảnh từ Google

## 💰 Quota & Giới hạn

### Free Tier (miễn phí)

- **100 searches/day** miễn phí
- **10,000 searches/month** tối đa cho free tier
- Không cần thẻ tín dụng

### Paid Tier (nếu cần nhiều hơn)

- **$5 per 1,000 queries** sau 100 queries đầu tiên mỗi ngày
- Cần enable billing trong Google Cloud

### Kiểm tra Usage

1. Vào Google Cloud Console
2. **APIs & Services** > **Dashboard**
3. Click "Custom Search API"
4. Xem metrics và quota

## 🛡️ Bảo mật

### Recommendations

1. **Restrict API Key**:
   - Chỉ cho phép Custom Search API
   - Giới hạn IP addresses trong production

2. **Environment Variables**:
   - Không commit file `.env` vào Git
   - File `.gitignore` đã bao gồm `.env`

3. **Rate Limiting**:
   - Backend tự động giới hạn 10 ảnh/request
   - Frontend chỉ gọi khi user yêu cầu

## 🔧 Troubleshooting

### Lỗi: "API Key not valid"

- Kiểm tra API Key đã copy đúng chưa
- Kiểm tra đã Enable Custom Search API chưa
- Kiểm tra API restrictions (nếu có)

### Lỗi: "Invalid Search Engine ID"

- Kiểm tra Search Engine ID (cx parameter)
- Đảm bảo đã bật "Image search" trong search engine settings

### Không tìm thấy ảnh

- Kiểm tra từ khóa tìm kiếm
- Backend có fallback images từ Wikipedia
- Kiểm tra logs: `tail -f logs/python-ai.log`

### Quota exceeded

- Free tier chỉ có 100 searches/day
- Chờ 24h để quota reset
- Hoặc upgrade lên paid tier

## 📚 Tham khảo

- [Custom Search JSON API Documentation](https://developers.google.com/custom-search/v1/overview)
- [Programmable Search Engine Help](https://support.google.com/programmable-search/)
- [Google Cloud Pricing](https://developers.google.com/custom-search/v1/overview#pricing)

## ✅ Checklist

- [ ] Tạo Google Cloud Project
- [ ] Enable Custom Search API
- [ ] Tạo API Key
- [ ] Tạo Custom Search Engine (bật Image search)
- [ ] Lấy Search Engine ID
- [ ] Thêm credentials vào `backend/.env`
- [ ] Khởi động lại backend
- [ ] Test với `curl`
- [ ] Test trên frontend

---

**Chúc mừng! 🎉** Bạn đã setup xong chức năng tìm kiếm ảnh. Bây giờ chatbot có thể tìm và hiển thị ảnh thật từ Google Images!
