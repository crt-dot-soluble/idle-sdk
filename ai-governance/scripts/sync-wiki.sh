#!/usr/bin/env bash
# Sync local docs/wiki to GitHub wiki repo for ai-governance
set -euo pipefail

WIKI_REMOTE="https://github.com/crt-dot-soluble/ai-governance.wiki.git"
WIKI_DIR=".wiki-sync-tmp"
LOCAL_WIKI="docs/wiki"

log_json() {
  local level="$1"; local message="$2"; local data="$3"
  printf '{"timestamp":"%s","level":"%s","message":"%s","data":%s}\n' "$(date -u +%Y-%m-%dT%H:%M:%SZ)" "$level" "$message" "$data"
}

# Clean up any previous temp
rm -rf "$WIKI_DIR"

echo "Cloning wiki repo..."
log_json "info" "wiki.sync.start" "{\"remote\":\"$WIKI_REMOTE\"}"
git clone "$WIKI_REMOTE" "$WIKI_DIR"

# Copy all local wiki files to the wiki repo
cp -v "$LOCAL_WIKI"/*.md "$WIKI_DIR"/

cd "$WIKI_DIR"
git add .
if git diff --cached --quiet; then
  echo "No changes to wiki."
  log_json "info" "wiki.sync.no_changes" "{}"
else
  git commit -m "docs: sync local wiki fallback to GitHub wiki"
  git push
  echo "Wiki updated and pushed."
  log_json "info" "wiki.sync.pushed" "{}"
fi
cd ..
rm -rf "$WIKI_DIR"
