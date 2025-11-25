#!/bin/bash
# ============================================================
# EduPortal - Check Status of All Environments
# ============================================================
# Usage: ./scripts/status.sh
# ============================================================

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

cd "$PROJECT_DIR"

echo "=============================================="
echo "EduPortal Environment Status"
echo "=============================================="
echo "Checked at: $(date)"
echo ""

# Check TEST environment
echo "----------------------------------------------"
echo "TEST Environment (Port 8080):"
echo "----------------------------------------------"
if docker ps --format '{{.Names}}' | grep -q "eduportal-api-test"; then
    echo "API Container: RUNNING"
    HEALTH=$(curl -s http://localhost:8080/health 2>/dev/null || echo "UNREACHABLE")
    echo "Health Check: $HEALTH"
else
    echo "API Container: STOPPED"
fi

if docker ps --format '{{.Names}}' | grep -q "eduportal-mssql-test"; then
    echo "MSSQL Container: RUNNING"
else
    echo "MSSQL Container: STOPPED"
fi

echo ""

# Check PRODUCTION environment
echo "----------------------------------------------"
echo "PRODUCTION Environment (Port 8081):"
echo "----------------------------------------------"
if docker ps --format '{{.Names}}' | grep -q "eduportal-api-prod"; then
    echo "API Container: RUNNING"
    HEALTH=$(curl -s http://localhost:8081/health 2>/dev/null || echo "UNREACHABLE")
    echo "Health Check: $HEALTH"
else
    echo "API Container: STOPPED"
fi
echo "MSSQL: External Server (configured in .env.prod)"

echo ""

# Docker resources
echo "----------------------------------------------"
echo "Docker Resources:"
echo "----------------------------------------------"
echo "Containers:"
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | grep eduportal || echo "  No EduPortal containers running"

echo ""
echo "Images:"
docker images --format "table {{.Repository}}\t{{.Tag}}\t{{.Size}}" | grep eduportal || echo "  No EduPortal images found"

echo ""
echo "Volumes:"
docker volume ls --format "table {{.Name}}\t{{.Driver}}" | grep eduportal || echo "  No EduPortal volumes found"

echo ""
echo "=============================================="
