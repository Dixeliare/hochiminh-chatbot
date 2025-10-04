# 🚀 HCM Chatbot - Hướng dẫn Deploy Production

> Hướng dẫn chi tiết để deploy HCM Chatbot lên server/cloud production

---

## 📋 Mục lục

1. [Tổng quan](#-tổng-quan)
2. [Yêu cầu](#-yêu-cầu)
3. [Deploy với Docker](#-deploy-với-docker-khuyến-nghị)
4. [Deploy lên VPS](#-deploy-lên-vps)
5. [Deploy lên Cloud Platform](#-deploy-lên-cloud-platform)
6. [SSL/HTTPS Setup](#-sslhttps-setup)
7. [Monitoring & Backup](#-monitoring--backup)
8. [Troubleshooting](#-troubleshooting)

---

## 🎯 Tổng quan

### Kiến trúc Production

```
Internet
    ↓
Nginx (Port 80/443) → SSL/TLS
    ↓
├── Frontend (Static files)
├── .NET API (Port 9000)
└── Python AI (Port 8000)
    ↓
PostgreSQL (Port 5432)
```

### Các phương án deploy

| Phương án | Giá | Độ khó | Khuyến nghị |
|-----------|-----|--------|-------------|
| **VPS + Docker** | $4-6/tháng | ⭐⭐⭐ | ✅ Tốt nhất |
| **Railway.app** | Free tier | ⭐ | ✅ Dễ nhất |
| **AWS EC2** | Free 12 tháng | ⭐⭐⭐⭐ | Có kinh nghiệm |
| **Google Cloud Run** | Pay-as-go | ⭐⭐⭐ | Serverless |

---

## ✅ Yêu cầu

### Server Requirements

- **CPU**: 2 cores (khuyến nghị)
- **RAM**: 2GB minimum, 4GB khuyến nghị
- **Disk**: 20GB SSD
- **OS**: Ubuntu 22.04 LTS (khuyến nghị) hoặc Debian 11+
- **Docker**: 24.0+
- **Docker Compose**: 2.0+

### Domain & DNS (tùy chọn)

- Domain name (VD: `chatbot.yourdomain.com`)
- DNS trỏ về IP server
- SSL certificate (Let's Encrypt - miễn phí)

---

## 🐳 Deploy với Docker (Khuyến nghị)

### Bước 1: Chuẩn bị Server

**Cài đặt Docker trên Ubuntu:**

```bash
# Update system
sudo apt update && sudo apt upgrade -y

# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Add user to docker group
sudo usermod -aG docker $USER
newgrp docker

# Install Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# Verify installation
docker --version
docker-compose --version
```

### Bước 2: Clone Project

```bash
# Clone repository
git clone https://github.com/your-username/hcm-chatbot.git
cd hcm-chatbot

# Or upload files via SCP/SFTP
scp -r hcm-chatbot/ user@your-server-ip:/home/user/
```

### Bước 3: Cấu hình Environment

```bash
# Tạo file .env.production
cp .env.production.example .env.production

# Edit với nano hoặc vim
nano .env.production
```

**Cấu hình bắt buộc:**

```bash
# Database password (tạo password mạnh)
POSTGRES_PASSWORD=your_strong_password_here

# Gemini API Key (lấy từ https://ai.google.dev)
GEMINI_API_KEY=your_gemini_api_key_here

# JWT Secret (tạo bằng: openssl rand -base64 32)
JWT_SECRET_KEY=your-super-secret-jwt-key-minimum-32-characters

# Frontend URL (domain hoặc IP của bạn)
FRONTEND_URL=http://your-domain.com
# Hoặc: FRONTEND_URL=http://123.456.789.012
```

### Bước 4: Deploy

**Linux/macOS:**
```bash
# Make deploy script executable
chmod +x deploy.sh

# Run deployment
./deploy.sh
```

**Windows PowerShell:**
```powershell
.\deploy.ps1
```

**Manual deployment:**
```bash
# Build and start services
docker-compose --env-file .env.production up -d --build

# View logs
docker-compose logs -f

# Check status
docker-compose ps
```

### Bước 5: Verify Deployment

```bash
# Check service health
curl http://localhost:9000/health
curl http://localhost:8000/health
curl http://localhost/

# Check logs for errors
docker-compose logs dotnet-api
docker-compose logs python-ai
docker-compose logs frontend
```

---

## 🌐 Deploy lên VPS

### Option 1: DigitalOcean (Khuyến nghị)

**Giá:** $6/tháng (2GB RAM, 1 CPU, 50GB SSD)

**Bước 1:** Tạo Droplet
1. Đăng ký tại: https://www.digitalocean.com/
2. Tạo Droplet: Ubuntu 22.04 LTS
3. Chọn plan: Basic - $6/month
4. Tạo SSH key hoặc dùng password

**Bước 2:** Kết nối SSH
```bash
ssh root@your-droplet-ip
```

**Bước 3:** Follow [Deploy với Docker](#-deploy-với-docker-khuyến-nghị)

### Option 2: AWS EC2 (Free Tier 12 tháng)

**Free tier:** t2.micro (1GB RAM, 1 CPU)

**Bước 1:** Tạo EC2 Instance
1. Đăng ký AWS: https://aws.amazon.com/
2. Launch EC2 instance
3. Chọn: Ubuntu Server 22.04 LTS
4. Instance type: t2.micro (Free tier)
5. Security Group: Open ports 80, 443, 22

**Bước 2:** Kết nối
```bash
chmod 400 your-key.pem
ssh -i your-key.pem ubuntu@your-ec2-ip
```

**Bước 3:** Follow [Deploy với Docker](#-deploy-với-docker-khuyến-nghị)

### Option 3: Oracle Cloud (Always Free)

**Free tier:** VM.Standard.E2.1.Micro (1GB RAM, 1 CPU)

1. Đăng ký: https://www.oracle.com/cloud/free/
2. Tạo Compute Instance: Ubuntu 22.04
3. Open ports trong Security List
4. SSH và deploy như VPS

---

## ☁️ Deploy lên Cloud Platform

### Railway.app (Dễ nhất - Free tier)

**Bước 1:** Chuẩn bị Repository
```bash
# Push code lên GitHub
git add .
git commit -m "Prepare for Railway deployment"
git push origin main
```

**Bước 2:** Deploy trên Railway
1. Đăng ký: https://railway.app/
2. Click "New Project" > "Deploy from GitHub repo"
3. Chọn repository `hcm-chatbot`
4. Railway tự động detect Dockerfile và deploy

**Bước 3:** Cấu hình Environment Variables
- Vào Settings > Variables
- Thêm:
  - `GEMINI_API_KEY`
  - `POSTGRES_PASSWORD`
  - `JWT_SECRET_KEY`
  - `FRONTEND_URL`

**Bước 4:** Add PostgreSQL Database
1. Click "New" > "Database" > "Add PostgreSQL"
2. Railway tự động tạo và connect

### Render.com (Free tier)

**Deploy từ Docker:**
1. Đăng ký: https://render.com/
2. New > Web Service > Connect repository
3. Chọn `Dockerfile.dotnet` cho .NET API
4. Chọn `Dockerfile.backend` cho Python AI
5. Static Site cho Frontend
6. Add PostgreSQL database

---

## 🔒 SSL/HTTPS Setup

### Với Nginx + Let's Encrypt (Free)

**Bước 1:** Cài đặt Certbot

```bash
sudo apt install certbot python3-certbot-nginx -y
```

**Bước 2:** Lấy SSL Certificate

```bash
# Thay your-domain.com bằng domain của bạn
sudo certbot --nginx -d your-domain.com -d www.your-domain.com

# Email để nhận thông báo
# Đồng ý Terms of Service
# Certbot tự động config Nginx
```

**Bước 3:** Auto-renewal

```bash
# Test renewal
sudo certbot renew --dry-run

# Certbot tự động renew mỗi 60 ngày
```

**Bước 4:** Update docker-compose.yml

```yaml
frontend:
  ports:
    - "80:80"
    - "443:443"
  volumes:
    - /etc/letsencrypt:/etc/letsencrypt:ro
```

### Với Cloudflare (Free SSL)

1. Đăng ký Cloudflare: https://www.cloudflare.com/
2. Add domain của bạn
3. Update nameservers theo hướng dẫn
4. SSL/TLS > Flexible hoặc Full
5. Cloudflare tự động cấp SSL

---

## 📊 Monitoring & Backup

### Health Monitoring

**Setup health check script:**

```bash
# Create health-check.sh
cat > health-check.sh << 'EOF'
#!/bin/bash
if ! curl -f http://localhost:9000/health > /dev/null 2>&1; then
    echo "API down! Restarting..."
    docker-compose restart dotnet-api
fi

if ! curl -f http://localhost:8000/health > /dev/null 2>&1; then
    echo "AI service down! Restarting..."
    docker-compose restart python-ai
fi
EOF

chmod +x health-check.sh

# Add to crontab (check every 5 minutes)
crontab -e
# Add: */5 * * * * /path/to/health-check.sh
```

### Database Backup

**Tự động backup PostgreSQL:**

```bash
# Create backup script
cat > backup-db.sh << 'EOF'
#!/bin/bash
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="/backup/postgresql"
mkdir -p $BACKUP_DIR

docker exec hcm-postgres pg_dump -U postgres hcm_chatbot_db | gzip > $BACKUP_DIR/backup_$DATE.sql.gz

# Keep only 7 days backup
find $BACKUP_DIR -name "backup_*.sql.gz" -mtime +7 -delete
EOF

chmod +x backup-db.sh

# Add to crontab (daily at 2 AM)
crontab -e
# Add: 0 2 * * * /path/to/backup-db.sh
```

### Logs Rotation

```bash
# Docker logs can grow large, setup rotation
cat > /etc/docker/daemon.json << EOF
{
  "log-driver": "json-file",
  "log-opts": {
    "max-size": "10m",
    "max-file": "3"
  }
}
EOF

sudo systemctl restart docker
```

---

## 🔧 Troubleshooting

### Services không start

```bash
# Check logs
docker-compose logs

# Specific service
docker-compose logs dotnet-api
docker-compose logs python-ai

# Restart services
docker-compose restart

# Rebuild và restart
docker-compose down
docker-compose up -d --build
```

### Database connection error

```bash
# Check PostgreSQL
docker-compose logs postgres

# Reset database
docker-compose down -v  # Warning: Deletes data!
docker-compose up -d
```

### Port conflicts

```bash
# Check what's using port
sudo lsof -i :80
sudo lsof -i :9000

# Kill process
sudo kill -9 <PID>

# Or change ports in docker-compose.yml
```

### Out of memory

```bash
# Check memory usage
docker stats

# Increase swap
sudo fallocate -l 2G /swapfile
sudo chmod 600 /swapfile
sudo mkswap /swapfile
sudo swapon /swapfile
```

### SSL certificate renewal failed

```bash
# Manual renewal
sudo certbot renew --force-renewal

# Check certbot logs
sudo journalctl -u certbot
```

---

## 📋 Checklist Deployment

### Pre-deployment

- [ ] Domain đã trỏ về server IP
- [ ] Firewall mở ports: 80, 443, 22
- [ ] `.env.production` đã cấu hình đúng
- [ ] Gemini API key còn quota
- [ ] PostgreSQL password đủ mạnh
- [ ] JWT secret key đã generate

### Post-deployment

- [ ] Tất cả services đang chạy (`docker-compose ps`)
- [ ] Health checks pass
- [ ] Frontend load được tại domain
- [ ] Đăng nhập admin thành công
- [ ] Chat AI hoạt động
- [ ] SSL/HTTPS đã setup (nếu có domain)
- [ ] Database backup đã schedule
- [ ] Monitoring/alerts đã setup

### Security

- [ ] Đổi password admin
- [ ] Update `.env.production` permissions: `chmod 600`
- [ ] Firewall configured
- [ ] SSH key-based auth (disable password)
- [ ] Regular security updates: `sudo apt update && sudo apt upgrade`

---

## 🆘 Support & Resources

**Documentation:**
- Docker: https://docs.docker.com/
- .NET Deployment: https://learn.microsoft.com/aspnet/core/host-and-deploy/
- PostgreSQL Backup: https://www.postgresql.org/docs/current/backup.html

**Community:**
- GitHub Issues: [Create an issue](https://github.com/your-repo/issues)
- Discussions: [Join discussions](https://github.com/your-repo/discussions)

---

**🇻🇳 Good luck với deployment! 🚀**
