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

In [Git-Konzepte](/modules/git_konzepte/git_konzepte.md) haben wir gesehen, dass ein Repository ein Graph von Momentaufnahmen ist. In diesem Modul legen wir ein solches Repository für den Geometrieeditor an und halten die ersten Versionen fest – ausschließlich auf der Kommandozeile. Das ist kein Selbstzweck: Die IDE-Schaltflächen, die wir in [Git in der IDE](/modules/git_in_der_ide/git_in_der_ide.md) kennenlernen, rufen genau diese Befehle auf, und wenn etwas schiefgeht, zeigen sie meist nur deren Fehlermeldung an. Wer die Befehle kennt, kann sie lesen.

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

Wir wechseln in den Ordner der Solution und machen daraus ein Repository. `git init` legt dabei nur den versteckten Ordner `.git/` an – die Dateien selbst bleiben unberührt und sind Git noch unbekannt:

```bash
cd Geometrieeditor
git init
# Initialized empty Git repository in /Users/.../Geometrieeditor/.git/
git status
# On branch main
# No commits yet
# Untracked files:
#   Geometrieeditor.slnx
#   Geometrieeditor.Fachkonzept/
#   Geometrieeditor.Datenhaltung/
#   ...
```

`git status` ist der Befehl, den du am häufigsten tippen wirst. Er zeigt, in welchem Branch du bist, welche Dateien Git nicht kennt (*untracked*), welche geändert sind und welche im Staging-Bereich liegen. Bevor wir etwas hinzufügen, müssen wir aber eine Falle entschärfen.

## .gitignore: Was nicht ins Repository gehört

Wer die Solution schon einmal gebaut hat, findet in jedem Projekt die Ordner `bin/` und `obj/` mit kompilierten `.dll`-Dateien und Zwischenständen des Compilers, dazu eventuell einen Ordner `.vs/` oder `.idea/` mit IDE-Einstellungen. Diese Dateien werden bei jedem Build neu erzeugt, sind binär und unterscheiden sich von Rechner zu Rechner – sie haben im Repository nichts verloren. Eine Datei `.gitignore` im Wurzelordner sagt Git, welche Pfade es ignorieren soll. Für .NET-Projekte erzeugt das SDK eine fertige Vorlage:

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
#   new file:   Geometrieeditor.slnx
#   new file:   Geometrieeditor.Fachkonzept/Figur.cs
#   ...
git commit -m "Geometrieeditor als Solution mit vier Projekten anlegen"
# [main (root-commit) 7d2b0e4] Geometrieeditor als Solution mit vier Projekten anlegen
#  21 files changed, 612 insertions(+)
```

Die Ausgabe nennt den Branch, die ersten sieben Zeichen des Commit-Hashs und die Anzahl geänderter Dateien. Ab jetzt ist dieser Stand unveränderlich gespeichert. Zwei Dateien gehören in praktisch jedes Repository und sollten früh committet werden: eine `README.md`, die erklärt, was das Projekt ist und wie man es baut (`dotnet build`, `dotnet test`), und eine `LICENSE`, die festlegt, was andere mit dem Code tun dürfen – ohne Lizenz ist Code trotz Veröffentlichung nicht frei nutzbar. GitLab und GitHub bieten beim Anlegen eines Projekts Vorlagen für beides an.

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

Wir ändern jetzt eine Datei – etwa eine neue Methode in `FigurenVerwaltung.cs` – und sehen uns den Weg einmal komplett an:

```bash
git status
# Changes not staged for commit:
#   modified:   Geometrieeditor.Fachkonzept/FigurenVerwaltung.cs
git diff
# -    public bool Entfernen(Figur figur)
# +    public bool Entfernen(Figur figur)
# +    {
# +        return figuren.Remove(figur);
# +    }
git add Geometrieeditor.Fachkonzept/FigurenVerwaltung.cs
git diff --staged
# (zeigt jetzt dieselbe Änderung – sie liegt im Staging-Bereich)
git commit -m "Entfernen einer Figur in FigurenVerwaltung ergänzen"
```

`git diff` ohne Argument vergleicht die Arbeitskopie mit dem Staging-Bereich, `git diff --staged` den Staging-Bereich mit dem letzten Commit. Vor jedem Commit lohnt sich ein Blick auf `git diff --staged`: Es zeigt genau das, was gleich in die Historie wandert – und verrät vergessene `Console.WriteLine`-Debugausgaben.

## Historie lesen und Änderungen verwerfen

`git log` listet die Commits vom neuesten zum ältesten. Die Option `--oneline` reduziert jeden auf Hash und Nachricht:

```bash
git log --oneline
# a1f9c3e Entfernen einer Figur in FigurenVerwaltung ergänzen
# 7d2b0e4 Geometrieeditor als Solution mit vier Projekten anlegen
```

Manchmal will man eine Änderung nicht behalten. `git restore Datei` setzt die Arbeitskopie auf den Stand des Staging-Bereichs bzw. des letzten Commits zurück; `git restore --staged Datei` nimmt eine Datei nur aus dem Staging-Bereich heraus, lässt die Änderung in der Arbeitskopie aber stehen:

```bash
git restore --staged Geometrieeditor.Web/Components/Pages/Home.razor   # doch nicht in diesen Commit
git restore Geometrieeditor.Web/Components/Pages/Home.razor            # Änderung komplett verwerfen
```

`git restore` ohne `--staged` löscht deine ungespeicherten Änderungen unwiderruflich – Git hat davon keine Kopie, weil sie nie committet wurden. Im Zweifel erst committen, dann aufräumen: Ein überflüssiger Commit ist harmlos, verlorene Arbeit nicht.
{: .notice--warning}

## Gute Commits

Ein Commit sollte **eine** zusammengehörige Änderung enthalten: ein Bugfix, ein kleines Feature, eine Umbenennung. Wer „Kreis-Bug gefixt, JSON-Speicher angefangen, Layout umgebaut“ in einen Commit packt, kann später nichts davon einzeln ansehen oder rückgängig machen. Die Nachricht beschreibt im **Imperativ**, was der Commit tut, wie eine Anweisung an das Projekt: „Fläche von Dreieck mit Heron-Formel berechnen“, nicht „habe was am Dreieck gemacht“. Die erste Zeile bleibt unter 70 Zeichen; braucht man mehr, folgt nach einer Leerzeile ein Absatz mit dem *Warum*.

Übung: Lege ein Repository für ein kleines Konsolenprojekt an (`dotnet new console`, `dotnet new gitignore`, `git init`). Baue das Projekt, prüfe mit `git status`, dass `bin/` und `obj/` nicht auftauchen, und mache drei Commits mit sinnvollen Nachrichten. Sieh dir mit `git log --oneline` und `git diff HEAD~1` an, was du festgehalten hast.
{: .notice--info}

## Weitere Quellen

- [Grundlagen – Pro Git (git-scm.com)](https://git-scm.com/book/de/v2/Git-Grundlagen-Ein-Git-Repository-anlegen)
- [Änderungen nachverfolgen und im Repository speichern – Pro Git (git-scm.com)](https://git-scm.com/book/de/v2/Git-Grundlagen-%C3%84nderungen-nachverfolgen-und-im-Repository-speichern)
- [dotnet new gitignore – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/tools/dotnet-new-sdk-templates#gitignore)
