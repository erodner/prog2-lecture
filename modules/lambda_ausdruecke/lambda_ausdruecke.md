---
title: "Lambda-Ausdrücke"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Mit [Delegaten](/modules/delegaten/delegaten.md) und [`Func`/`Action`](/modules/func_action/func_action.md) können wir Methoden wie Werte weiterreichen. Aber jedes Mal, wenn wir eine winzige Rechenvorschrift brauchen – „quadriere die Zahl“, „ist die Zahl gerade?“ –, müssen wir dafür eine benannte Methode an anderer Stelle schreiben. Das zerreißt den Code: Die Logik steht weit weg von der Stelle, an der sie gebraucht wird, und für einen Einzeiler wie `x * x` ist ein Methodenkopf mit Namen, Rückgabetyp und Parameterliste unverhältnismäßig viel Zeremonie. **Lambda-Ausdrücke** lösen das: Sie sind namenlose Methoden, die man direkt dort hinschreibt, wo ein Delegat erwartet wird.

## Von der Methode zum Lambda

Nehmen wir eine `Func<int, int>` und zwei passende Methoden, wie wir sie bisher geschrieben haben:

```csharp
static int Quadrat(int x) => x * x;
static int Inkrement(int x) => x + 1;

Func<int, int> f = Quadrat;
Console.WriteLine(f(3));   // 9
f = Inkrement;
Console.WriteLine(f(3));   // 4
```

Dieselben beiden Zuweisungen als Lambda-Ausdruck – die benannten Methoden entfallen komplett:

```csharp
Func<int, int> f = x => x * x;
Console.WriteLine(f(3));   // 9
f = x => x + 1;
Console.WriteLine(f(3));   // 4
```

Der Pfeil `=>` (gesprochen „geht nach“ oder *goes to*) trennt die Parameter vom Rumpf. Links stehen die Parameter, rechts der Ausdruck, dessen Wert zurückgegeben wird. `x => x * x` bedeutet also: „nimm ein `x` und liefere `x * x`“. Der Compiler erzeugt daraus im Hintergrund eine gewöhnliche Methode und weist sie dem Delegaten zu – ein Lambda ist keine neue Art von Objekt, sondern nur eine kürzere Schreibweise.

## Die Syntax im Detail

Ein Lambda hat die Form `Parameter => Ausdruck` oder `Parameter => { Anweisungen }`. Die Anzahl der Parameter ist beliebig; bei genau einem Parameter dürfen die Klammern wegfallen:

```csharp
Func<string> gruss = () => "Hallo!";                  // 0 Parameter
Func<int, int> verdopple = x => 2 * x;                 // 1 Parameter
Func<int, int, bool> groesser = (x, y) => x > y;       // 2 Parameter
Action<string, int> melde = (text, n) => Console.WriteLine($"{text}: {n}");

Console.WriteLine(gruss());          // Hallo!
Console.WriteLine(groesser(5, 3));   // True
melde("Anzahl", 42);                 // Anzahl: 42
```

Auffällig ist, dass nirgends ein Typ für `x`, `y` oder `text` steht. Der Compiler **inferiert** die Parametertypen aus dem Delegattyp, dem das Lambda zugewiesen wird: Bei `Func<int, int, bool>` müssen `x` und `y` vom Typ `int` sein, und der Ausdruck `x > y` muss `bool` ergeben – passt. Man kann die Typen auch hinschreiben (`(int x, int y) => x > y`), nötig ist das nur, wenn der Compiler den Zieltyp nicht kennt, etwa bei `var f = (int x) => x * x;`.

## Statement-Lambdas

Reicht ein einzelner Ausdruck nicht aus, bekommt der Rumpf geschweifte Klammern und darf beliebig viele Anweisungen enthalten. Ein Rückgabewert muss dann – wie in einer normalen Methode – mit `return` geliefert werden:

```csharp
Func<int, int, int> sichereDivision = (a, b) =>
{
    if (b == 0)
    {
        Console.WriteLine("Division durch null vermieden.");
        return 0;
    }
    return a / b;
};

Console.WriteLine(sichereDivision(10, 2));   // 5
Console.WriteLine(sichereDivision(10, 0));   // Division durch null vermieden.
                                             // 0
```

Wächst ein Statement-Lambda über fünf, sechs Zeilen hinaus, ist das meist ein Zeichen, dass eine benannte Methode besser wäre – dazu unten mehr.

## Lambdas als Argument

Die eigentliche Stärke zeigt sich, wenn eine Methode einen Delegaten als Parameter erwartet. Statt vorher eine Methode zu deklarieren, schreibt man das Verhalten direkt in den Aufruf:

```csharp
static List<int> Filtern(int[] zahlen, Func<int, bool> bedingung)
{
    List<int> ergebnis = [];
    foreach (int zahl in zahlen)
        if (bedingung(zahl))
            ergebnis.Add(zahl);
    return ergebnis;
}

int[] zahlen = [4, 7, 10, 13, 16, 21];
List<int> gerade = Filtern(zahlen, x => x % 2 == 0);
List<int> gross = Filtern(zahlen, x => x > 12);

Console.WriteLine(string.Join(", ", gerade));   // 4, 10, 16
Console.WriteLine(string.Join(", ", gross));    // 13, 16, 21
```

`Filtern` ist ein einziges Mal geschrieben und funktioniert mit jeder denkbaren Bedingung. Wer die LINQ-Query-Syntax aus dem Modul [LINQ-Abfragen](/modules/linq_query_syntax/linq_query_syntax.md) kennt, ahnt schon: `where x % 2 == 0` ist am Ende nichts anderes als so ein Lambda, das an eine Methode namens `Where` übergeben wird. Wie das genau aussieht, zeigt das Modul [LINQ-Methodensyntax](/modules/linq_methodensyntax/linq_methodensyntax.md).

## Closures: Zugriff auf äußere Variablen

Ein Lambda kann Variablen aus seiner Umgebung benutzen – lokale Variablen der umgebenden Methode, Parameter, Felder. Man sagt, das Lambda **schließt** diese Variablen **ein** (*closure*):

```csharp
int schwelle = 12;
List<int> ueberSchwelle = Filtern(zahlen, x => x > schwelle);
Console.WriteLine(string.Join(", ", ueberSchwelle));   // 13, 16, 21

schwelle = 20;
ueberSchwelle = Filtern(zahlen, x => x > schwelle);
Console.WriteLine(string.Join(", ", ueberSchwelle));   // 21
```

Das ist bequem, weil das Lambda so parametrisierbar wird, ohne dass `Filtern` einen zusätzlichen Parameter braucht. Aber Vorsicht: Das Lambda kopiert den Wert von `schwelle` **nicht** – es greift auf die Variable selbst zu, und zwar zu dem Zeitpunkt, an dem es *ausgeführt* wird, nicht wenn es *erzeugt* wird. Das führt zu einer klassischen Falle mit Schleifenvariablen:

```csharp
List<Action> aktionen = [];
for (int i = 0; i < 3; i++)
    aktionen.Add(() => Console.WriteLine(i));

foreach (Action aktion in aktionen)
    aktion();
// 3
// 3
// 3
```

Alle drei Lambdas teilen sich *dieselbe* Variable `i`. Wenn sie ausgeführt werden, ist die Schleife längst fertig und `i` steht auf 3. Die Lösung ist eine Kopie innerhalb des Schleifenrumpfs (`int kopie = i;` und dann `kopie` im Lambda verwenden) – jeder Durchlauf bekommt dann seine eigene Variable. Bei `foreach` ist das seit C# 5 nicht nötig, weil die Laufvariable dort für jeden Durchlauf neu angelegt wird.
{: .notice--warning}

## Lambda oder benannte Methode?

Beides ergibt denselben Delegaten, die Wahl ist eine Frage der Lesbarkeit. Ein Lambda ist die richtige Wahl, wenn das Verhalten kurz ist, nur an dieser einen Stelle gebraucht wird und ohne Namen verständlich bleibt – `x => x % 2 == 0` sagt alles. Eine benannte Methode lohnt sich, wenn die Logik mehrere Zeilen umfasst, an mehreren Stellen wiederverwendet wird, getestet werden soll oder wenn der Name eine Fachbedeutung transportiert: `IstZahlungsfaehig` liest sich besser als ein dreizeiliges Lambda mit Kontostand und Kreditlimit. Und wer eine Methode später wieder mit `-=` von einem Delegaten abmelden will, braucht eine Referenz darauf – ein anonym hingeschriebenes Lambda lässt sich nicht wiederfinden.

Übung: Schreibe mit `Filtern` und passenden Lambdas drei Abfragen über ein `int[]`: alle Vielfachen von 3, alle zweistelligen Zahlen, alle Zahlen zwischen zwei Grenzen `von` und `bis`, die als lokale Variablen vorliegen. Sage anschließend voraus, was die Schleifenfalle ausgibt, wenn du `for` durch `foreach (int i in new[] { 0, 1, 2 })` ersetzt – und prüfe es.
{: .notice--info}

## Weitere Quellen

- [Lambdaausdrücke – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/operators/lambda-expressions)
- [Anonyme Funktionen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/operators/delegate-operator)
- [Erfassung äußerer Variablen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/operators/lambda-expressions#capture-of-outer-variables-and-variable-scope-in-lambda-expressions)
