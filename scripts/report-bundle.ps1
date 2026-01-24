param(
  [Parameter(Mandatory = $false)][string]$OutputPath = ""
)

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
  $OutputPath = Join-Path $repoRoot ".vscode\report-bundle.zip"
}

function Write-JsonLog([string]$Level, [string]$Message, [hashtable]$Data = @{}) {
  $payload = [ordered]@{ timestamp = (Get-Date -Format "o"); level = $Level; message = $Message; data = $Data }
  $payload | ConvertTo-Json -Compress | Write-Output
}

$files = @(
  (Join-Path $repoRoot ".vscode\audit.json"),
  (Join-Path $repoRoot ".vscode\tooling.json"),
  (Join-Path $repoRoot "governance.config.json"),
  (Join-Path $repoRoot "spec\SPECIFICATION.md")
)

foreach ($f in $files) {
  if (-not (Test-Path $f)) {
    Write-JsonLog "error" "Missing report input" @{ path = $f }
    throw "Missing report input: $f"
  }
}

$targetDir = Split-Path -Parent $OutputPath
if (-not (Test-Path $targetDir)) {
  New-Item -ItemType Directory -Force -Path $targetDir | Out-Null
}

if (Test-Path $OutputPath) { Remove-Item $OutputPath -Force }

Write-JsonLog "info" "report.bundle.create" @{ output = $OutputPath }
try {
  Compress-Archive -Path $files -DestinationPath $OutputPath -Force
  Write-JsonLog "info" "report.bundle.created" @{ output = $OutputPath }
} catch {
  Write-JsonLog "error" "report.bundle.failed" @{ output = $OutputPath; error = $_.Exception.Message }
  throw
}
