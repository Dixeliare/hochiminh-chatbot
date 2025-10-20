"""
PYTHON AI BACKEND - HCM THOUGHT CHATBOT
Sử dụng FastAPI để tạo REST API cho AI chatbot
Tích hợp RAG (Retrieval-Augmented Generation) với Gemini AI
"""

# Import các thư viện cần thiết
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from .services.enhanced_rag_service import EnhancedRAGService
from .services.image_search_service import ImageSearchService

# ===== KHỞI TẠO FASTAPI APPLICATION =====
app = FastAPI(title="Enhanced HCM Thought Chatbot API", version="2.0.0")

# ===== CẤU HÌNH CORS =====
# Cho phép .NET API (localhost:9000) gọi Python API này
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # Trong production nên giới hạn origins
    allow_credentials=True,
    allow_methods=["*"],  # Cho phép tất cả HTTP methods
    allow_headers=["*"],  # Cho phép tất cả headers
)

# ===== KHỞI TẠO AI SERVICE =====
# Enhanced RAG service - kết hợp tìm kiếm tri thức và tạo văn bản
rag_service = EnhancedRAGService()
# Image search service - tìm kiếm ảnh trên Google
image_search_service = ImageSearchService()

# ===== DATA MODELS CHO API =====

class QuestionRequest(BaseModel):
    """Model cho request từ .NET API"""
    question: str  # Câu hỏi từ người dùng

class EnhancedChatResponse(BaseModel):
    """Model cho response trả về .NET API"""
    answer: str  # Câu trả lời từ AI
    sources: list = []  # Danh sách nguồn tham khảo
    confidence: int = 0  # Độ tin cậy (0-100)
    last_updated: str = None  # Thời gian cập nhật knowledge base

class ImageSearchRequest(BaseModel):
    """Model cho request tìm kiếm ảnh"""
    query: str  # Từ khóa tìm kiếm (VD: "Hồ Chí Minh ở Pháp")
    num_results: int = 5  # Số lượng ảnh (mặc định 5)

class ImageSearchResponse(BaseModel):
    """Model cho response tìm kiếm ảnh"""
    images: list = []  # Danh sách ảnh
    query: str  # Từ khóa đã tìm
    total: int = 0  # Tổng số ảnh tìm được

# ===== LIFECYCLE EVENTS =====

@app.on_event("startup")
async def startup_event():
    """
    Khởi tạo knowledge base khi server start
    Tải và xử lý tất cả tài liệu về tư tưởng Hồ Chí Minh
    """
    print("🚀 Starting Enhanced HCM Chatbot API...")
    rag_service.update_knowledge_base(force_update=True)
    print("✅ Enhanced Server ready!")

# ===== API ENDPOINTS =====

@app.get("/")
async def root():
    """Root endpoint - thông tin cơ bản về API"""
    return {"message": "Enhanced HCM Thought Chatbot API", "version": "2.0.0", "status": "running"}

@app.get("/health")
async def health_check():
    """Health check endpoint - kiểm tra tình trạng AI service"""
    stats = rag_service.get_stats()
    return {"status": "healthy", "stats": stats}

@app.post("/chat", response_model=EnhancedChatResponse)
async def enhanced_chat(request: QuestionRequest):
    """
    MAIN CHAT ENDPOINT - Xử lý câu hỏi và trả về phản hồi AI

    Quy trình:
    1. Validate input
    2. Sử dụng RAG service để tìm kiếm tri thức và tạo câu trả lời
    3. Nếu RAG thất bại, fallback về Gemini trực tiếp
    4. Trả về response với sources và confidence score
    """
    try:
        # ===== VALIDATION =====
        if not request.question.strip():
            raise HTTPException(status_code=400, detail="Câu hỏi không được để trống")

        # ===== XỬ LÝ VỚI RAG SERVICE =====
        try:
            # Sử dụng Enhanced RAG để tạo response với nguồn tham khảo
            result = rag_service.generate_response_with_sources(request.question)

            return EnhancedChatResponse(
                answer=result["answer"],  # Câu trả lời chi tiết
                sources=result["sources"],  # Nguồn tham khảo có cấu trúc
                confidence=result["confidence"],  # Độ tin cậy
                last_updated=result.get("last_updated", "2024-01-01")
            )

        except Exception as rag_error:
            print(f"RAG service error: {rag_error}")

            # ===== FALLBACK: SỬ DỤNG GEMINI TRỰC TIẾP =====
            # Khi RAG service gặp lỗi (thường do quota API), dùng Gemini trực tiếp
            import google.generativeai as genai
            import os

            genai.configure(api_key=os.getenv("GEMINI_API_KEY"))
            model = genai.GenerativeModel('gemini-2.5-flash')

            # Prompt template cho fallback response
            prompt = f"""
            Câu hỏi về tư tưởng Hồ Chí Minh: {request.question}

            Hãy trả lời dựa trên kiến thức về tư tưởng Hồ Chí Minh, bao gồm:
            - Độc lập dân tộc
            - Chủ nghĩa xã hội
            - Đạo đức cách mạng
            - Dân chủ
            - Đoàn kết dân tộc
            """

            response = model.generate_content(prompt)

            return EnhancedChatResponse(
                answer=response.text,
                sources=["Kiến thức chung về tư tưởng Hồ Chí Minh"],  # Nguồn generic
                confidence=75,  # Độ tin cậy thấp hơn vì không có RAG
                last_updated="2024-01-01"
            )

    except Exception as e:
        print(f"Error in enhanced chat endpoint: {e}")
        raise HTTPException(status_code=500, detail="Lỗi server, vui lòng thử lại")

@app.post("/search-image", response_model=ImageSearchResponse)
async def search_image(request: ImageSearchRequest):
    """
    IMAGE SEARCH ENDPOINT - Tìm kiếm ảnh trên Google Images

    Quy trình:
    1. Validate input (từ khóa tìm kiếm)
    2. Gọi Google Custom Search API
    3. Trả về danh sách ảnh với URL, title, thumbnail

    Args:
        request: ImageSearchRequest với query và num_results

    Returns:
        ImageSearchResponse với danh sách ảnh
    """
    try:
        # ===== VALIDATION =====
        if not request.query.strip():
            raise HTTPException(status_code=400, detail="Từ khóa tìm kiếm không được để trống")

        # Giới hạn số lượng ảnh
        num_results = min(request.num_results, 10)

        # ===== TÌM KIẾM ẢNH =====
        images = image_search_service.search_images(request.query, num_results)

        return ImageSearchResponse(
            images=images,
            query=request.query,
            total=len(images)
        )

    except Exception as e:
        print(f"Error in image search endpoint: {e}")
        raise HTTPException(status_code=500, detail="Lỗi khi tìm kiếm ảnh, vui lòng thử lại")

# ===== SERVER ENTRY POINT =====
if __name__ == "__main__":
    """
    Chạy server trực tiếp (cho development)
    Trong production, sử dụng: uvicorn app.main:app --host 0.0.0.0 --port 8000
    """
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
