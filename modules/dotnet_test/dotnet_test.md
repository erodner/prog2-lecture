---
title: "Tests ausführen: dotnet test und die IDE"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Ein Test, der nie läuft, ist nur Text. Der ganze Wert von Unit-Tests entsteht erst dadurch, dass sie ständig ausgeführt werden – nach jeder Änderung, vor jedem Commit, auf dem Server nach jedem Push. Dafür gibt es zwei Wege, die sich ergänzen: `dotnet test` im Terminal, das ohne IDE auskommt und deshalb auch auf einem Build-Server läuft, und der Test-Explorer der IDE, der einzelne Tests per Klick startet und beim Debuggen hilft.

## `dotnet test` im Terminal

Das Kommando kennst du schon von `dotnet build` und `dotnet run` aus dem Modul [dotnet-CLI und Projekte](/modules/dotnet_cli_projekte/dotnet_cli_projekte.md). `dotnet test` baut die Solution, sucht alle Testprojekte darin und führt deren Tests aus. Im Adventure-Repository genügt der Aufruf im Wurzelverzeichnis:

```bash
cd prog2-adventure
dotnet test
```

Bei Erfolg endet die Ausgabe mit einer Zusammenfassung pro Testprojekt:

```
Passed!  - Failed:     0, Passed:    13, Skipped:     0, Total:    13, Duration: 96 ms - Adventure.Tests.dll (net10.0)
```

Dreizehn Tests in unter einer Zehntelsekunde – neun aus `SpielfeldTests`, vier aus `DatenTests`. In dieser Zeit hat der Rechner unter anderem eine Tür aufgeschlossen, eine Wache gegen eine Wand laufen lassen, zwei komplette Level geparst, einen Spielstand als JSON geschrieben und wieder eingelesen. Von Hand wären das zwanzig Minuten Tastendrücken. Schlägt ein Test fehl, steht das Ergebnis oberhalb der Zusammenfassung – mit Name, Fehlermeldung und Stacktrace, in dem die Zeile des fehlgeschlagenen `Assert.That` genannt wird:

```
  Failed Tuer_Ohne_Schluessel_Bleibt_Zu [5 ms]
  Error Message:
     Assert.That(f.Spieler.Position, Is.EqualTo(new Position(0, 0)))
    Expected: (0, 0)
    But was:  (1, 0)
  Stack Trace:
     at Adventure.Tests.SpielfeldTests.Tuer_Ohne_Schluessel_Bleibt_Zu() in .../SpielfeldTests.cs:line 38

Failed!  - Failed:     1, Passed:    12, Skipped:     0, Total:    13, Duration: 88 ms - Adventure.Tests.dll (net10.0)
```

Der Name allein sagt schon, was los ist: Der Held steht hinter der Tür, obwohl er keinen Schlüssel hat. Das Kommando gibt außerdem einen Exit-Code ungleich 0 zurück, sobald ein Test fehlschlägt. Das interessiert dich im Terminal kaum, ist aber der Grund, warum ein Build-Server damit „rot“ werden kann.

## Filtern und mehr sehen

Bei größeren Projekten will man nicht immer alles laufen lassen – und beim Suchen eines Fehlers stört jede Ausgabe, die nichts mit ihm zu tun hat. `--filter` wählt Tests nach Name aus; `~` bedeutet „enthält“:

```bash
dotnet test --filter "FullyQualifiedName~Spielfeld"   # nur die Tests aus SpielfeldTests
dotnet test --filter "FullyQualifiedName~Daten"       # nur die Tests aus DatenTests
dotnet test --filter "Name~Tuer"                      # nur Tests, deren Methodenname Tuer enthält
```

Der volle Name eines Tests ist `Namespace.Klasse.Methode`, also etwa `Adventure.Tests.SpielfeldTests.Wache_Dreht_Um` – deshalb trifft `FullyQualifiedName~Spielfeld` die ganze Klasse und `Name~Tuer` beide Tür-Tests quer über die Klassen. Hier zahlt sich die Namensdisziplin aus dem Modul [NUnit-Testprojekt](/modules/nunit_testprojekt/nunit_testprojekt.md) ein zweites Mal aus: Wer seine Tests nach dem geprüften Spielobjekt benennt, kann sie auch danach filtern.

Standardmäßig zeigt `dotnet test` nur Fehlschläge und die Zusammenfassung. Mit `-v n` (Verbosity *normal*) siehst du auch die Build-Schritte, und mit `--logger "console;verbosity=normal"` listet der Test-Logger jeden einzelnen Test mit Ergebnis auf – nützlich, um zu prüfen, ob ein neuer Test überhaupt gefunden wurde. Mit `dotnet test Adventure.Tests` beschränkst du den Lauf auf ein einzelnes Projekt.

## Der Test-Explorer in der IDE

Alle drei großen IDEs erkennen NUnit-Projekte automatisch, sobald `NUnit3TestAdapter` und `Microsoft.NET.Test.Sdk` in der `.csproj` stehen:

- **Visual Studio**: *Test → Test-Explorer* zeigt alle Tests als Baum (Projekt → Klasse → Methode). Ein Klick führt einen Test aus, Rechtsklick → *Debuggen* hält an Breakpoints im Testcode und im getesteten Code.
- **Rider**: das Fenster *Unit Tests* funktioniert genauso; zusätzlich erscheint neben jeder `[Test]`-Methode ein Symbol im Editor-Rand, über das man den Test direkt startet.
- **VS Code**: mit dem *C# Dev Kit* gibt es ein Reagenzglas-Symbol in der Seitenleiste, das dieselbe Baumansicht anbietet, und ebenfalls Schaltflächen direkt über den Testmethoden.

Grün und rot in der IDE bedeuten exakt dasselbe wie im Terminal – es ist derselbe Adapter, der die Tests ausführt. Der Vorteil der IDE liegt im Debuggen: Wenn `Verfolger_Sieht_Nicht_Durch_Waende` rot ist und du nicht verstehst, warum, setzt du einen Breakpoint in `Spielfeld.HatSichtlinie` und lässt genau diesen einen Test im Debugger laufen. Nach zwei Schritten siehst du, welches Feld der Bresenham-Algorithmus gerade prüft. Das ist deutlich schneller, als das Spiel zu starten und die Situation mit Pfeiltasten nachzustellen – zumal du dafür erst einmal ein Level bräuchtest, in dem ein Verfolger hinter einer Wand steht.

## Der Rot-Grün-Zyklus in der Praxis

Egal ob Terminal oder IDE – der Arbeitsrhythmus ist immer derselbe: Test schreiben, ausführen und **rot sehen**, Code ändern, ausführen und **grün sehen**. Machen wir das mit der Falle aus der Motivation, einem Feld, das beim Betreten einen Lebenspunkt kostet. Zuerst der Test, noch bevor es die Klasse gibt:

```csharp
[Test] public void Falle_Kostet_Lebenspunkt()
{
    Spielfeld f = Feld("@X");
    f.SpielerZieht(Richtung.Rechts);
    Assert.That(f.Spieler.Lebenspunkte, Is.EqualTo(2));
    Assert.That(f.Spieler.Position, Is.EqualTo(new Position(1, 0)));
}
```

Der erste Lauf ist rot – aber nicht wegen eines fehlgeschlagenen `Assert.That`, sondern mit einer `ArgumentException` aus dem `LevelParser`: „Unbekanntes Zeichen 'X'“. Auch das ist ein nützliches Rot, denn es sagt genau, welcher Schritt fehlt. Jetzt entsteht die Klasse `Falle : StatischesObjekt` mit `IstPassierbar => true` und einer Methode `Ausloesen`, dazu ein Fall `'X' => new Falle(pos)` im Parser und eine Zeile in `Spielfeld.SpielerZieht`, die nach einer erfolgreichen Bewegung prüft, ob unter dem Helden eine Falle liegt. Der zweite Lauf:

```
Passed!  - Failed:     0, Passed:    14, Skipped:     0, Total:    14, Duration: 101 ms - Adventure.Tests.dll (net10.0)
```

Vierzehn statt dreizehn – und genau darin liegt der eigentliche Gewinn: Die dreizehn alten Tests sind mitgelaufen. Die Frage „Habe ich mit der neuen Falle die Tür kaputt gemacht?“ ist damit beantwortet, ohne dass jemand ein Level durchspielen musste. Hättest du beim Eingriff in `SpielerZieht` versehentlich die Reihenfolge von Bewegung und Interaktion vertauscht, wäre `Schluessel_Aufheben_Und_Tuer_Oeffnen` rot geworden.

Wenn ein Test nach einer Änderung rot wird, gibt es genau zwei Möglichkeiten: Der Code ist falsch oder der Test ist falsch. Beides kommt vor – aber die Antwort darf nie sein, das Assert so lange anzupassen, bis es grün wird, ohne zu verstehen, warum. Ein Test, der nur den aktuellen Zustand abschreibt, prüft nichts mehr.
{: .notice--warning}

## Ausblick: Tests auf dem Server (CI)

Weil `dotnet test` ohne IDE läuft, kann es auch ein Server ausführen – und zwar automatisch nach jedem Push, wie wir es beim [Arbeiten mit Remotes](/modules/git_remote/git_remote.md) kennengelernt haben. Das nennt man **Continuous Integration (CI)**. Bei GitHub Actions reicht dafür eine kleine Datei `.github/workflows/tests.yml` im Repository:

```yaml
name: Tests
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - run: dotnet test --verbosity normal
```

Nach jedem Push holt sich ein frischer Linux-Rechner den Code, installiert das SDK und führt die dreizehn Tests aus. Schlägt einer fehl, wird der Commit auf GitHub rot markiert und ein Pull Request lässt sich nicht mehr guten Gewissens mergen. Dass das Adventure eine Konsolen- *und* eine Web-Oberfläche hat, spielt dabei keine Rolle: Getestet wird der Kern, und der braucht weder Bildschirm noch Tastatur. In GitLab CI (etwa im HTW-GitLab) sieht die Datei `.gitlab-ci.yml` sehr ähnlich aus: ein Job mit dem Image `mcr.microsoft.com/dotnet/sdk:10.0` und dem Skript `dotnet test`. So wird aus „bei mir läuft es“ ein „es läuft überall, und wir sehen es sofort“.

## Ausblick: Testabdeckung mit coverlet

Das Paket `coverlet.collector`, das das NUnit-Template mitbringt, misst auf Wunsch, welche Codezeilen von Tests überhaupt durchlaufen werden:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

Das Ergebnis landet als XML-Datei im Ordner `TestResults` und lässt sich mit Werkzeugen wie *ReportGenerator* in eine HTML-Übersicht verwandeln; Rider und Visual Studio Enterprise zeigen die Abdeckung auch direkt im Editor. Für das Adventure ist der Blick lehrreich: `Spielfeld` und `Tuer` sind gut abgedeckt, `Adventure.Konsole` und `Adventure.Web` gar nicht – genau so soll es sein, denn Oberflächen prüft man nicht mit Unit-Tests. Eine Abdeckung von 100 % ist kein Ziel an sich; durchlaufener Code ist nicht automatisch geprüfter Code. Aber 0 % in einer Klasse des Kerns ist ein deutlicher Hinweis, wo Tests fehlen.

Übung: Ändere in `Verfolger` die Sichtweite von 5 auf 1 und führe `dotnet test --filter "FullyQualifiedName~Spielfeld"` aus. Welche Tests werden rot, und reichen ihre Namen, um den Fehler zu finden? Mache die Änderung rückgängig, drehe stattdessen in `Tuer.Interagieren` die Bedingung `!spieler.Inventar.Enthaelt<Schluessel>()` um und wiederhole den Lauf. Starte anschließend einen der roten Tests im Debugger deiner IDE mit einem Breakpoint in `Interagieren`.
{: .notice--info}

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v12-tests`).

## Weitere Quellen

- [dotnet test – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/tools/dotnet-test)
- [Komponententests filtern – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/testing/selective-unit-tests)
- [Codeabdeckung mit coverlet – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/testing/unit-testing-code-coverage)
- [Tests ausführen – NUnit-Dokumentation](https://docs.nunit.org/articles/nunit/running-tests/Index.html)
