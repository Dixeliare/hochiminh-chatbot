class HCMChatApp {
    constructor() {
        this.API_BASE = 'http://localhost:5000/api';
        this.currentConversationId = null;
        this.user = null;
        this.token = null;
        this.conversations = [];

        this.init();
    }

    async init() {
        // Check authentication
        this.token = localStorage.getItem('token');
        const userStr = localStorage.getItem('user');

        if (!this.token || !userStr) {
            window.location.href = 'auth.html';
            return;
        }

        try {
            this.user = JSON.parse(userStr);
            this.setupUI();
            this.bindEvents();
            await this.loadConversations();
        } catch (error) {
            console.error('Init error:', error);
            this.logout();
        }
    }

    setupUI() {
        // Update user info in sidebar
        document.getElementById('userName').textContent = this.user.fullName || this.user.username;
        document.getElementById('userRole').textContent = this.user.role || 'user';

        // Auto-resize textarea
        const messageInput = document.getElementById('messageInput');
        messageInput.addEventListener('input', this.autoResizeTextarea);
    }

    bindEvents() {
        // Chat form submission
        document.getElementById('chatForm').addEventListener('submit', (e) => {
            e.preventDefault();
            this.sendMessage();
        });

        // Mobile overlay click
        document.getElementById('mobileOverlay').addEventListener('click', () => {
            this.closeSidebar();
        });

        // Enter key handling
        document.getElementById('messageInput').addEventListener('keydown', (e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                this.sendMessage();
            }
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

    async sendMessage() {
        const input = document.getElementById('messageInput');
        const message = input.value.trim();

        if (!message) return;

        // Clear input and disable form
        input.value = '';
        input.style.height = 'auto';
        this.setInputDisabled(true);
        this.showTypingIndicator();

        try {
            // Add user message to UI immediately
            this.addMessageToUI({
                content: message,
                role: 'user',
                createdAt: new Date().toISOString()
            });

            // Send message to API
            const response = await this.fetchWithAuth('/chat/send', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    message: message,
                    conversationId: this.currentConversationId
                })
            });

            if (response.ok) {
                const data = await response.json();
                const result = data.data;

                // Update current conversation ID if new conversation was created
                if (!this.currentConversationId) {
                    this.currentConversationId = result.conversationId;
                }

                // Add assistant message to UI
                this.addMessageToUI(result.assistantMessage);

                // Refresh conversations list
                await this.loadConversations();
                this.hideEmptyState();

            } else {
                const errorData = await response.json();
                this.showError(errorData.message || 'Có lỗi xảy ra khi gửi tin nhắn');
            }

        } catch (error) {
            console.error('Send message error:', error);
            this.showError('Lỗi kết nối. Vui lòng thử lại.');
        } finally {
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

    async fetchWithAuth(endpoint, options = {}) {
        const url = this.API_BASE + endpoint;
        const headers = {
            'Authorization': `Bearer ${this.token}`,
            ...options.headers
        };

        const response = await fetch(url, {
            ...options,
            headers
        });

        // If unauthorized, redirect to login
        if (response.status === 401) {
            this.logout();
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

// Global functions for HTML onclick events
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

// Initialize the app when page loads
let chatApp;
document.addEventListener('DOMContentLoaded', () => {
    chatApp = new HCMChatApp();
});

// Add CSS for slide-in animation
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