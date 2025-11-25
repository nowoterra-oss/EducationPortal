#!/bin/bash
# ============================================================
# EduPortal - Deploy PRODUCTION Environment
# ============================================================
# Full deployment: pull latest code, rebuild, restart
# Usage: ./scripts/deploy-prod.sh
# ============================================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

cd "$PROJECT_DIR"

echo "=============================================="
echo "Deploying EduPortal PRODUCTION Environment..."
echo "=============================================="
echo "Started at: $(date)"
echo ""

# Check if .env.prod exists
if [ ! -f ".env.prod" ]; then
    echo "ERROR: .env.prod file not found!"
    exit 1
fi

# Warning for production
echo ""
echo "!!! WARNING !!!"
echo "You are about to deploy to PRODUCTION!"
echo ""
echo "This will:"
echo "  1. Pull latest code from git"
echo "  2. Stop the running API"
echo "  3. Rebuild Docker image"
echo "  4. Start new API container"
echo ""
echo "Make sure:"
echo "  - Database backup is taken"
echo "  - .env.prod is correctly configured"
echo "  - You have tested changes in TEST environment"
echo ""
read -p "Continue with PRODUCTION deployment? (yes/N): " confirm
if [ "$confirm" != "yes" ]; then
    echo "Cancelled."
    exit 0
fi

# Step 1: Pull latest code
echo "[1/5] Pulling latest code from git..."
git pull origin main || echo "Warning: Git pull failed or no remote configured"

# Step 2: Stop existing containers
echo "[2/5] Stopping existing containers..."
docker-compose -f docker-compose.prod.yml --env-file .env.prod down || true

# Step 3: Build new images (no cache for fresh build)
echo "[3/5] Building new Docker images..."
docker-compose -f docker-compose.prod.yml --env-file .env.prod build --no-cache

# Step 4: Start containers
echo "[4/5] Starting containers..."
docker-compose -f docker-compose.prod.yml --env-file .env.prod up -d

# Step 5: Wait and verify
echo "[5/5] Verifying deployment..."
sleep 10

# Check health
echo ""
echo "Checking health endpoint..."
HEALTH_RESPONSE=$(curl -s http://localhost:8081/health || echo "FAILED")
echo "Health response: $HEALTH_RESPONSE"

# Show status
echo ""
echo "=============================================="
echo "Container Status:"
echo "=============================================="
docker-compose -f docker-compose.prod.yml --env-file .env.prod ps

echo ""
echo "=============================================="
echo "PRODUCTION Deployment Complete!"
echo "=============================================="
echo "Finished at: $(date)"
echo ""
echo "API:    http://localhost:8081"
echo "Health: http://localhost:8081/health"
echo ""
echo "Don't forget to verify the application is working correctly!"
echo "=============================================="
