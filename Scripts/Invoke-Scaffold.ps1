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

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "Common" "ConsoleOutput.ps1")

$script:DatabaseDirectory = Join-Path $PSScriptRoot ".." "Database"
$script:DatabasePath = Join-Path $script:DatabaseDirectory "Quaply.db"

$script:DataProjectName = "Quaply.Data"
$script:GeneratedDirectory = "Generated"
$script:NewDatabasePath = `
    Join-Path $PSScriptRoot ".." $script:DataProjectName "Database" "Quaply.db"

# ------------------------------------------------------------------------------
# Prerequisites
# ------------------------------------------------------------------------------

function Assert-DotnetInstalled {
    if (Get-Command -Name "dotnet" -ErrorAction SilentlyContinue) {
        return
    }

    Write-Failure "dotnet CLI is not installed or not in the PATH."
    exit 1
}

# ------------------------------------------------------------------------------
# Steps
# ------------------------------------------------------------------------------

function Initialize-ScaffoldDatabase {
    $invokeDatabase = Join-Path $PSScriptRoot "Invoke-Database.ps1"

    if ($Force) {
        Write-Step "Resetting database..."
        & $invokeDatabase -Action Reset
    }
    elseif (-not (Test-Path $script:DatabasePath)) {
        Write-Step "Database not found. Initializing..."
        & $invokeDatabase -Action Initialize
    }
}

function Restore-DotnetTools {
    Write-Step "Restoring dotnet tools..."
    Write-Divider

    # Husky.NET runs `dotnet tool restore` only after a successful build, so
    # tools must be restored manually here to break the circular dependency.
    dotnet tool restore

    if ($LASTEXITCODE -ne 0) {
        Write-Failure "dotnet tool restore failed."
        exit 1
    }
}

function Move-DatabaseToDataProject {
    $destinationDirectory = Split-Path -Path $script:NewDatabasePath -Parent
    if (-not (Test-Path $destinationDirectory)) {
        [void](New-Item -ItemType Directory -Path $destinationDirectory -Force)
    }

    if (-not (Test-Path $script:DatabasePath)) {
        Write-Caution "Source database not found at $script:DatabasePath"
        return
    }

    try {
        Move-Item `
            -Path $script:DatabasePath `
            -Destination $script:NewDatabasePath `
            -Force `
            -ErrorAction Stop
    }
    catch {
        Write-Failure "Failed to move database: $($_.Exception.Message)"
        exit 1
    }
}

function Invoke-EfScaffold {
    $scaffoldArgs = @(
        "ef", "dbcontext", "scaffold",
        "Data Source=$script:NewDatabasePath",
        "Microsoft.EntityFrameworkCore.Sqlite",
        "--startup-project", "Quaply.Ui",
        "--project", $script:DataProjectName,
        "--namespace", "$script:DataProjectName.Models",
        "--context-namespace", "$script:DataProjectName.Contexts",
        "--context", "QuaplyDbContext",
        "--context-dir", (Join-Path $script:GeneratedDirectory "Contexts"),
        "--output-dir", (Join-Path $script:GeneratedDirectory "Models"),
        "--no-onconfiguring",
        "--no-build" # skip build; models may not exist yet
    )

    if ($Force) {
        $scaffoldArgs += "--force"
    }

    Write-Step "Scaffolding EF Core models..."
    dotnet @scaffoldArgs
}

# ------------------------------------------------------------------------------
# Entry point
# ------------------------------------------------------------------------------

Assert-DotnetInstalled
Initialize-ScaffoldDatabase
Restore-DotnetTools
Move-DatabaseToDataProject
Invoke-EfScaffold

Write-Success "Scaffold complete."
