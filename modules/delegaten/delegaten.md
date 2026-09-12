---
title: "Delegaten"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Stell dir vor, du hast keine Zeit, selbst einkaufen zu gehen. Du gibst deshalb einem Boten einen Auftrag mit fester Form: „Kauf für höchstens 25 Euro ein.“ Wichtig ist dabei: Du beschreibst den *Auftrag* – was hineingeht (ein Budget) und was herauskommt (ein Einkauf). *Wer* den Auftrag am Ende ausführt, ist egal – Hauptsache, die Form passt. Genau dieses Prinzip bringt der **Delegat** (*delegate*) nach C#: ein Typ, der nicht Daten beschreibt, sondern eine Methodensignatur. Eine Variable dieses Typs zeigt auf eine Methode – und kann wie eine Methode aufgerufen werden.

Warum ist das wichtig? Bisher konnten wir Methoden nur Werte übergeben: Zahlen, Strings, Objekte. Mit Delegaten können wir einer Methode **Verhalten** übergeben – eine Rechenvorschrift, eine Bedingung, eine Reaktion. Das ist die Grundlage für alles, was in dieser Vorlesung folgt: `Func` und `Action`, Lambda-Ausdrücke, die LINQ-Methodensyntax und Ereignisse.

## Ein Typ für Methoden

Eine gewöhnliche Variable besteht aus einem Datentyp und einem passenden Wert. Bei einem Delegaten ist der Datentyp die **Signatur** einer Methode (Rückgabetyp und Parameterliste), der Wert ein **Verweis auf eine Methode**, die diese Signatur erfüllt. Deklariert wird ein Delegattyp mit dem Schlüsselwort `delegate`:

```csharp
delegate Einkauf EinkaufHandler(double maxGeld);
```

Das liest sich wie ein Methodenkopf ohne Rumpf: `EinkaufHandler` ist der Name des neuen Typs, `Einkauf` der geforderte Rückgabetyp, `double maxGeld` der geforderte Parameter. Jede Methode, die einen `double` nimmt und einen `Einkauf` zurückgibt, kann diesem Boten als Auftrag zugewiesen werden – egal wie die Methode heißt und wo sie steht:

```csharp
static Einkauf GeheEinkaufen(double maxGeld)
{
    return new Einkauf("Brot und Kaese", Math.Min(maxGeld, 12.50));
}

EinkaufHandler meinBote = GeheEinkaufen;   // Methode zuweisen – ohne Klammern!
Einkauf einkauf = meinBote(25.0);          // aufrufen wie eine Methode
```

Zwei Dinge fallen auf. Erstens steht bei der Zuweisung `GeheEinkaufen` **ohne runde Klammern**: Klammern würden die Methode *aufrufen* und ihr Ergebnis zuweisen – wir wollen aber die Methode *selbst* in die Variable legen. Zweitens sieht der Aufruf `meinBote(25.0)` genauso aus wie ein normaler Methodenaufruf. Tatsächlich wird die dahinterliegende Methode `GeheEinkaufen` ausgeführt.

Für die Namen von Delegattypen hat sich das Suffix `Handler` eingebürgert – so wie Exception-Klassen auf `Exception` enden. Das ist eine Konvention, keine Regel: Die vordefinierten Delegaten `Func`, `Action` oder `Predicate` halten sich nicht daran.
{: .notice--primary}

## Ein Delegat als Parameter

Der eigentliche Nutzen zeigt sich, wenn ein Delegat **Parameter** einer Methode wird. Für Rechenoperationen mit zwei ganzen Zahlen sieht das so aus:

```csharp
delegate int RechenHandler(int a, int b);

static int Addiere(int x, int y) => x + y;
static int Subtrahiere(int x, int y) => x - y;

static void FuehreAus(RechenHandler rechnung)
{
    int a = 5, b = 3;
    Console.WriteLine($"Ergebnis für a = {a}, b = {b}: {rechnung(a, b)}");
}

FuehreAus(Addiere);       // Ergebnis für a = 5, b = 3: 8
FuehreAus(Subtrahiere);   // Ergebnis für a = 5, b = 3: 2
```

Die Parameternamen in der Delegatdeklaration (`a`, `b`) müssen nicht mit denen der Methoden (`x`, `y`) übereinstimmen – nur Typen und Reihenfolge zählen. `FuehreAus` weiß nicht, welche Rechnung sie ausführt; sie ruft einfach den Boten auf. Delegatvariablen sind Referenzen und dürfen `null` sein – ein Aufruf würde dann eine `NullReferenceException` auslösen. Sicher ist deshalb der null-bedingte Aufruf `rechnung?.Invoke(5, 3)`: `Invoke` ist der Name der Methode, die hinter `rechnung(5, 3)` steckt, und `?.` überspringt den Aufruf, wenn die Variable `null` ist.

## Gegnerverhalten als Delegat

Im Adventure steckt derselbe Gedanke an einer viel interessanteren Stelle. Jeder Gegner muss pro Runde genau eine Entscheidung treffen – wohin ziehe ich? – und dafür schreibt `Adventure.Kern` bisher eine eigene Klasse pro Verhalten:

```csharp
public abstract class Gegner : BeweglichesObjekt
{
    /// <summary>Liefert die Richtung für diese Runde oder null, wenn der Gegner stehen bleibt.</summary>
    public abstract Richtung? NaechsterZug(Spielfeld feld);
}
```

`Wache` läuft stur geradeaus und dreht um, wenn sie anstößt; `Verfolger` nimmt die Jagd auf, sobald der Held in Sichtweite ist. Zwei Klassen für zwei Entscheidungen – und für jede weitere Verhaltensvariante käme eine dritte dazu. Genau die Signatur dieser einen Entscheidung lässt sich aber als Delegattyp aufschreiben:

```csharp
delegate Richtung? ZugVerhalten(Spielfeld feld, Gegner gegner);
```

Der Gegner selbst wird zum zweiten Parameter, denn eine freistehende Methode kennt kein `this` – sie muss erfahren, *wessen* Zug sie gerade bestimmt. Damit reicht eine einzige Gegnerklasse, die ihr Verhalten im Konstruktor entgegennimmt:

```csharp
public sealed class Zufallsgegner : Gegner
{
    private readonly ZugVerhalten verhalten;

    public Zufallsgegner(Position position, ZugVerhalten verhalten) : base("Streuner", position)
    {
        this.verhalten = verhalten;
    }

    public override char Symbol => 'Z';

    public override Richtung? NaechsterZug(Spielfeld feld) => verhalten(feld, this);
}
```

`NaechsterZug` entscheidet nichts mehr selbst, sondern reicht die Frage an den Boten weiter. Die Verhalten sind jetzt gewöhnliche statische Methoden, die niemandem gehören:

```csharp
static Richtung? ZufaelligerZug(Spielfeld feld, Gegner gegner)
{
    for (int versuch = 0; versuch < 4; versuch++)
    {
        Richtung r = (Richtung)Random.Shared.Next(4);
        if (feld.IstFrei(gegner.Position.Verschoben(r))) return r;
    }
    return null;        // eingekeilt – diese Runde aussetzen
}

static Richtung? BleibtStehen(Spielfeld feld, Gegner gegner) => null;
```

Beim Erzeugen wird das Verhalten eingesetzt, und das Spielfeld merkt keinen Unterschied: Es ruft in `GegnerZiehen` weiterhin nur `g.NaechsterZug(this)` auf.

```csharp
feld.Hinzufuegen(new Zufallsgegner(new Position(4, 2), ZufaelligerZug));
feld.Hinzufuegen(new Zufallsgegner(new Position(7, 5), BleibtStehen));
```

Zwei Gegner derselben Klasse, zwei völlig verschiedene Verhalten – ohne Vererbung. Das ist der Unterschied zwischen „Verhalten festlegen, indem man eine Klasse ableitet“ und „Verhalten übergeben, wie man einen Wert übergibt“. Welche der beiden Varianten besser ist, hängt davon ab, ob das Verhalten eigenen Zustand braucht: `Wache` merkt sich ihre `Laufrichtung` und dreht sie um – das wäre in einer statischen Methode nicht unterzubringen.

## Wenn die Signatur nicht passt

Der Compiler prüft bei der Zuweisung Rückgabetyp sowie Anzahl, Typ und Art (Wert, `ref`, `out`) aller Parameter. Eine Methode, die nur die Richtung liefert, aber das Spielfeld nicht braucht, passt deshalb nicht:

```csharp
static Richtung? ImmerNachRechts(Gegner gegner) => Richtung.Rechts;

ZugVerhalten verhalten = ImmerNachRechts;
// Fehler CS0123: Für "ImmerNachRechts" stimmt keine Überladung
// mit dem Delegaten "ZugVerhalten" überein.
```

Es gibt zwei Auswege: Entweder bekommt die Methode den ungenutzten Parameter dazu (`static Richtung? ImmerNachRechts(Spielfeld feld, Gegner gegner)`), oder wir deklarieren einen zweiten Delegattyp. Der erste Weg ist fast immer der richtige – ein einheitlicher Auftrag ist mehr wert als ein gesparter Parameter. Anders als bei der Methodenüberladung gehört beim Delegaten übrigens auch der **Rückgabetyp zur Signatur**: Eine Methode, die `Richtung` statt `Richtung?` liefert, wird ebenfalls abgelehnt.

Ein häufiger Anfängerfehler ist `ZugVerhalten v = ZufaelligerZug();` – mit Klammern wird die Methode *aufgerufen* (und scheitert schon daran, dass die Argumente fehlen). Bei der Zuweisung einer Methode gehören die Klammern weg.
{: .notice--warning}

## Multicast: mehrere Methoden in einer Variablen

Ein Delegat kann nicht nur auf *eine* Methode zeigen, sondern auf eine ganze Liste. Mit `+=` hängt man eine weitere Methode an, mit `-=` entfernt man sie wieder. Beim Aufruf werden alle Methoden **in der Reihenfolge des Anhängens** ausgeführt:

```csharp
delegate void MeldungHandler(string text);

static void InKonsole(string text) => Console.WriteLine($"Konsole: {text}");
static void InProtokoll(string text) => Console.WriteLine($"Protokoll: {text}");

MeldungHandler? melder = InKonsole;
melder += InProtokoll;
melder("Der Held findet einen Schatz");
// Konsole: Der Held findet einen Schatz
// Protokoll: Der Held findet einen Schatz

melder -= InKonsole;
melder?.Invoke("Runde beendet");
// Protokoll: Runde beendet
```

Multicast ist vor allem für Delegaten mit Rückgabetyp `void` sinnvoll. Hat der Delegat einen Rückgabewert – wie unser `ZugVerhalten` –, wird beim Aufruf nur das Ergebnis der *zuletzt* angehängten Methode zurückgegeben; die anderen gehen verloren. Steht dieselbe Methode mehrfach in der Liste, entfernt `-=` nur das letzte Vorkommen. Wird die letzte Methode entfernt, ist die Variable wieder `null` – auch deshalb ist `?.Invoke` beim Aufruf die sichere Wahl. Genau dieser Mechanismus steckt hinter dem `+=` bei Ereignissen, mit denen der Spieler im Modul [Ereignisse](/modules/ereignisse/ereignisse.md) meldet, dass er einen Schatz gefunden hat.

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v02-interfaces`).

Übung: Schreibe ein `ZugVerhalten` namens `FlieheVorDemHelden`, das den Gegner in die Richtung schickt, die den Abstand `gegner.Position.Entfernung(feld.Spieler.Position)` vergrößert – und `null` liefert, wenn keine solche Richtung frei ist. Deklariere anschließend einen Delegattyp `PruefHandler`, der ein `Spielobjekt` entgegennimmt und `bool` zurückgibt, und eine Methode `Zaehle(IEnumerable<Spielobjekt> objekte, PruefHandler pruefung)`. Welche Signatur müsste ein Delegat haben, der `int.TryParse` aufnehmen kann?
{: .notice--info}

## Weitere Quellen

- [Delegaten – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/delegates/)
- [Verwenden von Delegaten – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/delegates/using-delegates)
- [Multicastdelegaten kombinieren – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/delegates/how-to-combine-delegates-multicast-delegates)
