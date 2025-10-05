/**
 * USER PROFILE PAGE
 * Quản lý profile và upload avatar
 */

class UserProfile {
    constructor() {
        this.API_BASE = 'http://localhost:9000/api';
        this.token = localStorage.getItem('token');
        this.user = null;

        if (!this.token) {
            window.location.href = 'auth.html';
            return;
        }

        this.init();
    }

    async init() {
        try {
            this.user = JSON.parse(localStorage.getItem('user'));

            await this.loadUserData();
            this.bindEvents();
        } catch (error) {
            console.error('Init error:', error);
            this.showNotification('Lỗi khởi tạo', 'error');
        }
    }

    async loadUserData() {
        try {
            this.showLoading(true);

            const response = await fetch(`${this.API_BASE}/users/${this.user.id}`, {
                headers: {
                    'Authorization': `Bearer ${this.token}`
                }
            });

            if (!response.ok) throw new Error('Failed to load user data');

            const result = await response.json();
            const userData = result.data;

            // Update user info
            this.user = userData;
            localStorage.setItem('user', JSON.stringify(userData));

            // Display data
            this.displayUserData(userData);

        } catch (error) {
            console.error('Load user error:', error);
            this.showNotification('Không thể tải thông tin người dùng', 'error');
        } finally {
            this.showLoading(false);
        }
    }

    displayUserData(userData) {
        // Display username, email, role
        document.getElementById('displayUsername').textContent = userData.username;
        document.getElementById('displayEmail').textContent = userData.email;
        document.getElementById('displayMessages').textContent = userData.totalMessages || 0;
        document.getElementById('displayConversations').textContent = userData.totalConversations || 0;

        // Display role with badge
        const roleEl = document.getElementById('displayRole');
        roleEl.textContent = userData.role;
        roleEl.className = `role-badge role-${userData.role}`;

        // Display full name
        document.getElementById('fullName').value = userData.fullName || '';

        // Display avatar
        this.displayAvatar(userData.avatarUrl, userData.fullName || userData.username);
    }

    displayAvatar(avatarUrl, name) {
        const avatarPreview = document.getElementById('avatarPreview');
        const deleteBtn = document.getElementById('deleteAvatarBtn');

        if (avatarUrl) {
            // Check if Cloudinary URL or local URL
            let imageUrl = avatarUrl;
            if (!avatarUrl.startsWith('http')) {
                // Local URL - prepend API base
                imageUrl = `${this.API_BASE.replace('/api', '')}${avatarUrl}`;
            }
            // Cloudinary URL - use directly

            // Show image avatar
            avatarPreview.innerHTML = `<img src="${imageUrl}" alt="Avatar">`;
            deleteBtn.style.display = 'inline-block';
        } else {
            // Show initial letter
            const initial = name ? name.charAt(0).toUpperCase() : '?';
            avatarPreview.innerHTML = `<span id="avatarInitial">${initial}</span>`;
            deleteBtn.style.display = 'none';
        }
    }

    bindEvents() {
        // Upload avatar
        document.getElementById('avatarInput').addEventListener('change', (e) => {
            this.handleAvatarUpload(e.target.files[0]);
        });

        // Delete avatar
        document.getElementById('deleteAvatarBtn').addEventListener('click', () => {
            this.deleteAvatar();
        });

        // Update profile form
        document.getElementById('profileForm').addEventListener('submit', (e) => {
            e.preventDefault();
            this.updateProfile();
        });
    }

    async handleAvatarUpload(file) {
        if (!file) return;

        // Validate file
        const validTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
        if (!validTypes.includes(file.type)) {
            this.showNotification('Chỉ chấp nhận file ảnh (jpg, png, gif, webp)', 'error');
            return;
        }

        if (file.size > 5 * 1024 * 1024) {
            this.showNotification('Kích thước file tối đa 5MB', 'error');
            return;
        }

        try {
            // Show upload progress
            const progressContainer = document.getElementById('uploadProgress');
            const progressBar = document.getElementById('uploadProgressBar');
            progressContainer.classList.add('active');
            progressBar.style.width = '0%';

            // Create FormData
            const formData = new FormData();
            formData.append('avatar', file);

            // Upload with progress
            const xhr = new XMLHttpRequest();

            xhr.upload.addEventListener('progress', (e) => {
                if (e.lengthComputable) {
                    const percentComplete = (e.loaded / e.total) * 100;
                    progressBar.style.width = percentComplete + '%';
                }
            });

            xhr.addEventListener('load', async () => {
                progressContainer.classList.remove('active');

                if (xhr.status === 200) {
                    const result = JSON.parse(xhr.responseText);

                    // Update avatar display
                    this.displayAvatar(result.data.avatarUrl, this.user.fullName || this.user.username);

                    // Update user data in localStorage
                    this.user.avatarUrl = result.data.avatarUrl;
                    localStorage.setItem('user', JSON.stringify(this.user));

                    this.showNotification('✅ Upload avatar thành công!', 'success');
                } else {
                    throw new Error('Upload failed');
                }
            });

            xhr.addEventListener('error', () => {
                progressContainer.classList.remove('active');
                this.showNotification('❌ Upload thất bại', 'error');
            });

            xhr.open('POST', `${this.API_BASE}/users/${this.user.id}/upload-avatar`);
            xhr.setRequestHeader('Authorization', `Bearer ${this.token}`);
            xhr.send(formData);

        } catch (error) {
            console.error('Upload error:', error);
            this.showNotification('Upload thất bại', 'error');
        }
    }

    async deleteAvatar() {
        if (!confirm('Bạn có chắc muốn xóa avatar?')) return;

        try {
            this.showLoading(true);

            const response = await fetch(`${this.API_BASE}/users/${this.user.id}/avatar`, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${this.token}`
                }
            });

            if (!response.ok) throw new Error('Delete failed');

            // Update display
            this.displayAvatar(null, this.user.fullName || this.user.username);

            // Update user data
            this.user.avatarUrl = null;
            localStorage.setItem('user', JSON.stringify(this.user));

            this.showNotification('✅ Đã xóa avatar', 'success');

        } catch (error) {
            console.error('Delete avatar error:', error);
            this.showNotification('Xóa avatar thất bại', 'error');
        } finally {
            this.showLoading(false);
        }
    }

    async updateProfile() {
        try {
            this.showLoading(true);

            const fullName = document.getElementById('fullName').value.trim();

            const response = await fetch(`${this.API_BASE}/users/${this.user.id}`, {
                method: 'PUT',
                headers: {
                    'Authorization': `Bearer ${this.token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    fullName: fullName,
                    avatarUrl: this.user.avatarUrl
                })
            });

            if (!response.ok) throw new Error('Update failed');

            const result = await response.json();

            // Update localStorage
            this.user = result.data;
            localStorage.setItem('user', JSON.stringify(result.data));

            this.showNotification('✅ Cập nhật thành công!', 'success');

        } catch (error) {
            console.error('Update profile error:', error);
            this.showNotification('Cập nhật thất bại', 'error');
        } finally {
            this.showLoading(false);
        }
    }

    showLoading(show) {
        const overlay = document.getElementById('loadingOverlay');
        if (show) {
            overlay.classList.add('active');
        } else {
            overlay.classList.remove('active');
        }
    }

    showNotification(message, type = 'success') {
        const notification = document.getElementById('notification');
        notification.textContent = message;
        notification.className = `notification ${type}`;
        notification.classList.add('show');

        setTimeout(() => {
            notification.classList.remove('show');
        }, 3000);
    }
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    new UserProfile();
});
