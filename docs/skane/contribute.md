# Contributions: Azure DevOps → Region Skåne GitHub → PR till Service Well GitHub

Det här dokumentet beskriver standardflödet för att göra contributions där utveckling sker i **Azure DevOps**, men bidraget ska levereras som **Pull Request i GitHub** till **Service Well Fhir Liquid Converter**.

---

## Syfte

Flödet säkerställer att:

- vi kan arbeta och granska internt i Azure DevOps (via `contribution`)
- vi kan skapa en PR i GitHub tillbaka till Service Well (för review/CI/merge)
- GitHub-underlaget alltid kommer från en definierad källa (ADO `contribution`) via en kontrollerad spegling (pipeline)

---

## Begrepp och branchar

- **`public/main`**: upstream-bas i GitHub (”upstream mirror branch”) som vi ska ligga i fas med
- **`contribution`**: integrationsbranch i Azure DevOps där interna contributions samlas
- **feature-branch**: din arbetsbranch som PR:as in i `contribution`
- **GitHub mirror-branch**: branch i Region Skånes GitHub som pipelinen uppdaterar (ex. `skane/contribution`)
- **Service Well GitHub (fork parent)**: repo där slutlig PR ska skapas och merge:as

---

## När flödet ska användas

Använd detta flöde när:

- ändringar görs i Azure DevOps men ska bidra tillbaka via GitHub PR
- ni vill följa Service Wells GitHub-process (checks/review/merge)
- ni behöver en repeterbar väg att skapa PR-underlag i GitHub från ADO

Undvik flödet om arbetet redan sker direkt i Service Wells GitHub (då används vanligt GitHub-branch + PR).

---

## Flöde

### 1. Skapa feature-branch och gör ändringar
1. Skapa en feature-branch från `contribution` i Azure DevOps.
2. Gör ändringar, commit:a och pusha.

---

### 2. Säkerställ att `contribution` och feature-branch är uppdaterade mot `public/main`
Det här steget görs innan PR till `contribution`, för att minska konflikter och ge en ren PR-kedja.

**2.1 Uppdatera `contribution`**
- Synka in senaste från `public/main` till `contribution` (merge eller rebase enligt teamets praxis).
- Pusha uppdaterad `contribution`.

**2.2 Uppdatera feature-branch**
- Synka feature-branchen mot den uppdaterade `contribution` (merge eller rebase enligt praxis).
- Lös konflikter i feature-branchen och pusha.

---

### 3. Skapa PR i Azure DevOps: feature → `contribution`
1. Skapa PR från feature-branch till `contribution`.
2. Genomför review och åtgärda feedback.
3. Merge:a PR:n så att `contribution` innehåller den färdiga ändringen.

---

### 4. Kör pipelinen: spegla ADO `contribution` till Region Skåne GitHub
När `contribution` är klar (uppdaterad och merge:ad) körs pipelinen som:

- speglar innehållet i ADO `contribution` till GitHub mirror-branchen (ex. `skane/contribution`)

**Viktigt:**
- mirror-branchen är **maskinskriven** och ska inte användas för manuella commits
- pipelinen används när du vill skapa/uppdatera GitHub PR-underlaget

---

### 5. Skapa PR i GitHub: Region Skåne → Service Well (fork parent)
1. Skapa en PR i GitHub där:
   - **Base repository**: Service Well Fhir Liquid Converter
   - **Base branch**: `main` (eller enligt överenskommelse)
   - **Compare branch**: GitHub mirror-branchen (ex. `skane/contribution`)
2. Lägg in beskrivning, länkar till ADO (PR/work items) och testinfo.
3. Låt checks gå grönt och genomför review.
4. Merge enligt Service Wells process.

---

## Regler och rekommendationer

- Gör inga manuella commits på GitHub mirror-branchen (den kan skrivas över av pipelinen).
- Hantera konflikter så tidigt som möjligt i feature-branchen (steg 2.2).
- Kör pipelinen “när det behövs” (inför PR i GitHub eller när PR behöver uppdateras), inte för varje liten commit.

---

## Vanliga problem

- **“No changes to sync” i pipelinen:** ADO `contribution` och GitHub mirror-branch matchar redan.
- **`--force-with-lease` stoppar push:** någon har uppdaterat mirror-branchen manuellt; undvik detta och kör om efter att branchen är “maskinägd”.
- **GitHub PR visar oväntade borttagningar:** kontrollera om filer faktiskt tagits bort i ADO `contribution` (speglingsflödet tar med deletions).
