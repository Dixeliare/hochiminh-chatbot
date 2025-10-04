#!/bin/bash

# HCM Chatbot - Production Deployment Script
# This script deploys the entire HCM Chatbot system using Docker

set -e  # Exit on error

echo "🚀 HCM Chatbot - Production Deployment"
echo "======================================"
echo ""

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed. Please install Docker first."
    echo "   Visit: https://docs.docker.com/get-docker/"
    exit 1
fi

# Check if Docker Compose is installed
if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
    echo "❌ Docker Compose is not installed. Please install Docker Compose first."
    exit 1
fi

# Check if .env.production exists
if [ ! -f .env.production ]; then
    echo "⚠️  .env.production not found!"
    echo ""
    echo "Creating .env.production from example..."
    cp .env.production.example .env.production
    echo ""
    echo "⚠️  IMPORTANT: Edit .env.production and fill in your values:"
    echo "   - POSTGRES_PASSWORD"
    echo "   - GEMINI_API_KEY"
    echo "   - JWT_SECRET_KEY"
    echo "   - FRONTEND_URL"
    echo ""
    echo "After editing, run this script again."
    exit 1
fi

# Load environment variables
export $(cat .env.production | grep -v '^#' | xargs)

echo "✅ Environment variables loaded"
echo ""

# Ask for confirmation
echo "📋 Deployment Configuration:"
echo "   - Frontend URL: ${FRONTEND_URL:-http://localhost}"
echo "   - Database: PostgreSQL 16"
echo "   - Gemini API: ${GEMINI_API_KEY:0:10}..."
echo ""
read -p "Continue with deployment? (y/n) " -n 1 -r
echo ""
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "Deployment cancelled."
    exit 1
fi

# Stop existing containers
echo "🛑 Stopping existing containers..."
docker-compose down 2>/dev/null || true
echo ""

# Build images
echo "🔨 Building Docker images..."
docker-compose build --no-cache
echo ""

# Start services
echo "🚀 Starting services..."
docker-compose --env-file .env.production up -d
echo ""

# Wait for services to be healthy
echo "⏳ Waiting for services to be ready..."
echo ""

max_attempts=30
attempt=1

while [ $attempt -le $max_attempts ]; do
    echo "   Checking health... (Attempt $attempt/$max_attempts)"

    # Check if all containers are running
    if docker-compose ps | grep -q "Up"; then
        # Check health endpoints
        if curl -s http://localhost:9000/health > /dev/null 2>&1 && \
           curl -s http://localhost:8000/health > /dev/null 2>&1 && \
           curl -s http://localhost/ > /dev/null 2>&1; then
            echo ""
            echo "✅ All services are healthy!"
            break
        fi
    fi

    if [ $attempt -eq $max_attempts ]; then
        echo ""
        echo "❌ Services failed to start. Check logs with:"
        echo "   docker-compose logs"
        exit 1
    fi

    sleep 3
    ((attempt++))
done

echo ""
echo "🎉 Deployment Successful!"
echo "========================"
echo ""
echo "📍 Service URLs:"
echo "   🌐 Frontend:    http://localhost/"
echo "   🔗 API Docs:    http://localhost:9000/swagger"
echo "   🤖 AI Docs:     http://localhost:8000/docs"
echo "   💚 Health:      http://localhost:9000/health"
echo ""
echo "👤 Admin Account:"
echo "   Username: admin"
echo "   Password: admin123"
echo ""
echo "📋 Useful Commands:"
echo "   View logs:       docker-compose logs -f"
echo "   Stop services:   docker-compose down"
echo "   Restart:         docker-compose restart"
echo "   View status:     docker-compose ps"
echo ""
echo "🔒 Security Reminder:"
echo "   - Change admin password after first login"
echo "   - Set up SSL/TLS for production"
echo "   - Configure firewall rules"
echo "   - Backup database regularly"
echo ""
echo "✨ HCM Chatbot is now live! 🇻🇳"
