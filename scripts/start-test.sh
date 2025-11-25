#!/bin/bash
# ============================================================
# EduPortal - Start TEST Environment
# ============================================================
# Usage: ./scripts/start-test.sh
# ============================================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

cd "$PROJECT_DIR"

echo "=============================================="
echo "Starting EduPortal TEST Environment..."
echo "=============================================="

# Check if .env.test exists
if [ ! -f ".env.test" ]; then
    echo "ERROR: .env.test file not found!"
    echo "Please copy .env.example to .env.test and configure it."
    exit 1
fi

# Build and start containers
echo "[1/3] Building Docker images..."
docker-compose -f docker-compose.test.yml --env-file .env.test build

echo "[2/3] Starting containers..."
docker-compose -f docker-compose.test.yml --env-file .env.test up -d

echo "[3/3] Waiting for services to be healthy..."
sleep 5

# Check container status
echo ""
echo "=============================================="
echo "Container Status:"
echo "=============================================="
docker-compose -f docker-compose.test.yml --env-file .env.test ps

echo ""
echo "=============================================="
echo "TEST Environment Started!"
echo "=============================================="
echo "API:   http://localhost:8080"
echo "MSSQL: localhost:1433"
echo "Health: http://localhost:8080/health"
echo ""
echo "To view logs: ./scripts/logs-test.sh"
echo "To stop: ./scripts/stop-test.sh"
echo "=============================================="
