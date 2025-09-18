import os
import json
import numpy as np
import google.generativeai as genai
from dotenv import load_dotenv
from typing import List, Dict

load_dotenv()

class SimpleVectorStore:
    def __init__(self):
        # Khởi tạo Gemini
        api_key = os.getenv("GEMINI_API_KEY")
        if not api_key:
            raise ValueError("GEMINI_API_KEY không tìm thấy")
        
        genai.configure(api_key=api_key)
        print("Gemini API đã sẵn sàng!")
        
        # Storage
        self.storage_path = "./simple_vector_storage"
        os.makedirs(self.storage_path, exist_ok=True)
        
        self.documents = []
        self.metadatas = []
        self.embeddings = []
        
        # Load existing data
        self.load_data()
    
    def get_embedding(self, text: str):
        """Tạo embedding bằng Gemini"""
        try:
            # Sử dụng Gemini embedding model
            result = genai.embed_content(
                model="models/embedding-001",
                content=text,
                task_type="retrieval_document"
            )
            return result['embedding']
        except Exception as e:
            print(f"Lỗi tạo embedding: {e}")
            # Fallback: tạo embedding đơn giản từ text
            return [hash(text) % 1000 / 1000.0] * 768
    
    def add_documents(self, texts: List[str], metadatas: List[Dict], ids: List[str] = None):
        """Thêm documents"""
        if ids is None:
            ids = [f"doc_{len(self.documents) + i}" for i in range(len(texts))]
        
        print(f"Đang thêm {len(texts)} documents...")
        
        for text, metadata in zip(texts, metadatas):
            # Tạo embedding
            embedding = self.get_embedding(text)
            
            # Lưu data
            self.documents.append(text)
            self.metadatas.append({**metadata, "text": text})
            self.embeddings.append(embedding)
        
        # Save to file
        self.save_data()
        print("Documents đã được thêm!")
    
    def search(self, query: str, n_results: int = 5):
        """Tìm kiếm documents"""
        if not self.documents:
            return {"documents": [[]], "metadatas": [[]]}
        
        print(f"Đang tìm kiếm: {query}")
        
        # Tạo embedding cho query
        query_embedding = self.get_embedding(query)
        
        # Tính similarity đơn giản (cosine similarity)
        similarities = []
        for i, doc_embedding in enumerate(self.embeddings):
            # Đơn giản hóa: chỉ so sánh text
            similarity = self.simple_similarity(query.lower(), self.documents[i].lower())
            similarities.append((similarity, i))
        
        # Sort theo similarity
        similarities.sort(reverse=True)
        
        # Lấy top results
        top_indices = [idx for _, idx in similarities[:n_results]]
        
        documents = [self.documents[i] for i in top_indices]
        metadatas = [{k: v for k, v in self.metadatas[i].items() if k != "text"} for i in top_indices]
        
        return {
            "documents": [documents],
            "metadatas": [metadatas]
        }
    
    def simple_similarity(self, query: str, doc: str):
        """Tính similarity đơn giản bằng cách đếm từ chung"""
        query_words = set(query.split())
        doc_words = set(doc.split())
        
        common_words = query_words.intersection(doc_words)
        return len(common_words) / max(len(query_words), 1)
    
    def save_data(self):
        """Lưu data xuống file"""
        data = {
            "documents": self.documents,
            "metadatas": self.metadatas,
            "embeddings": self.embeddings
        }
        
        with open(os.path.join(self.storage_path, "data.json"), "w", encoding="utf-8") as f:
            json.dump(data, f, ensure_ascii=False, indent=2)
    
    def load_data(self):
        """Load data từ file"""
        file_path = os.path.join(self.storage_path, "data.json")
        if os.path.exists(file_path):
            try:
                with open(file_path, "r", encoding="utf-8") as f:
                    data = json.load(f)
                
                self.documents = data.get("documents", [])
                self.metadatas = data.get("metadatas", [])
                self.embeddings = data.get("embeddings", [])
                
                print(f"Đã load {len(self.documents)} documents")
            except Exception as e:
                print(f"Lỗi load data: {e}")
    
    def get_collection_count(self):
        """Lấy số lượng documents"""
        return len(self.documents)

# Alias để tương thích
PineconeVectorStore = SimpleVectorStore
