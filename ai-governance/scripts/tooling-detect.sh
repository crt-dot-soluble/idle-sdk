#!/usr/bin/env bash
set -euo pipefail

OUTPUT_PATH="${1:-}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

log_json() {
  local level="$1"; local message="$2"; local data="$3"
  printf '{"timestamp":"%s","level":"%s","message":"%s","data":%s}\n' "$(date -u +%Y-%m-%dT%H:%M:%SZ)" "$level" "$message" "$data"
}

if [ -z "$OUTPUT_PATH" ]; then
  OUTPUT_PATH="$REPO_ROOT/.vscode/tooling.json"
fi

mkdir -p "$(dirname "$OUTPUT_PATH")"

has() {
  command -v "$1" >/dev/null 2>&1
}

GIT=false; if has git; then GIT=true; fi
PYTHON=false; if has python; then PYTHON=true; fi
PYTHON3=false; if has python3; then PYTHON3=true; fi
PWSH=false; if has pwsh; then PWSH=true; fi
POWERSHELL=false; if has powershell; then POWERSHELL=true; fi
BASH=false; if has bash; then BASH=true; fi
NODE=false; if has node; then NODE=true; fi
NPM=false; if has npm; then NPM=true; fi
PYTHON_ANY=false; if has python3 || has python; then PYTHON_ANY=true; fi

cat > "$OUTPUT_PATH" <<JSON
{
  "timestamp": "$(date -u +%Y-%m-%dT%H:%M:%SZ)",
  "tools": {
    "git": $GIT,
    "python": $PYTHON,
    "python3": $PYTHON3,
    "pythonAny": $PYTHON_ANY,
    "pwsh": $PWSH,
    "powershell": $POWERSHELL,
    "bash": $BASH,
    "node": $NODE,
    "npm": $NPM
  }
}
JSON

echo "Tooling report written to $OUTPUT_PATH"
log_json "info" "tooling.detect.done" "{\"path\":\"$OUTPUT_PATH\"}"
