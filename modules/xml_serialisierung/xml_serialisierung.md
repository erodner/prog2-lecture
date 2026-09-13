---
title: "XML-Serialisierung"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

JSON ist heute das Standardformat, aber es ist nicht das einzige. Konfigurationsdateien vieler Werkzeuge, Office-Dokumente, SVG-Grafiken, die `.csproj`-Dateien unserer eigenen Projekte und unzählige Schnittstellen in Behörden und Industrie sprechen **XML**. Wer mit solchen Systemen Daten austauschen muss, braucht die XML-Serialisierung – und wer beide Formate kennt, kann begründet entscheiden, welches zu einem Problem passt. Dieses Modul speichert denselben `Spielstand` wie das vorige Modul noch einmal, diesmal als XML, stellt beide Formate gegenüber und erklärt, warum die binäre Serialisierung aus älteren Lehrbüchern in modernem .NET nicht mehr existiert.

## `XmlSerializer` in Aktion

Der `XmlSerializer` aus `System.Xml.Serialization` arbeitet nach demselben Prinzip wie `JsonSerializer`: Er wandelt öffentliche Properties in Text und zurück. Anders als das JSON-Gegenstück ist er kein statischer Helfer, sondern ein Objekt, das man einmal pro Typ anlegt, und er schreibt direkt in einen Stream oder `TextWriter` – die Klassen aus dem Modul [Streams](/modules/streams/streams.md) tauchen hier also wieder auf:

```csharp
XmlSerializer serializer = new XmlSerializer(typeof(Spielstand));
string pfad = Path.Combine(Path.GetTempPath(), "spielstand.xml");

using (StreamWriter writer = File.CreateText(pfad))
{
    serializer.Serialize(writer, feld.Erfassen("kerker", level));
}

using (StreamReader reader = File.OpenText(pfad))
{
    Spielstand geladen = (Spielstand)serializer.Deserialize(reader)!;
    Console.WriteLine(geladen.Punkte);   // 100
}
```

`Deserialize` liefert `object`, weshalb der Cast auf `Spielstand` nötig ist – der Typ wurde ja schon dem Konstruktor übergeben. Die `using`-Blöcke schließen die Datei nach dem Schreiben, bevor sie zum Lesen wieder geöffnet wird; eine `using`-Deklaration würde hier nicht genügen, weil sie erst am Ende der Methode freigibt. Es ist derselbe Spielstand wie im vorigen Modul, diesmal auf der Platte als XML (gekürzt um `EntfernteGegenstaende` und `GeoeffneteTruhen`, die genauso aufgebaut sind wie `OffeneTueren`):

```xml
<?xml version="1.0" encoding="utf-8"?>
<Spielstand xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <LevelName>kerker</LevelName>
  <Runde>33</Runde>
  <SpielerPosition>
    <X>15</X>
    <Y>5</Y>
  </SpielerPosition>
  <Lebenspunkte>2</Lebenspunkte>
  <Punkte>100</Punkte>
  <Inventar />
  <OffeneTueren>
    <Position>
      <X>7</X>
      <Y>4</Y>
    </Position>
  </OffeneTueren>
  <GegnerPositionen>
    <Position>
      <X>8</X>
      <Y>2</Y>
    </Position>
    <Position>
      <X>15</X>
      <Y>4</Y>
    </Position>
  </GegnerPositionen>
</Spielstand>
```

Jede Property wird zu einem Element mit öffnendem und schließendem Tag; der Klassenname wird zum Wurzelelement, und die Elemente einer Liste bekommen den Namen ihres Elementtyps (`<Position>`). Eine leere Liste wie `Inventar` schrumpft auf ein einzelnes `<Inventar />`. Die `xmlns`-Attribute in der zweiten Zeile schreibt der Serialisierer immer, auch wenn man sie nicht braucht. Vergleicht man die Datei mit der JSON-Fassung, fällt sofort auf: Dieselbe Information braucht rund die doppelte Menge Text, weil jeder Name zweimal dasteht.

## Anforderungen an die Klasse

Dass das überhaupt funktioniert hat, liegt daran, wie wir `Spielstand` entworfen haben: reine Datenklasse, öffentliche Properties mit `get` *und* `set`, kein eigener Konstruktor. Der `XmlSerializer` ist nämlich deutlich wählerischer als `System.Text.Json`. Er verlangt einen **öffentlichen parameterlosen Konstruktor**, weil er das Objekt erst leer erzeugt und dann Property für Property befüllt. Die Klassen aus `Adventure.Kern` erfüllen das nicht:

```csharp
XmlSerializer tuerSerializer = new XmlSerializer(typeof(Tuer));
// InvalidOperationException: Adventure.Kern.Tuer cannot be serialized
// because it does not have a parameterless constructor.
```

Der Fehler kommt bereits beim Erzeugen des Serialisierers, nicht erst beim Schreiben. Auch `Dictionary<,>` und Interfaces als Property-Typen lehnt der `XmlSerializer` ab – das `Dictionary<Position, StatischesObjekt>` im `Spielfeld` wäre also ohnehin chancenlos –, und Polymorphie über eine abstrakte Basisklasse braucht zusätzliche `[XmlInclude]`-Attribute. Wer eine Klasse für XML entwirft, hält sie deshalb bewusst einfach: parameterloser Konstruktor, öffentliche Auto-Properties, konkrete Typen. Genau das ist unser `Spielstand` – und das ist kein Zufall, sondern derselbe Entwurf, der ihn auch für JSON tauglich macht.

## Aufbau steuern mit Attributen

Wie in JSON lässt sich das Ergebnis mit Attributen anpassen. XML kennt allerdings zwei Arten, einen Wert unterzubringen: als eigenes **Element** oder als **Attribut** im öffnenden Tag. Ein `[XmlAttribute]` eignet sich für kurze Kennungen und Zahlen, `[XmlElement]` und `[XmlRoot]` benennen Elemente um, und `[XmlIgnore]` lässt eine Property weg. Für die Positionen macht das die Datei spürbar kompakter:

```csharp
[XmlRoot("spielstand")]
public class Spielstand
{
    [XmlAttribute("level")]
    public string LevelName { get; set; } = "";

    [XmlAttribute("runde")]
    public int Runde { get; set; }

    [XmlElement("spieler")]
    public Position SpielerPosition { get; set; }

    [XmlIgnore]
    public DateTime Gespeichert { get; set; }
}
// <spielstand xmlns:xsi="..." xmlns:xsd="..." level="kerker" runde="33">
//   <spieler><X>15</X><Y>5</Y></spieler>
// </spielstand>
```

Solche Attribute braucht man vor allem dann, wenn ein fremdes System das XML-Format vorgibt: Dann muss die C#-Klasse dem Format folgen und nicht umgekehrt.

## JSON oder XML?

Beide Formate sind textbasiert, sprachunabhängig und von Menschen lesbar. Die Unterschiede liegen im Detail – und sie entscheiden, wann welches Format passt:

| | JSON (`System.Text.Json`) | XML (`XmlSerializer`) |
| :--- | :--- | :--- |
| Größe | kompakt | ausführlich, jedes Element doppelt benannt |
| Konstruktoren | parametrisiert möglich | parameterlos erforderlich |
| Polymorphie | `[JsonPolymorphic]` mit Diskriminator | `[XmlInclude]` und `xsi:type` |
| Schema / Validierung | JSON Schema (optional, extern) | XSD, Namensräume, fest eingebaut |
| Typisches Umfeld | Web-APIs, Konfiguration, Apps | Office-Dateien, SVG, Industrie- und Behördenschnittstellen |
| Typische Werkzeuge | Browser, Postman, jede Sprache | XPath, XSLT, Editoren mit Schemaprüfung |

XML spielt seine Stärken aus, wenn Dokumente formal validiert oder mit Metadaten angereichert werden müssen oder wenn die Gegenseite es schlicht vorgibt. In allen anderen Fällen gilt: **Im Zweifel JSON** – kleiner, einfacher, und es kommt mit den Klassen zurecht, die wir ohnehin schreiben. Unser Spiel speichert deshalb JSON; der `ISpielstandSpeicher` aus dem [vorigen Modul](/modules/json_serialisierung/json_serialisierung.md) ließe aber jederzeit eine zweite Umsetzung `XmlSpielstandSpeicher` daneben zu, ohne dass der Kern davon erfährt.

## Und die binäre Serialisierung?

In älteren Lehrbüchern findest du den `BinaryFormatter`: Klasse mit `[Serializable]` markieren, Objekt in einen Stream schreiben, fertig. Das Format war kompakt und konnte beliebige Objektgraphen abbilden – und genau das war sein Problem. Beim Deserialisieren erzeugte der `BinaryFormatter` jeden Typ, der in den Daten stand, und rief dessen Code auf. Eine manipulierte Datei konnte so beliebigen Code ausführen; ein heruntergeladener Spielstand wäre damit ein Einfallstor gewesen. Microsoft hat die Klasse deshalb schrittweise abgeschaltet; seit .NET 9 wirft sie in jedem Fall eine Ausnahme und ist aus den Laufzeitbibliotheken entfernt. Verwende sie nicht, auch nicht „nur für interne Dateien“ – JSON oder XML sind sicherer, lesbarer und zwischen Versionen deines Programms robuster.
{: .notice--warning}

Übung: Schreibe einen `XmlSpielstandSpeicher : ISpielstandSpeicher`, der dieselben zwei Methoden wie der `JsonSpielstandSpeicher` anbietet, aber `XmlSerializer` benutzt. Tausche ihn in `Program.cs` gegen die JSON-Variante und speichere denselben Spielstand in beiden Formaten – wie groß sind die Dateien? Ergänze anschließend `[XmlAttribute]` an `Runde`, `Lebenspunkte` und `Punkte` und miss noch einmal.
{: .notice--info}

## Weitere Quellen

- [XML-Serialisierung – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/serialization/introducing-xml-serialization)
- [`XmlSerializer`-Klasse – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.xml.serialization.xmlserializer)
- [Sicherheitsrisiken von BinaryFormatter – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/serialization/binaryformatter-security-guide)
- [JSON – die komplette Syntax auf einer Seite (deutsch)](https://www.json.org/json-de.html) – zum direkten Vergleich: JSON passt auf eine Seite, die XML-Spezifikation auf ein Buch.
