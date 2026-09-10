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

JSON ist heute das Standardformat, aber es ist nicht das einzige. Konfigurationsdateien vieler Werkzeuge, Office-Dokumente, SVG-Grafiken, die `.csproj`-Dateien unserer eigenen Projekte und unzählige Schnittstellen in Behörden und Industrie sprechen **XML**. Wer mit solchen Systemen Daten austauschen muss, braucht die XML-Serialisierung – und wer beide Formate kennt, kann begründet entscheiden, welches zu einem Problem passt. Dieses Modul zeigt den `XmlSerializer` in Kürze, stellt beide Formate gegenüber und erklärt, warum die binäre Serialisierung aus älteren Lehrbüchern in modernem .NET nicht mehr existiert.

## `XmlSerializer` in Aktion

Der `XmlSerializer` aus `System.Xml.Serialization` arbeitet nach demselben Prinzip wie `JsonSerializer`: Er wandelt öffentliche Properties in Text und zurück. Anders als das JSON-Gegenstück ist er kein statischer Helfer, sondern ein Objekt, das man einmal pro Typ anlegt, und er schreibt direkt in einen Stream oder `TextWriter`:

```csharp
public class Kontakt
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
}

XmlSerializer serializer = new XmlSerializer(typeof(Kontakt));
string pfad = Path.Combine(Path.GetTempPath(), "kontakt.xml");

using (StreamWriter writer = File.CreateText(pfad))
{
    serializer.Serialize(writer, new Kontakt { Id = 7, Name = "Ada", Email = "ada@example.org" });
}

using (StreamReader reader = File.OpenText(pfad))
{
    Kontakt geladen = (Kontakt)serializer.Deserialize(reader)!;
    Console.WriteLine(geladen.Name); // Ada
}
```

`Deserialize` liefert `object`, weshalb der Cast auf `Kontakt` nötig ist – der Typ wurde ja schon dem Konstruktor übergeben. Die `using`-Blöcke schließen die Datei nach dem Schreiben, bevor sie zum Lesen wieder geöffnet wird. Das Ergebnis auf der Platte sieht so aus:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Kontakt xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <Id>7</Id>
  <Name>Ada</Name>
  <Email>ada@example.org</Email>
</Kontakt>
```

Jede Property wird zu einem Element mit öffnendem und schließendem Tag; der Klassenname wird zum Wurzelelement. Die `xmlns`-Attribute in der ersten Zeile schreibt der Serialisierer immer, auch wenn man sie nicht braucht.

## Anforderungen an die Klasse

Der `XmlSerializer` ist wählerischer als `System.Text.Json`. Er verlangt einen **öffentlichen parameterlosen Konstruktor**, weil er das Objekt erst leer erzeugt und dann Property für Property befüllt; außerdem serialisiert er nur öffentliche Properties und Felder, die lesbar *und* schreibbar sind. Die Figuren des Geometrieeditors erfüllen das nicht:

```csharp
XmlSerializer figurSerializer = new XmlSerializer(typeof(Kreis));
// InvalidOperationException: Geometrieeditor.Fachkonzept.Kreis cannot be serialized
// because it does not have a parameterless constructor.
```

Der Fehler kommt bereits beim Erzeugen des Serialisierers, nicht erst beim Schreiben. Auch `Dictionary<,>` und Interfaces als Property-Typen lehnt der `XmlSerializer` ab, und Polymorphie über eine abstrakte Basisklasse braucht zusätzliche `[XmlInclude]`-Attribute. Wer eine Klasse für XML entwirft, hält sie deshalb bewusst einfach: parameterloser Konstruktor, öffentliche Auto-Properties, konkrete Typen.

## Aufbau steuern mit Attributen

Wie in JSON lässt sich das Ergebnis mit Attributen anpassen. XML kennt allerdings zwei Arten, einen Wert unterzubringen: als eigenes **Element** oder als **Attribut** im öffnenden Tag. Ein `[XmlAttribute]` eignet sich für kurze Kennungen wie eine ID, `[XmlElement]` und `[XmlRoot]` benennen Elemente um, und `[XmlIgnore]` lässt eine Property weg:

```csharp
[XmlRoot("kontakt")]
public class Kontakt
{
    [XmlAttribute("id")]
    public int Id { get; set; }

    [XmlElement("name")]
    public string Name { get; set; } = "";

    [XmlElement("email")]
    public string Email { get; set; } = "";

    [XmlIgnore]
    public string Telefon { get; set; } = "";
}
// <kontakt xmlns:xsi="..." xmlns:xsd="..." id="7">
//   <name>Ada</name>
//   <email>ada@example.org</email>
// </kontakt>
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

XML spielt seine Stärken aus, wenn Dokumente formal validiert oder mit Metadaten angereichert werden müssen oder wenn die Gegenseite es schlicht vorgibt. In allen anderen Fällen gilt: **Im Zweifel JSON** – kleiner, einfacher, und es kommt mit den Klassen zurecht, die wir ohnehin schreiben.

## Und die binäre Serialisierung?

In älteren Lehrbüchern findest du den `BinaryFormatter`: Klasse mit `[Serializable]` markieren, Objekt in einen Stream schreiben, fertig. Das Format war kompakt und konnte beliebige Objektgraphen abbilden – und genau das war sein Problem. Beim Deserialisieren erzeugte der `BinaryFormatter` jeden Typ, der in den Daten stand, und rief dessen Code aus. Eine manipulierte Datei konnte so beliebigen Code ausführen. Microsoft hat die Klasse deshalb schrittweise abgeschaltet; seit .NET 9 wirft sie in jedem Fall eine Ausnahme und ist aus den Laufzeitbibliotheken entfernt. Verwende sie nicht, auch nicht „nur für interne Dateien“ – JSON oder XML sind sicherer, lesbarer und zwischen Versionen deines Programms robuster.
{: .notice--warning}

Übung: Modelliere eine Klasse `Rezept` mit Titel, Portionen und einer `List<Zutat>` (Name, Menge, Einheit). Serialisiere ein Rezept einmal mit `XmlSerializer` und einmal mit `JsonSerializer` und vergleiche die Dateigrößen. Welche Attribute brauchst du, damit die Zutaten als `<zutat menge="200" einheit="g">Mehl</zutat>` erscheinen?
{: .notice--info}

## Weitere Quellen

- [XML-Serialisierung – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/serialization/introducing-xml-serialization)
- [`XmlSerializer`-Klasse – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.xml.serialization.xmlserializer)
- [Sicherheitsrisiken von BinaryFormatter – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/serialization/binaryformatter-security-guide)
