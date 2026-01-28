param(
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$csproj = Join-Path $root "src\idle-sdk.core\IdleSdk.Core.csproj"
$xmlPath = Join-Path $root "src\idle-sdk.core\bin\$Configuration\net10.0\IdleSdk.Core.xml"
$docPath = Join-Path $root "docs\GENERATED-API.md"

& dotnet build $csproj -c $Configuration | Out-Null

if (-not (Test-Path $xmlPath)) {
    throw "XML documentation not found at $xmlPath"
}

[xml]$xml = Get-Content $xmlPath
$members = $xml.doc.members.member | Where-Object { $_.name -like "T:IdleSdk.Core.Assets*" }

$lines = @()
$lines += "# Generated API Documentation"
$lines += ""
$lines += "Generated from inline XML documentation for IdleSdk.Core.Assets."
$lines += ""

foreach ($member in $members) {
    $name = $member.name -replace '^T:', ''
    $summary = ($member.summary | Out-String).Trim()
    if ([string]::IsNullOrWhiteSpace($summary)) { $summary = "(No summary)" }
    $lines += "## $name"
    $lines += ""
    $lines += $summary
    $lines += ""
}

$lines | Set-Content -Path $docPath -Encoding UTF8
Write-Output "Generated $docPath"
