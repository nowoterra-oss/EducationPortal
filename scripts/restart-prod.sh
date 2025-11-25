#!/bin/bash
# ============================================================
# EduPortal - Restart PRODUCTION Environment
# ============================================================
# Usage: ./scripts/restart-prod.sh
# ============================================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

cd "$PROJECT_DIR"

echo "=============================================="
echo "Restarting EduPortal PRODUCTION Environment..."
echo "=============================================="

docker-compose -f docker-compose.prod.yml --env-file .env.prod restart

echo ""
echo "=============================================="
echo "Container Status:"
echo "=============================================="
docker-compose -f docker-compose.prod.yml --env-file .env.prod ps

echo ""
echo "PRODUCTION Environment Restarted!"
echo "=============================================="
