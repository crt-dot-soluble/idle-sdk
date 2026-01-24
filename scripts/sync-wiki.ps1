# Sync local docs/wiki to GitHub wiki repo for ai-governance
param(
  [string]$WikiRemote = "https://github.com/crt-dot-soluble/ai-governance.wiki.git",
  [string]$WikiDir = ".wiki-sync-tmp",
  [string]$LocalWiki = "docs/wiki"
)

function Write-JsonLog([string]$Level, [string]$Message, [hashtable]$Data = @{}) {
  $payload = [ordered]@{ timestamp = (Get-Date -Format "o"); level = $Level; message = $Message; data = $Data }
  $payload | ConvertTo-Json -Compress | Write-Output
}

if (Test-Path $WikiDir) { Remove-Item -Recurse -Force $WikiDir }

Write-JsonLog "info" "wiki.sync.start" @{ remote = $WikiRemote }
Write-Host "Cloning wiki repo..."
git clone $WikiRemote $WikiDir

Get-ChildItem -Path $LocalWiki -Filter *.md | ForEach-Object {
  Copy-Item $_.FullName -Destination $WikiDir -Force
}

Push-Location $WikiDir

git add .
$changes = git diff --cached --quiet; $?
if ($LASTEXITCODE -eq 0) {
  Write-Host "No changes to wiki."
  Write-JsonLog "info" "wiki.sync.no_changes" @{}
} else {
  git commit -m "docs: sync local wiki fallback to GitHub wiki"
  git push
  Write-Host "Wiki updated and pushed."
  Write-JsonLog "info" "wiki.sync.pushed" @{}
}
Pop-Location
Remove-Item -Recurse -Force $WikiDir
