# ⬆️ dotnet-maui-9-to-dotnet-maui-10-upgrade.instructions.md

**Pfad:** `.github/.instructions/dotnet-maui-9-to-dotnet-maui-10-upgrade.instructions.md`  
**Typ:** Upgrade Instructions — .NET MAUI 9 → 10 Migration  
**Status:** ✅ Sofort verwendbar — automatisch auf `.csproj`, `.cs`, `.xaml` angewendet  
**Aktivierung:** Automatisch bei diesen Dateitypen ODER `/instructions dotnet-maui-9-to-dotnet-maui-10-upgrade`  
**Gilt für:** `**/*.csproj`, `**/*.cs`, `**/*.xaml`

---

## 🔍 Was ist diese Datei?

Ein detaillierter **Upgrade-Guide** für die Migration von .NET MAUI 9 auf .NET MAUI 10. Enthält Breaking Changes, deprecated APIs und Migrations-Strategien.

> ⭐ **Relevant für Zielprojekt** — das Projekt läuft auf .NET 9, ein Upgrade auf .NET 10 könnte folgen!

---

## 🚀 5-Schritte Upgrade-Prozess

```
Schritt 1: TargetFramework auf net10.0 aktualisieren
Schritt 2: CommunityToolkit.Maui auf 12.3.0+ aktualisieren (PFLICHT)
Schritt 3: Breaking Changes fixen — MessagingCenter (P0)
Schritt 4: ListView/TableView → CollectionView migrieren (P0 KRITISCH)
Schritt 5: Deprecated APIs fixen — Animation, DisplayAlert, IsBusy (P1)
```

---

## ⚠️ Breaking Changes (P0 — MUSS gefixt werden)

### 1. TargetFramework Änderung

```xml
<!-- ALT: .NET 9 -->
<TargetFrameworks>net9.0-windows10.0.19041.0</TargetFrameworks>

<!-- NEU: .NET 10 -->
<TargetFrameworks>net10.0-windows10.0.19041.0</TargetFrameworks>

<!-- Multi-Platform -->
<TargetFrameworks>
  net10.0-android;net10.0-ios;net10.0-maccatalyst;net10.0-windows10.0.19041.0
</TargetFrameworks>
```

### 2. MessagingCenter → WeakReferenceMessenger

```csharp
// ❌ ALT: MessagingCenter (jetzt internal — funktioniert nicht mehr)
MessagingCenter.Send(this, "Connected");
MessagingCenter.Subscribe<MainViewModel>(this, "Connected", (sender) => {});

// ✅ NEU: WeakReferenceMessenger (CommunityToolkit.Mvvm)
WeakReferenceMessenger.Default.Send(new ConnectedMessage());
WeakReferenceMessenger.Default.Register<ConnectedMessage>(this, (r, m) => {});
```

### 3. ListView → CollectionView (KRITISCHSTE Migration)

```xml
<!-- ❌ ALT: ListView (deprecated) -->
<ListView ItemsSource="{Binding Servers}">
    <ListView.ItemTemplate>
        <DataTemplate>
            <TextCell Text="{Binding Name}" />
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>

<!-- ✅ NEU: CollectionView (modern + performant) -->
<CollectionView ItemsSource="{Binding Servers}">
    <CollectionView.ItemTemplate>
        <DataTemplate x:DataType="models:Server">
            <Label Text="{Binding Name}" />
        </DataTemplate>
    </CollectionView.ItemTemplate>
</CollectionView>
```

### 4. TableView → CollectionView

```xml
<!-- ❌ ALT: TableView (deprecated) -->
<TableView>
    <TableRoot>
        <TableSection Title="Settings">
            <TextCell Text="Server" Detail="{Binding ServerName}" />
        </TableSection>
    </TableRoot>
</TableView>

<!-- ✅ NEU: CollectionView mit Grouping -->
<CollectionView ItemsSource="{Binding SettingsGroups}" IsGrouped="true">
    ...
</CollectionView>
```

---

## ⚡ Parameter-System

### Aktivierungswege

| Syntax | Effekt | Sofort nutzbar |
|--------|--------|----------------|
| **Automatisch** | Bei `.csproj`, `.cs`, `.xaml` Dateien | ✅ Ja |
| `/instructions dotnet-maui-9-to-dotnet-maui-10-upgrade` | Explizite Aktivierung | ✅ Ja |

---

## 📋 Deprecated APIs (P1 — Bald fixen)

| Altes API | Neues API | Warum |
|-----------|-----------|-------|
| `MessagingCenter.Send()` | `WeakReferenceMessenger.Default.Send()` | Internal in .NET 10 |
| `ListView` | `CollectionView` | Obsolete, bessere Performance |
| `TableView` | `CollectionView` mit Grouping | Obsolete |
| `Page.IsBusy` | `ActivityIndicator.IsRunning` | Deprecated |
| `Application.MainPage` | Shell Navigation | Deprecated Pattern |

---

## 🔧 CommunityToolkit.Maui Update

```xml
<!-- In Zielprojekt.Win.csproj -->
<PackageReference Include="CommunityToolkit.Maui" Version="12.3.0" />
<!-- MUSS Version 12.3.0 oder neuer sein für .NET MAUI 10 -->
```

---

## 💡 Praktische Anwendungsbeispiele

```bash
# Upgrade starten
/instructions dotnet-maui-9-to-dotnet-maui-10-upgrade aktualisiere Zielprojekt.Win.csproj

# ListView Migration
migriere alle ListView Elemente in ServersPage.xaml auf CollectionView

# MessagingCenter ersetzen
ersetze alle MessagingCenter Aufrufe mit WeakReferenceMessenger

# Upgrade vollständig durchführen
führe alle P0 Breaking Changes in Zielprojekt durch

# Deprecated APIs prüfen
suche alle deprecated APIs in Zielprojekt die für .NET 10 geändert werden müssen
```

---

## ⚙️ Anpassungen — Muss man etwas schreiben?

> **NEIN für Nutzung** — Die Instructions sind aktiv und sofort wirksam!

**Empfehlung für Zielprojekt:**
Diese Instructions erst aktivieren wenn ein Upgrade auf .NET 10 geplant ist. Das Projekt läuft aktuell auf `net9.0-windows10.0.19041.0` — die Instructions gelten dann automatisch.

---

*Erstellt von CODEX — Experten Team Auswahl README System*



