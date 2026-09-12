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

Tests sind Code – aber sie gehören nicht in dasselbe Projekt wie der Code, den sie prüfen. Niemand will das Test-Framework in der ausgelieferten Spielbibliothek haben, und niemand will, dass ein Testfehler das Bauen der Anwendung verhindert. Die Konvention in .NET ist deshalb einfach: Zu einer Solution gehört ein eigenes Testprojekt, das die zu prüfenden Projekte referenziert und NUnit als NuGet-Paket einbindet. In diesem Modul legen wir `Adventure.Tests` an und lernen die Bausteine kennen, aus denen jede NUnit-Testklasse besteht.

## Ein Testprojekt neben den anderen

Das Adventure besteht inzwischen aus fünf Projekten; das Testprojekt ist das sechste:

```
Adventure.slnx
├── Adventure.Kern/       ← Spielregeln
├── Adventure.Daten/      ← Levelquellen, Spielstände
├── Adventure.Konsole/    ← Oberfläche 1
├── Adventure.Web/        ← Oberfläche 2
└── Adventure.Tests/      ← NUnit-Testprojekt
    ├── Adventure.Tests.csproj
    ├── SpielfeldTests.cs
    └── DatenTests.cs
```

Die Namensgebung `<Projekt>.Tests` ist Konvention, keine Pflicht – aber Werkzeuge und Kolleginnen erwarten sie. Das Testprojekt hängt von den geprüften Projekten ab, niemals umgekehrt. `Adventure.Tests` referenziert deshalb `Adventure.Kern` und `Adventure.Daten`, aber weder die Konsole noch die Web-Oberfläche: Was wir testen wollen, liegt in den unteren beiden Schichten.

## Anlegen mit der CLI

Das .NET SDK bringt ein Template für NUnit-Projekte mit. Vier Kommandos erzeugen das Testprojekt, verknüpfen es mit den beiden Bibliotheken und nehmen es in die Solution auf:

```bash
dotnet new nunit -o Adventure.Tests
dotnet add Adventure.Tests reference Adventure.Kern
dotnet add Adventure.Tests reference Adventure.Daten
dotnet sln Adventure.slnx add Adventure.Tests
```

Das Template legt eine Beispieldatei `UnitTest1.cs` an, die du gleich löschen oder umbenennen kannst. Ab jetzt baut `dotnet build` auf der Solution auch das Testprojekt, und `dotnet test` findet die Tests automatisch. Für eine einzelne Klassenbibliothek – etwa das kleine Beispiel `Bruch` in `examples/12_unittests/Bruch` – sind es dieselben Kommandos mit einer Referenz weniger.

## Die Projektdatei

Ein Blick in die erzeugte `Adventure.Tests.csproj` zeigt, was das Template mitbringt – und knüpft direkt an das an, was wir über [NuGet-Pakete](/modules/nuget_pakete_hinzufuegen/nuget_pakete_hinzufuegen.md) gelernt haben:

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
  <ProjectReference Include="..\Adventure.Kern\Adventure.Kern.csproj" />
  <ProjectReference Include="..\Adventure.Daten\Adventure.Daten.csproj" />
</ItemGroup>
```

Jedes Paket hat eine Aufgabe: `NUnit` ist das Framework selbst mit den Attributen und `Assert.That`; `NUnit3TestAdapter` (der Name ist historisch, er funktioniert mit NUnit 4) und `Microsoft.NET.Test.Sdk` sorgen dafür, dass `dotnet test` und die IDE die Tests finden und ausführen können; `NUnit.Analyzers` warnt schon beim Kompilieren vor typischen Fehlern, etwa vor einem Test ohne Assertion; `coverlet.collector` misst auf Wunsch, welche Zeilen von Tests durchlaufen werden. Das `<Using Include="NUnit.Framework" />` ist ein globales `using` – deshalb steht in den Testdateien nur `using Adventure.Kern;` und `using Adventure.Daten;`, aber kein `using NUnit.Framework;`.

## `[TestFixture]` und `[Test]`

Eine Testklasse ist eine ganz normale Klasse mit dem Attribut `[TestFixture]`; jede Methode mit `[Test]` ist ein einzelner Testfall. NUnit sucht beim Ausführen alle so markierten Methoden, erzeugt pro Test eine frische Instanz der Klasse und ruft die Methoden nacheinander auf:

```csharp
using Adventure.Daten;
using Adventure.Kern;

namespace Adventure.Tests;

[TestFixture]
public class SpielfeldTests
{
    private static Spielfeld Feld(params string[] zeilen) => LevelParser.Parsen(new Level("t", zeilen));

    [Test] public void Trank_Heilt()
    {
        Spielfeld f = Feld("@!");
        f.Spieler.SchadenNehmen(2);
        f.SpielerZieht(Richtung.Rechts);
        Assert.That(f.Spieler.Lebenspunkte, Is.EqualTo(2));
    }
}
```

Testmethoden sind `public`, geben `void` (oder `Task` bei asynchronem Code) zurück und haben keine Parameter – außer bei `[TestCase]`, dazu mehr im Modul [Assertions](/modules/assertions/assertions.md). Der Klassenname folgt dem Muster `<Klasse>Tests`, damit man Tests und getestete Klasse sofort zuordnen kann.

## Der Trick mit `Feld(...)`

Die erste Zeile der Klasse ist keine Testmethode, sondern eine private Hilfsmethode – und sie ist der Grund, warum die Tests des Adventures so kurz sind:

```csharp
private static Spielfeld Feld(params string[] zeilen) => LevelParser.Parsen(new Level("t", zeilen));
```

Sie nimmt die Zeilen einer Karte als Strings entgegen, verpackt sie in ein `Level` und schickt sie durch den `LevelParser`, den wir ohnehin schon haben. Aus `Feld("@kD.E")` wird so ein komplettes Spielfeld mit Held, Schlüssel, Tür, Boden und Ausgang – die Karte *ist* die Ausgangslage des Tests und steht direkt lesbar in der ersten Zeile. Wer mehrere Reihen braucht, schreibt sie untereinander:

```csharp
Spielfeld f = Feld("#####",
                   "#@#..",
                   "#####");
```

Ohne diesen Helfer müsste jeder Test ein `Spielfeld` mit Breite und Höhe anlegen, einen `Spieler` erzeugen und jedes Objekt einzeln mit `Hinzufuegen` an eine ausgerechnete `Position` setzen – zehn Zeilen Rüstzeug, bei denen niemand mehr sieht, worum es geht. Solche Helfer sind in Testklassen ausdrücklich erwünscht: Sie halten den Arrange-Teil so klein, dass das Verhalten im Vordergrund steht. Wichtig ist nur, dass der Helfer selbst simpel bleibt – Logik, die im Test schiefgehen kann, prüft niemand.
{: .notice--primary}

## Arrange – Act – Assert

Fast jeder Test hat dieselben drei Abschnitte, die sich als Kommentare oder zumindest als Leerzeilen im Code wiederfinden. Am Test für die Wand sieht man sie besonders deutlich:

```csharp
[Test] public void Wand_Blockiert()
{
    // Arrange – Ausgangslage aufbauen: Held eingemauert, rechts eine Wand
    Spielfeld f = Feld("#####", "#@#..", "#####");

    // Act – genau eine Aktion ausführen
    f.SpielerZieht(Richtung.Rechts);

    // Assert – Ergebnis prüfen
    Assert.That(f.Spieler.Position, Is.EqualTo(new Position(1, 1)));
    Assert.That(f.LetzteMeldung, Does.Contain("nicht weiter"));
}
```

**Arrange** stellt alles bereit, was der Test braucht. **Act** ruft die zu testende Methode auf – idealerweise eine einzige Zeile. **Assert** vergleicht das Ergebnis mit der Erwartung, hier gleich zweifach: Der Held steht noch auf `(1, 1)`, und die Meldung erklärt auch, warum. Wer sich an diese Reihenfolge hält, schreibt Tests, die andere in fünf Sekunden lesen können. Mehrere Act-Schritte hintereinander sind ein Warnsignal – es sei denn, das geprüfte Verhalten braucht sie tatsächlich, wie beim Aufheben des Schlüssels und dem anschließenden Aufschließen der Tür.

## `[SetUp]` und `[TearDown]`

Die Spielfeld-Tests brauchen keinen gemeinsamen Aufbau, weil `Feld(...)` das in einer Zeile erledigt. Sobald ein Test aber Spuren außerhalb des Arbeitsspeichers hinterlässt, sieht das anders aus. `DatenTests` prüft `TextdateiLevelQuelle` und `JsonSpielstandSpeicher` – beide arbeiten mit echten Dateien. Also bekommt jeder Test einen eigenen Ordner, der danach wieder verschwindet:

```csharp
[TestFixture]
public class DatenTests
{
    private string ordner = "";

    [SetUp]
    public void Vorbereiten()
    {
        ordner = Path.Combine(Path.GetTempPath(), "adventure_" + Guid.NewGuid());
        Directory.CreateDirectory(ordner);
    }

    [TearDown]
    public void Aufraeumen()
    {
        Directory.Delete(ordner, recursive: true);
    }

    [Test]
    public void TextdateiLevelQuelle_ListetUndLaedtLevel()
    {
        File.WriteAllLines(Path.Combine(ordner, "mini.txt"), new[] { "#####", "#@.E#", "#####", "" });

        TextdateiLevelQuelle quelle = new(ordner);

        Assert.That(quelle.LevelNamen, Is.EqualTo(new[] { "mini" }));
        Assert.That(quelle.Laden("mini").Zeilen, Has.Count.EqualTo(3));
    }
}
```

NUnit ruft `[SetUp]` **vor jedem einzelnen Test** auf und `[TearDown]` **danach** – auch dann, wenn der Test fehlgeschlagen ist. Jeder Test bekommt damit ein frisches Verzeichnis und kann die anderen nicht beeinflussen. Die `Guid` im Namen sorgt zusätzlich dafür, dass sich parallel laufende Tests nicht in die Quere kommen. Wer stattdessen einen festen Pfad wie `levels/` benutzt, bekommt Tests, die mal grün und mal rot sind – je nachdem, welcher zuerst dran war und was vom letzten Lauf übrig blieb.
{: .notice--warning}

## Benennung: Was, unter welcher Bedingung, mit welchem Ergebnis

Alle Tests im Adventure folgen einem dreiteiligen Namensschema aus geprüftem Ding, Situation und erwartetem Verhalten: `Tuer_Ohne_Schluessel_Bleibt_Zu`, `Verfolger_Sieht_Nicht_Durch_Waende`, `Wache_Dreht_Um`, `Spielstand_SpeichernUndLaden_StelltSpielWiederHer`. Bei einem fehlgeschlagenen Test liest man dann schon in der Ausgabe, welche Spielregel verletzt ist – ohne den Testcode zu öffnen. Wenn dir kein eigenes Szenario einfällt (wie bei `Trank_Heilt`), darf der mittlere Teil entfallen; Hauptsache, der Name beschreibt ein Verhalten und nicht nur `Test1`. In Projekten ohne Spielobjekte nennt man statt des Dings meist die Methode, also `Laden_OhneDatei_WirftFileNotFoundException` – das Muster bleibt dasselbe.

Übung: Lege in `SpielfeldTests` einen Test `Schatz_Erhoeht_Punkte` an. Baue mit `Feld("@$")` ein Mini-Level, ziehe einmal nach rechts und prüfe, dass `f.Spieler.Punkte` danach 25 beträgt und der Schatz im Inventar liegt (`f.Spieler.Inventar.Anzahl`). Schau vorher im `LevelParser` nach, mit welchem Wert ein `Schatz` erzeugt wird.
{: .notice--info}

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v12-tests`).

## Weitere Quellen

- [Komponententests mit NUnit – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/testing/unit-testing-csharp-with-nunit)
- [SetUp und TearDown – NUnit-Dokumentation](https://docs.nunit.org/articles/nunit/writing-tests/attributes/setup.html)
- [dotnet new nunit – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/tools/dotnet-new-sdk-templates)
