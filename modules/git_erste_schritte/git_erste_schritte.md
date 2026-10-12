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

In [Git-Konzepte](/modules/git_konzepte/git_konzepte.md) haben wir gesehen, dass ein Repository ein Graph von Commits ist, also von Änderungspaketen, die jeweils auf ihren Vorgänger zeigen. In diesem Modul legen wir ein solches Repository für unser Adventure an und halten die ersten Versionen fest – ausschließlich auf der Kommandozeile. Das ist kein Selbstzweck: Auch die Git-Schaltflächen in Visual Studio rufen genau diese Befehle auf, und wenn etwas schiefgeht, zeigen sie meist nur deren Fehlermeldung an. Wer die Befehle kennt, kann sie lesen.

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
```

Die erzeugte Datei ist eine einfache Textdatei mit einem Muster pro Zeile. Ein Ausschnitt:

```
# Build results
[Dd]ebug/
[Rr]elease/
x64/
[Bb]in/
[Oo]bj/
```

Eckige Klammern stehen für „eines dieser Zeichen“: `[Bb]in/` passt also auf `bin/` und auf `Bin/`, der Schrägstrich am Ende bedeutet „nur Ordner“.

Die Vorlage deckt `bin/`, `obj/`, `.vs/`, Testergebnisse und Dutzende weitere Fälle ab. Ein `git status` zeigt jetzt nur noch Quelltexte und Projektdateien. Die `.gitignore` selbst wird mit committet, damit alle im Team dieselben Regeln haben.

Erst `.gitignore` anlegen, dann `git add`. Ist `bin/` einmal committet, hilft die `.gitignore` nicht mehr – Git verfolgt die Dateien weiter, bis man sie mit `git rm -r --cached bin` explizit aus dem Index entfernt. Und die alten Binärdateien bleiben für immer in der Historie.
{: .notice--warning}

## Der erste Commit: add und commit

Ein Commit entsteht in zwei Schritten. `git add` legt Änderungen in den Staging-Bereich, `git commit` schnürt daraus ein Änderungspaket. Der Punkt bei `git add .` bedeutet „alles im aktuellen Ordner“ – abzüglich dessen, was `.gitignore` ausschließt:

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

<svg viewBox="0 0 700 222" role="img" aria-labelledby="git-zyklus-titel" xmlns="http://www.w3.org/2000/svg" style="max-width:100%;height:auto;font-family:system-ui,sans-serif"><title id="git-zyklus-titel">Der Zyklus einer Änderung: git add bringt sie von der Arbeitskopie in den Staging-Bereich, git commit vom Staging-Bereich ins Repository; git restore --staged nimmt sie aus dem Staging-Bereich zurück, git switch holt einen anderen Stand aus dem Repository in die Arbeitskopie.</title><defs><marker id="pfeil-zyklus" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="6" markerHeight="6" orient="auto-start-reverse"><path d="M 0 0 L 10 5 L 0 10 z" fill="currentColor"/></marker></defs><g stroke="currentColor" stroke-width="1.5" fill="currentColor" fill-opacity="0.06"><rect x="20" y="10" width="180" height="46" rx="4"/><rect x="260" y="10" width="180" height="46" rx="4"/><rect x="500" y="10" width="180" height="46" rx="4"/></g><g stroke="currentColor" stroke-opacity="0.35" stroke-width="1" stroke-dasharray="4 4"><line x1="110" y1="56" x2="110" y2="215"/><line x1="350" y1="56" x2="350" y2="215"/><line x1="590" y1="56" x2="590" y2="215"/></g><g stroke="currentColor" stroke-width="1.5" fill="none" marker-end="url(#pfeil-zyklus)"><line x1="114" y1="92" x2="346" y2="92"/><line x1="354" y1="127" x2="586" y2="127"/><line x1="346" y1="162" x2="114" y2="162"/><line x1="586" y1="197" x2="114" y2="197"/></g><g fill="currentColor" text-anchor="middle"><text x="110" y="30" font-size="13">Arbeitskopie</text><text x="110" y="47" font-size="12">(modified)</text><text x="350" y="30" font-size="13">Staging-Bereich</text><text x="350" y="47" font-size="12">(staged)</text><text x="590" y="30" font-size="13">Repository</text><text x="590" y="47" font-size="12">(committed)</text></g><g fill="currentColor" font-family="ui-monospace,monospace" font-size="12" text-anchor="middle"><text x="230" y="85">git add Datei</text><text x="470" y="120">git commit</text><text x="230" y="155">git restore --staged Datei</text><text x="350" y="190">git switch (anderen Stand holen)</text></g></svg>

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

## ➕ Stände benennen: Tags

**➕ Erweitertes Wissen.** Die folgenden Module setzen diesen Abschnitt nicht voraus.
{: .notice--success}

Ein Hash wie `a1f9c3e` ist eindeutig, aber nichts, was man sich merkt. Mit einem **Tag** gibt man einem Commit einen festen Namen, typischerweise für eine veröffentlichte Version. Anders als ein Branch wandert ein Tag nicht mit. Im Adventure-Repository markieren Tags wie `v01-vererbung` den Stand nach jeder Vorlesung.

```bash
git tag -a v1.0 -m "Erste spielbare Version"   # Tag setzen
git push --tags                                 # Tags werden nicht automatisch gepusht
git checkout v01-vererbung                      # alten Stand ansehen
git switch main                                 # zurück zum aktuellen Stand
```

## Gute Commits

Wie in [Git-Konzepte](/modules/git_konzepte/git_konzepte.md) gesagt: Ein Commit ist ein Änderungspaket mit **inhaltlich zusammenhängenden** Änderungen, also genau ein Feature, ein Bugfix, eine Dokumentationsänderung oder ein Refactoring. „Wachen-Bug gefixt, Fallen angefangen, Beschreibung umformuliert“ sind drei Pakete, nicht eines. Die Nachricht beschreibt im **Imperativ**, was der Commit tut, wie eine Anweisung an das Projekt: „Wache beim Anstoßen umdrehen lassen“, nicht „habe was an der Wache gemacht“. Die erste Zeile bleibt unter 70 Zeichen; braucht man mehr, folgt nach einer Leerzeile ein Absatz mit dem *Warum*.

Übung: Lege ein Repository für dein eigenes Adventure an (`dotnet new gitignore`, `git init` im Projektordner). Baue die Solution, prüfe mit `git status`, dass `bin/` und `obj/` nicht auftauchen, und mache drei Commits mit sinnvollen Nachrichten – zum Beispiel eine neue Gegenstandsart, ein weiteres Level und eine Korrektur an der Konsolenausgabe. Sieh dir mit `git log --oneline` und `git diff HEAD~1` an, was du festgehalten hast.
{: .notice--info}

## Weitere Quellen

- [Grundlagen – Pro Git (git-scm.com)](https://git-scm.com/book/de/v2/Git-Grundlagen-Ein-Git-Repository-anlegen)
- [Änderungen nachverfolgen und im Repository speichern – Pro Git (git-scm.com)](https://git-scm.com/book/de/v2/Git-Grundlagen-%C3%84nderungen-nachverfolgen-und-im-Repository-speichern)
- [dotnet new gitignore – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/tools/dotnet-new-sdk-templates#gitignore)
- [Oh Shit, Git!?! (deutsch)](https://ohshitgit.com/de) – kurze Rezeptsammlung für die Momente, in denen etwas schiefgegangen ist: falsche Commit-Nachricht, falscher Branch, versehentlich verworfene Änderung.
