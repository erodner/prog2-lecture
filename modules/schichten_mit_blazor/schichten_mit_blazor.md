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

<div class="editoren" markdown="1">
<div class="editor" markdown="1">
<div class="editor-titel"><a href="https://github.com/erodner/prog2-adventure/blob/v03-blazor/Adventure.slnx">Adventure.slnx</a></div>

```xml
<Solution>
  <Project Path="Adventure.Daten/Adventure.Daten.csproj" />
  <Project Path="Adventure.Kern/Adventure.Kern.csproj" />
  <Project Path="Adventure.Konsole/Adventure.Konsole.csproj" />
  <Project Path="Adventure.Web/Adventure.Web.csproj" />
</Solution>
```

</div>
</div>

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

Die Referenzen ergeben folgendes Bild:

<svg viewBox="0 0 700 320" role="img" aria-labelledby="titel-projekte" xmlns="http://www.w3.org/2000/svg" style="max-width:100%;height:auto;font-family:system-ui,sans-serif"><title id="titel-projekte">Projektabhängigkeiten des Adventure: Adventure.Konsole und Adventure.Web kennen und benutzen Adventure.Kern und Adventure.Daten. Adventure.Daten implementiert das Interface ILevelQuelle aus Adventure.Kern. Auf Konsole und Web zeigt kein Pfeil, Kern zeigt auf nichts.</title><defs><marker id="pfeil-proj" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="7" markerHeight="7" orient="auto-start-reverse"><path d="M 0 0 L 10 5 L 0 10 z" fill="currentColor"/></marker><marker id="real-proj" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="10" markerHeight="10" markerUnits="userSpaceOnUse" orient="auto"><path d="M0 0 L10 5 L0 10 Z" fill="none" stroke="currentColor" stroke-width="1.5" stroke-dasharray="none" stroke-linejoin="round"/></marker></defs><g fill="currentColor" fill-opacity="0.06" stroke="currentColor" stroke-width="1.5"><rect x="30" y="20" width="270" height="84" rx="4"/><rect x="400" y="20" width="270" height="84" rx="4"/><rect x="30" y="200" width="270" height="84" rx="4"/><rect x="400" y="200" width="270" height="84" rx="4"/></g><g stroke="currentColor" stroke-width="1.5" fill="none" marker-end="url(#pfeil-proj)"><line x1="165" y1="104" x2="165" y2="198"/><line x1="535" y1="104" x2="535" y2="198"/><line x1="255" y1="104" x2="445" y2="198"/><line x1="445" y1="104" x2="255" y2="198"/></g><line x1="398" y1="262" x2="304" y2="262" stroke="currentColor" stroke-width="1.5" stroke-dasharray="5 4" marker-end="url(#real-proj)"/><g fill="currentColor" text-anchor="middle"><text x="165" y="46" font-size="14" font-weight="600" font-family="ui-monospace,monospace">Adventure.Konsole</text><text x="165" y="68" font-size="12">Program.cs: Spielschleife</text><text x="165" y="86" font-size="12">ReadKey, AlsText()</text><text x="535" y="46" font-size="14" font-weight="600" font-family="ui-monospace,monospace">Adventure.Web</text><text x="535" y="68" font-size="12">Home.razor, Statusleiste, SpielEndeDialog</text><text x="535" y="86" font-size="12">Program.cs wählt die Levelquelle</text><text x="165" y="226" font-size="14" font-weight="600" font-family="ui-monospace,monospace">Adventure.Kern</text><text x="165" y="248" font-size="12">Spielobjekt, Spielfeld, Spieler</text><text x="165" y="266" font-size="12">LevelParser, Level, ILevelQuelle</text><text x="535" y="226" font-size="14" font-weight="600" font-family="ui-monospace,monospace">Adventure.Daten</text><text x="535" y="248" font-size="12">EingebauteLevelQuelle</text></g><g fill="currentColor" font-size="12"><line x1="30" y1="306" x2="62" y2="306" stroke="currentColor" stroke-width="1.5" marker-end="url(#pfeil-proj)"/><text x="70" y="310">kennt und benutzt (Projektreferenz)</text><line x1="330" y1="306" x2="362" y2="306" stroke="currentColor" stroke-width="1.5" stroke-dasharray="5 4" marker-end="url(#real-proj)"/><text x="372" y="310">implementiert <tspan font-family="ui-monospace,monospace">ILevelQuelle</tspan></text></g></svg>

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

Schichtregel 4 verlangt, dass eine Schicht die darunterliegende nur über Schnittstellen benutzt. Der Kern legt daher selbst fest, was er von einer Levelquelle braucht – die beiden Typen stehen in `Adventure.Kern/Level.cs` (links). Zwei Mitglieder genügen: eine Liste der verfügbaren Namen für die Auswahl und eine Methode, die zu einem Namen die Karte liefert. Rechts implementiert die Datenschicht diesen Vertrag, vorerst mit zwei fest einprogrammierten Karten:

<div class="editoren" markdown="1">
<div class="editor" markdown="1">
<div class="editor-titel"><a href="https://github.com/erodner/prog2-adventure/blob/v03-blazor/Adventure.Kern/Level.cs">Adventure.Kern/Level.cs</a></div>

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

</div>
<div class="editor" markdown="1">
<div class="editor-titel"><a href="https://github.com/erodner/prog2-adventure/blob/v03-blazor/Adventure.Daten/EingebauteLevelQuelle.cs">Adventure.Daten/EingebauteLevelQuelle.cs</a></div>

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

    public IReadOnlyList<string> LevelNamen
    {
        get { return new List<string>(level.Keys); }
    }

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

</div>
</div>

Man beachte die Richtung: Das Interface liegt im Projekt `Adventure.Kern`, und `Adventure.Daten` verweist *auf den Kern*, nicht umgekehrt. Dieses Prinzip heißt **Dependency Inversion**: Die Abhängigkeit zeigt zur Abstraktion im Fachkonzept, und deshalb bleibt die Schichtregel „nur über Schnittstellen“ erfüllt, obwohl die Datenhaltung die tiefere Schicht ist. Der Gewinn zeigt sich in [Vorlesung 09](/lectures/09/09.md): Dort kommen eine `TextdateiLevelQuelle` und eine `HttpLevelQuelle` dazu, und weder der Kern noch eine Komponente ändert sich dafür.

## Dependency Injection: wer erzeugt die Levelquelle?

Bleibt die Frage, wer `new EingebauteLevelQuelle()` schreibt. Der Kern darf es nicht, sonst würde er die Datenschicht kennen. Die Komponente sollte es nicht, sonst müsste jede Seite wissen, welche Quelle gerade gilt – und ein Wechsel wäre eine Suchen-und-Ersetzen-Aktion. ASP.NET Core sieht dafür genau eine Stelle vor, den **DI-Container** in `Program.cs` (links). Die Komponente holt das fertige Objekt mit `@inject` ab (rechts):

<div class="editoren" markdown="1">
<div class="editor" markdown="1">
<div class="editor-titel"><a href="https://github.com/erodner/prog2-adventure/blob/v03-blazor/Adventure.Web/Program.cs">Adventure.Web/Program.cs</a></div>

```csharp
// Die eine Stelle, an der entschieden wird, woher die Level kommen.
builder.Services.AddSingleton<ILevelQuelle, EingebauteLevelQuelle>();
```

</div>
<div class="editor" markdown="1">
<div class="editor-titel"><a href="https://github.com/erodner/prog2-adventure/blob/v03-blazor/Adventure.Web/Components/Pages/Home.razor">Adventure.Web/Components/Pages/Home.razor</a></div>

```razor
@page "/"
@using Adventure.Kern
@inject ILevelQuelle LevelQuelle
```

</div>
</div>

Die Zeile registriert: „Wer einen `ILevelQuelle` braucht, bekommt eine `EingebauteLevelQuelle`.“ Das ist **Dependency Injection**: Objekte bekommen ihre Abhängigkeiten von außen geliefert, statt sie selbst zu erzeugen. Deshalb braucht das Web-Projekt überhaupt eine Referenz auf `Adventure.Daten` – nur für diese eine Zeile. Die Konsolenversion kommt ohne Container aus und schreibt die Entscheidung in die erste Zeile ihrer `Program.cs` (`ILevelQuelle levelQuelle = new EingebauteLevelQuelle();`) – auch das ist in Ordnung, denn es ist wieder *eine* Stelle, ganz oben im Programm.

Ab da steht `LevelQuelle` in Markup und `@code` als Property zur Verfügung – die `@foreach`-Schleife der Levelauswahl liest `LevelQuelle.LevelNamen`, `NeuStarten` ruft `LevelQuelle.Laden(levelName)`. Entscheidend ist der Typ: `@inject ILevelQuelle` nennt das **Interface**, nicht die Klasse. Die Komponente könnte gar nicht bemerken, wenn morgen eine Datei- oder Webquelle dahinterstünde.

`AddSingleton` heißt: Es gibt **eine** Levelquelle für den ganzen Server. Das passt, weil sie nur Karten liest, alle Spieler dürfen sie teilen. Das `Spielfeld` steht dagegen nicht hier, sondern ist ein Feld der Komponente. Deshalb hat jedes Browserfenster sein eigenes Spiel.
{: .notice--primary}

## Was man damit gewinnt

Drei Dinge, die ohne die Trennung nicht gingen. Erstens lässt sich die Levelquelle **austauschen, ohne eine Komponente anzufassen** – eine Zeile in `Program.cs`. Zweitens lässt sich der Kern **ohne Browser prüfen**: Ein kleines Programm kann ein `Spielfeld` mit dem `LevelParser` bauen, `SpielerZieht` aufrufen und `Status`, `Lebenspunkte` oder `Punkte` kontrollieren, ohne dass eine Seite gerendert wird. Wie man solche Prüfungen automatisiert, zeigt [Vorlesung 11](/lectures/11/11.md). Und drittens konnte diese ganze Vorlesung existieren: Eine neue Oberfläche zu bauen war möglich, *ohne* das Spiel neu zu schreiben.

## Dieselbe Oberfläche, zweimal ausgeliefert

Wie weit die Trennung trägt, zeigt ein letztes Projekt: `Adventure.Wasm`. Es liefert dasselbe Spiel aus, aber als **WebAssembly** – der C#-Code läuft dann nicht auf einem Server, sondern im Browser des Spielers. Probier es aus: [Adventure im Browser spielen](https://www.erodner.de/prog2-adventure/). Nach dem ersten Laden braucht die Seite keinen Server mehr, sie liegt als reine Dateisammlung auf GitHub Pages.

Bemerkenswert daran ist, was dafür *nicht* nötig war. Weder `Adventure.Kern` noch die Komponenten wurden angefasst. Das neue Projekt verweist auf dieselben `.razor`-Dateien wie `Adventure.Web` (links). Neu ist nur der Rahmen: eine eigene `Program.cs`, die den Browser statt einen Webserver hochfährt, und eine `index.html`. Und eine Entscheidung, die uns das Interface abnimmt: Im Browser gibt es kein Dateisystem, also kann dort keine `TextdateiLevelQuelle` laufen (rechts):

<div class="editoren" markdown="1">
<div class="editor" markdown="1">
<div class="editor-titel"><a href="https://github.com/erodner/prog2-adventure/blob/v11-tests/Adventure.Wasm/Adventure.Wasm.csproj">Adventure.Wasm/Adventure.Wasm.csproj</a></div>

```xml
<ItemGroup>
  <Content Include="..\Adventure.Web\Components\Pages\Home.razor" LinkBase="Components/Pages" />
  <Content Include="..\Adventure.Web\Components\Statusleiste.razor" LinkBase="Components" />
  <Content Include="..\Adventure.Web\Components\SpielEndeDialog.razor" LinkBase="Components" />
</ItemGroup>
```

</div>
<div class="editor" markdown="1">
<div class="editor-titel"><a href="https://github.com/erodner/prog2-adventure/blob/v11-tests/Adventure.Wasm/Program.cs">Adventure.Wasm/Program.cs</a></div>

```csharp
builder.Services.AddSingleton<ILevelQuelle, EingebauteLevelQuelle>();
```

</div>
</div>

Eine Zeile, eine andere Umgebung, dasselbe Spiel. Dass das überhaupt geht, ist kein Verdienst von Blazor, sondern der Schichtung: Weil das Fachkonzept nie eine Datei geöffnet hat, sondern immer nur `ILevelQuelle` kannte, ist der Wechsel von der Festplatte in den Browser eine Konfigurationsfrage geblieben.
{: .notice--primary}

Übung: Das Spiel soll einen Spielstand speichern und laden können. Schreibe auf, was in welches Projekt kommt: das Interface für den Speicher, die Klasse, die tatsächlich in eine Datei schreibt, die Buttons „Speichern“ und „Laden“, die Methode, die den Zustand des Spielfelds einsammelt, und die Zeile, die entscheidet, in welche Datei geschrieben wird. Prüfe deine Verteilung mit der Konsolen-Probe – und überlege, welche der fünf Teile die Konsolenversion mitbenutzen könnte.
{: .notice--info}

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag [`v03-blazor`](https://github.com/erodner/prog2-adventure/tree/v03-blazor)).

## Weitere Quellen

- [Dependency Injection in ASP.NET Core Blazor – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/fundamentals/dependency-injection)
- [Abhängigkeitsinjektion in .NET – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/extensions/dependency-injection)
- [dotnet sln – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/tools/dotnet-sln)
- [Singleton – Game Programming Patterns](https://gameprogrammingpatterns.com/singleton.html) – zeigt, warum global erreichbare Objekte in Spielen so verführerisch und so teuer sind; die Antwort darauf ist genau das, was `builder.Services` für uns übernimmt.
- [Blazor University](https://blazor-university.com/) – das Kapitel zu Dependency Injection erklärt den Unterschied zwischen `Singleton`, `Scoped` und `Transient` an kleinen Blazor-Beispielen.
