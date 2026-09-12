---
title: "Schichten-Architektur mit Blazor umsetzen"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Die [Schichten-Architektur](/modules/schichten_architektur/schichten_architektur.md) haben wir bisher als Bauplan kennengelernt: drei Schichten, vier Regeln, Zugriff nur nach unten und nur über Schnittstellen. Jetzt geht es an die Umsetzung – und dabei tauchen Fragen auf, die der Bauplan nicht beantwortet. Wie werden aus den Schichten Projekte? Woran erkennt man beim Schreiben einer Komponente, dass man gerade eine Spielregel hineinschmuggelt? Und wenn der Kern nur ein Interface `ILevelQuelle` kennt – wer erzeugt dann das konkrete Objekt, das dahintersteht? Das Adventure beantwortet alle drei Fragen, und die dritte führt uns zu einem Mechanismus, den jede ASP.NET-Core-Anwendung mitbringt: **Dependency Injection**.

## Die Solution: vier Projekte

Jede Schicht wird ein eigenes Projekt – und weil die GUI-Schicht zwei Bewohner hat, sind es vier. Die Datei `Adventure.slnx` fasst sie zusammen:

```xml
<Solution>
  <Project Path="Adventure.Daten/Adventure.Daten.csproj" />
  <Project Path="Adventure.Kern/Adventure.Kern.csproj" />
  <Project Path="Adventure.Konsole/Adventure.Konsole.csproj" />
  <Project Path="Adventure.Web/Adventure.Web.csproj" />
</Solution>
```

Angelegt wird das mit den Befehlen aus [Projekte mit der dotnet-CLI](/modules/dotnet_cli_projekte/dotnet_cli_projekte.md) – zwei Klassenbibliotheken, eine Konsolen-App, eine Blazor Web App, und dann die Projektreferenzen, die die erlaubten Abhängigkeiten festlegen:

```bash
dotnet new classlib -o Adventure.Kern
dotnet new classlib -o Adventure.Daten
dotnet new console  -o Adventure.Konsole
dotnet new blazor   -o Adventure.Web --empty -int Server -ai

dotnet add Adventure.Daten   reference Adventure.Kern
dotnet add Adventure.Konsole reference Adventure.Kern Adventure.Daten
dotnet add Adventure.Web     reference Adventure.Kern Adventure.Daten
```

Die Referenzen ergeben folgendes Bild – jeder Pfeil bedeutet „kennt und benutzt“:

```
   Adventure.Konsole                     Adventure.Web
   Program.cs: while-Schleife            Home.razor, Statusleiste, SpielEndeDialog
   ReadKey, AlsText()                    Program.cs wählt die Levelquelle aus
        │            │                        │            │
        │            └───────────┐   ┌────────┘            │
        ▼                        ▼   ▼                     ▼
   Adventure.Kern                          Adventure.Daten
   Spielobjekt, Spielfeld, Spieler         EingebauteLevelQuelle
   LevelParser, Level                             │
   ILevelQuelle   ◄───────────────────────────────┘ implementiert
```

Auffällig ist, dass **niemand** auf `Adventure.Web` oder `Adventure.Konsole` zeigt und dass der Kern auf **nichts** zeigt. Damit sind die Schichtregeln 1 und 2 vom Compiler garantiert: Wollte jemand aus der Datenschicht eine Komponente aufrufen, bräuchte er eine Referenz auf das Web-Projekt, und die wäre ein Zirkelbezug. Die Pfeile von den beiden Oberflächen zur Datenschicht sind die eine bewusste Ausnahme; wozu sie nötig sind, sehen wir gleich in `Program.cs`.

## Zwei Oberflächen als Beweis

Die eigentliche Prüfung einer Schichtung ist nicht das Diagramm, sondern der Versuch. Und der ist in diesem Projekt jederzeit machbar:

```bash
dotnet run --project Adventure.Konsole    # ASCII-Karte im Terminal, Pfeiltasten oder WASD
dotnet run --project Adventure.Web        # dieselbe Karte im Browser, mit Emojis
```

Beide Programme spielen dasselbe Spiel mit denselben Regeln, und im Kern gibt es **keine einzige Zeile**, die weiß, welche der beiden Oberflächen gerade läuft – kein `if (istWeb)`, keine Konsolenausgabe, kein HTML. Das ist kein hübsches Extra, sondern ein Testverfahren: Jede Spielregel, die nur in einer der beiden Versionen funktioniert, liegt am falschen Ort. Wer die Schichtung überprüfen will, baut einfach die zweite Oberfläche – sie findet die Verstöße von selbst.

Ein Gegenbeispiel macht das konkret. Nehmen wir an, jemand ergänzt in `TasteGedrueckt` eine Zeile, die vor dem Zug prüft, ob das Zielfeld eine Wand ist, und dann den Zug unterdrückt. Die Browserversion verhält sich danach sinnvoll – und die Konsolenversion nicht, weil sie diese Prüfung nicht kennt. Die Regel ist dupliziert worden, statt an einer Stelle zu leben. Die Reparatur ist immer dieselbe: Die Prüfung wandert in `Spielfeld.SpielerZieht` (dort gibt es sie längst, als `IstFrei`), und beide Oberflächen profitieren.
{: .notice--warning}

## Die Komponente enthält keine Spielregeln

Die wichtigste Regel für die GUI-Schicht ist leicht zu formulieren und schwer durchzuhalten: Eine Komponente **liest Eingaben, ruft das Fachkonzept und zeigt das Ergebnis an** – mehr nicht. In `Home.razor` sehen die Handler deshalb erschreckend leer aus:

```csharp
private void NeuStarten()
{
    feld = LevelParser.Parsen(LevelQuelle.Laden(levelName));
}

private void TasteGedrueckt(KeyboardEventArgs e)
{
    Richtung? richtung = e.Key switch
    {
        "ArrowUp" or "w" or "W" => Richtung.Oben,
        "ArrowDown" or "s" or "S" => Richtung.Unten,
        "ArrowLeft" or "a" or "A" => Richtung.Links,
        "ArrowRight" or "d" or "D" => Richtung.Rechts,
        _ => null
    };
    if (richtung is Richtung r)
    {
        feld.SpielerZieht(r);   // danach rendert Blazor die Komponente automatisch neu
    }
}
```

Das ist der gesamte aktive Code der Seite. `TasteGedrueckt` **übersetzt** – von der Browserwelt (`"ArrowUp"`) in die Spielwelt (`Richtung.Oben`) – und delegiert dann. Nirgends steht hier, ob man durch eine Tür gehen darf, wie viel Schaden eine Wache macht oder wann das Spiel gewonnen ist. Die Komponente erfährt das Ergebnis einer Runde ausschließlich daran, dass sich `feld.Status`, `feld.LetzteMeldung` und die Positionen der Objekte geändert haben.

Was in die Komponente gehört: die Übersetzung von Tasten in Richtungen, die Auswahl von Emojis und CSS-Klassen, die Herzendarstellung, das Feld `levelName`, das Ein- und Ausblenden des Dialogs. Was nicht hineingehört: alles, was eine Regel des Spiels ist. Im Zweifel in den Kern – dorthin kann man es testen, aus der Komponente nicht.
{: .notice--primary}

Die restlichen Mitglieder des `@code`-Blocks – `SymbolFuer`, `KlasseFuer`, `OnInitialized`, `OnAfterRenderAsync` – sind reine Darstellung. Nicht zufällig sind `SymbolFuer` und `KlasseFuer` **statisch**: Eine Methode, die nur ihr Argument in eine Zeichenkette übersetzt, braucht keinen Zugriff auf den Zustand der Komponente. Wenn ein Handler dagegen anfängt, mehrere Objekte zu vergleichen und Bedingungen zu verknüpfen, ist das ein zuverlässiges Warnsignal.

## `ILevelQuelle`: der Kern bestimmt, die Datenschicht liefert

Schichtregel 4 verlangt, dass eine Schicht die darunterliegende nur über Schnittstellen benutzt. Der Kern legt daher selbst fest, was er von einer Levelquelle braucht – die beiden Typen stehen in `Adventure.Kern/Level.cs`:

```csharp
namespace Adventure.Kern;

/// <summary>Ein Level ist eine Karte aus Textzeilen plus ein Name.</summary>
public record Level(string Name, IReadOnlyList<string> Zeilen);

/// <summary>Woher die Level kommen, ist dem Spiel egal – Datei, Netz oder fest im Code.</summary>
public interface ILevelQuelle
{
    IReadOnlyList<string> LevelNamen { get; }
    Level Laden(string name);
}
```

Zwei Mitglieder, mehr braucht es nicht: eine Liste der verfügbaren Namen für die Auswahl und eine Methode, die zu einem Namen die Karte liefert. Die Datenschicht implementiert diesen Vertrag – vorerst mit zwei fest einprogrammierten Karten:

```csharp
using Adventure.Kern;

namespace Adventure.Daten;

/// <summary>Zwei Level fest im Code – solange wir noch keine Dateien lesen können.</summary>
public class EingebauteLevelQuelle : ILevelQuelle
{
    private static readonly Dictionary<string, string[]> level = new()
    {
        ["Kerker"] = new[]
        {
            "####################",
            "#@.....#...........#",
            // ... weitere Zeilen ...
            "####################",
        },
        // ["Katakomben"] = ...
    };

    public IReadOnlyList<string> LevelNamen => level.Keys.ToList();

    public Level Laden(string name)
    {
        if (!level.TryGetValue(name, out string[]? zeilen))
        {
            throw new KeyNotFoundException($"Es gibt kein Level namens '{name}'.");
        }
        return new Level(name, zeilen);
    }
}
```

Man beachte die Richtung: Das Interface liegt im Projekt `Adventure.Kern`, und `Adventure.Daten` verweist *auf den Kern*, nicht umgekehrt. Dieses Prinzip heißt **Dependency Inversion**: Die Abhängigkeit zeigt zur Abstraktion im Fachkonzept, und deshalb bleibt die Schichtregel „nur über Schnittstellen“ erfüllt, obwohl die Datenhaltung die tiefere Schicht ist. Der Gewinn zeigt sich in [Vorlesung 09](/lectures/09/09.md): Dort kommen eine `TextdateiLevelQuelle` und eine `HttpLevelQuelle` dazu, und weder der Kern noch eine Komponente ändert sich dafür.

## Dependency Injection: wer erzeugt die Levelquelle?

Bleibt die Frage, wer `new EingebauteLevelQuelle()` schreibt. Der Kern darf es nicht, sonst würde er die Datenschicht kennen. Die Komponente sollte es nicht, sonst müsste jede Seite wissen, welche Quelle gerade gilt – und ein Wechsel wäre eine Suchen-und-Ersetzen-Aktion. ASP.NET Core sieht dafür genau eine Stelle vor, den **DI-Container** in `Program.cs`:

```csharp
// Die eine Stelle, an der entschieden wird, woher die Level kommen.
builder.Services.AddSingleton<ILevelQuelle, EingebauteLevelQuelle>();
```

Die Zeile registriert: „Wer einen `ILevelQuelle` braucht, bekommt eine `EingebauteLevelQuelle`.“ Das ist **Dependency Injection**: Objekte bekommen ihre Abhängigkeiten von außen geliefert, statt sie selbst zu erzeugen. Deshalb braucht das Web-Projekt überhaupt eine Referenz auf `Adventure.Daten` – nur für diese eine Zeile. Die Konsolenversion hat kein `Program.cs` mit Container und schreibt die Entscheidung direkt hin (`ILevelQuelle levelQuelle = new EingebauteLevelQuelle();`) – auch das ist in Ordnung, denn es ist wieder *eine* Stelle, ganz oben im Programm.

In der Komponente holt eine Anweisung das fertige Objekt ab:

```razor
@page "/"
@using Adventure.Kern
@inject ILevelQuelle LevelQuelle
```

Ab da steht `LevelQuelle` in Markup und `@code` als Property zur Verfügung – die `@foreach`-Schleife der Levelauswahl liest `LevelQuelle.LevelNamen`, `NeuStarten` ruft `LevelQuelle.Laden(levelName)`. Entscheidend ist der Typ: `@inject ILevelQuelle` nennt das **Interface**, nicht die Klasse. Die Komponente könnte gar nicht bemerken, wenn morgen eine Datei- oder Webquelle dahinterstünde.

**Singleton, Scoped oder Transient?** `AddSingleton` erzeugt ein Objekt für den ganzen Server, `AddScoped` in Blazor eines pro Browser-Verbindung (Circuit), `AddTransient` bei jedem `@inject` ein neues. Für die Levelquelle ist `Singleton` richtig, weil sie nur liest und keinen benutzerabhängigen Zustand hat – zwei Spieler dürfen sich dieselben Karten teilen. Das `Spielfeld` steht dagegen bewusst **nicht** im Container: Es ist ein Feld der Komponente, und damit hat jede Browser-Sitzung automatisch ihr eigenes Spiel.
{: .notice--primary}

## Was man damit gewinnt

Drei Dinge, die ohne die Trennung nicht gingen. Erstens lässt sich die Levelquelle **austauschen, ohne eine Komponente anzufassen** – eine Zeile in `Program.cs`. Zweitens lässt sich der Kern **ohne Browser testen**: Ein NUnit-Test baut sich ein `Spielfeld` mit dem `LevelParser`, ruft `SpielerZieht` und prüft `Status`, `Lebenspunkte` oder `Punkte`, ohne dass eine Seite gerendert wird. Genau das holen wir in [Vorlesung 12](/lectures/12/12.md) nach; warum das so wertvoll ist, vertieft das Modul [Warum Unit-Tests?](/modules/unit_tests_motivation/unit_tests_motivation.md). Und drittens konnte diese ganze Vorlesung existieren: Eine neue Oberfläche zu bauen war möglich, *ohne* das Spiel neu zu schreiben.

Übung: Das Spiel soll einen Spielstand speichern und laden können. Schreibe auf, was in welches Projekt kommt: das Interface für den Speicher, die Klasse, die tatsächlich in eine Datei schreibt, die Buttons „Speichern“ und „Laden“, die Methode, die den Zustand des Spielfelds einsammelt, und die Zeile, die entscheidet, in welche Datei geschrieben wird. Prüfe deine Verteilung mit der Konsolen-Probe – und überlege, welche der fünf Teile die Konsolenversion mitbenutzen könnte.
{: .notice--info}

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v04-blazor`).

## Weitere Quellen

- [Dependency Injection in ASP.NET Core Blazor – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/fundamentals/dependency-injection)
- [Abhängigkeitsinjektion in .NET – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/extensions/dependency-injection)
- [dotnet sln – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/tools/dotnet-sln)
