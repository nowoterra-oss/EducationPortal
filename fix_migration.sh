#!/bin/bash

echo "========================================="
echo "Migration Fix Script"
echo "========================================="
echo ""

# Renk kodları
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Step 1: Removing problematic migration...${NC}"
dotnet ef migrations remove --project src/EduPortal.Infrastructure --startup-project src/EduPortal.API --force

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Migration removed successfully${NC}"
else
    echo -e "${RED}✗ Failed to remove migration${NC}"
    echo -e "${YELLOW}Trying alternative approach...${NC}"

    # Migration dosyasını bul ve sil
    MIGRATION_FILE=$(find src/EduPortal.Infrastructure/Migrations -name "*AddPaymentPlanSystem.cs" 2>/dev/null | head -1)
    MIGRATION_DESIGNER=$(find src/EduPortal.Infrastructure/Migrations -name "*AddPaymentPlanSystem.Designer.cs" 2>/dev/null | head -1)

    if [ -n "$MIGRATION_FILE" ]; then
        echo "Found migration file: $MIGRATION_FILE"
        rm -f "$MIGRATION_FILE"
        rm -f "$MIGRATION_DESIGNER"
        echo -e "${GREEN}✓ Migration files deleted manually${NC}"
    fi
fi

echo ""
echo -e "${YELLOW}Step 2: Creating new migration...${NC}"
dotnet ef migrations add AddPaymentPlanSystem --project src/EduPortal.Infrastructure --startup-project src/EduPortal.API

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Migration created successfully${NC}"
else
    echo -e "${RED}✗ Failed to create migration${NC}"
    exit 1
fi

echo ""
echo -e "${YELLOW}Step 3: Applying migration to database...${NC}"
dotnet ef database update --project src/EduPortal.Infrastructure --startup-project src/EduPortal.API

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Database updated successfully${NC}"
    echo ""
    echo -e "${GREEN}=========================================${NC}"
    echo -e "${GREEN}Migration fix completed successfully!${NC}"
    echo -e "${GREEN}=========================================${NC}"
else
    echo -e "${RED}✗ Failed to update database${NC}"
    echo ""
    echo -e "${YELLOW}Please check the migration file and ensure:${NC}"
    echo "  1. No duplicate column additions"
    echo "  2. All foreign keys are valid"
    echo "  3. No circular references"
    exit 1
fi
