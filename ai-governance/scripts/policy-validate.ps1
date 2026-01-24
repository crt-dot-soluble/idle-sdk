$ErrorActionPreference = "Stop"

function Write-JsonLog([string]$Level, [string]$Message, [hashtable]$Data = @{}) {
  $payload = [ordered]@{ timestamp = (Get-Date -Format "o"); level = $Level; message = $Message; data = $Data }
  $payload | ConvertTo-Json -Compress | Write-Output
}

$policyPath = Join-Path $PSScriptRoot "..\governance.config.json"
if (-not (Test-Path $policyPath)) { throw "Missing governance.config.json" }

$schemaPath = Join-Path $PSScriptRoot "..\schemas\governance.schema.json"
if (-not (Test-Path $schemaPath)) { throw "Missing schemas/governance.schema.json" }

$required = @(
  "version", "policyGeneratedBy", "bootstrap", "versionControl", "testing",
  "documentation", "language", "autonomy", "phases", "ciCdEnforced", "remoteRequired"
)

$policy = Get-Content $policyPath -Raw | ConvertFrom-Json
$schema = Get-Content $schemaPath -Raw | ConvertFrom-Json

function Test-JsonSchema($Schema, $Data, $Path = "$") {
  $errs = @()
  if ($Schema.type) {
    switch ($Schema.type) {
      "object" {
        if (-not ($Data -is [psobject])) { $errs += "$Path should be object"; return $errs }
        if ($Schema.required) {
          foreach ($req in $Schema.required) {
            if (-not ($Data.PSObject.Properties.Name -contains $req)) { $errs += "$Path missing required '$req'" }
          }
        }
        if ($Schema.properties) {
          foreach ($prop in $Schema.properties.PSObject.Properties.Name) {
            if ($Data.PSObject.Properties.Name -contains $prop) {
              $errs += Test-JsonSchema $Schema.properties.$prop $Data.$prop "$Path.$prop"
            }
          }
        }
      }
      "array" {
        if (-not ($Data -is [System.Collections.IEnumerable])) { $errs += "$Path should be array"; return $errs }
        if ($Schema.items) {
          $i = 0
          foreach ($item in $Data) {
            $errs += Test-JsonSchema $Schema.items $item "$Path[$i]"
            $i++
          }
        }
      }
      "string" { if (-not ($Data -is [string])) { $errs += "$Path should be string" } }
      "boolean" { if (-not ($Data -is [bool])) { $errs += "$Path should be boolean" } }
      "integer" { if (-not ($Data -is [int])) { $errs += "$Path should be integer" } }
      "number" { if (-not ($Data -is [double] -or $Data -is [int])) { $errs += "$Path should be number" } }
    }
  }
  if ($Schema.enum) {
    if (-not ($Schema.enum -contains $Data)) { $errs += "$Path must be one of $($Schema.enum -join ', ')" }
  }
  return $errs
}
$missing = @()
foreach ($key in $required) {
  if (-not ($policy.PSObject.Properties.Name -contains $key)) {
    $missing += $key
  }
}

$schemaErrors = Test-JsonSchema $schema $policy
if ($missing.Count -gt 0 -or $schemaErrors.Count -gt 0) {
  $all = @()
  if ($missing.Count -gt 0) { $all += ("Missing policy keys: " + ($missing -join ", ")) }
  if ($schemaErrors.Count -gt 0) { $all += $schemaErrors }
  Write-JsonLog "error" "policy.validation_failed" @{ errors = $all }
  throw ($all -join "; ")
}

Write-JsonLog "info" "policy.validation_ok" @{ path = $policyPath; schema = $schemaPath }
Write-Output "Policy validated"
