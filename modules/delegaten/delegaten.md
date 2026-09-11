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
class Einkauf
{
    public string Ware { get; }
    public double Preis { get; }

    public Einkauf(string ware, double preis)
    {
        Ware = ware;
        Preis = preis;
    }
}

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

## Ein Delegat für die Grundrechenarten

Im echten Leben erledigt ein Bote viele verschiedene Aufträge. In C# braucht man pro Auftragsart einen eigenen Delegattyp, dessen Signatur genau festgelegt ist. Für Rechenoperationen mit zwei ganzen Zahlen sieht das so aus:

```csharp
delegate int RechenHandler(int a, int b);

static int Addiere(int x, int y) => x + y;
static int Subtrahiere(int x, int y) => x - y;
```

Die Parameternamen in der Delegatdeklaration (`a`, `b`) müssen nicht mit denen der Methoden (`x`, `y`) übereinstimmen – nur Typen und Reihenfolge zählen. Der eigentliche Nutzen zeigt sich, wenn wir den Delegaten als **Parameter** einer Methode verwenden. Die Methode `FuehreAus` weiß nicht, welche Rechnung sie ausführt – sie ruft einfach den Boten auf:

```csharp
static void FuehreAus(RechenHandler rechnung)
{
    int a = 5, b = 3;
    int ergebnis = rechnung(a, b);
    Console.WriteLine($"Ergebnis für a = {a}, b = {b}: {ergebnis}");
}

FuehreAus(Addiere);       // Ergebnis für a = 5, b = 3: 8
FuehreAus(Subtrahiere);   // Ergebnis für a = 5, b = 3: 2
```

Damit lässt sich zum Beispiel ein kleiner Taschenrechner bauen, bei dem die Nutzerin eine Operation wählt. Die `switch`-Anweisung befüllt nur die Delegatvariable – die Ausführung passiert an einer einzigen Stelle:

```csharp
Console.Write("1: Addieren, 2: Subtrahieren – Auswahl: ");
int auswahl = int.Parse(Console.ReadLine() ?? "0");

RechenHandler? rechnung = null;
switch (auswahl)
{
    case 1: rechnung = Addiere; break;
    case 2: rechnung = Subtrahiere; break;
    default: Console.WriteLine("Unbekannte Auswahl."); break;
}

if (rechnung is not null)
    FuehreAus(rechnung);
```

Die Variable `rechnung` ist als `RechenHandler?` deklariert, denn Delegatvariablen sind Referenzen und können `null` sein. Eine Delegatvariable mit dem Wert `null` enthält keine Methode – ein Aufruf würde eine `NullReferenceException` auslösen. Deshalb die Prüfung vor `FuehreAus`. Kürzer geht das mit dem null-bedingten Aufruf `rechnung?.Invoke(5, 3)`: `Invoke` ist der Name der Methode, die hinter dem Aufruf `rechnung(5, 3)` steckt, und `?.` überspringt den Aufruf, wenn die Variable `null` ist.

## Wenn die Signatur nicht passt

Was passiert, wenn wir eine dritte Operation ergänzen wollen und die Division sinnvollerweise einen `double` zurückgeben lassen?

```csharp
static double Dividiere(int x, int y) => (double)x / y;

RechenHandler rechnung = Dividiere;
// Fehler CS0123: Für "Dividiere" stimmt keine Überladung
// mit dem Delegaten "RechenHandler" überein.
```

Der Compiler lehnt die Zuweisung ab: `RechenHandler` verlangt einen `int` als Rückgabetyp, `Dividiere` liefert `double`. Anders als bei der Methodenüberladung gehört beim Delegaten der **Rückgabetyp zur Signatur**. Es gibt zwei Auswege: Entweder `Dividiere` gibt ein `int` zurück (und wir verlieren den Nachkommateil), oder wir stellen den Delegaten und alle Rechenmethoden auf `double` um. Welcher Weg richtig ist, hängt davon ab, was der Taschenrechner leisten soll – der Compiler zwingt uns nur, die Entscheidung bewusst zu treffen.

Der Compiler prüft bei der Zuweisung Rückgabetyp sowie Anzahl, Typ und Art (Wert, `ref`, `out`) aller Parameter. Ein häufiger Anfängerfehler ist `RechenHandler r = Addiere();` – mit Klammern wird `Addiere` aufgerufen (und scheitert schon daran, dass Argumente fehlen). Bei der Zuweisung einer Methode gehören die Klammern weg.
{: .notice--warning}

## Multicast: mehrere Methoden in einer Variablen

Ein Delegat kann nicht nur auf *eine* Methode zeigen, sondern auf eine ganze Liste. Mit `+=` hängt man eine weitere Methode an, mit `-=` entfernt man sie wieder. Beim Aufruf werden alle Methoden **in der Reihenfolge des Anhängens** ausgeführt:

```csharp
delegate void MeldungHandler(string text);

static void InKonsole(string text) => Console.WriteLine($"Konsole: {text}");
static void InProtokoll(string text) => Console.WriteLine($"Protokoll: {text}");

MeldungHandler? melder = InKonsole;
melder += InProtokoll;
melder("Figur gespeichert");
// Konsole: Figur gespeichert
// Protokoll: Figur gespeichert

melder -= InKonsole;
melder?.Invoke("Nur noch Protokoll");
// Protokoll: Nur noch Protokoll
```

Multicast ist vor allem für Delegaten mit Rückgabetyp `void` sinnvoll. Hat der Delegat einen Rückgabewert, wird beim Aufruf nur das Ergebnis der *zuletzt* angehängten Methode zurückgegeben – die anderen gehen verloren. Steht dieselbe Methode mehrfach in der Liste, entfernt `-=` nur das letzte Vorkommen. Wird die letzte Methode entfernt, ist die Variable wieder `null` – auch deshalb ist `?.Invoke` beim Aufruf die sichere Wahl. Genau dieser Mechanismus steckt hinter dem `+=` bei Ereignissen, die wir im Modul [Ereignisse](/modules/ereignisse/ereignisse.md) kennenlernen.

Übung: Deklariere einen Delegattyp `PruefHandler`, der einen `string` entgegennimmt und `bool` zurückgibt. Schreibe zwei passende Methoden (`IstNichtLeer`, `IstKurz` für weniger als 10 Zeichen) und eine Methode `ZaehleGueltige(string[] woerter, PruefHandler pruefung)`, die zählt, wie viele Wörter die Prüfung bestehen. Welche Signatur müsste ein Delegat haben, der `int.TryParse` aufnehmen kann?
{: .notice--info}

## Weitere Quellen

- [Delegaten – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/delegates/)
- [Verwenden von Delegaten – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/delegates/using-delegates)
- [Multicastdelegaten kombinieren – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/delegates/how-to-combine-delegates-multicast-delegates)
