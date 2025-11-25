#!/bin/bash
# ============================================================
# EduPortal - Start PRODUCTION Environment
# ============================================================
# Usage: ./scripts/start-prod.sh
# ============================================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

cd "$PROJECT_DIR"

echo "=============================================="
echo "Starting EduPortal PRODUCTION Environment..."
echo "=============================================="

# Check if .env.prod exists
if [ ! -f ".env.prod" ]; then
    echo "ERROR: .env.prod file not found!"
    echo "Please copy .env.example to .env.prod and configure it."
    exit 1
fi

# Warning for production
echo ""
echo "WARNING: You are starting PRODUCTION environment!"
echo "Make sure .env.prod is properly configured with:"
echo "  - Production database credentials"
echo "  - Secure JWT key"
echo "  - Correct CORS origins"
echo ""
read -p "Continue? (y/N): " confirm
if [ "$confirm" != "y" ] && [ "$confirm" != "Y" ]; then
    echo "Cancelled."
    exit 0
fi

# Build and start containers
echo "[1/3] Building Docker images..."
docker-compose -f docker-compose.prod.yml --env-file .env.prod build

echo "[2/3] Starting containers..."
docker-compose -f docker-compose.prod.yml --env-file .env.prod up -d

echo "[3/3] Waiting for services to be healthy..."
sleep 5

# Check container status
echo ""
echo "=============================================="
echo "Container Status:"
echo "=============================================="
docker-compose -f docker-compose.prod.yml --env-file .env.prod ps

echo ""
echo "=============================================="
echo "PRODUCTION Environment Started!"
echo "=============================================="
echo "API:    http://localhost:8081"
echo "Health: http://localhost:8081/health"
echo ""
echo "Note: MSSQL is external (configured in .env.prod)"
echo ""
echo "To view logs: ./scripts/logs-prod.sh"
echo "To stop: ./scripts/stop-prod.sh"
echo "=============================================="
