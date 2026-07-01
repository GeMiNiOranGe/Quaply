#Requires -Version 7.0
<#
.SYNOPSIS
Scaffolds EF Core models from the Quaply SQLite database.

.DESCRIPTION
Ensures the database exists, restores dotnet tools, then scaffolds EF Core
models into Quaply.Data. Use -Force to reset the database and overwrite
existing models.

.PARAMETER Force
Resets the database and overwrites existing scaffolded models.

.EXAMPLE
.\Invoke-Scaffold.ps1
.\Invoke-Scaffold.ps1 -Force
#>
param(
    [switch]$Force
)

$databaseDirectory = Join-Path $PSScriptRoot ".." "Database"
$databasePath = Join-Path $databaseDirectory "Quaply.db"

# Prerequisites ----------------------------------------------------------------

if (-not (Get-Command -Name "dotnet" -ErrorAction SilentlyContinue)) {
    Write-Host "dotnet CLI is not installed or not in the PATH." `
        -ForegroundColor Red
    exit 1
}

# Database ---------------------------------------------------------------------

$invokeDatabase = Join-Path $PSScriptRoot "Invoke-Database.ps1"

if ($Force) {
    & $invokeDatabase -Action Reset
}
elseif (-not (Test-Path $databasePath)) {
    & $invokeDatabase -Action Initialize
}

# Tool restore -----------------------------------------------------------------

# Husky.NET runs `dotnet tool restore` only after a successful build, so tools
# must be restored manually here to break the circular dependency.
dotnet tool restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "dotnet tool restore failed." -ForegroundColor Red
    exit 1
}

# Scaffold ---------------------------------------------------------------------

$generatedDirectory = "Generated"
$dataProjectName = "Quaply.Data"
$newDatabasePath = Join-Path $PSScriptRoot ".." `
    $dataProjectName "Database" "Quaply.db"

# Ensure destination directory exists
$newDatabaseDir = Split-Path -Path $newDatabasePath -Parent
if (-not (Test-Path $newDatabaseDir)) {
    New-Item -ItemType Directory -Path $newDatabaseDir -Force | Out-Null
}

# Only move if source exists; use -Force to overwrite existing file
if (Test-Path $databasePath) {
    try {
        Move-Item `
            -Path $databasePath `
            -Destination $newDatabasePath `
            -Force `
            -ErrorAction Stop
    }
    catch {
        Write-Host "Failed to move database: $($_.Exception.Message)" `
            -ForegroundColor Red
        exit 1
    }
}
else {
    Write-Host "Source database not found at $databasePath" `
        -ForegroundColor Yellow
}

$dataDatabasePath = $newDatabasePath

$scaffoldArgs = @(
    "ef", "dbcontext", "scaffold",
    "Data Source=$dataDatabasePath",
    "Microsoft.EntityFrameworkCore.Sqlite",
    "--startup-project", "Quaply.Ui",
    "--project", $dataProjectName,
    "--namespace", "$dataProjectName.Models",
    "--context-namespace", "$dataProjectName.Contexts",
    "--context", "QuaplyDbContext",
    "--context-dir", (Join-Path $generatedDirectory "Contexts"),
    "--output-dir", (Join-Path $generatedDirectory "Models"),
    "--no-onconfiguring",
    # skip build; models may not exist yet
    "--no-build"
)

if ($Force) {
    $scaffoldArgs += "--force"
}

dotnet @scaffoldArgs
