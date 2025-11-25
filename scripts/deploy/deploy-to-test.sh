#!/bin/bash
# ============================================================
# EduPortal - Deploy to TEST Server
# ============================================================
# Usage: ./scripts/deploy/deploy-to-test.sh [image-tag]
#
# Environment variables required:
#   TEST_SERVER_HOST - Test server hostname/IP
#   TEST_SERVER_USER - SSH username
#   SSH_KEY_PATH     - Path to SSH private key
#   DOCKERHUB_USERNAME - Docker Hub username
# ============================================================

set -e

# Configuration
IMAGE_TAG="${1:-latest}"
PROJECT_DIR="/opt/eduportal"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

log_info() { echo -e "${GREEN}[INFO]${NC} $1"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# Validate environment variables
validate_env() {
    local missing=0

    if [ -z "$TEST_SERVER_HOST" ]; then
        log_error "TEST_SERVER_HOST is not set"
        missing=1
    fi

    if [ -z "$TEST_SERVER_USER" ]; then
        log_error "TEST_SERVER_USER is not set"
        missing=1
    fi

    if [ -z "$SSH_KEY_PATH" ]; then
        log_error "SSH_KEY_PATH is not set"
        missing=1
    fi

    if [ -z "$DOCKERHUB_USERNAME" ]; then
        log_error "DOCKERHUB_USERNAME is not set"
        missing=1
    fi

    if [ $missing -eq 1 ]; then
        exit 1
    fi
}

# Main deployment function
deploy() {
    log_info "========================================"
    log_info "Deploying EduPortal to TEST"
    log_info "========================================"
    log_info "Server: $TEST_SERVER_HOST"
    log_info "Image Tag: $IMAGE_TAG"
    log_info "========================================"

    # SSH command wrapper
    SSH_CMD="ssh -i $SSH_KEY_PATH -o StrictHostKeyChecking=no ${TEST_SERVER_USER}@${TEST_SERVER_HOST}"

    # Step 1: Pull latest image
    log_info "[1/5] Pulling Docker image..."
    $SSH_CMD "docker pull ${DOCKERHUB_USERNAME}/eduportal-api:${IMAGE_TAG}"

    # Step 2: Stop current containers
    log_info "[2/5] Stopping current containers..."
    $SSH_CMD "cd ${PROJECT_DIR} && docker-compose -f docker-compose.test.yml --env-file .env.test down" || true

    # Step 3: Tag new image
    if [ "$IMAGE_TAG" != "latest" ]; then
        log_info "[3/5] Tagging image as latest..."
        $SSH_CMD "docker tag ${DOCKERHUB_USERNAME}/eduportal-api:${IMAGE_TAG} ${DOCKERHUB_USERNAME}/eduportal-api:latest"
    else
        log_info "[3/5] Using latest tag..."
    fi

    # Step 4: Start new containers
    log_info "[4/5] Starting new containers..."
    $SSH_CMD "cd ${PROJECT_DIR} && docker-compose -f docker-compose.test.yml --env-file .env.test up -d"

    # Step 5: Health check
    log_info "[5/5] Running health check..."
    sleep 15

    HEALTH_STATUS=$($SSH_CMD "curl -s -o /dev/null -w '%{http_code}' http://localhost:8080/health" || echo "000")

    if [ "$HEALTH_STATUS" = "200" ]; then
        log_info "========================================"
        log_info "✅ Deployment Successful!"
        log_info "========================================"
        log_info "API URL: http://${TEST_SERVER_HOST}:8080"
        log_info "Health:  http://${TEST_SERVER_HOST}:8080/health"

        # Clean up old images
        $SSH_CMD "docker image prune -f" || true

        return 0
    else
        log_error "========================================"
        log_error "❌ Deployment Failed!"
        log_error "Health check returned: $HEALTH_STATUS"
        log_error "========================================"

        # Show logs for debugging
        log_info "Container logs:"
        $SSH_CMD "cd ${PROJECT_DIR} && docker-compose -f docker-compose.test.yml --env-file .env.test logs --tail=50"

        return 1
    fi
}

# Run
validate_env
deploy
