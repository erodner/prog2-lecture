---
title: "Git-Konzepte"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Wer schon einmal einen Ordner mit `Geometrieeditor_final`, `Geometrieeditor_final2` und `Geometrieeditor_final_wirklich` gesehen hat, kennt das Problem, das Versionsverwaltung löst. Kopien von Hand sind unübersichtlich, man weiß nach einer Woche nicht mehr, was sich zwischen zwei Ständen geändert hat, und sobald zwei Personen an derselben Datei arbeiten, überschreibt eine die Arbeit der anderen. Ein Versionsverwaltungssystem ist eine **Zeitmaschine** für ein Projekt: Jeder Stand wird mit Datum, Autor und Beschreibung festgehalten, man kann jederzeit zurückspringen und sieht genau, wer wann welche Zeile geändert hat. Git ist das mit Abstand verbreitetste dieser Systeme – in Firmen, in Open-Source-Projekten und auf dem GitLab der HTW.

## Warum Versionsverwaltung?

Ein Versionsverwaltungssystem (*Version Control System*, VCS) leistet mehrere Dinge auf einmal:

- **Zeitmaschine:** Jeder festgehaltene Stand („Version“) lässt sich wiederherstellen. Wenn die Flächenberechnung im `Dreieck` seit Dienstag falsche Werte liefert, kann man den Stand von Montag ansehen und vergleichen.
- **Zusammenarbeit:** Mehrere Personen ändern dasselbe Projekt, und das System führt ihre Änderungen zusammen, statt dass die letzte Person gewinnt.
- **Sicherung:** Das Projekt liegt nicht nur auf einem Laptop, sondern auch auf einem Server und auf den Rechnern aller Beteiligten.
- **Undo auf Projektebene:** Eine misslungene Änderung lässt sich rückgängig machen, auch wenn sie zehn Dateien betrifft.
- **Zweige:** Ein Feature kann isoliert entwickelt werden, während der Hauptstand weiter funktioniert – dazu mehr in [Branches und Merges](/modules/git_branching/git_branching.md).

## Textdateien und Binärdateien

Versionsverwaltung funktioniert am besten mit **Textdateien**: `.cs`, `.csproj`, `.json`, `.md`. Für Text kann Git zwei Versionen zeilenweise vergleichen und beim Speichern nur die Unterschiede (*Deltas*) komprimieren. Bei Binärdateien – Bilder, PDFs, Word-Dokumente, vor allem aber kompilierte `.dll`- und `.exe`-Dateien – geht das nicht: Jede Änderung bedeutet eine komplette neue Kopie, und das Repository wächst mit jeder Version. Daraus folgt die wichtigste Spielregel: Ins Repository gehören Quelltexte und Projektdateien, nicht die Build-Ausgaben in `bin/` und `obj/`. Wie man Git das beibringt, sehen wir in [Erste Schritte mit Git](/modules/git_erste_schritte/git_erste_schritte.md).

## Zentral oder dezentral?

Ältere Systeme wie Subversion sind **zentral**: Es gibt genau ein Repository auf einem Server, und jeder Entwickler hat nur eine Arbeitskopie des aktuellen Stands. Ohne Netzverbindung kann man weder die Historie ansehen noch eine Version festhalten. Git ist **dezentral**: Jeder Beteiligte hat das **vollständige** Repository mit der gesamten Historie auf dem eigenen Rechner. Man kann im Zug offline Versionen anlegen, Zweige erstellen und alte Stände vergleichen – und synchronisiert erst später mit dem Server. Der Server ist bei Git technisch nur ein weiteres Repository, das alle als gemeinsamen Treffpunkt vereinbart haben. Fällt er aus, hat jeder Entwickler weiterhin eine vollständige Kopie.

## Die wichtigsten Begriffe

| Begriff | Bedeutung |
| :--- | :--- |
| **Repository** | Die Datenbank mit allen Versionen; liegt im versteckten Ordner `.git/` im Projektordner |
| **Arbeitskopie** (*Working Tree*) | Die normalen Dateien im Projektordner, die du im Editor siehst und bearbeitest |
| **Staging-Bereich** (*Index*) | Zwischenablage: Änderungen, die in die nächste Version aufgenommen werden sollen |
| **Commit** | Eine festgehaltene Version: Momentaufnahme aller Dateien plus Autor, Zeit, Nachricht und Vorgänger |
| **Branch** | Ein benannter Zweig der Historie, z. B. `main` oder `feature/ellipse` |
| **Remote** | Ein anderes Repository, meist auf einem Server, mit dem man Commits austauscht |

Der Staging-Bereich irritiert Einsteiger am meisten: Warum nicht einfach alles direkt festhalten? Weil man oft an mehreren Dingen gleichzeitig gearbeitet hat – ein Bugfix in `Kreis.cs` und ein halbfertiges Feature in `FigurenVerwaltung.cs` – und nur den Bugfix als eigenständige Version festhalten möchte. Der Staging-Bereich erlaubt es, den nächsten Commit gezielt zusammenzustellen.

## Die Objektdatenbank

Im Ordner `.git/` speichert Git alles in einer einfachen Objektdatenbank mit drei Objektarten. Ein **Blob** ist der Inhalt einer Datei, ohne Namen. Ein **Tree** ist ein Verzeichnis: eine Liste von Namen, die auf Blobs (Dateien) oder weitere Trees (Unterordner) zeigen. Ein **Commit** zeigt auf genau einen Tree – den Wurzelordner des Projekts zu diesem Zeitpunkt – und zusätzlich auf seinen Vorgänger-Commit:

```
commit a1f9c3e  "Entfernen einer Figur in FigurenVerwaltung ergänzen"
  parent  ──► commit 7d2b0e4  "Geometrieeditor mit Fachkonzept und Konsolenprogramm anlegen"
  tree    ──► tree 3c8e...
                ├── .gitignore                     ──► blob 9f01...
                ├── Geometrieeditor.Fachkonzept/   ──► tree 51aa...
                │     ├── Figur.cs                 ──► blob e7c2...
                │     ├── Kreis.cs                 ──► blob 6a3f...
                │     └── FigurenVerwaltung.cs     ──► blob 0b4d...
                └── Geometrieeditor.Konsole/       ──► tree 8e13...
                      └── Program.cs               ──► blob 44f8...
```

Jedes Objekt wird über den **SHA-1-Hash** seines Inhalts benannt – die 40-stelligen Hexadezimalzahlen, von denen man im Alltag nur die ersten sieben Zeichen sieht. Das hat zwei Konsequenzen. Erstens ist der Name eines Objekts eine Prüfsumme: Ändert sich ein einziges Byte in `Figur.cs`, entsteht ein Blob mit einem anderen Namen, und damit auch ein neuer Tree und ein neuer Commit. Nichts in der Historie lässt sich unbemerkt manipulieren. Zweitens werden unveränderte Dateien nicht doppelt gespeichert: Zeigt der neue Commit in zwei Ordnern auf denselben Blob wie der alte, liegt der Inhalt nur einmal auf der Platte.

Ein Commit ist eine **Momentaufnahme** (*Snapshot*) des gesamten Projekts, kein Diff. Git speichert nicht „in Zeile 12 wurde `figuren.Add` eingefügt“, sondern den kompletten Zustand aller Dateien – und berechnet Unterschiede erst, wenn du sie mit `git diff` sehen willst. Die Deltakompression aus dem Abschnitt oben passiert erst darunter, beim Packen der Objekte auf der Festplatte.
{: .notice--primary}

## Git ist ein Graph

Weil jeder Commit auf seinen Vorgänger zeigt, bildet die Historie eine Kette – und sobald zwei Personen vom selben Commit aus weiterarbeiten, eine Verzweigung. Ein Branch wie `main` ist in Git nichts weiter als ein beweglicher Zeiger auf einen Commit:

```
        main
          │
          ▼
 C1 ◄── C2 ◄── C3
          ▲
          └── C4 ◄── C5
                     ▲
                     │
              feature/ellipse
```

`C3` und `C4` haben denselben Elternteil `C2`. Führt man später beide Zweige zusammen, entsteht ein Commit mit **zwei** Eltern. Alles, was Git sonst noch kann – Branches, Merges, das Zurückspringen auf alte Stände –, ist nur ein Bewegen von Zeigern in diesem Graphen von Momentaufnahmen. Wer dieses Bild im Kopf hat, versteht auch die Fehlermeldungen, die Git gelegentlich ausgibt.

Übung: Zeichne den Graphen für folgendes Szenario: Du legst drei Commits an, erstellst dann einen Branch `feature/sortieren`, machst dort zwei Commits und in `main` inzwischen einen weiteren. Welche Commits haben denselben Elternteil? Auf welchen Commit zeigt `main`?
{: .notice--info}

## Weitere Quellen

- [Was ist Versionsverwaltung? – Pro Git (git-scm.com)](https://git-scm.com/book/de/v2/Erste-Schritte-Was-ist-Versionsverwaltung%3F)
- [Was ist Git? – Pro Git (git-scm.com)](https://git-scm.com/book/de/v2/Erste-Schritte-Was-ist-Git%3F)
- [Git-Objekte – Pro Git (git-scm.com)](https://git-scm.com/book/de/v2/Git-Internals-Git-Objekte)
