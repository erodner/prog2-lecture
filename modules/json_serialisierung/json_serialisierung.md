---
title: "JSON-Serialisierung"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Mit `File` und Streams können wir Text und Bytes auf die Platte bringen. Aber unsere Programme arbeiten nicht mit Text, sondern mit Objekten: einem `Rechteck` mit Breite und Höhe, einer `List<Figur>`, einem Adressbuch voller `Kontakt`-Objekte mit verschachtelten Adressen. Diese Objekte Feld für Feld von Hand in Zeilen zu schreiben und beim Lesen wieder zusammenzusetzen, ist mühsam und fehleranfällig. **Serialisierung** nimmt uns das ab: Ein Objektgraph wird in eine flache Darstellung verwandelt – wie ein Möbelstück, das für den Umzug zerlegt und in Kartons verpackt wird –, und die **Deserialisierung** baut daraus wieder das Original. Das mit Abstand wichtigste Format dafür ist heute JSON.

## JSON in einer Minute

JSON (*JavaScript Object Notation*) ist ein Textformat, das nur wenige Bausteine kennt: Objekte in `{}` mit `"name": wert`-Paaren, Arrays in `[]`, Zeichenketten, Zahlen, `true`/`false` und `null`. Weil es so einfach ist, kann praktisch jede Programmiersprache es lesen und schreiben – und Menschen können es im Editor prüfen:

```json
{
  "Name": "k1",
  "X": 10,
  "Y": 5,
  "Radius": 2.5,
  "Tags": ["rund", "klein"],
  "Kommentar": null
}
```

Die Ähnlichkeit zu einem C#-Objekt mit Properties ist kein Zufall. Genau diese Übersetzung – Property-Name wird zum Schlüssel, Property-Wert wird zum Wert – übernimmt in .NET der Namensraum `System.Text.Json`.

## `Serialize` und `Deserialize<T>`

Die statische Klasse `JsonSerializer` erledigt beide Richtungen mit je einem Aufruf. Wir nehmen den `Kreis` aus dem Geometrieeditor, den wir seit [Vorlesung 02](/lectures/02/02.md) kennen:

```csharp
Kreis kreis = new Kreis("k1", 10, 5, 2.5);

string json = JsonSerializer.Serialize(kreis);
Console.WriteLine(json);
// {"Radius":2.5,"Flaeche":19.634954084936208,"Umfang":15.707963267948966,"Name":"k1","X":10,"Y":5}

Kreis? kopie = JsonSerializer.Deserialize<Kreis>(json);
Console.WriteLine(kopie?.Beschreibung()); // k1 bei (10, 5) mit Fläche 19,63 (r = 2,5)
```

`Serialize` liefert einen `string`, den man mit `File.WriteAllText` speichern kann; `Deserialize<T>` bekommt den Typ als generischen Parameter, weil der Text allein nicht verrät, welche Klasse er beschreibt. Der Rückgabetyp ist `Kreis?` – aus dem JSON-Literal `null` würde nämlich `null` entstehen.

Zwei Dinge fallen an der Ausgabe auf. Erstens tauchen `Flaeche` und `Umfang` auf, obwohl sie nur berechnet werden: Der Serialisierer schreibt **alle öffentlichen Properties**, auch die nur lesbaren; beim Laden werden sie schlicht ignoriert. Zweitens stehen die Properties der abgeleiteten Klasse vor denen der Basisklasse – für JSON spielt die Reihenfolge keine Rolle. Private Felder werden nie serialisiert; was gespeichert werden soll, muss eine öffentliche Property sein.

## Konstruktoren und Property-Namen

Wie erzeugt `Deserialize` überhaupt einen `Kreis`, der gar keinen parameterlosen Konstruktor hat? `System.Text.Json` sucht den öffentlichen Konstruktor und ordnet jeden Parameter anhand seines Namens einer Property zu (Groß-/Kleinschreibung spielt dabei keine Rolle). Das funktioniert nur, wenn die Namen zusammenpassen – deshalb steht im `Dreieck` des Geometrieeditors dieser Kommentar:

```csharp
// Die Parameternamen entsprechen den Property-Namen – das braucht System.Text.Json beim Laden.
public Dreieck(string name, double x, double y, double seiteA, double seiteB, double seiteC)
    : base(name, x, y)
{
    if (seiteA + seiteB <= seiteC || seiteA + seiteC <= seiteB || seiteB + seiteC <= seiteA)
    {
        throw new ArgumentException("Die Seitenlängen ergeben kein Dreieck.");
    }
    ...
}
```

Hieße der Parameter `a` statt `seiteA`, könnte der Serialisierer ihn keiner Property zuordnen und würde beim Laden eine Ausnahme werfen. Der Konstruktor bringt einen zweiten Vorteil: Seine Validierung läuft auch beim Deserialisieren. Ein JSON mit den Seiten 1, 1 und 5 endet mit der `ArgumentException` aus dem Konstruktor – ein unmögliches Dreieck kann also auch über den Umweg Datei nicht entstehen.

## Optionen und Attribute

Die einzeilige Ausgabe ist für Maschinen gedacht. Für Dateien, die Menschen öffnen, konfiguriert man den Serialisierer über `JsonSerializerOptions`. Dasselbe Objekt muss man dann auch beim Deserialisieren übergeben:

```csharp
JsonSerializerOptions optionen = new()
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};

Console.WriteLine(JsonSerializer.Serialize(kreis, optionen));
// {
//   "radius": 2.5,
//   "flaeche": 19.634954084936208,
//   ...
//   "name": "k1",
//   "x": 10,
//   "y": 5
// }
```

`WriteIndented` sorgt für Zeilenumbrüche und Einrückung, `PropertyNamingPolicy` übersetzt die C#-üblichen PascalCase-Namen in das in JSON-APIs übliche camelCase. Wenn ein einzelner Name im JSON ganz anders lauten soll – etwa weil eine fremde API ihn vorgibt – oder eine Property gar nicht gespeichert werden soll, helfen Attribute aus `System.Text.Json.Serialization`:

```csharp
class Kontakt
{
    public string Name { get; set; } = "";

    [JsonPropertyName("e_mail")]
    public string Email { get; set; } = "";

    [JsonIgnore]
    public string Passwort { get; set; } = "";
}

Console.WriteLine(JsonSerializer.Serialize(new Kontakt { Name = "Ada", Email = "ada@example.org", Passwort = "geheim" }));
// {"Name":"Ada","e_mail":"ada@example.org"}
```

`[JsonPropertyName]` hat Vorrang vor jeder `PropertyNamingPolicy`. `[JsonIgnore]` ist die richtige Wahl für alles, was nicht in eine Datei gehört – Passwörter, Zwischenergebnisse oder Referenzen zurück auf ein Elternobjekt, die sonst zu einer Endlosschleife beim Serialisieren führen würden.

## Polymorphie: Figuren gemischt speichern

Der Geometrieeditor verwaltet keine `Kreis`-Liste, sondern eine `List<Figur>` mit Rechtecken, Kreisen und Dreiecken durcheinander. Beim Laden steht der Serialisierer vor einem Problem: `Figur` ist abstrakt, und aus `{"Name":"k1","X":10,"Y":5,"Radius":2.5}` allein kann er nicht wissen, dass ein `Kreis` gemeint ist. Die Lösung ist ein **Diskriminator** – ein zusätzliches Feld, das den konkreten Typ benennt. In `Figur.cs` ist er so deklariert:

```csharp
// Damit System.Text.Json abstrakte Figuren speichern und wieder laden kann,
// bekommt jede abgeleitete Klasse einen Namen im JSON ("typ": "kreis").
[JsonPolymorphic(TypeDiscriminatorPropertyName = "typ")]
[JsonDerivedType(typeof(Rechteck), "rechteck")]
[JsonDerivedType(typeof(Kreis), "kreis")]
[JsonDerivedType(typeof(Dreieck), "dreieck")]
public abstract class Figur
{
    ...
}
```

`[JsonPolymorphic]` schaltet die Unterstützung ein und legt den Namen des Feldes fest; je ein `[JsonDerivedType]` verknüpft eine Unterklasse mit ihrem Kürzel. Die Kürzel sind bewusst kurze Strings und keine .NET-Typnamen – so bleibt die Datei lesbar, und man kann Klassen später umbenennen, ohne alte Dateien zu verlieren. Serialisiert man nun über den **Basistyp**, erscheint das Feld ganz vorn:

```csharp
List<Figur> figuren = [new Rechteck("r1", 0, 0, 4, 3), new Kreis("k1", 10, 5, 2.5)];
Console.WriteLine(JsonSerializer.Serialize(figuren, new JsonSerializerOptions { WriteIndented = true }));
// [
//   {
//     "typ": "rechteck",
//     "Breite": 4,
//     "Hoehe": 3,
//     ...
//   },
//   {
//     "typ": "kreis",
//     "Radius": 2.5,
//     ...
//   }
// ]

List<Figur> geladen = JsonSerializer.Deserialize<List<Figur>>(json)!;
Console.WriteLine(geladen[1].GetType().Name); // Kreis
```

Beim Laden liest der Serialisierer zuerst `typ`, wählt die passende Klasse und ruft deren Konstruktor auf. Das Ergebnis ist eine Liste mit den richtigen Laufzeittypen, sodass `Flaeche` wieder virtuell aufgelöst wird – genau wie in [Vorlesung 01](/lectures/01/01.md) besprochen.

Der Diskriminator muss die **erste** Property jedes Objekts sein. Fehlt `typ` oder steht es weiter hinten, bricht `Deserialize` mit einer `NotSupportedException` ab. Wer eine JSON-Datei von Hand schreibt oder bearbeitet, muss also nicht nur die Property-Namen treffen, sondern auch diese Reihenfolge einhalten. Das Serialisieren über eine Variable vom Typ `Kreis` statt `Figur` erzeugt übrigens *kein* `typ`-Feld – Polymorphie greift nur, wenn der deklarierte Typ die Basisklasse ist.
{: .notice--warning}

## `JsonFigurSpeicher`: Datenhaltung für den Geometrieeditor

Damit haben wir alles für einen Speicher, der den Geometrieeditor über das Programmende hinaus retten kann. Im Modul [Schichten mit Blazor](/modules/schichten_mit_blazor/schichten_mit_blazor.md) hat das Fachkonzept mit `IFigurSpeicher` festgelegt, *was* ein Speicher können muss – `Speichern` und `Laden` –, ohne sich für das *Wie* zu interessieren. Der `ArbeitsspeicherFigurSpeicher` war die erste Umsetzung; hier ist die zweite, gekürzt aus `Geometrieeditor.Datenhaltung/JsonFigurSpeicher.cs`:

```csharp
public class JsonFigurSpeicher : IFigurSpeicher
{
    private readonly string pfad;

    private static readonly JsonSerializerOptions optionen = new()
    {
        WriteIndented = true
    };

    public JsonFigurSpeicher(string pfad)
    {
        this.pfad = pfad;
    }

    public void Speichern(IEnumerable<Figur> figuren)
    {
        string json = JsonSerializer.Serialize(figuren, optionen);
        File.WriteAllText(pfad, json);
    }

    public List<Figur> Laden()
    {
        if (!File.Exists(pfad))
        {
            return new List<Figur>();
        }
        string json = File.ReadAllText(pfad);
        return JsonSerializer.Deserialize<List<Figur>>(json, optionen) ?? new List<Figur>();
    }
}
```

Die ganze Klasse besteht aus einem `Serialize`- und einem `Deserialize`-Aufruf plus dem Dateizugriff aus dem Modul [Dateien und Verzeichnisse](/modules/dateien_verzeichnisse/dateien_verzeichnisse.md). Die Polymorphie-Attribute an `Figur` erledigen den Rest. Die Optionen liegen in einem `static readonly`-Feld, weil `JsonSerializerOptions` intern Metadaten über die Typen aufbaut und deren Wiederverwendung deutlich schneller ist als ein neues Objekt pro Aufruf.

Der Clou steckt in einer einzigen Zeile der GUI-Schicht, in der `Program.cs` von `Geometrieeditor.Web`:

```csharp
builder.Services.AddScoped<IFigurSpeicher>(_ => new JsonFigurSpeicher("figuren.json"));
```

Vorher stand hier `new ArbeitsspeicherFigurSpeicher()`. Keine Seite, kein Dialog und keine Zeile in `FigurenVerwaltung` musste angepasst werden – das ist der versprochene Lohn der Schichtenarchitektur: Die Datenhaltung lässt sich austauschen, ohne dass die Oberfläche davon erfährt. Das vollständige Projekt findest du im Repository unter `examples/03_blazor/Geometrieeditor`; die Tests in `Geometrieeditor.Tests/JsonFigurSpeicherTests.cs` prüfen genau, dass nach Speichern und Laden die konkreten Typen erhalten bleiben.

Übung: Öffne die vom Geometrieeditor erzeugte `figuren.json` im Editor, ändere bei einem Dreieck `SeiteC` auf `100` und starte das Programm neu. Welche Ausnahme siehst du, und in welcher Schicht sollte sie abgefangen werden – in `JsonFigurSpeicher`, in `FigurenVerwaltung` oder im Fenster?
{: .notice--info}

## Weitere Quellen

- [Übersicht über System.Text.Json – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/serialization/system-text-json/overview)
- [Serialisieren von Eigenschaften abgeleiteter Klassen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/serialization/system-text-json/polymorphism)
- [Unveränderliche Typen und Konstruktoren – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/serialization/system-text-json/immutability)
- [Anpassen von Eigenschaftennamen und -werten – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/serialization/system-text-json/customize-properties)
