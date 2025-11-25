#!/bin/bash
# ============================================================
# EduPortal - Stop TEST Environment
# ============================================================
# Usage: ./scripts/stop-test.sh
# ============================================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

cd "$PROJECT_DIR"

echo "=============================================="
echo "Stopping EduPortal TEST Environment..."
echo "=============================================="

docker-compose -f docker-compose.test.yml --env-file .env.test down

echo ""
echo "=============================================="
echo "TEST Environment Stopped!"
echo "=============================================="
