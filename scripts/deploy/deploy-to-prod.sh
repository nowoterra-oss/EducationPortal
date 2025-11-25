#!/bin/bash
# ============================================================
# EduPortal - Deploy to PRODUCTION Server
# ============================================================
# Usage: ./scripts/deploy/deploy-to-prod.sh [image-tag] [--no-backup]
#
# Environment variables required:
#   PROD_SERVER_HOST   - Production server hostname/IP
#   PROD_SERVER_USER   - SSH username
#   SSH_KEY_PATH       - Path to SSH private key
#   DOCKERHUB_USERNAME - Docker Hub username
# ============================================================

set -e

# Configuration
IMAGE_TAG="${1:-prod-latest}"
SKIP_BACKUP=false
PROJECT_DIR="/opt/eduportal"
BACKUP_DIR="/opt/eduportal/backups"

# Parse arguments
for arg in "$@"; do
    case $arg in
        --no-backup)
            SKIP_BACKUP=true
            shift
            ;;
    esac
done

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

log_info() { echo -e "${GREEN}[INFO]${NC} $1"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }
log_step() { echo -e "${BLUE}[STEP]${NC} $1"; }

# Validate environment variables
validate_env() {
    local missing=0

    if [ -z "$PROD_SERVER_HOST" ]; then
        log_error "PROD_SERVER_HOST is not set"
        missing=1
    fi

    if [ -z "$PROD_SERVER_USER" ]; then
        log_error "PROD_SERVER_USER is not set"
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

# Confirm production deployment
confirm_deployment() {
    log_warn "========================================"
    log_warn "⚠️  PRODUCTION DEPLOYMENT"
    log_warn "========================================"
    log_warn "Server: $PROD_SERVER_HOST"
    log_warn "Image:  ${DOCKERHUB_USERNAME}/eduportal-api:${IMAGE_TAG}"
    log_warn "========================================"
    echo ""

    read -p "Are you sure you want to deploy to PRODUCTION? (yes/no): " confirm
    if [ "$confirm" != "yes" ]; then
        log_info "Deployment cancelled."
        exit 0
    fi
}

# Create backup
create_backup() {
    if [ "$SKIP_BACKUP" = true ]; then
        log_warn "Skipping backup (--no-backup flag set)"
        return 0
    fi

    log_step "Creating pre-deployment backup..."

    $SSH_CMD << 'ENDSSH'
        TIMESTAMP=$(date +'%Y%m%d_%H%M%S')
        BACKUP_PATH="/opt/eduportal/backups/${TIMESTAMP}"
        mkdir -p $BACKUP_PATH

        # Save current image info
        docker inspect eduportal-api-prod --format='{{.Config.Image}}' > $BACKUP_PATH/previous_image.txt 2>/dev/null || echo "No previous image"

        # Save current container ID
        docker ps -q --filter "name=eduportal-api-prod" > $BACKUP_PATH/previous_container.txt 2>/dev/null || true

        echo "Backup created at: $BACKUP_PATH"
ENDSSH
}

# Health check with retries
health_check() {
    local max_retries=10
    local retry_count=0

    log_step "Running health checks..."

    while [ $retry_count -lt $max_retries ]; do
        sleep 5
        HEALTH_STATUS=$($SSH_CMD "curl -s -o /dev/null -w '%{http_code}' http://localhost:8081/health" || echo "000")

        if [ "$HEALTH_STATUS" = "200" ]; then
            log_info "✅ Health check passed!"
            return 0
        fi

        retry_count=$((retry_count + 1))
        log_warn "Health check attempt $retry_count/$max_retries - Status: $HEALTH_STATUS"
    done

    log_error "❌ Health check failed after $max_retries attempts!"
    return 1
}

# Rollback function
rollback() {
    log_error "Deployment failed! Initiating rollback..."

    $SSH_CMD << 'ENDSSH'
        cd /opt/eduportal

        # Find previous image
        PREV_IMAGE=$(cat /opt/eduportal/backups/*/previous_image.txt 2>/dev/null | tail -1)

        if [ -n "$PREV_IMAGE" ]; then
            echo "Rolling back to: $PREV_IMAGE"
            docker-compose -f docker-compose.prod.yml --env-file .env.prod stop api-prod
            docker tag $PREV_IMAGE ${DOCKERHUB_USERNAME}/eduportal-api:prod-latest 2>/dev/null || true
            docker-compose -f docker-compose.prod.yml --env-file .env.prod up -d api-prod
            echo "Rollback completed."
        else
            echo "No previous image found for rollback!"
        fi
ENDSSH
}

# Main deployment function
deploy() {
    log_info "========================================"
    log_info "🚀 Deploying EduPortal to PRODUCTION"
    log_info "========================================"
    log_info "Server: $PROD_SERVER_HOST"
    log_info "Image Tag: $IMAGE_TAG"
    log_info "Timestamp: $(date)"
    log_info "========================================"

    # SSH command wrapper
    SSH_CMD="ssh -i $SSH_KEY_PATH -o StrictHostKeyChecking=no ${PROD_SERVER_USER}@${PROD_SERVER_HOST}"

    # Step 1: Create backup
    create_backup

    # Step 2: Pull new image
    log_step "[1/5] Pulling Docker image..."
    $SSH_CMD "docker pull ${DOCKERHUB_USERNAME}/eduportal-api:${IMAGE_TAG}"

    # Step 3: Stop current container (graceful)
    log_step "[2/5] Gracefully stopping current container..."
    $SSH_CMD "cd ${PROJECT_DIR} && docker-compose -f docker-compose.prod.yml --env-file .env.prod stop api-prod" || true

    # Step 4: Tag and start new container
    log_step "[3/5] Starting new container..."
    if [ "$IMAGE_TAG" != "prod-latest" ]; then
        $SSH_CMD "docker tag ${DOCKERHUB_USERNAME}/eduportal-api:${IMAGE_TAG} ${DOCKERHUB_USERNAME}/eduportal-api:prod-latest"
    fi
    $SSH_CMD "cd ${PROJECT_DIR} && docker-compose -f docker-compose.prod.yml --env-file .env.prod up -d api-prod"

    # Step 5: Health check
    log_step "[4/5] Waiting for startup..."
    sleep 15

    if health_check; then
        log_step "[5/5] Cleaning up..."
        $SSH_CMD "docker image prune -f" || true

        log_info "========================================"
        log_info "✅ PRODUCTION Deployment Successful!"
        log_info "========================================"
        log_info "API URL: http://${PROD_SERVER_HOST}:8081"
        log_info "Health:  http://${PROD_SERVER_HOST}:8081/health"
        log_info "========================================"

        return 0
    else
        rollback

        log_error "========================================"
        log_error "❌ Deployment Failed - Rolled back!"
        log_error "========================================"

        # Show logs
        log_info "Container logs:"
        $SSH_CMD "cd ${PROJECT_DIR} && docker-compose -f docker-compose.prod.yml --env-file .env.prod logs --tail=50 api-prod"

        return 1
    fi
}

# Run
validate_env
confirm_deployment
deploy
