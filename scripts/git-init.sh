#!/usr/bin/env bash
set -euo pipefail

log_json() {
  local level="$1"; local message="$2"; local data="$3"
  printf '{"timestamp":"%s","level":"%s","message":"%s","data":%s}\n' "$(date -u +%Y-%m-%dT%H:%M:%SZ)" "$level" "$message" "$data"
}

if [ -d ".git" ]; then
  log_json "info" "git.init.skip" "{}"
  echo "Git already initialized."
  exit 0
fi

git init

echo "Git initialized. Configure remote as required by policy."
log_json "info" "git.init.done" "{}"
