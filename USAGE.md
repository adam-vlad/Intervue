# Ghid de utilizare

Acest fisier prezinta fluxul de utilizare a aplicatiei. Asigurati-va ca toate serviciile sunt pornite urmand pasii din fisierul README.

## Cum functioneaza aplicatia

1. **Incarcarea CV-ului**
   Accesati `localhost:5173` si incarcati fisierul PDF cu CV-ul. Aplicatia va extrage textul si va folosi modelul de limbaj pentru a identifica tehnologiile cunoscute si nivelul de experienta. Acest proces de analiza poate dura pana la cateva minute, in functie de performanta procesorului.

2. **Profilul candidatului**
   Dupa finalizarea analizei, veti fi redirectionat catre profilul generat. Aici puteti revizui datele extrase din CV inainte de a incepe interviul.

3. **Sesiunea de interviu**
   Porniti interviul si selectati limba dorita (romana sau engleza). Modelul AI va adresa intrebari tehnice adaptate profilului extras anterior. Raspunsurile primite sunt generate in timp real (streaming).
   Pentru ca raportul final sa fie cat mai precis, este recomandat sa raspundeti cat mai serios si sa mentineti conversatia timp de cel putin 3 sau 4 schimburi de mesaje.

4. **Raportul de feedback**
   Interviul poate fi incheiat manual in orice moment. Sistemul va analiza intreaga discutie si va genera un raport cu un scor general, scoruri detaliate pe categorii, puncte forte si recomandari de imbunatatire. Pentru a mentine consistenta evaluarilor, acest raport este generat intotdeauna in limba engleza.

## Posibile probleme si rezolvari

- In cazul unei erori de conexiune cu Ollama, verificati daca Docker functioneaza si containerul `intervue-ollama` este pornit.
- Daca API-ul intampina o eroare referitoare la portul 5000 ocupat, puteti modifica portul in fisierul `launchSettings.json`.
- Baza de date poate fi inspectata prin pgAdmin la `localhost:5050`, folosind contul `intervue` si parola `intervue_dev`.

## Oprirea serviciilor

Pentru a inchide aplicatia, opriti procesele din terminale cu comanda `Ctrl+C`. Containerele pot fi oprite folosind:
```bash
docker compose down
```
Nu veti pierde istoricul din baza de date sau modelul LLM descarcat.
