---
title: "Ein NUnit-Testprojekt anlegen"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Tests sind Code – aber sie gehören nicht in dasselbe Projekt wie der Code, den sie prüfen. Niemand will NUnit in seiner ausgelieferten Bibliothek haben, und niemand will, dass ein Testfehler das Bauen der Anwendung verhindert. Die Konvention in .NET ist deshalb einfach: Zu jeder Klassenbibliothek gibt es ein eigenes Testprojekt, das die Bibliothek referenziert und das Test-Framework als NuGet-Paket einbindet. In diesem Modul legen wir so ein Projekt für die Klasse `Bruch` an und lernen die Bausteine kennen, aus denen jede NUnit-Testklasse besteht.

## Ein Testprojekt pro Bibliothek

Das Beispielprojekt besteht aus zwei Projekten in einer Solution:

```
Bruch/
├── Bruch.slnx
├── Bruch/                  ← Klassenbibliothek
│   ├── Bruch.csproj
│   └── Bruch.cs
└── Bruch.Tests/            ← NUnit-Testprojekt
    ├── Bruch.Tests.csproj
    └── BruchTests.cs
```

Die Namensgebung `<Projekt>.Tests` ist Konvention, keine Pflicht – aber Werkzeuge und Kolleginnen erwarten sie. Das Testprojekt hängt von der Bibliothek ab, niemals umgekehrt. Im Geometrieeditor referenziert `Geometrieeditor.Tests` entsprechend `Fachkonzept` und `Datenhaltung`, aber nicht die GUI.

## Anlegen mit der CLI

Das .NET SDK bringt ein Template für NUnit-Projekte mit. Drei Kommandos reichen, um das Testprojekt zu erzeugen, mit der Bibliothek zu verknüpfen und in die Solution aufzunehmen:

```bash
dotnet new nunit -o Bruch.Tests
dotnet add Bruch.Tests reference Bruch
dotnet sln Bruch.slnx add Bruch.Tests
```

Das Template legt eine Beispieldatei `UnitTest1.cs` an, die du gleich löschen oder umbenennen kannst. Ab jetzt baut `dotnet build` auf der Solution beide Projekte, und `dotnet test` findet die Tests automatisch.

## Die Projektdatei

Ein Blick in die erzeugte `Bruch.Tests.csproj` zeigt, was das Template alles mitbringt – und knüpft direkt an das an, was wir über [NuGet-Pakete](/modules/nuget_pakete_hinzufuegen/nuget_pakete_hinzufuegen.md) gelernt haben:

```xml
<ItemGroup>
  <PackageReference Include="coverlet.collector" Version="6.0.4" />
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.0" />
  <PackageReference Include="NUnit" Version="4.3.2" />
  <PackageReference Include="NUnit.Analyzers" Version="4.7.0" />
  <PackageReference Include="NUnit3TestAdapter" Version="5.0.0" />
</ItemGroup>

<ItemGroup>
  <Using Include="NUnit.Framework" />
</ItemGroup>

<ItemGroup>
  <ProjectReference Include="..\Bruch\Bruch.csproj" />
</ItemGroup>
```

Jedes Paket hat eine Aufgabe: `NUnit` ist das Framework selbst mit den Attributen und `Assert.That`; `NUnit3TestAdapter` (der Name ist historisch, er funktioniert mit NUnit 4) und `Microsoft.NET.Test.Sdk` sorgen dafür, dass `dotnet test` und die IDE die Tests finden und ausführen können; `NUnit.Analyzers` warnt schon beim Kompilieren vor typischen Fehlern; `coverlet.collector` misst auf Wunsch, welche Zeilen von Tests durchlaufen werden. Das `<Using Include="NUnit.Framework" />` ist ein globales `using` – deshalb steht in den Testdateien kein `using NUnit.Framework;`.

## `[TestFixture]` und `[Test]`

Eine Testklasse ist eine ganz normale Klasse mit dem Attribut `[TestFixture]`; jede Methode mit `[Test]` ist ein einzelner Testfall. NUnit sucht beim Ausführen alle so markierten Methoden und ruft sie nacheinander auf:

```csharp
namespace Bruch.Tests;

[TestFixture]
public class BruchTests
{
    [Test]
    public void Konstruktor_KuerztDenBruch()
    {
        // Arrange + Act
        Bruch b = new Bruch(6, 8);

        // Assert
        Assert.That(b.Zaehler, Is.EqualTo(3));
        Assert.That(b.Nenner, Is.EqualTo(4));
    }
}
```

Testmethoden sind `public`, geben `void` (oder `Task` bei asynchronem Code) zurück und haben keine Parameter – außer bei `[TestCase]`, dazu mehr im Modul [Assertions](/modules/assertions/assertions.md). Der Name der Klasse folgt dem Muster `<Klasse>Tests`, damit man Tests und getestete Klasse sofort zuordnen kann.

## Arrange – Act – Assert

Fast jeder Test hat dieselben drei Abschnitte, die sich als Kommentare oder zumindest als Leerzeilen im Code wiederfinden:

```csharp
[Test]
public void Plus_EinHalbPlusEinDrittel_ErgibtFuenfSechstel()
{
    // Arrange – Ausgangslage aufbauen
    Bruch a = new Bruch(1, 2);
    Bruch b = new Bruch(1, 3);

    // Act – genau eine Aktion ausführen
    Bruch summe = a.Plus(b);

    // Assert – Ergebnis prüfen
    Assert.That(summe, Is.EqualTo(new Bruch(5, 6)));
}
```

**Arrange** stellt alles bereit, was der Test braucht. **Act** ruft die zu testende Methode auf – idealerweise eine einzige Zeile. **Assert** vergleicht das Ergebnis mit der Erwartung. Wer sich an diese Reihenfolge hält, schreibt Tests, die andere in fünf Sekunden lesen können. Mehrere Act-Schritte in einem Test sind ein Warnsignal: Vermutlich sollten das zwei Tests sein.

## `[SetUp]` und `[TearDown]`

Wenn jeder Test dieselbe Ausgangslage braucht, wandert der Arrange-Teil in eine Methode mit `[SetUp]`. NUnit ruft sie **vor jedem einzelnen Test** auf – jeder Test bekommt also ein frisches Objekt und kann die anderen nicht beeinflussen. Genau so beginnt die Testklasse für die `FigurenVerwaltung`:

```csharp
[TestFixture]
public class FigurenVerwaltungTests
{
    private FigurenVerwaltung verwaltung = null!;

    [SetUp]
    public void Vorbereiten()
    {
        verwaltung = new FigurenVerwaltung(new ArbeitsspeicherFigurSpeicher());
    }

    [Test]
    public void Hinzufuegen_NeueFigur_ErscheintInAlleFiguren()
    {
        Kreis kreis = new Kreis("k1", 0, 0, 1);

        verwaltung.Hinzufuegen(kreis);

        Assert.That(verwaltung.AlleFiguren, Has.Count.EqualTo(1));
        Assert.That(verwaltung.AlleFiguren[0], Is.SameAs(kreis));
    }
}
```

Das `= null!` beim Feld beruhigt den Nullable-Compiler: Er kann nicht wissen, dass `[SetUp]` das Feld vor jedem Test befüllt. Das Gegenstück `[TearDown]` läuft **nach jedem Test** und räumt auf – wichtig, sobald ein Test Spuren außerhalb des Arbeitsspeichers hinterlässt. Der `JsonFigurSpeicher` schreibt eine echte Datei, also bekommt jeder Test einen eigenen Pfad im Temp-Verzeichnis, der danach wieder gelöscht wird:

```csharp
private string pfad = "";

[SetUp]
public void Vorbereiten()
{
    pfad = Path.Combine(Path.GetTempPath(), $"figuren_{Guid.NewGuid()}.json");
}

[TearDown]
public void Aufraeumen()
{
    if (File.Exists(pfad)) File.Delete(pfad);
}

[Test]
public void Laden_OhneDatei_LiefertLeereListe()
{
    JsonFigurSpeicher speicher = new JsonFigurSpeicher(pfad);

    Assert.That(speicher.Laden(), Is.Empty);
}
```

Die `Guid` im Dateinamen sorgt dafür, dass sich zwei parallel laufende Tests nicht in die Quere kommen. Wer stattdessen einen festen Pfad wie `figuren.json` benutzt, bekommt Tests, die mal grün und mal rot sind – je nachdem, welcher zuerst dran war.
{: .notice--warning}

## Benennung: `Methode_Szenario_ErwartetesErgebnis`

Alle Tests in diesem Kurs folgen einem dreiteiligen Namensschema: `Konstruktor_NennerNull_WirftArgumentException`, `Laden_OhneDatei_LiefertLeereListe`, `SpeichernUndLaden_BehaeltKonkreteTypen`. Der erste Teil nennt die getestete Methode, der zweite die Situation, der dritte das erwartete Verhalten. Bei einem fehlgeschlagenen Test liest man dann in der Ausgabe schon am Namen, was kaputt ist – ohne den Testcode zu öffnen. Wenn dir kein Szenario einfällt (wie bei `Konstruktor_KuerztDenBruch`), darf der mittlere Teil auch entfallen; Hauptsache, der Name beschreibt ein Verhalten und nicht nur `Test1`.

Übung: Lege für den Geometrieeditor eine Testklasse `KreisTests` an und schreibe zwei Tests nach dem Namensschema: einen für `Flaeche` bei Radius 1 und einen für `Verschieben`, der prüft, dass sich `X` und `Y` um die übergebenen Werte ändern. Führe sie mit `dotnet test` aus.
{: .notice--info}

Das vollständige Projekt findest du im Repository unter `examples/12_unittests/Bruch`, die Tests des Geometrieeditors unter `examples/03_blazor/Geometrieeditor/Geometrieeditor.Tests`.

## Weitere Quellen

- [Komponententests mit NUnit – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/testing/unit-testing-csharp-with-nunit)
- [SetUp und TearDown – NUnit-Dokumentation](https://docs.nunit.org/articles/nunit/writing-tests/attributes/setup.html)
- [dotnet new nunit – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/tools/dotnet-new-sdk-templates)
