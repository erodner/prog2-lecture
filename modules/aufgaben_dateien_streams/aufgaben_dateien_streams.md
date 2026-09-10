---
title: "🧩 Aufgaben und Beispiele: Dateien, Streams und Serialisierung"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Programmieren lernt man nicht nur durch Codezeilen tippen – sondern auch durch **Nachdenken**. Die folgenden Aufgaben trainieren *Computational Thinking*: die Fähigkeit, Probleme so zu strukturieren, dass ein Computer sie lösen kann. Dazu gehören Abstraktion, Zerlegung, Mustererkennung und Algorithmenentwurf. Bei Dateien und Netzwerk kommt eine Zutat hinzu, die in den bisherigen Vorlesungen kaum eine Rolle spielte: Die Außenwelt hält sich nicht an unsere Annahmen. Nimm dir für jede Aufgabe Zeit, bevor du die Lösung aufklappst.

## Aufgabe 1 — Mustererkennung

Die folgende Methode soll eine Liste von Notizen in eine Datei sichern. Sie kompiliert – und läuft trotzdem in drei voneinander unabhängige Probleme. Finde sie, ohne den Code auszuführen.

```csharp
static void Sichern(List<string> notizen)
{
    string pfad = "C:\\Daten\\Notizen\\sicherung.txt";
    FileStream fs = new FileStream(pfad, FileMode.Open, FileAccess.Write);
    StreamWriter writer = new StreamWriter(fs);
    foreach (string notiz in notizen)
    {
        writer.WriteLine(notiz);
    }
    Console.WriteLine($"{new FileInfo(pfad).Length} Bytes gesichert.");
}
```

- Was gibt die letzte Zeile aus, wenn alles „funktioniert“ – und warum?
- Was passiert beim allerersten Aufruf auf einem frischen Rechner? Was, wenn die Datei vorher *länger* war als die neuen Notizen?
- Auf welchen Betriebssystemen läuft die Methode überhaupt?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Das fehlende `using`:** Weder `fs` noch `writer` werden freigegeben. Der `StreamWriter` puffert, deshalb meldet `FileInfo.Length` in der letzten Zeile **0 Bytes**, obwohl `WriteLine` mehrfach aufgerufen wurde – die Daten liegen noch im Arbeitsspeicher. Die Datei bleibt zudem gesperrt, bis der Garbage Collector irgendwann den Finalizer ausführt; ob die Notizen dann noch auf der Platte landen, ist Glückssache.

**Schritt 2 — Der falsche `FileMode`:** `FileMode.Open` verlangt, dass die Datei existiert – beim ersten Aufruf gibt es eine `FileNotFoundException`. Existiert sie, wird sie *nicht* geleert: Der Stream schreibt ab Position 0 über den alten Inhalt, und war dieser länger, bleibt sein Ende als Müll stehen. Für „komplett neu schreiben“ ist `FileMode.Create` gemeint.

**Schritt 3 — Der hart codierte Pfad:** `C:\Daten\...` existiert nur unter Windows, und selbst dort nur, wenn jemand das Verzeichnis angelegt hat. Pfade gehören mit `Path.Combine` aus einem Systemordner zusammengebaut, das Verzeichnis wird bei Bedarf erzeugt.

```csharp
static void Sichern(List<string> notizen)
{
    string ordner = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Notizen");
    Directory.CreateDirectory(ordner);
    string pfad = Path.Combine(ordner, "sicherung.txt");

    using (StreamWriter writer = new StreamWriter(pfad, append: false))
    {
        foreach (string notiz in notizen)
        {
            writer.WriteLine(notiz);
        }
    }   // erst hier ist alles auf der Platte
    Console.WriteLine($"{new FileInfo(pfad).Length} Bytes gesichert.");
}
```

**Zentrale Designentscheidungen:**

- **`using`-Block statt -Deklaration:** Die Datei muss *vor* der `Console.WriteLine`-Zeile geschlossen sein, sonst zeigt `Length` wieder den Puffer-Stand.
- **`StreamWriter` direkt mit Pfad:** `append: false` entspricht `FileMode.Create` – ein Objekt weniger, das freigegeben werden muss.
- **`Directory.CreateDirectory` ohne `Exists`-Prüfung:** Die Methode tut nichts, wenn der Ordner schon da ist.

</details>

## Aufgabe 2 — Algorithmenentwurf

Eine Textdatei ist mehrere Gigabyte groß – etwa ein Wikipedia-Export. Entwirf eine Methode, die die zehn häufigsten Wörter samt Anzahl ausgibt.

- Warum scheidet `File.ReadAllText` aus, und wie liest man stattdessen?
- Wie wird eine Zeile in Wörter zerlegt? Wie gehst du mit `Haus`, `haus` und `Haus,` um?
- Welche Datenstruktur zählt effizient? Wie groß wird sie – hängt das von der Dateigröße ab?
- Wie testest du die Methode, ohne eine Riesendatei zu brauchen?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Zeilenweise lesen:** Die Datei passt nicht in den Speicher, aber eine einzelne Zeile schon. Ein `StreamReader` liefert Zeile für Zeile; der Speicherbedarf bleibt konstant. Die Methode nimmt einen `TextReader` entgegen statt eines Pfads – so lässt sie sich im Test mit einem `StringReader` füttern.

**Schritt 2 — Wörter normalisieren:** `Split` mit einer Liste von Trennzeichen entfernt Satzzeichen, `ToLowerInvariant` macht `Haus` und `haus` gleich. Das ist eine bewusste Vereinfachung – Bindestriche und Apostrophe bleiben Grenzfälle.

**Schritt 3 — Zählen mit `Dictionary`:** Ein [`Dictionary<string, int>`](https://www.erodner.de/prog-lecture/modules/dictionary/dictionary/) hat pro *verschiedenem* Wort einen Eintrag. Seine Größe hängt vom Wortschatz ab (einige hunderttausend Einträge), nicht von der Dateigröße.

```csharp
static Dictionary<string, int> WoerterZaehlen(TextReader quelle)
{
    Dictionary<string, int> haeufigkeit = new();
    char[] trenner = [' ', '\t', '.', ',', ';', ':', '!', '?', '"', '(', ')'];

    while (quelle.ReadLine() is string zeile)
    {
        foreach (string wort in zeile.Split(trenner, StringSplitOptions.RemoveEmptyEntries))
        {
            string schluessel = wort.ToLowerInvariant();
            haeufigkeit[schluessel] = haeufigkeit.GetValueOrDefault(schluessel) + 1;
        }
    }
    return haeufigkeit;
}

using StreamReader reader = File.OpenText(pfad);
var top10 = WoerterZaehlen(reader)
    .OrderByDescending(paar => paar.Value)
    .Take(10);
foreach (var (wort, anzahl) in top10)
{
    Console.WriteLine($"{wort,-15} {anzahl}");
}
```

**Zentrale Designentscheidungen:**

- **`TextReader` als Parameter:** Die Zählung weiß nichts von Dateien. `new StringReader("das Haus. Das haus!")` genügt für einen Unit-Test.
- **`GetValueOrDefault` statt `ContainsKey` + Zugriff:** Ein Lookup pro Wort statt zwei – bei Milliarden Wörtern zählt das.
- **Sortieren erst am Ende:** Die Top 10 ergeben sich mit LINQ aus dem fertigen Dictionary; während des Lesens wäre Sortieren unnötig teuer.

</details>

## Aufgabe 3 — Abstraktion

Ein Adressbuch soll als JSON gespeichert werden. Ein Kontakt hat Vor- und Nachnamen, optional einen Geburtstag, beliebig viele Adressen (jeweils mit Art wie „privat“ oder „dienstlich“, Straße, PLZ, Ort) und beliebig viele Telefonnummern. Entwirf zuerst die JSON-Struktur, dann die C#-Klassen.

- Ist die Wurzel ein Array von Kontakten oder ein Objekt? Was spricht für was?
- Wie stellst du „kein Geburtstag bekannt“ dar? Wie die Art einer Adresse?
- Was muss an den Klassen gelten, damit `System.Text.Json` sie ohne Sonderbehandlung liest und schreibt?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — JSON-Struktur:** Die Wurzel ist ein Objekt, kein Array. So gibt es Platz für Metadaten wie eine `version`, mit der ein späteres Programm alte Dateien erkennen und konvertieren kann. Verschachtelte Dinge bleiben verschachtelt: Adressen sind Objekte in einem Array, keine zusammengeklebten Strings.

```json
{
  "version": 1,
  "kontakte": [
    {
      "vorname": "Ada",
      "nachname": "Lovelace",
      "geburtstag": "1815-12-10",
      "adressen": [
        { "art": "privat", "strasse": "St. James's Square 12", "plz": "SW1Y", "ort": "London" }
      ],
      "telefonnummern": ["+44 20 1234"]
    },
    { "vorname": "Max", "nachname": "Muster", "geburtstag": null, "adressen": [], "telefonnummern": [] }
  ]
}
```

**Schritt 2 — Klassen:** Jede JSON-Ebene wird eine Klasse mit öffentlichen Properties. `DateOnly?` bildet den optionalen Geburtstag ab, ein `enum` die Adressart. Listen werden mit `[]` initialisiert, damit ein frisch angelegter Kontakt nie `null`-Listen hat.

```csharp
enum AdressArt { Privat, Dienstlich }

class Adresse
{
    public AdressArt Art { get; set; }
    public string Strasse { get; set; } = "";
    public string Plz { get; set; } = "";
    public string Ort { get; set; } = "";
}

class Kontakt
{
    public string Vorname { get; set; } = "";
    public string Nachname { get; set; } = "";
    public DateOnly? Geburtstag { get; set; }
    public List<Adresse> Adressen { get; set; } = [];
    public List<string> Telefonnummern { get; set; } = [];
}

class Adressbuch
{
    public int Version { get; set; } = 1;
    public List<Kontakt> Kontakte { get; set; } = [];
}

JsonSerializerOptions optionen = new()
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
};
```

**Zentrale Designentscheidungen:**

- **Wurzelobjekt mit `version`:** Kostet eine Zeile, spart später Migrationsschmerzen. Ein nacktes Array kann man nachträglich nicht erweitern, ohne alle alten Dateien ungültig zu machen.
- **`enum` als String:** Ohne `JsonStringEnumConverter` würde `Privat` als `0` gespeichert – unlesbar, und beim Umsortieren des Enums stimmen alte Dateien nicht mehr.
- **Keine Rückreferenz `Adresse.Kontakt`:** Sie wäre praktisch, würde aber einen Zyklus erzeugen, den der Serialisierer nicht abbilden kann. Wer sie braucht, markiert sie mit `[JsonIgnore]` und setzt sie nach dem Laden.

</details>

## Aufgabe 4 — Zerlegung

Der Geometrieeditor soll seine Figuren alternativ als CSV speichern können, damit sie sich in einer Tabellenkalkulation öffnen lassen. Entwirf `CsvFigurSpeicher : IFigurSpeicher`.

- Wie sieht eine Zeile aus, wenn Rechteck, Kreis und Dreieck unterschiedlich viele Werte haben?
- In welche Teilschritte zerfällt `Laden`? Welche davon lassen sich isoliert testen?
- Was passiert bei einer unbekannten Typkennung, einer fehlenden Spalte, einem `2,5` statt `2.5` – abbrechen oder überspringen?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Format festlegen:** Eine Kopfzeile, dann pro Figur eine Zeile mit festen sieben Spalten `typ;name;x;y;a;b;c`. Nicht genutzte Spalten bleiben leer. Die Typkennung übernimmt die Rolle des `typ`-Diskriminators aus `Figur.cs`; Zahlen werden mit `CultureInfo.InvariantCulture` geschrieben, damit auf einem deutschen System kein `2,5` entsteht, das mit dem Trennzeichen kollidiert.

```
typ;name;x;y;a;b;c
rechteck;r1;0;0;4;3;
kreis;k1;10;5;2.5;;
dreieck;d1;-3;7;3;4;5
```

**Schritt 2 — Zerlegung in Methoden:** `Speichern` und `Laden` sind nur Schleifen um eine Datei. Die eigentliche Arbeit steckt in zwei kleinen Übersetzern: `ZeileFuer(Figur)` und `FigurAus(string, int)`. Beide sind `static`, brauchen keine Datei und lassen sich mit einem einzigen String testen.

```csharp
public class CsvFigurSpeicher : IFigurSpeicher
{
    private static readonly CultureInfo inv = CultureInfo.InvariantCulture;
    private readonly string pfad;

    public CsvFigurSpeicher(string pfad) => this.pfad = pfad;

    public void Speichern(IEnumerable<Figur> figuren)
    {
        using StreamWriter writer = File.CreateText(pfad);
        writer.WriteLine("typ;name;x;y;a;b;c");
        foreach (Figur f in figuren) writer.WriteLine(ZeileFuer(f));
    }

    public List<Figur> Laden()
    {
        List<Figur> figuren = [];
        if (!File.Exists(pfad)) return figuren;

        using StreamReader reader = File.OpenText(pfad);
        reader.ReadLine();                       // Kopfzeile überspringen
        int nummer = 1;
        while (reader.ReadLine() is string zeile)
        {
            nummer++;
            if (zeile.Trim().Length > 0) figuren.Add(FigurAus(zeile, nummer));
        }
        return figuren;
    }

    private static string ZeileFuer(Figur f)
    {
        string basis = $"{f.Name};{f.X.ToString(inv)};{f.Y.ToString(inv)}";
        return f switch
        {
            Rechteck r => $"rechteck;{basis};{r.Breite.ToString(inv)};{r.Hoehe.ToString(inv)};",
            Kreis k    => $"kreis;{basis};{k.Radius.ToString(inv)};;",
            Dreieck d  => $"dreieck;{basis};{d.SeiteA.ToString(inv)};{d.SeiteB.ToString(inv)};{d.SeiteC.ToString(inv)}",
            _ => throw new NotSupportedException($"CSV kennt den Typ {f.GetType().Name} nicht.")
        };
    }

    private static Figur FigurAus(string zeile, int nummer)
    {
        string[] felder = zeile.Split(';');
        if (felder.Length != 7)
            throw new FormatException($"Zeile {nummer}: 7 Spalten erwartet, {felder.Length} gefunden.");

        double Zahl(int i) => double.TryParse(felder[i], NumberStyles.Float, inv, out double wert)
            ? wert
            : throw new FormatException($"Zeile {nummer}: '{felder[i]}' ist keine Zahl.");

        return felder[0] switch
        {
            "rechteck" => new Rechteck(felder[1], Zahl(2), Zahl(3), Zahl(4), Zahl(5)),
            "kreis"    => new Kreis(felder[1], Zahl(2), Zahl(3), Zahl(4)),
            "dreieck"  => new Dreieck(felder[1], Zahl(2), Zahl(3), Zahl(4), Zahl(5), Zahl(6)),
            _ => throw new FormatException($"Zeile {nummer}: unbekannter Typ '{felder[0]}'.")
        };
    }
}
```

**Schritt 3 — Fehlerfälle:** Der Speicher bricht bei der ersten defekten Zeile mit einer `FormatException` ab, die die Zeilennummer nennt. Ein Dreieck mit unmöglichen Seiten scheitert wie beim JSON-Laden an der `ArgumentException` des Konstruktors – die Validierung bleibt im Fachkonzept.

**Zentrale Designentscheidungen:**

- **Abbrechen statt überspringen:** Stillschweigend ausgelassene Figuren würde der Benutzer erst beim nächsten Speichern bemerken – dann sind sie endgültig weg. Eine Ausnahme mit Zeilennummer lässt sich in der GUI als Meldung anzeigen.
- **Feste Spaltenzahl:** Einfacher zu parsen und zu prüfen als Zeilen variabler Länge; die leeren Spalten kosten nichts.
- **Bekannte Grenze:** Ein Name mit `;` zerstört die Zeile. Wer das braucht, quotiert die Felder oder greift zu einer CSV-Bibliothek – und hat spätestens dann ein Argument für JSON.

</details>
