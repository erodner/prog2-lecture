---
title: "Dateien und Verzeichnisse"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Alle Variablen, Listen und Objekte, die wir bisher angelegt haben, leben nur so lange, wie das Programm läuft. Unser Adventure zeigt gleich zwei Seiten dieses Problems: Die Level stecken in `EingebauteLevelQuelle` als String-Arrays im Quelltext – wer ein neues Level bauen will, muss das Programm neu übersetzen –, und ein Spielstand geht beim Schließen des Fensters restlos verloren. Daten müssen also **persistiert** werden: in einer Datenbank, in der Cloud oder – als einfachster Fall – in einer Datei auf der Festplatte. Für Dateien und Verzeichnisse bringt .NET den Namensraum `System.IO` mit. Wer eine Datei anlegen, lesen, prüfen und in einem Verzeichnis wiederfinden kann, hat das Handwerkszeug für alles Weitere in dieser Vorlesung.

## Text schreiben und lesen mit `File`

Die statische Klasse `File` bietet für die häufigsten Fälle jeweils eine einzige Methode, die eine Datei öffnet, den Inhalt komplett schreibt oder liest und die Datei wieder schließt. Man braucht also weder ein Objekt anzulegen noch etwas aufzuräumen:

```csharp
string pfad = Path.Combine(Path.GetTempPath(), "arena.txt");

File.WriteAllText(pfad, "#####");                          // erstellt oder überschreibt
File.AppendAllText(pfad, Environment.NewLine + "#@.E#");   // hängt ans Ende an

string alles = File.ReadAllText(pfad);
string[] zeilen = File.ReadAllLines(pfad);
Console.WriteLine(zeilen.Length); // 2
Console.WriteLine(zeilen[1]);     // #@.E#
```

`WriteAllText` legt die Datei an, falls sie nicht existiert – und überschreibt sie kommentarlos, falls doch. Wer Inhalte behalten möchte, nimmt `AppendAllText`. `Environment.NewLine` liefert den Zeilenumbruch des jeweiligen Betriebssystems (`\r\n` unter Windows, `\n` unter Linux und macOS), damit die Datei überall korrekt aussieht.

Für zeilenorientierte Daten gibt es die Gegenstücke `WriteAllLines` und `ReadAllLines`, die mit beliebigen `IEnumerable<string>` arbeiten – also auch mit einer `List<string>` oder dem Ergebnis einer LINQ-Abfrage. Ein Level ist genau so etwas: eine Liste von Zeichenketten.

```csharp
List<string> karte = ["#####", "#@kD#", "#..E#", "#####"];
File.WriteAllLines(pfad, karte);

foreach (string zeile in File.ReadAllLines(pfad))
{
    Console.WriteLine(zeile);
}
```

Diese `All`-Methoden laden immer die **gesamte** Datei in den Speicher. Für Level, Spielstände oder ein paar tausend Zeilen ist das ideal. Für eine Logdatei von mehreren Gigabyte ist es keine gute Idee – dafür gibt es [Streams](/modules/streams/streams.md).
{: .notice--primary}

## Pfade plattformneutral bauen

In den Beispielen oben taucht `Path.Combine` auf, und das ist kein Zufall. Windows trennt Verzeichnisse mit `\`, Linux und macOS mit `/`. Wer Pfade als feste Zeichenkette zusammenklebt, schreibt Code, der nur auf einem System läuft. `Path.Combine` setzt das richtige Trennzeichen ein und kümmert sich um doppelte oder fehlende Schrägstriche:

```csharp
string ordner = Path.Combine(AppContext.BaseDirectory, "levels");
string datei = Path.Combine(ordner, "kerker.txt");

Console.WriteLine(Path.GetFileName(datei));                  // kerker.txt
Console.WriteLine(Path.GetFileNameWithoutExtension(datei));  // kerker
Console.WriteLine(Path.GetExtension(datei));                 // .txt
```

`AppContext.BaseDirectory` ist der Ordner, in dem die gebaute Anwendung liegt – genau von dort holt sich `Adventure.Konsole` seine Level. Für Daten, die der Benutzer selbst anlegt, ist dagegen ein Systemordner die richtige Wahl: `Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)` liefert „Dokumente“ unabhängig vom Betriebssystem. Falls du in fremdem Code doch einmal Windows-Pfade als Literal siehst, dann meist als **Verbatim-String** mit `@`: In `@"C:\Spiele\Adventure"` ist `\` kein Escape-Zeichen, sodass man es nicht als `\\` verdoppeln muss.

Backslashes in Pfad-Literalen wie `"C:\\spiele\\levels.txt"` sind gleich doppelt problematisch: Sie funktionieren nur unter Windows, und ohne Verbatim-`@` wird aus `\n` ein Zeilenumbruch und aus `\t` ein Tabulator. Im Zweifel `Path.Combine`.
{: .notice--warning}

## Ein Ordner voller Level: `Directory.GetFiles`

Damit können wir die erste Einschränkung des Spiels beseitigen. Im Repository liegt neben den Projekten ein Ordner `levels/` mit den Dateien `kerker.txt`, `katakomben.txt` und `schatzkammer.txt`. Die Klasse `TextdateiLevelQuelle` in `Adventure.Daten` liest ihn aus – und weil sie dasselbe Interface `ILevelQuelle` erfüllt wie die eingebaute Variante, merkt der Rest des Spiels nichts von der Umstellung:

```csharp
public class TextdateiLevelQuelle : ILevelQuelle
{
    private readonly string ordner;

    public TextdateiLevelQuelle(string ordner)
    {
        if (!Directory.Exists(ordner))
        {
            throw new DirectoryNotFoundException($"Level-Ordner '{ordner}' nicht gefunden.");
        }
        this.ordner = ordner;
    }

    public IReadOnlyList<string> LevelNamen =>
        Directory.GetFiles(ordner, "*.txt")
            .Select(Path.GetFileNameWithoutExtension)
            .OrderBy(n => n)
            .ToList()!;
```

`GetFiles` liefert alle passenden **vollständigen Pfade** als `string[]`; das Muster `"*.txt"` filtert nach Dateiendung. Aus `/…/levels/kerker.txt` macht `Path.GetFileNameWithoutExtension` den Levelnamen `kerker` – die Methode ist hier als [Methodengruppe](/modules/func_action/func_action.md) an `Select` übergeben. Das anschließende `OrderBy` sorgt dafür, dass die Reihenfolge nicht vom Dateisystem abhängt: Ohne es liefert derselbe Ordner unter Windows und Linux womöglich eine andere Liste, und das Spiel startet mit einem anderen Level.

Bei sehr großen Verzeichnissen ist `Directory.EnumerateFiles` die bessere Wahl: Es gibt ein `IEnumerable<string>` zurück, das die Einträge erst liefert, wenn man darüber iteriert – genau das Prinzip der [verzögerten Ausführung](/modules/linq_deferred_execution/linq_deferred_execution.md). Schreibt man umgekehrt in ein Verzeichnis, das es noch nicht gibt, legt man es vorher mit `Directory.CreateDirectory(ordner)` an; die Methode tut nichts, wenn der Ordner schon existiert, und erzeugt bei Bedarf auch alle Zwischenverzeichnisse.

## Existiert die Datei? `File.Exists` und `FileInfo`

Bevor man eine Datei liest, sollte man wissen, ob es sie gibt. Genau damit beginnt die Lade-Methode derselben Klasse:

```csharp
    public Level Laden(string name)
    {
        string pfad = Path.Combine(ordner, name + ".txt");
        if (!File.Exists(pfad))
        {
            throw new FileNotFoundException($"Es gibt kein Level namens '{name}'.", pfad);
        }
        // ... zeilenweise lesen, siehe Modul "Streams"
    }
```

Die Methode prüft nicht nur, sondern wirft eine **sprechende** Ausnahme: Der Text nennt den Levelnamen, den der Benutzer eingegeben hat, und der zweite Konstruktorparameter hält den Pfad fest, an dem gesucht wurde. Das ist deutlich hilfreicher als die Standardmeldung, die .NET beim Öffnen einer fehlenden Datei erzeugt.

Wer mehr über eine Datei wissen möchte als ihre bloße Existenz, erzeugt ein `FileInfo`-Objekt und fragt dessen Properties ab:

```csharp
FileInfo info = new FileInfo(pfad);
Console.WriteLine($"{info.Name}: {info.Length} Bytes");    // kerker.txt: 189 Bytes
Console.WriteLine($"Liegt in: {info.DirectoryName}");
Console.WriteLine($"Zuletzt geändert: {info.LastWriteTime}");
```

`FileInfo` ist die objektorientierte Sicht auf *eine* Datei, `File` die statische Werkzeugkiste für schnelle Einzelaufrufe. Beide können im Kern dasselbe – `info.Exists` und `File.Exists(pfad)` liefern dieselbe Antwort.

## Wie kommen die Level neben das Programm?

Ein `.txt` im Projektordner landet nicht von allein im Ausgabeverzeichnis, in dem die gebaute `.dll` liegt. Deshalb steht in `Adventure.Konsole.csproj` eine Zeile, die die Level-Dateien beim Bauen mitkopiert:

```xml
<ItemGroup>
  <None Include="..\levels\*.txt" Link="levels\%(Filename)%(Extension)" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

`None` heißt „keine Quelldatei, die übersetzt wird“, `CopyToOutputDirectory="PreserveNewest"` kopiert nur, wenn sich die Datei geändert hat, und `Link` bestimmt, wo sie im Ausgabeordner landet. Damit findet `Path.Combine(AppContext.BaseDirectory, "levels")` zur Laufzeit genau die Dateien, die im Repository neben den Projekten liegen. Wer ein neues Level `arena.txt` in `levels/` anlegt, muss keine einzige Zeile C# ändern – es taucht beim nächsten `dotnet run` in `LevelNamen` auf.

## Wenn etwas schiefgeht: Ausnahmen beim Dateizugriff

Dateizugriffe sind der klassische Fall für Laufzeitfehler, die nicht am eigenen Code liegen: Die Datei wurde gelöscht, der USB-Stick abgezogen, der Ordner ist schreibgeschützt. Solche Situationen meldet .NET als Ausnahmen, die wir mit [`try`/`catch` aus Programmierung 1](https://www.erodner.de/prog-lecture/modules/try_catch/try_catch/) abfangen:

```csharp
try
{
    Level level = quelle.Laden(levelName);
    Spielfeld feld = LevelParser.Parsen(level);
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"Level nicht gefunden: {ex.Message}");
}
catch (DirectoryNotFoundException)
{
    Console.WriteLine("Der Level-Ordner fehlt – wurde das Projekt richtig gebaut?");
}
catch (UnauthorizedAccessException)
{
    Console.WriteLine("Keine Berechtigung für diese Datei.");
}
catch (IOException ex)
{
    Console.WriteLine($"Allgemeiner Ein-/Ausgabefehler: {ex.Message}");
}
```

Die Reihenfolge der `catch`-Blöcke ist wichtig: `FileNotFoundException` und `DirectoryNotFoundException` sind Unterklassen von `IOException`. Stünde der allgemeine `IOException`-Block zuerst, würde er alles abfangen und die spezifischen Blöcke wären unerreichbar – der Compiler meldet das als Fehler. `UnauthorizedAccessException` ist dagegen keine `IOException`, sondern steht für sich.

Ein `File.Exists`-Aufruf vor dem Lesen ersetzt die Ausnahmebehandlung nicht: Zwischen Prüfung und Zugriff kann ein anderes Programm die Datei löschen. Prüfen ist gut für die Benutzerführung, `try`/`catch` bleibt trotzdem nötig.
{: .notice--primary}

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v09-daten`).

Übung: Erweitere `TextdateiLevelQuelle` um eine Methode `void Speichern(Level level)`, die die Zeilen eines Levels als `<name>.txt` in den Ordner schreibt. Lege den Ordner bei Bedarf mit `Directory.CreateDirectory` an, verweigere Namen, die `Path.GetInvalidFileNameChars()` enthalten, und prüfe anschließend mit `LevelNamen`, dass das neue Level auftaucht.
{: .notice--info}

## Weitere Quellen

- [File-Klasse – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.io.file)
- [FileInfo-Klasse – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.io.fileinfo)
- [Path-Klasse – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.io.path)
- [Behandeln von E/A-Fehlern – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/io/handling-io-errors)
- [RogueBasin](https://www.roguebasin.com/index.php/Main_Page) – Fundgrube für Kerker-Generatoren und Kartenformate, wenn dein `levels/`-Ordner wachsen soll.
- [Vorgehensweise: Lesen von Text aus einer Datei – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/io/how-to-read-text-from-a-file)
