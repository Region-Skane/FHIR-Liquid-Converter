## Installation

Det finns två sätt att installera och köra **rs-flc**:

- **A)** Som **.NET Tool** (rekommenderas) via NuGet-feed
- **B)** Som **förbyggd Windows-EXE (win-x64)** via artefakt/ZIP, alternativt via NuGet-package som innehåller EXE

### Förutsättningar

- **.NET SDK 8.x** (för .NET tool-varianten)
- Åtkomst till Azure DevOps Artifacts-feeden som innehåller paketen

---

## Ladda ner paket från Azure DevOps (Artifacts)

Paketen publiceras i Azure DevOps Artifacts-feeden:

- **Feed:** `RS-nuget-feed`
- **Projekt:** `rs-on-fhir`
- **Organisation:** `regionskane-utv`

Det finns två vanliga sätt att hämta dem:

### 1) Via webbgränssnittet (Azure DevOps)

1. Gå till Azure DevOps-projektet: `rs-on-fhir`
2. Öppna **Artifacts** i vänstermenyn
3. Välj feeden **RS-nuget-feed**
4. Sök efter paketnamn:
   - `FLC.Converter.Tool.RS` (tool)
   - `FLC.Converter.Tool.RS.Exe` (exe-package)
5. Klicka på paketet och välj version
6. Ladda ner:
   - `.nupkg` för respektive paket (kan packas upp som zip)

> Tips: en `.nupkg` är en zip-fil. Du kan byta filändelse till `.zip` och packa upp för att inspektera innehållet.

### 2) Via CLI (NuGet / dotnet)

För att kunna hämta paket från en privat feed behöver du normalt:
- feed-URL (v3)
- autentisering (t.ex. PAT) via `dotnet nuget add source` eller via `NuGet.config`

Feed-URL (v3) är:

```text
https://pkgs.dev.azure.com/regionskane-utv/rs-on-fhir/_packaging/RS-nuget-feed/nuget/v3/index.json
```

#### A) Ladda ner en specifik `.nupkg` med `nuget.exe` (Windows)

```powershell
nuget install FLC.Converter.Tool.RS -Version <VERSION> -Source "https://pkgs.dev.azure.com/regionskane-utv/rs-on-fhir/_packaging/RS-nuget-feed/nuget/v3/index.json"
```

För EXE-paketet:

```powershell
nuget install FLC.Converter.Tool.RS.Exe -Version <VERSION> -Source "https://pkgs.dev.azure.com/regionskane-utv/rs-on-fhir/_packaging/RS-nuget-feed/nuget/v3/index.json"
```

Detta laddar ner paketet och packar upp det i en mapp i nuvarande katalog.

#### B) Ladda ner via `dotnet` (genom att installera/uppdatera tool)

Om du kör `.NET Tool`-installationen (se nedan) så sker nedladdning automatiskt från feeden när du installerar/uppdaterar.

---

## A) Installera som .NET Tool (rekommenderas)

Detta installerar verktyget som en .NET tool och ger kommandot:

- `rs-flc`

### 1) Konfigurera NuGet-källa (Azure Artifacts)

Lägg till er feed som NuGet source (exempel med `dotnet nuget`):

```bash
dotnet nuget add source "https://pkgs.dev.azure.com/regionskane-utv/rs-on-fhir/_packaging/RS-nuget-feed/nuget/v3/index.json" ^
  --name "RS-nuget-feed" ^
  --username "AzureDevOps" ^
  --password "<PAT>" ^
  --store-password-in-clear-text
```

> Byt ut `<PAT>` mot en Personal Access Token som har rätt att läsa från feeden.

### 2) Installera verktyget

**Globalt:**

```bash
dotnet tool install --global FLC.Converter.Tool.RS --add-source "RS-nuget-feed"
```

**Eller lokalt i repo** (rekommenderat för reproducerbara builds):

```bash
dotnet new tool-manifest # endast första gången i repo
dotnet tool install FLC.Converter.Tool.RS --add-source "RS-nuget-feed"
```

### 3) Kör

Global installation:

```bash
rs-flc --help
```

Lokal installation (tool-manifest):

```bash
dotnet tool run rs-flc --help
```

### Uppdatera

Global:

```bash
dotnet tool update --global FLC.Converter.Tool.RS --add-source "RS-nuget-feed"
```

Lokal:

```bash
dotnet tool update FLC.Converter.Tool.RS --add-source "RS-nuget-feed"
```

### Avinstallera

Global:

```bash
dotnet tool uninstall --global FLC.Converter.Tool.RS
```

Lokal:

```bash
dotnet tool uninstall FLC.Converter.Tool.RS
```

---

## B) Installera som Windows-EXE (win-x64)

Detta alternativ är för Windows-miljöer där du vill köra en fristående EXE (self-contained).

### Alternativ B1: ZIP-artefakt (förbyggd publish output)

1. Hämta ZIP-filen som byggs av pipeline (namnformat):
   - `rs-flc-win-x64-<version>.zip`

2. Packa upp ZIP:en till valfri mapp, t.ex. `C:\Tools\rs-flc\`

3. Kör:

```powershell
C:\Tools\rs-flc\rs-flc.exe --help
```

> Tips: Lägg till mappen i `PATH` om du vill kunna köra `rs-flc` direkt i terminalen.

### Alternativ B2: NuGet-package som innehåller EXE

Det finns även ett NuGet-paket som innehåller den publicerade EXE-outputen:

- `FLC.Converter.Tool.RS.Exe`

I paketet ligger filer under:

- `tools\win-x64\...`

Ett enkelt sätt att hämta paketet är att ladda ner `.nupkg` från feeden och packa upp det (en `.nupkg` är en zip-fil):

1. Hämta `FLC.Converter.Tool.RS.Exe.<version>.nupkg` från feeden (se “Ladda ner paket…” ovan)
2. Byt namn till `.zip` och packa upp
3. Gå till `tools\win-x64\` och kör:

```powershell
.\rs-flc.exe --help
```

---

## Versioner

Paketen och ZIP-artefakterna versioneras som:

- `0.1.<patch>` (ev. med suffix som `-preview` om det används)

För att se vilka versioner som finns: titta i Azure Artifacts-feeden för:
- `FLC.Converter.Tool.RS` (tool)
- `FLC.Converter.Tool.RS.Exe` (exe-package)
