---
title: "Branches und Merges"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Stell dir vor, du ergänzt im Adventure eine neue Objektart `Falle` und bist mittendrin – die Methode `Ausloesen` ist fertig, das überschriebene `Symbol` fehlt noch, und weil die abstrakte Property nicht implementiert ist, kompiliert das Projekt nicht. Genau jetzt meldet eine Kommilitonin einen Fehler in `Spieler.Heilen`, der schnell behoben werden muss. Ohne Branches müsstest du deinen halbfertigen Umbau irgendwie beiseitelegen oder den Bugfix mit deinem Chaos zusammen committen. Mit Branches arbeitest du am Feature in einem eigenen Zweig, wechselst kurz auf `main`, behebst den Fehler, und kehrst zurück. Der Hauptzweig bleibt dabei zu jedem Zeitpunkt baubar. Branches sind in Git so billig – ein Zeiger auf einen Commit, wie wir in [Git-Konzepte](/modules/git_konzepte/git_konzepte.md) gesehen haben –, dass man sie für jedes Feature und jeden Bugfix anlegt.

## Branches anlegen und wechseln

`git branch` ohne Argument listet die Branches, der Stern markiert den aktuellen. `git switch -c` legt einen neuen Branch an und wechselt sofort hinein:

```bash
git branch
# * main
git switch -c feature/falle
# Switched to a new branch 'feature/falle'
git branch
#   main
# * feature/falle
```

Der neue Branch zeigt zunächst auf denselben Commit wie `main` – nichts hat sich an den Dateien geändert. Erst der nächste Commit lässt die beiden auseinanderlaufen. Wir legen `Falle.cs` an – eine weitere Unterklasse von `StatischesObjekt` nach dem Muster von `Wand` und `Tuer` – und committen:

```csharp
public sealed class Falle : StatischesObjekt
{
    public int Schaden { get; }
    public bool IstAusgeloest { get; private set; }

    public Falle(Position position, int schaden = 1) : base("Falle", position) => Schaden = schaden;

    public override char Symbol => IstAusgeloest ? '^' : '.';
    public override bool IstPassierbar => true;

    public string Ausloesen(Spieler spieler)
    {
        if (IstAusgeloest) return "";
        IstAusgeloest = true;
        spieler.SchadenNehmen(Schaden);
        return $"Eine Falle! {spieler.Name} verliert {Schaden} Lebenspunkt(e).";
    }
}
```

Die Falle sieht aus wie Boden (`.`), bis jemand hineintritt – deshalb ist sie passierbar und verrät sich erst durch das Symbol `^`. Sie ist unsere Erweiterung für dieses Modul; den Stand, auf dem sie aufsetzt, findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v02-interfaces`). Die neue Datei ist Git noch unbekannt, also braucht sie ein `git add`:

```bash
git add Adventure.Kern/Falle.cs
git commit -m "Falle als neue Objektart mit Schaden beim Betreten ergänzen"
# [feature/falle 5c21a8f] Falle als neue Objektart mit Schaden beim Betreten ergänzen
git log --oneline --all
# 5c21a8f (HEAD -> feature/falle) Falle als neue Objektart mit Schaden beim Betreten ergänzen
# a1f9c3e (main) Spielfeld: Objekte wieder vom Feld entfernen können
```

`HEAD` ist Gits Bezeichnung für „der Branch, in dem ich gerade bin“. Jetzt kommt der Bugfix dazwischen. Wir wechseln zu `main` – Git tauscht dabei die Dateien in der Arbeitskopie aus, `Falle.cs` verschwindet vorübergehend – und beheben den Fehler dort:

```bash
git switch main
# Switched to branch 'main'
# ... Spieler.cs korrigieren: Heilen mit Math.Min begrenzen ...
git commit -am "Spieler: Heilen auf MaxLebenspunkte begrenzen"
git switch feature/falle               # zurück zum Feature, Falle.cs ist wieder da
```

`git commit -a` staged alle bereits verfolgten, geänderten Dateien automatisch – praktisch für kleine Fixes, aber neue Dateien braucht weiterhin ein `git add`. Ein Wechsel funktioniert nur, wenn die Arbeitskopie sauber ist oder die Änderungen nicht mit dem Ziel kollidieren; sonst verlangt Git, dass du erst committest oder mit `git stash` beiseitelegst.

## Zusammenführen: Fast-Forward

Ist das Feature fertig, kommt es zurück nach `main`. Dazu wechselt man in den Branch, der die Änderungen **aufnehmen** soll, und ruft `git merge` mit dem Namen des Branches auf, der **hineinfließen** soll. Der einfachste Fall tritt ein, wenn `main` sich seit dem Abzweigen nicht bewegt hat:

```
 vorher:                            nachher (Fast-Forward):

 C2 ◄── C3 ◄── C5                   C2 ◄── C3 ◄── C5
        ▲      ▲                                  ▲
       main   feature                        main, feature
```

Git muss nichts zusammenführen – es schiebt den Zeiger `main` einfach nach vorn auf `C5`, weil `C5` alle Commits von `main` bereits enthält. Die Ausgabe nennt das *Fast-forward*; genau das haben wir beim `git pull` in [Remote-Repositorys](/modules/git_remote/git_remote.md) schon gesehen.

## Zusammenführen: Merge-Commit

In unserem Szenario hat `main` sich aber bewegt: Der Bugfix an `Spieler.Heilen` liegt als `C4` dort, das Feature `C5` im anderen Zweig. Beide haben `C3` als gemeinsamen Vorfahren. Git erzeugt jetzt einen neuen **Merge-Commit** mit zwei Eltern, der die Änderungen beider Seiten enthält:

```
 vorher:                            nachher (Merge-Commit):

 C2 ◄── C3 ◄── C4                   C2 ◄── C3 ◄── C4 ◄── M
         ▲     ▲                            ▲            ▲  ▲
         │    main                          │          main │
         └──── C5                           └──── C5 ◄──────┘
               ▲                                  ▲
            feature                            feature
```

```bash
git switch main
git merge feature/falle
# Merge made by the 'ort' strategy.
#  Adventure.Kern/Falle.cs | 19 +++++++++++++++++++
#  1 file changed, 19 insertions(+)
git log --oneline --graph --all
# *   b90e2d1 (HEAD -> main) Merge branch 'feature/falle'
# |\
# | * 5c21a8f (feature/falle) Falle als neue Objektart mit Schaden beim Betreten ergänzen
# * | 3e7f0c2 Spieler: Heilen auf MaxLebenspunkte begrenzen
# |/
# * a1f9c3e Spielfeld: Objekte wieder vom Feld entfernen können
```

Die Ausgabe von `git log --oneline --graph --all` ist das Werkzeug, um sich jederzeit den Graphen anzusehen: Jeder Stern ist ein Commit, die Linien zeigen die Elternbeziehungen, die Namen in Klammern die Branch-Zeiger. Dass Git die beiden Änderungen hier ohne Nachfrage zusammenfügen konnte, liegt daran, dass sie unterschiedliche Dateien betreffen. Ändern beide Seiten dieselben Zeilen, entsteht ein Konflikt – das Thema von [Merge-Konflikte lösen](/modules/git_merge_konflikte/git_merge_konflikte.md).

Nach dem Merge hat der Feature-Branch seinen Zweck erfüllt und wird gelöscht. Das entfernt nur den Zeiger, keine Commits – sie sind über `main` weiterhin erreichbar:

```bash
git branch -d feature/falle
# Deleted branch feature/falle (was 5c21a8f).
git push origin --delete feature/falle      # falls er auch auf dem Server lag
```

`git branch -d` weigert sich, einen Branch zu löschen, der noch nicht gemergte Commits enthält – eine Sicherung gegen Datenverlust. Wer die Commits wirklich verwerfen will, braucht `-D`.
{: .notice--primary}

## Der Feature-Branch-Workflow

Das Muster aus diesem Modul ist der in Teams verbreitetste Arbeitsablauf:

1. `main` ist immer baubar und getestet; niemand committet direkt hinein.
2. Für jede Aufgabe entsteht ein Branch mit sprechendem Namen: `feature/bogenschuetze`, `bugfix/wache-laeuft-durch-wand`.
3. Der Branch wird regelmäßig gepusht (`git push -u origin feature/...`) – als Sicherung und damit andere ihn sehen.
4. Ist die Arbeit fertig, wird sie nicht lokal gemergt, sondern als **Merge Request** (GitLab) bzw. **Pull Request** (GitHub) auf dem Server eröffnet.
5. Eine zweite Person liest den Diff, kommentiert einzelne Zeilen, bittet um Änderungen oder stimmt zu. Automatische Tests laufen (dazu mehr in [Vorlesung 12](/lectures/12/12.md)).
6. Der Merge Request wird über die Weboberfläche gemergt, der Branch dabei gelöscht.

Der Merge Request ist dabei kein Git-Befehl, sondern eine Funktion der Plattform: eine Diskussionsseite um einen geplanten Merge herum. Der Review-Schritt ist der eigentliche Gewinn – vier Augen sehen mehr als zwei, und man erklärt seine Änderung einmal in Worten, bevor sie Teil des Projekts wird.

Übung: Lege in deinem Repository einen Branch `feature/gegner-zaehlen` an, ergänze im `Spielfeld` eine Property `AnzahlGegner` und committe. Wechsle zu `main`, ändere dort die `README.md`, committe, und merge das Feature. Sieh dir das Ergebnis mit `git log --oneline --graph --all` an. Wiederhole das Ganze so, dass ein Fast-Forward entsteht – was musst du anders machen?
{: .notice--info}

## Weitere Quellen

- [Branches auf einen Blick – Pro Git (git-scm.com)](https://git-scm.com/book/de/v2/Git-Branching-Branches-auf-einen-Blick)
- [Einfaches Branching und Merging – Pro Git (git-scm.com)](https://git-scm.com/book/de/v2/Git-Branching-Einfaches-Branching-und-Merging)
- [Merge requests – GitLab-Dokumentation](https://docs.gitlab.com/user/project/merge_requests/)
