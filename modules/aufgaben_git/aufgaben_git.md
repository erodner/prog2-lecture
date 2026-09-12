---
title: "🧩 Aufgaben und Beispiele: Git"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Programmieren lernt man nicht nur durch Codezeilen tippen – sondern auch durch **Nachdenken**. Die folgenden Aufgaben trainieren *Computational Thinking* rund um Versionsverwaltung: einen Commit-Graphen lesen, ein Feature in Schritte zerlegen, einen Konflikt systematisch auflösen und einen kaputten Repository-Zustand reparieren. Als Material dient durchgehend unser Adventure; das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v02-interfaces`). Für die meisten Aufgaben brauchst du kein Terminal, nur Papier und die Module dieser Vorlesung. Nimm dir für jede Aufgabe Zeit, bevor du die Lösung aufklappst.

## Aufgabe 1 — Mustererkennung

Gegeben ist der folgende Ausschnitt aus `git log --oneline --graph --all` im Repository einer Gruppe:

```
* 9e1c4b7 (feature/bogenschuetze) Konsole: Legende um den Bogenschützen ergänzen
* 2a7d0f3 Bogenschütze als Gegner mit Fernkampf ergänzen
| * c4e88a1 (HEAD -> main) README: Steuerung und Spielregeln beschreiben
| * 71b2d9e Wache: beim Anstoßen umdrehen statt stehen bleiben
|/
* a1f9c3e Spielfeld: Objekte wieder vom Feld entfernen können
* 7d2b0e4 Adventure mit Kern-Bibliothek und Konsolenprogramm anlegen
```

Beantworte ohne Terminal:
- Welcher Commit ist der gemeinsame Vorfahr von `main` und `feature/bogenschuetze`?
- Wie viele Commits enthält `feature/bogenschuetze`, die `main` nicht hat – und umgekehrt?
- Was passiert bei `git switch main` gefolgt von `git merge feature/bogenschuetze`: Fast-Forward oder Merge-Commit? Wie viele Eltern hat der neueste Commit danach?
- `71b2d9e` ändert `Wache.NaechsterZug`, und `2a7d0f3` fügt die neue Klasse `Bogenschuetze` hinzu. Beide Klassen stehen in derselben Datei `Adventure.Kern/Gegner.cs`. Gibt es deshalb zwangsläufig einen Konflikt?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Den Graphen lesen:**

Die Sterne sind Commits, die Linien Elternbeziehungen, oben ist das Neueste. Die Zeile `|/` zeigt, wo sich die beiden Zweige treffen: Beide Linien führen auf `a1f9c3e`. Das ist der **gemeinsame Vorfahr** (*merge base*). `7d2b0e4` ist zwar auch ein Vorfahr beider, aber nicht der jüngste.

**Schritt 2 — Zählen:**

`feature/bogenschuetze` hat zwei Commits, die `main` nicht kennt: `2a7d0f3` und `9e1c4b7`. `main` hat zwei Commits, die dem Feature fehlen: `71b2d9e` und `c4e88a1`. Beide Zweige sind also seit `a1f9c3e` auseinandergelaufen (*diverged*).

**Schritt 3 — Merge vorhersagen:**

Ein Fast-Forward ist nur möglich, wenn der Zielbranch ein Vorfahr des hereinkommenden ist. `main` steht aber auf `c4e88a1`, das nicht in der Historie von `9e1c4b7` liegt. Git muss also einen **Merge-Commit** erzeugen, dessen Eltern `c4e88a1` und `9e1c4b7` sind – zwei Eltern. Der Graph danach:

```
*   f0a3e12 (HEAD -> main) Merge branch 'feature/bogenschuetze'
|\
| * 9e1c4b7 (feature/bogenschuetze) Konsole: Legende um den Bogenschützen ergänzen
| * 2a7d0f3 Bogenschütze als Gegner mit Fernkampf ergänzen
* | c4e88a1 README: Steuerung und Spielregeln beschreiben
* | 71b2d9e Wache: beim Anstoßen umdrehen statt stehen bleiben
|/
* a1f9c3e Spielfeld: Objekte wieder vom Feld entfernen können
```

**Schritt 4 — Konfliktmöglichkeit:**

Dieselbe Datei reicht nicht: Ein Konflikt entsteht nur, wenn **beide** Seiten dieselben oder unmittelbar benachbarte Zeilen gegenüber `a1f9c3e` geändert haben. `71b2d9e` arbeitet in der Methode `NaechsterZug` der Klasse `Wache`, `2a7d0f3` hängt eine neue Klasse `Bogenschuetze` ans Dateiende – die Bereiche überlappen nicht, Git führt `Gegner.cs` automatisch zusammen und meldet nur `Auto-merging Adventure.Kern/Gegner.cs`. Anders sähe es aus, wenn der Bogenschütze eine gemeinsame Hilfsmethode direkt neben `NaechsterZug` eingefügt hätte.

**Zentrale Erkenntnisse:**

- **Der gemeinsame Vorfahr ist der jüngste Commit, der von beiden Branch-Zeigern aus erreichbar ist** – nicht der Wurzel-Commit.
- **Fast-Forward oder Merge-Commit ist eine Frage der Topologie**, nicht des Inhalts: Liegt `main` auf der Linie des Features, wird vorgespult.
- **Konflikte sind eine Frage der Zeilen**, nicht der Dateien und nicht der Topologie: Ein Merge-Commit kann konfliktfrei sein, und ein einziger Commit pro Seite reicht für einen Konflikt.

</details>

## Aufgabe 2 — Zerlegung

Du sollst im Adventure eine neue Gegnerart umsetzen: den **Bogenschützen**. Er bleibt stehen, wo er ist, schießt aber, sobald der Spieler in gerader Linie ohne Wände dazwischen vor ihm steht – ein Treffer kostet einen Lebenspunkt. Betroffen sind `Adventure.Kern/Gegner.cs` (die neue Klasse), `Adventure.Kern/Spielfeld.cs` (der Rundenablauf), `Adventure.Kern/LevelParser.cs` (das Kartenzeichen) und `Adventure.Konsole/Program.cs` (die Legende).

Zerlege das Feature in eine Folge von Commits auf einem Branch `feature/bogenschuetze`:
- Wie viele Commits sind sinnvoll, und was enthält jeder?
- In welcher Reihenfolge – und warum ist die Reihenfolge nicht beliebig?
- Formuliere für jeden Commit eine Nachricht nach den Regeln aus [Erste Schritte mit Git](/modules/git_erste_schritte/git_erste_schritte.md).
- Welcher Commit darf **nicht** allein auf `main` landen, weil er den Build brechen würde?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Regeln für den Schnitt:**

Ein Commit soll eine zusammengehörige Änderung enthalten, und jeder Commit soll für sich bauen. Das zweite Kriterium bestimmt die Reihenfolge: Das `Spielfeld` darf erst schießen lassen, wenn es die Klasse `Bogenschuetze` gibt, und der `LevelParser` darf erst ein `B` auf sie abbilden, wenn beides existiert. Wir committen also **in Richtung der Abhängigkeiten**, von der Spiellogik nach außen zur Konsole.

**Schritt 2 — Die Commit-Folge:**

```
git switch -c feature/bogenschuetze

# Commit 1: die neue Gegnerart selbst
#   Gegner.cs: sealed class Bogenschuetze : Gegner, Symbol 'B',
#   NaechsterZug gibt null zurück (er bleibt stehen),
#   bool KannSchiessen(Spielfeld feld) nutzt HatSichtlinie und gleiche Zeile/Spalte
git commit -m "Bogenschütze als Gegner mit Sichtprüfung ergänzen"

# Commit 2: Wirkung im Rundenablauf
#   Spielfeld.cs: in GegnerZiehen die Bogenschützen schießen lassen,
#   Treffer über Spieler.SchadenNehmen() und Meldung anhängen
git commit -m "Spielfeld: Bogenschützen in jeder Runde schießen lassen"

# Commit 3: Bogenschützen in Karten platzieren
#   LevelParser.cs: 'B' => new Bogenschuetze(pos), Kommentar mit der Zeichenlegende
git commit -m "LevelParser: Zeichen B auf den Bogenschützen abbilden"

# Commit 4: Anzeige
#   Program.cs: Legende um 'B' ergänzen
git commit -m "Konsole: Legende um den Bogenschützen ergänzen"
```

Commit 1 ist in sich abgeschlossen – beide Projekte bauen, denn noch erzeugt niemand einen `Bogenschuetze`. Commit 2 gibt ihm Wirkung, Commit 3 macht ihn über Leveldateien erreichbar, Commit 4 erklärt ihn den Spielenden. Sobald das Adventure Unit-Tests hat (dazu mehr in [Vorlesung 12](/lectures/12/12.md)), gehört zu Commit 1 auch ein Test für `KannSchiessen` – Spieler in derselben Zeile mit freier Bahn, derselben Zeile mit Wand dazwischen, diagonal – wer testgetrieben arbeitet, committet ihn sogar **vor** der Implementierung; dann ist ein Commit mit rotem Test auf dem Feature-Branch akzeptabel, solange er vor dem Merge grün wird.

**Schritt 3 — Was nicht allein auf `main` darf:**

Die Commits 2, 3 und 4 allein würden `main` brechen, weil `Bogenschuetze` dort nicht existiert – am deutlichsten Commit 3, der den Konstruktor aufruft. Genau das ist der Grund, warum man Features auf einem Branch entwickelt und als Ganzes per Merge Request integriert: Zwischenstände dürfen im Branch unvollständig sein, `main` bekommt nur das fertige Feature.

**Zentrale Designentscheidungen:**

- **Von innen nach außen committen:** Erst die Klasse in `Adventure.Kern`, dann die Regeln, dann Parser und Konsole – in Richtung der Abhängigkeiten. Wenn ab [Vorlesung 04](/lectures/04/04.md) eine Weboberfläche als eigene Schicht dazukommt, bleibt die Regel dieselbe.
- **Jeder Commit baut:** Wer später mit `git log` oder `git blame` sucht, kann jeden Stand ausprobieren.
- **Imperativ und Kontext in der Nachricht:** „Spielfeld: …“, „Konsole: …“ nennt den Bereich, das Verb sagt, was passiert.

</details>

## Aufgabe 3 — Algorithmenentwurf

Zwei Personen haben am Trank gearbeitet. Auf `main` ist aufgefallen, dass die Meldung lügt, sobald der Spieler fast volle Lebenspunkte hat: `Spieler.Heilen` begrenzt die Heilung auf `MaxLebenspunkte`, die Meldung nennt aber trotzdem den vollen Wert. Im Branch `feature/staerkerer-trank` wurde die Standardheilung von 1 auf 2 erhöht und die Meldung neu formuliert. Beim Merge bleibt `Adventure.Kern/Gegenstand.cs` im folgenden Zustand zurück:

```csharp
public override string Aufheben(Spieler spieler)
{
<<<<<<< HEAD
    int vorher = spieler.Lebenspunkte;
    spieler.Heilen(Heilung);
    return $"{spieler.Name} trinkt einen Trank (+{spieler.Lebenspunkte - vorher}).";
=======
    spieler.Heilen(Heilung);
    return $"{spieler.Name} trinkt einen Trank und fühlt sich deutlich besser.";
>>>>>>> feature/staerkerer-trank
}
```

Die geänderte Voreinstellung `int heilung = 2` im Konstruktor von `Trank` hat Git dagegen ohne Nachfrage übernommen – sie steht ein paar Zeilen weiter oben.

- Welche Zeilen stammen von welcher Seite, und wie sah der gemeinsame Vorfahr aus?
- Entwirf die korrekt aufgelöste Methode. Welche Änderung beider Seiten muss erhalten bleiben, welche ist eine Geschmacksfrage?
- Nenne die vollständige Befehlsfolge vom Auflösen bis zum abgeschlossenen Merge, inklusive der Prüfung, dass nichts kaputt ist.
- Wie lautet der Weg zurück, falls du dich gegen den Merge entscheidest?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Seiten identifizieren:**

Zwischen `<<<<<<< HEAD` und `=======` steht `main` (ours), zwischen `=======` und `>>>>>>>` das Feature (theirs). Der gemeinsame Vorfahr ist die ursprüngliche Methode aus dem Repository:

```csharp
public override string Aufheben(Spieler spieler)
{
    spieler.Heilen(Heilung);
    return $"{spieler.Name} trinkt einen Trank (+{Heilung}).";
}
```

Beide Seiten haben genau diese zwei Zeilen angefasst – deshalb der Konflikt. Dass die Konstruktoränderung konfliktfrei durchging, zeigt noch einmal: Es zählen die Zeilen, nicht die Datei.

**Schritt 2 — Aufgelöste Datei:**

Die Messung der tatsächlich geheilten Punkte ist ein Bugfix und muss erhalten bleiben; sie wird durch die stärkere Voreinstellung sogar wichtiger, denn mit `heilung = 2` weicht die Meldung noch häufiger vom Versprochenen ab. Die Formulierung der Meldung ist Geschmackssache – hier die des Features, aber mit dem echten Wert. Die Reihenfolge ist dabei nicht verhandelbar: `vorher` muss **vor** `Heilen` gelesen werden:

```csharp
public override string Aufheben(Spieler spieler)
{
    int vorher = spieler.Lebenspunkte;
    spieler.Heilen(Heilung);
    int geheilt = spieler.Lebenspunkte - vorher;
    return geheilt > 0
        ? $"{spieler.Name} trinkt einen Trank und fühlt sich besser (+{geheilt})."
        : $"{spieler.Name} trinkt einen Trank – aber es tut sich nichts.";
}
```

Weder „ours übernehmen“ noch „theirs übernehmen“ hätte dieses Ergebnis geliefert – das ist der Grund, warum ein Merge-Tool die Entscheidung nicht abnehmen kann.

**Schritt 3 — Befehlsfolge:**

```bash
git status                                       # zeigt "both modified: Adventure.Kern/Gegenstand.cs"
# Datei im Editor wie oben bearbeiten, alle Marker entfernen
grep -rn "<<<<<<<\|>>>>>>>" --include=*.cs .     # keine Marker mehr übrig?
dotnet build                                     # kompiliert?
dotnet run --project Adventure.Konsole           # mit vollen Lebenspunkten über einen Trank laufen
git add Adventure.Kern/Gegenstand.cs
git commit                                       # vorgeschlagene Nachricht "Merge branch ..." übernehmen
git log --oneline --graph -5                     # Merge-Commit mit zwei Eltern sichtbar
```

Der beste Nachweis, dass die Auflösung den Bugfix bewahrt hat, ist ein kurzer Versuch im Spiel: Mit 3 von 3 Lebenspunkten über einen Trank laufen – die Meldung darf kein Plus versprechen, das der Spieler nie bekommen hat. Sobald das Adventure Unit-Tests hat ([Vorlesung 12](/lectures/12/12.md)), wird aus diesem Versuch ein Test, der bei jedem Merge automatisch läuft.

**Schritt 4 — Rückzug:**

```bash
git merge --abort
```

stellt den Zustand vor dem `git merge` wieder her; beide Branches bleiben unverändert. Das geht nur, solange der Merge-Commit noch nicht gemacht ist.

**Zentrale Designentscheidungen:**

- **Reihenfolge ist Semantik:** `vorher` vor `Heilen` – danach ist die Information unwiederbringlich weg.
- **Keine Seite darf stillschweigend verlieren:** Beide Änderungen hatten einen Grund; beim Auflösen wird zusammengeführt, nicht ausgewählt.
- **Bauen und Ausprobieren gehören zum Auflösen** – der Merge ist erst fertig, wenn `dotnet build` durchläuft und das Verhalten beider Seiten geprüft ist.

</details>

## Aufgabe 4 — Fehler finden

Eine Gruppe hat ihr Adventure – die Klassenbibliothek und das Konsolenprogramm, das Level von einem Webdienst nachlädt – auf das GitLab gepusht. Ein Blick ins Repository zeigt:

```
$ git ls-files | head
Adventure.Kern/bin/Debug/net10.0/Adventure.Kern.dll
Adventure.Kern/obj/project.assets.json
Adventure.Kern/Spielfeld.cs
Adventure.Konsole/bin/Debug/net10.0/Adventure.Konsole.dll
Adventure.Konsole/levelserver.json
Adventure.Konsole/Program.cs
spielstand.json
...
$ cat Adventure.Konsole/levelserver.json
{ "LevelDienst": { "Url": "https://api.example.org/levels", "ApiKey": "sk-live-7f3a…" } }
$ ls -a | grep gitignore
$
```

Es gibt keine `.gitignore`, `bin/` und `obj/` sind committet, die beim Spielen erzeugte Datei `spielstand.json` ebenfalls – und in `levelserver.json` liegt ein API-Schlüssel, seit drei Commits, das Repository ist für alle Studierenden des Kurses sichtbar.

- Welche Probleme siehst du, und welches ist das dringendste?
- Warum ist ein committeter Spielstand nicht nur unschön, sondern eine Konfliktquelle?
- Welche Befehle bringen das Repository in Ordnung? Reicht es, die Dateien zu löschen und zu committen?
- Warum gilt der API-Schlüssel als kompromittiert, obwohl man ihn aus der Datei entfernen kann? Wie hätte die Gruppe ihn von Anfang an behandeln sollen?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Priorisieren:**

1. **Das Secret** – dringend, weil jede Person mit Lesezugriff es bereits kopiert haben kann.
2. **Erzeugte Dateien im Repository** – lästig: Jeder Build ändert `.dll`-Dateien, und `spielstand.json` ändert sich bei *jeder gespielten Runde*. Wer spielt, hat danach eine geänderte Datei im `git status`, committet sie versehentlich mit, und beim nächsten Merge streiten sich zwei Spielstände um dieselben Zeilen – ein Konflikt, der niemanden interessiert und den man nicht sinnvoll auflösen kann. Verfolgt werden sollen nur Dateien, die Menschen schreiben.
3. **Fehlende `.gitignore`** – die Ursache von Problem 2 und Voraussetzung dafür, dass es nicht wieder passiert.

**Schritt 2 — Das Secret behandeln:**

Zuerst den Schlüssel beim Dienst **zurückziehen** und einen neuen erzeugen. Alles andere ist zweitrangig, denn: Git ist ein Graph von Momentaufnahmen. Ein neuer Commit, der den Schlüssel aus `levelserver.json` löscht, ändert nichts an den drei alten Commits – `git show HEAD~2:Adventure.Konsole/levelserver.json` zeigt ihn weiterhin, und jeder Klon enthält die gesamte Historie. Man kann die Historie mit Spezialwerkzeugen umschreiben (`git filter-repo`) und dann per Force-Push ersetzen, aber Klone, die bereits existieren, erreicht man damit nicht. Deshalb gilt die Regel: **Ein einmal gepushtes Secret ist kompromittiert, Punkt.** Das Umschreiben der Historie ist Aufräumen, kein Ersatz für das Zurückziehen.

**Schritt 3 — Repository bereinigen:**

```bash
dotnet new gitignore                          # Vorlage für .NET anlegen
git rm -r --cached '**/bin' '**/obj'          # aus dem Index entfernen, Dateien auf der Platte bleiben
git rm --cached Adventure.Konsole/levelserver.json spielstand.json
printf 'levelserver.json\nspielstand*.json\n' >> .gitignore
git status                                    # bin/, obj/ erscheinen jetzt als gelöscht, .gitignore als neu
git add .gitignore
git commit -m "Build-Ausgaben, Spielstand und levelserver.json aus dem Repository entfernen"
git push
```

`--cached` ist der entscheidende Schalter: Ohne ihn würde `git rm` die Dateien auch von der Festplatte löschen – und damit den Spielstand der Gruppe. Nach diesem Commit verfolgt Git die Ordner nicht mehr, und die `.gitignore` verhindert, dass sie beim nächsten `git add .` zurückkommen. Genau deshalb endet die `.gitignore` im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) mit zwei zusätzlichen Zeilen unter der Standardvorlage:

```
# Spielstände
spielstand*.json
```

**Schritt 4 — Wie es von Anfang an hätte laufen sollen:**

Der Schlüssel gehört nicht in eine committete Datei. Übliche Wege: eine Datei `levelserver.lokal.json`, die in der `.gitignore` steht, während eine committete `levelserver.json` nur Platzhalter enthält; oder eine Umgebungsvariable, die das Programm mit `Environment.GetEnvironmentVariable("LEVELDIENST_APIKEY")` liest; für .NET-Projekte zusätzlich `dotnet user-secrets` in der Entwicklung. In jedem Fall beschreibt die `README.md`, welche Werte man lokal setzen muss. Und die Reihenfolge beim Anlegen eines Repositorys lautet immer: `dotnet new gitignore`, **dann** `git add .`, und vor dem ersten Push einmal `git status` und `git diff --staged` lesen.

**Zentrale Designentscheidungen:**

- **Zuerst den Schaden begrenzen** (Schlüssel zurückziehen), dann aufräumen.
- **Nur Quellen gehören ins Repository:** Alles, was ein Programm erzeugt – Builds, Logs, Spielstände –, wird beim nächsten Lauf ohnehin neu geschrieben.
- **Löschen ist keine Löschung:** Die Historie bewahrt alles, was je committet wurde – das ist die Stärke von Git und hier sein Problem.
- **`.gitignore` vor dem ersten `git add`:** Was nie im Index war, muss nie mühsam wieder heraus.

</details>
