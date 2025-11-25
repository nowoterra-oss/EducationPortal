#!/bin/bash
# ============================================================
# EduPortal - Rollback PRODUCTION
# ============================================================
# Usage: ./scripts/deploy/rollback-prod.sh [version]
#
# Examples:
#   ./scripts/deploy/rollback-prod.sh              # Rollback to previous
#   ./scripts/deploy/rollback-prod.sh v1.0.0       # Rollback to specific version
#   ./scripts/deploy/rollback-prod.sh prod-20240101-abc1234
#
# Environment variables required:
#   PROD_SERVER_HOST   - Production server hostname/IP
#   PROD_SERVER_USER   - SSH username
#   SSH_KEY_PATH       - Path to SSH private key
#   DOCKERHUB_USERNAME - Docker Hub username
# ============================================================

set -e

# Configuration
TARGET_VERSION="${1:-}"
PROJECT_DIR="/opt/eduportal"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

log_info() { echo -e "${GREEN}[INFO]${NC} $1"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# Validate environment
validate_env() {
    if [ -z "$PROD_SERVER_HOST" ] || [ -z "$PROD_SERVER_USER" ] || [ -z "$SSH_KEY_PATH" ]; then
        log_error "Required environment variables are not set"
        log_error "Please set: PROD_SERVER_HOST, PROD_SERVER_USER, SSH_KEY_PATH, DOCKERHUB_USERNAME"
        exit 1
    fi
}

# Confirm rollback
confirm_rollback() {
    log_warn "========================================"
    log_warn "⚠️  PRODUCTION ROLLBACK"
    log_warn "========================================"

    if [ -n "$TARGET_VERSION" ]; then
        log_warn "Target Version: $TARGET_VERSION"
    else
        log_warn "Target: Previous version"
    fi

    log_warn "========================================"
    echo ""

    read -p "Enter reason for rollback: " REASON

    if [ -z "$REASON" ]; then
        log_error "Reason is required for rollback"
        exit 1
    fi

    read -p "Confirm PRODUCTION rollback? (yes/no): " confirm
    if [ "$confirm" != "yes" ]; then
        log_info "Rollback cancelled."
        exit 0
    fi

    # Log rollback
    echo "$(date) - Rollback initiated by $USER - Reason: $REASON" >> /tmp/eduportal-rollback.log
}

# Perform rollback
rollback() {
    SSH_CMD="ssh -i $SSH_KEY_PATH -o StrictHostKeyChecking=no ${PROD_SERVER_USER}@${PROD_SERVER_HOST}"

    log_info "========================================"
    log_info "🔄 Starting PRODUCTION Rollback"
    log_info "========================================"

    if [ -n "$TARGET_VERSION" ]; then
        # Rollback to specific version
        log_info "Rolling back to version: $TARGET_VERSION"

        TARGET_IMAGE="${DOCKERHUB_USERNAME}/eduportal-api:${TARGET_VERSION}"

        # Pull target image
        log_info "[1/4] Pulling target image..."
        $SSH_CMD "docker pull $TARGET_IMAGE" || {
            log_error "Failed to pull image: $TARGET_IMAGE"
            log_error "Available versions:"
            # List available tags (if Docker Hub API is accessible)
            exit 1
        }

        # Stop current container
        log_info "[2/4] Stopping current container..."
        $SSH_CMD "cd ${PROJECT_DIR} && docker-compose -f docker-compose.prod.yml --env-file .env.prod stop api-prod"

        # Tag as prod-latest
        log_info "[3/4] Tagging and starting rolled-back version..."
        $SSH_CMD "docker tag $TARGET_IMAGE ${DOCKERHUB_USERNAME}/eduportal-api:prod-latest"
        $SSH_CMD "cd ${PROJECT_DIR} && docker-compose -f docker-compose.prod.yml --env-file .env.prod up -d api-prod"

    else
        # Rollback to previous version
        log_info "Rolling back to previous version..."

        $SSH_CMD << 'ENDSSH'
            cd /opt/eduportal

            # Find previous image
            PREV_IMAGE=$(find /opt/eduportal/backups -name "previous_image.txt" -exec cat {} \; 2>/dev/null | tail -1)

            if [ -z "$PREV_IMAGE" ]; then
                echo "ERROR: No previous image found!"
                exit 1
            fi

            echo "Found previous image: $PREV_IMAGE"

            # Stop current container
            echo "[2/4] Stopping current container..."
            docker-compose -f docker-compose.prod.yml --env-file .env.prod stop api-prod

            # Start previous version
            echo "[3/4] Starting previous version..."
            docker tag $PREV_IMAGE ${DOCKERHUB_USERNAME}/eduportal-api:prod-latest 2>/dev/null || true
            docker-compose -f docker-compose.prod.yml --env-file .env.prod up -d api-prod
ENDSSH
    fi

    # Health check
    log_info "[4/4] Verifying rollback..."
    sleep 15

    MAX_RETRIES=10
    RETRY_COUNT=0

    while [ $RETRY_COUNT -lt $MAX_RETRIES ]; do
        HEALTH_STATUS=$($SSH_CMD "curl -s -o /dev/null -w '%{http_code}' http://localhost:8081/health" || echo "000")

        if [ "$HEALTH_STATUS" = "200" ]; then
            log_info "========================================"
            log_info "✅ Rollback Successful!"
            log_info "========================================"
            log_info "API URL: http://${PROD_SERVER_HOST}:8081"
            log_info "Health:  http://${PROD_SERVER_HOST}:8081/health"
            return 0
        fi

        RETRY_COUNT=$((RETRY_COUNT + 1))
        log_warn "Health check attempt $RETRY_COUNT/$MAX_RETRIES - Status: $HEALTH_STATUS"
        sleep 5
    done

    log_error "========================================"
    log_error "❌ Rollback Health Check Failed!"
    log_error "========================================"

    # Show logs
    $SSH_CMD "cd ${PROJECT_DIR} && docker-compose -f docker-compose.prod.yml --env-file .env.prod logs --tail=50 api-prod"

    return 1
}

# Run
validate_env
confirm_rollback
rollback
