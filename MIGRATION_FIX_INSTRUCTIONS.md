# Migration Fix - ActualCost Duplicate Column Issue

## Problem
Migration tries to add `ActualCost` column to `StudyAbroadPrograms` table, but it already exists in the database.

## Solution

### Step 1: Open Migration File
Open: `src/EduPortal.Infrastructure/Migrations/20251122191049_AddPaymentPlanSystem.cs`

### Step 2: Find and Remove Duplicate ActualCost Operations

#### In the `Up` method:
Find and **DELETE** these lines:

```csharp
migrationBuilder.AddColumn<decimal>(
    name: "ActualCost",
    table: "StudyAbroadPrograms",
    type: "decimal(18,2)",
    nullable: true);
```

#### In the `Down` method:
Find and **DELETE** these lines:

```csharp
migrationBuilder.DropColumn(
    name: "ActualCost",
    table: "StudyAbroadPrograms");
```

### Step 3: Save and Apply Migration

```bash
dotnet ef database update --project src/EduPortal.Infrastructure --startup-project src/EduPortal.API
```

---

## Alternative: Let Me Fix It

If you prefer, commit the migration file and push it, then I can edit it for you:

```bash
git add src/EduPortal.Infrastructure/Migrations/
git commit -m "Add payment plan migration (needs fix)"
git push
```

Then I'll fix it and push the corrected version.
