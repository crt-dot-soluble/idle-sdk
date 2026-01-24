#!/usr/bin/env bash
set -euo pipefail

RENDERER="${1:-}"
PERSISTENCE="${2:-}"
PLUGIN_PACKAGING="${3:-}"

if [[ -z "$RENDERER" || -z "$PERSISTENCE" || -z "$PLUGIN_PACKAGING" ]]; then
  echo "Usage: record-architecture-decisions.sh <renderer> <persistence> <pluginPackaging>" >&2
  exit 1
fi

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PATH_OUT="$ROOT_DIR/docs/ARCHITECTURE-DECISIONS.md"
DATE_STAMP="$(date +%Y-%m-%d)"

cat > "$PATH_OUT" <<EOF
# Architecture Decisions (Phase 2 Prerequisites)

- Date: $DATE_STAMP
- Renderer abstraction: $RENDERER
- Persistence backend default: $PERSISTENCE
- Plugin packaging format: $PLUGIN_PACKAGING

EOF

echo "Recorded architecture decisions to $PATH_OUT"
