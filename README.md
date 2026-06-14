# Intervue

Aplicație web de simulare a interviurilor tehnice, personalizate pe baza CV-ului. Uploadezi un PDF, un model LLM local parsează CV-ul și generează un interviu adaptat profilului tău — tehnologii, nivel de experiență, proiecte. La final primești un raport de feedback cu scoruri și recomandări concrete.

Totul rulează local, fără niciun API extern. Datele nu ajung nicăieri în afara mașinii tale.

> Lucrare de licență — FII UAIC, coordonator: Florin Olariu

---

## Cum funcționează

1. Uploadezi un CV în format PDF
2. LLM-ul extrage tehnologiile, experiența profesională și proiectele, apoi determină automat nivelul de dificultate (Junior / Mid / Senior)
3. Alegi limba interviului (română sau engleză) și pornești conversația
4. AI-ul joacă rolul de intervievator — pune întrebări legate de stackul tău, follow-up-uri pe răspunsurile tale, adaptate nivelului detectat
5. Când termini, generezi raportul de feedback — scor general 0–100, scoruri pe categorii, puncte tari, puncte slabe și sugestii concrete

---

## Tech stack

| Componentă | Tehnologie |
|---|---|
| Backend | ASP.NET Core Web API, .NET 10 |
| Frontend | React 19, TypeScript, Vite |
| Bază de date | PostgreSQL 17 |
| ORM | Entity Framework Core 10 + Npgsql |
| LLM | Ollama + Llama 3 8B Q4 (rulează local în Docker) |
| Extragere PDF | PdfPig |
| Containerizare | Docker Compose |
| CQRS / Mediator | MediatR |
| Validare | FluentValidation |
| Teste | xUnit + Moq + FluentAssertions |

---

## Arhitectură

Proiectul urmează Clean Architecture, cu patru straturi și dependențe îndreptate spre interior:

```
Api  →  Application  →  Domain
            ↑
     Infrastructure
```

- **Domain** — entitățile, value objects, enums și regulile de business. Zero dependențe externe.
- **Application** — comenzile și query-urile (CQRS prin MediatR), validările FluentValidation, interfețele pentru servicii externe
- **Infrastructure** — implementările concrete: EF Core + PostgreSQL, client Ollama, PdfPig, hashing SHA-256
- **Api** — controllerele HTTP, versioning prin URL segment, Swagger

Pe lângă Clean Architecture, am aplicat câteva pattern-uri: Result\<T\> în loc de excepții propagate, DDD cu aggregate roots (CvProfile și Interview), și repository pattern cu AsNoTracking pe query-urile de citire.

---

## Cerințe de sistem

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Node.js 20+](https://nodejs.org/)
- Minim 16 GB RAM — modelul LLM are nevoie de memorie
- (Opțional) GPU NVIDIA cu 6+ GB VRAM, accelerează semnificativ inferența

---

## Instalare

### 1. Clonează repository-ul

```bash
git clone https://github.com/adam-vlad/Intervue.git
cd Intervue
```

### 2. Compilează backend-ul

```bash
dotnet restore Intervue.sln
dotnet build Intervue.sln
```

Trebuie să apară `Build succeeded` cu 0 erori.

### 3. Pornește serviciile Docker

Docker Desktop trebuie să fie deschis (verifică icoana din system tray).

```bash
docker compose up -d
```

Pornește trei containere: Ollama pentru LLM, PostgreSQL ca bază de date și pgAdmin pentru vizualizare.

### 4. Descarcă modelul LLM (doar prima dată, ~4.7 GB)

```bash
docker exec intervue-ollama ollama pull llama3:8b-instruct-q4_0
```

Durează câteva minute în funcție de conexiune.

### 5. Pornește backend-ul

```bash
dotnet run --project src/Intervue.Api
```

La prima rulare, EF Core aplică automat migrările și creează tabelele în PostgreSQL.

### 6. Pornește frontend-ul

```bash
cd client
npm install
npm run dev
```

---

## Accesare

| Serviciu | URL |
|---|---|
| Frontend | http://localhost:5173 |
| API | http://localhost:5000 |
| Swagger UI | http://localhost:5000/swagger |
| pgAdmin | http://localhost:5050 |

---

## Endpoint-uri API (v1)

Toate rutele folosesc versioning prin URL segment (`/api/v1/...`).

### CV

| Metodă | Endpoint | Descriere |
|---|---|---|
| `POST` | `/api/v1/cv/upload` | Uploadează un PDF și extrage textul |
| `POST` | `/api/v1/cv/parse` | Parsează CV-ul cu LLM-ul și salvează în baza de date |
| `GET` | `/api/v1/cv` | Returnează toate profilurile CV |
| `GET` | `/api/v1/cv/{id}` | Returnează un profil CV complet cu tehnologii, experiențe, proiecte |
| `GET` | `/api/v1/cv/{id}/interviews` | Returnează toate interviurile asociate unui CV |

### Interviuri

| Metodă | Endpoint | Descriere |
|---|---|---|
| `POST` | `/api/v1/interview/start` | Pornește un interviu nou pentru un CV |
| `POST` | `/api/v1/interview/message` | Trimite un mesaj și primește răspunsul complet |
| `POST` | `/api/v1/interview/feedback` | Generează raportul final de feedback |
| `GET` | `/api/v1/interview` | Returnează toate interviurile |
| `GET` | `/api/v1/interview/{id}` | Returnează un interviu complet cu istoricul de mesaje |
| `GET` | `/api/v1/interview/{id}/stream?content=...` | Streaming SSE al răspunsului AI, token cu token |

### Altele

| Metodă | Endpoint | Descriere |
|---|---|---|
| `GET` | `/health` | Verificare stare serviciu |

---

## Baza de date

Toate datele sunt persistate în PostgreSQL 17, gestionate prin EF Core cu două migrări aplicate automat la pornire.

| Tabel | Ce stochează |
|---|---|
| `cv_profiles` | CV-urile parsate — text brut, educație, nivel de dificultate, date personale hash-uite SHA-256 |
| `technologies` | Tehnologiile extrase din CV (nume, ani de experiență) |
| `experiences` | Experiența profesională (rol, companie, durată în luni) |
| `projects` | Proiectele (nume, descriere, tehnologii stocate ca `jsonb`) |
| `interviews` | Sesiunile de interviu (status, limbă, timestamps) |
| `interview_messages` | Mesajele din conversație (rol, conținut, timestamp) |
| `feedback_reports` | Rapoartele finale (scor general, scoruri pe categorii ca `jsonb`, puncte tari, puncte slabe, sugestii) |

**pgAdmin** este disponibil la `http://localhost:5050`. Pentru conectare la serverul PostgreSQL: host `postgres`, port `5432`, user `intervue`, parolă `intervue_dev`.

---

## Teste

```bash
dotnet test Intervue.sln
```

Aproximativ 160 de teste: unit tests pentru Domain, Handlers, Behaviors, Validators și sistemul de prompts, plus integration tests cu EF Core InMemory și LLM mockat.

---

## Structura proiectului

```
Intervue/
├── src/
│   ├── Intervue.Domain/            # Entități, value objects, enums, reguli de business
│   ├── Intervue.Application/       # CQRS (commands/queries), validări, DTOs, prompts
│   ├── Intervue.Infrastructure/    # EF Core, OllamaClient, PdfPig, SHA-256, repositories
│   └── Intervue.Api/               # Controllers, Swagger, configurare, versioning
├── tests/
│   ├── Intervue.UnitTests/
│   └── Intervue.IntegrationTests/
├── client/                         # Frontend React (TypeScript + Vite)
│   └── src/
│       ├── pages/                  # Home, Dashboard, Upload, Profile, Interview, Feedback
│       ├── components/             # Layout, Navbar, Sidebar, Spinner, MessageContent
│       ├── services/               # api.ts — tot contractul cu backend-ul într-un singur fișier
│       ├── i18n/                   # Traduceri EN și RO
│       └── styles/                 # Variabile CSS, reset, stiluri globale
└── docker-compose.yml
```

---

## Licență

Proiect academic - FII UAIC.
