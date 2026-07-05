#!/usr/bin/env bash
# afterFileEdit hook — lints a file right after the agent edits it.
#
# Reads a JSON payload on stdin (fields: file_path, edits, hook_event_name, ...).
# afterFileEdit is INFORMATIONAL: Cursor ignores the output, so this hook only surfaces
# lint findings; it cannot (and should not) block the edit.
#
# Uses the same PATH-robust JSON parsing as guard-shell.sh (Cursor runs hooks with the
# GUI app PATH, which may lack python3/jq). Set OMC_HOOKS_DEBUG=1 to log invocations.

set -uo pipefail

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
INPUT="$(cat)"

json_field_bash() {
  local field="$1" data="$2"
  if [[ "$data" =~ \"${field}\"[[:space:]]*:[[:space:]]*\"(([^\"\\]|\\.)*)\" ]]; then
    local val="${BASH_REMATCH[1]}"
    val="${val//\\n/$'\n'}"
    val="${val//\\r/$'\r'}"
    val="${val//\\t/$'\t'}"
    val="${val//\\\"/\"}"
    val="${val//\\\\/\\}"
    printf '%s' "$val"
    return 0
  fi
  return 1
}

json_field() {
  local field="$1" data="$2" out py perl_cmd

  if command -v jq >/dev/null 2>&1; then
    if out="$(printf '%s' "$data" | jq -r --arg f "$field" '.[$f] // empty' 2>/dev/null)"; then
      printf '%s' "$out"
      return 0
    fi
  fi

  if command -v py >/dev/null 2>&1; then
    if out="$(printf '%s' "$data" | py -3 -c "import sys,json; print(json.load(sys.stdin).get('$field',''), end='')" 2>/dev/null)"; then
      printf '%s' "$out"
      return 0
    fi
  fi

  for py in python3 python; do
    if command -v "$py" >/dev/null 2>&1; then
      if out="$(printf '%s' "$data" | "$py" -c "import sys,json; print(json.load(sys.stdin).get('$field',''), end='')" 2>/dev/null)"; then
        printf '%s' "$out"
        return 0
      fi
    fi
  done

  perl_cmd=""
  if command -v perl >/dev/null 2>&1; then
    perl_cmd="perl"
  elif [ -x /usr/bin/perl ]; then
    perl_cmd="/usr/bin/perl"
  fi
  if [ -n "$perl_cmd" ]; then
    if out="$(printf '%s' "$data" | "$perl_cmd" -0777 -ne 'if(/"'"$field"'"\s*:\s*"((?:[^"\\]|\\.)*)"/s){my $x=$1;$x=~s/\\n/\n/g;$x=~s/\\(.)/$1/g;print $x}' 2>/dev/null)"; then
      if [ -n "$out" ] || printf '%s' "$data" | grep -q "\"${field}\""; then
        printf '%s' "$out"
        return 0
      fi
    fi
  fi

  json_field_bash "$field" "$data"
}

FILE="$(json_field file_path "$INPUT")"

if [ "${OMC_HOOKS_DEBUG:-}" = "1" ]; then
  printf '%s\tedit\t%s\n' "$(date '+%FT%T' 2>/dev/null)" "${FILE:-<unparsed>}" >> "$HERE/last-invocation.log" 2>/dev/null || true
fi

[ -z "$FILE" ] && exit 0
[ -f "$FILE" ] || exit 0

ext="${FILE##*.}"

case "$ext" in
  ts|tsx|js|jsx)
    command -v npx >/dev/null 2>&1 && npx eslint --no-error-on-unmatched-pattern "$FILE" 2>/dev/null || true
    ;;
  py)
    if command -v ruff >/dev/null 2>&1; then
      ruff check "$FILE" 2>/dev/null || true
    elif command -v flake8 >/dev/null 2>&1; then
      flake8 "$FILE" 2>/dev/null || true
    fi
    ;;
  rs)
    command -v cargo >/dev/null 2>&1 && cargo clippy --message-format=short 2>/dev/null || true
    ;;
  go)
    command -v golangci-lint >/dev/null 2>&1 && golangci-lint run "$FILE" 2>/dev/null || true
    ;;
esac

exit 0
