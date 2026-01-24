#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
OUTPUT_PATH="${1:-$REPO_ROOT/.vscode/report-bundle.zip}"

log_json() {
  local level="$1"; local message="$2"; local data="$3"
  printf '{"timestamp":"%s","level":"%s","message":"%s","data":%s}\n' "$(date -u +%Y-%m-%dT%H:%M:%SZ)" "$level" "$message" "$data"
}

ZIP_BIN=""
if command -v zip >/dev/null 2>&1; then
  ZIP_BIN="zip"
fi

FILES=("$REPO_ROOT/.vscode/audit.json" "$REPO_ROOT/.vscode/tooling.json" "$REPO_ROOT/governance.config.json" "$REPO_ROOT/spec/SPECIFICATION.md")
for f in "${FILES[@]}"; do
  if [ ! -f "$f" ]; then
    log_json "error" "Missing report input" "{\"path\":\"$f\"}"
    echo "Missing report input: $f" >&2
    exit 1
  fi
done

mkdir -p "$(dirname "$OUTPUT_PATH")"
rm -f "$OUTPUT_PATH"

OUTPUT_PATH_ABS="$OUTPUT_PATH"
case "$OUTPUT_PATH" in
  /*) ;;
  *) OUTPUT_PATH_ABS="$REPO_ROOT/$OUTPUT_PATH";;
esac

if [ -n "$ZIP_BIN" ]; then
  log_json "info" "report.bundle.create" "{\"output\":\"$OUTPUT_PATH_ABS\",\"format\":\"zip\"}"
  (cd "$REPO_ROOT" && zip -r "$OUTPUT_PATH_ABS" ".vscode/audit.json" ".vscode/tooling.json" "governance.config.json" "spec/SPECIFICATION.md" >/dev/null)
else
  case "$OUTPUT_PATH_ABS" in
    *.zip) OUTPUT_PATH_ABS="${OUTPUT_PATH_ABS%.zip}.tar.gz" ;;
  esac
  log_json "info" "report.bundle.create" "{\"output\":\"$OUTPUT_PATH_ABS\",\"format\":\"tar.gz\"}"
  (cd "$REPO_ROOT" && tar -czf "$OUTPUT_PATH_ABS" ".vscode/audit.json" ".vscode/tooling.json" "governance.config.json" "spec/SPECIFICATION.md")
fi

log_json "info" "report.bundle.created" "{\"output\":\"$OUTPUT_PATH_ABS\"}"
