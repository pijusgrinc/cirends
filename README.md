# Cirends

## 1. Tikslas

**Projekto tikslas** – sukurti planavimo sistemą, kuri palengvintų draugų ar bendraminčių grupių bendrų veiklų organizavimą, suteikiant galimybę patogiai planuoti veiklas, paskirstyti užduotis ir sekti finansus.

**Veikimo principas** – sistema sudaryta iš dviejų dalių:
- **Internetinė aplikacija**, kuria naudosis paprasti vartotojai ir administratorius;
- **API sąsaja**, užtikrinanti veiklų, užduočių ir išlaidų valdymą bei duomenų mainus tarp kliento ir serverio.

Naudotojas, prisijungęs prie aplikacijos, galės:
- kurti veiklas (pvz., „Kelionė prie ežero“),  
- priskirti užduotis (pvz., „Nupirkti maisto“, „Surinkti žmones“),  
- pridėti susijusias išlaidas (pvz., „Degalai – 50 €“).  

Sistema leis paskirstyti atsakomybę, matyti, kas ką padarė, ir kaip pasidalintos išlaidos.

---

## 1.1 Sistemos paskirtis

Sistemos paskirtis – palengvinti vartotojų bendravimą ir organizavimą, leidžiant jiems efektyviai:
- planuoti veiklas,  
- dalintis atsakomybėmis,  
- valdyti išlaidas.  

Sistema suteiks galimybę vartotojams bendrauti, dalintis informacija ir užtikrinti, kad visi dalyviai būtų informuoti apie planuojamas veiklas.

---

## 1.2 Funkciniai reikalavimai

### Neregistruotas naudotojas (Svečias) galės:
1. Peržiūrėti reprezentacinį puslapį apie projektą („Cirends“ aprašymas, funkcijos).  
2. Užsiregistruoti sistemoje.  
3. Prisijungti prie sistemos.  

### Registruotas naudotojas (Narys) galės:
1. Atsijungti nuo sistemos.  
2. Kurti naują veiklą (pvz., „Kelionė į kalnus“).  
3. Redaguoti savo sukurtą veiklą.  
4. Šalinti savo sukurtą veiklą.  
5. Peržiūrėti savo veiklų sąrašą.  
6. Pakviesti kitus naudotojus dalyvauti veikloje.  
7. Patvirtinti ar atmesti pakvietimą į veiklą.  
8. Pridėti užduotis prie veiklos (pvz., „Nupirkti maisto“).  
9. Priskirti atsakingą naudotoją prie užduoties.  
10. Nustatyti užduoties terminą.  
11. Keisti užduoties būseną (planuojama → vykdoma → atlikta).  
12. Peržiūrėti veiklai priskirtų užduočių sąrašą.  
13. Pridėti išlaidas prie veiklos ar užduoties (pvz., „Benzinas – 50 €“).  
14. Nurodyti, kas sumokėjo už išlaidą.  
15. Paskirstyti išlaidą tarp dalyvių (lygiai arba procentiškai).  
16. Peržiūrėti bendrą išlaidų suvestinę veikloje.  
17. Matyti individualią savo skolą ar permoką.  

### Administratorius galės:
1. Peržiūrėti visų sistemos naudotojų sąrašą.  
2. Šalinti naudotojus.  
3. Valdyti sistemos prieigos teises.  

---

## 2. Sistemos architektūra

- **Kliento pusė (Front-End):** HTML, CSS, Vue.js  
- **Serverio pusė (Back-End):** C# su .NET  
- **Duomenų bazė:** PostgreSQL
- **Debesų technologijos:** MonsterAPI (talpinimui)  

