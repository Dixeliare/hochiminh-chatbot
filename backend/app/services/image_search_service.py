"""
IMAGE SEARCH SERVICE - Multiple Image Search APIs
Tìm kiếm ảnh thật trên Google Images, Pexels, hoặc Unsplash
"""

import os
import requests
from typing import List, Dict, Optional

class ImageSearchService:
    """
    Service để tìm kiếm ảnh sử dụng nhiều nguồn:
    1. Google Custom Search API (nếu có setup)
    2. Pexels API (miễn phí, unlimited)
    3. Fallback: Wikipedia images

    Setup (tùy chọn):
    - Google: Thêm GOOGLE_CUSTOM_SEARCH_API_KEY và GOOGLE_SEARCH_ENGINE_ID vào .env
    - Pexels: Thêm PEXELS_API_KEY vào .env (free tại https://www.pexels.com/api/)
    """

    def __init__(self):
        """Khởi tạo service với API credentials"""
        self.google_api_key = os.getenv("GOOGLE_CUSTOM_SEARCH_API_KEY")
        self.google_search_engine_id = os.getenv("GOOGLE_SEARCH_ENGINE_ID")
        self.pexels_api_key = os.getenv("PEXELS_API_KEY")
        self.google_url = "https://www.googleapis.com/customsearch/v1"
        self.pexels_url = "https://api.pexels.com/v1/search"

    def search_images(self, query: str, num_results: int = 5) -> List[Dict]:
        """
        Tìm kiếm ảnh - tự động chọn nguồn tốt nhất

        Args:
            query: Từ khóa tìm kiếm (VD: "Hồ Chí Minh ở Pháp")
            num_results: Số lượng ảnh trả về (max 10)

        Returns:
            List[Dict]: Danh sách ảnh với thông tin {url, title, thumbnail, source}
        """
        # Thử Google Custom Search trước (nếu có setup)
        if self.google_api_key and self.google_search_engine_id:
            images = self._search_google(query, num_results)
            if images:
                return images

        # Fallback: Pexels API (nếu có setup)
        if self.pexels_api_key:
            images = self._search_pexels(query, num_results)
            if images:
                return images

        # Fallback cuối cùng: Wikipedia images
        print("⚠️ Không có API nào được cấu hình, sử dụng ảnh mặc định từ Wikipedia")
        return self._get_fallback_images(query)

    def _optimize_query(self, query: str) -> str:
        """
        Tối ưu query để tìm kiếm chính xác hơn
        - Loại bỏ từ "cho tôi", "tìm", "xem"
        - Thêm từ khóa chính xác
        """
        query_lower = query.lower()

        # Chuẩn hóa tên TRƯỚC (để tránh bị replace sai)
        query_lower = query_lower.replace("chủ tịch hồ chí minh", "Ho Chi Minh")
        query_lower = query_lower.replace("hồ chí minh", "Ho Chi Minh")
        query_lower = query_lower.replace("bác hồ", "Ho Chi Minh")
        query_lower = query_lower.replace("chủ tịch", "president")

        # Địa điểm & thời gian
        query_lower = query_lower.replace("ở pháp", "in France")
        query_lower = query_lower.replace("tại pháp", "in France")
        query_lower = query_lower.replace("pháp", "France")
        query_lower = query_lower.replace("hồi còn", "")
        query_lower = query_lower.replace("hồi", "")
        query_lower = query_lower.replace("ngài", "")
        query_lower = query_lower.replace("còn", "")
        query_lower = query_lower.replace("thời", "period")

        # Loại bỏ các từ thừa
        remove_words = ["cho tôi", "tìm", "xem", "ảnh", "hình", "hình ảnh", "của", "về", "đi", "nào", "giúp", "với"]
        for word in remove_words:
            query_lower = query_lower.replace(word, " ")

        # Làm sạch và thêm từ khóa tăng độ chính xác
        query_optimized = " ".join(query_lower.split())

        # Thêm từ khóa để filter tốt hơn
        if "Ho Chi Minh" in query_optimized:
            query_optimized += " president Vietnam historical photo"

        return query_optimized

    def _search_google(self, query: str, num_results: int) -> List[Dict]:
        """Tìm kiếm ảnh qua Google Custom Search API"""
        try:
            # Tối ưu query
            optimized_query = self._optimize_query(query)
            print(f"🔍 Original query: {query}")
            print(f"🔍 Optimized query: {optimized_query}")

            num_results = min(num_results, 10)

            params = {
                "key": self.google_api_key,
                "cx": self.google_search_engine_id,
                "q": optimized_query,
                "searchType": "image",
                "num": num_results,
                "safe": "active",
                "imgSize": "medium",
                "fileType": "jpg,png",  # Chỉ lấy JPG và PNG
            }

            response = requests.get(self.google_url, params=params, timeout=10)
            response.raise_for_status()
            data = response.json()

            images = []
            if "items" in data:
                for item in data["items"]:
                    # Lấy tất cả ảnh (đã filter qua query optimization rồi)
                    images.append({
                        "url": item.get("link"),
                        "title": item.get("title"),
                        "thumbnail": item.get("image", {}).get("thumbnailLink"),
                        "source": item.get("displayLink", "Google"),
                        "context": item.get("snippet", ""),
                    })

            return images

        except Exception as e:
            print(f"❌ Google API error: {e}")
            return []

    def _search_pexels(self, query: str, num_results: int) -> List[Dict]:
        """Tìm kiếm ảnh qua Pexels API (miễn phí unlimited)"""
        try:
            headers = {
                "Authorization": self.pexels_api_key
            }

            params = {
                "query": query,
                "per_page": min(num_results, 15),
                "orientation": "landscape"
            }

            response = requests.get(self.pexels_url, headers=headers, params=params, timeout=10)
            response.raise_for_status()
            data = response.json()

            images = []
            if "photos" in data:
                for photo in data["photos"]:
                    images.append({
                        "url": photo.get("src", {}).get("large"),
                        "title": photo.get("alt", "Photo from Pexels"),
                        "thumbnail": photo.get("src", {}).get("medium"),
                        "source": "Pexels.com",
                        "context": f"Photo by {photo.get('photographer', 'Unknown')}"
                    })

            return images

        except Exception as e:
            print(f"❌ Pexels API error: {e}")
            return []

    def _get_fallback_images(self, query: str) -> List[Dict]:
        """
        Trả về ảnh mặc định khi không tìm được hoặc API lỗi
        Sử dụng ảnh từ Wikimedia Commons (public domain)
        """
        # Ảnh công khai về Hồ Chí Minh (verified working URLs)
        fallback_images = [
            {
                "url": "https://images.pexels.com/photos/1134166/pexels-photo-1134166.jpeg",
                "title": "Hồ Chí Minh - Anh hùng dân tộc",
                "thumbnail": "https://images.pexels.com/photos/1134166/pexels-photo-1134166.jpeg?auto=compress&cs=tinysrgb&w=300",
                "source": "Sample Image",
                "context": "Để xem ảnh thật về Hồ Chí Minh, vui lòng setup Google Custom Search API hoặc Pexels API"
            },
            {
                "url": "https://images.pexels.com/photos/1612461/pexels-photo-1612461.jpeg",
                "title": "Vietnam - Historical Photos",
                "thumbnail": "https://images.pexels.com/photos/1612461/pexels-photo-1612461.jpeg?auto=compress&cs=tinysrgb&w=300",
                "source": "Sample Image",
                "context": "API chưa được cấu hình - đây là ảnh mẫu"
            }
        ]

        return fallback_images

    def search_historical_images(self, topic: str) -> List[Dict]:
        """
        Tìm kiếm ảnh lịch sử dựa trên chủ đề

        Args:
            topic: Chủ đề (VD: "Hồ Chí Minh ở Pháp", "Bác Hồ với thiếu nhi")

        Returns:
            List[Dict]: Danh sách ảnh lịch sử
        """
        # Thêm từ khóa để tìm ảnh lịch sử chính xác hơn
        enhanced_query = f"{topic} lịch sử historical"
        return self.search_images(enhanced_query, num_results=6)
