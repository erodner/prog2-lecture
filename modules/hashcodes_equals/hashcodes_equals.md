---
title: "Hashcodes und Equals – wie ein Dictionary seine Schlüssel findet"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Ein Bibliothekar, der ein Buch sucht, geht nicht Regal für Regal durch – er schaut auf die Signatur, weiß dadurch sofort, in welchem Regal das Buch steht, und muss nur noch in diesem einen Regal nachsehen. Genau nach diesem Prinzip arbeitet ein `Dictionary<K, V>`: Aus dem Schlüssel wird eine Zahl berechnet, die direkt sagt, in welchem „Regal“ der Wert liegt. Deshalb ist der Zugriff so schnell, egal ob zehn oder zehn Millionen Einträge gespeichert sind. Unser Spielfeld lebt von dieser Eigenschaft: Bei jedem Zeichnen fragt es für jedes einzelne Feld nach, was dort liegt. Aus [Programmierung 1](https://www.erodner.de/prog-lecture/modules/dictionary/dictionary/) kennen wir das `Dictionary` als Werkzeug – in diesem Modul schauen wir hinein, denn sobald wir eine **eigene Klasse als Schlüssel** verwenden, funktioniert das Werkzeug ohne dieses Wissen nicht mehr.

## Assoziative Arrays

Ein Array kennt nur ganzzahlige Indizes von `0` bis `Length - 1`. Ein `Dictionary` verallgemeinert diese Idee: Der „Index“ darf ein beliebiger Typ sein – ein `string`, ein `int`, ein `DateTime` oder eben eine `Position`. Weil Schlüssel und Wert fest zusammengehören (assoziiert sind), spricht man von einem **assoziativen Array**. Im `Spielfeld` sieht das so aus:

```csharp
// Statische Objekte nach Position: schneller Zugriff beim Zeichnen und bei Kollisionen.
private readonly Dictionary<Position, StatischesObjekt> statische = new();

public StatischesObjekt? StatischesObjektAn(Position p)
{
    return statische.TryGetValue(p, out StatischesObjekt? s) ? s : null;
}
```

`TryGetValue` liefert `true`, wenn der Schlüssel vorhanden ist, und legt den Wert im `out`-Parameter ab – das erspart die `KeyNotFoundException`, die der Indexzugriff `statische[p]` bei einem unbekannten Schlüssel wirft. Aufgerufen wird die Methode ständig: `AlsText()` fragt für jedes Feld der Karte nach, `IstFrei` prüft vor jedem Schritt, und `HatSichtlinie` läuft die Sichtlinie eines Verfolgers Feld für Feld ab. Wie aber kommt das Dictionary von `new Position(5, 2)` zu einer Speicherstelle, ohne alle Einträge zu vergleichen?

## Wie eine Hashtabelle funktioniert

Intern besitzt ein `Dictionary` ein Array von **Buckets** (Behältern). Beim Einfügen und beim Nachschlagen passiert dreimal dasselbe:

1. Aus dem Schlüssel wird mit einer **Hashfunktion** eine ganze Zahl berechnet, der **Hashcode**.
2. Der Hashcode wird mit `Modulo Anzahl der Buckets` auf einen gültigen Array-Index abgebildet.
3. In diesem Bucket wird der Eintrag abgelegt beziehungsweise gesucht.

Eine Hashfunktion bildet eine riesige Eingabemenge (alle möglichen Positionen) auf eine kleine Zielmenge (die `int`-Werte) ab. Zwangsläufig bekommen dabei manchmal zwei verschiedene Schlüssel denselben Hashcode – das nennt man eine **Kollision**. Das `Dictionary` löst Kollisionen durch **Verkettung**: Jeder Bucket kann mehrere Einträge aufnehmen, die wie in einer kurzen verketteten Liste hintereinanderhängen. Beim Nachschlagen wird also zuerst über den Hashcode der Bucket bestimmt und dann innerhalb des Buckets mit `Equals` der wirklich passende Schlüssel gesucht.

<svg viewBox="0 0 710 292" role="img" aria-labelledby="hashtabelle-titel" xmlns="http://www.w3.org/2000/svg" style="max-width:100%;height:auto;font-family:system-ui,sans-serif"><title id="hashtabelle-titel">Drei Positionen werden über GetHashCode auf ein Array aus acht Buckets abgebildet; zwei davon landen im selben Bucket und hängen dort als verkettete Liste.</title><defs><marker id="pfeil-hash" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="6" markerHeight="6" orient="auto-start-reverse"><path d="M 0 0 L 10 5 L 0 10 z" fill="currentColor"/></marker></defs><g fill="currentColor" font-size="13" text-anchor="middle"><text x="79" y="72">Schlüssel</text><text x="435" y="30">Bucket-Array</text><text x="601" y="30">Einträge</text></g><g stroke="currentColor" stroke-width="1.5" fill="currentColor" fill-opacity="0.06"><rect x="8" y="90" width="142" height="32" rx="4"/><rect x="8" y="146" width="142" height="32" rx="4"/><rect x="8" y="202" width="142" height="32" rx="4"/><rect x="205" y="140" width="120" height="44" rx="4"/><rect x="400" y="40" width="70" height="26"/><rect x="400" y="70" width="70" height="26"/><rect x="400" y="100" width="70" height="26"/><rect x="400" y="160" width="70" height="26"/><rect x="400" y="190" width="70" height="26"/><rect x="400" y="220" width="70" height="26"/><rect x="400" y="250" width="70" height="26"/><rect x="504" y="220" width="80" height="26" rx="4"/></g><g stroke="#d33682" stroke-width="1.5" fill="#d33682" fill-opacity="0.12"><rect x="400" y="130" width="70" height="26"/><rect x="504" y="130" width="80" height="26" rx="4"/><rect x="618" y="130" width="80" height="26" rx="4"/></g><g stroke="currentColor" stroke-width="1.5" fill="none" marker-end="url(#pfeil-hash)"><line x1="152" y1="106" x2="203" y2="150"/><line x1="152" y1="162" x2="203" y2="162"/><line x1="152" y1="218" x2="203" y2="174"/><line x1="327" y1="150" x2="396" y2="136"/><line x1="327" y1="164" x2="396" y2="150"/><line x1="327" y1="178" x2="396" y2="233"/><line x1="472" y1="233" x2="500" y2="233"/></g><g stroke="#d33682" stroke-width="1.5" fill="none" marker-end="url(#pfeil-hash)"><line x1="472" y1="143" x2="500" y2="143"/><line x1="586" y1="143" x2="614" y2="143"/></g><g fill="currentColor" font-family="ui-monospace,monospace" font-size="12" text-anchor="middle"><text x="79" y="110">Position(1, 1)</text><text x="79" y="166">Position(7, 4)</text><text x="79" y="222">Position(15, 6)</text><text x="265" y="166">GetHashCode()</text><text x="544" y="238">(7, 4)</text></g><g fill="currentColor" font-family="ui-monospace,monospace" font-size="12" text-anchor="end"><text x="392" y="58">0</text><text x="392" y="88">1</text><text x="392" y="118">2</text><text x="392" y="148">3</text><text x="392" y="178">4</text><text x="392" y="208">5</text><text x="392" y="238">6</text><text x="392" y="268">7</text></g><g fill="#d33682" font-family="ui-monospace,monospace" font-size="12" text-anchor="middle"><text x="544" y="148">(1, 1)</text><text x="658" y="148">(15, 6)</text></g><text x="601" y="120" fill="#d33682" font-size="13" text-anchor="middle">Kollision</text></svg>

Im Bild landen `Position(1, 1)` und `Position(15, 6)` im selben Bucket 3 – ihre Hashcodes sind verschieden, aber nach dem Modulo fallen sie auf denselben Index. Genau das passiert im `Spielfeld` ständig: Das `Dictionary<Position, StatischesObjekt>` hat deutlich weniger Buckets als es mögliche Positionen gibt, und trotzdem kostet `StatischesObjektAn` kaum etwas, weil pro Bucket nur eine Handvoll Einträge liegt. Der Hashcode entscheidet, in welche Zeile des rechten Arrays gesprungen wird, `Equals` entscheidet danach innerhalb dieser Zeile, welcher der beiden Einträge gemeint ist. Fällt einer der beiden Schritte aus, findet das Dictionary die Wand nicht mehr.

Zwei Regeln muss eine brauchbare Hashfunktion erfüllen: Gleiche Schlüssel liefern **immer** denselben Hashcode, und verschiedene Schlüssel liefern **möglichst oft** verschiedene Hashcodes. Die erste Regel ist Pflicht, die zweite bestimmt die Geschwindigkeit – wenn alle Positionen im selben Bucket landen, ist aus dem Dictionary eine langsame Liste geworden.
{: .notice--primary}

## Was `GetHashCode()` liefert

Die Hashfunktion ist in .NET keine Zauberei, sondern die Methode `GetHashCode()`, die jede Klasse von [`object`](/modules/object_basisklasse/object_basisklasse.md) erbt. Was sie zurückgibt, hängt davon ab, wer sie überschrieben hat:

```csharp
Console.WriteLine(42.GetHashCode());        // 42
Console.WriteLine("Kerker".GetHashCode());  // z. B. -1327847390 (inhaltsbasiert)
Console.WriteLine(new Position(5, 2).GetHashCode() == new Position(5, 2).GetHashCode()); // True

object a = new object();
object b = new object();
Console.WriteLine(a.GetHashCode() == b.GetHashCode()); // False
```

Für **Werttypen** wie `int` oder eigene `struct`s berechnet .NET den Hashcode aus den Feldwerten – zwei Strukturen mit gleichem Inhalt haben denselben Hashcode. Der eingebaute Referenztyp **`string`** überschreibt `GetHashCode` ebenfalls inhaltsbasiert. Für **eigene Klassen** dagegen gilt die Standardimplementierung von `object`: Der Hashcode hängt an der Objektidentität. Zwei Objekte, die inhaltlich gleich sind, aber an verschiedenen Stellen im Speicher liegen, bekommen verschiedene Hashcodes.

## Warum `Position` als Schlüssel funktioniert

Unsere `Position` ist ein `readonly record struct` – und damit fällt beides gleichzeitig vom Himmel:

```csharp
public readonly record struct Position(int X, int Y)
{
    public Position Verschoben(Richtung richtung) { /* ... */ }

    public int Entfernung(Position andere)
    {
        return Math.Abs(X - andere.X) + Math.Abs(Y - andere.Y);
    }
}
```

Das Schlüsselwort `record` weist den Compiler an, `Equals` und `GetHashCode` **inhaltsbasiert** zu erzeugen: Zwei Positionen sind gleich, wenn `X` und `Y` gleich sind, und dann ist auch ihr Hashcode gleich. Genau deshalb findet `statische.TryGetValue(new Position(5, 2), out _)` die Wand, die irgendwann einmal mit einer anderen `Position`-Instanz eingefügt wurde. Und weil `record struct` außerdem `==` sinnvoll definiert, funktioniert auch `if (p == Spieler.Position)` im `Spielfeld` so, wie man es erwartet.

## Das Problem: eine eigene Klasse als Schlüssel

Jetzt die Gegenprobe. Angenommen, wir hätten die Position nicht als `record struct`, sondern als gewöhnliche Klasse geschrieben – ein völlig naheliegender erster Entwurf:

```csharp
public class Koordinate
{
    public int X { get; }
    public int Y { get; }

    public Koordinate(int x, int y)
    {
        X = x;
        Y = y;
    }
}
```

Damit bauen wir ein Spielfeld auf und versuchen, ein Feld nachzuschlagen:

```csharp
Dictionary<Koordinate, StatischesObjekt> statische = new Dictionary<Koordinate, StatischesObjekt>();

Koordinate wandStelle = new Koordinate(5, 2);
statische.Add(wandStelle, new Wand(new Position(5, 2)));

Console.WriteLine(statische.ContainsKey(wandStelle));             // True
StatischesObjekt s = statische[new Koordinate(5, 2)];             // KeyNotFoundException!
```

Aus unserer Sicht ist `new Koordinate(5, 2)` dasselbe Feld wie `wandStelle`. Aus Sicht des Dictionaries sind es zwei Fremde: Die zweite Instanz liefert einen anderen Hashcode, das Dictionary schaut in den falschen Bucket, findet nichts und wirft die Exception. Selbst wenn beide zufällig im selben Bucket landen würden, scheiterte der zweite Schritt: `Equals` vergleicht standardmäßig ebenfalls nur die Identität. Für das Spiel heißt das: Der Held läuft durch alle Wände, denn `StatischesObjektAn` liefert für jedes frisch berechnete Zielfeld `null`.

## Die Lösung: `Equals` und `GetHashCode` überschreiben

Damit das Dictionary unsere Vorstellung von Gleichheit teilt, müssen wir ihm beide Schritte beibringen – die Bucket-Wahl über `GetHashCode` und den Vergleich innerhalb des Buckets über `Equals`:

```csharp
public class Koordinate
{
    public int X { get; }
    public int Y { get; }

    public Koordinate(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override bool Equals(object? obj)
    {
        return obj is Koordinate andere && X == andere.X && Y == andere.Y;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }
}
```

`obj is Koordinate andere` prüft den Typ und legt in einem Schritt die Variable `andere` an – für `null` oder fremde Typen ergibt der Ausdruck `false`. `HashCode.Combine` ist die empfohlene Art, aus mehreren Feldern einen gut verteilten Hashcode zu erzeugen; alle Felder, die in `Equals` verglichen werden, gehören hinein, und keine anderen. Mit dieser Version findet `statische[new Koordinate(5, 2)]` die Wand – und der Held stößt wieder dagegen.

Der **Vertrag** zwischen beiden Methoden ist nicht verhandelbar: Wenn `a.Equals(b)` `true` liefert, **muss** `a.GetHashCode() == b.GetHashCode()` gelten. Die Umkehrung ist nicht gefordert – gleiche Hashcodes bei ungleichen Objekten sind eine erlaubte Kollision. Wer nur `Equals` überschreibt, verletzt den Vertrag, und das Dictionary schaut weiterhin in den falschen Bucket. Der Compiler warnt in diesem Fall (CS0659).
{: .notice--warning}

## Schlüssel müssen unveränderlich sein

In `Koordinate` haben `X` und `Y` bewusst nur einen Getter – genau wie `Position` ein `readonly record struct` ist. Der Grund: Der Bucket wird beim Einfügen aus dem damaligen Hashcode bestimmt. Änderte sich `X` später, würde `GetHashCode` einen anderen Wert liefern, das Dictionary suchte im neuen Bucket – der Eintrag liegt aber noch im alten und ist damit praktisch verloren. Für das Spiel wäre das ein Albtraum: eine Wand, die im Dictionary steht, dort aber niemand mehr findet.

Deshalb bewegt sich im `Spielfeld` auch nie ein Schlüssel. Wenn ein Gegenstand aufgehoben wird, verschwindet der Eintrag komplett (`statische.Remove(gegenstand.Position)`), und bewegliche Objekte liegen gar nicht erst im Dictionary, sondern in `List<Gegner>`. Das ist kein Zufall, sondern die Konsequenz aus dieser Regel: **Nur was sich nicht bewegt, taugt als Schlüssel.**

Übung: Baue das Beispiel mit `Koordinate` nach und lege drei Wände in ein `Dictionary<Koordinate, StatischesObjekt>`. Prüfe zunächst ohne Überschreibungen, wie viele davon du wiederfindest, wenn du mit frisch erzeugten Koordinaten suchst. Ergänze dann `Equals` und `GetHashCode` und wiederhole den Test. Lass anschließend `GetHashCode` konstant `0` zurückgeben: Funktioniert das Spiel noch – und was hat sich verändert? Schreibe zum Schluss die Klasse als `public readonly record struct Koordinate(int X, int Y);` und vergleiche den Aufwand.
{: .notice--info}

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v02-interfaces`).

## Weitere Quellen

- [Dictionary<TKey,TValue> – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.collections.generic.dictionary-2)
- [Object.GetHashCode – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.object.gethashcode)
- [HashCode.Combine – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.hashcode.combine)
- [Gleichheitsvergleiche – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/statements-expressions-operators/equality-comparisons)
- [Hashtabellen interaktiv – VisuAlgo](https://visualgo.net/en/hashtable) – Schlüssel einfügen, Kollisionen entstehen sehen und die Verkettung im Bucket beobachten
- [Records – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/types/records) – warum `record` und `record struct` `Equals` und `GetHashCode` gleich mitliefern
- [Big-O Cheat Sheet](https://www.bigocheatsheet.com/) – die Zeile „Hash Table“ zeigt schwarz auf weiß, was ein schlechter Hashcode kostet: aus O(1) im Mittel wird O(n) im schlechtesten Fall
