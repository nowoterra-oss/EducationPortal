#!/bin/bash
# ============================================================
# EduPortal - View TEST Environment Logs
# ============================================================
# Usage: ./scripts/logs-test.sh [service]
# Examples:
#   ./scripts/logs-test.sh          # All services
#   ./scripts/logs-test.sh api-test # API only
#   ./scripts/logs-test.sh mssql-test # MSSQL only
# ============================================================

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

cd "$PROJECT_DIR"

SERVICE=${1:-""}

echo "=============================================="
echo "EduPortal TEST Environment Logs"
echo "=============================================="
echo "Press Ctrl+C to exit"
echo ""

if [ -z "$SERVICE" ]; then
    docker-compose -f docker-compose.test.yml --env-file .env.test logs -f
else
    docker-compose -f docker-compose.test.yml --env-file .env.test logs -f "$SERVICE"
fi
