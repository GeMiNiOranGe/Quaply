# Scripts\Common\ConsoleOutput.ps1
<#
.SYNOPSIS
Shared console output helpers for Quaply automation scripts.

.DESCRIPTION
Centralizes coloring/formatting so scripts emit consistent status
messages. Indentation is configurable per-script via Set-OutputIndent,
so a script can align flush-left or with a fixed left margin without
duplicating Write-Host calls everywhere.
#>

Set-StrictMode -Version Latest

$script:Indent = ""

function Set-OutputIndent {
    param([int]$Spaces = 0)
    $script:Indent = " " * $Spaces
}

function Write-Header {
    param([Parameter(Mandatory)][string]$Title)

    $width = $Title.Length + 4
    $top = "╔" + ("═" * $width) + "╗"
    $bottom = "╚" + ("═" * $width) + "╝"
    $middle = "║  $Title  ║"

    Write-Host ""
    Write-Host "$script:Indent$top" -ForegroundColor Cyan
    Write-Host "$script:Indent$middle" -ForegroundColor Cyan
    Write-Host "$script:Indent$bottom" -ForegroundColor Cyan
    Write-Host ""
}

function Write-Divider {
    Write-Host "$script:Indent$('─' * 39)" -ForegroundColor DarkGray
}

function Write-Step {
    param([Parameter(Mandatory)][string]$Message)
    Write-Host "$script:Indent$Message" -ForegroundColor DarkGray
}

function Write-Success {
    param([Parameter(Mandatory)][string]$Message)
    Write-Host "$script:Indent$Message" -ForegroundColor Green
}

function Write-Caution {
    param([Parameter(Mandatory)][string]$Message)
    Write-Host "$script:Indent$Message" -ForegroundColor Yellow
}

function Write-Failure {
    param([Parameter(Mandatory)][string]$Message)
    Write-Host "$script:Indent[ERROR] $Message" -ForegroundColor Red
}

function Write-Prompt {
    param([Parameter(Mandatory)][string]$Message)
    Write-Host "$script:Indent$Message" -NoNewline -ForegroundColor Cyan
}

function Write-Label {
    param(
        [Parameter(Mandatory)][string]$Label,
        [Parameter(Mandatory)][string]$Value,
        [string]$ValueColor = "White"
    )
    Write-Host "$script:Indent$Label" -NoNewline -ForegroundColor DarkGray
    Write-Host $Value -ForegroundColor $ValueColor
}
