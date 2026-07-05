#!/usr/bin/env bash
# beforeShellExecution hook — guards the agent's shell calls.
#
# Reads a JSON payload on stdin (fields: command, cwd, hook_event_name, ...) and prints a
# JSON decision on stdout per Cursor's hook protocol:
#   {"permission": "allow|deny|ask", "user_message": "...", "agent_message": "..."}
#
# Blocks (deny) clearly destructive commands and commits that contain the anti-patterns
# Team Avatar agents are told never to introduce.
#
# Robustness notes:
#  - Cursor runs hooks with the GUI app's PATH, which often lacks dev-tool paths.
#    json_field tries jq, py/python, perl, then a pure-bash string extractor.
#  - On Windows, hooks.json must invoke this script via bash (Git Bash), not directly.
#  - We cd into the payload's `cwd` before running git checks so `git diff --cached` resolves
#    in the agent's actual worktree.
#  - Pair with "failClosed": true in hooks.json so a script error denies instead of allowing.
#
# Env switches:
#   OMC_HOOKS_OBSERVE=1  run non-blocking: downgrade every deny to allow + an informational note.
#   OMC_HOOKS_DEBUG=1    append each invocation's command to hooks/last-invocation.log (proves firing).

set -uo pipefail

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
INPUT="$(cat)"

# --- Pure-bash JSON string-field extraction (last resort; handles escaped quotes) ---
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

# --- Robust JSON string-field extraction (no reliance on a single PATH layout) ---
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

  for py in /opt/homebrew/bin/python3 /usr/local/bin/python3 /usr/bin/python3; do
    if [ -x "$py" ] || command -v "$py" >/dev/null 2>&1; then
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
    if out="$(printf '%s' "$data" | "$perl_cmd" -0777 -ne 'if(/"'"$field"'"\s*:\s*"((?:[^"\\]|\\.)*)"/s){my $x=$1;$x=~s/\\n/\n/g;$x=~s/\\t/\t/g;$x=~s/\\(.)/$1/g;print $x}' 2>/dev/null)"; then
      if [ -n "$out" ] || printf '%s' "$data" | grep -q "\"${field}\""; then
        printf '%s' "$out"
        return 0
      fi
    fi
  fi

  json_field_bash "$field" "$data"
}

COMMAND="$(json_field command "$INPUT")"; PARSE_RC=$?
CWD="$(json_field cwd "$INPUT")"

if [ "${OMC_HOOKS_DEBUG:-}" = "1" ]; then
  printf '%s\t%s\n' "$(date '+%FT%T' 2>/dev/null)" "${COMMAND:-<unparsed>}" >> "$HERE/last-invocation.log" 2>/dev/null || true
fi

# Emit valid JSON (Windows paths/quotes break naive printf JSON).
_emit_hook_json_python() {
  HOOK_PERMISSION="$1" HOOK_USER_MSG="${2:-}" HOOK_AGENT_MSG="${3:-}" "$4" -c '
import json, os
obj = {"permission": os.environ["HOOK_PERMISSION"]}
user_msg = os.environ.get("HOOK_USER_MSG", "")
agent_msg = os.environ.get("HOOK_AGENT_MSG", "")
if user_msg:
    obj["user_message"] = user_msg
if agent_msg:
    obj["agent_message"] = agent_msg
print(json.dumps(obj))
' 2>/dev/null
}

emit_hook_json() {
  local permission="$1" user_message="${2:-}" agent_message="${3:-}" out py

  for py in python3 python; do
    if command -v "$py" >/dev/null 2>&1; then
      out="$(_emit_hook_json_python "$permission" "$user_message" "$agent_message" "$py")" || true
      if [ -n "$out" ]; then printf '%s\n' "$out"; return 0; fi
    fi
  done

  if command -v py >/dev/null 2>&1; then
    out="$(HOOK_PERMISSION="$permission" HOOK_USER_MSG="$user_message" HOOK_AGENT_MSG="$agent_message" \
      py -3 -c 'import json,os
obj={"permission":os.environ["HOOK_PERMISSION"]}
um=os.environ.get("HOOK_USER_MSG","")
am=os.environ.get("HOOK_AGENT_MSG","")
if um: obj["user_message"]=um
if am: obj["agent_message"]=am
print(json.dumps(obj))' 2>/dev/null)" || true
    if [ -n "$out" ]; then printf '%s\n' "$out"; return 0; fi
  fi

  for py in /c/Windows/py.exe /c/Python311/python.exe /c/Python310/python.exe /c/Python39/python.exe; do
    if [ -x "$py" ]; then
      out="$(_emit_hook_json_python "$permission" "$user_message" "$agent_message" "$py")" || true
      if [ -n "$out" ]; then printf '%s\n' "$out"; return 0; fi
    fi
  done

  # Minimal fallback: allow-only without messages (safe literal JSON).
  if [ "$permission" = "allow" ] && [ -z "$user_message" ] && [ -z "$agent_message" ]; then
    printf '{"permission":"allow"}\n'
    return 0
  fi

  printf '{"permission":"deny","user_message":"Hook JSON emitter unavailable","agent_message":"Install Python 3 so guard-shell.sh can emit valid JSON."}\n'
  return 1
}

allow() { emit_hook_json allow; exit 0; }
ask() { emit_hook_json ask "$1" "$1"; exit 0; }
decide() {
  local reason="$1"
  if [ "${OMC_HOOKS_OBSERVE:-}" = "1" ]; then
    emit_hook_json allow "" "[observe] would block: $reason"
    exit 0
  fi
  emit_hook_json deny "Blocked by oh-my-cursor: $reason" "Denied by policy: $reason. Adjust .cursor/hooks/guard-shell.sh or set OMC_HOOKS_OBSERVE=1 to override."
  exit 0
}
# Surface for human approval (deterministic "hold for review"). Used where the auto-review
# classifier is unreliable, e.g. reading credential/secret files.
hold() {
  local reason="$1"
  if [ "${OMC_HOOKS_OBSERVE:-}" = "1" ]; then
    emit_hook_json allow "" "[observe] would hold for review: $reason"
    exit 0
  fi
  emit_hook_json ask "Held by oh-my-cursor for review: $reason" "Held for review: $reason. Approve only if this credential/secret access is intended."
  exit 0
}

# Received a payload but couldn't parse the command -> surface instead of silently allowing.
if [ -n "$INPUT" ] && { [ "$PARSE_RC" -ne 0 ] || { [ -z "$COMMAND" ] && printf '%s' "$INPUT" | grep -q '"command"'; }; }; then
  [ "${OMC_HOOKS_OBSERVE:-}" = "1" ] && allow
  ask "oh-my-cursor guard could not parse the shell command; review before running"
fi

[ -z "$COMMAND" ] && allow

# Resolve git checks in the agent's real worktree.
if [ -n "$CWD" ] && [ -d "$CWD" ]; then cd "$CWD" 2>/dev/null || true; fi

# --- Destructive command denylist (unambiguous, always guarded) ---
case "$COMMAND" in
  *"rm -rf /"*|*"rm -rf ~"*|*"rm -rf /*"*|*":(){ :|:& };:"*) decide "destructive filesystem command" ;;
  *"git push"*"--force"*"main"*|*"git push"*"--force"*"master"*|*"git push -f"*"main"*|*"git push -f"*"master"*) decide "force-push to a protected branch" ;;
  *"git reset --hard origin/main"*|*"git reset --hard origin/master"*) decide "hard reset of a shared branch" ;;
  *"chmod -R 777 /"*|*"mkfs"*|*"dd if="*"of=/dev/"*) decide "system-level destructive command" ;;
esac

# --- Commit anti-pattern guard ---
case "$COMMAND" in
  *"git commit"*)
    if [ -x "$HERE/pre-commit-check.sh" ]; then
      if ! OUT="$("$HERE/pre-commit-check.sh" 2>&1)"; then
        decide "commit contains forbidden anti-patterns ($(printf '%s' "$OUT" | tr '\n' ' ' | cut -c1-160))"
      fi
    fi
    ;;
esac

# --- Credential / secret access guard (hold for review) ---
# The permissions.json auto-review classifier is best-effort and let `cat ~/.ssh/config`
# through during E2E test C2, so enforce a deterministic hold here regardless of the classifier.
case "$COMMAND" in
  *"/.ssh/"*|*"/.aws/"*|*"/.gnupg/"*|*"id_rsa"*|*"id_dsa"*|*"id_ecdsa"*|*"id_ed25519"*|*".pem"*|*".netrc"*|*".pgpass"*|*"/.kube/config"*|*"kubeconfig"*|*"/.docker/config.json"*|*"/.config/gcloud/"*|*".aws/credentials"*)
    hold "command accesses a credential/secret file" ;;
esac

allow
