## arm-migration.agent

> Quelle: `.github/agents/arm-migration.agent.md`  
> Typ: Agent (x86 → ARM Migration)  
> Status: Bedingt nutzbar (MCP/Container-Tools nötig)

### Zweck
Unterstützt Portierung von Workloads auf ARM-Infrastruktur inkl. Abhängigkeitsprüfung, Container-Kompatibilität und Optimierungsvorschlägen.

### Schwerpunkte
- Dockerfile-/Base-Image-Checks
- Paketkompatibilität und Architekturrisiken
- Sprache-/Build-spezifische Migrationshinweise
- Multi-Arch Build- und Validierungspfad

### Aktivierung
`/agent arm-migration-agent`

### Parameter
Keine Slash-Subparameter; die Steuerung erfolgt über den vordefinierten Arbeitsablauf.

### Einsatz
Universell für Infrastruktur- und Plattformmigrationen auf ARM.
