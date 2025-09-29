/**
 * HCM CHATBOT FRONTEND APPLICATION
 * Class chính quản lý toàn bộ giao diện và logic frontend
 *
 * Chức năng:
 * - Xác thực người dùng với JWT token
 * - Quản lý cuộc trò chuyện (tạo mới, load, xóa)
 * - Gửi tin nhắn và hiển thị phản hồi AI
 * - Tích hợp với .NET API backend
 */
class HCMChatApp {
    constructor() {
        // ===== CẤU HÌNH API =====
        this.API_BASE = 'http://localhost:9000/api'; // URL của .NET API

        // ===== STATE MANAGEMENT =====
        this.currentConversationId = null; // ID cuộc trò chuyện hiện tại
        this.user = null; // Thông tin người dùng đã đăng nhập
        this.token = null; // JWT token cho authentication
        this.conversations = []; // Danh sách cuộc trò chuyện

        // Khởi tạo ứng dụng
        this.init();
    }

    /**
     * KHỞI TẠO ỨNG DỤNG
     * Kiểm tra authentication và setup giao diện
     */
    async init() {
        // ===== KIỂM TRA AUTHENTICATION =====
        this.token = localStorage.getItem('token'); // Lấy token từ localStorage
        const userStr = localStorage.getItem('user'); // Lấy thông tin user

        // Nếu chưa đăng nhập, chuyển về trang auth
        if (!this.token || !userStr) {
            window.location.href = 'auth.html';
            return;
        }

        try {
            // Parse thông tin user từ JSON
            this.user = JSON.parse(userStr);

            // Kiểm tra quyền truy cập - Admin không được chat
            if (this.user.role === 'admin') {
                alert('Admin không được sử dụng chức năng chat. Chuyển về trang quản trị.');
                window.location.href = 'admin.html';
                return;
            }

            // Setup giao diện và events
            this.setupUI();
            this.bindEvents();

            // Load danh sách cuộc trò chuyện
            await this.loadConversations();
        } catch (error) {
            console.error('Init error:', error);
            // Nếu có lỗi, logout và về trang auth
            this.logout();
        }
    }

    /**
     * SETUP GIAO DIỆN
     * Cấu hình thông tin user và auto-resize textarea
     */
    setupUI() {
        // Hiển thị thông tin user trong sidebar
        document.getElementById('userName').textContent = this.user.fullName || this.user.username;
        document.getElementById('userRole').textContent = this.user.role || 'user';

        // Auto-resize textarea khi người dùng gõ
        const messageInput = document.getElementById('messageInput');
        messageInput.addEventListener('input', this.autoResizeTextarea);
    }

    /**
     * BIND EVENTS
     * Gắn các event listener cho tương tác người dùng
     */
    bindEvents() {
        // Xử lý submit form chat
        document.getElementById('chatForm').addEventListener('submit', (e) => {
            e.preventDefault(); // Ngăn reload trang
            this.sendMessage();
        });

        // Đóng sidebar khi click overlay (mobile)
        document.getElementById('mobileOverlay').addEventListener('click', () => {
            this.closeSidebar();
        });

        // Xử lý phím Enter để gửi tin nhắn
        document.getElementById('messageInput').addEventListener('keydown', (e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault(); // Ngăn xuống dòng
                this.sendMessage();
            }
            // Shift+Enter vẫn cho phép xuống dòng
        });
    }

    autoResizeTextarea(e) {
        const textarea = e.target;
        textarea.style.height = 'auto';
        const scrollHeight = Math.min(textarea.scrollHeight, 120);
        textarea.style.height = scrollHeight + 'px';
    }

    async loadConversations() {
        try {
            const response = await this.fetchWithAuth('/chat/conversations');
            if (response.ok) {
                const data = await response.json();
                this.conversations = data.data || [];
                this.renderConversations();
            } else {
                console.error('Failed to load conversations');
            }
        } catch (error) {
            console.error('Error loading conversations:', error);
        }
    }

    renderConversations() {
        const container = document.getElementById('conversationsList');

        if (this.conversations.length === 0) {
            container.innerHTML = `
                <div style="padding: 20px; text-align: center; color: #666;">
                    <i class="fas fa-comments" style="font-size: 2rem; margin-bottom: 10px; color: #ddd;"></i>
                    <p>Chưa có cuộc trò chuyện nào</p>
                    <small>Bắt đầu chat để tạo cuộc trò chuyện đầu tiên</small>
                </div>
            `;
            return;
        }

        const conversationsHTML = this.conversations.map(conv => `
            <div class="conversation-item ${conv.id === this.currentConversationId ? 'active' : ''}"
                 onclick="chatApp.selectConversation('${conv.id}')">
                <div class="conversation-title">${conv.title}</div>
                <div class="conversation-meta">
                    <span>${conv.messageCount} tin nhắn</span>
                    <span>${this.formatDate(conv.updatedAt)}</span>
                </div>
                <i class="fas fa-trash delete-conversation"
                   onclick="event.stopPropagation(); chatApp.deleteConversation('${conv.id}')"></i>
            </div>
        `).join('');

        container.innerHTML = conversationsHTML;
    }

    async selectConversation(conversationId) {
        this.currentConversationId = conversationId;
        this.renderConversations(); // Update active state

        // Load messages for this conversation
        await this.loadMessages(conversationId);
        this.hideEmptyState();
        this.closeSidebar(); // Close sidebar on mobile after selection
    }

    async loadMessages(conversationId) {
        try {
            const response = await this.fetchWithAuth(`/chat/conversations/${conversationId}/messages`);
            if (response.ok) {
                const data = await response.json();
                const messages = data.data || [];
                this.renderMessages(messages);
            } else {
                console.error('Failed to load messages');
            }
        } catch (error) {
            console.error('Error loading messages:', error);
        }
    }

    renderMessages(messages) {
        const container = document.getElementById('chatMessages');

        if (messages.length === 0) {
            container.innerHTML = `
                <div style="padding: 40px; text-align: center; color: #666;">
                    <i class="fas fa-comment-dots" style="font-size: 3rem; margin-bottom: 15px; color: #ddd;"></i>
                    <h3>Cuộc trò chuyện trống</h3>
                    <p>Hãy bắt đầu câu hỏi đầu tiên!</p>
                </div>
            `;
            return;
        }

        const messagesHTML = messages.map(msg => this.createMessageHTML(msg)).join('');
        container.innerHTML = messagesHTML;
        this.scrollToBottom();
    }

    createMessageHTML(message) {
        const isUser = message.role === 'user';
        const avatar = isUser ? '<i class="fas fa-user"></i>' : '<i class="fas fa-robot"></i>';

        let sourcesHTML = '';
        if (!isUser && message.sources) {
            try {
                const sources = typeof message.sources === 'string'
                    ? JSON.parse(message.sources)
                    : message.sources;

                if (Array.isArray(sources) && sources.length > 0) {
                    sourcesHTML = `
                        <div class="message-sources">
                            <h4>Nguồn tham khảo:</h4>
                            ${sources.map(source => `<div class="source-item">${source}</div>`).join('')}
                        </div>
                    `;
                }
            } catch (e) {
                console.error('Error parsing sources:', e);
            }
        }

        let metaHTML = '';
        if (!isUser) {
            metaHTML = `
                <div class="message-meta">
                    <span>${this.formatDateTime(message.createdAt)}</span>
                    ${message.confidenceScore ? `<span class="confidence-badge">Độ tin cậy: ${message.confidenceScore}%</span>` : ''}
                </div>
            `;
        }

        return `
            <div class="message ${isUser ? 'user-message' : 'bot-message'}">
                <div class="message-avatar">${avatar}</div>
                <div class="message-content">
                    <div class="message-bubble">${this.formatMessageContent(message.content)}</div>
                    ${sourcesHTML}
                    ${metaHTML}
                </div>
            </div>
        `;
    }

    formatMessageContent(content) {
        return content.replace(/\n/g, '<br>');
    }

    /**
     * GỬI TIN NHẮN - Function chính xử lý chat
     *
     * Quy trình:
     * 1. Validate input và update UI
     * 2. Gửi request đến .NET API
     * 3. API gọi Python AI và trả về response
     * 4. Hiển thị phản hồi AI trên giao diện
     */
    async sendMessage() {
        const input = document.getElementById('messageInput');
        const message = input.value.trim();

        // Không gửi nếu tin nhắn trống
        if (!message) return;

        // ===== BƯỚC 1: CHUẨN BỊ UI =====
        // Xóa input và disable form để tránh spam
        input.value = '';
        input.style.height = 'auto';
        this.setInputDisabled(true);
        this.showTypingIndicator(); // Hiển thị "AI đang trả lời..."

        try {
            // ===== BƯỚC 2: HIỂN THỊ TIN NHẮN NGƯỜI DÙNG NGAY LẬP TỨC =====
            this.addMessageToUI({
                content: message,
                role: 'user',
                createdAt: new Date().toISOString()
            });

            // ===== BƯỚC 3: GỬI REQUEST ĐẾN .NET API =====
            // Tạo AbortController để timeout sau 60 giây
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), 60000); // 60 seconds timeout

            // Gọi .NET API với authentication
            const response = await this.fetchWithAuth('/chat/send', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    message: message,
                    conversationId: this.currentConversationId // null nếu cuộc trò chuyện mới
                }),
                signal: controller.signal // Cho phép timeout
            });

            clearTimeout(timeoutId);

            // ===== BƯỚC 4: XỬ LÝ RESPONSE =====
            if (response.ok) {
                const data = await response.json();
                const result = data.data; // ChatApiResponse từ .NET

                // Nếu tạo cuộc trò chuyện mới, lưu ID
                if (!this.currentConversationId) {
                    this.currentConversationId = result.conversationId;
                }

                // Hiển thị phản hồi AI với sources và confidence
                this.addMessageToUI(result.assistantMessage);

                // Cập nhật danh sách cuộc trò chuyện trong sidebar
                await this.loadConversations();
                this.hideEmptyState();

            } else {
                // Xử lý lỗi từ API
                const errorData = await response.json();
                this.showError(errorData.message || 'Có lỗi xảy ra khi gửi tin nhắn');
            }

        } catch (error) {
            console.error('Send message error:', error);
            // Xử lý các loại lỗi khác nhau
            if (error.name === 'AbortError') {
                this.showError('Timeout: AI đang xử lý quá lâu. Vui lòng thử câu hỏi ngắn hơn.');
            } else {
                this.showError('Lỗi kết nối. Vui lòng thử lại.');
            }
        } finally {
            // ===== BƯỚC 5: CLEANUP =====
            // Luôn enable lại form và ẩn typing indicator
            this.setInputDisabled(false);
            this.hideTypingIndicator();
            document.getElementById('messageInput').focus();
        }
    }

    addMessageToUI(message) {
        const container = document.getElementById('chatMessages');
        const messageHTML = this.createMessageHTML(message);
        container.insertAdjacentHTML('beforeend', messageHTML);
        this.scrollToBottom();
    }

    setInputDisabled(disabled) {
        document.getElementById('messageInput').disabled = disabled;
        document.getElementById('sendButton').disabled = disabled;
    }

    showTypingIndicator() {
        document.getElementById('typingIndicator').style.display = 'block';
        this.scrollToBottom();
    }

    hideTypingIndicator() {
        document.getElementById('typingIndicator').style.display = 'none';
    }

    hideEmptyState() {
        const emptyState = document.getElementById('emptyState');
        if (emptyState) {
            emptyState.style.display = 'none';
        }
    }

    async startNewChat() {
        this.currentConversationId = null;
        document.getElementById('chatMessages').innerHTML = `
            <div class="empty-state">
                <i class="fas fa-comment-dots"></i>
                <h3>Cuộc trò chuyện mới</h3>
                <p>Hãy bắt đầu với câu hỏi đầu tiên về tư tưởng Hồ Chí Minh</p>
            </div>
        `;

        // Clear active conversation
        this.renderConversations();
        this.closeSidebar();
        document.getElementById('messageInput').focus();
    }

    async deleteConversation(conversationId) {
        if (!confirm('Bạn có chắc muốn xóa cuộc trò chuyện này?')) {
            return;
        }

        try {
            const response = await this.fetchWithAuth(`/chat/conversations/${conversationId}`, {
                method: 'DELETE'
            });

            if (response.ok) {
                // Remove from local list
                this.conversations = this.conversations.filter(c => c.id !== conversationId);

                // If this was the current conversation, start a new one
                if (this.currentConversationId === conversationId) {
                    this.startNewChat();
                }

                this.renderConversations();
            } else {
                this.showError('Không thể xóa cuộc trò chuyện');
            }
        } catch (error) {
            console.error('Delete conversation error:', error);
            this.showError('Lỗi khi xóa cuộc trò chuyện');
        }
    }

    sendSuggestedMessage(message) {
        document.getElementById('messageInput').value = message;
        this.sendMessage();
    }

    scrollToBottom() {
        const container = document.getElementById('chatMessages');
        setTimeout(() => {
            container.scrollTop = container.scrollHeight;
        }, 100);
    }

    toggleSidebar() {
        const sidebar = document.getElementById('sidebar');
        const overlay = document.getElementById('mobileOverlay');

        sidebar.classList.toggle('open');
        overlay.classList.toggle('show');
    }

    closeSidebar() {
        const sidebar = document.getElementById('sidebar');
        const overlay = document.getElementById('mobileOverlay');

        sidebar.classList.remove('open');
        overlay.classList.remove('show');
    }

    showError(message) {
        // Create a temporary error notification
        const errorDiv = document.createElement('div');
        errorDiv.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background: #f44336;
            color: white;
            padding: 15px 20px;
            border-radius: 8px;
            z-index: 10000;
            animation: slideIn 0.3s ease;
        `;
        errorDiv.textContent = message;

        document.body.appendChild(errorDiv);

        setTimeout(() => {
            errorDiv.remove();
        }, 5000);
    }

    /**
     * FETCH VỚI AUTHENTICATION
     * Wrapper cho fetch API tự động thêm JWT token và xử lý unauthorized
     *
     * @param {string} endpoint - API endpoint (VD: '/chat/send')
     * @param {object} options - Fetch options (method, body, headers, etc.)
     * @returns {Promise<Response>} - Response từ server
     */
    async fetchWithAuth(endpoint, options = {}) {
        const url = this.API_BASE + endpoint; // Tạo URL đầy đủ

        // Thêm Authorization header với JWT token
        const headers = {
            'Authorization': `Bearer ${this.token}`, // Bearer token format
            ...options.headers // Merge với headers khác
        };

        // Gọi API với authenticated headers
        const response = await fetch(url, {
            ...options,
            headers
        });

        // Nếu token hết hạn hoặc không hợp lệ, logout
        if (response.status === 401) {
            this.logout(); // Xóa token và về trang đăng nhập
            return;
        }

        return response;
    }

    logout() {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        window.location.href = 'auth.html';
    }

    formatDate(dateString) {
        const date = new Date(dateString);
        const now = new Date();
        const diffDays = Math.floor((now - date) / (1000 * 60 * 60 * 24));

        if (diffDays === 0) {
            return 'Hôm nay';
        } else if (diffDays === 1) {
            return 'Hôm qua';
        } else if (diffDays < 7) {
            return `${diffDays} ngày trước`;
        } else {
            return date.toLocaleDateString('vi-VN');
        }
    }

    formatDateTime(dateString) {
        const date = new Date(dateString);
        return date.toLocaleString('vi-VN');
    }
}

// ===== GLOBAL FUNCTIONS CHO HTML ONCLICK EVENTS =====
// Các function này được gọi trực tiếp từ HTML onclick attributes

function toggleSidebar() {
    chatApp.toggleSidebar();
}

function startNewChat() {
    chatApp.startNewChat();
}

function logout() {
    chatApp.logout();
}

function sendSuggestedMessage(message) {
    chatApp.sendSuggestedMessage(message);
}

// ===== USER PROFILE FUNCTIONS =====
/**
 * Mở modal chỉnh sửa profile cho user
 */
function openUserProfile() {
    const modal = document.getElementById('profileModal');

    // Lấy thông tin user từ localStorage
    const userStr = localStorage.getItem('user');
    if (!userStr) {
        alert('Không tìm thấy thông tin người dùng!');
        return;
    }

    try {
        const user = JSON.parse(userStr);

        // Điền thông tin vào form
        document.getElementById('profileUsername').value = user.username || '';
        document.getElementById('profileEmail').value = user.email || '';
        document.getElementById('profileFullName').value = user.fullName || '';
        document.getElementById('profileRole').value = user.role || 'user';

        // Clear password fields
        document.getElementById('userCurrentPassword').value = '';
        document.getElementById('userNewPassword').value = '';
        document.getElementById('userConfirmPassword').value = '';

        // Hiển thị modal
        modal.style.display = 'block';
    } catch (error) {
        console.error('Error parsing user data:', error);
        alert('Lỗi khi tải thông tin người dùng!');
    }
}

/**
 * Đóng modal profile
 */
function closeUserProfile() {
    const modal = document.getElementById('profileModal');
    modal.style.display = 'none';
}

/**
 * Cập nhật profile user
 */
async function updateUserProfile() {
    try {
        const email = document.getElementById('profileEmail').value.trim();
        const fullName = document.getElementById('profileFullName').value.trim();

        if (!email) {
            alert('Vui lòng nhập email!');
            return;
        }

        const token = localStorage.getItem('token');
        if (!token) {
            alert('Phiên đăng nhập đã hết hạn!');
            window.location.href = 'auth.html';
            return;
        }

        const response = await fetch('http://localhost:9000/api/auth/profile', {
            method: 'PUT',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                email: email,
                fullName: fullName
            })
        });

        if (response.ok) {
            const data = await response.json();
            if (data.success) {
                // Cập nhật localStorage
                const userStr = localStorage.getItem('user');
                const user = JSON.parse(userStr);
                user.email = email;
                user.fullName = fullName;
                localStorage.setItem('user', JSON.stringify(user));

                // Cập nhật hiển thị tên
                document.getElementById('userName').textContent = fullName || user.username;

                alert('Cập nhật profile thành công!');
                closeUserProfile();
            } else {
                alert('Lỗi cập nhật profile: ' + (data.message || 'Không xác định'));
            }
        } else {
            alert('Lỗi kết nối server khi cập nhật profile');
        }
    } catch (error) {
        console.error('Error updating profile:', error);
        alert('Lỗi cập nhật profile: ' + error.message);
    }
}

/**
 * Đổi mật khẩu user
 */
async function changeUserPassword() {
    try {
        const currentPassword = document.getElementById('userCurrentPassword').value;
        const newPassword = document.getElementById('userNewPassword').value;
        const confirmPassword = document.getElementById('userConfirmPassword').value;

        // Validation
        if (!currentPassword || !newPassword || !confirmPassword) {
            alert('Vui lòng điền đầy đủ thông tin mật khẩu!');
            return;
        }

        if (newPassword !== confirmPassword) {
            alert('Mật khẩu mới và xác nhận mật khẩu không khớp!');
            return;
        }

        if (newPassword.length < 6) {
            alert('Mật khẩu mới phải có ít nhất 6 ký tự!');
            return;
        }

        const token = localStorage.getItem('token');
        if (!token) {
            alert('Phiên đăng nhập đã hết hạn!');
            window.location.href = 'auth.html';
            return;
        }

        const response = await fetch('http://localhost:9000/api/auth/change-password', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                currentPassword: currentPassword,
                newPassword: newPassword
            })
        });

        if (response.ok) {
            const data = await response.json();
            if (data.success) {
                alert('Đổi mật khẩu thành công!');
                // Clear password fields
                document.getElementById('userCurrentPassword').value = '';
                document.getElementById('userNewPassword').value = '';
                document.getElementById('userConfirmPassword').value = '';
            } else {
                alert('Lỗi đổi mật khẩu: ' + (data.message || 'Không xác định'));
            }
        } else {
            alert('Lỗi kết nối server khi đổi mật khẩu');
        }
    } catch (error) {
        console.error('Error changing password:', error);
        alert('Lỗi đổi mật khẩu: ' + error.message);
    }
}

// ===== KHỞI TẠO ỨNG DỤNG =====
let chatApp;
document.addEventListener('DOMContentLoaded', () => {
    // Khởi tạo chatbot khi DOM đã load xong
    chatApp = new HCMChatApp();

    // Setup modal click outside to close
    const profileModal = document.getElementById('profileModal');
    profileModal.addEventListener('click', (e) => {
        if (e.target === profileModal) {
            closeUserProfile();
        }
    });
});

// ===== DYNAMIC CSS =====
// Thêm CSS animation cho error notifications
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from {
            transform: translateX(100%);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
`;
document.head.appendChild(style);