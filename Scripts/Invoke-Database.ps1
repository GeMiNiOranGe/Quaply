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
    [string]$Action
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ------------------------------------------------------------------------------
# Constants
# ------------------------------------------------------------------------------

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

# ------------------------------------------------------------------------------
# Prerequisites
# ------------------------------------------------------------------------------

function Assert-SqliteInstalled {
    if (-not (Get-Command -Name "sqlite3" -ErrorAction SilentlyContinue)) {
        Write-Host ""
        Write-Host "  [ERROR] sqlite3 is not installed or not on PATH." `
            -ForegroundColor Red
        Write-Host "  Please install SQLite3 and add it to your system PATH." `
            -ForegroundColor Red
        Write-Host ""
        exit 1
    }
}

# ------------------------------------------------------------------------------
# Low-level helpers
# ------------------------------------------------------------------------------

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
            "sqlite3 returned exit code $LASTEXITCODE while executing "
            "'$SqlFile'.`n$output"
        ) -join ""
    }
}

# ------------------------------------------------------------------------------
# Database operations
# ------------------------------------------------------------------------------

function Invoke-SchemaInit {
    Invoke-SqlFile -SqlFile $script:SqlFiles.InitSchema `
        -DatabasePath $script:DatabasePath
}

function Invoke-DataPopulate {
    Invoke-SqlFile -SqlFile $script:SqlFiles.PopulateMasterData `
        -DatabasePath $script:DatabasePath
    Invoke-SqlFile -SqlFile $script:SqlFiles.PopulateSeedData `
        -DatabasePath $script:DatabasePath
}

function Invoke-DataClear {
    Invoke-SqlFile -SqlFile $script:SqlFiles.ClearData `
        -DatabasePath $script:DatabasePath
}

# ------------------------------------------------------------------------------
# Menu actions
# ------------------------------------------------------------------------------

function Initialize-Database {
    if (Test-Path $script:DatabasePath) {
        Write-Host "  Database already exists. Choose [3] Reset to overwrite." `
            -ForegroundColor Yellow
        return
    }

    Write-Host "  Initializing database..." -ForegroundColor DarkGray

    Invoke-SchemaInit
    Invoke-DataPopulate

    Write-Host "  Database initialized: $script:DatabasePath" `
        -ForegroundColor Green
}

function Remove-Database {
    param([switch]$Force)

    if (-not (Test-Path $script:DatabasePath)) {
        Write-Host "  No database file found." -ForegroundColor Yellow
        return
    }

    if (-not $Force) {
        Write-Host "  WARNING: This will permanently delete the database." `
            -ForegroundColor Red
        $confirm = Read-Host "  Type '[y]es' to confirm"

        if ($confirm -notin "yes", "y") {
            Write-Host "  Cancelled." -ForegroundColor DarkGray
            return
        }
    }

    # Remove only the exact database file (no wildcard)
    Remove-Item $script:DatabasePath -Force

    Write-Host "  Database removed." -ForegroundColor Green
}

function Reset-Database {
    Write-Host "  Removing old database..." -ForegroundColor DarkGray
    Remove-Database -Force

    Write-Host "  Re-initializing..." -ForegroundColor DarkGray
    Initialize-Database
}

function Restore-Data {
    if (-not (Test-Path $script:DatabasePath)) {
        Write-Host "  No database found. Run Initialize first." `
            -ForegroundColor Yellow
        return
    }

    Write-Host "  Clearing existing data..." -ForegroundColor DarkGray
    Invoke-DataClear

    Write-Host "  Repopulating from seed files..." -ForegroundColor DarkGray
    Invoke-DataPopulate

    Write-Host "  Data restored to defaults." -ForegroundColor Green
}

function Export-InsertTemplates {
    if (-not (Test-Path $script:DatabasePath)) {
        Write-Host "  No database found. Run Initialize first." `
            -ForegroundColor Yellow
        return
    }

    $outputPath = Join-Path $script:GeneratedDirectory "InsertTemplates.sql"

    [void](New-Item -Path $outputPath -ItemType File -Force)
    Get-Content $script:SqlFiles.GenerateInsertTemplates | `
        sqlite3 $script:DatabasePath | `
        Set-Content $outputPath

    Write-Host "  Templates written to: $outputPath" -ForegroundColor Green
}

# ------------------------------------------------------------------------------
# UI helpers
# ------------------------------------------------------------------------------

function Write-MenuHeader {
    $DatabaseExists = Test-Path $script:DatabasePath
    $dbStatus = $DatabaseExists ? "EXISTS  $script:DatabasePath" : "NOT FOUND"
    $statusColor = $DatabaseExists ? "Green" : "DarkGray"

    Write-Host ""
    Write-Host "  ╔══════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "  ║   Quaply  Database  Management   ║" -ForegroundColor Cyan
    Write-Host "  ╚══════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  DB: " -NoNewline -ForegroundColor DarkGray
    Write-Host $dbStatus -ForegroundColor $statusColor
    Write-Host ""
    Write-Host "  [1]  Initialize database" -ForegroundColor White
    Write-Host "  [2]  Remove database" -ForegroundColor White
    Write-Host "  [3]  Reset database" -ForegroundColor White
    Write-Host "  [4]  Restore default data" -ForegroundColor White
    Write-Host "  [5]  Export INSERT query templates" -ForegroundColor White
    Write-Host "  [Q]  Quit" -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "  Select: " -NoNewline -ForegroundColor Cyan
}

function Invoke-MenuAction {
    param([string]$Option)

    Write-Host ""
    Write-Host "  ─────────────────────────────────────" `
        -ForegroundColor DarkGray

    try {
        switch ($Option) {
            "1" { 
                Initialize-Database
            }
            "2" { 
                Remove-Database
            }
            "3" { 
                Reset-Database
            }
            "4" { 
                Restore-Data    
            }
            "5" {
                Export-InsertTemplates
            }
            "q" {
                Write-Host "  Goodbye." -ForegroundColor DarkGray
                Write-Host ""
                exit 0
            }
            default {
                Write-Host "  '$Option' is not a valid option." `
                    -ForegroundColor Red
                # no pause needed
                return $false
            }
        }
    }
    catch {
        Write-Host ""
        Write-Host "  [ERROR] $_" -ForegroundColor Red
    }

    return $true
}

# ------------------------------------------------------------------------------
# Entry point
# ------------------------------------------------------------------------------

Assert-SqliteInstalled

if ($Action) {
    switch ($Action) {
        "Initialize" { 
            Initialize-Database
        }
        "Remove" { 
            Remove-Database
        }
        "Reset" { 
            Reset-Database
        }
        "RestoreData" { 
            Restore-Data 
        }
        "ExportInsertTemplates" {
            Export-InsertTemplates
        }
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
        Write-Host "  Press any key to return to menu..." `
            -ForegroundColor DarkGray
        $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown") | Out-Null
    }
    else {
        Start-Sleep -Milliseconds 600
    }
}
