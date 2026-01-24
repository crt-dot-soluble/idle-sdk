param(
  [Parameter(Mandatory = $false)][string]$OutputPath = ""
)
$ErrorActionPreference = "Stop"

function Write-JsonLog([string]$Level, [string]$Message, [hashtable]$Data = @{}) {
  $payload = [ordered]@{ timestamp = (Get-Date -Format "o"); level = $Level; message = $Message; data = $Data }
  $payload | ConvertTo-Json -Compress | Write-Output
}

$null = Write-JsonLog "info" "selfaudit.start" @{}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
  $OutputPath = Join-Path $repoRoot ".vscode\audit.json"
}

$targetDir = Split-Path -Parent $OutputPath
if (-not (Test-Path $targetDir)) {
  New-Item -ItemType Directory -Force -Path $targetDir | Out-Null
}

$errors = @()
$warnings = @()

$requiredDirs = @(".ai", ".vscode", "scripts", "templates", "docs", "plans", "src", "spec", ".github", "schemas")
foreach ($dir in $requiredDirs) {
  if (-not (Test-Path (Join-Path $repoRoot $dir))) {
    $errors += "Missing required directory: $dir"
  }
}

$requiredFiles = @("CONSTITUTION.md", "MEMORY-LEDGER.md", "TODO-LEDGER.md", "CHANGELOG.md")
foreach ($file in $requiredFiles) {
  if (-not (Test-Path (Join-Path $repoRoot $file))) {
    $errors += "Missing required file: $file"
  }
}

$policyConfig = Join-Path $repoRoot "governance.config.json"
$policyAlt = Join-Path $repoRoot "POLICY.md"
if (-not (Test-Path $policyConfig) -and -not (Test-Path $policyAlt)) {
  $errors += "Missing policy file: governance.config.json or POLICY.md"
}

$specPath = Join-Path $repoRoot "spec\SPECIFICATION.md"
$schemaPath = Join-Path $repoRoot "schemas\governance.schema.json"
if (-not (Test-Path $schemaPath)) {
  $errors += "Missing schema: schemas/governance.schema.json"
}

$manifestPath = Join-Path $repoRoot "manifest.json"
if (-not (Test-Path $manifestPath)) {
  $errors += "Missing manifest.json"
}
if (-not (Test-Path $specPath)) {
  $errors += "Missing spec/SPECIFICATION.md"
}

$tasksPath = Join-Path $repoRoot ".vscode\tasks.json"
if (-not (Test-Path $tasksPath)) {
  $errors += "Missing .vscode/tasks.json"
} else {
  try {
    $tasksJson = Get-Content $tasksPath -Raw | ConvertFrom-Json
    $labels = @()
    foreach ($task in $tasksJson.tasks) { $labels += $task.label }
    $requiredTasks = @(
      "Governance Preflight",
      "Governance Init Repository",
      "Governance Policy Validate",
      "Governance Git Init",
      "Governance Wiki Sync",
      "Governance Self-Audit",
      "Governance Bootstrap",
      "Governance Bootstrap (Defaults)",
      "Governance Policy Revision",
      "Set Autonomy Policy",
      "Set Workflow Mode",
      "Start Spec Implementation",
      "Governance Report Bundle"
    )
    foreach ($label in $requiredTasks) {
      if (-not ($labels -contains $label)) {
        $errors += "Missing VS Code task: $label"
      }
    }
  } catch {
    $errors += "Failed to parse .vscode/tasks.json"
  }
}

if (Test-Path $policyConfig) {
  try {
    $policy = Get-Content $policyConfig -Raw | ConvertFrom-Json
    $requiredKeys = @(
      "version", "policyGeneratedBy", "bootstrap", "versionControl", "testing",
      "documentation", "language", "autonomy", "phases", "ciCdEnforced", "remoteRequired"
    )
    foreach ($key in $requiredKeys) {
      if (-not ($policy.PSObject.Properties.Name -contains $key)) {
        $errors += "Missing policy key: $key"
      }
    }
  } catch {
    $errors += "Failed to parse governance.config.json"
  }
}

$scriptsPath = Join-Path $repoRoot "scripts"
if (Test-Path $scriptsPath) {
  $ps1 = Get-ChildItem -Path $scriptsPath -Filter *.ps1 | ForEach-Object { $_.BaseName }
  $sh = Get-ChildItem -Path $scriptsPath -Filter *.sh | ForEach-Object { $_.BaseName }
  foreach ($name in $ps1) {
    if (-not ($sh -contains $name)) {
      $errors += "Missing .sh counterpart for scripts/$name.ps1"
    }
  }
  foreach ($name in $sh) {
    if (-not ($ps1 -contains $name)) {
      $errors += "Missing .ps1 counterpart for scripts/$name.sh"
    }
  }
}

$report = [ordered]@{
  timestamp = (Get-Date -Format "o")
  ok = ($errors.Count -eq 0)
  errors = $errors
  warnings = $warnings
}

$report | ConvertTo-Json -Depth 6 | Out-File -FilePath $OutputPath -Encoding UTF8

if ($errors.Count -gt 0) {
  Write-JsonLog "error" "selfaudit.failed" @{ output = $OutputPath; errors = $errors }
  Write-Error "Self-audit failed. See $OutputPath"
  exit 1
}

Write-Output "Self-audit passed. Report written to $OutputPath"
Write-JsonLog "info" "selfaudit.passed" @{ output = $OutputPath }
