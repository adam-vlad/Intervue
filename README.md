# Intervue — AI Interview Coach

Platformă de simulare a interviurilor tehnice, personalizate pe baza CV-ului candidatului. Totul rulează local, fără niciun serviciu extern — confidențialitatea datelor e garantată.

> **Lucrare de licență** — FII UAIC, coordonator Florin Olariu

---

## Despre ce e vorba

Ideea e simplă: uploadezi un CV în format PDF, iar aplicația îl analizează automat cu un LLM local (Llama 3, prin Ollama). Pe baza informațiilor extrase (tehnologii, experiență, proiecte), se generează un interviu tehnic personalizat, cu întrebări adaptate profilului candidatului.

Fluxul complet arată cam așa:

1. Uploadezi un **CV (PDF)** → textul e extras cu PdfPig
2. LLM-ul **parsează** textul în date structurate (JSON) — prin prompt engineering, fără regex fragil
3. Se pornește un **interviu personalizat** — AI-ul pune o primă întrebare legată de stackul candidatului
4. Urmează o **conversație multi-turn**: răspunzi, AI-ul pune follow-up questions pe baza răspunsurilor tale
5. La final, se generează un **raport de feedback** cu scoruri pe categorii, puncte tari, puncte slabe și sugestii concrete

### De ce am făcut asta

- Interviurile tehnice necesită practică, dar nu ai mereu cu cine să exersezi
- Platformele existente fie sunt generice (aceleași întrebări pentru toți), fie costă bani
- Voiam ceva care să funcționeze complet offline, fără să trimită datele personale pe internet
- A fost un bun pretext să lucrez cu Clean Architecture, DDD, CQRS și integrare cu LLM-uri

---

## Stack tehnic

| Componentă | Tehnologie |
|---|---|
| Backend | ASP.NET Core Web API (C#), .NET 10 |
| Bază de date | PostgreSQL 17 (Docker) |
| ORM | Entity Framework Core 10 + Npgsql |
| PDF extraction | PdfPig (NuGet) |
| CV Parsing | LLM prin prompt engineering |
| LLM | Ollama + Llama 3 8B Q4 (local, în Docker) |
| Securitate | SHA-256 hashing pentru date personale |
| Containerizare | Docker Compose (Ollama + PostgreSQL + pgAdmin) |
| Mediator/CQRS | MediatR |
| Validare | FluentValidation |
| API Versioning | Asp.Versioning.Mvc (URL segment) |
| Teste | xUnit + Moq + FluentAssertions |
| DB Viewer | pgAdmin 4 |

---

## Arhitectură

Proiectul urmează **Clean Architecture**, cu separare clară între layere. Dependențele merg spre interior — Domain nu știe nimic despre restul lumii.

```
Api → Infrastructure → Application → Domain
```

Pe scurt:
- **Domain** — entitățile, value objects, enums, regulile de business. Zero dependențe externe.
- **Application** — comenzile (CQRS prin MediatR), validările (FluentValidation), interfețele de repository
- **Infrastructure** — implementările concrete: EF Core + PostgreSQL, OllamaClient, PdfPig, SHA-256
- **Api** — controllerele HTTP, Swagger, configurarea aplicației

Am folosit și câteva pattern-uri care mi s-au părut utile:
- **Result\<T\>** în loc de excepții — fiecare operație returnează succes sau eroare, cu cod și mesaj
- **CQRS** — separarea clară între comenzi (modifică stare) și query-uri (citesc)
- **DDD** — aggregate roots (CvProfile, Interview), entități, value objects, Guards

### Structura soluției

```
Intervue.sln
├── src/
│   ├── Intervue.Domain/            # Entități, Value Objects, Enums, Guards, interfețe Repository
│   ├── Intervue.Application/       # Commands/Queries (CQRS), Result<T>, MediatR behaviors, DTOs
│   ├── Intervue.Infrastructure/    # EF Core (PostgreSQL), OllamaClient, PdfPig, SHA-256, Repositories
│   └── Intervue.Api/              # Controllers, Program.cs, Swagger, API versioning
├── tests/
│   ├── Intervue.UnitTests/        # xUnit + Moq + FluentAssertions
│   └── Intervue.IntegrationTests/ # Microsoft.AspNetCore.Mvc.Testing
├── docker-compose.yml             # Ollama + PostgreSQL + pgAdmin
└── README.md
```

---

## Cerințe de sistem

- **OS**: Windows 10/11 (testat), Linux, macOS
- **RAM**: minim 16 GB (recomandat 32 GB)
- **GPU**: NVIDIA cu minim 6 GB VRAM (opțional — accelerează inferența LLM)
- **.NET SDK 10**: https://dotnet.microsoft.com/download/dotnet/10.0
- **Docker Desktop**: https://www.docker.com/products/docker-desktop/
- **Visual Studio Code** + extensia **C# Dev Kit**
- **Git**: https://git-scm.com/downloads

---

## Setup pas cu pas

### 1. Clonează repository-ul

```bash
git clone https://github.com/<USERNAME>/Intervue.git
cd Intervue
```

### 2. Verifică .NET SDK

```bash
dotnet --version
# Trebuie să afișeze 10.x.x
```

### 3. Restaurează pachetele și compilează

```bash
dotnet restore Intervue.sln
dotnet build Intervue.sln
```

Trebuie să vezi `Build succeeded` cu 0 erori.

### 4. Pornește serviciile Docker

Docker Desktop trebuie să fie deschis (icoana din system tray).

```bash
# Pornește Ollama + PostgreSQL + pgAdmin
docker compose up -d
```

Containerele pornite:
- **Ollama** — serverul LLM, pe portul `11434`
- **PostgreSQL 17** — baza de date, pe portul `5432`
- **pgAdmin** — interfață web pentru vizualizarea bazei de date, pe portul `5050`

### 5. Descarcă modelul LLM (doar prima dată, ~4.7 GB)

```bash
docker exec intervue-ollama ollama pull llama3:8b-instruct-q4_0
```

Verifică descărcarea:

```bash
docker exec intervue-ollama ollama list
```

### 6. Pornește backend-ul

```bash
dotnet run --project src/Intervue.Api
```

La prima pornire, EF Core creează automat tabelele în PostgreSQL (prin migrarea `InitialCreate`).

- API: `http://localhost:5000`
- Swagger UI: `http://localhost:5000/swagger`
- pgAdmin: `http://localhost:5050` (login: `admin@intervue.dev` / `admin`)

---

## Comenzi utile

| Comandă | Ce face |
|---|---|
| `dotnet build Intervue.sln` | Compilează toată soluția |
| `dotnet test Intervue.sln` | Rulează toate testele |
| `dotnet run --project src/Intervue.Api` | Pornește backend-ul local |
| `docker compose up -d` | Pornește toate containerele (Ollama + PG + pgAdmin) |
| `docker compose down` | Oprește toate containerele |
| `docker compose logs -f ollama` | Log-uri Ollama în timp real |
| `docker exec intervue-ollama ollama list` | Modelele LLM descărcate |

---

## API Endpoints (v1)

Toate rutele folosesc versioning prin URL segment (`/api/v1/...`).

| Metodă | Endpoint | Descriere |
|---|---|---|
| `POST` | `/api/v1/cv/upload` | Uploadează un PDF, extrage textul |
| `POST` | `/api/v1/cv/parse` | LLM-ul parsează CV-ul în date structurate |
| `POST` | `/api/v1/interview/start` | Pornește un interviu nou |
| `POST` | `/api/v1/interview/message` | Trimite un răspuns, primește follow-up |
| `POST` | `/api/v1/interview/feedback` | Generează raportul final de feedback |
| `GET`  | `/api/v1/interview/{id}` | Returnează istoricul complet al unui interviu |

---

## Baza de date

Datele sunt persistate în PostgreSQL 17, prin Entity Framework Core cu provider-ul Npgsql.

### Tabele

| Tabel | Ce stochează |
|---|---|
| `cv_profiles` | CV-urile parsate (text brut, educație, nivel, date personale hash-uite) |
| `technologies` | Tehnologiile extrase din CV (nume, ani experiență) |
| `experiences` | Experiențele profesionale (rol, companie, durată) |
| `projects` | Proiectele personale (nume, descriere, tehnologii ca `jsonb`) |
| `interviews` | Sesiunile de interviu (status, timestamps, FK spre CV) |
| `interview_messages` | Mesajele din conversație (rol, conținut, timestamp) |
| `feedback_reports` | Rapoartele de feedback (scor overall, scoruri pe categorii ca `jsonb`, puncte tari/slabe) |

Câmpurile complexe (`CategoryScores`, `TechnologiesUsed`) sunt stocate ca `jsonb` — nu am creat tabele separate pentru ele fiindcă sunt date care se citesc mereu împreună cu entitatea părinte.

### pgAdmin

pgAdmin rulează pe `http://localhost:5050`. La prima deschidere setezi un master password (eu am pus `admin`). După care adaugi serverul:
- **Host**: `postgres`
- **Port**: `5432`
- **Username**: `intervue`
- **Password**: `intervue_dev`

### Migrări EF Core

Migrarea `InitialCreate` se aplică automat la pornirea API-ului (prin `MigrateAsync()` în `Program.cs`). Dacă vrei să adaugi migrări noi:

```bash
dotnet ef migrations add <NumeMigrare> --project src/Intervue.Infrastructure --startup-project src/Intervue.Api
```

---

## Structura Domain (DDD)

### Aggregate Roots
- **CvProfile** — CV-ul parsat (conține liste de Technologies, Experiences, Projects)
- **Interview** — Sesiunea de interviu (conține Messages + FeedbackReport)

### Entități
- `Technology`, `Experience`, `Project`, `InterviewMessage`, `FeedbackReport`

### Value Objects
- `HashedPersonalData` — datele personale hash-uite cu SHA-256
- `InterviewScore` — un scor pe o categorie (ex: "Technical Knowledge: 85")
- `SkillLevel` — nivelul de competență pe o tehnologie

### Enums
- `InterviewStatus` (NotStarted → InProgress → Completed)
- `MessageRole` (Interviewer / Candidate)
- `DifficultyLevel` (Junior / Mid / Senior)

---

## Detalii de implementare

### Timeout HTTP — 10 minute
`HttpClient`-ul care comunică cu Ollama are timeout de **10 minute**. Inferența pe CPU poate dura 2-5 minute pentru prompturi mari (parsare CV, feedback). Pe GPU durează sub un minut.

### Parsare robustă a răspunsurilor LLM
JSON-ul returnat de LLM trece prin mai multe etape de normalizare:
- Se elimină markdown code fences (` ```json ... ``` `)
- Se convertește `snake_case` → `camelCase` (LLM-ul nu e consistent cu formatul)
- Deserializare case-insensitive cu `AllowTrailingCommas`
- Fallback defaults dacă lipsesc câmpuri

Cam singurul lucru care poate da 500 e dacă LLM-ul returnează ceva complet în afara schemei — dar se întâmplă rar, iar la retry de obicei merge.

### API Versioning
Rutele sunt versionate prin URL segment (`/api/v1/...`), configurat cu `Asp.Versioning.Mvc`. Când/dacă voi adăuga endpointuri noi cu breaking changes, vor fi pe `/api/v2/...` fără să afecteze clienții existenți.

---

## Variabile de mediu

| Variabilă | Default | Descriere |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | (vezi appsettings.json) | Connection string PostgreSQL |
| `Ollama__BaseUrl` | `http://localhost:11434` | URL-ul serverului Ollama |
| `Ollama__Model` | `llama3:8b-instruct-q4_0` | Modelul LLM folosit |
| `ASPNETCORE_ENVIRONMENT` | `Development` | Mediul ASP.NET Core |

---

## Licență

Proiect academic — FII UAIC.
