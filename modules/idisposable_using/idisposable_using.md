---
title: "`IDisposable` und `using`"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

In den letzten beiden Modulen stand vor fast jedem `FileStream` und `StreamReader` ein `using` – mit dem Versprechen, das hier zu erklären. Der Grund ist ein Stück Verantwortung, das uns der Garbage Collector *nicht* abnimmt. Wer eine Datei öffnet, bekommt vom Betriebssystem einen **Handle**: eine Art Leihschein, der die Datei für andere sperrt und einen Platz in einer begrenzten Tabelle des Systems belegt. Wie ein Bibliotheksbuch muss man ihn zurückgeben – und zwar dann, wenn man fertig ist, nicht „irgendwann“. `IDisposable` ist der Vertrag für dieses Zurückgeben, und `using` sorgt dafür, dass man es nie vergisst.

## Warum der Garbage Collector hier nicht hilft

Im Modul [Garbage Collection](/modules/garbage_collection/garbage_collection.md) haben wir gesehen: Der GC gibt den Speicher von Objekten frei, die von keiner Wurzel mehr erreichbar sind – aber wann er das tut, entscheidet er selbst. Für ein paar Kilobyte Objektspeicher ist das egal. Ein Datei-Handle, eine Datenbankverbindung oder ein Netzwerk-Socket sind aber **nicht-verwaltete Ressourcen** (*unmanaged resources*): Sie gehören dem Betriebssystem, der GC weiß nichts über sie, und solange der Handle offen ist, bleibt die Datei gesperrt und der Schreibpuffer ungeleert. Das lässt sich mit einem Spielstand leicht vorführen:

```csharp
string pfad = Path.Combine(Path.GetTempPath(), "spielstand.json");

FileStream erster = File.OpenWrite(pfad);
erster.Write(Encoding.UTF8.GetBytes("{\"Punkte\":100}"));
Console.WriteLine(new FileInfo(pfad).Length);   // 0 – der Spielstand steckt noch im Puffer!

try
{
    FileStream zweiter = File.OpenWrite(pfad);
}
catch (IOException ex)
{
    Console.WriteLine(ex.Message);
    // The process cannot access the file '.../spielstand.json' because it is being used by another process.
}
```

Der zweite Zugriff scheitert, obwohl `erster` längst nicht mehr gebraucht wird. `erster = null` würde daran nichts ändern: Der GC läuft vielleicht erst in einer Minute, vielleicht erst beim Programmende – und bis dahin bleibt die Datei gesperrt und die Bytes liegen im Arbeitsspeicher statt auf der Platte. Genau so verliert man einen Spielstand, obwohl das Programm „gespeichert“ gemeldet hat. Erst ein Aufruf von `erster.Dispose()` gibt den Handle zurück, leert den Puffer und macht den Weg frei. Dieses **deterministische** Aufräumen – zu einem Zeitpunkt, den der Code bestimmt – ist die Aufgabe von `IDisposable`.

## Das Interface `IDisposable`

`IDisposable` ist eines der kleinsten Interfaces in .NET und hat genau eine Methode:

```csharp
public interface IDisposable
{
    void Dispose();
}
```

Der Vertrag lautet: Wer `Dispose()` aufruft, sagt „ich bin fertig mit dir“, und das Objekt gibt daraufhin alles frei, was es an Ressourcen außerhalb des verwalteten Speichers hält. Bei einem `FileStream` heißt das: Puffer auf die Platte schreiben und den Handle schließen. `FileStream`, `StreamReader`, `StreamWriter`, `MemoryStream`, Datenbankverbindungen, `HttpClient`, Bitmaps – sie alle implementieren `IDisposable`. Nach `Dispose()` ist das Objekt unbrauchbar; jeder weitere Zugriff löst eine `ObjectDisposedException` aus.

Man könnte `Dispose()` nun einfach von Hand aufrufen. Das Problem: Wenn zwischen Öffnen und `Dispose()` eine Ausnahme fliegt, wird die Zeile nie erreicht. Wer das korrekt absichern will, landet bei [`try`/`finally`](https://www.erodner.de/prog-lecture/modules/try_catch/try_catch/) – und weil dieses Muster so häufig gebraucht wird, hat C# dafür eine eigene Syntax.

## Der `using`-Block

Ein `using`-Block umschließt die Lebensdauer eines Objekts: Es wird im Kopf erzeugt und am Ende des Blocks garantiert freigegeben – auch wenn im Block eine Ausnahme auftritt:

```csharp
using (StreamReader leser = File.OpenText(pfad))
{
    while (leser.ReadLine() is string zeile)
    {
        Console.WriteLine(zeile);
    }
}   // hier wird leser.Dispose() aufgerufen – immer
```

Das ist kein Zauber, sondern reine Bequemlichkeit: Der Compiler übersetzt den Block in genau das `try`/`finally`, das man sonst selbst schreiben müsste. Der `null`-Test ist nötig, weil die Initialisierung selbst `null` liefern könnte:

```csharp
StreamReader leser = File.OpenText(pfad);
try
{
    while (leser.ReadLine() is string zeile)
    {
        Console.WriteLine(zeile);
    }
}
finally
{
    if (leser != null) leser.Dispose();
}
```

Zwischen den beiden Versionen gibt es keinen Unterschied im Verhalten – aber die erste ist kürzer, lesbarer und man kann das Aufräumen nicht vergessen. Ein `using`-Block funktioniert übrigens nur mit Typen, die `IDisposable` implementieren; für alles andere meldet der Compiler einen Fehler.

## Die `using`-Deklaration

Seit C# 8 geht es noch knapper: Eine **`using`-Deklaration** hat keinen eigenen Block, sondern gilt bis zum Ende des umgebenden Blocks – also bis zum Ende der Methode oder der Schleife, in der sie steht. Genau diese Form benutzt `TextdateiLevelQuelle.Laden` in `Adventure.Daten`:

```csharp
List<string> zeilen = new();
using StreamReader leser = new(pfad);
string? zeile;
while ((zeile = leser.ReadLine()) != null)
{
    if (zeile.Trim().Length > 0) zeilen.Add(zeile.TrimEnd());
}
return new Level(name, zeilen);
```

Die Datei wird freigegeben, sobald die Methode endet – auch auf dem `return`-Weg und auch dann, wenn `ReadLine` eine Ausnahme wirft. Das spart eine Einrückungsebene, was besonders bei mehreren Ressourcen hintereinander angenehm ist; die Freigabe erfolgt dann in umgekehrter Reihenfolge der Deklaration:

```csharp
using StreamReader quelle = File.OpenText(Path.Combine(ordner, "kerker.txt"));
using StreamWriter ziel = File.CreateText(Path.Combine(ordner, "kerker_gespiegelt.txt"));

while (quelle.ReadLine() is string zeile)
{
    ziel.WriteLine(new string(zeile.Reverse().ToArray()));
}
// am Ende des umgebenden Blocks: erst ziel.Dispose(), dann quelle.Dispose()
```

Der Block mit Klammern bleibt sinnvoll, wenn die Ressource *vor* dem Ende der Methode freigegeben werden soll – etwa wenn man ein Level erst schreibt und es danach in derselben Methode wieder einliest, um es zu prüfen.

Das Schlüsselwort `using` hat zwei völlig verschiedene Bedeutungen: Am Dateianfang (`using Adventure.Kern;`) importiert es einen Namensraum, im Methodenrumpf steuert es die Lebensdauer eines `IDisposable`-Objekts. Der Compiler erkennt am Kontext, was gemeint ist.
{: .notice--primary}

## Eigene Klassen mit `Dispose`

Sobald eine eigene Klasse ein `IDisposable`-Objekt als Feld hält, „erbt“ sie dessen Verantwortung: Wer die Klasse benutzt, kann den inneren `StreamWriter` nicht schließen, weil er ihn nicht sieht. Die Klasse muss also selbst `IDisposable` implementieren und den Aufruf weiterreichen. Ein Rundenprotokoll für das Adventure, das sich an das Ereignis `RundeBeendet` des Spielfelds hängt, ist ein gutes Beispiel:

```csharp
class Rundenprotokoll : IDisposable
{
    private readonly StreamWriter writer;
    private readonly Spielfeld feld;

    public Rundenprotokoll(Spielfeld feld, string pfad)
    {
        this.feld = feld;
        writer = new StreamWriter(pfad, append: true);
        feld.RundeBeendet += Notieren;
    }

    private void Notieren(object? sender, RundeEventArgs e)
    {
        writer.WriteLine($"Runde {e.Runde}: {e.Meldung}");
    }

    public void Dispose()
    {
        feld.RundeBeendet -= Notieren;   // Ereignis wieder abmelden
        writer.Dispose();                // leert den Puffer und schließt die Datei
    }
}
```

`Dispose()` enthält keine eigene Logik – es meldet den Handler ab und gibt die Verantwortung an das Feld weiter. Das Abmelden ist kein Schmuck: Ein angemeldeter Handler hält eine Referenz auf das Protokoll, sodass der GC es nicht einsammeln könnte, selbst wenn niemand sonst es mehr benutzt. Damit lässt sich `Rundenprotokoll` genauso benutzen wie ein `StreamWriter`:

```csharp
using (Rundenprotokoll protokoll = new Rundenprotokoll(feld, "runden.log"))
{
    feld.SpielerZieht(Richtung.Rechts);
    feld.SpielerZieht(Richtung.Unten);
}
// Datei ist geschlossen, beide Zeilen stehen sicher auf der Platte
```

Ohne das `using` – oder ohne die `Dispose`-Methode – blieben die Einträge womöglich im Puffer des `StreamWriter` stecken und die Datei wäre nach dem Programmende leer. Der Aufrufer sieht am Typ `IDisposable` sofort, dass er sich um die Freigabe kümmern muss.

`Dispose()` sollte mehrfach aufrufbar sein, ohne beim zweiten Mal zu scheitern – die eingebauten Klassen halten sich daran. In der Dokumentation findest du außerdem ein aufwendigeres „Dispose-Muster“ mit Finalizer und `Dispose(bool)`. Das brauchst du nur, wenn deine Klasse *direkt* Betriebssystem-Handles hält. Wer nur andere `IDisposable`-Objekte kapselt, kommt mit der einfachen Form oben aus.
{: .notice--warning}

## Faustregel

Im Zweifel gilt: **Alles, was `IDisposable` implementiert, gehört in ein `using`.** Ob ein Typ dazugehört, verrät die IDE (Vervollständigung zeigt `Dispose`) oder die Dokumentation. Die einzige verbreitete Ausnahme ist `HttpClient`, von dem man bewusst *eine* langlebige Instanz für das ganze Programm hält – genau so macht es `HttpLevelQuelle` mit ihrem `private static readonly HttpClient`, mehr dazu im Modul [`HttpClient` und REST](/modules/httpclient_rest/httpclient_rest.md). Für Dateien, Streams und Verbindungen gibt es dagegen keinen guten Grund, auf das `using` zu verzichten.

Übung: Erweitere `Rundenprotokoll` um einen Zähler, sodass `Dispose()` als letzte Zeile „`n Runden protokolliert`“ in die Datei schreibt. Was passiert, wenn du `Dispose()` zweimal aufrufst – und wie sicherst du das mit einem `bool`-Feld ab? Vergleiche anschließend, was in der Datei steht, wenn du das `using` weglässt und das Programm beendest.
{: .notice--info}

## Weitere Quellen

- [`IDisposable`-Schnittstelle – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.idisposable)
- [`using`-Anweisung – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/statements/using)
- [Implementieren einer Dispose-Methode – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/garbage-collection/implementing-dispose)
- [SharpLab](https://sharplab.io/) – zeigt live, in welches `try`/`finally` der Compiler ein `using` übersetzt (Ansicht „C#“).
- [Grundlagen der Garbage Collection – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/garbage-collection/fundamentals) – warum der GC nicht-verwaltete Ressourcen nicht rechtzeitig freigibt.
