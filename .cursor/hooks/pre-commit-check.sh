#!/usr/bin/env bash
# Pre-commit hook: enforce hard constraints before any commit.
# Place in .cursor/hooks/ and configure via Cursor's hook system.
#
# Catches anti-patterns that agents are instructed never to use,
# providing system-level enforcement as a safety net.

set -euo pipefail

STAGED_FILES=$(git diff --cached --name-only --diff-filter=ACM 2>/dev/null || true)

if [ -z "$STAGED_FILES" ]; then
  exit 0
fi

VIOLATIONS=0

check_pattern() {
  local pattern="$1"
  local label="$2"
  local file="$3"

  if grep -qn "$pattern" "$file" 2>/dev/null; then
    echo "VIOLATION in $file: $label"
    grep -n "$pattern" "$file" 2>/dev/null | head -5
    echo ""
    VIOLATIONS=$((VIOLATIONS + 1))
  fi
}

for file in $STAGED_FILES; do
  if [ ! -f "$file" ]; then
    continue
  fi

  ext="${file##*.}"

  # Determine file category for selective checks
  is_test_file=false
  case "$file" in
    *.test.ts|*.test.tsx|*.spec.ts|*.spec.tsx) is_test_file=true ;;
  esac

  # Legacy JS files in HorosCodeWebSite are exempt from type-safety checks
  is_legacy_js=false
  case "$file" in
    HorosCodeWebSite/*) [ "$ext" = "js" ] && is_legacy_js=true ;;
  esac

  case "$ext" in
    ts|tsx|js|jsx)
      # Type-safety checks: skip in test files and legacy HorosCodeWebSite JS
      if [ "$is_test_file" = false ] && [ "$is_legacy_js" = false ]; then
        check_pattern "as any" "Type safety: 'as any' suppresses type checking" "$file"
        # Only flag @ts-ignore / @ts-expect-error when no justification follows on same line
        check_pattern "@ts-ignore[[:space:]]*$" "Type safety: '@ts-ignore' without justification comment" "$file"
        check_pattern "@ts-expect-error[[:space:]]*$" "Type safety: '@ts-expect-error' without justification comment" "$file"
      fi
      # Empty catch block: flag only when line has no '=>' (avoids false positives like catch(() => ({})))
      empty_catch=$(grep -n "catch.*{[[:space:]]*}" "$file" 2>/dev/null | grep -v "=>" || true)
      if [ -n "$empty_catch" ]; then
        echo "VIOLATION in $file: Error handling: empty catch block"
        echo "$empty_catch" | head -5
        echo ""
        VIOLATIONS=$((VIOLATIONS + 1))
      fi
      ;;
    py)
      check_pattern "except:$" "Error handling: bare except clause" "$file"
      check_pattern "pass$" "Error handling: potential empty except/pass" "$file"
      ;;
  esac
done

if [ "$VIOLATIONS" -gt 0 ]; then
  echo "Found $VIOLATIONS constraint violation(s). Fix before committing."
  exit 1
fi
