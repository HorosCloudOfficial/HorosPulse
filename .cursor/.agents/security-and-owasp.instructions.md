# 🛡️ security-and-owasp.instructions.md

**Pfad:** `.github/.instructions/security-and-owasp.instructions.md`  
**Typ:** Security Instructions — OWASP Top 10 Standards  
**Status:** ✅ Sofort verwendbar — automatisch auf **ALLE** Dateien angewendet  
**Aktivierung:** Automatisch für alle Dateien ODER `/instructions security-and-owasp`  
**Gilt für:** `*` (alle Dateitypen und Sprachen)

---

## 🔍 Was ist diese Datei?

Die umfassendsten **Security-Coding-Richtlinien** für alle Sprachen und Frameworks, basierend auf dem **OWASP Top 10** und Best Practices. Wird automatisch auf ALLE Dateien angewendet.

> ⭐ **Kritisch für Zielprojekt** — Apps müssen höchste Sicherheitsstandards einhalten!

---

## 🔒 OWASP Top 10 — Implementierte Regeln

### A01: Broken Access Control
```csharp
// ✅ Principle of Least Privilege
// ✅ Deny by Default
// ✅ Path Traversal Prevention

// Beispiel: Datei-Zugriff sicher
var safeFileName = Path.GetFileName(userInput); // Kein Path Traversal
var fullPath = Path.Combine(configDir, safeFileName);
if (!fullPath.StartsWith(configDir)) throw new SecurityException("Path traversal!");
```

### A02: Kryptografische Fehler
```csharp
// ✅ RICHTIG: AES-256 für sensible Daten
using var aes = Aes.Create();
aes.KeySize = 256;

// ✅ RICHTIG: Starke Hashing-Algorithmen
// Bcrypt oder Argon2 für Passwörter

// ❌ VERBOTEN: MD5 oder SHA-1 für Passwörter
// ❌ VERBOTEN: HTTP statt HTTPS

// ✅ RICHTIG: Secrets aus Umgebung laden
var apiKey = Environment.GetEnvironmentVariable("APP_API_KEY");
// NIEMALS: var apiKey = "sk-hardcoded-key"; ← Security-Verletzung!
```

### A03: Injection Prevention
```csharp
// ✅ RICHTIG: Parameterized Queries
cmd.CommandText = "SELECT * FROM Servers WHERE Id = @id";
cmd.Parameters.AddWithValue("@id", serverId);

// ❌ VERBOTEN: String Concatenation
// cmd.CommandText = $"SELECT * FROM Servers WHERE Id = {serverId}";

// ✅ RICHTIG: Shell Command Escaping
var process = new Process();
process.StartInfo.FileName = "openvpn";
process.StartInfo.ArgumentList.Add("--config");  // Separate Args = kein Injection
process.StartInfo.ArgumentList.Add(configPath);
```

### A05: Security Misconfiguration
```csharp
// ✅ In Production: Verbose Errors deaktivieren
#if !DEBUG
app.UseExceptionHandler("/error");
#endif

// ✅ Security Headers für Web-Teile
response.Headers.Add("X-Content-Type-Options", "nosniff");
response.Headers.Add("Strict-Transport-Security", "max-age=31536000");
```

### A07: Authentication Failures
```csharp
// ✅ Rate Limiting für Login
// ✅ Session Rotation nach Login
// ✅ Secure + HttpOnly Cookies
```

---

## ⚡ Parameter-System

### Aktivierungswege

| Syntax | Effekt | Sofort nutzbar |
|--------|--------|----------------|
| **Automatisch** | Gilt für ALLE Dateien immer | ✅ Ja |
| `/instructions security-and-owasp` | Explizite Aktivierung | ✅ Ja |
| `CODEX Security Mode` | Fokus auf Security | ✅ Ja |

### Kombinationen

```bash
# Security Audit + Elite Team
/agent ULTIMATE-PROJECT-ANALYSIS-TEAM /focus=security

# Security + Performance Review
CODEX Security Mode /focus=performance

# Vollständiger OWASP Check
/instructions security-and-owasp überprüfe Service.cs auf alle OWASP Top 10
```

---

## 🚨 Top Security Regeln für Zielprojekt

### Secrets Management
```csharp
// ❌ NIEMALS in Code
private const string AppApiKey = "abc123secret";

// ✅ RICHTIG: Umgebungsvariable oder Windows Credential Store
var appKey = Environment.GetEnvironmentVariable("APP_SECRET_KEY");
// oder
var credential = new Windows.Security.Credentials.PasswordVault()
    .Retrieve("Zielprojekt", "ApiKey");
```

### Verbindungs-Sicherheit
```csharp
// ✅ HTTPS für alle API-Calls
private const string BaseUrl = "https://api.Zielprojekt.com"; // HTTPS!

// ✅ Certificate Validation NICHT deaktivieren
// ❌ VERBOTEN: ServerCertificateValidationCallback = (s,c,ch,e) => true;

// ✅ CancellationToken für alle Operationen
public async Task ConnectAsync(Server server, CancellationToken ct)
```

### Input Validation
```csharp
// ✅ Server-URLs validieren (SSRF Prevention)
if (!Uri.TryCreate(serverUrl, UriKind.Absolute, out var uri) || 
    uri.Scheme != "https")
{
    throw new ArgumentException("Invalid server URL");
}
```

---

## 📋 Security Review Checkliste für Zielprojekt

```
✅ Keine Hardcoded Credentials
✅ HTTPS für alle Verbindungen
✅ Input Validation für alle User-Inputs
✅ Keine Shell Injection bei OpenVPN Aufrufen
✅ Secure Credential Storage (Windows Credential Store)
✅ Certificate Validation aktiv
✅ Logging ohne sensitive Daten
✅ Rate Limiting für Server-Zugriff
```

---

## 💡 Praktische Anwendungsbeispiele

```bash
# Vollständiger OWASP Audit
/instructions security-and-owasp führe einen vollständigen Security Audit durch

# Spezifischer Check
überprüfe Service.cs auf SQL Injection und Command Injection Risiken

# Secrets suchen
suche alle hardcoded Credentials und API-Keys im gesamten Zielprojekt Projekt

# Verbindungs-Sicherheit
prüfe ob alle Verbindungen TLS/HTTPS verwenden und Zertifikate validiert werden
```

---

## ⚙️ Anpassungen — Muss man etwas schreiben?

> **NEIN** — Diese Instructions gelten automatisch für ALLE Dateien!

Besonders wichtig für Anwendungen — bereits aktiv ohne Konfiguration.

**Empfehlung:** Für einen initialen Security Audit explizit aufrufen:
```bash
CODEX Security Mode: vollständiger OWASP Top 10 Audit für Zielprojekt
```

---

*Erstellt von CODEX — Experten Team Auswahl README System*



