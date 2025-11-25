#!/bin/bash
# ============================================================
# EduPortal - Deploy TEST Environment
# ============================================================
# Full deployment: pull latest code, rebuild, restart
# Usage: ./scripts/deploy-test.sh
# ============================================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

cd "$PROJECT_DIR"

echo "=============================================="
echo "Deploying EduPortal TEST Environment..."
echo "=============================================="
echo "Started at: $(date)"
echo ""

# Check if .env.test exists
if [ ! -f ".env.test" ]; then
    echo "ERROR: .env.test file not found!"
    exit 1
fi

# Step 1: Pull latest code
echo "[1/5] Pulling latest code from git..."
git pull origin main || echo "Warning: Git pull failed or no remote configured"

# Step 2: Stop existing containers
echo "[2/5] Stopping existing containers..."
docker-compose -f docker-compose.test.yml --env-file .env.test down || true

# Step 3: Build new images (no cache for fresh build)
echo "[3/5] Building new Docker images..."
docker-compose -f docker-compose.test.yml --env-file .env.test build --no-cache

# Step 4: Start containers
echo "[4/5] Starting containers..."
docker-compose -f docker-compose.test.yml --env-file .env.test up -d

# Step 5: Wait and verify
echo "[5/5] Verifying deployment..."
sleep 10

# Check health
echo ""
echo "Checking health endpoint..."
HEALTH_RESPONSE=$(curl -s http://localhost:8080/health || echo "FAILED")
echo "Health response: $HEALTH_RESPONSE"

# Show status
echo ""
echo "=============================================="
echo "Container Status:"
echo "=============================================="
docker-compose -f docker-compose.test.yml --env-file .env.test ps

echo ""
echo "=============================================="
echo "TEST Deployment Complete!"
echo "=============================================="
echo "Finished at: $(date)"
echo ""
echo "API:    http://localhost:8080"
echo "Health: http://localhost:8080/health"
echo "=============================================="
