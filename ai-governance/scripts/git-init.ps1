function Write-JsonLog([string]$Level, [string]$Message, [hashtable]$Data = @{}) {
  $payload = [ordered]@{ timestamp = (Get-Date -Format "o"); level = $Level; message = $Message; data = $Data }
  $payload | ConvertTo-Json -Compress | Write-Output
}

if (Test-Path ".git") {
  Write-JsonLog "info" "git.init.skip" @{}
  Write-Output "Git already initialized."
  exit 0
}

git init

Write-Output "Git initialized. Configure remote as required by policy."
Write-JsonLog "info" "git.init.done" @{}
