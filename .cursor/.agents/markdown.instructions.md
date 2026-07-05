# 📝 markdown.instructions

> **Quelle:** `.github/.instructions/markdown.instructions.md`
> **Typ:** File Instructions (`applyTo: **/*.md`)
> **Status:** ✅ Sofort aktiv bei Markdown-Dateien

## Kurzbeschreibung
Standardisiert Markdown-Inhalte hinsichtlich Struktur, Formatierung und Validierungsanforderungen.

## Hauptregeln
- Kein H1 im Inhalt (wird häufig extern generiert)
- Hierarchische Überschriften (`##`, `###`)
- Fenced Codeblocks mit Sprachangabe
- Korrekte Links/Bilder/Tabellen
- Sinnvolle Whitespace-Struktur

## Validation-Hinweise
Die Datei nennt strikte Front-Matter-Felder (z.B. `post_title`, `author1`, `post_slug`, `categories`, `post_date`).

## Aktivierung
Automatisch über `applyTo: **/*.md`.

## Parameter
Keine Slash-Parameter; wirkt als Regelwerk für Markdown-Erstellung.

## Eignung
Sehr gut für konsistente Doku-Qualität. Für rein interne MD-Dateien sollte geprüft werden, ob alle Front-Matter-Felder wirklich erforderlich sind.



