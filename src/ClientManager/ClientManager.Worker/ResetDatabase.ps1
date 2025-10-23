<#
.SYNOPSIS
    Drops and recreates the local development database and migrations.
    WARNING: This is intended for local development only.

.DESCRIPTION
    - Verifies not running in production.
    - Drops the existing database.
    - Deletes the EF Core Migrations folder.
    - Adds a fresh InitialCreate migration.
    - Updates the database to match current models.

.NOTES
    Run from the project root where your .csproj file exists.
#>

# Stop on error
$ErrorActionPreference = "Stop"

# Safety check
if ($env:ASPNETCORE_ENVIRONMENT -eq "Production") {
    Write-Host "❌ Refusing to reset database in Production environment." -ForegroundColor Red
    exit 1
}

Write-Host "⚙️  Resetting Entity Framework database and migrations..." -ForegroundColor Cyan

# Drop the database (force = skip confirmation)
dotnet ef database drop -f

# Remove migrations folder if it exists
$migrationsPath = Join-Path (Get-Location) "Migrations"
if (Test-Path $migrationsPath) {
    Write-Host "🧹 Removing existing migrations..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $migrationsPath
}

# Create a new initial migration
Write-Host "🧱 Creating new migration: InitialCreate" -ForegroundColor Cyan
dotnet ef migrations add InitialCreate --startup-project ./

# Apply migration (recreate DB)
Write-Host "💾 Updating database..." -ForegroundColor Cyan
dotnet ef database update --startup-project ./

Write-Host "✅ Database reset complete and synchronized with current models." -ForegroundColor Green
