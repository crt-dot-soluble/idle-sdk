param(
  [Parameter(Mandatory = $false)][string]$OutputPath = ""
)

function Write-JsonLog([string]$Level, [string]$Message, [hashtable]$Data = @{}) {
  $payload = [ordered]@{ timestamp = (Get-Date -Format "o"); level = $Level; message = $Message; data = $Data }
  $payload | ConvertTo-Json -Compress | Write-Output
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
  $OutputPath = Join-Path $repoRoot ".vscode\tooling.json"
}

$targetDir = Split-Path -Parent $OutputPath
if (-not (Test-Path $targetDir)) {
  New-Item -ItemType Directory -Force -Path $targetDir | Out-Null
}

function Test-Tool([string]$Name) {
  $cmd = Get-Command $Name -ErrorAction SilentlyContinue
  return $null -ne $cmd
}

$tools = [ordered]@{
  git = (Test-Tool "git")
  python = (Test-Tool "python")
  python3 = (Test-Tool "python3")
  pwsh = (Test-Tool "pwsh")
  powershell = (Test-Tool "powershell")
  bash = (Test-Tool "bash")
  node = (Test-Tool "node")
  npm = (Test-Tool "npm")
}

$tools["pythonAny"] = $tools.python -or $tools.python3

$report = [ordered]@{
  timestamp = (Get-Date -Format "o")
  tools = $tools
}

$report | ConvertTo-Json -Depth 4 | Out-File -FilePath $OutputPath -Encoding UTF8
Write-Output "Tooling report written to $OutputPath"
Write-JsonLog "info" "tooling.detect.done" @{ path = $OutputPath }
