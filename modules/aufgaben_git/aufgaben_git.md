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

Programmieren lernt man nicht nur durch Codezeilen tippen – sondern auch durch **Nachdenken**. Die folgenden Aufgaben trainieren *Computational Thinking* rund um Versionsverwaltung: einen Commit-Graphen lesen, ein Feature in Schritte zerlegen, einen Konflikt systematisch auflösen und einen kaputten Repository-Zustand reparieren. Für die meisten Aufgaben brauchst du kein Terminal, nur Papier und die Module dieser Vorlesung. Nimm dir für jede Aufgabe Zeit, bevor du die Lösung aufklappst.

## Aufgabe 1 — Mustererkennung

Gegeben ist der folgende Ausschnitt aus `git log --oneline --graph --all` im Repository des Geometrieeditors:

```
* 9e1c4b7 (feature/sortieren) Figuren nach Fläche sortieren
* 2a7d0f3 IComparable<Figur> in Figur implementieren
| * c4e88a1 (HEAD -> main) README: Bauanleitung mit dotnet build ergänzen
| * 71b2d9e Kreis: Umfang mit 2·π·r berechnen
|/
* a1f9c3e Entfernen einer Figur in FigurenVerwaltung ergänzen
* 7d2b0e4 Geometrieeditor mit Fachkonzept und Konsolenprogramm anlegen
```

Beantworte ohne Terminal:
- Welcher Commit ist der gemeinsame Vorfahr von `main` und `feature/sortieren`?
- Wie viele Commits enthält `feature/sortieren`, die `main` nicht hat – und umgekehrt?
- Was passiert bei `git switch main` gefolgt von `git merge feature/sortieren`: Fast-Forward oder Merge-Commit? Wie viele Eltern hat der neueste Commit danach?
- Angenommen, `71b2d9e` hätte `Figur.cs` nicht angefasst, `2a7d0f3` aber schon. Kann es trotzdem einen Konflikt geben? Wo müsste er liegen?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Den Graphen lesen:**

Die Sterne sind Commits, die Linien Elternbeziehungen, oben ist das Neueste. Die Zeile `|/` zeigt, wo sich die beiden Zweige treffen: Beide Linien führen auf `a1f9c3e`. Das ist der **gemeinsame Vorfahr** (*merge base*). `7d2b0e4` ist zwar auch ein Vorfahr beider, aber nicht der jüngste.

**Schritt 2 — Zählen:**

`feature/sortieren` hat zwei Commits, die `main` nicht kennt: `2a7d0f3` und `9e1c4b7`. `main` hat zwei Commits, die dem Feature fehlen: `71b2d9e` und `c4e88a1`. Beide Zweige sind also seit `a1f9c3e` auseinandergelaufen (*diverged*).

**Schritt 3 — Merge vorhersagen:**

Ein Fast-Forward ist nur möglich, wenn der Zielbranch ein Vorfahr des hereinkommenden ist. `main` steht aber auf `c4e88a1`, das nicht in der Historie von `9e1c4b7` liegt. Git muss also einen **Merge-Commit** erzeugen, dessen Eltern `c4e88a1` und `9e1c4b7` sind – zwei Eltern. Der Graph danach:

```
*   f0a3e12 (HEAD -> main) Merge branch 'feature/sortieren'
|\
| * 9e1c4b7 (feature/sortieren) Figuren nach Fläche sortieren
| * 2a7d0f3 IComparable<Figur> in Figur implementieren
* | c4e88a1 README: Bauanleitung mit dotnet build ergänzen
* | 71b2d9e Kreis: Umfang mit 2·π·r berechnen
|/
* a1f9c3e Entfernen einer Figur in FigurenVerwaltung ergänzen
```

**Schritt 4 — Konfliktmöglichkeit:**

Ein Konflikt entsteht nur, wenn **beide** Seiten dieselben Zeilen einer Datei gegenüber `a1f9c3e` geändert haben. `Figur.cs` wurde nur vom Feature geändert – kein Konflikt dort. `Kreis.cs` nur von `main` – kein Konflikt. Bleibt `FigurenVerwaltung.cs`: Wenn `9e1c4b7` dort eine Methode `NachFlaecheSortiert()` direkt unter `GesamtFlaeche()` einfügt und `main` keine der Dateien angefasst hat, gibt es keinen Konflikt. Merge-Konflikte hängen also nie davon ab, *wie viele* Commits ein Zweig hat, sondern nur davon, *welche Zeilen* die beiden Seiten seit dem gemeinsamen Vorfahren geändert haben.

**Zentrale Erkenntnisse:**

- **Der gemeinsame Vorfahr ist der jüngste Commit, der von beiden Branch-Zeigern aus erreichbar ist** – nicht der Wurzel-Commit.
- **Fast-Forward oder Merge-Commit ist eine Frage der Topologie**, nicht des Inhalts: Liegt `main` auf der Linie des Features, wird vorgespult.
- **Konflikte sind eine Frage des Inhalts**, nicht der Topologie: Ein Merge-Commit kann konfliktfrei sein, und ein einziger Commit pro Seite reicht für einen Konflikt.

</details>

## Aufgabe 2 — Zerlegung

Du sollst im Geometrieeditor das Feature „Figuren nach Fläche sortieren“ umsetzen: Das Konsolenprogramm soll alle Figuren aufsteigend nach Fläche ausgeben. Betroffen sind die Klassenbibliothek (`Figur`, `FigurenVerwaltung`) und das Konsolenprogramm (`Geometrieeditor.Konsole/Program.cs`).

Zerlege das Feature in eine Folge von Commits auf einem Branch `feature/sortieren`:
- Wie viele Commits sind sinnvoll, und was enthält jeder?
- In welcher Reihenfolge – und warum ist die Reihenfolge nicht beliebig?
- Formuliere für jeden Commit eine Nachricht nach den Regeln aus [Erste Schritte mit Git](/modules/git_erste_schritte/git_erste_schritte.md).
- Welcher Commit darf **nicht** allein auf `main` landen, weil er den Build brechen würde?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Regeln für den Schnitt:**

Ein Commit soll eine zusammengehörige Änderung enthalten, und jeder Commit soll für sich bauen. Das zweite Kriterium bestimmt die Reihenfolge: `FigurenVerwaltung` darf erst sortieren, wenn `Figur` vergleichbar ist, und das Konsolenprogramm darf erst eine Methode aufrufen, die es schon gibt.

**Schritt 2 — Die Commit-Folge:**

```
git switch -c feature/sortieren

# Commit 1: Vergleichbarkeit im Fachkonzept
#   Figur.cs: IComparable<Figur> implementieren, CompareTo vergleicht Flaeche
git commit -m "Figur: IComparable<Figur> über die Fläche implementieren"

# Commit 2: Sortierte Sicht in der Verwaltung
#   FigurenVerwaltung.cs: IReadOnlyList<Figur> NachFlaecheSortiert()
git commit -m "FigurenVerwaltung: nach Fläche sortierte Liste bereitstellen"

# Commit 3: Ausgabe im Konsolenprogramm
#   Program.cs: NachFlaecheSortiert() aufrufen und jede Figur mit Beschreibung() ausgeben
git commit -m "Konsole: Figuren nach Fläche sortiert ausgeben"
```

Commit 1 ist in sich abgeschlossen – beide Projekte bauen, denn noch ruft niemand `CompareTo` auf. Commit 2 nutzt die Vergleichbarkeit für die sortierte Liste, Commit 3 hängt nur noch die Ausgabe an. Sobald der Geometrieeditor Unit-Tests hat (dazu mehr in [Vorlesung 12](/lectures/12/12.md)), gehört zu Commit 2 auch ein Test für `NachFlaecheSortiert()` mit Rechteck (6), Kreis (~3.14) und Dreieck (2) – wer testgetrieben arbeitet, committet ihn sogar **vor** der Implementierung; dann ist ein Commit mit rotem Test auf dem Feature-Branch akzeptabel, solange er vor dem Merge grün wird.

**Schritt 3 — Was nicht allein auf `main` darf:**

Commit 3 allein würde `main` brechen, weil `NachFlaecheSortiert()` dort nicht existiert. Genau das ist der Grund, warum man Features auf einem Branch entwickelt und als Ganzes per Merge Request integriert: Zwischenstände dürfen im Branch unvollständig sein, `main` bekommt nur das fertige Feature.

**Zentrale Designentscheidungen:**

- **Von innen nach außen committen:** Erst `Figur`, dann `FigurenVerwaltung`, dann das Programm, das beide benutzt – in Richtung der Abhängigkeiten. Wenn ab [Vorlesung 04](/lectures/04/04.md) eine Oberfläche als eigene Schicht dazukommt, bleibt die Regel dieselbe.
- **Jeder Commit baut:** Wer später mit `git log` oder `git blame` sucht, kann jeden Stand ausprobieren.
- **Imperativ und Kontext in der Nachricht:** „Figur: …“, „Konsole: …“ nennt den Bereich, das Verb sagt, was passiert.

</details>

## Aufgabe 3 — Algorithmenentwurf

Beim Merge von `feature/figur-validierung` in `main` bleibt `FigurenVerwaltung.cs` im folgenden Zustand zurück:

```csharp
public void Hinzufuegen(Figur figur)
{
<<<<<<< HEAD
    if (figuren.Any(f => f.Name.Equals(figur.Name, StringComparison.OrdinalIgnoreCase)))
    {
        throw new ArgumentException($"Es gibt bereits eine Figur mit dem Namen '{figur.Name}'.");
    }
=======
    ArgumentNullException.ThrowIfNull(figur);
    if (figuren.Any(f => f.Name == figur.Name))
    {
        throw new ArgumentException($"Name '{figur.Name}' ist bereits vergeben.");
    }
>>>>>>> feature/figur-validierung
    figuren.Add(figur);
}
```

`main` hat die Namensprüfung auf Groß-/Kleinschreibung-unabhängig umgestellt, das Feature hat eine `null`-Prüfung ergänzt und die Fehlermeldung umformuliert.

- Welche Zeilen stammen von welcher Seite, und was ist der gemeinsame Vorfahr?
- Entwirf die korrekt aufgelöste Methode. Welche Änderung beider Seiten muss erhalten bleiben, welche ist eine Geschmacksfrage?
- Nenne die vollständige Befehlsfolge vom Auflösen bis zum abgeschlossenen Merge, inklusive der Prüfung, dass nichts kaputt ist.
- Wie lautet der Weg zurück, falls du dich gegen den Merge entscheidest?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Seiten identifizieren:**

Zwischen `<<<<<<< HEAD` und `=======` steht `main` (ours), zwischen `=======` und `>>>>>>>` das Feature (theirs). Der gemeinsame Vorfahr ist die ursprüngliche Methode ohne `null`-Prüfung mit `f.Name == figur.Name` und der alten Meldung – genau der Stand von `FigurenVerwaltung.cs` im Repository. Beide Seiten haben dieselbe `if`-Zeile geändert, deshalb der Konflikt.

**Schritt 2 — Aufgelöste Datei:**

Die `null`-Prüfung ist fachlich notwendig und muss **vor** jedem Zugriff auf `figur.Name` stehen. Die `OrdinalIgnoreCase`-Änderung ist eine bewusste Verhaltensänderung von `main` und muss erhalten bleiben – wer sie beim Auflösen verliert, macht einen Bugfix stillschweigend rückgängig. Nur die Fehlermeldung ist Geschmackssache; hier die des Features:

```csharp
public void Hinzufuegen(Figur figur)
{
    ArgumentNullException.ThrowIfNull(figur);
    if (figuren.Any(f => f.Name.Equals(figur.Name, StringComparison.OrdinalIgnoreCase)))
    {
        throw new ArgumentException($"Name '{figur.Name}' ist bereits vergeben.");
    }
    figuren.Add(figur);
}
```

Weder „ours übernehmen“ noch „theirs übernehmen“ hätte dieses Ergebnis geliefert – das ist der Grund, warum ein Merge-Tool die Entscheidung nicht abnehmen kann.

**Schritt 3 — Befehlsfolge:**

```bash
git status                                   # zeigt "both modified: ...FigurenVerwaltung.cs"
# Datei im Editor wie oben bearbeiten, alle Marker entfernen
grep -rn "<<<<<<<\|>>>>>>>" --include=*.cs .  # keine Marker mehr übrig?
dotnet build                                 # kompiliert?
dotnet run --project Geometrieeditor.Konsole  # verhält sich Hinzufuegen wie erwartet?
git add Geometrieeditor.Fachkonzept/FigurenVerwaltung.cs
git commit                                   # vorgeschlagene Nachricht "Merge branch ..." übernehmen
git log --oneline --graph -5                 # Merge-Commit mit zwei Eltern sichtbar
```

Der beste Nachweis, dass die Auflösung die Änderung von `main` bewahrt hat, ist ein kurzer Versuch im Konsolenprogramm: `"kreis1"` und `"Kreis1"` müssen als Duplikat abgelehnt werden, `null` mit einer `ArgumentNullException`. Sobald der Geometrieeditor Unit-Tests hat ([Vorlesung 12](/lectures/12/12.md)), wird aus diesem Versuch ein Test, der bei jedem Merge automatisch läuft.

**Schritt 4 — Rückzug:**

```bash
git merge --abort
```

stellt den Zustand vor dem `git merge` wieder her; beide Branches bleiben unverändert. Das geht nur, solange der Merge-Commit noch nicht gemacht ist.

**Zentrale Designentscheidungen:**

- **Reihenfolge ist Semantik:** Die `null`-Prüfung zuerst, sonst wirft die Namensprüfung eine `NullReferenceException`.
- **Keine Seite darf stillschweigend verlieren:** Beide Änderungen hatten einen Grund; beim Auflösen wird zusammengeführt, nicht ausgewählt.
- **Bauen und Ausprobieren gehören zum Auflösen** – der Merge ist erst fertig, wenn `dotnet build` durchläuft und das Verhalten beider Seiten geprüft ist.

</details>

## Aufgabe 4 — Fehler finden

Eine Gruppe hat ihren Geometrieeditor – die Klassenbibliothek und ein Konsolenprogramm, das Figuren an einen Webdienst schickt – auf das GitLab gepusht. Ein Blick ins Repository zeigt:

```
$ git ls-files | head
Geometrieeditor.Fachkonzept/bin/Debug/net10.0/Geometrieeditor.Fachkonzept.dll
Geometrieeditor.Fachkonzept/obj/project.assets.json
Geometrieeditor.Konsole/bin/Debug/net10.0/Geometrieeditor.Konsole.dll
Geometrieeditor.Konsole/einstellungen.json
Geometrieeditor.Konsole/Program.cs
...
$ cat Geometrieeditor.Konsole/einstellungen.json
{ "FigurenDienst": { "Url": "https://api.example.org/figuren", "ApiKey": "sk-live-7f3a…" } }
$ ls -a | grep gitignore
$
```

Es gibt keine `.gitignore`, `bin/` und `obj/` sind committet, und in `einstellungen.json` liegt ein API-Schlüssel – seit drei Commits, das Repository ist für alle Studierenden des Kurses sichtbar.

- Welche drei Probleme siehst du, und welches ist das dringendste?
- Welche Befehle bringen das Repository in Ordnung? Reicht es, die Dateien zu löschen und zu committen?
- Warum gilt der API-Schlüssel als kompromittiert, obwohl man ihn aus der Datei entfernen kann?
- Wie hätte die Gruppe die Datei mit dem Schlüssel von Anfang an behandeln sollen?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Priorisieren:**

1. **Das Secret** – dringend, weil jede Person mit Lesezugriff es bereits kopiert haben kann.
2. **Build-Ausgaben im Repository** – lästig: Jeder Build ändert `.dll`-Dateien, jeder Commit enthält Binärmüll, jeder Merge produziert sinnlose Konflikte in `obj/`.
3. **Fehlende `.gitignore`** – die Ursache von Problem 2 und Voraussetzung dafür, dass es nicht wieder passiert.

**Schritt 2 — Das Secret behandeln:**

Zuerst den Schlüssel beim Dienst **zurückziehen** und einen neuen erzeugen. Alles andere ist zweitrangig, denn: Git ist ein Graph von Momentaufnahmen. Ein neuer Commit, der den Schlüssel aus `einstellungen.json` löscht, ändert nichts an den drei alten Commits – `git show HEAD~2:Geometrieeditor.Konsole/einstellungen.json` zeigt ihn weiterhin, und jeder Klon enthält die gesamte Historie. Man kann die Historie mit Spezialwerkzeugen umschreiben (`git filter-repo`) und dann per Force-Push ersetzen, aber Klone, die bereits existieren, erreicht man damit nicht. Deshalb gilt die Regel: **Ein einmal gepushtes Secret ist kompromittiert, Punkt.** Das Umschreiben der Historie ist Aufräumen, kein Ersatz für das Zurückziehen.

**Schritt 3 — Repository bereinigen:**

```bash
dotnet new gitignore                          # Vorlage für .NET anlegen
git rm -r --cached '**/bin' '**/obj'          # aus dem Index entfernen, Dateien auf der Platte bleiben
git rm --cached Geometrieeditor.Konsole/einstellungen.json
echo "einstellungen.json" >> .gitignore       # die Datei mit dem Secret nie wieder stagen
git status                                    # bin/, obj/ erscheinen jetzt als gelöscht, .gitignore als neu
git add .gitignore
git commit -m "Build-Ausgaben und einstellungen.json aus dem Repository entfernen, .gitignore ergänzen"
git push
```

`--cached` ist der entscheidende Schalter: Ohne ihn würde `git rm` die Dateien auch von der Festplatte löschen. Nach diesem Commit verfolgt Git die Ordner nicht mehr, und die `.gitignore` verhindert, dass sie beim nächsten `git add .` zurückkommen.

**Schritt 4 — Wie es von Anfang an hätte laufen sollen:**

Der Schlüssel gehört nicht in eine committete Datei. Übliche Wege: eine Datei `einstellungen.lokal.json`, die in der `.gitignore` steht, während eine committete `einstellungen.json` nur Platzhalter enthält; oder eine Umgebungsvariable, die das Programm mit `Environment.GetEnvironmentVariable("FIGURENDIENST_APIKEY")` liest; für .NET-Projekte zusätzlich `dotnet user-secrets` in der Entwicklung. In jedem Fall beschreibt die `README.md`, welche Werte man lokal setzen muss. Und die Reihenfolge beim Anlegen eines Repositorys lautet immer: `dotnet new gitignore`, **dann** `git add .`, und vor dem ersten Push einmal `git status` und `git diff --staged` lesen.

**Zentrale Designentscheidungen:**

- **Zuerst den Schaden begrenzen** (Schlüssel zurückziehen), dann aufräumen.
- **Löschen ist keine Löschung:** Die Historie bewahrt alles, was je committet wurde – das ist die Stärke von Git und hier sein Problem.
- **`.gitignore` vor dem ersten `git add`:** Was nie im Index war, muss nie mühsam wieder heraus.

</details>
