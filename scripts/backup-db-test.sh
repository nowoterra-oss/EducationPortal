#!/bin/bash
# ============================================================
# EduPortal - Backup TEST Database
# ============================================================
# Creates a SQL Server backup of the TEST database
# Usage: ./scripts/backup-db-test.sh
# ============================================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

cd "$PROJECT_DIR"

# Load environment variables
if [ -f ".env.test" ]; then
    export $(grep -v '^#' .env.test | xargs)
else
    echo "ERROR: .env.test file not found!"
    exit 1
fi

# Create backup directory
BACKUP_DIR="$PROJECT_DIR/backups"
mkdir -p "$BACKUP_DIR"

# Generate backup filename with timestamp
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
BACKUP_FILE="EduPortalDb_Test_${TIMESTAMP}.bak"

echo "=============================================="
echo "Backing up TEST Database..."
echo "=============================================="
echo "Database: ${DB_NAME}"
echo "Backup file: ${BACKUP_FILE}"
echo ""

# Create backup using sqlcmd in the MSSQL container
docker exec eduportal-mssql-test /opt/mssql-tools18/bin/sqlcmd \
    -S localhost \
    -U sa \
    -P "${MSSQL_SA_PASSWORD}" \
    -C \
    -Q "BACKUP DATABASE [${DB_NAME}] TO DISK = N'/var/opt/mssql/backup/${BACKUP_FILE}' WITH NOFORMAT, NOINIT, NAME = 'EduPortal-Test-Full', SKIP, NOREWIND, NOUNLOAD, STATS = 10"

# Copy backup from container to host
echo ""
echo "Copying backup to host..."
docker cp "eduportal-mssql-test:/var/opt/mssql/backup/${BACKUP_FILE}" "$BACKUP_DIR/"

# Verify backup file
if [ -f "$BACKUP_DIR/$BACKUP_FILE" ]; then
    BACKUP_SIZE=$(du -h "$BACKUP_DIR/$BACKUP_FILE" | cut -f1)
    echo ""
    echo "=============================================="
    echo "Backup Completed Successfully!"
    echo "=============================================="
    echo "File: $BACKUP_DIR/$BACKUP_FILE"
    echo "Size: $BACKUP_SIZE"
    echo ""

    # List recent backups
    echo "Recent backups:"
    ls -lh "$BACKUP_DIR"/*.bak 2>/dev/null | tail -5
else
    echo "ERROR: Backup file was not created!"
    exit 1
fi

# Optional: Clean up old backups (keep last 5)
echo ""
echo "Cleaning up old backups (keeping last 5)..."
cd "$BACKUP_DIR"
ls -t *.bak 2>/dev/null | tail -n +6 | xargs -r rm -v

echo ""
echo "Done!"
