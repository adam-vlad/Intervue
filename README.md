# MockInterview — AI Mock Interview Platform

Aplicație de mock interview personalizat pe CV, rulată integral local folosind un LLM (Ollama + Llama 3 8B).

> **Lucrare de licență** — FII UAIC, coordonator Florin Olariu

---

## Ce face aplicația

1. Utilizatorul uploadează un **CV (PDF)**
2. **PdfPig** extrage textul brut
3. **LLM-ul parsează** textul → JSON structurat (tehnologii, experiență, nivel, proiecte) — prin prompt engineering, fără regex
4. LLM-ul generează **întrebări personalizate** pe profil
5. **Conversație multi-turn**: utilizatorul răspunde, AI-ul dă follow-up questions
6. La final: **raport de feedback** (puncte tari, slabe, sugestii, scoruri pe categorii)

### Diferențiatori
- Personalizat pe CV-ul fiecărui candidat
- Totul rulează **local** (fără API-uri externe → confidențialitate)
- Gratuit și reproductibil prin Docker

---

## Stack tehnic

| Componentă | Tehnologie |
|---|---|
| Backend | ASP.NET Core Web API (C#), .NET 10 |
| Frontend | React / Next.js |
| PDF extraction | PdfPig (NuGet) |
| CV Parsing | LLM prin prompt engineering |
| LLM | Ollama + Llama 3 8B Q4 (local, în Docker) |
| Securitate | SHA-256 hashing pentru date personale |
| Containerizare | Docker Compose |
| Mediator/CQRS | MediatR |
| Validare | FluentValidation |
| Teste | xUnit + Moq + FluentAssertions |
| HTTP Resilience | HttpClient cu timeout 10 min (inferență LLM locală) |

---

## Arhitectură

**Clean Architecture + DDD + CQRS + Result Pattern**

```
Api → Infrastructure → Application → Domain
```

Domain nu cunoaște pe nimeni din exterior. Dependențele merg spre interior.

### Proiecte în soluție

```
MockInterview.sln
├── src/
│   ├── MockInterview.Domain/            # Entități, Value Objects, Enums, Guard, Repository interfaces
│   ├── MockInterview.Application/       # CQRS (Commands/Queries), Result<T>, MediatR behaviors, Interfaces
│   ├── MockInterview.Infrastructure/    # Implementări: OllamaClient, PdfPig, SHA-256, Repositories in-memory
│   └── MockInterview.Api/              # Controllers, Program.cs, Swagger
├── tests/
│   ├── MockInterview.UnitTests/        # xUnit + Moq + FluentAssertions
│   └── MockInterview.IntegrationTests/ # Microsoft.AspNetCore.Mvc.Testing
├── docker-compose.yml
└── README.md
```

---

## Cerințe de sistem

- **OS**: Windows 10/11 (testat), Linux, macOS
- **RAM**: minim 16 GB (recomandat 32 GB)
- **GPU**: NVIDIA cu minim 6 GB VRAM (opțional, accelerează LLM-ul)
- **.NET SDK 10**: https://dotnet.microsoft.com/download/dotnet/10.0
- **Docker Desktop**: https://www.docker.com/products/docker-desktop/
- **Visual Studio Code** + extensia **C# Dev Kit**
- **Git**: https://git-scm.com/downloads

---

## Setup pas cu pas (mașină nouă)

### 1. Clonează repository-ul

```bash
git clone https://github.com/<USERNAME>/MockInterview.git
cd MockInterview
```

### 2. Verifică .NET SDK

```bash
dotnet --version
# Trebuie să afișeze 10.x.x
```

Dacă nu ai .NET 10, descarcă-l de la: https://dotnet.microsoft.com/download/dotnet/10.0

### 3. Restaurează pachetele NuGet

```bash
dotnet restore MockInterview.sln
```

### 4. Verifică build-ul

```bash
dotnet build MockInterview.sln
```

Trebuie să vezi `Build succeeded` și 0 erori.

### 5. Pornește Ollama cu Docker Compose

Asigură-te că Docker Desktop este pornit (icoana Docker din system tray e activă).

```bash
docker compose up ollama -d
```

Aceasta pornește containerul Ollama pe portul `11434`.

### 6. Descarcă modelul LLM (~4.7 GB, se face o singură dată)

```bash
docker exec mockinterview-ollama ollama pull llama3:8b-instruct-q4_0
```

Verifică că modelul s-a descărcat:

```bash
docker exec mockinterview-ollama ollama list
```

Trebuie să vezi `llama3:8b-instruct-q4_0` în listă.

### 7. Rulează backend-ul (development)

```bash
dotnet run --project src/MockInterview.Api
```

API-ul pornește pe `http://localhost:5000`.
Swagger UI: `http://localhost:5000/swagger`

### 8. (Alternativ) Rulează totul cu Docker Compose

```bash
docker compose up --build -d
```

Aceasta pornește **ollama** + **backend** împreună.
- Backend: `http://localhost:5000`
- Ollama: `http://localhost:11434`

---

## Comenzi utile

| Comandă | Ce face |
|---|---|
| `dotnet build MockInterview.sln` | Compilează toată soluția |
| `dotnet test MockInterview.sln` | Rulează toate testele |
| `dotnet run --project src/MockInterview.Api` | Pornește backend-ul local |
| `docker compose up -d` | Pornește toate containerele |
| `docker compose down` | Oprește toate containerele |
| `docker compose logs -f ollama` | Vezi log-urile Ollama |
| `docker exec mockinterview-ollama ollama list` | Listează modelele descărcate |

---

## API Endpoints

| Metodă | Endpoint | Descriere |
|---|---|---|
| `POST` | `/api/cv/upload` | Uploadează PDF, extrage text |
| `POST` | `/api/cv/parse` | LLM parsează CV-ul |
| `POST` | `/api/interview/start` | Pornește mock interview |
| `POST` | `/api/interview/message` | Trimite răspuns, primește follow-up |
| `POST` | `/api/interview/feedback` | Generează raport final |
| `GET` | `/api/interview/{id}` | Istoricul unui interviu |

---

## Structura Domain (DDD)

### Aggregate Roots
- **CvProfile** — CV-ul parsat (tehnologii, experiență, proiecte)
- **Interview** — Sesiunea de mock interview (mesaje, feedback)

### Entități
- `Technology`, `Experience`, `Project`, `InterviewMessage`, `FeedbackReport`

### Value Objects
- `HashedPersonalData`, `InterviewScore`, `SkillLevel`

### Enums
- `InterviewStatus` (NotStarted → InProgress → Completed)
- `MessageRole` (Interviewer / Candidate)
- `DifficultyLevel` (Junior / Mid / Senior)

---

## Detalii de implementare

### Timeout HTTP — 10 minute
Comunicarea cu Ollama folosește un `HttpClient` cu timeout de **10 minute** (în loc de 100 secunde implicit). Inferența locală pe CPU poate dura 2-5 minute pentru prompturi complexe (parsare CV, generare feedback).

### Parsare robustă a răspunsurilor LLM
Răspunsurile JSON de la LLM (parsare CV, feedback) sunt procesate cu:
- Eliminare markdown code fences (` ```json ... ``` `)
- Normalizare `snake_case` → `camelCase` (ex: `overall_score` → `overallScore`)
- Deserializare case-insensitive cu `AllowTrailingCommas`
- Fallback defaults pentru câmpuri lipsă (scoruri pe categorii, text placeholder)

Acest lucru asigură că endpoint-urile funcționează indiferent de variațiile de format ale LLM-ului.

---

## Variabile de mediu

| Variabilă | Default | Descriere |
|---|---|---|
| `Ollama__BaseUrl` | `http://ollama:11434` | URL-ul Ollama (în Docker) |
| `Ollama__Model` | `llama3:8b-instruct-q4_0` | Modelul LLM folosit |
| `ASPNETCORE_ENVIRONMENT` | `Development` | Mediul ASP.NET |

Când rulezi local (fără Docker), setează `Ollama__BaseUrl=http://localhost:11434`.

---

## Licență

Proiect academic — FII UAIC.
