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

Die [Schichten-Architektur](/modules/schichten_architektur/schichten_architektur.md) haben wir bisher als Bauplan kennengelernt: drei Schichten, vier Regeln, Zugriff nur nach unten und nur über Schnittstellen. Jetzt geht es an die Umsetzung – und dabei tauchen Fragen auf, die der Bauplan nicht beantwortet. Wie werden aus den Schichten Projekte? Woran erkennt man beim Schreiben einer Komponente, dass man gerade Fachlogik hineinschmuggelt? Und wenn das Fachkonzept nur ein Interface `IFigurSpeicher` kennt – wer erzeugt dann das konkrete Objekt, das dahintersteht? Der Geometrieeditor beantwortet alle drei Fragen, und die dritte führt uns zu einem Mechanismus, den jede ASP.NET-Core-Anwendung mitbringt: **Dependency Injection**.

## Die Solution: vier Projekte

Jede Schicht wird ein eigenes Projekt, dazu kommt ein Testprojekt. Die Datei `Geometrieeditor.slnx` fasst sie zusammen:

```xml
<Solution>
  <Project Path="Geometrieeditor.Datenhaltung/Geometrieeditor.Datenhaltung.csproj" />
  <Project Path="Geometrieeditor.Fachkonzept/Geometrieeditor.Fachkonzept.csproj" />
  <Project Path="Geometrieeditor.Tests/Geometrieeditor.Tests.csproj" />
  <Project Path="Geometrieeditor.Web/Geometrieeditor.Web.csproj" />
</Solution>
```

Angelegt wird das mit den Befehlen aus [Projekte mit der dotnet-CLI](/modules/dotnet_cli_projekte/dotnet_cli_projekte.md) – zwei Klassenbibliotheken, eine Blazor Web App, ein NUnit-Projekt, und dann die Projektreferenzen, die die erlaubten Abhängigkeiten festlegen:

```bash
dotnet new sln -n Geometrieeditor
dotnet new classlib -o Geometrieeditor.Fachkonzept
dotnet new classlib -o Geometrieeditor.Datenhaltung
dotnet new blazor -o Geometrieeditor.Web --empty -int Server -ai
dotnet new nunit -o Geometrieeditor.Tests
dotnet sln add Geometrieeditor.Fachkonzept Geometrieeditor.Datenhaltung Geometrieeditor.Web Geometrieeditor.Tests

dotnet add Geometrieeditor.Datenhaltung reference Geometrieeditor.Fachkonzept
dotnet add Geometrieeditor.Web reference Geometrieeditor.Fachkonzept Geometrieeditor.Datenhaltung
dotnet add Geometrieeditor.Tests reference Geometrieeditor.Fachkonzept Geometrieeditor.Datenhaltung
```

Die Referenzen ergeben folgendes Bild – jeder Pfeil bedeutet „kennt und benutzt“:

```
   Geometrieeditor.Web                      Geometrieeditor.Tests
   Home.razor, NeueFigurDialog.razor         FigurenVerwaltungTests
   Program.cs wählt den Speicher aus         JsonFigurSpeicherTests
        │            │                             │           │
        │            └────────────┐   ┌────────────┘           │
        ▼                         ▼   ▼                        ▼
   Geometrieeditor.Fachkonzept          Geometrieeditor.Datenhaltung
   Figur, Rechteck, Kreis, Dreieck      ArbeitsspeicherFigurSpeicher
   FigurenVerwaltung                    JsonFigurSpeicher
   IFigurSpeicher        ◄───────────── implementiert IFigurSpeicher
```

Auffällig ist, dass **niemand** auf `Geometrieeditor.Web` zeigt und das Fachkonzept auf **nichts** zeigt. Damit sind die Schichtregeln 1 und 2 vom Compiler garantiert: Wollte jemand aus der Datenhaltung eine Komponente aufrufen, bräuchte er eine Referenz auf das Web-Projekt, und die wäre ein Zirkelbezug. Der Pfeil vom Web-Projekt zur Datenhaltung ist die eine bewusste Ausnahme; wozu er nötig ist, sehen wir gleich in `Program.cs`. Das vollständige Projekt findest du im Repository unter `examples/04_blazor/Geometrieeditor`.

## Die Komponente enthält keine Geschäftslogik

Die wichtigste Regel für die GUI-Schicht ist leicht zu formulieren und schwer durchzuhalten: Eine Komponente **liest Eingaben, ruft das Fachkonzept und zeigt das Ergebnis an** – mehr nicht. In `Home.razor` sehen die Handler deshalb alle gleich aus:

```razor
@inject FigurenVerwaltung Verwaltung

@code {
    private Figur? ausgewaehlt;
    private string status = "";

    private void StatusAktualisieren()
    {
        status = $"{Verwaltung.AlleFiguren.Count} Figuren, Gesamtfläche {Verwaltung.GesamtFlaeche():F2}";
    }

    private void DialogGeschlossen(Figur? neueFigur)
    {
        dialogOffen = false;
        if (neueFigur is null)
        {
            status = "Abgebrochen.";
            return;
        }

        try
        {
            Verwaltung.Hinzufuegen(neueFigur);
            StatusAktualisieren();
        }
        catch (ArgumentException ex)
        {
            status = ex.Message;
        }
    }

    private void Entfernen()
    {
        if (ausgewaehlt is not null)
        {
            Verwaltung.Entfernen(ausgewaehlt);
            ausgewaehlt = null;
            StatusAktualisieren();
        }
    }
}
```

Nirgends steht hier, dass Namen eindeutig sein müssen oder wie eine Flächensumme gebildet wird. `DialogGeschlossen` gibt die Figur weiter und reagiert auf das Ergebnis – die Regel „kein doppelter Name“ lebt in `FigurenVerwaltung.Hinzufuegen`, die Komponente erfährt sie nur als `ArgumentException`. `StatusAktualisieren` ruft `GesamtFlaeche()` auf, statt selbst zu addieren. Die Probe aus dem Architektur-Modul hilft beim Einordnen: Könnte die Zeile unverändert in einer Konsolenversion stehen? `Verwaltung.Hinzufuegen(neueFigur)` ja, `status = ex.Message` nein – also ist die Verteilung richtig.

Was in die Komponente gehört: Felder für Auswahl und Statuszeile, das Öffnen und Schließen des Dialogs, das Umwandeln von Text in Zahlen und Zahlen in Text. Was nicht hineingehört: alles, was eine Regel des Anwendungsgebiets ist. Im Zweifel ins Fachkonzept – dorthin kann man es testen, aus der Komponente nicht.
{: .notice--primary}

## `IFigurSpeicher`: das Fachkonzept bestimmt, die Datenhaltung liefert

Schichtregel 4 verlangt, dass eine Schicht die darunterliegende nur über Schnittstellen benutzt. Das Fachkonzept legt daher selbst fest, was es von einem Speicher braucht:

```csharp
namespace Geometrieeditor.Fachkonzept;

public interface IFigurSpeicher
{
    void Speichern(IEnumerable<Figur> figuren);
    List<Figur> Laden();
}
```

Die `FigurenVerwaltung` bekommt den Speicher im Konstruktor und kennt ihn nur als `IFigurSpeicher`:

```csharp
public class FigurenVerwaltung
{
    private readonly List<Figur> figuren = new();
    private readonly IFigurSpeicher speicher;

    public FigurenVerwaltung(IFigurSpeicher speicher)
    {
        this.speicher = speicher;
    }

    public void Speichern() => speicher.Speichern(figuren);
}
```

Die Datenhaltung implementiert das Interface – `JsonFigurSpeicher` mit `System.Text.Json` und einer Datei, `ArbeitsspeicherFigurSpeicher` mit einer Liste. Man beachte die Richtung: Das Interface liegt im Projekt `Fachkonzept`, und die Datenhaltung verweist *auf das Fachkonzept*, nicht umgekehrt. Dieses Prinzip heißt **Dependency Inversion**: Die Abhängigkeit zeigt zur Abstraktion im Fachkonzept, und deshalb bleibt die Schichtregel „nur über Schnittstellen“ erfüllt, obwohl die Datenhaltung die tiefere Schicht ist.

## Dependency Injection: wer erzeugt den Speicher?

Bleibt die Frage, wer `new JsonFigurSpeicher("figuren.json")` schreibt. Die `FigurenVerwaltung` darf es nicht, sonst würde sie die Datenhaltung kennen. Die Komponente sollte es nicht, sonst müsste jede Seite wissen, welcher Speicher gerade gilt. ASP.NET Core sieht dafür genau eine Stelle vor, den **DI-Container** in `Program.cs`:

```csharp
// Hier wird entschieden, welche Datenhaltung hinter dem Fachkonzept steckt.
// "Scoped" heißt in Blazor: ein Objekt pro Browser-Verbindung (Circuit).
builder.Services.AddScoped<IFigurSpeicher>(_ => new JsonFigurSpeicher("figuren.json"));
builder.Services.AddScoped<FigurenVerwaltung>();
```

Die erste Zeile registriert: „Wer einen `IFigurSpeicher` braucht, bekommt einen `JsonFigurSpeicher`.“ Die zweite registriert die `FigurenVerwaltung` ohne Fabrikfunktion – der Container sieht ihren Konstruktor, erkennt den Parameter `IFigurSpeicher` und setzt den registrierten Speicher ein. Das ist **Dependency Injection**: Objekte bekommen ihre Abhängigkeiten von außen geliefert, statt sie selbst zu erzeugen. Deshalb braucht das Web-Projekt die Referenz auf die Datenhaltung – nur für diese eine Zeile.

In der Komponente holt eine Anweisung das fertige Objekt ab:

```razor
@inject FigurenVerwaltung Verwaltung
```

Ab da steht `Verwaltung` in Markup und `@code` als Property zur Verfügung. **Scoped** bedeutet dabei: ein Objekt pro Browser-Verbindung. Öffnen zwei Personen die Seite, hat jede ihre eigene `FigurenVerwaltung` mit eigener Figurenliste; alle Komponenten derselben Verbindung teilen sich aber dasselbe Objekt. Die Alternativen wären `AddSingleton` (ein Objekt für den ganzen Server – alle Benutzer sähen dieselben Figuren) und `AddTransient` (bei jedem `@inject` ein neues Objekt – die Figuren wären nach jedem Seitenwechsel weg).

## Was man damit gewinnt

Zwei Dinge, die ohne die Trennung nicht gingen. Erstens lässt sich der Speicher **austauschen, ohne eine Komponente anzufassen**: Wird aus der ersten Zeile in `Program.cs` `AddScoped<IFigurSpeicher, ArbeitsspeicherFigurSpeicher>()`, läuft alles weiter, nur ohne Datei. Wie der `JsonFigurSpeicher` die Figuren mit `System.Text.Json` tatsächlich in die Datei bringt, sehen wir in [JSON-Serialisierung](/modules/json_serialisierung/json_serialisierung.md). Zweitens lässt sich das Fachkonzept **ohne Browser testen**, weil ein Test denselben Konstruktor benutzt wie der DI-Container:

```csharp
[SetUp]
public void Vorbereiten()
{
    // Für Tests reicht der Speicher im Arbeitsspeicher – keine Datei, keine GUI.
    verwaltung = new FigurenVerwaltung(new ArbeitsspeicherFigurSpeicher());
}

[Test]
public void Hinzufuegen_DoppelterName_WirftArgumentException()
{
    verwaltung.Hinzufuegen(new Kreis("k1", 0, 0, 1));

    Assert.That(() => verwaltung.Hinzufuegen(new Rechteck("k1", 0, 0, 2, 3)),
                Throws.ArgumentException);
}
```

Die Regel „kein doppelter Name“ wird hier geprüft, ohne dass ein Dialog geöffnet oder ein Button geklickt wird – warum das so wertvoll ist, vertieft das Modul [Warum Unit-Tests?](/modules/unit_tests_motivation/unit_tests_motivation.md).

Übung: Der Geometrieeditor soll eine Funktion „Alle Figuren um 10 nach rechts verschieben“ bekommen. Schreibe auf, welche Zeilen in welches Projekt kommen: Button und Handler, die Schleife über die Figuren, der Aufruf von `Verschieben`. Prüfe anschließend mit der Konsolen-Probe, ob deine Verteilung stimmt – und ob du die Funktion mit einem NUnit-Test absichern könntest.
{: .notice--info}

## Weitere Quellen

- [Dependency Injection in ASP.NET Core Blazor – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/fundamentals/dependency-injection)
- [Abhängigkeitsinjektion in .NET – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/extensions/dependency-injection)
- [dotnet sln – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/tools/dotnet-sln)
