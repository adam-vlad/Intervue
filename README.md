# Intervue - Aplicatie de simulare a interviurilor tehnice

Acesta este proiectul meu pentru lucrarea de licenta, realizat la FII UAIC, sub coordonarea domnului profesor Florin Olariu.

Aplicatia are scopul de a simula un interviu tehnic cu ajutorul unui model de limbaj (LLM). Utilizatorul incarca un CV in format PDF, modelul analizeaza informatiile (tehnologii, experienta), iar apoi poarta o conversatie interactiva pe baza acestora. La finalul interviului, se genereaza un raport detaliat de feedback.

Pentru a proteja datele personale, intregul sistem ruleaza 100% local, fara a folosi un API extern.

## Tehnologii utilizate
- **Backend**: C#, ASP.NET Core 10
- **Frontend**: React, TypeScript, Vite
- **Baza de date**: PostgreSQL si EF Core
- **AI**: Ollama (modelul Llama 3 rulat local)
- **Alte instrumente**: MediatR, FluentValidation, Docker, PdfPig pentru extragerea textului.

## Instructiuni de instalare

Sistemul necesita urmatoarele:
- .NET 10 SDK
- Docker Desktop (trebuie sa fie deschis)
- Node.js
- Memorie RAM: recomandat minim 16 GB pentru modelul LLM

1. **Clonarea proiectului si build-ul:**
   ```bash
   git clone https://github.com/adam-vlad/Intervue.git
   cd Intervue
   dotnet build Intervue.sln
   ```

2. **Pornirea containerelor Docker:**
   ```bash
   docker compose up -d
   ```

3. **Descarcarea modelului (este necesar doar la prima rulare):**
   ```bash
   docker exec intervue-ollama ollama pull llama3:8b-instruct-q4_0
   ```
   Acest proces poate dura cateva minute, in functie de conexiune.

4. **Pornirea backend-ului:**
   ```bash
   dotnet run --project src/Intervue.Api
   ```
   Baza de date va fi creata si migrata automat.

5. **Pornirea frontend-ului (intr-un terminal nou):**
   ```bash
   cd client
   npm install
   npm run dev
   ```

Interfata web poate fi accesata la `http://localhost:5173`, iar documentatia API-ului (Swagger) la `http://localhost:5000/swagger`.

Pentru instructiuni detaliate despre utilizare, consultati fisierul [USAGE.md](USAGE.md).
