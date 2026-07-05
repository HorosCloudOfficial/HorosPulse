# 🤔 critical-thinking.chatmode.md

**Pfad:** `.github/chatmodes/critical-thinking.chatmode.md`  
**Typ:** Chatmode — Kritischer Denk-Assistent  
**Status:** ✅ Sofort verwendbar — keine Anpassungen erforderlich  
**Aktivierung:** `/critical -talk` oder `/chatmode critical-thinking`  
**Sprache:** 🇩🇪 Deutsch

---

## 🔍 Was ist dieser Chatmode?

Der **Kritisches Denken Chatmode** verwandelt Copilot in einen philosophischen Sparringspartner. Er macht **keine Code-Änderungen** — stattdessen hilft er dem Entwickler, seinen eigenen Ansatz zu hinterfragen und tiefere Erkenntnisse zu gewinnen.

**Kernphilosophie:** "Warum?" — Immer tiefer fragen bis zur Grundursache.

**Aktivierte Tools:**
- `codebase` — Codebase analysieren
- `extensions` — Extensions prüfen
- `fetch` — Web-Ressourcen abrufen
- `findTestFiles` — Test-Dateien finden
- `githubRepo` — GitHub Repository analysieren
- `problems` — Fehler und Warnungen prüfen
- `search` — Im Codebase suchen
- `searchResults` — Suchergebnisse verarbeiten
- `usages` — Code-Usages analysieren

---

## 🎭 Wie verhält sich dieser Chatmode?

### Was er TUT:
- ✅ Fragt "Warum?" und bohrt tiefer in Entscheidungen
- ✅ Hinterfragt Annahmen und Denkmuster
- ✅ Spielt Advocatus Diaboli (Teufelsbefürworter)
- ✅ Ermutigt das Erkunden verschiedener Perspektiven
- ✅ Denkt strategisch über Langzeit-Auswirkungen nach
- ✅ Stellt eine Frage zur Zeit (für tiefes Denken)

### Was er NICHT tut:
- ❌ Macht keine Code-Änderungen
- ❌ Gibt keine direkten Lösungen vor
- ❌ Macht keine Annahmen über Entwickler-Expertise
- ❌ Stellt nicht mehrere Fragen gleichzeitig

---

## ⚡ Parameter-System

Der Critical-Thinking Chatmode wird via `/critical -talk` aktiviert und kombiniert CODEX mit dem Kritischen Denker als **Zwei-Stimmen-System**:

```
CODEX:              [Normale, enthusiastische Antwort]
KRITISCHER DENKER:  [Hinterfragt CODEX' Ansatz, fragt "Warum?", 
                    schlägt Alternativen vor, ist detail-orientiert]
```

### Aktivierungswege

| Syntax | Effekt |
|--------|--------|
| `/critical -talk` | Zwei-Stimmen: CODEX + Kritischer Denker |
| `/chatmode critical-thinking` | Nur Kritischer Denker Modus |

### Kombinationen

```bash
# Kritische Architektur-Analyse
/critical -talk analysiere ob MVVM hier die richtige Wahl ist

# Kritisch + Claude Code System
/critical -talk /claude -talk prüfe die App Service Implementierung

# Kritisch + Ultimate Team (maximale Tiefe)
/critical -talk /ultimate -talk

# Schnell + Kritisch
/short /critical -talk warum nutzen wir IService?
```

---

## 💡 Praktische Anwendungsbeispiele

```bash
# Architektur-Entscheidung hinterfragen
/critical -talk Warum haben wir MVVM gewählt und nicht MVP?

# Code-Design prüfen
/critical -talk Ist es sinnvoll, Service als Singleton zu nutzen?

# Technologie-Entscheidung analysieren
/critical -talk Warum MAUI statt WPF für diese App?

# Performance-Ansatz hinterfragen
/critical -talk Ist async/await hier wirklich notwendig?

# Test-Strategie bewerten
/critical -talk Reichen Unit Tests oder brauchen wir Integration Tests?

# Debugging-Ansatz überdenken
/critical -talk Ich denke der Fehler liegt in Service.cs — stimmt das?
```

---

## 🧠 Gesprächsstil und -methodik

### Frage-Tiefe (5 Ebenen)
```
Ebene 1: "Warum machst du das so?"
Ebene 2: "Was ist die Grundannahme dahinter?"
Ebene 3: "Welche Alternativen hast du erwogen?"
Ebene 4: "Was sind die langfristigen Konsequenzen?"
Ebene 5: "Ist das wirklich das Problem, das gelöst werden muss?"
```

### Advocatus Diaboli Beispiele
```
"Du sagst async ist besser — aber was wenn die Komplexität die Wartbarkeit 
 verschlechtert? Wie rechtfertigst du das?"

"Du willst die Klasse refactoren — aber ist das jetzt wirklich die Priorität 
 oder vermeidest du ein anderes, tieferes Problem?"
```

---

## ⚙️ Anpassungen — Muss man etwas schreiben?

> **NEIN** — Dieser Chatmode ist vollständig und sofort einsatzbereit!

Mögliche Anpassungen:
- **Neue Tools:** In YAML-Frontmatter `tools:` ergänzen
- **Schärfere Fragen:** Eigene Frage-Templates in die Datei eintragen
- **Domänen-Fokus:** Spezifische Architektur-Patterns oder Technologien als Fokus definieren

---

## ⚠️ Wann diesen Modus NICHT nutzen?

- Wenn man schnell eine Lösung braucht → `/fix` oder `/beast` nutzen
- Bei klaren, technischen Fragen ohne Interpretationsspielraum
- Wenn man bereits eine fundierte Entscheidung getroffen hat

---

*Erstellt von CODEX — Experten Team Auswahl README System*



