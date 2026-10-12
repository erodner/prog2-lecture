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

Wer schon einmal einen Ordner mit `Adventure_final`, `Adventure_final2` und `Adventure_final_wirklich` gesehen hat, kennt das Problem, das Versionsverwaltung löst. Kopien von Hand sind unübersichtlich, man weiß nach einer Woche nicht mehr, was sich zwischen zwei Ständen geändert hat, und sobald zwei Personen an derselben Datei arbeiten, überschreibt eine die Arbeit der anderen. Ein Versionsverwaltungssystem ist eine **Zeitmaschine** für ein Projekt: Jeder Stand wird mit Datum, Autor und Beschreibung festgehalten, man kann jederzeit zurückspringen und sieht genau, wer wann welche Zeile geändert hat. Git ist das mit Abstand verbreitetste dieser Systeme – in Firmen, in Open-Source-Projekten und auf dem GitLab der HTW.

## Warum Versionsverwaltung?

Ein Versionsverwaltungssystem (*Version Control System*, VCS) leistet mehrere Dinge auf einmal:

- **Zeitmaschine:** Jeder festgehaltene Stand („Version“) lässt sich wiederherstellen. Wenn die Wachen seit Dienstag durch Wände laufen, kann man den Stand von Montag ansehen und vergleichen.
- **Zusammenarbeit:** Mehrere Personen ändern dasselbe Projekt, und das System führt ihre Änderungen zusammen, statt dass die letzte Person gewinnt.
- **Sicherung:** Das Projekt liegt nicht nur auf einem Laptop, sondern auch auf einem Server und auf den Rechnern aller Beteiligten.
- **Undo auf Projektebene:** Eine misslungene Änderung lässt sich rückgängig machen, auch wenn sie zehn Dateien betrifft.
- **Zweige:** Ein Feature kann isoliert entwickelt werden, während der Hauptstand weiter funktioniert – dazu mehr in [Branches und Merges](/modules/git_branching/git_branching.md) (erweitertes Wissen).

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
| **Commit** | Ein **Änderungspaket**: zusammengehörige Änderungen an einer oder mehreren Dateien, festgehalten mit Autor, Zeit, Nachricht und Vorgänger |
| **Branch** | Ein **Zeiger auf einen Commit** mit einem Namen, z. B. `main` oder `feature/falle`; bei jedem neuen Commit wandert er mit |
| **Remote** | Ein anderes Repository, meist auf einem Server, mit dem man Commits austauscht |
| **Tag** | Ein fester Name für einen bestimmten Commit, z. B. `v02-interfaces` für den Stand nach Vorlesung 02 |

Der Staging-Bereich irritiert Einsteiger am meisten: Warum nicht einfach alles direkt festhalten? Weil man oft an mehreren Dingen gleichzeitig gearbeitet hat – ein Bugfix in `Gegner.cs` und ein halbfertiges Feature in `Spielfeld.cs` – und nur den Bugfix als eigenes Änderungspaket festhalten möchte. Im Staging-Bereich schnürt man das nächste Paket gezielt zusammen.

Ein Änderungspaket enthält immer **inhaltlich zusammenhängende** Änderungen, und zwar genau eine dieser Arten:

- ein **neues Feature**, zum Beispiel eine neue Gegnerart,
- ein **Bugfix**, zum Beispiel die Wache, die durch Wände läuft,
- eine **Dokumentationsänderung**, zum Beispiel im README oder in Kommentaren,
- ein **Refactoring**: Die Struktur des Codes ändert sich, die Funktionalität aber nicht, zum Beispiel eine Umbenennung oder eine neue Zwischenklasse.

Wer Bugfix und halbes Feature in ein Paket schnürt, kann später keines von beiden einzeln ansehen oder rückgängig machen.
{: .notice--primary}

## Der Ordner `.git`

Das Repository liegt im versteckten Ordner `.git/` direkt im Projektordner. Darin speichert Git alle Commits mit der gesamten Historie. Die Dateien daneben sind nur die Arbeitskopie, also der Stand, an dem du gerade arbeitest:

```
Adventure/
├── .git/                  ← das Repository: alle Commits, Branches, Tags
├── .gitignore
├── Adventure.slnx
├── Adventure.Kern/
└── Adventure.Konsole/
```

Daraus folgen zwei praktische Regeln. In `.git/` arbeitet man nie von Hand, das erledigen ausschließlich die `git`-Befehle. Und wer `.git/` löscht, behält zwar die aktuellen Dateien, verliert aber die komplette Historie. Umgekehrt genügt es, einen Projektordner samt `.git/` zu kopieren, um das ganze Repository mitzunehmen.

Jeder Commit bekommt als Namen einen **Hash**, eine 40-stellige Hexadezimalzahl, die Git aus dem Inhalt des Commits berechnet. Im Alltag sieht man meist nur die ersten sieben Zeichen. Außerdem merkt sich jeder Commit seinen Vorgänger:

```
commit eabce72  "Vorlesung 02: abstrakte Klassen, Interfaces, Gegner, Türen, Truhen"
  Vorgänger ──► commit 6441a70  "Vorlesung 01: Spielobjekt, Wand und Spieler"
```

Weil der Hash aus dem Inhalt berechnet wird, ist er zugleich eine Prüfsumme: Ändert sich auch nur ein Byte, entsteht ein anderer Hash. Nichts in der Historie lässt sich deshalb unbemerkt manipulieren.

## Git ist ein Graph

Weil jeder Commit auf seinen Vorgänger zeigt, bildet die Historie eine Kette – und sobald zwei Personen vom selben Commit aus weiterarbeiten, eine Verzweigung. Ein Branch wie `main` ist in Git nichts weiter als ein beweglicher Zeiger auf einen Commit:

<svg viewBox="0 0 600 190" role="img" aria-labelledby="git-graph-titel" xmlns="http://www.w3.org/2000/svg" style="max-width:100%;height:auto;font-family:system-ui,sans-serif"><title id="git-graph-titel">Commit-Graph: C1, C2 und C3 liegen auf main, C4 und C5 zweigen von C2 ab und liegen auf feature/falle. Jeder Commit zeigt auf seinen Vorgänger, die Branches zeigen auf ihren neuesten Commit.</title><defs><marker id="pfeil-graph" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="6" markerHeight="6" orient="auto-start-reverse"><path d="M 0 0 L 10 5 L 0 10 z" fill="currentColor"/></marker></defs><g stroke="currentColor" stroke-width="1.5" fill="none" marker-end="url(#pfeil-graph)"><line x1="267" y1="80" x2="183" y2="80"/><line x1="157" y1="80" x2="73" y2="80"/><path d="M 267 155 C 220 155, 185 130, 176 92"/><line x1="377" y1="155" x2="293" y2="155"/><line x1="350" y1="80" x2="297" y2="80"/><line x1="460" y1="155" x2="407" y2="155"/></g><g stroke="currentColor" stroke-width="1.5" fill="currentColor" fill-opacity="0.06"><circle cx="60" cy="80" r="13"/><circle cx="170" cy="80" r="13"/><circle cx="280" cy="80" r="13"/><circle cx="280" cy="155" r="13"/><circle cx="390" cy="155" r="13"/></g><g fill="currentColor" font-size="13" text-anchor="middle"><text x="60" y="56">C1</text><text x="170" y="56">C2</text><text x="280" y="56">C3</text><text x="280" y="131">C4</text><text x="390" y="131">C5</text></g><g fill="currentColor" font-family="ui-monospace,monospace" font-size="12"><text x="356" y="84">main</text><text x="466" y="159">feature/falle</text></g></svg>

`C3` und `C4` haben denselben Elternteil `C2`. Führt man später beide Zweige zusammen, entsteht ein Commit mit **zwei** Eltern. Alles, was Git sonst noch kann – Branches, Merges, das Zurückspringen auf alte Stände –, ist nur ein Bewegen von Zeigern in diesem Graphen von Commits. Wer dieses Bild im Kopf hat, versteht auch die Fehlermeldungen, die Git gelegentlich ausgibt.

Übung: Zeichne den Graphen für folgendes Szenario: Du legst drei Commits an, erstellst dann einen Branch `feature/truhen`, machst dort zwei Commits und in `main` inzwischen einen weiteren. Welche Commits haben denselben Elternteil? Auf welchen Commit zeigt `main`?
{: .notice--info}

## Weitere Quellen

- [Was ist Versionsverwaltung? – Pro Git (git-scm.com)](https://git-scm.com/book/de/v2/Erste-Schritte-Was-ist-Versionsverwaltung%3F)
- [Was ist Git? – Pro Git (git-scm.com)](https://git-scm.com/book/de/v2/Erste-Schritte-Was-ist-Git%3F)
- [Git-Objekte – Pro Git (git-scm.com)](https://git-scm.com/book/de/v2/Git-Interna-Git-Objekte)
- [Visualizing Git](https://git-school.github.io/visualizing-git/) – ein Spielplatz im Browser: Du tippst `git commit`, `git branch` oder `git merge` ein und siehst den Graphen live wachsen.
- [Git Objects – Pro Git (englisch)](https://git-scm.com/book/en/v2/Git-Internals-Git-Objects) – die englische Fassung des Interna-Kapitels; sie ist ausführlicher und enthält die Beispiele mit `git hash-object` und `git cat-file`.
