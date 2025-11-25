#!/bin/bash
# ============================================================
# EduPortal - Health Check Script
# ============================================================
# Usage: ./scripts/deploy/health-check.sh <url> [timeout] [retries]
#
# Examples:
#   ./scripts/deploy/health-check.sh http://localhost:8080/health
#   ./scripts/deploy/health-check.sh http://api.example.com/health 30 5
# ============================================================

set -e

# Configuration
HEALTH_URL="${1:-http://localhost:8080/health}"
TIMEOUT="${2:-10}"
MAX_RETRIES="${3:-5}"
RETRY_DELAY=5

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

log_info() { echo -e "${GREEN}[INFO]${NC} $1"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# Parse URL components
parse_url() {
    # Extract host and port from URL
    HOST=$(echo "$HEALTH_URL" | sed -e 's|^[^/]*//||' -e 's|/.*$||' -e 's|:.*$||')
    PORT=$(echo "$HEALTH_URL" | grep -oP ':\K[0-9]+' || echo "80")

    echo "Target: $HEALTH_URL"
    echo "Host: $HOST"
    echo "Port: $PORT"
}

# Check if host is reachable
check_connectivity() {
    log_info "Checking connectivity to $HOST:$PORT..."

    if command -v nc &> /dev/null; then
        nc -zv $HOST $PORT -w $TIMEOUT 2>&1 || {
            log_error "Cannot connect to $HOST:$PORT"
            return 1
        }
    fi

    log_info "Host is reachable"
    return 0
}

# Perform health check
health_check() {
    local retry_count=0

    log_info "========================================"
    log_info "Starting Health Check"
    log_info "========================================"
    log_info "URL: $HEALTH_URL"
    log_info "Timeout: ${TIMEOUT}s"
    log_info "Max Retries: $MAX_RETRIES"
    log_info "========================================"

    while [ $retry_count -lt $MAX_RETRIES ]; do
        retry_count=$((retry_count + 1))
        log_info "Attempt $retry_count/$MAX_RETRIES..."

        # Make request
        RESPONSE=$(curl -s -o /tmp/health_response.json -w "%{http_code}" \
            --connect-timeout $TIMEOUT \
            --max-time $((TIMEOUT * 2)) \
            "$HEALTH_URL" 2>/dev/null || echo "000")

        # Check HTTP status
        if [ "$RESPONSE" = "200" ]; then
            log_info "✅ Health check PASSED (HTTP $RESPONSE)"

            # Parse response
            if [ -f /tmp/health_response.json ]; then
                echo ""
                log_info "Response:"
                cat /tmp/health_response.json | python3 -m json.tool 2>/dev/null || cat /tmp/health_response.json
                echo ""
            fi

            log_info "========================================"
            log_info "✅ Service is healthy!"
            log_info "========================================"

            return 0
        fi

        log_warn "Health check failed (HTTP $RESPONSE)"

        # Show response body for debugging
        if [ -f /tmp/health_response.json ] && [ -s /tmp/health_response.json ]; then
            log_warn "Response body:"
            cat /tmp/health_response.json
        fi

        # Wait before retry (unless last attempt)
        if [ $retry_count -lt $MAX_RETRIES ]; then
            log_info "Waiting ${RETRY_DELAY}s before retry..."
            sleep $RETRY_DELAY
        fi
    done

    log_error "========================================"
    log_error "❌ Health check FAILED after $MAX_RETRIES attempts"
    log_error "========================================"

    return 1
}

# Detailed health check
detailed_check() {
    log_info "Running detailed health check..."

    # DNS resolution
    echo ""
    log_info "DNS Resolution:"
    nslookup $HOST 2>/dev/null || host $HOST 2>/dev/null || echo "DNS lookup not available"

    # TCP connection test
    echo ""
    log_info "TCP Connection Test:"
    if command -v nc &> /dev/null; then
        timeout $TIMEOUT nc -zv $HOST $PORT 2>&1 || echo "Connection test failed"
    fi

    # HTTP headers
    echo ""
    log_info "HTTP Response Headers:"
    curl -sI --connect-timeout $TIMEOUT "$HEALTH_URL" 2>/dev/null || echo "Failed to get headers"

    # Response time
    echo ""
    log_info "Response Time:"
    curl -s -o /dev/null -w "DNS: %{time_namelookup}s\nConnect: %{time_connect}s\nTTFB: %{time_starttransfer}s\nTotal: %{time_total}s\n" \
        --connect-timeout $TIMEOUT "$HEALTH_URL" 2>/dev/null || echo "Failed to measure response time"
}

# Main
main() {
    parse_url

    # Check connectivity first
    check_connectivity || {
        detailed_check
        exit 1
    }

    # Perform health check
    health_check || {
        log_error "Health check failed. Running detailed diagnostics..."
        detailed_check
        exit 1
    }
}

main
