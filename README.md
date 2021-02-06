# Mabron.DiscordBots

## Games.Werwolf

### Levelsystem

```
l ... neues Level
e ... Erfahrung von alten bis neuen Level

e = 40 * x ^ 1.2 + 40 * 1.1 ^ (x ^ 0.5)
```

XP Belohnungen setzten sich wie folgt zusammen:

| Belohnung | XP |
|-|-|
| Spielleiter (kein Spielteilnehmer) | 100 |
| Überlebt | 160 |
| Gewonnen | 120 |
| Gruppenmodifikator | `(Anzahl Teilnehmer) * 0.15 - 0.15` |

**Durchschnittliche XP unter folgender Annahme:**

- 50% Siege
- 25% Überlebt
- 1 konstant Spielleiter
- Spielgruppenbonus wird nicht berücksichtigt

| Gruppe | XP |
|-|-|
| Spielleiter | 100 |
| Spieler | `120 * 0.5 + 160 * 0.25 = 60 + 40 = 100` |

**Gruppenmodifikator:**

Dazu wird folgende Formel zurate gezogen: `(Anzahl Teilnehmer) * 0.15 - 0.15`. Dadurch erhält man
folgende Multiplikatoren:

| Spieler | Multiplikator |
|-|-|
|  1 | 0,00 |
|  2 | 0,15 |
|  3 | 0,30 |
|  4 | 0,45 |
|  5 | 0,60 |
|  6 | 0,75 |
|  7 | 0,90 |
|  8 | 1,05 |
|  9 | 1,20 |
| 10 | 1,35 |
| 11 | 1,50 |
| 12 | 1,65 |
| 13 | 1,80 |
| 14 | 1,95 |
| 15 | 2,10 |
| 16 | 2,25 |
| 17 | 2,40 |
| 18 | 2,55 |

Erst bei einer sinnvollen Teilnehmerzahl von 8 bekommt man den vollen XP Bonus. Grinden wird dadurch
schwieriger gestaltet. 

Wenn man zwei gleich große Gruppen zusammenlegt und zusammen spielt, dann
ist der XP Bonus sogar leicht höher (Bsp.: 2x8 mit je 1,05 oder 1x16 mit 2,25). Das macht es
attraktiver in größeren Gruppen zu spielen (Erfahrungsgemäß kann ich sagen, dass eine große Gruppe
sogar schneller spielt, als wenn sie in kleinen Gruppen arbeitet).

Ein Spieler alleine bekommt keine XP. Dafür braucht man schon einen zweiten.

### Killsystem

- Leben hat jetzt mehrere Stufen:
    - `Alive` - ganz normal am Leben
    - `MarkedKill` - Je nach Quelle gibt es einen anderen Verursacher. Das Opfer ist nur als zu töten markiert. Dieser Zustand lässt sich rückgängig machen.
    - `AboutToKill` - Das Opfer wird vorbereitet zum töten. Hier werden die Verliebten zum töten markiert.
    - `BeforeKill` - Alle sehen das Opfer schon als tot. Theoretisch sieht man auch schon die Eigenschaften des Verstorbenen. Der Verstorbene sieht aber noch nicht der anderen. Hier werden noch letzte Aktionen ausgeführt.
    - `Killed` - Jetzt ist das Opfer wirklich tot und hat keine Handlung mehr. Der Killzustand wird gelöscht.
- Jemanden umzubringen durchläuft nun folgende Schritte:
    1. Der Zustand wird auf `MarkedKill` gesetzt und ein Zustandsobjekt wird gesetzt. Über diesen könnte man später Details erfragen.
    2. `MarkedKill` lässt sich rückgängig machen.
    3. `MarkedKill` wird zu `AboutToKill` gewechselt. In dieser Phase werden auch verkettete Tote abgerufen. Diese werden auch auf `AboutToKill` gesetzt.
    2. Falls möglich werden nun Benachrichtigungen rausgeschickt. Alle Spieler mit `AboutToKill` werden auf `BeforeKill` gesetzt.
    3. Die Spieler mit passender `BeforeKill` können nun ihre Handlung ausführen.
    4. Danach werden alle `BeforeKill` auf `Killed` gesetzt. Diese bekommen noch die Informationen der Lebenden zugeschickt.
- Kills müssen nur am Ende der Stages abgearbeitet werden und sind prinzipiell ähnlich aufgebaut. Von daher kann man solche Phasen in `PhaseBuilder` zusammenfassen.
