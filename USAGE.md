# Ghid de utilizare — Intervue

Acest document explică pas cu pas cum să pornești și să folosești aplicația, de la instalare până la generarea raportului de feedback. Am încercat să fie suficient de detaliat încât să poți urma pașii chiar dacă nu ai mai lucrat cu Docker sau .NET înainte.

---

## 1. Pregătirea mediului

### Ce trebuie instalat
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Visual Studio Code](https://code.visualstudio.com/) + extensia **C# Dev Kit**
- [Git](https://git-scm.com/downloads)
- Minim **16 GB RAM** (recomandat 32 GB)
- (Opțional) Placă video NVIDIA cu 6+ GB VRAM — face inferența LLM mult mai rapidă

### Verificare rapidă

Deschide un terminal (PowerShell sau CMD) și rulează:

```bash
dotnet --version       # trebuie să vezi 10.x.x
docker --version       # trebuie să vezi Docker version 2x.x.x
git --version          # trebuie să vezi git version 2.x.x
```

Dacă oricare comandă nu funcționează, instalează componenta lipsă de la linkurile de mai sus.

---

## 2. Clonare și build

```bash
# Clonează proiectul
git clone https://github.com/<USERNAME>/Intervue.git
cd Intervue

# Restaurează pachetele NuGet
dotnet restore Intervue.sln

# Compilează soluția
dotnet build Intervue.sln
```

Dacă vezi **Build succeeded** cu 0 erori, ești gata.

---

## 3. Pornirea serviciilor Docker

**Important:** Docker Desktop trebuie să fie deschis (verifică icoana din system tray, lângă ceas).

### Pasul 1 — Pornește toate containerele

```bash
docker compose up -d
```

Aceasta pornește 3 containere:
- **Ollama** — serverul LLM (port `11434`)
- **PostgreSQL 17** — baza de date (port `5432`)
- **pgAdmin** — interfață web pentru baza de date (port `5050`)

### Pasul 2 — Descarcă modelul LLM (doar prima dată, ~4.7 GB)

```bash
docker exec intervue-ollama ollama pull llama3:8b-instruct-q4_0
```

Durează 5-15 minute, depinde de conexiune. Verifici dacă s-a descărcat:

```bash
docker exec intervue-ollama ollama list
```

Trebuie să apară `llama3:8b-instruct-q4_0` în tabel.

### Pasul 3 — Pornește backend-ul

```bash
dotnet run --project src/Intervue.Api
```

La prima pornire, aplicația creează automat tabelele în PostgreSQL (migrarea `InitialCreate` se aplică prin `MigrateAsync()`).

---

## 4. Accesarea aplicației

| Serviciu | URL | Notă |
|---|---|---|
| Backend API | http://localhost:5000 | API-ul principal |
| Swagger UI | http://localhost:5000/swagger | Interfață vizuală pentru testare |
| pgAdmin | http://localhost:5050 | Browser-based database viewer |
| Ollama | http://localhost:11434 | Serverul LLM (nu trebuie accesat direct) |

### Swagger
Swagger este o pagină generată automat unde poți vedea toate endpoint-urile și le poți testa din browser, fără să scrii cod. E cel mai rapid mod de a testa API-ul.

### pgAdmin
La prima deschidere, pgAdmin cere un **master password** — pune ce vrei (eu am pus `admin`). Apoi adaugi serverul PostgreSQL:
- **Host**: `postgres` (nu `localhost` — containerele comunică între ele prin numele de serviciu)
- **Port**: `5432`
- **Username**: `intervue`
- **Password**: `intervue_dev`

După care navighezi la: **Servers → Intervue → Databases → intervue_db → Schemas → public → Tables** și poți vedea datele cu Query Tool (`Tools → Query Tool`, apoi `SELECT * FROM cv_profiles;`).

---

## 5. Fluxul complet al unui interviu

### Pasul 5.1 — Upload CV

Trimite un fișier PDF cu CV-ul:

```
POST /api/v1/cv/upload
Content-Type: multipart/form-data

Body: fișierul PDF
```

**Răspuns:** un `id` (GUID) — identificatorul CV-ului în sistem.

### Pasul 5.2 — Parsarea CV-ului

LLM-ul analizează textul și extrage datele structurate:

```
POST /api/v1/cv/parse
Content-Type: application/json

{ "cvProfileId": "id-ul primit la upload" }
```

**Răspuns:** un obiect JSON cu tehnologiile, experiența, proiectele și nivelul detectat. Datele sunt salvate în PostgreSQL.

**Notă:** Acest pas poate dura 15-30 secunde (pe GPU) sau 1-3 minute (pe CPU).

### Pasul 5.3 — Pornire interviu

```
POST /api/v1/interview/start
Content-Type: application/json

{ "cvProfileId": "id-ul CV-ului" }
```

**Răspuns:** interviul nou creat + prima întrebare de la AI, personalizată pe stackul din CV.

### Pasul 5.4 — Conversație (se repetă)

Trimiți răspunsul tău și primești o întrebare follow-up:

```
POST /api/v1/interview/message
Content-Type: application/json

{
  "interviewId": "id-ul interviului",
  "content": "Răspunsul tău aici..."
}
```

**Răspuns:** următoarea întrebare de la AI. Repetă de câte ori vrei (minim 3 pentru un feedback relevant).

### Pasul 5.5 — Generare feedback

După ce ai răspuns la câteva întrebări, ceri raportul final:

```
POST /api/v1/interview/feedback
Content-Type: application/json

{ "interviewId": "id-ul interviului" }
```

**Notă:** Poate dura 2-5 minute pe CPU — LLM-ul analizează toată conversația.

**Răspuns:**
- **Scor general** (0–100)
- **Scoruri pe categorii** (cunoștințe tehnice, comunicare, problem solving, relevanță experiență)
- **Puncte tari** — ce a fost bine
- **Puncte slabe** — unde se poate îmbunătăți
- **Sugestii** — recomandări concrete

### Pasul 5.6 — Vizualizare interviu

Poți revizui oricând un interviu complet (inclusiv dacă repornești aplicația — datele sunt în PostgreSQL):

```
GET /api/v1/interview/{id}
```

---

## 6. Oprirea serviciilor

```bash
# Oprește toate containerele Docker
docker compose down

# Oprește doar backend-ul: Ctrl+C în terminalul unde rulează
```

**Notă:** Datele din PostgreSQL și modelul LLM rămân pe disc (în volumele Docker `postgres_data` și `ollama_data`). La repornire nu se pierde nimic.

---

## 7. Troubleshooting

### „Docker Desktop is not running"
→ Deschide Docker Desktop din Start Menu și așteaptă 30-60 secunde să se inițializeze.

### „Cannot connect to Ollama" / „Connection refused"
→ Verifică dacă containerul rulează:
```bash
docker ps --filter name=intervue-ollama
```
Dacă nu apare, pornește-l: `docker compose up ollama -d`

### „Model not found"
→ Modelul nu a fost descărcat. Rulează:
```bash
docker exec intervue-ollama ollama pull llama3:8b-instruct-q4_0
```

### Build-ul dă erori
→ Verifică `dotnet --version` (trebuie 10.x.x)
→ Rulează `dotnet restore Intervue.sln`

### Răspunsurile de la LLM sunt foarte lente
→ Normal pe CPU — un răspuns complex poate dura 1-5 minute. Cu GPU NVIDIA durează 10-30 secunde.
→ HttpClient-ul are timeout de 10 minute, e suficient și pentru hardware modest.
→ În Docker Desktop: Settings → Resources → GPU — verifică că Docker are acces la GPU.

### Eroare la feedback (500)
→ Uneori LLM-ul returnează JSON într-un format neașteptat. Parser-ul tratează majoritatea variațiilor (markdown fences, snake_case, câmpuri lipsă), dar dacă tot dă eroare, dă retry — la următoarea generare de obicei merge altfel.

### Portul 5000 e ocupat
→ Oprește procesul care-l folosește sau modifică portul în `Properties/launchSettings.json`.

### pgAdmin nu vede tabelele
→ Tabelele se creează la prima pornire a API-ului (prin EF Core migration). Asigură-te că ai rulat `dotnet run --project src/Intervue.Api` cel puțin o dată.

---

## 8. Structura folderelor

```
Intervue/
├── src/
│   ├── Intervue.Domain/            # Entități, reguli de business, value objects
│   ├── Intervue.Application/       # Commands, queries, validări, DTOs
│   ├── Intervue.Infrastructure/    # EF Core, Ollama, PdfPig, SHA-256, repositories
│   └── Intervue.Api/              # Controllers HTTP, Swagger, configurare
├── tests/
│   ├── Intervue.UnitTests/        # Teste unitare (xUnit + Moq)
│   └── Intervue.IntegrationTests/ # Teste de integrare
├── docker-compose.yml             # Ollama + PostgreSQL + pgAdmin
├── README.md                      # Descriere proiect + setup
├── USAGE.md                       # Acest fișier
└── .gitignore
```
