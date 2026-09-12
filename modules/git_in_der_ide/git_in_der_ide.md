---
title: "Git in der IDE"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Nachdem wir Git auf der Kommandozeile kennengelernt haben, darf die IDE die Tipparbeit übernehmen. Visual Studio 2022, Rider und VS Code bringen alle einen vollständigen Git-Client mit: Sie zeigen geänderte Zeilen farbig am Rand des Editors, listen Änderungen auf, committen, pushen und lösen Konflikte in einer Drei-Wege-Ansicht. Die Empfehlung dieses Moduls lautet dabei: **CLI verstehen, IDE benutzen.** Die IDE ruft im Hintergrund genau die Befehle auf, die du kennst – und wenn sie einen Fehler anzeigt, ist es die Meldung von `git`. Wer weiß, was `push` und `pull` bedeuten, findet sich in jeder dieser Oberflächen sofort zurecht. Daneben gibt es eigenständige grafische Clients wie Sourcetree oder GitKraken; sie bieten nichts, was die IDE nicht auch könnte.

## Befehl ↔ IDE-Aktion

Die folgende Tabelle ordnet jedem Kommandozeilenbefehl aus den vorigen Modulen die entsprechende Stelle in den drei IDEs zu:

| CLI-Befehl | Visual Studio 2022 | Rider | VS Code |
| :--- | :--- | :--- | :--- |
| `git init` / `git clone` | *Git → Repository erstellen* / *Repository klonen* im Startfenster | *Git → Clone…* bzw. *VCS → Create Git Repository* | Source Control → *Clone Repository* / *Initialize Repository* |
| `git status` | Fenster *Git-Änderungen*: Liste der Änderungen | Toolwindow *Commit*: Baum der Änderungen | Ansicht *Source Control* (Symbol in der Seitenleiste) |
| `git add Datei` | Plus-Symbol an der Datei → Abschnitt *Gestaged* | Häkchen an der Datei im Commit-Toolwindow | Plus-Symbol → Abschnitt *Staged Changes* |
| `git diff` | Doppelklick auf eine Datei öffnet den Vergleich | Doppelklick bzw. *Show Diff* | Klick auf eine Datei zeigt den Diff |
| `git commit -m "…"` | Nachricht eingeben → *Commit staged* | Nachricht eingeben → *Commit* | Nachricht eingeben → Häkchen *Commit* |
| `git log --oneline --graph` | Fenster *Git-Repository* (Verlauf) | Toolwindow *Git → Log* | Erweiterung *Git Graph* oder GitLens (optional) |
| `git switch -c feature/…` | Branch-Auswahl in der Statusleiste → *Neuer Branch* | Branch-Auswahl in der Statusleiste → *New Branch* | Branch-Name in der Statusleiste → *Create new branch* |
| `git switch main` | Branch-Auswahl → Branch auswählen | Branch-Auswahl → *Checkout* | Branch-Name in der Statusleiste → Branch wählen |
| `git merge feature/…` | *Git-Repository*-Fenster → Rechtsklick auf Branch → *In aktuellen Branch mergen* | *Git → Log* → Rechtsklick → *Merge into Current* | Kommandopalette → *Git: Merge Branch…* |
| `git fetch` / `git pull` / `git push` | Pfeil-Symbole oben im Fenster *Git-Änderungen* | Pfeil-Symbole in der Git-Toolbar / *Git → Push…* | Symbol *Synchronize Changes* in der Statusleiste (= pull + push) |
| `git restore Datei` | Rechtsklick → *Rückgängig machen* | Rechtsklick → *Rollback* | Pfeil-Symbol *Discard Changes* |
| `git stash` | *Git-Änderungen* → *Stash* | *Git → Uncommitted Changes → Stash* | Kommandopalette → *Git: Stash* |
| Konflikt lösen | *Git-Änderungen* → *Nicht gemergte Änderungen* → Merge-Editor | *Resolve Conflicts* → Drei-Wege-Dialog | Datei öffnen → *Resolve in Merge Editor* |
| `git tag` / `git checkout <tag>` | Branch-Auswahl → Abschnitt *Tags* | Branch-Auswahl → *Tags* → *Checkout* | Branch-Name in der Statusleiste → Abschnitt *Tags* |
| `git blame Datei` | Rechtsklick im Editor → *Git → Blame (Anmerkungen)* | Rechtsklick auf den Zeilenrand → *Annotate with Git Blame* | GitLens: Anmerkung am Zeilenende (optional) |

Die Namen unterscheiden sich, das Modell dahinter nicht: Es gibt immer eine Liste ungestagter Änderungen, eine Liste gestagter Änderungen, ein Feld für die Nachricht und einen Commit-Knopf.

## Typische Arbeitsschritte in der IDE

Der Tagesablauf aus [Remote-Repositorys](/modules/git_remote/git_remote.md) sieht in der IDE so aus. Zuerst **pullen**: der Pfeil nach unten in der Git-Toolbar (Visual Studio zeigt im Fenster *Git-Änderungen* an, wie viele Commits eingehend bzw. ausgehend sind). Dann arbeiten. Geänderte Zeilen markiert jede IDE mit einem farbigen Streifen am linken Rand des Editors – ein Klick darauf zeigt die alte Version und bietet an, die Änderung zurückzunehmen; das ist `git diff` und `git restore` für eine einzelne Stelle.

Zum **Committen** öffnest du das Änderungsfenster. Hier zahlt sich der Staging-Bereich aus, den wir in [Erste Schritte mit Git](/modules/git_erste_schritte/git_erste_schritte.md) kennengelernt haben: Du kannst einzelne Dateien oder in Rider und VS Code sogar einzelne Zeilenblöcke (*Hunks*) stagen und so aus einer unordentlichen Arbeitssitzung zwei saubere Commits machen. Visual Studio bietet zusätzlich *Commit staged und pushen* als einen Knopf – bequem, aber du solltest wissen, dass es zwei Befehle sind, weil ein abgelehnter Push (Remote ist neuer) genau an dieser Stelle auftritt.

Beim **Branch-Wechsel** über die Statusleiste verhält sich die IDE wie `git switch`: Hat die Arbeitskopie ungespeicherte Änderungen, die mit dem Ziel kollidieren, fragt Rider, ob es sie stashen soll; Visual Studio verweigert den Wechsel mit derselben Meldung wie die Kommandozeile.

Bei einem **Konflikt** nach Merge oder Pull erscheint die betroffene Datei in einem eigenen Abschnitt. Der Merge-Editor zeigt die drei Versionen aus [Merge-Konflikte lösen](/modules/git_merge_konflikte/git_merge_konflikte.md) nebeneinander: links den aktuellen Branch, rechts den hereinkommenden, unten das Ergebnis. Für jeden Konfliktblock gibt es Häkchen, um die linke, die rechte oder beide Seiten zu übernehmen – und das Ergebnisfenster ist ein normaler Editor, in dem du die Reihenfolge anpassen kannst. *Merge akzeptieren* entspricht `git add`; der Commit folgt danach wie gewohnt.

Was die IDE dagegen **nicht** für dich übernimmt, ist das Nachdenken über die `.gitignore`: Alle drei bieten zwar einen Kontextmenüeintrag *Add to .gitignore*, aber die Vorlage aus `dotnet new gitignore` legst du weiterhin selbst an – und zwar vor dem ersten Commit.

Die IDE zeigt oft nur eine Kurzfassung der Git-Ausgabe. Schlägt eine Aktion fehl, lohnt sich ein Blick ins Ausgabefenster (Visual Studio: *Ausgabe → Quelle: Git*; Rider und VS Code: *Git*-Ausgabe) – dort steht die vollständige Meldung, meist inklusive `hint:`-Zeilen mit dem Lösungsvorschlag.
{: .notice--primary}

## Ein fremdes Repository in der IDE öffnen

Der schnellste Weg, das alles auszuprobieren, ist ein Repository, das es schon gibt. Alle drei IDEs bieten im Startfenster einen Klon-Dialog: Du fügst die URL `https://github.com/erodner/prog2-adventure.git` ein, wählst einen Zielordner, und die IDE ruft im Hintergrund `git clone` auf und öffnet anschließend die Solution. Das ist derselbe Vorgang wie in [Remote-Repositorys](/modules/git_remote/git_remote.md), nur mit Formularfeldern statt Argumenten.

Interessant wird danach die Branch-Auswahl in der Statusleiste: Sie listet nicht nur Branches, sondern in einem eigenen Abschnitt auch die **Tags** – hier also `v01-vererbung` bis `v12-tests`. Ein Klick auf `v02-interfaces` entspricht `git checkout v02-interfaces` und legt die Arbeitskopie auf den Stand nach Vorlesung 02; die IDE weist dabei wie die Kommandozeile darauf hin, dass du dich nicht mehr auf einem Branch befindest. Über dieselbe Auswahl kommst du mit einem Klick auf `main` zurück.

Das Verlaufsfenster (*Git-Repository* in Visual Studio, *Git → Log* in Rider) zeigt denselben Graphen wie `git log --oneline --graph --all`, nur klickbar: Links stehen die Commits, rechts die geänderten Dateien, und ein Doppelklick auf eine Datei öffnet den Diff dieses Commits. Für das Nachlesen einer fremden Codebasis ist das oft der bequemste Einstieg – man sieht, welche Dateien zusammen entstanden sind.

## Nachvollziehen mit Blame

Irgendwann fragt man sich: Warum verbraucht das Aufschließen einer Tür den Schlüssel, und seit wann? `git blame` beantwortet das Zeile für Zeile – jede Zeile wird mit dem Commit, dem Autor und dem Datum ihrer letzten Änderung annotiert:

```bash
git blame -s Adventure.Kern/Tuer.cs | sed -n '15,26p'
# 6d0f9a2 15)     public string Interagieren(Spieler spieler)
# 6d0f9a2 16)     {
# 5c21a8f 17)         if (IstOffen)
# 5c21a8f 18)         {
# 5c21a8f 19)             return "Die Tür ist schon offen.";
# 5c21a8f 20)         }
# 3e7f0c2 21)         if (!spieler.Inventar.Enthaelt<Schluessel>())
# 3e7f0c2 22)         {
# 3e7f0c2 23)             return "Die Tür ist verschlossen. Du brauchst einen Schlüssel.";
# 3e7f0c2 24)         }
# 3e7f0c2 25)         spieler.Inventar.Entfernen<Schluessel>();
# 3e7f0c2 26)         IstOffen = true;
```

Mit `git show 3e7f0c2` sieht man dann die vollständige Commit-Nachricht und den gesamten Diff – gute Commit-Nachrichten zahlen sich genau hier aus. In den IDEs heißt dieselbe Funktion *Annotate* (Rider) oder *Blame* (Visual Studio, GitLens) und blendet die Informationen direkt neben dem Code ein. Blame ist kein Werkzeug, um Schuldige zu finden, sondern um den Kontext einer Zeile zu verstehen – deshalb sind die Commit-Nachrichten wichtiger als die Namen.

Übung: Klone zuerst `https://github.com/erodner/prog2-adventure.git` über den Klon-Dialog deiner IDE statt über das Terminal – so siehst du, welche Felder dort dem Befehl entsprechen. Öffne danach dein eigenes Adventure und führe den kompletten Ablauf einmal ohne Terminal durch: neuen Branch anlegen, eine Änderung in zwei getrennten Commits stagen und committen, pushen, zu `main` wechseln, mergen. Prüfe danach im Terminal mit `git log --oneline --graph --all`, dass der Graph so aussieht, wie du ihn erwartet hast.
{: .notice--info}

## Weitere Quellen

- [Git in Visual Studio – Microsoft Learn](https://learn.microsoft.com/de-de/visualstudio/version-control/git-with-visual-studio)
- [Git-Fenster in Visual Studio (Git-Änderungen, Git-Repository) – Microsoft Learn](https://learn.microsoft.com/de-de/visualstudio/version-control/git-browse-repository)
- [Version Control in VS Code – Visual Studio Code Docs](https://code.visualstudio.com/docs/sourcecontrol/overview)
- [Git in JetBrains Rider – JetBrains-Dokumentation](https://www.jetbrains.com/help/rider/Using_Git_Integration.html)
