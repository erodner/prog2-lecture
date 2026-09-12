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

Mit `File` und Streams können wir Text und Bytes auf die Platte bringen. Aber unsere Programme arbeiten nicht mit Text, sondern mit Objekten: einem `Spieler` mit Lebenspunkten, Punkten und Inventar, einem `Spielfeld` voller Türen, Truhen und Gegner. Diese Objekte Feld für Feld von Hand in Zeilen zu schreiben und beim Lesen wieder zusammenzusetzen, ist mühsam und fehleranfällig. **Serialisierung** nimmt uns das ab: Ein Objektgraph wird in eine flache Darstellung verwandelt – wie ein Möbelstück, das für den Umzug zerlegt und in Kartons verpackt wird –, und die **Deserialisierung** baut daraus wieder das Original. Das mit Abstand wichtigste Format dafür ist heute JSON. Am Ende dieses Moduls kann unser Adventure mit F5 speichern und mit F9 weiterspielen.

## JSON in einer Minute

JSON (*JavaScript Object Notation*) ist ein Textformat, das nur wenige Bausteine kennt: Objekte in `{}` mit `"name": wert`-Paaren, Arrays in `[]`, Zeichenketten, Zahlen, `true`/`false` und `null`. Weil es so einfach ist, kann praktisch jede Programmiersprache es lesen und schreiben – und Menschen können es im Editor prüfen:

```json
{ "LevelName": "kerker", "Lebenspunkte": 2,
  "Inventar": ["Schlüssel"], "IstFertig": false, "Bestzeit": null }
```

Die Ähnlichkeit zu einem C#-Objekt mit Properties ist kein Zufall. Genau diese Übersetzung – Property-Name wird zum Schlüssel, Property-Wert wird zum Wert – übernimmt in .NET der Namensraum `System.Text.Json`.

## Warum wir das Spielfeld *nicht* serialisieren

Der naheliegende Gedanke wäre, einfach das ganze `Spielfeld` in eine Datei zu schreiben. Das geht schief, und zwar aus mehreren Richtungen gleichzeitig: `Spielfeld` hält ein `Dictionary<Position, StatischesObjekt>` mit einem *abstrakten* Werttyp, `StatischesObjekt` hat keinen parameterlosen Konstruktor, eine `Truhe` verweist auf ihren `Schatz`, und das Ereignis `RundeBeendet` zeigt auf Handler, die es nach dem Laden gar nicht mehr gibt. Selbst wenn man all das mit Attributen erzwingen könnte, wäre die Datei ein Abbild unserer heutigen Klassenstruktur – jede Umbenennung einer Klasse würde alte Spielstände unbrauchbar machen.

Deshalb speichern wir nicht das Spiel, sondern **das, was man braucht, um es fortzusetzen**: das Level, in dem gespielt wird, plus alle Abweichungen vom Ausgangszustand. Die Klasse `Spielstand` in `Adventure.Kern` ist genau diese Liste:

```csharp
/// <summary>
/// Alles, was man braucht, um ein laufendes Spiel später fortzusetzen.
/// Bewusst nur Daten, keine Logik – so lässt es sich als JSON speichern.
/// </summary>
public class Spielstand
{
    public string LevelName { get; set; } = "";
    public int Runde { get; set; }
    public Position SpielerPosition { get; set; }
    public int Lebenspunkte { get; set; }
    public int Punkte { get; set; }
    public List<string> Inventar { get; set; } = new();
    public List<Position> EntfernteGegenstaende { get; set; } = new();
    public List<Position> OffeneTueren { get; set; } = new();
    public List<Position> GeoeffneteTruhen { get; set; } = new();
    public List<Position> GegnerPositionen { get; set; } = new();
}
```

Drei Entwurfsentscheidungen stecken darin. Erstens: **keine Logik**. Die Klasse hat keine Methode, keine Prüfung, keinen Konstruktor – sie ist eine Transportkiste, deren einziger Zweck der Weg durch die Datei ist. Zweitens: **öffentliche `get`- und `set`-Zugriffe** an jeder Property. `System.Text.Json` schreibt nur öffentliche Properties und braucht beim Laden eine Möglichkeit, sie zu setzen; ein `private set` wie bei `Spieler.Lebenspunkte` bliebe beim Deserialisieren leer. Drittens: **Positionen statt Objektverweise**. Statt „diese Tür ist offen“ steht in der Datei „die Tür auf Feld (7, 4) ist offen“ – das überlebt jede Umbenennung im Kern und ist beim Lesen sofort verständlich.

Ein solcher Spielstand ist genau das, was das Entwurfsmuster *Memento* beschreibt: ein Schnappschuss des Zustands, den das Objekt selbst erzeugt und wieder einspielt – bei uns mit `Spielfeld.Erfassen(levelName, level)` und `Spielfeld.Wiederherstellen(level, stand)`.

## `Serialize` und `Deserialize<T>`

Die statische Klasse `JsonSerializer` erledigt beide Richtungen mit je einem Aufruf:

```csharp
Spielstand stand = feld.Erfassen("kerker", level);
string json = JsonSerializer.Serialize(stand);
// {"LevelName":"kerker","Runde":31,"SpielerPosition":{"X":15,"Y":7}, ... }

Spielstand? zurueck = JsonSerializer.Deserialize<Spielstand>(json);
Console.WriteLine(zurueck?.Punkte);   // 100
```

`Serialize` liefert einen `string`, den man mit `File.WriteAllText` speichern kann; `Deserialize<T>` bekommt den Typ als generischen Parameter, weil der Text allein nicht verrät, welche Klasse er beschreibt. Der Rückgabetyp ist `Spielstand?` – aus dem JSON-Literal `null` würde nämlich `null` entstehen.

Die einzeilige Ausgabe ist für Maschinen gedacht. Für Dateien, die Menschen öffnen, konfiguriert man den Serialisierer über `JsonSerializerOptions`, und `WriteIndented = true` sorgt für Zeilenumbrüche und Einrückung. So sieht ein echter Spielstand aus, nachdem der Held den Schlüssel geholt, die Tür aufgeschlossen und die Truhe geplündert hat:

```json
{
  "LevelName": "kerker",
  "Runde": 31,
  "SpielerPosition": {
    "X": 15,
    "Y": 7
  },
  "Lebenspunkte": 2,
  "Punkte": 100,
  "Inventar": [],
  "EntfernteGegenstaende": [
    {
      "X": 3,
      "Y": 3
    }
  ],
  "OffeneTueren": [
    {
      "X": 7,
      "Y": 4
    }
  ],
  "GeoeffneteTruhen": [
    {
      "X": 15,
      "Y": 6
    }
  ],
  "GegnerPositionen": [
    {
      "X": 13,
      "Y": 2
    },
    {
      "X": 11,
      "Y": 5
    }
  ]
}
```

Man kann die Datei lesen wie einen Bericht: Runde 31, der Held steht auf (15, 7) mit zwei von drei Lebenspunkten und 100 Punkten, sein Inventar ist leer (der Schlüssel wurde für die Tür verbraucht und liegt deshalb auch nicht mehr auf (3, 3)), die Tür auf (7, 4) ist offen, die Truhe auf (15, 6) geplündert, und die beiden Gegner stehen auf (13, 2) und (11, 5).

Interessant ist, was mit `Position` passiert. Der Typ ist ein `readonly record struct` mit den Properties `X` und `Y` – und genau die schreibt der Serialisierer als verschachteltes JSON-Objekt. Beim Laden findet er den Konstruktor `Position(int X, int Y)`, ordnet jeden Parameter anhand seines **Namens** einer Property zu (Groß-/Kleinschreibung spielt keine Rolle) und ruft ihn auf. Dass Positionen dadurch pro Eintrag vier Zeilen brauchen, ist der Preis der Lesbarkeit; ein eigener Konverter könnte daraus `"15,7"` machen.

## Attribute: Namen ändern, Properties auslassen

Wenn ein Name im JSON anders lauten soll als in C# – etwa weil eine fremde API ihn vorgibt – oder eine Property gar nicht gespeichert werden soll, helfen Attribute aus `System.Text.Json.Serialization`:

```csharp
[JsonPropertyName("level")]
public string LevelName { get; set; } = "";

[JsonIgnore]
public DateTime Gespeichert { get; set; } = DateTime.Now;
// {"level":"kerker","Runde":31, ... }   – "Gespeichert" taucht nicht auf
```

`[JsonPropertyName]` hat Vorrang vor jeder `PropertyNamingPolicy` (etwa `JsonNamingPolicy.CamelCase`, die alle Namen in das in Web-APIs übliche `levelName` übersetzt). `[JsonIgnore]` ist die richtige Wahl für alles, was nicht in eine Datei gehört: berechnete Zwischenergebnisse, Passwörter oder Referenzen zurück auf ein Elternobjekt, die sonst zu einer Endlosschleife beim Serialisieren führen würden.

## Ausblick: Polymorphie mit `[JsonPolymorphic]`

Was wäre, wenn wir doch einmal eine `List<Spielobjekt>` mit Wänden, Türen und Truhen durcheinander speichern wollten? Beim Laden stünde der Serialisierer vor einem Problem: `Spielobjekt` ist abstrakt, und aus `{"Name":"Tür","Position":{"X":7,"Y":4}}` allein kann er nicht wissen, dass eine `Tuer` gemeint ist. Die Lösung ist ein **Diskriminator** – ein zusätzliches Feld, das den konkreten Typ benennt:

```csharp
[JsonPolymorphic(TypeDiscriminatorPropertyName = "typ")]
[JsonDerivedType(typeof(Wand), "wand")]
[JsonDerivedType(typeof(Tuer), "tuer")]
public abstract class StatischesObjekt : Spielobjekt { /* ... */ }
// [ { "typ": "tuer", "Name": "Tür", ... }, { "typ": "wand", ... } ]
```

Die Kürzel sind bewusst kurze Strings und keine .NET-Typnamen – so bleibt die Datei lesbar, und man kann Klassen später umbenennen, ohne alte Dateien zu verlieren. Der Diskriminator muss die **erste** Property jedes Objekts sein; fehlt er, bricht `Deserialize` mit einer `NotSupportedException` ab.

Unser Spiel braucht das nicht, und das ist eine bewusste Entscheidung: Ein Spielstand voller serialisierter Spielobjekte wäre um ein Vielfaches größer, würde Konstruktoren und `sealed`-Klassen gegen sich haben und bei jeder Änderung im Kern kaputtgehen. Level + Abweichungen ist die robustere Darstellung.
{: .notice--warning}

## `JsonSpielstandSpeicher`: Datenhaltung für das Spiel

Damit haben wir alles für einen Speicher, der das Spiel über das Programmende hinaus rettet. `Adventure.Kern` legt mit dem Interface fest, *was* ein Speicher können muss, ohne sich für das *Wie* zu interessieren:

```csharp
/// <summary>Wo Spielstände landen (Datei, Browser, Datenbank), ist dem Spiel egal.</summary>
public interface ISpielstandSpeicher
{
    void Speichern(Spielstand spielstand);
    Spielstand? Laden();
}
```

Die Umsetzung in `Adventure.Daten` besteht dann aus je einem Serialisierer- und einem Dateiaufruf:

```csharp
public class JsonSpielstandSpeicher : ISpielstandSpeicher
{
    private static readonly JsonSerializerOptions optionen = new() { WriteIndented = true };
    private readonly string pfad;

    public JsonSpielstandSpeicher(string pfad) => this.pfad = pfad;

    public void Speichern(Spielstand spielstand)
    {
        File.WriteAllText(pfad, JsonSerializer.Serialize(spielstand, optionen));
    }

    public Spielstand? Laden()
    {
        if (!File.Exists(pfad)) return null;
        return JsonSerializer.Deserialize<Spielstand>(File.ReadAllText(pfad), optionen);
    }
}
```

`Laden` liefert `null`, wenn es noch keine Datei gibt – „kein Spielstand vorhanden“ ist kein Fehler, sondern der Normalfall beim ersten Start. Die Optionen liegen in einem `static readonly`-Feld, weil `JsonSerializerOptions` intern Metadaten über die Typen aufbaut und deren Wiederverwendung deutlich schneller ist als ein neues Objekt pro Aufruf.

In `Adventure.Konsole/Program.cs` hängen daran nur noch zwei Tasten – hier ohne die Hinweismeldungen, die der Spieler danach zu sehen bekommt:

```csharp
ISpielstandSpeicher speicher = new JsonSpielstandSpeicher("spielstand.json");
// ...
if (taste == ConsoleKey.F5) speicher.Speichern(feld.Erfassen(levelName, level));

if (taste == ConsoleKey.F9 && speicher.Laden() is Spielstand stand)
{
    levelName = stand.LevelName;
    level = levelQuelle.Laden(levelName);     // das Level kommt aus der ILevelQuelle …
    feld = Spielfeld.Wiederherstellen(level, stand);   // … die Abweichungen aus dem Spielstand
}
```

Weil die Spielschleife nur `ISpielstandSpeicher` kennt, ließe sich die JSON-Datei jederzeit gegen eine Datenbank oder den `localStorage` des Browsers austauschen, ohne eine Zeile im Kern zu ändern – derselbe Lohn der Schichtenarchitektur, den wir schon bei `ILevelQuelle` gesehen haben.

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v09-daten`).

Übung: Speichere ein Spiel mit F5, öffne `spielstand.json` im Editor und setze `Lebenspunkte` auf `99`. Lade mit F9 – wie viele Lebenspunkte hat der Held, und in welcher Methode wird das begrenzt? Entferne anschließend eine Position aus `OffeneTueren` und überlege, warum das Spiel danach trotzdem läuft, während ein `"Runde": "viele"` eine `JsonException` auslöst.
{: .notice--info}

## Weitere Quellen

- [Übersicht über System.Text.Json – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/serialization/system-text-json/overview)
- [Serialisieren von Eigenschaften abgeleiteter Klassen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/serialization/system-text-json/polymorphism)
- [Unveränderliche Typen und Konstruktoren – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/serialization/system-text-json/immutability)
- [Anpassen von Eigenschaftennamen und -werten – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/serialization/system-text-json/customize-properties)
