---
title: "Erste Schritte mit Git"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

In [Git-Konzepte](/modules/git_konzepte/git_konzepte.md) haben wir gesehen, dass ein Repository ein Graph von Momentaufnahmen ist. In diesem Modul legen wir ein solches Repository für unser Adventure an und halten die ersten Versionen fest – ausschließlich auf der Kommandozeile. Das ist kein Selbstzweck: Die IDE-Schaltflächen, die wir in [Git in der IDE](/modules/git_in_der_ide/git_in_der_ide.md) kennenlernen, rufen genau diese Befehle auf, und wenn etwas schiefgeht, zeigen sie meist nur deren Fehlermeldung an. Wer die Befehle kennt, kann sie lesen.

## Installation und Konfiguration

Git gibt es für alle Betriebssysteme auf [git-scm.com](https://git-scm.com/). Unter macOS bringt es das Xcode-Kommandozeilenpaket mit, unter Linux der Paketmanager, unter Windows der Installer von git-scm.com (dort „Git Bash“ als Terminal mitinstallieren). Ob alles funktioniert, verrät ein Aufruf im Terminal:

```bash
git --version
# git version 2.51.0
```

Bevor der erste Commit entsteht, muss Git wissen, wer du bist – Name und E-Mail landen in jedem Commit, den du anlegst. Die Option `--global` speichert die Einstellung für alle Repositorys auf diesem Rechner:

```bash
git config --global user.name "Vorname Nachname"
git config --global user.email "vorname.nachname@student.htw-berlin.de"
```

## Ein Repository anlegen

Wir wechseln in den Ordner `Adventure`, der die Klassenbibliothek `Adventure.Kern` aus der letzten Vorlesung und das Konsolenprojekt `Adventure.Konsole` zum Spielen enthält, und machen daraus ein Repository. Die Weboberfläche, die Datenhaltung und die Tests kommen in späteren Vorlesungen als weitere Projekte daneben – für Git ändert das nichts. `git init` legt dabei nur den versteckten Ordner `.git/` an – die Dateien selbst bleiben unberührt und sind Git noch unbekannt:

```bash
cd Adventure
git init
# Initialized empty Git repository in /Users/.../Adventure/.git/
git status
# On branch main
# No commits yet
# Untracked files:
#   Adventure.Kern/
#   Adventure.Konsole/
```

`git status` ist der Befehl, den du am häufigsten tippen wirst. Er zeigt, in welchem Branch du bist, welche Dateien Git nicht kennt (*untracked*), welche geändert sind und welche im Staging-Bereich liegen. Bevor wir etwas hinzufügen, müssen wir aber eine Falle entschärfen.

## .gitignore: Was nicht ins Repository gehört

Wer die Projekte schon einmal gebaut hat, findet in jedem Projektordner die Ordner `bin/` und `obj/` mit kompilierten `.dll`-Dateien und Zwischenständen des Compilers, dazu eventuell einen Ordner `.vs/` oder `.idea/` mit IDE-Einstellungen. Diese Dateien werden bei jedem Build neu erzeugt, sind binär und unterscheiden sich von Rechner zu Rechner – sie haben im Repository nichts verloren. Eine Datei `.gitignore` im Wurzelordner sagt Git, welche Pfade es ignorieren soll. Für .NET-Projekte erzeugt das SDK eine fertige Vorlage:

```bash
dotnet new gitignore
# The template ".NET gitignore file" was created successfully.
head -n 12 .gitignore
# # Build results
# [Dd]ebug/
# [Rr]elease/
# x64/
# [Bb]in/
# [Oo]bj/
# ...
```

Die Vorlage deckt `bin/`, `obj/`, `.vs/`, Testergebnisse und Dutzende weitere Fälle ab. Ein `git status` zeigt jetzt nur noch Quelltexte und Projektdateien. Die `.gitignore` selbst wird mit committet, damit alle im Team dieselben Regeln haben.

Erst `.gitignore` anlegen, dann `git add`. Ist `bin/` einmal committet, hilft die `.gitignore` nicht mehr – Git verfolgt die Dateien weiter, bis man sie mit `git rm -r --cached bin` explizit aus dem Index entfernt. Und die alten Binärdateien bleiben für immer in der Historie.
{: .notice--warning}

## Der erste Commit: add und commit

Ein Commit entsteht in zwei Schritten. `git add` legt Änderungen in den Staging-Bereich, `git commit` macht daraus eine Version. Der Punkt bei `git add .` bedeutet „alles im aktuellen Ordner“ – abzüglich dessen, was `.gitignore` ausschließt:

```bash
git add .
git status
# On branch main
# Changes to be committed:
#   new file:   .gitignore
#   new file:   Adventure.Kern/Position.cs
#   new file:   Adventure.Kern/Spielfeld.cs
#   new file:   Adventure.Kern/Spielobjekt.cs
#   ...
#   new file:   Adventure.Konsole/Program.cs
git commit -m "Adventure mit Kern-Bibliothek und Konsolenprogramm anlegen"
# [main (root-commit) 7d2b0e4] Adventure mit Kern-Bibliothek und Konsolenprogramm anlegen
#  13 files changed, 743 insertions(+)
```

Die Ausgabe nennt den Branch, die ersten sieben Zeichen des Commit-Hashs und die Anzahl geänderter Dateien. Ab jetzt ist dieser Stand unveränderlich gespeichert. Zwei Dateien gehören in praktisch jedes Repository und sollten früh committet werden: eine `README.md`, die erklärt, was das Projekt ist und wie man es baut (`dotnet build`), und eine `LICENSE`, die festlegt, was andere mit dem Code tun dürfen – ohne Lizenz ist Code trotz Veröffentlichung nicht frei nutzbar. GitLab und GitHub bieten beim Anlegen eines Projekts Vorlagen für beides an.

## Der Zyklus: modified → staged → committed

Jede Datei in einem Repository befindet sich in einem von drei Zuständen, und die drei Befehle bewegen sie zwischen ihnen:

```
   Arbeitskopie              Staging-Bereich            Repository
   (modified)                (staged)                   (committed)
        │                         │                          │
        │──── git add Datei ─────►│                          │
        │                         │──── git commit ─────────►│
        │                         │                          │
        │◄─── git restore Datei ──┘ (--staged: aus dem Index)│
        │◄──────────────── git switch / neuer Stand ─────────┘
```

Wir ändern jetzt eine Datei – etwa eine neue Methode in `Spielfeld.cs`, mit der man ein eingesammeltes Objekt wieder vom Feld nehmen kann – und sehen uns den Weg einmal komplett an:

```bash
git status
# Changes not staged for commit:
#   modified:   Adventure.Kern/Spielfeld.cs
git diff
# +    public void Entfernen(Spielobjekt objekt)
# +    {
# +        if (objekt is StatischesObjekt s) statische.Remove(s.Position);
# +        if (objekt is Gegner g) gegner.Remove(g);
# +    }
git add Adventure.Kern/Spielfeld.cs
git diff --staged
# (zeigt jetzt dieselbe Änderung – sie liegt im Staging-Bereich)
git commit -m "Spielfeld: Objekte wieder vom Feld entfernen können"
```

`git diff` ohne Argument vergleicht die Arbeitskopie mit dem Staging-Bereich, `git diff --staged` den Staging-Bereich mit dem letzten Commit. Vor jedem Commit lohnt sich ein Blick auf `git diff --staged`: Es zeigt genau das, was gleich in die Historie wandert – und verrät vergessene `Console.WriteLine`-Debugausgaben.

## Historie lesen und Änderungen verwerfen

`git log` listet die Commits vom neuesten zum ältesten. Die Option `--oneline` reduziert jeden auf Hash und Nachricht:

```bash
git log --oneline
# a1f9c3e Spielfeld: Objekte wieder vom Feld entfernen können
# 7d2b0e4 Adventure mit Kern-Bibliothek und Konsolenprogramm anlegen
```

Manchmal will man eine Änderung nicht behalten. `git restore Datei` setzt die Arbeitskopie auf den Stand des Staging-Bereichs bzw. des letzten Commits zurück; `git restore --staged Datei` nimmt eine Datei nur aus dem Staging-Bereich heraus, lässt die Änderung in der Arbeitskopie aber stehen:

```bash
git restore --staged Adventure.Kern/Gegner.cs   # doch nicht in diesen Commit
git restore Adventure.Kern/Gegner.cs            # Änderung komplett verwerfen
```

`git restore` ohne `--staged` löscht deine ungespeicherten Änderungen unwiderruflich – Git hat davon keine Kopie, weil sie nie committet wurden. Im Zweifel erst committen, dann aufräumen: Ein überflüssiger Commit ist harmlos, verlorene Arbeit nicht.
{: .notice--warning}

## Stände benennen: Tags

Ein Hash wie `a1f9c3e` ist eindeutig, aber nichts, was man sich merkt. Deshalb kann man einzelne Commits mit einem **Tag** benennen – einem festen Namen für genau diese Momentaufnahme. Anders als ein Branch wandert ein Tag nie weiter; er markiert dauerhaft einen Stand, typischerweise eine veröffentlichte Version. Das vollständige Adventure findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure), und dort ist jeder Vorlesungsstand getaggt. Wenn du es dir mit `git clone https://github.com/erodner/prog2-adventure.git` holst (mehr dazu in [Remote-Repositorys](/modules/git_remote/git_remote.md)), findest du im Wesentlichen einen Commit pro Vorlesung und fünf Tags, die auf diese Stände zeigen:

```bash
git log --oneline
# c5f7d94 CI: dotnet build und test bei jedem Push
# e784378 Vorlesung 12: NUnit-Tests für Spielfeld, Datenhaltung und Inventar
# 6cdc213 Vorlesung 09: Level aus Textdateien, Spielstände als JSON, Level per HTTP
# f15b001 Vorlesung 04: Blazor-Oberfläche und Schichten (Web → Kern ← Daten)
# a3ff341 Vorlesung 02: abstrakte Klassen, Interfaces, Gegner, Türen, Truhen – spielbar in der Konsole
# b946807 Vorlesung 01: Spielobjekt, Wand und Spieler – ein Raum in der Konsole
git tag
# v01-vererbung
# v02-interfaces
# v04-blazor
# v09-daten
# v12-tests
```

Der oberste Commit fällt aus der Reihe: Er gehört zu keiner Vorlesung, sondern richtet eine automatische Prüfung ein, die bei jedem Push `dotnet build` und `dotnet test` laufen lässt (siehe [Branches und Merges](/modules/git_branching/git_branching.md)). `v12-tests` zeigt auf genau diesen obersten Commit und damit immer auf den aktuellen Endstand: Ein Tag bewegt sich nie von selbst, wohl aber, wenn man ihn mit `git tag -f` ausdrücklich neu setzt.

Mit `git checkout <tag>` legt Git die Arbeitskopie auf diesen Stand zurück – alle Dateien im Ordner sehen aus wie damals. Das ist genau der Weg, um den Code einer älteren Vorlesung nachzulesen:

```bash
git checkout v01-vererbung
# Note: switching to 'v01-vererbung'.
# You are in 'detached HEAD' state. ...
ls Adventure.Kern
# Adventure.Kern.csproj  Position.cs  Richtung.cs  Spieler.cs  Spielfeld.cs  Spielobjekt.cs  Wand.cs
git switch -                              # zurück zum vorherigen Branch
# Switched to branch 'main'
```

Der Hinweis *detached HEAD* bedeutet nur: Du stehst auf einem Commit, nicht auf einem Branch. Zum Ansehen ist das völlig in Ordnung; wer von hier aus weiterarbeiten will, legt mit `git switch -c name` erst einen Branch an. Einen eigenen Tag setzt man mit `git tag -a v1.0 -m "Erste spielbare Version"`, und weil `git push` Tags nicht automatisch mitnimmt, braucht es dafür ein `git push --tags`.

## Gute Commits

Ein Commit sollte **eine** zusammengehörige Änderung enthalten: ein Bugfix, ein kleines Feature, eine Umbenennung. Wer „Wachen-Bug gefixt, Fallen angefangen, Beschreibung umformuliert“ in einen Commit packt, kann später nichts davon einzeln ansehen oder rückgängig machen. Die Nachricht beschreibt im **Imperativ**, was der Commit tut, wie eine Anweisung an das Projekt: „Wache beim Anstoßen umdrehen lassen“, nicht „habe was an der Wache gemacht“. Die erste Zeile bleibt unter 70 Zeichen; braucht man mehr, folgt nach einer Leerzeile ein Absatz mit dem *Warum*.

Übung: Lege ein Repository für dein eigenes Adventure an (`dotnet new gitignore`, `git init` im Projektordner). Baue die Solution, prüfe mit `git status`, dass `bin/` und `obj/` nicht auftauchen, und mache drei Commits mit sinnvollen Nachrichten – zum Beispiel eine neue Gegenstandsart, ein weiteres Level und eine Korrektur an der Konsolenausgabe. Sieh dir mit `git log --oneline` und `git diff HEAD~1` an, was du festgehalten hast.
{: .notice--info}

## Weitere Quellen

- [Grundlagen – Pro Git (git-scm.com)](https://git-scm.com/book/de/v2/Git-Grundlagen-Ein-Git-Repository-anlegen)
- [Änderungen nachverfolgen und im Repository speichern – Pro Git (git-scm.com)](https://git-scm.com/book/de/v2/Git-Grundlagen-%C3%84nderungen-nachverfolgen-und-im-Repository-speichern)
- [dotnet new gitignore – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/tools/dotnet-new-sdk-templates#gitignore)
- [Oh Shit, Git!?! (deutsch)](https://ohshitgit.com/de) – kurze Rezeptsammlung für die Momente, in denen etwas schiefgegangen ist: falsche Commit-Nachricht, falscher Branch, versehentlich verworfene Änderung.
