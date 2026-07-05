## write-coding-standards-from-file.prompt

> Quelle: ``.github\prompts\write-coding-standards-from-file.prompt.md``
> Typ: Prompt
> Status: Sofort nutzbar

### Zweck
Write a coding standards document for a project using the coding styles from the file(s) and/or folder(s) passed as arguments in the prompt.

### Schwerpunkte
- Primaeres Thema: Write Coding Standards From File
- Geltungsbereich: Nicht spezifiziert
- Umfang der Quelle: 316 Zeilen

### Wichtige Kapitel
- Rules and Configuration
- Variable and Parameter Configuration Conditions
- **if** `${fetchStyleURL} == true`
- Coding Standards Templates
- 1. Introduction
- 2. Naming Conventions

### Aktivierung
`/prompt write-coding-standards-from-file`

### Parameter
- ``description``: ``Write a coding standards document for a project using the coding styles from the file(s) and/or folder(s) passed as arguments in the prompt.``
- ``mode``: ``agent``
- ``tools``: ``['createFile', 'editFiles', 'fetch', 'githubRepo', 'search', 'testFailure']``

### Einsatz
Universell einsetzbar als wiederverwendbare Arbeitsregel fuer den jeweils beschriebenen Tech- oder Prozesskontext.

