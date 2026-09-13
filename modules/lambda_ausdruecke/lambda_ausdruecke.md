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

Mit [Delegaten](/modules/delegaten/delegaten.md) und [`Func`/`Action`](/modules/func_action/func_action.md) können wir Methoden wie Werte weiterreichen. Aber jedes Mal, wenn wir eine winzige Bedingung brauchen – „ist das ein Verfolger?“, „steht der Gegner nah genug?“ –, müssen wir dafür eine benannte Methode an anderer Stelle schreiben. Das zerreißt den Code: Die Logik steht weit weg von der Stelle, an der sie gebraucht wird, und für einen Einzeiler ist ein Methodenkopf mit Namen, Rückgabetyp und Parameterliste unverhältnismäßig viel Zeremonie. Am Ende des letzten Moduls sind wir sogar an eine Grenze gestoßen: Eine statische Methode kann keine lokale Variable der aufrufenden Methode benutzen. **Lambda-Ausdrücke** lösen beides: Sie sind namenlose Methoden, die man direkt dort hinschreibt, wo ein Delegat erwartet wird – und die ihre Umgebung mitnehmen.

## Von der Methode zum Lambda

Nehmen wir eine `Func<int, int>` und zwei passende Methoden, wie wir sie bisher geschrieben haben:

```csharp
static int Quadrat(int x) => x * x;

Func<int, int> f = Quadrat;
Console.WriteLine(f(3));   // 9
```

Dieselbe Zuweisung als Lambda-Ausdruck – die benannte Methode entfällt komplett:

```csharp
Func<int, int> f = x => x * x;
Console.WriteLine(f(3));   // 9
```

Der Pfeil `=>` (gesprochen „geht nach“ oder *goes to*) trennt die Parameter vom Rumpf. Links stehen die Parameter, rechts der Ausdruck, dessen Wert zurückgegeben wird. `x => x * x` bedeutet also: „nimm ein `x` und liefere `x * x`“. Der Compiler erzeugt daraus im Hintergrund eine gewöhnliche Methode und weist sie dem Delegaten zu – ein Lambda ist keine neue Art von Objekt, sondern nur eine kürzere Schreibweise.

## Die Syntax im Detail

Ein Lambda hat die Form `Parameter => Ausdruck` oder `Parameter => { Anweisungen }`. Die Anzahl der Parameter ist beliebig; bei genau einem Parameter dürfen die Klammern wegfallen:

```csharp
Func<string> gruss = () => "Willkommen im Dungeon!";              // 0 Parameter
Func<Gegner, bool> jagt = g => g is Verfolger;                    // 1 Parameter
Func<Position, Position, int> abstand = (a, b) => a.Entfernung(b); // 2 Parameter
Action<string> melde = text => Console.WriteLine($"> {text}");

Console.WriteLine(jagt(new Wache(new Position(2, 2))));           // False
Console.WriteLine(abstand(new Position(0, 0), new Position(3, 4))); // 7
```

Auffällig ist, dass nirgends ein Typ für `g`, `a` oder `text` steht. Der Compiler **inferiert** die Parametertypen aus dem Delegattyp, dem das Lambda zugewiesen wird: Bei `Func<Position, Position, int>` müssen `a` und `b` vom Typ `Position` sein, und der Ausdruck `a.Entfernung(b)` muss `int` ergeben – passt. Man kann die Typen auch hinschreiben (`(Position a, Position b) => a.Entfernung(b)`), nötig ist das nur, wenn der Compiler den Zieltyp nicht kennt, etwa bei `var f = (int x) => x * x;`.

Ein Lambda, das kein `Func` und kein `Action` sein soll, sondern ein Gegnerverhalten, funktioniert genauso. So wird aus dem `Zufallsgegner` aus dem Delegaten-Modul ein Einzeiler, der seine Beute stur nach rechts jagt:

```csharp
Gegner stur = new Zufallsgegner(new Position(4, 2), (feld, gegner) =>
    feld.IstFrei(gegner.Position.Verschoben(Richtung.Rechts)) ? Richtung.Rechts : null);
```

## Statement-Lambdas

Reicht ein einzelner Ausdruck nicht aus, bekommt der Rumpf geschweifte Klammern und darf beliebig viele Anweisungen enthalten. Ein Rückgabewert muss dann – wie in einer normalen Methode – mit `return` geliefert werden:

```csharp
Func<Spielfeld, Gegner, Richtung?> vorsichtig = (feld, gegner) =>
{
    Richtung? zug = gegner.NaechsterZug(feld);
    if (zug is Richtung r && gegner.Position.Verschoben(r) == feld.Spieler.Position)
    {
        Console.WriteLine($"{gegner.Name} zögert.");
        return null;                  // greift den Helden nicht an
    }
    return zug;
};
```

Wächst ein Statement-Lambda über fünf, sechs Zeilen hinaus, ist das meist ein Zeichen, dass eine benannte Methode besser wäre – dazu unten mehr.

## Lambdas als Argument

Die eigentliche Stärke zeigt sich, wenn eine Methode einen Delegaten als Parameter erwartet. Statt vorher eine Methode zu deklarieren, schreibt man das Verhalten direkt in den Aufruf:

```csharp
static List<Gegner> Filtern(IEnumerable<Gegner> gegner, Func<Gegner, bool> bedingung)
{
    List<Gegner> ergebnis = [];
    foreach (Gegner g in gegner)
        if (bedingung(g))
            ergebnis.Add(g);
    return ergebnis;
}

// feld ist das eingebaute Level „Kerker“: eine Wache bei (13, 2), ein Verfolger bei (11, 5)
Position held = feld.Spieler.Position;          // (1, 1)

List<Gegner> jaeger = Filtern(feld.Gegner, g => g is Verfolger);
List<Gegner> nah = Filtern(feld.Gegner, g => g.Position.Entfernung(held) <= 15);

Console.WriteLine(jaeger.Count);                // 1
Console.WriteLine(nah.Count);                   // 2
```

`Filtern` ist ein einziges Mal geschrieben und funktioniert mit jeder denkbaren Bedingung. Genau so arbeitet auch die Klasse `Inventar<T>` aus [Vorlesung 05](/lectures/05/05.md) – nur dass sie die Schleife nicht selbst schreibt, sondern an fertige LINQ-Methoden übergibt:

```csharp
public bool Enthaelt<TArt>() where TArt : T
{
    return inhalt.Any(d => d is TArt);
}

public bool Entfernen<TArt>() where TArt : T
{
    T? treffer = inhalt.FirstOrDefault(d => d is TArt);
    return treffer is not null && inhalt.Remove(treffer);
}
```

`Any` und `FirstOrDefault` sind fremder Code, der nicht weiß, wonach gesucht wird; `d => d is TArt` ist unser Beitrag. Wer die LINQ-Query-Syntax aus dem Modul [LINQ-Abfragen](/modules/linq_query_syntax/linq_query_syntax.md) kennt, ahnt schon: `where d is TArt` ist am Ende nichts anderes als so ein Lambda, das an eine Methode namens `Where` übergeben wird. Wie das genau aussieht, zeigt das Modul [LINQ-Methodensyntax](/modules/linq_methodensyntax/linq_methodensyntax.md).

## Closures: Zugriff auf äußere Variablen

Ein Lambda kann noch mehr als eine benannte Methode: Es darf auf alles aus seiner Umgebung zugreifen – lokale Variablen, Parameter, Felder. Im Beispiel oben ist das die Variable `held` der umgebenden Methode; eine statische Methode wie `NachEntfernungZurEcke` aus dem [letzten Modul](/modules/func_action/func_action.md) kam gerade deshalb nicht an sie heran. Man sagt, das Lambda **schließt** diese Variablen **ein** (*closure*):

```csharp
int sichtweite = 15;
List<Gegner> gefahr = Filtern(feld.Gegner, g => g.Position.Entfernung(held) <= sichtweite);
Console.WriteLine(gefahr.Count);      // 2 – Wache (13) und Verfolger (14) liegen darunter

sichtweite = 5;
gefahr = Filtern(feld.Gegner, g => g.Position.Entfernung(held) <= sichtweite);
Console.WriteLine(gefahr.Count);      // 0 – dieselbe Zeile, anderes Ergebnis
```

Das ist bequem, weil das Lambda so parametrisierbar wird, ohne dass `Filtern` einen zusätzlichen Parameter braucht. Aber Vorsicht: Das Lambda kopiert den Wert von `sichtweite` **nicht** – es greift auf die Variable selbst zu, und zwar zu dem Zeitpunkt, an dem es *ausgeführt* wird, nicht wenn es *erzeugt* wird. Das führt zu einer klassischen Falle mit Schleifenvariablen:

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

Beides ergibt denselben Delegaten, die Wahl ist eine Frage der Lesbarkeit. Ein Lambda ist die richtige Wahl, wenn das Verhalten kurz ist, nur an dieser einen Stelle gebraucht wird und ohne Namen verständlich bleibt – `g => g is Verfolger` sagt alles. Eine benannte Methode lohnt sich, wenn die Logik mehrere Zeilen umfasst, an mehreren Stellen wiederverwendet wird, getestet werden soll oder wenn der Name eine Fachbedeutung transportiert: `HatSichtlinie` liest sich besser als ein zwanzigzeiliges Lambda mit dem Bresenham-Algorithmus. Und wer eine Methode später wieder mit `-=` von einem Delegaten abmelden will, braucht eine Referenz darauf – ein anonym hingeschriebenes Lambda lässt sich nicht wiederfinden. Darauf kommen wir im Modul [Ereignisse](/modules/ereignisse/ereignisse.md) zurück.

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v04-blazor`).

Übung: Schreibe mit `Filtern` und passenden Lambdas drei Abfragen über `feld.Gegner`: alle Wachen, alle Gegner mit gerader X-Koordinate, alle Gegner, die zwischen zwei lokalen Grenzen `nah` und `fern` vom Helden entfernt stehen. Sage anschließend voraus, was die Schleifenfalle ausgibt, wenn du `for` durch `foreach (int i in new[] { 0, 1, 2 })` ersetzt – und prüfe es.
{: .notice--info}

## Weitere Quellen

- [Lambdaausdrücke – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/operators/lambda-expressions)
- [Anonyme Funktionen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/operators/delegate-operator)
- [Erfassung äußerer Variablen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/operators/lambda-expressions#capture-of-outer-variables-and-variable-scope-in-lambda-expressions)
- [SharpLab](https://sharplab.io/) – macht die Klasse sichtbar, die der Compiler für ein Closure anlegt; damit wird die Schleifenfalle oben schlagartig verständlich.
- [.NET Fiddle](https://dotnetfiddle.net/) – die Schleifenfalle in zehn Sekunden selbst nachstellen, mit `for` und mit `foreach`.
