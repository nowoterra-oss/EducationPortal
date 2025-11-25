#!/bin/bash
# ============================================================
# EduPortal - View PRODUCTION Environment Logs
# ============================================================
# Usage: ./scripts/logs-prod.sh
# ============================================================

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

cd "$PROJECT_DIR"

echo "=============================================="
echo "EduPortal PRODUCTION Environment Logs"
echo "=============================================="
echo "Press Ctrl+C to exit"
echo ""

docker-compose -f docker-compose.prod.yml --env-file .env.prod logs -f
