#Requires -Version 7.0
<#
.SYNOPSIS
Scaffolds EF Core models from the Quaply SQLite database.

.DESCRIPTION
Ensures the database exists, restores dotnet tools, scaffolds EF Core models
into a throwaway EfScaffoldSandbox project (to avoid the circular dependency
with Quaply.Data), then copies the generated files into Quaply.Data.

.EXAMPLE
.\Invoke-Scaffold.ps1
#>
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "Common" "ConsoleOutput.ps1")

$script:DatabaseDirectory = Join-Path $PSScriptRoot ".." "Database"
$script:DatabasePath = Join-Path $script:DatabaseDirectory "Quaply.db"

$script:DataProjectName = "Quaply.Data"
$script:GeneratedDirectory = "Generated"
$script:NewDatabasePath = `
    Join-Path $PSScriptRoot ".." $script:DataProjectName "Database" "Quaply.db"

$script:SandboxProjectName = "EfScaffold"
$script:SandboxDirectory = `
    Join-Path $PSScriptRoot ".." "Sandbox" $script:SandboxProjectName
$script:SandboxProjectPath = `
    Join-Path $script:SandboxDirectory "$script:SandboxProjectName.csproj"
$script:SandboxGeneratedDirectory = `
    Join-Path $script:SandboxDirectory $script:GeneratedDirectory

$script:DataProjectDirectory = `
    Join-Path $PSScriptRoot ".." $script:DataProjectName
$script:DataGeneratedDirectory = `
    Join-Path $script:DataProjectDirectory $script:GeneratedDirectory

# Prerequisites ----------------------------------------------------------------

function Assert-DotnetInstalled {
    if (Get-Command -Name "dotnet" -ErrorAction SilentlyContinue) {
        return
    }

    Write-Failure "dotnet CLI is not installed or not in the PATH."
    exit 1
}

# Steps ------------------------------------------------------------------------

function Initialize-ScaffoldDatabase {
    $invokeDatabase = Join-Path $PSScriptRoot "Invoke-Database.ps1"

    if (-not (Test-Path $script:DatabasePath)) {
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

    if (Test-Path $script:NewDatabasePath) {
        Remove-Item -Path $script:NewDatabasePath -Recurse -Force
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

function Initialize-SandboxProject {
    if (Test-Path $script:SandboxProjectPath) {
        return
    }

    Write-Step "Creating $script:SandboxProjectName sandbox project..."

    [void](New-Item -ItemType Directory -Path $script:SandboxDirectory -Force)

    dotnet new classlib `
        --name $script:SandboxProjectName `
        --output $script:SandboxDirectory `
        --force

    if ($LASTEXITCODE -ne 0) {
        Write-Failure "Failed to create $script:SandboxProjectName project."
        exit 1
    }

    Push-Location $script:SandboxDirectory
    try {
        dotnet add package Microsoft.EntityFrameworkCore.Sqlite
        if ($LASTEXITCODE -ne 0) {
            Write-Failure "Failed to add Microsoft.EntityFrameworkCore.Sqlite."
            exit 1
        }

        dotnet add package Microsoft.EntityFrameworkCore.Design
        if ($LASTEXITCODE -ne 0) {
            Write-Failure "Failed to add Microsoft.EntityFrameworkCore.Design."
            exit 1
        }
    }
    finally {
        Pop-Location
    }
}

function Invoke-EfScaffold {
    $scaffoldArgs = @(
        "ef", "dbcontext", "scaffold",
        "Data Source=$script:NewDatabasePath",
        "Microsoft.EntityFrameworkCore.Sqlite",
        "--project", $script:SandboxProjectPath,
        "--startup-project", $script:SandboxProjectPath,
        "--namespace", "$script:DataProjectName.Models",
        "--output-dir", (Join-Path $script:SandboxGeneratedDirectory "Models"),
        "--context-namespace", "$script:DataProjectName.Contexts",
        "--context-dir", (
            Join-Path $script:SandboxGeneratedDirectory "Contexts"
        ),
        "--context", "QuaplyDbContext",
        "--no-onconfiguring"
    )

    Write-Step "Scaffolding EF Core models into sandbox project..."
    dotnet @scaffoldArgs

    if ($LASTEXITCODE -ne 0) {
        Write-Failure "EF scaffold failed."
        exit 1
    }
}

function Move-GeneratedFilesToDataProject {
    Write-Step "Moving scaffolded files into $script:DataProjectName..."

    if (-not (Test-Path $script:SandboxGeneratedDirectory)) {
        Write-Caution `
            "No generated files found at $script:SandboxGeneratedDirectory"
        return
    }

    # Move-Item cannot overwrite an existing directory, so the destination
    # must be cleared first.
    if (Test-Path $script:DataGeneratedDirectory) {
        Remove-Item -Path $script:DataGeneratedDirectory -Recurse -Force
    }

    $destinationParent = Split-Path -Path $script:DataGeneratedDirectory -Parent
    if (-not (Test-Path $destinationParent)) {
        [void](New-Item -ItemType Directory -Path $destinationParent -Force)
    }

    Move-Item `
        -Path $script:SandboxGeneratedDirectory `
        -Destination $script:DataGeneratedDirectory `
        -Force
}

# Entry point ------------------------------------------------------------------

Assert-DotnetInstalled
Initialize-ScaffoldDatabase
Restore-DotnetTools
Move-DatabaseToDataProject
Initialize-SandboxProject
Invoke-EfScaffold
Move-GeneratedFilesToDataProject

Write-Success "Scaffold complete."
