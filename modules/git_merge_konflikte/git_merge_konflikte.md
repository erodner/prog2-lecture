---
title: "Merge-Konflikte lösen"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Ein Merge-Konflikt ist kein Fehler und kein Zeichen, dass jemand etwas falsch gemacht hat. Er ist Gits Art zu sagen: „Zwei Leute haben dieselbe Stelle unterschiedlich geändert, und ich weiß nicht, welche Version stimmt – entscheidet ihr.“ Git ist beim Zusammenführen erstaunlich klug, solange Änderungen in verschiedenen Dateien oder verschiedenen Bereichen einer Datei liegen. Bei denselben Zeilen kann es aber keine inhaltliche Entscheidung treffen – ob ein Zug gegen eine Wand eine Runde kosten soll oder nicht, weiß nur, wer die Spielregeln kennt. Deshalb lernen wir hier, Konflikte gelassen zu lesen und aufzulösen.

## Wann entsteht ein Konflikt?

Beim Merge in [Branches und Merges](/modules/git_branching/git_branching.md) hat Git einen **Drei-Wege-Merge** durchgeführt. Es betrachtet drei Versionen jeder Datei: den **gemeinsamen Vorfahren** (*base*, der letzte Commit, den beide Branches teilen), **unsere** Seite (*ours*, der Branch, in dem du bist) und **ihre** Seite (*theirs*, der Branch, den du hereinholst). Für jede Zeile gilt: Hat nur eine Seite sie gegenüber dem Vorfahren geändert, übernimmt Git diese Änderung. Haben beide Seiten dieselben Zeilen unterschiedlich geändert, gibt es einen Konflikt:

```
              base (C3)
             /         \
   ours (main, C4)    theirs (feature, C5)
             \         /
           Merge-Ergebnis
```

Konflikte entstehen also nur, wenn drei Bedingungen zusammenkommen: zwei Branches, dieselbe Datei, überlappende Zeilen. Ein Umbenennen der Datei auf einer Seite und eine Änderung auf der anderen kann Git meist noch auflösen – eine Zeile, die beide Seiten unterschiedlich geschrieben haben, nicht.

## Ein Konflikt im Adventure

Zwei Personen arbeiten am Herzstück des Spiels, der Methode `Spielfeld.SpielerZieht`, die eine komplette Runde abwickelt. Auf `main` hat jemand eine Regel korrigiert: Ein Zug gegen eine Wand soll keine Runde kosten, die Gegner dürfen dafür also nicht ziehen. Im Branch `feature/falle` baut jemand anderes die neue Objektart aus [Branches und Merges](/modules/git_branching/git_branching.md) ein: Steht der Spieler zu Beginn seines Zuges auf einer Falle, löst sie aus. Beide haben dafür die ersten Zeilen derselben Methode umgestellt. Nebenbei hat der Feature-Branch auch `Gegner.cs` angefasst, damit eine `Wache` nicht in die eigene Falle läuft – diese Datei hat auf `main` niemand berührt. Beim Merge meldet Git:

```bash
git switch main
git merge feature/falle
# Auto-merging Adventure.Kern/Gegner.cs
# Auto-merging Adventure.Kern/Spielfeld.cs
# CONFLICT (content): Merge conflict in Adventure.Kern/Spielfeld.cs
# Automatic merge failed; fix conflicts and then commit the result.
git status
# You have unmerged paths.
#   (fix conflicts and run "git commit")
#   (use "git merge --abort" to abort the merge)
# Changes to be committed:
#   modified:   Adventure.Kern/Gegner.cs
# Unmerged paths:
#   both modified:   Adventure.Kern/Spielfeld.cs
```

An `Gegner.cs` sieht man, wie viel Git allein schafft: Die Änderung an `Wache.NaechsterZug` ist bereits zusammengeführt und liegt im Staging-Bereich, weil nur eine Seite sie vorgenommen hat. Auch in `Spielfeld.cs` ist alles Unstrittige schon in der Arbeitskopie – nur die umkämpfte Stelle enthält jetzt **Konfliktmarker**:

```csharp
public void SpielerZieht(Richtung richtung)
{
    if (Status != Spielstatus.Laeuft) return;

<<<<<<< HEAD
    Position ziel = Spieler.Position.Verschoben(richtung);
    StatischesObjekt? davor = StatischesObjektAn(ziel);
    if (davor is Wand)
    {
        LetzteMeldung = "Da ist eine Wand.";
        return;                       // ein Zug gegen die Wand kostet keine Runde
    }

    Runde++;
    StringBuilder meldung = new();
=======
    Runde++;
    StringBuilder meldung = new();
    if (StatischesObjektAn(Spieler.Position) is Falle falle)
    {
        meldung.Append(falle.Ausloesen(Spieler));
    }

    Position ziel = Spieler.Position.Verschoben(richtung);
    StatischesObjekt? davor = StatischesObjektAn(ziel);
>>>>>>> feature/falle

    if (davor is IInteragierbar interagierbar && !davor.IstPassierbar)
    {
        meldung.Append(interagierbar.Interagieren(Spieler));
    }
    // ... unverändert weiter: bewegen, aufheben, Gegner ziehen lassen
}
```

Zwischen `<<<<<<< HEAD` und `=======` steht unsere Seite (der aktuelle Branch `main`), zwischen `=======` und `>>>>>>> feature/falle` ihre Seite. Alles außerhalb der Marker war unstrittig. Die Datei kompiliert in diesem Zustand natürlich nicht – die Marker sind kein C#.

## Den Konflikt auflösen

Auflösen heißt: die Datei so bearbeiten, dass sie fachlich richtig ist, und alle Marker entfernen. Oft ist die Antwort nicht „ours“ oder „theirs“, sondern **beides in der richtigen Reihenfolge**. Hier muss die Wandprüfung zuerst kommen: Sonst würde ein Zug gegen die Wand die Falle auslösen und einen Lebenspunkt kosten, obwohl der Spieler sich gar nicht bewegt hat. Außerdem dürfen `ziel` und `davor` nur **einmal** deklariert werden – wer beide Seiten unbesehen untereinander klebt, bekommt vom Compiler eine doppelte Variablendeklaration:

```csharp
public void SpielerZieht(Richtung richtung)
{
    if (Status != Spielstatus.Laeuft) return;

    Position ziel = Spieler.Position.Verschoben(richtung);
    StatischesObjekt? davor = StatischesObjektAn(ziel);
    if (davor is Wand)
    {
        LetzteMeldung = "Da ist eine Wand.";
        return;                       // ein Zug gegen die Wand kostet keine Runde
    }

    Runde++;
    StringBuilder meldung = new();
    if (StatischesObjektAn(Spieler.Position) is Falle falle)
    {
        meldung.Append(falle.Ausloesen(Spieler));
    }

    if (davor is IInteragierbar interagierbar && !davor.IstPassierbar)
    {
        meldung.Append(interagierbar.Interagieren(Spieler));
    }
    // ... unverändert weiter: bewegen, aufheben, Gegner ziehen lassen
}
```

Nach dem Bearbeiten teilst du Git mit `git add` mit, dass der Konflikt in dieser Datei gelöst ist, und schließt den Merge mit einem Commit ab. Git schlägt die Nachricht selbst vor:

```bash
dotnet build                                    # kompiliert es wieder?
dotnet run --project Adventure.Konsole          # einmal gegen die Wand und einmal über die Falle laufen
git add Adventure.Kern/Spielfeld.cs
git commit
# [main 6d0f9a2] Merge branch 'feature/falle'
```

Bauen und Ausprobieren **vor** dem Commit ist kein optionaler Schritt. Ein Merge kann auch ohne Konfliktmarker fachlich falsch sein: Wenn eine Seite eine Methode umbenennt und die andere an einer ganz anderen Stelle den alten Namen aufruft, fügt Git beides klaglos zusammen – und erst der Compiler meckert. Sobald es Unit-Tests gibt (dazu mehr in [Vorlesung 12](/lectures/12/12.md)), läuft an dieser Stelle zusätzlich `dotnet test`.
{: .notice--warning}

Bist du mitten im Merge und merkst, dass du erst mit der anderen Person reden willst, bricht `git merge --abort` alles ab und stellt den Zustand vor dem `git merge` wieder her. Nichts geht verloren, beide Branches bleiben unverändert.

## Merge-Tools in der IDE

Konfliktmarker im Texteditor zu bearbeiten funktioniert, ist bei längeren Konflikten aber unübersichtlich. Visual Studio, Rider und VS Code bieten eine **Drei-Wege-Ansicht**: links „ours“, rechts „theirs“, in der Mitte das Ergebnis, und für jeden Block Schaltflächen, um die linke, die rechte oder beide Seiten zu übernehmen. Das Ergebnis speichert die IDE als aufgelöste Datei und führt das `git add` meist gleich mit aus; der abschließende Commit bleibt derselbe. Wie man dorthin gelangt, zeigt [Git in der IDE](/modules/git_in_der_ide/git_in_der_ide.md). Wer die Marker einmal von Hand aufgelöst hat, versteht, was die Schaltflächen tun – deshalb zuerst der Weg über die Kommandozeile.

## Konflikte vermeiden

Konflikte lassen sich nicht ganz verhindern, aber selten und klein halten:

- **Klein committen, oft pushen:** Ein Branch, der drei Tage alt ist, kollidiert eher als einer, der drei Stunden alt ist.
- **Oft pullen bzw. `main` in den Feature-Branch mergen:** Wer regelmäßig `git merge main` im Feature-Branch ausführt, löst kleine Konflikte sofort statt eines großen am Ende.
- **Absprachen im Team:** Wenn zwei Personen gleichzeitig `Spielfeld.cs` umbauen wollen, hilft ein kurzes Gespräch mehr als jedes Werkzeug. Eine saubere Aufteilung in Klassen und Dateien hilft ebenfalls: Wer an `Tuer.cs` arbeitet, kommt niemandem in `Gegner.cs` in die Quere – und sobald Oberfläche und Spiellogik ab [Vorlesung 04](/lectures/04/04.md) in getrennten Projekten liegen, gilt das erst recht.
- **Keine kosmetischen Massenänderungen:** Ein Commit, der alle Dateien neu formatiert, kollidiert mit jedem offenen Branch. Formatierung nur in abgesprochenen, eigenen Commits ändern.

Übung: Erzeuge in deinem Repository absichtlich einen Konflikt: Ändere in `main` die Meldung „Da geht es nicht weiter.“ in `Spielfeld.SpielerZieht`, lege dann einen Branch vom vorigen Commit an (`git switch -c test HEAD~1`) und ändere dort dieselbe Zeile anders. Merge den Branch in `main`, sieh dir die Marker an, löse den Konflikt auf und prüfe mit `git log --oneline --graph`, dass ein Merge-Commit mit zwei Eltern entstanden ist.
{: .notice--info}

## Weitere Quellen

- [Einfaches Branching und Merging – Merge-Konflikte – Pro Git (git-scm.com)](https://git-scm.com/book/de/v2/Git-Branching-Einfaches-Branching-und-Merging#_basic_merge_conflicts)
- [Fortgeschrittenes Merging – Pro Git (git-scm.com)](https://git-scm.com/book/de/v2/Git-Tools-Fortgeschrittenes-Merging)
- [Mergekonflikte in Visual Studio auflösen – Microsoft Learn](https://learn.microsoft.com/de-de/visualstudio/version-control/git-resolve-conflicts)
- [Oh Shit, Git!?! (deutsch)](https://ohshitgit.com/de) – Rezepte für den Moment nach dem misslungenen Merge: abbrechen, zurückdrehen, eine einzelne Datei von einer Seite holen.
