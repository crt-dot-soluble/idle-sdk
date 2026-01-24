param(
  [Parameter(Mandatory=$false)][string]$VersionControl = "",
  [Parameter(Mandatory=$false)][string]$Testing = "",
  [Parameter(Mandatory=$false)][string]$Documentation = "",
  [Parameter(Mandatory=$false)][string]$Language = "",
  [Parameter(Mandatory=$false)][string]$Frameworks = "",
  [Parameter(Mandatory=$false)][string]$Autonomy = ""
)

function Write-JsonLog([string]$Level, [string]$Message, [hashtable]$Data = @{}) {
  $payload = [ordered]@{ timestamp = (Get-Date -Format "o"); level = $Level; message = $Message; data = $Data }
  $payload | ConvertTo-Json -Compress | Write-Output
}

Write-JsonLog "info" "policy.update.start" @{}

$policyPath = Join-Path $PSScriptRoot "..\governance.config.json"
if (-not (Test-Path $policyPath)) { throw "Missing governance.config.json" }

$allowedVersionControl = @("git-local", "git-remote", "git-remote-ci")
$allowedTesting = @("full", "baseline")
$allowedDocumentation = @("inline", "comments-only", "generate")
$allowedAutonomy = @("feature", "milestone", "fully-autonomous")

$policy = Get-Content $policyPath -Raw | ConvertFrom-Json

if ($VersionControl -ne "") {
  if (-not ($allowedVersionControl -contains $VersionControl)) { throw "Invalid VersionControl" }
  $policy.versionControl = $VersionControl
  $policy.remoteRequired = ($VersionControl -ne "git-local")
}

if ($Testing -ne "") {
  if (-not ($allowedTesting -contains $Testing)) { throw "Invalid Testing" }
  $policy.testing = $Testing
}

if ($Documentation -ne "") {
  if (-not ($allowedDocumentation -contains $Documentation)) { throw "Invalid Documentation" }
  $policy.documentation = $Documentation
}

if ($Language -ne "") {
  $policy.language.primary = $Language
}

if ($Frameworks -ne "") {
  if ($Frameworks -eq "None") {
    $policy.language.frameworks = @()
  } else {
    $frameworkList = $Frameworks.Split(",") | ForEach-Object { $_.Trim() } | Where-Object { $_ -ne "" }
    $policy.language.frameworks = $frameworkList
  }
}

if ($Autonomy -ne "") {
  if (-not ($allowedAutonomy -contains $Autonomy)) { throw "Invalid Autonomy" }
  $policy.autonomy = $Autonomy
}

$policy | ConvertTo-Json -Depth 6 | Out-File -FilePath $policyPath -Encoding UTF8
Write-Output "Updated governance policy at $policyPath"
Write-JsonLog "info" "policy.update.done" @{ path = $policyPath }
