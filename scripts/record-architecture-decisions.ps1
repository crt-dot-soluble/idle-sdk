param(
    [Parameter(Mandatory=$true)][string]$Renderer,
    [Parameter(Mandatory=$true)][string]$Persistence,
    [Parameter(Mandatory=$true)][string]$PluginPackaging
)

$timestamp = Get-Date -Format "yyyy-MM-dd"
$path = Join-Path $PSScriptRoot "..\docs\ARCHITECTURE-DECISIONS.md"

$content = @"
# Architecture Decisions (Phase 2 Prerequisites)

- Date: $timestamp
- Renderer abstraction: $Renderer
- Persistence backend default: $Persistence
- Plugin packaging format: $PluginPackaging

"@

$content | Set-Content -Path $path -Encoding UTF8
Write-Host "Recorded architecture decisions to $path"
