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

Ein Merge-Konflikt ist kein Fehler und kein Zeichen, dass jemand etwas falsch gemacht hat. Er ist Gits Art zu sagen: „Zwei Leute haben dieselbe Stelle unterschiedlich geändert, und ich weiß nicht, welche Version stimmt – entscheidet ihr.“ Git ist beim Zusammenführen erstaunlich klug, solange Änderungen in verschiedenen Dateien oder verschiedenen Bereichen einer Datei liegen. Bei denselben Zeilen kann es aber keine inhaltliche Entscheidung treffen – ob die Prüfung auf doppelte Namen oder die auf `null` zuerst kommen soll, weiß nur, wer den Code versteht. Deshalb lernen wir hier, Konflikte gelassen zu lesen und aufzulösen.

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

## Ein Konflikt im Geometrieeditor

Zwei Personen arbeiten an `FigurenVerwaltung.Hinzufuegen`. Die eine will auf `main`, dass Figuren mit leerem Namen abgelehnt werden. Die andere fügt im Branch `feature/figur-validierung` eine Prüfung hinzu, dass `figur` nicht `null` sein darf. Beide ändern die ersten Zeilen der Methode. Beim Merge meldet Git:

```bash
git switch main
git merge feature/figur-validierung
# Auto-merging Geometrieeditor.Fachkonzept/FigurenVerwaltung.cs
# CONFLICT (content): Merge conflict in Geometrieeditor.Fachkonzept/FigurenVerwaltung.cs
# Automatic merge failed; fix conflicts and then commit the result.
git status
# You have unmerged paths.
#   (fix conflicts and run "git commit")
#   (use "git merge --abort" to abort the merge)
# Unmerged paths:
#   both modified:   Geometrieeditor.Fachkonzept/FigurenVerwaltung.cs
```

Git hat den Merge nicht abgebrochen, sondern angehalten: Alle unproblematischen Änderungen sind bereits in der Arbeitskopie, nur die betroffene Datei enthält jetzt **Konfliktmarker**:

```csharp
public void Hinzufuegen(Figur figur)
{
<<<<<<< HEAD
    if (string.IsNullOrWhiteSpace(figur.Name))
    {
        throw new ArgumentException("Eine Figur braucht einen Namen.");
    }
=======
    ArgumentNullException.ThrowIfNull(figur);
>>>>>>> feature/figur-validierung
    if (figuren.Any(f => f.Name == figur.Name))
    {
        throw new ArgumentException($"Es gibt bereits eine Figur mit dem Namen '{figur.Name}'.");
    }
    figuren.Add(figur);
}
```

Zwischen `<<<<<<< HEAD` und `=======` steht unsere Seite (der aktuelle Branch `main`), zwischen `=======` und `>>>>>>> feature/figur-validierung` ihre Seite. Alles außerhalb der Marker war unstrittig. Die Datei kompiliert in diesem Zustand natürlich nicht – die Marker sind kein C#.

## Den Konflikt auflösen

Auflösen heißt: die Datei so bearbeiten, dass sie fachlich richtig ist, und alle Marker entfernen. Oft ist die Antwort nicht „ours“ oder „theirs“, sondern **beides in der richtigen Reihenfolge**. Hier muss die `null`-Prüfung zuerst kommen, sonst wirft `figur.Name` bereits eine `NullReferenceException`:

```csharp
public void Hinzufuegen(Figur figur)
{
    ArgumentNullException.ThrowIfNull(figur);
    if (string.IsNullOrWhiteSpace(figur.Name))
    {
        throw new ArgumentException("Eine Figur braucht einen Namen.");
    }
    if (figuren.Any(f => f.Name == figur.Name))
    {
        throw new ArgumentException($"Es gibt bereits eine Figur mit dem Namen '{figur.Name}'.");
    }
    figuren.Add(figur);
}
```

Nach dem Bearbeiten teilst du Git mit `git add` mit, dass der Konflikt in dieser Datei gelöst ist, und schließt den Merge mit einem Commit ab. Git schlägt die Nachricht selbst vor:

```bash
dotnet build                                    # kompiliert es wieder?
dotnet test                                     # laufen die Tests?
git add Geometrieeditor.Fachkonzept/FigurenVerwaltung.cs
git commit
# [main 6d0f9a2] Merge branch 'feature/figur-validierung'
```

Bauen und Testen **vor** dem Commit ist kein optionaler Schritt. Ein Merge kann auch ohne Konfliktmarker fachlich falsch sein: Wenn eine Seite eine Methode umbenennt und die andere an einer ganz anderen Stelle den alten Namen aufruft, fügt Git beides klaglos zusammen – und erst der Compiler meckert.
{: .notice--warning}

Bist du mitten im Merge und merkst, dass du erst mit der anderen Person reden willst, bricht `git merge --abort` alles ab und stellt den Zustand vor dem `git merge` wieder her. Nichts geht verloren, beide Branches bleiben unverändert.

## Merge-Tools in der IDE

Konfliktmarker im Texteditor zu bearbeiten funktioniert, ist bei längeren Konflikten aber unübersichtlich. Visual Studio, Rider und VS Code bieten eine **Drei-Wege-Ansicht**: links „ours“, rechts „theirs“, in der Mitte das Ergebnis, und für jeden Block Schaltflächen, um die linke, die rechte oder beide Seiten zu übernehmen. Das Ergebnis speichert die IDE als aufgelöste Datei und führt das `git add` meist gleich mit aus; der abschließende Commit bleibt derselbe. Wie man dorthin gelangt, zeigt [Git in der IDE](/modules/git_in_der_ide/git_in_der_ide.md). Wer die Marker einmal von Hand aufgelöst hat, versteht, was die Schaltflächen tun – deshalb zuerst der Weg über die Kommandozeile.

## Konflikte vermeiden

Konflikte lassen sich nicht ganz verhindern, aber selten und klein halten:

- **Klein committen, oft pushen:** Ein Branch, der drei Tage alt ist, kollidiert eher als einer, der drei Stunden alt ist.
- **Oft pullen bzw. `main` in den Feature-Branch mergen:** Wer regelmäßig `git merge main` im Feature-Branch ausführt, löst kleine Konflikte sofort statt eines großen am Ende.
- **Absprachen im Team:** Wenn zwei Personen gleichzeitig `Home.razor` umbauen wollen, hilft ein kurzes Gespräch mehr als jedes Werkzeug. Die Schichten-Architektur aus [Vorlesung 03](/lectures/03/03.md) zahlt sich hier aus: Wer am Fachkonzept arbeitet, kommt der GUI nicht in die Quere.
- **Keine kosmetischen Massenänderungen:** Ein Commit, der alle Dateien neu formatiert, kollidiert mit jedem offenen Branch. Formatierung nur in abgesprochenen, eigenen Commits ändern.

Übung: Erzeuge in deinem Repository absichtlich einen Konflikt: Ändere in `main` die Nachricht der `ArgumentException` in `Hinzufuegen`, lege dann einen Branch vom vorigen Commit an (`git switch -c test HEAD~1`) und ändere dort dieselbe Zeile anders. Merge den Branch in `main`, sieh dir die Marker an, löse den Konflikt auf und prüfe mit `git log --oneline --graph`, dass ein Merge-Commit mit zwei Eltern entstanden ist.
{: .notice--info}

## Weitere Quellen

- [Einfaches Branching und Merging – Merge-Konflikte – Pro Git (git-scm.com)](https://git-scm.com/book/de/v2/Git-Branching-Einfaches-Branching-und-Merging#_basic_merge_conflicts)
- [Fortgeschrittenes Merging – Pro Git (git-scm.com)](https://git-scm.com/book/de/v2/Git-Tools-Fortgeschrittenes-Merging)
- [Mergekonflikte in Visual Studio auflösen – Microsoft Learn](https://learn.microsoft.com/de-de/visualstudio/version-control/git-resolve-conflicts)
