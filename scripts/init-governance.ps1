
# Support both param and $args for one-liner execution
param(
  [Parameter(Mandatory = $false)][string]$SpecPath = ""
)

function Write-JsonLog([string]$Level, [string]$Message, [hashtable]$Data = @{}) {
  $payload = [ordered]@{ timestamp = (Get-Date -Format "o"); level = $Level; message = $Message; data = $Data }
  $payload | ConvertTo-Json -Compress | Write-Output
}

if ($args.Count -gt 0) {
  Write-JsonLog "error" "initgovernance.invalid_args" @{}
  throw "Exactly one argument is required: path to SPECIFICATION.md"
}

if ([string]::IsNullOrWhiteSpace($SpecPath)) {
  Write-JsonLog "error" "initgovernance.missing_spec" @{}
  throw "Missing SPECIFICATION.md path. Usage: init-governance.ps1 <path-to-SPECIFICATION.md>"
}

$resolvedSpecPath = Resolve-Path $SpecPath -ErrorAction Stop
$specLeaf = Split-Path $resolvedSpecPath -Leaf
if ($specLeaf -ne "SPECIFICATION.md") {
  Write-JsonLog "error" "initgovernance.invalid_spec_name" @{ name = $specLeaf }
  throw "Invalid spec filename. Expected SPECIFICATION.md"
}

$specDir = Split-Path $resolvedSpecPath -Parent
$specDirLeaf = Split-Path $specDir -Leaf
if ($specDirLeaf -ieq "spec") {
  $targetRoot = Split-Path $specDir -Parent
} else {
  $targetRoot = $specDir
}


$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")


$requiredDirs = @(".ai", ".vscode", "scripts", "templates", "docs", "plans", "src", "spec", ".github")
foreach ($dir in $requiredDirs) {
  $targetPath = Join-Path $targetRoot $dir
  if (Test-Path $targetPath) {
    Write-JsonLog "error" "initgovernance.target_conflict" @{ path = $targetPath }
    throw "Target already contains $dir at $targetPath. Aborting to avoid overwrite."
  }
}



$requiredFiles = @(
  "CONSTITUTION.md",
  "CHANGELOG.md",
  "MEMORY-LEDGER.md",
  "TODO-LEDGER.md",
  "README.md",
  "governance.config.json",
  "ai-governance.code-workspace"
)
foreach ($file in $requiredFiles) {
  $targetPath = Join-Path $targetRoot $file
  if (Test-Path $targetPath) {
    Write-JsonLog "error" "initgovernance.target_conflict" @{ path = $targetPath }
    throw "Target already contains $file at $targetPath. Aborting to avoid overwrite."
  }
}


Copy-Item -Path (Join-Path $repoRoot ".ai") -Destination (Join-Path $targetRoot ".ai") -Recurse
Copy-Item -Path (Join-Path $repoRoot ".vscode") -Destination (Join-Path $targetRoot ".vscode") -Recurse
Copy-Item -Path (Join-Path $repoRoot "scripts") -Destination (Join-Path $targetRoot "scripts") -Recurse
Copy-Item -Path (Join-Path $repoRoot "templates") -Destination (Join-Path $targetRoot "templates") -Recurse
Copy-Item -Path (Join-Path $repoRoot "docs") -Destination (Join-Path $targetRoot "docs") -Recurse
Copy-Item -Path (Join-Path $repoRoot "plans") -Destination (Join-Path $targetRoot "plans") -Recurse
Copy-Item -Path (Join-Path $repoRoot "src") -Destination (Join-Path $targetRoot "src") -Recurse
Copy-Item -Path (Join-Path $repoRoot ".github") -Destination (Join-Path $targetRoot ".github") -Recurse


Copy-Item -Path (Join-Path $repoRoot "templates\CONSTITUTION.md") -Destination (Join-Path $targetRoot "CONSTITUTION.md")
Copy-Item -Path (Join-Path $repoRoot "templates\CHANGELOG.md") -Destination (Join-Path $targetRoot "CHANGELOG.md")
Copy-Item -Path (Join-Path $repoRoot "templates\MEMORY-LEDGER.md") -Destination (Join-Path $targetRoot "MEMORY-LEDGER.md")
Copy-Item -Path (Join-Path $repoRoot "templates\TODO-LEDGER.md") -Destination (Join-Path $targetRoot "TODO-LEDGER.md")
Copy-Item -Path (Join-Path $repoRoot "templates\README.md") -Destination (Join-Path $targetRoot "README.md")
Copy-Item -Path (Join-Path $repoRoot "governance.config.json") -Destination (Join-Path $targetRoot "governance.config.json")
Copy-Item -Path (Join-Path $repoRoot "templates\ai-governance.code-workspace") -Destination (Join-Path $targetRoot "ai-governance.code-workspace")

if (-not (Test-Path (Join-Path $targetRoot "spec"))) {
  New-Item -ItemType Directory -Force -Path (Join-Path $targetRoot "spec") | Out-Null
}

Copy-Item -Path $resolvedSpecPath -Destination (Join-Path $targetRoot "spec\SPECIFICATION.md") -Force

Write-Output "Initialized governance repository in $targetRoot.\n\nPlaced SPECIFICATION.md in the 'spec' folder."
Write-JsonLog "info" "initgovernance.done" @{ target = $targetRoot }
