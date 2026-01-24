$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$policyPath = Join-Path $repoRoot "governance.config.json"
$specPath = Join-Path $repoRoot "spec\SPECIFICATION.md"

function Write-JsonLog([string]$Level, [string]$Message, [hashtable]$Data = @{}) {
  $payload = [ordered]@{ timestamp = (Get-Date -Format "o"); level = $Level; message = $Message; data = $Data }
  $payload | ConvertTo-Json -Compress | Write-Output
}

if (-not (Test-Path $policyPath)) {
  Write-JsonLog "error" "specstart.missing_policy" @{ path = $policyPath }
  throw "Missing governance.config.json. Run the Governance Bootstrap task first."
}

if (-not (Test-Path $specPath)) {
  Write-JsonLog "error" "specstart.missing_spec" @{ path = $specPath }
  throw "Missing spec/SPECIFICATION.md. Create it before starting from spec."
}

Write-Output "Spec execution can begin. Follow spec/SPECIFICATION.md using the active governance policy."
Write-JsonLog "info" "specstart.ready" @{ policy = $policyPath; spec = $specPath }
