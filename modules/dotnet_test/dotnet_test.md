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

Das Kommando kennst du schon von `dotnet build` und `dotnet run` aus dem Modul [dotnet-CLI und Projekte](/modules/dotnet_cli_projekte/dotnet_cli_projekte.md). `dotnet test` baut die Solution, sucht alle Testprojekte und führt deren Tests aus:

```bash
cd examples/12_unittests/Bruch
dotnet test
```

Bei Erfolg endet die Ausgabe mit einer Zusammenfassung pro Testprojekt:

```
Passed!  - Failed:     0, Passed:     7, Skipped:     0, Total:     7, Duration: 31 ms - Bruch.Tests.dll (net10.0)
```

Sieben Tests, obwohl `BruchTests.cs` nur fünf Methoden hat? Die drei `[TestCase]`-Zeilen zählen einzeln. Schlägt ein Test fehl, steht das Ergebnis oberhalb der Zusammenfassung – mit Name, Fehlermeldung und Stacktrace, in dem die Zeile des fehlgeschlagenen `Assert.That` genannt wird:

```
  Failed Konstruktor_KuerztDenBruch [3 ms]
  Error Message:
     Assert.That(b.Zaehler, Is.EqualTo(3))
    Expected: 3
    But was:  6
  Stack Trace:
     at Bruch.Tests.BruchTests.Konstruktor_KuerztDenBruch() in .../BruchTests.cs:line 13

Failed!  - Failed:     1, Passed:     6, Skipped:     0, Total:     7, Duration: 40 ms - Bruch.Tests.dll (net10.0)
```

Das Kommando gibt außerdem einen Exit-Code ungleich 0 zurück, sobald ein Test fehlschlägt. Das interessiert dich im Terminal kaum, ist aber der Grund, warum ein Build-Server damit „rot“ werden kann.

## Filtern und mehr sehen

Bei großen Projekten will man nicht immer alles laufen lassen. `--filter` wählt Tests nach Name aus; `~` bedeutet „enthält“:

```bash
dotnet test --filter "FullyQualifiedName~Bruch"          # alle Tests, deren voller Name Bruch enthält
dotnet test --filter "Name~Plus"                         # nur Tests, deren Methodenname Plus enthält
dotnet test --filter "FullyQualifiedName~FigurenVerwaltungTests"
```

Standardmäßig zeigt `dotnet test` nur Fehlschläge und die Zusammenfassung. Mit `-v n` (Verbosity *normal*) siehst du auch die Build-Schritte, und mit `--logger "console;verbosity=normal"` listet der Test-Logger jeden einzelnen Test mit Ergebnis auf – nützlich, um zu prüfen, ob ein neuer Test überhaupt gefunden wurde.

## Der Test-Explorer in der IDE

Alle drei großen IDEs erkennen NUnit-Projekte automatisch, sobald `NUnit3TestAdapter` und `Microsoft.NET.Test.Sdk` in der `.csproj` stehen:

- **Visual Studio**: *Test → Test-Explorer* zeigt alle Tests als Baum (Projekt → Klasse → Methode). Ein Klick führt einen Test aus, Rechtsklick → *Debuggen* hält an Breakpoints im Testcode und im getesteten Code.
- **Rider**: das Fenster *Unit Tests* funktioniert genauso; zusätzlich erscheint neben jeder `[Test]`-Methode ein Symbol im Editor-Rand, über das man den Test direkt startet.
- **VS Code**: mit dem *C# Dev Kit* gibt es ein Reagenzglas-Symbol in der Seitenleiste, das dieselbe Baumansicht anbietet, und ebenfalls Schaltflächen direkt über den Testmethoden.

Grün und rot in der IDE bedeuten exakt dasselbe wie im Terminal – es ist derselbe Adapter, der die Tests ausführt. Der Vorteil der IDE liegt im Debuggen: Wenn ein Test rot ist und du nicht verstehst, warum, setzt du einen Breakpoint in die getestete Methode und lässt genau diesen einen Test im Debugger laufen. Das ist deutlich schneller, als das Programm zu starten und den Zustand von Hand nachzustellen.

## Der Rot-Grün-Zyklus in der Praxis

Egal ob Terminal oder IDE – der Arbeitsrhythmus ist immer derselbe: Test schreiben, ausführen und **rot sehen**, Code ändern, ausführen und **grün sehen**. Der rote Schritt ist keine Formalität. Er beweist, dass der Test wirklich das prüft, was du denkst. Ein typischer Anfängerfehler ist ein Test, der aus Versehen immer besteht – etwa weil eine Exception im `catch` verschluckt wird oder das Assert fehlt. Solche Tests fallen nur auf, wenn man sie einmal bewusst scheitern lässt.

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
      - run: dotnet test examples/12_unittests/Bruch
```

Nach jedem Push holt sich ein frischer Linux-Rechner den Code, installiert das SDK und führt die Tests aus. Schlägt einer fehl, wird der Commit auf GitHub rot markiert und ein Pull Request lässt sich nicht mehr guten Gewissens mergen. In GitLab CI (etwa im HTW-GitLab) sieht die Datei `.gitlab-ci.yml` sehr ähnlich aus: ein Job mit dem Image `mcr.microsoft.com/dotnet/sdk:10.0` und dem Skript `dotnet test`. So wird aus „bei mir läuft es“ ein „es läuft überall, und wir sehen es sofort“.

## Ausblick: Testabdeckung mit coverlet

Das Paket `coverlet.collector`, das das NUnit-Template mitbringt, misst auf Wunsch, welche Codezeilen von Tests überhaupt durchlaufen werden:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

Das Ergebnis landet als XML-Datei im Ordner `TestResults` und lässt sich mit Werkzeugen wie *ReportGenerator* in eine HTML-Übersicht verwandeln; Rider und Visual Studio Enterprise zeigen die Abdeckung auch direkt im Editor. Eine Abdeckung von 100 % ist kein Ziel an sich – ein durchlaufener Code ist nicht automatisch ein geprüfter Code. Aber 0 % Abdeckung in einer Klasse ist ein deutlicher Hinweis, wo Tests fehlen.

Übung: Baue absichtlich einen Fehler in `Bruch.Ggt` ein (zum Beispiel `return a == 0 ? 0 : a;`), führe `dotnet test` aus und lies aus der Ausgabe ab, welche Tests betroffen sind und warum. Starte anschließend einen davon im Debugger deiner IDE mit einem Breakpoint im Konstruktor.
{: .notice--info}

## Weitere Quellen

- [dotnet test – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/tools/dotnet-test)
- [Komponententests filtern – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/testing/selective-unit-tests)
- [Codeabdeckung mit coverlet – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/testing/unit-testing-code-coverage)
- [Tests ausführen – NUnit-Dokumentation](https://docs.nunit.org/articles/nunit/running-tests/Index.html)
