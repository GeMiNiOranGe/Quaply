#Requires -Version 7.0
<#
.SYNOPSIS
Quaply Database Management CLI

.DESCRIPTION
Interactive menu for managing the Quaply SQLite database:
initialize, remove, reset, or restore seed data.
#>
param(
    [ValidateSet(
        "Initialize",
        "Remove",
        "Reset",
        "RestoreData",
        "ExportInsertTemplates"
    )]
    [string]$Action,

    [ValidateSet(
        "Development",
        "Testing",
        "Staging",
        "Production"
    )]
    [string]$Environment = "Development"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "Common" "ConsoleOutput.ps1")
Set-OutputIndent -Spaces 2

# Constants --------------------------------------------------------------------

$script:DatabaseDirectory = Join-Path $PSScriptRoot ".." "Database"
$script:DatabasePath = Join-Path $script:DatabaseDirectory "Quaply.db"

$script:ToolsDirectory = Join-Path $PSScriptRoot "Tools"
$script:GeneratedDirectory = Join-Path $PSScriptRoot "Generated"

$script:SqlFiles = @{
    InitSchema              = `
        Join-Path $script:DatabaseDirectory "InitSchema.sql"
    PopulateMasterData      = `
        Join-Path $script:DatabaseDirectory "PopulateMasterData.sql"
    PopulateSeedData        = `
        Join-Path $script:DatabaseDirectory "PopulateSeedData.sql"
    ClearData               = `
        Join-Path $script:DatabaseDirectory "ClearData.sql"
    GenerateInsertTemplates = `
        Join-Path $script:ToolsDirectory "GenerateInsertTemplates.sql"
}

# Prerequisites ----------------------------------------------------------------

function Assert-SqliteInstalled {
    if (Get-Command -Name "sqlite3" -ErrorAction SilentlyContinue) {
        return
    }

    Write-Failure "sqlite3 is not installed or not on PATH."
    Write-Failure "Please install SQLite3 and add it to your system PATH."
    Write-Host ""
    exit 1
}

# Low-level helpers ------------------------------------------------------------

<#
.SYNOPSIS
Pipes a .sql file into sqlite3 and surfaces errors.
#>
function Invoke-SqlFile {
    param(
        [Parameter(Mandatory)][string]$SqlFile,
        [Parameter(Mandatory)][string]$DatabasePath
    )

    if (-not (Test-Path $SqlFile)) {
        throw "SQL file not found: $SqlFile"
    }

    $output = Get-Content $SqlFile | sqlite3 $DatabasePath 2>&1

    if ($LASTEXITCODE -ne 0) {
        throw @(
            "sqlite3 exited with code $LASTEXITCODE while executing '$SqlFile'."
            "$output"
        ) -join "`n"
    }
}

# Database operations ----------------------------------------------------------

function Invoke-SchemaInit {
    Invoke-SqlFile -SqlFile $script:SqlFiles.InitSchema `
        -DatabasePath $script:DatabasePath
}

function Invoke-DataPopulate {
    Invoke-SqlFile -SqlFile $script:SqlFiles.PopulateMasterData `
        -DatabasePath $script:DatabasePath
    if ($Environment -ne "Production") {
        Invoke-SqlFile -SqlFile $script:SqlFiles.PopulateSeedData `
            -DatabasePath $script:DatabasePath
    }
}

function Invoke-DataClear {
    Invoke-SqlFile -SqlFile $script:SqlFiles.ClearData `
        -DatabasePath $script:DatabasePath
}

# Menu actions -----------------------------------------------------------------

function Initialize-Database {
    if (Test-Path $script:DatabasePath) {
        Write-Caution "Database already exists. Choose [3] Reset to overwrite."
        return
    }

    Write-Step "Initializing database... (Environment: $Environment)"
    switch ($Environment) {
        "Development" {
            Invoke-SchemaInit
            Invoke-DataPopulate
        }
        "Testing" {
            # No seed data for automated test runs; schema only, if any.
        }
        "Staging" {
            # Intentionally left for future staging-specific setup.
        }
        "Production" {
            Invoke-SchemaInit
            Invoke-DataPopulate
        }
    }

    Write-Success "Database initialized: $script:DatabasePath"
}

function Remove-Database {
    param([switch]$Force)

    if (-not (Test-Path $script:DatabasePath)) {
        Write-Caution "No existing database found. Run Initialize first."
        return
    }

    if (-not $Force) {
        Write-Failure "WARNING: This will permanently delete the database."
        $confirm = Read-Host "  Type '[y]es' to confirm"

        if ($confirm -notin "yes", "y") {
            Write-Step "Cancelled."
            return
        }
    }

    # Remove only the exact database file (no wildcard)
    Remove-Item $script:DatabasePath -Force

    Write-Success "Database removed."
}

function Reset-Database {
    if (-not (Test-Path $script:DatabasePath)) {
        Write-Caution "No existing database found. Run Initialize first."
        return
    }

    Write-Step "Removing old database..."
    Remove-Database -Force

    Write-Step "Re-initializing..."
    Initialize-Database
}

function Restore-Data {
    if (-not (Test-Path $script:DatabasePath)) {
        Write-Caution "No existing database found. Run Initialize first."
        return
    }

    Write-Step "Clearing existing data..."
    Invoke-DataClear

    Write-Step "Repopulating from seed files..."
    Invoke-DataPopulate

    Write-Success "Data restored to defaults."
}

function Export-InsertTemplates {
    if (-not (Test-Path $script:DatabasePath)) {
        Write-Caution "No existing database found. Run Initialize first."
        return
    }

    $outputPath = Join-Path $script:GeneratedDirectory "InsertTemplates.sql"

    [void](New-Item -Path $outputPath -ItemType File -Force)
    Get-Content $script:SqlFiles.GenerateInsertTemplates | `
        sqlite3 $script:DatabasePath | `
        Set-Content $outputPath

    Write-Success "Templates written to: $outputPath"
}

# UI helpers -------------------------------------------------------------------

function Write-MenuHeader {
    Write-Header -Title "Quaply Database Management"

    $databaseExists = Test-Path $script:DatabasePath
    $status = $databaseExists ? "EXISTS  $script:DatabasePath" : "NOT FOUND"
    $statusColor = $databaseExists ? "Green" : "DarkGray"
    Write-Label -Label "DB: " -Value $status -ValueColor $statusColor
    Write-Host ""

    @(
        "[1]  Initialize database"
        "[2]  Remove database"
        "[3]  Reset database"
        "[4]  Restore default data"
        "[5]  Export INSERT query templates"
    ) | ForEach-Object { Write-Host "$script:Indent$_" -ForegroundColor White }

    Write-Host "$script:Indent[Q]  Quit" -ForegroundColor DarkGray
    Write-Host ""
    Write-Prompt "Select: "
}

function Invoke-MenuAction {
    param([string]$Option)

    Write-Host ""
    Write-Divider

    try {
        switch ($Option.ToLowerInvariant()) {
            "1" { Initialize-Database }
            "2" { Remove-Database }
            "3" { Reset-Database }
            "4" { Restore-Data }
            "5" { Export-InsertTemplates }
            "q" {
                Write-Step "Goodbye."
                Write-Host ""
                exit 0
            }
            default {
                Write-Failure "'$Option' is not a valid option."
                return $false
            }
        }
    }
    catch {
        Write-Host ""
        Write-Failure $_
    }

    return $true
}

# Entry point ------------------------------------------------------------------

Assert-SqliteInstalled

if ($Action) {
    switch ($Action) {
        "Initialize" { Initialize-Database }
        "Remove" { Remove-Database }
        "Reset" { Reset-Database }
        "RestoreData" { Restore-Data }
        "ExportInsertTemplates" { Export-InsertTemplates }
    }
    exit 0
}

while ($true) {
    Clear-Host
    Write-MenuHeader

    $key = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    $option = $key.Character.ToString()
    Write-Host $option

    $needsPause = Invoke-MenuAction -Option $option

    if ($needsPause) {
        Write-Host ""
        Write-Step "Press any key to return to menu..."
        $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown") | Out-Null
    }
    else {
        Start-Sleep -Milliseconds 600
    }
}
