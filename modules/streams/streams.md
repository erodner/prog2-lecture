---
title: "Streams"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

`File.ReadAllText` ist bequem, hat aber einen Haken: Es holt die gesamte Datei auf einmal in den Speicher. Bei einer Logdatei mit mehreren Gigabyte, einem Video oder Daten, die gerade erst über das Netzwerk hereintröpfeln, funktioniert das nicht. Die Lösung heißt **Stream** – ein Fließband für Bytes. Beim Schreiben legt man kleine Pakete auf das Band, beim Lesen nimmt man sie herunter, und zu keinem Zeitpunkt muss alles gleichzeitig im Speicher liegen. Das Schöne daran: Ob am anderen Ende des Bandes eine Datei, eine Netzwerkverbindung oder ein Stück Arbeitsspeicher steht, ist für den Code, der liest und schreibt, völlig egal. Genau davon lebt die `TextdateiLevelQuelle` unseres Adventures, die wir am Ende dieses Moduls fertig lesen.

## Ein Stream ist eine Abstraktion

`Stream` ist eine abstrakte Klasse im Namensraum `System.IO` – ganz im Sinne dessen, was wir im Modul [abstrakte Klassen](/modules/abstrakte_klassen/abstrakte_klassen.md) gesehen haben. Sie legt fest, was jeder Stream kann: Bytes lesen (`Read`), Bytes schreiben (`Write`) und gegebenenfalls die Position verändern (`Seek`). Konkrete Unterklassen sind unter anderem `FileStream` für Dateien, `NetworkStream` für Verbindungen und `MemoryStream` für ein Byte-Array im Speicher.

Nicht jeder Stream kann alles: Ein Netzwerkstream lässt sich nicht zurückspulen, ein nur zum Lesen geöffneter Dateistream nicht beschreiben. Dafür gibt es drei Abfrage-Properties:

```csharp
using FileStream fs = File.OpenRead(pfad);
Console.WriteLine(fs.CanRead);  // True
Console.WriteLine(fs.CanWrite); // False
Console.WriteLine(fs.CanSeek);  // True
```

Wer trotzdem versucht, in einen nur lesbaren Stream zu schreiben, erhält eine `NotSupportedException`. Die `using`-Deklaration in der ersten Zeile sorgt dafür, dass die Datei am Ende des Blocks wieder geschlossen wird – warum das nötig ist, klärt das nächste Modul zu [`IDisposable`](/modules/idisposable_using/idisposable_using.md).

## Bytes schreiben mit `FileStream`

Ein Stream kennt nur Bytes. Wer Text schreiben will, muss ihn zuerst in Bytes umwandeln – dafür ist eine **Kodierung** wie UTF-8 zuständig:

```csharp
using FileStream fs = new FileStream(pfad, FileMode.Create, FileAccess.Write);
byte[] daten = Encoding.UTF8.GetBytes("#####");
fs.Write(daten, 0, daten.Length);
```

Der Konstruktor bekommt neben dem Pfad einen `FileMode` und einen `FileAccess`. Der Modus beschreibt, was mit einer vorhandenen oder fehlenden Datei passieren soll:

| `FileMode` | Verhalten |
| :--- | :--- |
| `Create` | Neu anlegen; vorhandene Datei wird überschrieben |
| `CreateNew` | Neu anlegen; Fehler, falls die Datei schon existiert |
| `Open` | Öffnen; Fehler, falls die Datei fehlt |
| `OpenOrCreate` | Öffnen oder bei Bedarf anlegen |
| `Append` | Öffnen und ans Ende springen (nur mit `FileAccess.Write`) |
| `Truncate` | Öffnen und Inhalt auf Länge 0 kürzen |

`FileAccess.Read`, `Write` oder `ReadWrite` legt zusätzlich fest, in welche Richtung das Fließband laufen darf. Die Abkürzungen `File.OpenRead`, `File.OpenWrite` und `File.Create` setzen sinnvolle Kombinationen für dich.

## Bytes lesen: der Puffer

Beim Lesen wird der Vorteil des Fließbands sichtbar. Man legt einen **Puffer** fester Größe an und füllt ihn in einer Schleife immer wieder neu, bis nichts mehr kommt:

```csharp
using FileStream fs = File.OpenRead(pfad);
byte[] puffer = new byte[1024];
int gelesen;
while ((gelesen = fs.Read(puffer, 0, puffer.Length)) > 0)
{
    Console.Write(Encoding.UTF8.GetString(puffer, 0, gelesen));
}
```

`Read` füllt den Puffer ab Index 0 mit bis zu `puffer.Length` Bytes und gibt zurück, wie viele es tatsächlich waren. Am Dateiende liefert es `0`, und die Schleife endet. Wichtig ist, beim Umwandeln nur die ersten `gelesen` Bytes zu verwenden – der letzte Block ist meist nicht voll, und der Rest des Puffers enthält noch Daten vom vorherigen Durchlauf. Egal, ob die Datei 3 Kilobyte oder 3 Gigabyte groß ist: Der Speicherbedarf bleibt bei 1024 Bytes.

`Read` darf auch *weniger* Bytes liefern, als angefordert wurden – selbst mitten in der Datei, und bei Netzwerkstreams ist das die Regel. Deshalb nie annehmen, dass der Puffer voll ist, sondern immer den Rückgabewert auswerten.
{: .notice--warning}

## Springen mit `Seek` und `Position`

Anders als ein echtes Fließband kann man bei Dateien vor- und zurückspulen. `Position` verrät, wo man gerade steht, `Seek` verschiebt den Lesekopf relativ zu Anfang, aktueller Position oder Ende:

```csharp
using FileStream fs = new FileStream(pfad, FileMode.Open, FileAccess.ReadWrite);
Console.WriteLine(fs.Length);     // Größe in Bytes
fs.Seek(0, SeekOrigin.End);       // ans Ende springen
fs.Write(Encoding.UTF8.GetBytes("#####"));
fs.Position = 0;                  // zurück zum Anfang
Console.WriteLine((char)fs.ReadByte()); // #
```

So lässt sich Text anhängen, ohne die Datei vorher komplett zu lesen – das ist genau das, was `File.AppendAllText` intern tut. Bei Netzwerkstreams ist `CanSeek` dagegen `false`, weil bereits empfangene Bytes nicht zurückgeholt werden können.

## Text bequem: `StreamReader` und `StreamWriter`

Für Textdateien ist die Hantierung mit Byte-Puffern und Kodierungen umständlich. Deshalb gibt es zwei Klassen, die sich *um* einen Stream legen und die Übersetzung zwischen Zeichen und Bytes übernehmen. `StreamWriter` bietet `Write` und `WriteLine` wie die Konsole:

```csharp
using StreamWriter schreiber = new StreamWriter(pfad, append: false, Encoding.UTF8);
foreach (string zeile in level.Zeilen)
{
    schreiber.WriteLine(zeile);
}
```

`StreamReader` ist das Gegenstück: `ReadLine` liefert die nächste Zeile ohne Zeilenumbruch und `null`, wenn das Ende erreicht ist – das nutzt die Schleife als Abbruchbedingung. Genau so liest unser Spiel ein Level aus einer Textdatei; das ist der Rest der Methode `Laden`, deren Anfang wir im Modul [Dateien und Verzeichnisse](/modules/dateien_verzeichnisse/dateien_verzeichnisse.md) gesehen haben:

```csharp
public Level Laden(string name)
{
    string pfad = Path.Combine(ordner, name + ".txt");
    if (!File.Exists(pfad))
    {
        throw new FileNotFoundException($"Es gibt kein Level namens '{name}'.", pfad);
    }

    List<string> zeilen = new();
    using StreamReader leser = new(pfad);
    string? zeile;
    while ((zeile = leser.ReadLine()) != null)
    {
        if (zeile.Trim().Length > 0) zeilen.Add(zeile.TrimEnd());
    }
    return new Level(name, zeilen);
}
```

Warum zeilenweise, wo `File.ReadAllLines` für eine Karte aus neun Zeilen genauso gereicht hätte? Weil beim Lesen bereits **entschieden** wird: Leerzeilen am Dateiende – etwa die, die viele Editoren automatisch anhängen – fallen weg, und `TrimEnd()` entfernt unsichtbare Leerzeichen am Zeilenende, die den `LevelParser` sonst als zusätzliche Spalte zählen würde. Das passiert in einem einzigen Durchlauf, ohne ein Zwischenarray mit allen Rohzeilen. Und der entscheidende Punkt: Dieselbe Schleife funktioniert unverändert, wenn die Zeilen statt aus einer Datei aus einer Netzwerkverbindung kommen – dann steckt hinter dem `StreamReader` eben ein `NetworkStream`.

`File.OpenText(pfad)` und `File.CreateText(pfad)` sind Abkürzungen, die direkt einen `StreamReader` bzw. `StreamWriter` mit UTF-8 liefern. `new StreamReader(pfad)` wie oben tut dasselbe.

## `MemoryStream`: ein Stream ohne Datei

Manchmal möchte man Code, der einen Stream erwartet, ohne Datei testen oder Daten erst im Speicher zusammenbauen. Ein `MemoryStream` verhält sich nach außen wie jeder andere Stream, speichert aber nur in einem Byte-Array:

```csharp
using MemoryStream speicher = new MemoryStream();
using (StreamWriter w = new StreamWriter(speicher, leaveOpen: true))
{
    w.WriteLine("#@.E#");
}
speicher.Position = 0;
using StreamReader r = new StreamReader(speicher);
Console.WriteLine(r.ReadLine()); // #@.E#
```

Ohne `leaveOpen: true` würde der `StreamWriter` beim Schließen auch den darunterliegenden `MemoryStream` schließen. Für Tests ist das ein praktisches Muster: Eine Methode, die einen `Stream` oder `TextReader` entgegennimmt statt eines Pfads, lässt sich mit einem `MemoryStream` oder `StringReader` füttern – ganz ohne Dateisystem. Diesen Gedanken greifen wir in [Vorlesung 12](/lectures/12/12.md) wieder auf.

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v09-daten`).

Übung: Schreibe eine Methode `Level LevelLesen(TextReader quelle, string name)`, die die Schleife aus `Laden` enthält, aber keinen Pfad mehr kennt. Rufe sie einmal mit `File.OpenText("kerker.txt")` und einmal mit `new StringReader("#####\n#@.E#\n#####")` auf. Welchen Vorteil hat diese Signatur, wenn das Level später per HTTP kommt?
{: .notice--info}

## Weitere Quellen

- [Stream-Klasse – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.io.stream)
- [FileStream-Klasse – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.io.filestream)
- [Vorgehensweise: Lesen von Text aus einer Datei – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/io/how-to-read-text-from-a-file)
- [Zeichencodierung in .NET – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/base-types/character-encoding-introduction)
- [.NET Fiddle](https://dotnetfiddle.net/) – die Puffer-Schleife und `MemoryStream` im Browser ausprobieren, ohne ein Projekt anzulegen.
