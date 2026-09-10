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

Alle Variablen, Listen und Objekte, die wir bisher angelegt haben, leben nur so lange, wie das Programm läuft. Ein Spielstand, der beim Beenden verloren geht, ist aber wertlos – und ein Geometrieeditor, der jede Figur vergisst, ebenso. Daten müssen also **persistiert** werden: in einer Datenbank, in der Cloud oder – als einfachster Fall – in einer Datei auf der Festplatte. Für Dateien und Verzeichnisse bringt .NET den Namensraum `System.IO` mit, dessen wichtigste Klassen du in diesem Modul kennenlernst. Wer eine Datei anlegen, lesen, anhängen, prüfen und in einem Verzeichnis wiederfinden kann, hat das Handwerkszeug für alles Weitere in dieser Vorlesung.

## Text schreiben und lesen mit `File`

Die statische Klasse `File` bietet für die häufigsten Fälle jeweils eine einzige Methode, die eine Datei öffnet, den Inhalt komplett schreibt oder liest und die Datei wieder schließt. Man braucht also weder ein Objekt anzulegen noch etwas aufzuräumen:

```csharp
string pfad = Path.Combine(Path.GetTempPath(), "notizen.txt");

File.WriteAllText(pfad, "Hallo erst mal");                      // erstellt oder überschreibt
File.AppendAllText(pfad, Environment.NewLine + "Zweite Zeile"); // hängt ans Ende an

string alles = File.ReadAllText(pfad);
string[] zeilen = File.ReadAllLines(pfad);
Console.WriteLine(zeilen.Length); // 2
Console.WriteLine(zeilen[1]);     // Zweite Zeile
```

`WriteAllText` legt die Datei an, falls sie nicht existiert – und überschreibt sie kommentarlos, falls doch. Wer Inhalte behalten möchte, nimmt `AppendAllText`. `Environment.NewLine` liefert den Zeilenumbruch des jeweiligen Betriebssystems (`\r\n` unter Windows, `\n` unter Linux und macOS), damit die Datei überall korrekt aussieht.

Für zeilenorientierte Daten gibt es die Gegenstücke `WriteAllLines` und `ReadAllLines`, die mit beliebigen `IEnumerable<string>` arbeiten – also auch mit einer `List<string>` oder dem Ergebnis einer LINQ-Abfrage:

```csharp
List<string> einkauf = ["Milch", "Brot", "Kaffee"];
File.WriteAllLines(pfad, einkauf);

foreach (string zeile in File.ReadAllLines(pfad))
{
    Console.WriteLine($"- {zeile}");
}
// - Milch
// - Brot
// - Kaffee
```

Diese `All`-Methoden laden immer die **gesamte** Datei in den Speicher. Für Konfigurationen, Spielstände oder ein paar tausend Zeilen ist das ideal. Für eine Logdatei von mehreren Gigabyte ist es keine gute Idee – dafür gibt es [Streams](/modules/streams/streams.md).
{: .notice--primary}

## Pfade plattformneutral bauen

In den Beispielen oben taucht `Path.Combine` auf, und das ist kein Zufall. Windows trennt Verzeichnisse mit `\`, Linux und macOS mit `/`. Wer Pfade als feste Zeichenkette zusammenklebt, schreibt Code, der nur auf einem System läuft. `Path.Combine` setzt das richtige Trennzeichen ein und kümmert sich um doppelte oder fehlende Schrägstriche:

```csharp
string dokumente = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
string ordner = Path.Combine(dokumente, "Geometrieeditor");
string datei = Path.Combine(ordner, "figuren.json");

Console.WriteLine(Path.GetFileName(datei));   // figuren.json
Console.WriteLine(Path.GetExtension(datei));  // .json
Console.WriteLine(Path.GetDirectoryName(datei) == ordner); // True
```

`Environment.GetFolderPath` liefert Systemordner wie „Dokumente“ oder das Benutzerprofil unabhängig vom Betriebssystem – ein Programm sollte seine Daten dort ablegen und nicht in einem fest verdrahteten `C:\Daten`. Falls du in fremdem Code doch einmal Windows-Pfade als Literal siehst, dann meist als **Verbatim-String** mit `@`: In `@"C:\Schroedinger\katze.txt"` ist `\` kein Escape-Zeichen, sodass man es nicht als `\\` verdoppeln muss.

Backslashes in Pfad-Literalen wie `"C:\\daten\\datei.txt"` sind gleich doppelt problematisch: Sie funktionieren nur unter Windows, und ohne Verbatim-`@` wird aus `\n` ein Zeilenumbruch und aus `\t` ein Tabulator. Im Zweifel `Path.Combine`.
{: .notice--warning}

## Existiert die Datei? Metadaten mit `FileInfo`

Bevor man eine Datei liest, sollte man wissen, ob es sie gibt. `File.Exists` beantwortet die Frage statisch; wer mehr über eine Datei wissen möchte, erzeugt ein `FileInfo`-Objekt und fragt dessen Properties ab:

```csharp
if (File.Exists(datei))
{
    FileInfo info = new FileInfo(datei);
    Console.WriteLine($"{info.Name}: {info.Length} Bytes");
    Console.WriteLine($"Endung: {info.Extension}");           // .json
    Console.WriteLine($"Liegt in: {info.DirectoryName}");
    Console.WriteLine($"Zuletzt geändert: {info.LastWriteTime}");
}
```

`FileInfo` ist die objektorientierte Sicht auf eine Datei: Man erzeugt ein Objekt für *eine* Datei und arbeitet damit weiter. `File` ist die statische Werkzeugkiste für schnelle Einzelaufrufe. Beide können im Kern dasselbe – `info.Exists` und `File.Exists(pfad)` liefern dieselbe Antwort.

## Verzeichnisse anlegen und durchsuchen

Schreibt man in ein Verzeichnis, das es nicht gibt, schlägt der Zugriff fehl. Deshalb legt man Verzeichnisse vorher an – `Directory.CreateDirectory` tut nichts, wenn der Ordner bereits existiert, und erzeugt bei Bedarf auch alle Zwischenverzeichnisse:

```csharp
if (!Directory.Exists(ordner))
{
    Directory.CreateDirectory(ordner);
}

foreach (string pfadZurDatei in Directory.GetFiles(ordner, "*.json"))
{
    Console.WriteLine(Path.GetFileName(pfadZurDatei));
}
```

`GetFiles` liefert alle passenden Pfade als `string[]`; das Muster `"*.json"` filtert nach Dateiendung. Bei sehr großen Verzeichnissen ist `Directory.EnumerateFiles` die bessere Wahl: Es gibt ein `IEnumerable<string>` zurück, das die Einträge erst liefert, wenn man darüber iteriert – genau das Prinzip der [verzögerten Ausführung](/modules/linq_deferred_execution/linq_deferred_execution.md), das wir bei LINQ gesehen haben.

## Wenn etwas schiefgeht: Ausnahmen beim Dateizugriff

Dateizugriffe sind der klassische Fall für Laufzeitfehler, die nicht am eigenen Code liegen: Die Datei wurde gelöscht, der USB-Stick abgezogen, der Ordner ist schreibgeschützt. Solche Situationen meldet .NET als Ausnahmen, die wir mit [`try`/`catch` aus Programmierung 1](https://www.erodner.de/prog-lecture/modules/try_catch/try_catch/) abfangen:

```csharp
try
{
    string[] inhalt = File.ReadAllLines(datei);
    Console.WriteLine($"{inhalt.Length} Zeilen gelesen.");
}
catch (FileNotFoundException)
{
    Console.WriteLine("Die Datei gibt es nicht.");
}
catch (DirectoryNotFoundException)
{
    Console.WriteLine("Das Verzeichnis gibt es nicht.");
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

Übung: Schreibe ein Programm, das beim Start alle Zeilen einer Datei `tagebuch.txt` im Dokumente-Ordner ausgibt (falls vorhanden), dann eine neue Zeile von der Konsole einliest und sie mit Datum versehen an die Datei anhängt. Lege das Verzeichnis bei Bedarf an und fange die möglichen Ausnahmen ab.
{: .notice--info}

## Weitere Quellen

- [File-Klasse – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.io.file)
- [FileInfo-Klasse – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.io.fileinfo)
- [Path-Klasse – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.io.path)
- [Behandeln von E/A-Fehlern – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/io/handling-io-errors)
