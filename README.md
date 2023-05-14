# Vizualizácia vývoja softvéru
Matúš Adamička, Dominik Mikuška
## Spustenie projektu
Pre správne spustenie je potrebné stiahnuť unity 2021.3.12f1.

## Dáta
Dáta sú dostupné tu: https://github.com/ReliSA/IMiGEr/tree/master/examples/SPADe%20json

Popis dát dostupný tu: https://github.com/ReliSA/IMiGEr/blob/master/examples/SPADe%20json/navrh.json

My sme si dáta predspracovali pomocou skriptu parser.py. Ten je možné nájsť v priečinku Preprocessing.

## Popis projektu

### Kto je zákazník
Zákazníkmi budú hlavný vývojár, líder-vedúci tímu, projektový manažér

### Čo sa má vizualizovať
Cieľom je vizualizovať dáta z manažmentu a procesu vývoja softvérových projektov, pričom snaha je vizualizovať:
evidenciu úloh vrátane k nim prislúchajúcich artefaktov
vzťahy autorov a commit-ov a časový vývoj práce na projekte

### Problémy, ktoré chce zákazník vizuálnou analýzou vyriešiť
- Chce vizuálne preskúmať rozloženie úloh v čase vzhľadom na typ úloh a identifikovať problematické miesta v procese realizácie úloh.
- Chce analyzovať vzťahy medzi členmi tímu a úlohami a zistiť, ako efektívne členovia vypracovali pridelené úlohy.
- Chce vizuálne preskúmať konkrétne časové intervaly aby rozpoznal opakujúce sa vzory správania počas vývoja (vid. poznámka nižšie)
- Chce detailne porovnať priebeh práce naprieč viacerými projektami a nájsť zaujímavé spoločné-odlišné črty.
- Chce vizuálne porovnať výkon členov tímu, nájsť najvýkonnejšieho a najmenej výkonného člena tímu.

### Vlastné scenáre
- Analyzovať role členov tímu na základe ich aktivít na projekte, resp. overenie naplnenia ich zodpovedností,
- Vizuálne preskúmať problémové oblasti z hľadiska implementácie, resp. analyzovať repozitáre a súbory a prácu s nimi.
