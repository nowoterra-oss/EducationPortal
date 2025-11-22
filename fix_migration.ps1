# PowerShell Migration Fix Script

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Migration Fix Script" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Step 1: Removing problematic migration..." -ForegroundColor Yellow
try {
    dotnet ef migrations remove --project src/EduPortal.Infrastructure --startup-project src/EduPortal.API --force
    Write-Host "✓ Migration removed successfully" -ForegroundColor Green
}
catch {
    Write-Host "✗ Failed to remove migration" -ForegroundColor Red
    Write-Host "Trying alternative approach..." -ForegroundColor Yellow

    # Migration dosyasını bul ve sil
    $migrationFile = Get-ChildItem -Path "src/EduPortal.Infrastructure/Migrations" -Filter "*AddPaymentPlanSystem.cs" -ErrorAction SilentlyContinue | Select-Object -First 1
    $migrationDesigner = Get-ChildItem -Path "src/EduPortal.Infrastructure/Migrations" -Filter "*AddPaymentPlanSystem.Designer.cs" -ErrorAction SilentlyContinue | Select-Object -First 1

    if ($migrationFile) {
        Write-Host "Found migration file: $($migrationFile.FullName)" -ForegroundColor Yellow
        Remove-Item $migrationFile.FullName -Force
        if ($migrationDesigner) {
            Remove-Item $migrationDesigner.FullName -Force
        }
        Write-Host "✓ Migration files deleted manually" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "Step 2: Creating new migration..." -ForegroundColor Yellow
$createResult = dotnet ef migrations add AddPaymentPlanSystem --project src/EduPortal.Infrastructure --startup-project src/EduPortal.API 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Migration created successfully" -ForegroundColor Green
}
else {
    Write-Host "✗ Failed to create migration" -ForegroundColor Red
    Write-Host $createResult -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Step 3: Applying migration to database..." -ForegroundColor Yellow
$updateResult = dotnet ef database update --project src/EduPortal.Infrastructure --startup-project src/EduPortal.API 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Database updated successfully" -ForegroundColor Green
    Write-Host ""
    Write-Host "=========================================" -ForegroundColor Green
    Write-Host "Migration fix completed successfully!" -ForegroundColor Green
    Write-Host "=========================================" -ForegroundColor Green
}
else {
    Write-Host "✗ Failed to update database" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please check the migration file and ensure:" -ForegroundColor Yellow
    Write-Host "  1. No duplicate column additions" -ForegroundColor White
    Write-Host "  2. All foreign keys are valid" -ForegroundColor White
    Write-Host "  3. No circular references" -ForegroundColor White
    Write-Host ""
    Write-Host "Error details:" -ForegroundColor Yellow
    Write-Host $updateResult -ForegroundColor Red
    exit 1
}
