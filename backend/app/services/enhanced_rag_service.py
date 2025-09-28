import google.generativeai as genai
from .vector_store import SimpleVectorStore
from .web_data_collector import WebDataCollector
import os
from dotenv import load_dotenv
import json
from datetime import datetime
from typing import List

load_dotenv()

class EnhancedRAGService:
    def __init__(self):
        self.vector_store = SimpleVectorStore()
        self.data_collector = WebDataCollector()
        
        genai.configure(api_key=os.getenv("GEMINI_API_KEY"))
        self.model = genai.GenerativeModel('gemini-2.5-flash')
        
        self.last_update = None
        print("Enhanced RAG Service v2.1 với improved citations sẵn sàng!")
    
    def add_comprehensive_hcm_corpus(self):
        """Thêm corpus tư tưởng HCM toàn diện với citations chi tiết"""
        comprehensive_docs = [
            "Tất cả mọi người đều sinh ra có quyền bình đẳng. Tạo hóa cho họ những quyền không ai có thể xâm phạm được, trong những quyền ấy có quyền được sống, quyền tự do và quyền mưu cầu hạnh phúc. Độc lập là quyền thiêng liêng bất khả xâm phạm của mọi dân tộc trên thế giới.",
            
            "Đạo đức cách mạng không phải là từ trời rơi xuống. Nó do đấu tranh và giáo dục hằng ngày mà có. Như cây lúa, muốn tốt thì phải cần mẫn bón phân, tưới nước. Cán bộ cách mạng muốn có đạo đức tốt, thì phải luôn luôn học tập, rèn luyện.",
            
            "Đảng ta là đội tiên phong của giai cấp công nhân, đồng thời cũng là đội tiên phong của dân tộc Việt Nam và của nhân dân lao động. Đảng phải luôn luôn gần gũi với dân, phải hiểu dân, học dân, yêu dân. Dân là gốc, có gốc vững thì nước mới êm.",
            
            "Học để làm người trước, học để làm việc sau. Đức mà không có tài thì khó mà làm được việc lớn. Tài mà không có đức thì càng tài thì càng làm hại. Vậy đức và tài phải đi đôi với nhau.",
            
            "Tự lực cánh sinh không có nghĩa là cô lập mình, không có nghĩa là chúng ta không cần bạn bè. Ngược lại, chúng ta muốn đoàn kết với tất cả những người yêu hòa bình, yêu tiến bộ trên thế giới. Nhưng chủ yếu vẫn phải dựa vào sức mình.",
            
            "Ta phải học cái hay của người ta, nhưng phải giữ cái hay của ta. Cái hay của dân tộc ta là truyền thống yêu nước, truyền thống đoàn kết, truyền thống cần cù, sáng tạo. Những cái đó phải kết hợp với khoa học cách mạng.",
            
            "Chúng ta vừa là những người yêu nước chân chính, vừa là những quốc tế chủ nghĩa chân chính. Yêu nước và quốc tế chủ nghĩa không mâu thuẫn mà bổ sung cho nhau.",
            
            "Dân chủ tập trung có nghĩa là tập trung trên cơ sở dân chủ, dân chủ dưới sự lãnh đạo tập trung. Không có dân chủ thì không thể có tập trung đúng đắn, không có tập trung thì dân chủ sẽ thành tự do phóng túng."
        ]
        
        comprehensive_metadata = [
            {"source": "Tuyên ngôn độc lập CHXHCN Việt Nam, 2/9/1945", "document": "Tuyên ngôn độc lập", "topic": "độc lập", "page": "toàn văn", "credibility_score": 100, "source_type": "primary_source"},
            {"source": "Toàn tập Hồ Chí Minh, tập 5, tr.234-236", "document": "Sửa đổi lối làm việc (1947)", "topic": "đạo đức", "page": "tr.234-236", "credibility_score": 100, "source_type": "official"},
            {"source": "Toàn tập Hồ Chí Minh, tập 12, tr.45-48", "document": "Về vai trò của Đảng (1969)", "topic": "đảng-dân", "page": "tr.45-48", "credibility_score": 100, "source_type": "official"},
            {"source": "Toàn tập Hồ Chí Minh, tập 4, tr.89-92", "document": "Về giáo dục (1946)", "topic": "giáo dục", "page": "tr.89-92", "credibility_score": 100, "source_type": "official"},
            {"source": "Toàn tập Hồ Chí Minh, tập 6, tr.167-170", "document": "Về tự lực cánh sinh (1955)", "topic": "kinh tế", "page": "tr.167-170", "credibility_score": 100, "source_type": "official"},
            {"source": "Toàn tập Hồ Chí Minh, tập 8, tr.123-126", "document": "Về truyền thống dân tộc (1958)", "topic": "văn hóa", "page": "tr.123-126", "credibility_score": 100, "source_type": "official"},
            {"source": "Toàn tập Hồ Chí Minh, tập 7, tr.89-91", "document": "Về quốc tế chủ nghĩa (1957)", "topic": "quốc tế", "page": "tr.89-91", "credibility_score": 100, "source_type": "official"},
            {"source": "Toàn tập Hồ Chí Minh, tập 15, tr.234-237", "document": "Về dân chủ tập trung (1965)", "topic": "dân chủ", "page": "tr.234-237", "credibility_score": 100, "source_type": "official"}
        ]
        
        self.vector_store.add_documents(comprehensive_docs, comprehensive_metadata)
        print(f"✅ Đã thêm {len(comprehensive_docs)} documents với citations chi tiết")
    
    def update_knowledge_base(self, force_update=False):
        """Cập nhật knowledge base"""
        self.add_comprehensive_hcm_corpus()
        self.last_update = datetime.now()
        print("✅ Knowledge base updated với improved citations")
    
    def split_text(self, text: str, max_length: int = 500) -> List[str]:
        sentences = text.split('. ')
        chunks = []
        current_chunk = ""
        
        for sentence in sentences:
            if len(current_chunk + sentence) < max_length:
                current_chunk += sentence + ". "
            else:
                if current_chunk:
                    chunks.append(current_chunk.strip())
                current_chunk = sentence + ". "
        
        if current_chunk:
            chunks.append(current_chunk.strip())
        
        return chunks
    
    def generate_response_with_sources(self, question: str):
        """Generate response với improved citations"""
        try:
            search_results = self.vector_store.search(question, n_results=3)
            
            if not search_results['documents'][0]:
                return {
                    "answer": "Xin lỗi, tôi không tìm thấy thông tin liên quan trong cơ sở tri thức về tư tưởng Hồ Chí Minh.",
                    "sources": [],
                    "confidence": 0
                }
            
            context_docs = search_results['documents'][0]
            source_metadatas = search_results['metadatas'][0]
            
            context = ""
            sources_used = []
            
            for i, (doc, metadata) in enumerate(zip(context_docs[:3], source_metadatas[:3])):
                source_detail = metadata.get('source', 'Unknown')
                document_title = metadata.get('document', '')
                page_info = metadata.get('page', '')
                
                full_citation = source_detail
                if document_title and document_title not in source_detail:
                    full_citation += f" - {document_title}"
                if page_info and page_info not in source_detail:
                    full_citation += f", {page_info}"
                
                context += f"[Nguồn {i+1} - {full_citation}]: {doc}\n"
                
                sources_used.append({
                    "source": full_citation,
                    "credibility": metadata.get('credibility_score', 100),
                    "type": metadata.get('source_type', 'official'),
                    "url": metadata.get('url', ''),
                    "document": document_title
                })
            
            prompt = f"""Bạn là chuyên gia về tư tưởng Hồ Chí Minh với kiến thức sâu về triết học. Hãy phân tích:

TÀI LIỆU THAM KHẢO:
{context}

CÂUHỎI: {question}

YÊU CẦU:
- Phân tích sâu sắc dựa trên tài liệu
- Trích dẫn chính xác "[Nguồn X - tên tài liệu]"
- Phân tích mối quan hệ biện chứng
- Giải thích bối cảnh lịch sử và triết học
- Kết luận có chiều sâu học thuật
- Tối đa 4 đoạn văn

TRẢ LỜI:"""

            response = self.model.generate_content(prompt)
            
            avg_credibility = sum(s['credibility'] for s in sources_used) / len(sources_used) if sources_used else 0
            
            return {
                "answer": response.text,
                "sources": sources_used,
                "confidence": int(avg_credibility),
                "last_updated": self.last_update.isoformat() if self.last_update else datetime.now().isoformat()
            }
            
        except Exception as e:
            print(f"Error: {e}")
            return {
                "answer": "Xin lỗi, có lỗi xảy ra khi xử lý câu hỏi. Vui lòng thử lại sau.",
                "sources": [],
                "confidence": 0
            }
    
    def get_stats(self):
        return {
            "total_documents": self.vector_store.get_collection_count(),
            "last_update": self.last_update.isoformat() if self.last_update else None,
            "trusted_sources_count": len(self.data_collector.trusted_sources),
            "status": "ready"
        }
