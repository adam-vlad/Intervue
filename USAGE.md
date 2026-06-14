# Ghid de utilizare — Intervue

Acest document explică pas cu pas cum pornești aplicația și cum o folosești, de la instalare până la generarea raportului de feedback.

---

## 1. Ce trebuie instalat

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Node.js 20+](https://nodejs.org/)
- [Git](https://git-scm.com/downloads)
- Minim 16 GB RAM
- (Opțional) Placă video NVIDIA cu 6+ GB VRAM — face inferența LLM mult mai rapidă

### Verificare rapidă

Deschide un terminal și rulează:

```bash
dotnet --version    # trebuie să afișeze 10.x.x
docker --version    # trebuie să afișeze Docker version 2x.x.x
node --version      # trebuie să afișeze v20.x.x sau mai nou
```

Dacă oricare comandă lipsește, instalează componenta de la linkul corespunzător de mai sus.

---

## 2. Clonare și build

```bash
git clone https://github.com/adam-vlad/Intervue.git
cd Intervue

dotnet restore Intervue.sln
dotnet build Intervue.sln
```

Dacă la final apare `Build succeeded` cu 0 erori, ești gata să continui.

---

## 3. Pornirea serviciilor Docker

Docker Desktop trebuie să fie deschis — verifică icoana din system tray, lângă ceas.

```bash
docker compose up -d
```

Aceasta pornește trei containere:
- **Ollama** — serverul LLM (port `11434`)
- **PostgreSQL 17** — baza de date (port `5432`)
- **pgAdmin** — interfață web pentru baza de date (port `5050`)

### Descărcarea modelului LLM (doar prima dată, ~4.7 GB)

```bash
docker exec intervue-ollama ollama pull llama3:8b-instruct-q4_0
```

Durează între 5 și 15 minute în funcție de conexiunea la internet. Ca să verifici că s-a descărcat:

```bash
docker exec intervue-ollama ollama list
```

Trebuie să apară `llama3:8b-instruct-q4_0` în lista afișată.

---

## 4. Pornirea backend-ului

```bash
dotnet run --project src/Intervue.Api
```

La prima rulare, aplicația creează automat tabelele în PostgreSQL prin EF Core. Când vezi în consolă că serverul ascultă pe portul 5000, backend-ul e gata.

---

## 5. Pornirea frontend-ului

Deschide un terminal separat, în același folder:

```bash
cd client
npm install
npm run dev
```

Frontend-ul pornește pe `http://localhost:5173`.

---

## 6. Utilizarea aplicației

### Pasul 1 — Uploadează un CV

Navighează la `http://localhost:5173` și mergi la pagina **Upload**. Trage un fișier PDF în zona de upload sau selectează-l manual. Aplicația extrage textul din PDF și pornește parsarea cu LLM-ul.

Parsarea poate dura între 15 secunde (GPU) și 2-3 minute (CPU) — LLM-ul analizează tot textul și generează un profil structurat.

### Pasul 2 — Profilul CV

După parsare, ești redirecționat automat la pagina de profil. Aici poți vedea ce a extras LLM-ul: tehnologiile cu anii de experiență, experiența profesională, proiectele și nivelul de dificultate detectat automat (Junior, Mid sau Senior).

Nivelul e calculat din suma lunilor de experiență profesională, nu ales de tine.

### Pasul 3 — Pornirea interviului

De pe pagina de profil, apasă butonul de pornire a interviului. Un dialog îți cere să alegi limba — română sau engleză. Alegi și interviul pornește.

Prima întrebare vine direct de la AI, adaptată la stackul din CV-ul tău.

### Pasul 4 — Conversația

Scrie răspunsurile tale în câmpul de text și trimite. AI-ul răspunde în streaming, caracterele apar pe ecran pe măsură ce sunt generate. Poți vedea și dacă răspunsul conține cod — e afișat cu syntax highlighting.

Încearcă să dai răspunsuri cât mai complete, pentru că AI-ul pune follow-up-uri pe baza a ce ai spus. Un interviu bun are cel puțin 3-4 schimburi de mesaje.

### Pasul 5 — Generarea feedback-ului

Butonul de finalizare a interviului devine activ după minimum 3 mesaje din partea ta. Când ești gata, apasă-l. LLM-ul analizează toată conversația și generează raportul — poate dura 1-3 minute.

Raportul conține:
- Scor general (0–100)
- Scoruri pe categorii: cunoștințe tehnice, comunicare, problem solving, relevanța experienței
- Puncte tari
- Puncte slabe
- Sugestii concrete de îmbunătățire

Conținutul raportului e mereu în engleză, indiferent de limba aleasă pentru interviu — e generat de LLM și e mai consistent astfel.

### Pasul 6 — Dashboard

Pagina de Dashboard arată o privire de ansamblu: toate CV-urile uploadate, toate interviurile, și câteva statistici vizuale dacă ai mai multe sesiuni acumulate (distribuția nivelurilor, progresul scorurilor în timp, top tehnologii).

---

## 7. Oprirea aplicației

```bash
# Oprește containerele Docker
docker compose down

# Oprește backend-ul: Ctrl+C în terminalul unde rulează
# Oprește frontend-ul: Ctrl+C în terminalul unde rulează
```

Datele din PostgreSQL și modelul LLM rămân salvate pe disc în volumele Docker. La repornire nu pierzi nimic.

---

## 8. Comenzi utile

| Comandă | Ce face |
|---|---|
| `dotnet build Intervue.sln` | Compilează toată soluția |
| `dotnet test Intervue.sln` | Rulează toate testele |
| `dotnet run --project src/Intervue.Api` | Pornește backend-ul |
| `docker compose up -d` | Pornește containerele |
| `docker compose down` | Oprește containerele |
| `docker compose up -d --build api` | Rebuild și repornire container API |
| `docker compose logs -f ollama` | Log-uri Ollama în timp real |
| `docker exec intervue-ollama ollama list` | Modelele LLM descărcate |

---

## 9. Troubleshooting

**„Docker Desktop is not running"**
Deschide Docker Desktop din Start Menu și așteaptă 30-60 de secunde să se inițializeze complet.

**„Cannot connect to Ollama" / „Connection refused"**
Verifică dacă containerul rulează:
```bash
docker ps --filter name=intervue-ollama
```
Dacă nu apare, pornește-l manual: `docker compose up ollama -d`

**„Model not found"**
Modelul nu a fost descărcat sau download-ul a eșuat. Rulează din nou:
```bash
docker exec intervue-ollama ollama pull llama3:8b-instruct-q4_0
```

**Build dă erori**
Verifică că ai .NET 10 cu `dotnet --version`. Dacă versiunea e corectă, încearcă `dotnet restore Intervue.sln` înainte de build.

**Răspunsurile LLM sunt foarte lente**
E normal pe CPU — un răspuns complex poate dura 1-5 minute. Cu GPU NVIDIA durează 10-30 de secunde. Timeout-ul HTTP e setat la 10 minute, deci aplicația nu cade, doar așteaptă.

**Eroare 500 la generarea feedback-ului**
Uneori LLM-ul returnează un JSON în format neașteptat. Parser-ul acoperă majoritatea variațiilor, dar dacă tot eșuează, încearcă din nou — de obicei la a doua încercare merge.

**Frontend nu pornește / erori npm**
Asigură-te că ai Node.js 20+ cu `node --version`, apoi șterge `client/node_modules/` și rulează `npm install` din nou.

**Portul 5000 e ocupat**
Modifică portul în `src/Intervue.Api/Properties/launchSettings.json`.
