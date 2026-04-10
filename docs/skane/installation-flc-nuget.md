## Installation

Den här pipelinen publicerar **ett NuGet-paket** (bibliotek) för **Fhir.Liquid.Converter**.

- **NuGet-packageId:** `Fhir.Liquid.Converter.RS`
- **Feed:** `RS-nuget-feed`

### Förutsättningar

- **.NET SDK 8.x** (om du ska bygga lokalt, eller om du vill använda `dotnet`/NuGet-verktyg)
- Åtkomst till Azure DevOps Artifacts-feeden som innehåller paketet

---

## Ladda ner paket från Azure DevOps (Artifacts)

Paketet publiceras i Azure DevOps Artifacts-feeden:

- **Feed:** `RS-nuget-feed`
- **Projekt:** `rs-on-fhir`
- **Organisation:** `regionskane-utv`

Det finns två vanliga sätt att hämta paketet:

### 1) Via webbgränssnittet (Azure DevOps)

1. Gå till Azure DevOps-projektet: `rs-on-fhir`
2. Öppna **Artifacts** i vänstermenyn
3. Välj feeden **RS-nuget-feed**
4. Sök efter paketet: `Fhir.Liquid.Converter.RS`
5. Klicka på paketet och välj version
6. Ladda ner `.nupkg`

> Tips: en `.nupkg` är en zip-fil. Du kan byta filändelse till `.zip` och packa upp för att inspektera innehållet.

### 2) Via CLI (NuGet / dotnet)

Feed-URL (v3):

```text
https://pkgs.dev.azure.com/regionskane-utv/rs-on-fhir/_packaging/RS-nuget-feed/nuget/v3/index.json
```

#### A) Ladda ner en specifik `.nupkg` med `nuget.exe` (Windows)

```powershell
nuget install Fhir.Liquid.Converter.RS -Version <VERSION> -Source "https://pkgs.dev.azure.com/regionskane-utv/rs-on-fhir/_packaging/RS-nuget-feed/nuget/v3/index.json"
```

Detta laddar ner paketet och packar upp det i en mapp i nuvarande katalog.

---

## Använd paketet i ett .NET-projekt

### 1) Lägg till NuGet-källa (Azure Artifacts)

Om du inte redan har feeden konfigurerad lokalt kan du lägga till den som en NuGet source (exempel med `dotnet nuget`):

```bash
dotnet nuget add source "https://pkgs.dev.azure.com/regionskane-utv/rs-on-fhir/_packaging/RS-nuget-feed/nuget/v3/index.json" ^
  --name "RS-nuget-feed" ^
  --username "AzureDevOps" ^
  --password "<PAT>" ^
  --store-password-in-clear-text
```

> Byt ut `<PAT>` mot en Personal Access Token som har rätt att läsa från feeden.

### 2) Installera paketet

I ditt projekt:

```bash
dotnet add package Fhir.Liquid.Converter.RS --source "RS-nuget-feed"
```

Alternativt (om du vill ange exakt version):

```bash
dotnet add package Fhir.Liquid.Converter.RS --version <VERSION> --source "RS-nuget-feed"
```

### 3) Restore / Build

```bash
dotnet restore
dotnet build
```

---

## Versioner

Paketet versioneras som:

- `0.0.<patch>` (ev. med suffix som `-preview` / `-rc` om det används)

För att se vilka versioner som finns: titta i Azure Artifacts-feeden för:
- `Fhir.Liquid.Converter.RS`
