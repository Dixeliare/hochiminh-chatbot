# 🖼️ Setup Pexels API - MIỄN PHÍ UNLIMITED!

## ⚡ Tại sao chọn Pexels?

- ✅ **MIỄN PHÍ UNLIMITED** - không giới hạn số lượng requests
- ✅ **Dễ setup** - chỉ 2 phút!
- ✅ **Không cần thẻ tín dụng**
- ✅ **Ảnh chất lượng cao**
- ❌ Nhược điểm: Ít ảnh lịch sử về Bác Hồ (vì là stock photos)

**👉 Nếu cần ảnh lịch sử chính xác về Bác Hồ, dùng Google Custom Search (xem GOOGLE_IMAGE_SEARCH_SETUP.md)**

## 🚀 Cách setup Pexels API (2 phút)

### Bước 1: Đăng ký tài khoản Pexels

1. Truy cập: https://www.pexels.com/api/
2. Click **"Get Started"** hoặc **"Sign Up"**
3. Đăng ký bằng email hoặc Google

### Bước 2: Lấy API Key

1. Sau khi đăng nhập, bạn sẽ thấy **Your API Key** ngay trên dashboard
2. Copy API Key (dạng: `563492ad6f91700001000001...`)

**VẬY LÀ XONG!** 🎉

### Bước 3: Thêm vào .env

Mở file `backend/.env` và thêm:

```bash
# Pexels API - Miễn phí unlimited
PEXELS_API_KEY=563492ad6f91700001000001YOUR_KEY_HERE
```

### Bước 4: Khởi động lại backend

```bash
cd backend
source venv/bin/activate  # macOS/Linux
# hoặc: .\venv\Scripts\activate  # Windows

uvicorn app.main:app --reload --host 0.0.0.0 --port 8000
```

## 🧪 Test thử

### Test trực tiếp Python API:

```bash
curl -X POST "http://localhost:8000/search-image" \
  -H "Content-Type: application/json" \
  -d '{
    "query": "Vietnam landscape",
    "num_results": 5
  }'
```

### Test trên Frontend:

1. Truy cập: http://localhost:3000/chat.html
2. Gửi: **"Cho tôi ảnh về Việt Nam"**
3. Hoặc: **"Tìm ảnh phong cảnh Việt Nam"**

## 📊 So sánh APIs

| Feature | Pexels | Google Custom Search |
|---------|--------|---------------------|
| **Miễn phí** | ✅ Unlimited | ⚠️ 100/day |
| **Setup** | 🟢 Dễ (2 phút) | 🟡 Khó (10 phút) |
| **Ảnh lịch sử Bác Hồ** | ❌ Ít | ✅ Nhiều |
| **Ảnh phong cảnh VN** | ✅ Đẹp | ✅ Có |
| **Thẻ tín dụng** | ❌ Không cần | ❌ Không cần (free tier) |

## 💡 Khuyến nghị

### Dùng Pexels khi:
- Tìm ảnh phong cảnh, cảnh đẹp Việt Nam
- Ảnh chung chung về văn hóa, con người
- Cần unlimited searches

### Dùng Google Custom Search khi:
- Cần ảnh lịch sử chính xác về Bác Hồ
- Tìm ảnh sự kiện lịch sử cụ thể
- OK với 100 searches/day

### Dùng cả hai (recommended!):
Thêm cả 2 API keys vào `.env`:

```bash
# Google Custom Search - cho ảnh lịch sử
GOOGLE_CUSTOM_SEARCH_API_KEY=your_google_key
GOOGLE_SEARCH_ENGINE_ID=your_engine_id

# Pexels - cho ảnh general
PEXELS_API_KEY=your_pexels_key
```

**Backend sẽ tự động:**
1. Thử Google trước (ảnh lịch sử chính xác)
2. Nếu Google hết quota → dùng Pexels
3. Nếu cả 2 đều không có → dùng ảnh mẫu

## 🔒 Rate Limits

- **Free plan**: 200 requests/hour
- **Unlimited requests** - không giới hạn tổng số
- Không cần payment method

## 📚 Docs

- API Documentation: https://www.pexels.com/api/documentation/
- Image Search Endpoint: https://www.pexels.com/api/documentation/#photos-search

## ✅ Checklist

- [ ] Đăng ký tài khoản Pexels
- [ ] Lấy API Key
- [ ] Thêm `PEXELS_API_KEY` vào `backend/.env`
- [ ] Khởi động lại backend
- [ ] Test với frontend

---

**🎉 Chúc mừng!** Bạn đã có unlimited image search miễn phí!
