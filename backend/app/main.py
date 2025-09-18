from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from .services.enhanced_rag_service import EnhancedRAGService

app = FastAPI(title="Enhanced HCM Thought Chatbot API", version="2.0.0")

# CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Khởi tạo Enhanced RAG service
rag_service = EnhancedRAGService()

# Models
class QuestionRequest(BaseModel):
    question: str

class EnhancedChatResponse(BaseModel):
    answer: str
    sources: list = []
    confidence: int = 0
    last_updated: str = None

@app.on_event("startup")
async def startup_event():
    """Khởi tạo enhanced knowledge base"""
    print("🚀 Starting Enhanced HCM Chatbot API...")
    rag_service.update_knowledge_base(force_update=True)
    print("✅ Enhanced Server ready!")

@app.get("/")
async def root():
    return {"message": "Enhanced HCM Thought Chatbot API", "version": "2.0.0", "status": "running"}

@app.get("/health")
async def health_check():
    stats = rag_service.get_stats()
    return {"status": "healthy", "stats": stats}

@app.post("/chat", response_model=EnhancedChatResponse)
async def enhanced_chat(request: QuestionRequest):
    """Enhanced chat endpoint với source citation"""
    try:
        if not request.question.strip():
            raise HTTPException(status_code=400, detail="Câu hỏi không được để trống")

        # Generate enhanced response
        result = rag_service.generate_response_with_sources(request.question)
        
        return EnhancedChatResponse(
            answer=result["answer"],
            sources=result["sources"],
            confidence=result["confidence"],
            last_updated=result.get("last_updated")
        )

    except Exception as e:
        print(f"Error in enhanced chat endpoint: {e}")
        raise HTTPException(status_code=500, detail="Lỗi server, vui lòng thử lại")

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
