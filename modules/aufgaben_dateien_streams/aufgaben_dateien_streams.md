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

Die folgende Methode soll ein im Editor gebautes Level in den Level-Ordner sichern. Sie kompiliert – und läuft trotzdem in drei voneinander unabhängige Probleme. Finde sie, ohne den Code auszuführen.

```csharp
static void LevelSichern(Level level)
{
    string pfad = "C:\\Spiele\\Adventure\\levels\\" + level.Name + ".txt";
    FileStream fs = new FileStream(pfad, FileMode.Open, FileAccess.Write);
    StreamWriter writer = new StreamWriter(fs);
    foreach (string zeile in level.Zeilen)
    {
        writer.WriteLine(zeile);
    }
    Console.WriteLine($"{new FileInfo(pfad).Length} Bytes gesichert.");
}
```

- Was gibt die letzte Zeile aus, wenn alles „funktioniert“ – und warum?
- Was passiert beim allerersten Sichern eines neuen Levels? Was, wenn die alte Karte *größer* war als die neue?
- Auf welchen Betriebssystemen läuft die Methode überhaupt?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Das fehlende `using`:** Weder `fs` noch `writer` werden freigegeben. Der `StreamWriter` puffert, deshalb meldet `FileInfo.Length` in der letzten Zeile **0 Bytes**, obwohl `WriteLine` für jede Zeile aufgerufen wurde – die Karte liegt noch im Arbeitsspeicher. Die Datei bleibt zudem gesperrt, sodass ein anschließendes `TextdateiLevelQuelle.Laden` mit einer `IOException` scheitert; ob die Zeilen jemals auf der Platte landen, ist Glückssache.

**Schritt 2 — Der falsche `FileMode`:** `FileMode.Open` verlangt, dass die Datei existiert – ein neues Level endet mit einer `FileNotFoundException`. Existiert die Datei, wird sie *nicht* geleert: Der Stream schreibt ab Position 0 über den alten Inhalt, und war die alte Karte größer, bleiben deren letzte Zeilen stehen. Beim nächsten Laden hat das Level plötzlich Wände, die nie jemand gezeichnet hat. Gemeint ist `FileMode.Create`.

**Schritt 3 — Der hart codierte Pfad:** `C:\Spiele\...` existiert nur unter Windows, und selbst dort nur, wenn jemand das Verzeichnis angelegt hat. Der Ordner gehört als Feld in die Klasse (wie in `TextdateiLevelQuelle`), der Pfad wird mit `Path.Combine` gebaut, und das Verzeichnis wird bei Bedarf erzeugt.

```csharp
static void LevelSichern(string ordner, Level level)
{
    Directory.CreateDirectory(ordner);
    string pfad = Path.Combine(ordner, level.Name + ".txt");

    using (StreamWriter writer = new StreamWriter(pfad, append: false))
    {
        foreach (string zeile in level.Zeilen)
        {
            writer.WriteLine(zeile);
        }
    }   // erst hier ist alles auf der Platte und die Datei wieder frei
    Console.WriteLine($"{new FileInfo(pfad).Length} Bytes gesichert.");
}
```

**Zentrale Designentscheidungen:**

- **`using`-Block statt -Deklaration:** Die Datei muss *vor* der `Console.WriteLine`-Zeile geschlossen sein, sonst zeigt `Length` wieder den Puffer-Stand.
- **`StreamWriter` direkt mit Pfad:** `append: false` entspricht `FileMode.Create` – ein Objekt weniger, das freigegeben werden muss.
- **Ordner als Parameter:** Die Methode entscheidet nicht mehr selbst, wo Level liegen. Damit lässt sie sich im Test auf ein Temp-Verzeichnis richten.

</details>

## Aufgabe 2 — Algorithmenentwurf

Das Spiel soll nach jeder gewonnenen Partie einen Eintrag in eine Datei `highscores.csv` anhängen und beim Start die fünf besten anzeigen. Eine Zeile hat die Form `name;level;punkte;runden`; Zeilen, die mit `#` beginnen, sind Kommentare. Die Datei wird von Spielern auch von Hand bearbeitet – es stehen also kaputte Zeilen darin.

- Wie liest man die Datei, wenn sie über die Jahre auf zehntausende Zeilen anwächst?
- Was passiert bei einer Zeile mit drei statt vier Feldern oder mit `zwölf` statt `12`? Abbrechen oder überspringen?
- Welche Sortierung ergibt eine sinnvolle Bestenliste? Was ist der Unterschied zwischen 300 Punkten in 20 Runden und 300 Punkten in 90 Runden?
- Wie hängst du einen neuen Eintrag an, ohne die ganze Datei neu zu schreiben? Wie testest du das alles ohne Datei?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Zeilenweise lesen, nicht alles auf einmal:** Für die Top 5 braucht niemand die ganze Datei im Speicher. Ein `StreamReader` (oder `File.ReadLines`, das intern genau das tut) liefert Zeile für Zeile. Die Lesemethode bekommt einen `TextReader` statt eines Pfads – damit kennt sie kein Dateisystem und lässt sich mit einem `StringReader` testen.

**Schritt 2 — Robust parsen, kaputte Zeilen überspringen:** Eine Bestenliste ist kein kritisches Dokument. Eine unlesbare Zeile darf den Start des Spiels nicht verhindern; sie wird gezählt und übersprungen. `int.TryParse` prüft und wandelt in einem Schritt, ohne Exception.

```csharp
public record Highscore(string Name, string Level, int Punkte, int Runden);

public static IEnumerable<Highscore> Lesen(TextReader quelle, Action<string>? warnung = null)
{
    int nummer = 0;
    while (quelle.ReadLine() is string zeile)
    {
        nummer++;
        if (zeile.Trim().Length == 0 || zeile.StartsWith('#')) continue;

        string[] f = zeile.Split(';');
        if (f.Length != 4 || !int.TryParse(f[2], out int punkte) || !int.TryParse(f[3], out int runden))
        {
            warnung?.Invoke($"Zeile {nummer} übersprungen: '{zeile}'");
            continue;
        }
        yield return new Highscore(f[0], f[1], punkte, runden);
    }
}
```

**Schritt 3 — Sortieren und anhängen:** Viele Punkte sind besser, und bei Gleichstand gewinnt, wer weniger Runden gebraucht hat – das sind zwei Kriterien, also `OrderByDescending` gefolgt von `ThenBy`. Das Anhängen braucht die alten Zeilen gar nicht zu kennen: `append: true` springt ans Dateiende, der Aufwand ist unabhängig von der Dateigröße.

```csharp
using StreamReader leser = File.OpenText(pfad);
foreach (Highscore h in Lesen(leser, Console.Error.WriteLine)
             .OrderByDescending(h => h.Punkte)
             .ThenBy(h => h.Runden)
             .Take(5))
{
    Console.WriteLine($"{h.Name,-12} {h.Level,-14} {h.Punkte,5} Punkte in {h.Runden} Runden");
}

public static void Anhaengen(string pfad, Highscore h)
{
    using StreamWriter writer = new StreamWriter(pfad, append: true);
    writer.WriteLine($"{h.Name};{h.Level};{h.Punkte};{h.Runden}");
}
```

**Zentrale Designentscheidungen:**

- **`TextReader` als Parameter:** Die Auswertung weiß nichts von Dateien. `new StringReader("#kommentar\nAda;kerker;300;20")` genügt für einen Unit-Test – genau der Trick, den wir in [Vorlesung 12](/lectures/12/12.md) brauchen werden.
- **Überspringen statt abbrechen:** Anders als beim Spielstand (Aufgabe 4) ist ein verlorener Eintrag kein Schaden. Wer die Warnung sehen will, gibt ein `Action<string>` mit; wer nicht, lässt es weg.
- **`yield return` statt Liste:** Die Methode liefert die Einträge verzögert, sodass `Take(5)` nach fünf Treffern nicht mehr weiterlesen müsste, wenn die Datei bereits sortiert wäre.
- **Anhängen statt neu schreiben:** Ein Absturz mitten im Schreiben kann so höchstens die letzte Zeile beschädigen, nicht die ganze Bestenliste.

</details>

## Aufgabe 3 — Abstraktion

Für das Adventure soll ein grafischer **Level-Editor** entstehen. Das bisherige Format – eine reine Textkarte – reicht dafür nicht mehr: Der Editor will auch einen Titel, einen Autor, eine Schwierigkeit und vor allem Einstellungen für einzelne Objekte speichern (wie wertvoll *diese* Truhe ist, wie stark *dieser* Trank heilt, in welche Richtung *diese* Wache losläuft). Entwirf das JSON-Format, dann die C#-Klassen.

- Bleibt die Karte eine Liste von Zeilen, oder wird jedes Feld ein JSON-Objekt? Was spricht für was?
- Wo bringst du die Zusatzangaben unter, ohne die Karte unlesbar zu machen?
- Wie sorgst du dafür, dass ein Spiel von morgen eine Datei von heute noch lesen kann – und umgekehrt?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Die Karte bleibt Text:** Die naheliegende Idee, jedes Feld als `{"x":3,"y":7,"art":"trank"}` zu speichern, bläht eine 20 × 9-Karte auf 180 Objekte auf – und niemand erkennt darin noch einen Raum. Die ASCII-Karte ist kompakt, im Editor *und* im Texteditor lesbar, und Git zeigt Änderungen zeilenweise an. Sie bleibt also, und der vorhandene `LevelParser` funktioniert unverändert weiter.

**Schritt 2 — Zusatzangaben als Ausnahmen, nicht als Wiederholung:** Die Objektliste beschreibt nur, was vom Standard abweicht. Eine Truhe ohne Eintrag ist 100 Punkte wert, ein Trank heilt 1 – so bleiben die meisten Level kurz. Die Wurzel ist ein Objekt (kein Array), damit Metadaten wie `version` Platz haben.

```json
{
  "version": 1,
  "name": "kerker",
  "autor": "ada",
  "schwierigkeit": "leicht",
  "karte": [
    "####################",
    "#@.....#...........#",
    "#......#.....W.....#",
    "#..k...#...........#",
    "#......D...........#",
    "#......#...V.......#",
    "#......#.......T...#",
    "#..!...#..........E#",
    "####################"
  ],
  "objekte": [
    { "position": { "x": 15, "y": 6 }, "art": "truhe", "wert": 250 },
    { "position": { "x": 13, "y": 2 }, "art": "wache", "laufrichtung": "Links" },
    { "position": { "x": 3, "y": 7 }, "art": "trank", "heilung": 2 }
  ]
}
```

**Schritt 3 — Klassen dazu:** Jede JSON-Ebene wird eine Klasse mit öffentlichen Properties – dieselbe Regel wie beim `Spielstand`. Optionale Angaben werden `int?`: `null` heißt „nimm den Standardwert“, und `0` bleibt dadurch ein gültiger, ausdrücklich gewünschter Wert.

```csharp
class LevelDatei
{
    public int Version { get; set; } = 1;
    public string Name { get; set; } = "";
    public string? Autor { get; set; }
    public string? Schwierigkeit { get; set; }
    public List<string> Karte { get; set; } = [];
    public List<ObjektDaten> Objekte { get; set; } = [];
}

class ObjektDaten
{
    public Position Position { get; set; }
    public string Art { get; set; } = "";
    public int? Wert { get; set; }
    public int? Heilung { get; set; }
    public Richtung? Laufrichtung { get; set; }
}

JsonSerializerOptions optionen = new()
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    Converters = { new JsonStringEnumConverter() }
};
```

**Zentrale Designentscheidungen:**

- **Wurzelobjekt mit `version`:** Kostet eine Zeile, spart später Migrationsschmerzen. Ein nacktes Array kann man nachträglich nicht erweitern, ohne alle alten Dateien ungültig zu machen. Ein Spiel, das `version > 1` liest, kann eine verständliche Meldung zeigen statt abzustürzen.
- **Unbekannte Felder ignorieren:** `System.Text.Json` überliest Properties, die es nicht kennt. Eine ältere Programmversion kann eine neuere Datei also weiterhin spielen, nur ohne die neuen Feinheiten – das ist die andere Hälfte der Abwärtskompatibilität.
- **`enum` als String:** Ohne `JsonStringEnumConverter` würde `Richtung.Links` als `2` gespeichert – unlesbar, und beim Umsortieren des Enums stimmen alle alten Level nicht mehr.
- **Kein zweites Wahrheitsmodell:** Die Objektliste *ergänzt* die Karte, sie ersetzt sie nicht. Gäbe es beide Quellen für dieselbe Information, müsste man bei jedem Widerspruch entscheiden, welche gewinnt.

</details>

## Aufgabe 4 — Zerlegung

Ein Spielstand soll sich alternativ in einem zeilenbasierten Textformat speichern lassen, das man mit jedem Editor reparieren kann. Entwirf `CsvSpielstandSpeicher : ISpielstandSpeicher` mit `void Speichern(Spielstand)` und `Spielstand? Laden()`.

- `Spielstand` enthält fünf Listen unterschiedlicher Länge. Wie bildet man das in einem flachen Zeilenformat ab – mit festen Spalten oder anders?
- In welche Teilschritte zerfällt das Speichern und Laden? Welche davon lassen sich ohne Datei testen?
- Was tust du bei einer unbekannten Kennung, einer fehlenden Spalte, einem `zwölf` statt `12` – abbrechen oder überspringen? Warum hier anders als bei den Highscores?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Format festlegen:** Feste Spalten scheitern, weil ein Spielstand mal zwei und mal sieben Gegner hat. Stattdessen beginnt jede Zeile mit einer **Kennung**, die sagt, was danach kommt; Listen werden zu mehreren Zeilen derselben Kennung. Das Format ist damit erweiterbar, ohne alte Dateien zu brechen.

```
version;1
level;kerker
runde;33
spieler;15;5
leben;2
punkte;100
entfernt;3;3
tuer;7;4
truhe;15;6
gegner;8;2
gegner;15;4
```

**Schritt 2 — Zerlegung in Methoden:** `Speichern` und `Laden` sind nur der Dateizugriff. Die eigentliche Arbeit steckt in zwei Übersetzern, `ZeilenFuer` und `StandAus`, die beide `static` sind, keine Datei kennen und sich mit einem String-Array prüfen lassen.

```csharp
public class CsvSpielstandSpeicher : ISpielstandSpeicher
{
    private readonly string pfad;

    public CsvSpielstandSpeicher(string pfad) => this.pfad = pfad;

    public void Speichern(Spielstand stand) => File.WriteAllLines(pfad, ZeilenFuer(stand));

    public Spielstand? Laden() => File.Exists(pfad) ? StandAus(File.ReadLines(pfad)) : null;

    internal static IEnumerable<string> ZeilenFuer(Spielstand s)
    {
        yield return "version;1";
        yield return $"level;{s.LevelName}";
        yield return $"runde;{s.Runde}";
        yield return $"spieler;{s.SpielerPosition.X};{s.SpielerPosition.Y}";
        yield return $"leben;{s.Lebenspunkte}";
        yield return $"punkte;{s.Punkte}";
        foreach (string name in s.Inventar) yield return $"inventar;{name}";
        foreach (Position p in s.EntfernteGegenstaende) yield return $"entfernt;{p.X};{p.Y}";
        foreach (Position p in s.OffeneTueren) yield return $"tuer;{p.X};{p.Y}";
        foreach (Position p in s.GeoeffneteTruhen) yield return $"truhe;{p.X};{p.Y}";
        foreach (Position p in s.GegnerPositionen) yield return $"gegner;{p.X};{p.Y}";
    }

    internal static Spielstand StandAus(IEnumerable<string> zeilen)
    {
        Spielstand s = new();
        int nummer = 0;
        foreach (string zeile in zeilen)
        {
            nummer++;
            if (zeile.Trim().Length == 0) continue;
            string[] f = zeile.Split(';');
            Position Pos() => new(Zahl(f, 1, nummer), Zahl(f, 2, nummer));

            switch (f[0])
            {
                case "version": break;                               // für spätere Formatwechsel
                case "level": s.LevelName = Feld(f, 1, nummer); break;
                case "runde": s.Runde = Zahl(f, 1, nummer); break;
                case "spieler": s.SpielerPosition = Pos(); break;
                case "leben": s.Lebenspunkte = Zahl(f, 1, nummer); break;
                case "punkte": s.Punkte = Zahl(f, 1, nummer); break;
                case "inventar": s.Inventar.Add(Feld(f, 1, nummer)); break;
                case "entfernt": s.EntfernteGegenstaende.Add(Pos()); break;
                case "tuer": s.OffeneTueren.Add(Pos()); break;
                case "truhe": s.GeoeffneteTruhen.Add(Pos()); break;
                case "gegner": s.GegnerPositionen.Add(Pos()); break;
                default: throw new FormatException($"Zeile {nummer}: unbekannte Kennung '{f[0]}'.");
            }
        }
        return s;
    }

    private static string Feld(string[] f, int i, int nummer) => i < f.Length
        ? f[i]
        : throw new FormatException($"Zeile {nummer}: Feld {i} fehlt.");

    private static int Zahl(string[] f, int i, int nummer) =>
        int.TryParse(Feld(f, i, nummer), out int wert)
            ? wert
            : throw new FormatException($"Zeile {nummer}: '{f[i]}' ist keine Zahl.");
}
```

**Schritt 3 — Fehlerfälle:** Der Speicher bricht bei der ersten defekten Zeile mit einer `FormatException` ab, die die Zeilennummer nennt – die Spielschleife kann daraus „Spielstand beschädigt“ machen. Das Gegenteil wäre gefährlich: Würde man die Zeile `tuer;7;4` stillschweigend überspringen, stünde der Held hinter einer plötzlich verschlossenen Tür ohne Schlüssel und käme nie wieder heraus. Bei den Highscores in Aufgabe 2 kostet eine verlorene Zeile dagegen nichts.

**Zentrale Designentscheidungen:**

- **Kennung statt fester Spaltenzahl:** Listen beliebiger Länge passen ohne Verrenkungen hinein, und eine neue Kennung bricht keine alte Datei – solange das Laden unbekannte Zeilen entweder kennt oder ehrlich ablehnt.
- **Reine Übersetzer, getrennt vom Dateizugriff:** `ZeilenFuer` und `StandAus` sind als Paar testbar (`StandAus(ZeilenFuer(stand))` muss den Ausgangswert ergeben), ohne dass ein Test je eine Datei anlegt.
- **`Laden` liefert `null` statt einer Exception, wenn die Datei fehlt:** Das ist der Vertrag aus `ISpielstandSpeicher`, den auch der `JsonSpielstandSpeicher` erfüllt – „noch nie gespeichert“ ist kein Fehler.
- **Bekannte Grenze:** Ein Levelname mit `;` zerstört die Zeile. Wer das braucht, quotiert die Felder – und hat damit ein sehr gutes Argument für JSON: `JsonSpielstandSpeicher` erledigt dasselbe in drei Zeilen und kümmert sich um Sonderzeichen von allein.

</details>
