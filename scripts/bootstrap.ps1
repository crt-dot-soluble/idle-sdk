param(
  [Parameter(Mandatory=$false)][string]$Mode = "",
  [Parameter(Mandatory=$false)][string]$VersionControl = "",
  [Parameter(Mandatory=$false)][string]$Testing = "",
  [Parameter(Mandatory=$false)][string]$Documentation = "",
  [Parameter(Mandatory=$false)][string]$Language = "",
  [Parameter(Mandatory=$false)][string]$Frameworks = "",
  [Parameter(Mandatory=$false)][string]$Autonomy = "",
  [Parameter(Mandatory=$false)][string]$IncludeWorkspaceFile = "Yes (recommended)"
)
$ErrorActionPreference = "Stop"

function Write-JsonLog([string]$Level, [string]$Message, [hashtable]$Data = @{}) {
  $payload = [ordered]@{ timestamp = (Get-Date -Format "o"); level = $Level; message = $Message; data = $Data }
  $payload | ConvertTo-Json -Compress | Write-Output
}

Write-JsonLog "info" "bootstrap.start" @{ mode = $Mode }
$workspaceFilePath = Join-Path $PSScriptRoot "..\ai-governance.code-workspace"
$templateWorkspaceFile = Join-Path $PSScriptRoot "..\templates\ai-governance.code-workspace"

$shouldCopyWorkspace = $IncludeWorkspaceFile -eq "Yes (recommended)"

# Copy workspace file if requested and not already present
if ($shouldCopyWorkspace -and -not (Test-Path $workspaceFilePath)) {
  if (Test-Path $templateWorkspaceFile) {
    Copy-Item $templateWorkspaceFile $workspaceFilePath
    Write-Output "Copied VS Code workspace file to $workspaceFilePath"
  }
}

$allowedMode = @("defaults", "customize")
$allowedVersionControl = @("git-local", "git-remote", "git-remote-ci")
$allowedTesting = @("full", "baseline")
$allowedDocumentation = @("inline", "comments-only", "generate")
$allowedAutonomy = @("feature", "milestone", "fully-autonomous")

if ([string]::IsNullOrWhiteSpace($Mode)) {
  Write-JsonLog "error" "bootstrap.missing_mode" @{}
  throw "Missing bootstrap mode. Re-run the Governance Bootstrap task and select a mode."
}

if (-not ($allowedMode -contains $Mode)) { throw "Invalid Mode" }

if ($Mode -ne "customize") {
  $VersionControl = "git-remote-ci"
  $Testing = "full"
  $Documentation = "inline"
  $Language = "unspecified"
  $Frameworks = ""
  $Autonomy = "feature"
} else {
  if ([string]::IsNullOrWhiteSpace($VersionControl) -or
      [string]::IsNullOrWhiteSpace($Testing) -or
      [string]::IsNullOrWhiteSpace($Documentation) -or
      [string]::IsNullOrWhiteSpace($Language) -or
      [string]::IsNullOrWhiteSpace($Autonomy)) {
    throw "Missing bootstrap inputs. Re-run the Governance Bootstrap task and complete all pickers."
  }
}

if ($Frameworks -eq "None") { $Frameworks = "" }

$readmePath = Join-Path $PSScriptRoot "..\README.md"
if (Test-Path $readmePath) {
  $readmeContent = Get-Content $readmePath -Raw
  if ($readmeContent -match "AI AGENTS MUST IGNORE THIS FILE") {
    Remove-Item $readmePath -Force
    Write-Output "Removed default README.md"
  }
}

$specPath = Join-Path $PSScriptRoot "..\spec\SPECIFICATION.md"
if (-not (Test-Path $specPath)) {
  Write-JsonLog "error" "bootstrap.missing_spec" @{ path = $specPath }
  throw "Missing spec/SPECIFICATION.md. Create it before running bootstrap."
}

if (-not ($allowedVersionControl -contains $VersionControl)) { throw "Invalid VersionControl" }
if (-not ($allowedTesting -contains $Testing)) { throw "Invalid Testing" }
if (-not ($allowedDocumentation -contains $Documentation)) { throw "Invalid Documentation" }
if (-not ($allowedAutonomy -contains $Autonomy)) { throw "Invalid Autonomy" }

$frameworkList = @()
if ($Frameworks -ne "") {
  $frameworkList = $Frameworks.Split(",") | ForEach-Object { $_.Trim() } | Where-Object { $_ -ne "" }
}

$policy = [ordered]@{
  version = "1.0.0"
  policyGeneratedBy = "bootstrap"
  bootstrap = [ordered]@{
    mode = $Mode
    skipped = ($Mode -ne "customize")
    timestamp = (Get-Date -Format "yyyy-MM-dd")
  }
  versionControl = $VersionControl
  testing = $Testing
  documentation = $Documentation
  language = [ordered]@{
    primary = $Language
    frameworks = $frameworkList
  }
  autonomy = $Autonomy
  phases = [ordered]@{
    required = $true
    list = @(0,1,2,3,4,5)
  }
  ciCdEnforced = $true
  remoteRequired = ($VersionControl -ne "git-local")
}

$policyPath = Join-Path $PSScriptRoot "..\governance.config.json"
$policy | ConvertTo-Json -Depth 6 | Out-File -FilePath $policyPath -Encoding UTF8
Write-Output "Wrote governance policy to $policyPath"
Write-JsonLog "info" "bootstrap.policy_written" @{ path = $policyPath }
