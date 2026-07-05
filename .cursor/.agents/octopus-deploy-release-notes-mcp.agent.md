## octopus-deploy-release-notes-mcp.agent

> Quelle: ``.github\agents\octopus-deploy-release-notes-mcp.agent.md``
> Typ: Agent
> Status: Sofort nutzbar

### Zweck
Generate release notes for a release in Octopus Deploy. The tools for this MCP server provide access to the Octopus Deploy APIs.

### Schwerpunkte
- Primaeres Thema: Release Notes for Octopus Deploy
- Geltungsbereich: Nicht spezifiziert
- Umfang der Quelle: 52 Zeilen

### Wichtige Kapitel
- Keine Unterkapitel im Quelltext erkannt

### Aktivierung
`/agent octopus-deploy-release-notes-mcp`

### Parameter
- ``args``: ``(leer)``
- ``command``: ``npx``
- ``description``: ``Generate release notes for a release in Octopus Deploy. The tools for this MCP server provide access to the Octopus Deploy APIs.``
- ``env``: ``(leer)``
- ``mcp-servers``: ``(leer)``
- ``name``: ``octopus-release-notes-with-mcp``
- ``octopus``: ``(leer)``
- ``OCTOPUS_API_KEY``: ``${{ secrets.OCTOPUS_API_KEY }}``
- ``OCTOPUS_SERVER_URL``: ``${{ secrets.OCTOPUS_SERVER_URL }}``
- ``tools``: ``(leer)``
- ``type``: ``local``

### Einsatz
Universell einsetzbar als wiederverwendbare Arbeitsregel fuer den jeweils beschriebenen Tech- oder Prozesskontext.

