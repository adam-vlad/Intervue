# Ghid de utilizare — Intervue

Acest document explică cum să folosești aplicația Intervue, de la instalare până la generarea raportului de feedback.

---

## 1. Pregătirea mediului

### Ce ai nevoie instalat
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Visual Studio Code](https://code.visualstudio.com/) + extensia **C# Dev Kit**
- [Git](https://git-scm.com/downloads)
- Minim **16 GB RAM** (recomandat 32 GB)
- (Opțional) Placă video NVIDIA cu 6+ GB VRAM pentru accelerarea LLM-ului

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

Dacă vezi **Build succeeded** cu 0 erori, totul e în regulă.

---

## 3. Pornirea serviciilor (Docker)

**Important:** Docker Desktop trebuie să fie pornit (vezi icoana Docker în system tray, lângă ceas).

### Pasul 1 — Pornește Ollama (serverul LLM)

```bash
docker compose up ollama -d
```

### Pasul 2 — Descarcă modelul (doar prima dată, ~4.7 GB)

```bash
docker exec intervue-ollama ollama pull llama3:8b-instruct-q4_0
```

Aștepți până se descarcă (poate dura 5-15 minute, depinde de internet). Verifici:

```bash
docker exec intervue-ollama ollama list
```

Trebuie să apară `llama3:8b-instruct-q4_0` în tabel.

### Pasul 3 — Pornește backend-ul

**Varianta A — local (pentru development):**

```bash
dotnet run --project src/Intervue.Api
```

**Varianta B — totul în Docker:**

```bash
docker compose up --build -d
```

---

## 4. Accesarea aplicației

| Serviciu | URL | Notă |
|---|---|---|
| Backend API (local) | http://localhost:5000 | Când rulezi cu `dotnet run` |
| Backend API (Docker) | http://localhost:5000 | Când rulezi cu `docker compose` |
| Swagger UI | http://localhost:5000/swagger | Interfață vizuală pentru testarea API-ului |
| Ollama | http://localhost:11434 | Serverul LLM (nu trebuie accesat direct) |

### Ce este Swagger?
Swagger este o pagină web generată automat unde poți vedea toate endpoint-urile API-ului și le poți testa direct din browser — fără să scrii cod.

---

## 5. Fluxul complet al unui interviu

### Pasul 5.1 — Upload CV

Trimite un fișier PDF cu CV-ul tău:

```
POST /api/v1/cv/upload
Content-Type: multipart/form-data

Body: fișierul PDF
```

**Răspuns:** primești un `id` (GUID) — acesta identifică CV-ul tău în sistem.

### Pasul 5.2 — Parsarea CV-ului

Trimite id-ul CV-ului pentru parsare:

```
POST /api/v1/cv/parse
Content-Type: application/json

{ "cvProfileId": "id-ul primit la upload" }
```

**Răspuns:** un obiect JSON cu tehnologiile, experiența, proiectele și nivelul detectat din CV.

### Pasul 5.3 — Pornire interviu

```
POST /api/v1/interview/start
Content-Type: application/json

{ "cvProfileId": "id-ul CV-ului" }
```

**Răspuns:** interviul creat + prima întrebare de la AI.

### Pasul 5.4 — Conversație (se repetă)

Trimite răspunsul tău și primești o întrebare follow-up:

```
POST /api/v1/interview/message
Content-Type: application/json

{
  "interviewId": "id-ul interviului",
  "content": "Răspunsul tău aici..."
}
```

**Răspuns:** următoarea întrebare de la AI. Repetă acest pas de câte ori dorești (minim 3 răspunsuri).

### Pasul 5.5 — Generare feedback

După minim 3 răspunsuri, poți cere raportul final:

```
POST /api/v1/interview/feedback
Content-Type: application/json

{ "interviewId": "id-ul interviului" }
```

**Notă:** Acest pas poate dura 2-5 minute pe CPU deoarece LLM-ul analizează toată conversația.

**Răspuns:** raport complet cu:
- **Scor general** (0–100)
- **Scoruri pe categorii** (cunoștințe tehnice, comunicare, rezolvare probleme, relevanță experiență)
- **Puncte tari** — ce ai făcut bine
- **Puncte slabe** — unde poți îmbunătăți
- **Sugestii** — recomandări concrete

Parser-ul de feedback este robust: tratează automat variații de format din LLM (markdown fences, snake_case, câmpuri lipsă).

### Pasul 5.6 — Vizualizare interviu

Poți revizui oricând un interviu complet:

```
GET /api/v1/interview/{id}
```

---

## 6. Oprirea serviciilor

```bash
# Oprește toate containerele Docker
docker compose down

# Sau oprește doar Ollama
docker compose stop ollama
```

**Notă:** Modelul LLM rămâne descărcat pe disc (în volumul Docker `ollama_data`). Nu trebuie descărcat din nou la repornire.

---

## 7. Troubleshooting (probleme frecvente)

### „Docker Desktop is not running"
→ Deschide Docker Desktop din Start Menu și așteaptă până se inițializează (30-60 secunde).

### „Cannot connect to Ollama" / „Connection refused"
→ Verifică dacă containerul Ollama rulează:
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
→ Verifică versiunea .NET: `dotnet --version` (trebuie 10.x.x)
→ Restaurează pachetele: `dotnet restore Intervue.sln`

### Răspunsurile de la LLM sunt foarte lente
→ Normal — pe CPU, un răspuns poate dura 1-5 minute pentru operații complexe (parsare CV, feedback). Cu GPU NVIDIA, durează 10-30 secunde.
→ HttpClient-ul are timeout de **10 minute**, suficient și pentru hardware modest.
→ Verifică că Docker Desktop are access la GPU: Settings → Resources → GPU.

### Feedback-ul dă eroare de parsare (500 — Feedback.ParseFailed)
→ Uneori LLM-ul returnează JSON într-un format ușor diferit. Parser-ul tratează automat:
  - Markdown code fences (`json ... `)
  - snake_case în loc de camelCase
  - Câmpuri lipsă (se completează cu defaults)
→ Dacă tot dă eroare, reîncearcă — LLM-ul poate genera alt format la următoarea cerere.

### Portul 5000 este ocupat
→ Oprește procesul care folosește portul sau modifică portul în `docker-compose.yml` (linia `ports`).

---

## 8. Structura folderelor

```
Intervue/
├── src/
│   ├── Intervue.Domain/            # Entități, reguli de business
│   ├── Intervue.Application/       # Comenzi, query-uri, validări
│   ├── Intervue.Infrastructure/    # Ollama, PdfPig, SHA-256
│   └── Intervue.Api/              # Controllere HTTP, Swagger
├── tests/
│   ├── Intervue.UnitTests/        # Teste unitare
│   └── Intervue.IntegrationTests/ # Teste de integrare
├── docker-compose.yml                   # Orchestrare containere
├── README.md                            # Descriere proiect + setup
├── USAGE.md                             # Acest fișier
└── .gitignore                           # Fișiere excluse din Git
```
