#!/bin/bash
# ============================================================
# EduPortal - Restart TEST Environment
# ============================================================
# Usage: ./scripts/restart-test.sh
# ============================================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

cd "$PROJECT_DIR"

echo "=============================================="
echo "Restarting EduPortal TEST Environment..."
echo "=============================================="

docker-compose -f docker-compose.test.yml --env-file .env.test restart

echo ""
echo "=============================================="
echo "Container Status:"
echo "=============================================="
docker-compose -f docker-compose.test.yml --env-file .env.test ps

echo ""
echo "TEST Environment Restarted!"
echo "=============================================="
