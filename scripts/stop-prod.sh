#!/bin/bash
# ============================================================
# EduPortal - Stop PRODUCTION Environment
# ============================================================
# Usage: ./scripts/stop-prod.sh
# ============================================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

cd "$PROJECT_DIR"

echo "=============================================="
echo "Stopping EduPortal PRODUCTION Environment..."
echo "=============================================="

docker-compose -f docker-compose.prod.yml --env-file .env.prod down

echo ""
echo "=============================================="
echo "PRODUCTION Environment Stopped!"
echo "=============================================="
