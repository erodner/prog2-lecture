---
title: "Generische Methoden"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Manche Algorithmen sind vollkommen gleichgültig gegenüber dem Datentyp, mit dem sie arbeiten. Zwei Variablen vertauschen, das größere von zwei Elementen finden, ein Array umdrehen – der Ablauf ist immer derselbe, ob es sich um `int`, `string` oder `Figur` handelt. Trotzdem zwingt uns die statische Typisierung von C# scheinbar dazu, für jeden Typ eine eigene Methode zu schreiben. Generische Methoden lösen diesen Widerspruch: Man schreibt den Algorithmus **einmal** mit einem Platzhalter für den Typ, und der Compiler setzt bei jedem Aufruf den passenden konkreten Typ ein. Das ist wie ein Formular mit einem Leerfeld – das Formular ist fertig gedruckt, aber was im Feld steht, entscheidet erst, wer es ausfüllt.

## Das Problem: Copy & Paste für jeden Typ

Nehmen wir eine Methode, die zwei Variablen vertauscht. Mit `ref`-Parametern, die wir aus [Programmierung 1](https://www.erodner.de/prog-lecture/modules/methoden/methoden/) kennen, ist das schnell geschrieben:

```csharp
static void Tausche(ref int a, ref int b)
{
    int temp = a;
    a = b;
    b = temp;
}

int x = 3, y = 7;
Tausche(ref x, ref y);
Console.WriteLine($"{x} {y}"); // 7 3
```

Das funktioniert – aber nur für `int`. Sobald wir zwei Strings oder zwei Figuren vertauschen wollen, brauchen wir eine weitere Methode mit exakt demselben Rumpf, in dem nur das Wort `int` ausgetauscht ist:

```csharp
static void Tausche(ref string a, ref string b)
{
    string temp = a;
    a = b;
    b = temp;
}

static void Tausche(ref Figur a, ref Figur b)
{
    Figur temp = a;
    a = b;
    b = temp;
}
```

Drei Methoden, ein Algorithmus. Jede Verbesserung müsste dreimal eingebaut werden, jeder Fehler steckt dreimal im Code. Das ist genau die Art von Duplikation, die wir im Modul [Vererbung](/modules/vererbung_grundlagen/vererbung_grundlagen.md) mit Basisklassen bekämpft haben – nur hilft Vererbung hier nicht weiter, denn `int` und `string` haben außer `object` keine gemeinsame Basisklasse.

## Der Umweg über `object` – und warum er nicht reicht

Die naheliegende Idee: Da im Modul [`object` als Basisklasse](/modules/object_basisklasse/object_basisklasse.md) jeder Typ von `object` erbt, schreiben wir die Methode einfach für `object`. Für `Tausche` mit `ref` geht das nicht direkt (eine `int`-Variable ist keine `object`-Variable), aber bei einem einfachen Rückgabewert lässt sich das Problem gut zeigen:

```csharp
static object ErstesElement(object[] elemente)
{
    return elemente[0];
}

object[] zahlen = { 1, 2, 3 };
int erste = (int)ErstesElement(zahlen);       // Cast nötig
string text = (string)ErstesElement(zahlen);  // kompiliert – und stürzt ab!
// InvalidCastException: Unable to cast object of type 'System.Int32' to type 'System.String'.
```

Die zweite Zeile ist das eigentliche Problem: Der Compiler kann nicht wissen, was tatsächlich in dem `object[]` steckt, und lässt den Cast durchgehen. Der Fehler taucht erst zur Laufzeit auf – im schlimmsten Fall beim Kunden statt beim Entwickler. Dazu kommt, dass Werttypen wie `int` beim Umwandeln in `object` in ein Heap-Objekt verpackt werden müssen (**Boxing**) und beim Cast zurück wieder ausgepackt werden (**Unboxing**). Das kostet Zeit und Speicher, wie wir im Modul [Garbage Collection](/modules/garbage_collection/garbage_collection.md) gesehen haben.

`object` als „universeller Typ“ tauscht Typsicherheit zur Kompilierzeit gegen Casts und mögliche Abstürze zur Laufzeit. Im Zweifel: lieber generisch als `object`.
{: .notice--warning}

## Die Lösung: ein Typparameter

Eine generische Methode bekommt hinter dem Namen einen **Typparameter** in spitzen Klammern. Innerhalb der Methode verwendet man diesen Platzhalter wie einen ganz normalen Typ – für Parameter, lokale Variablen und den Rückgabetyp:

```csharp
static void Tausche<T>(ref T a, ref T b)
{
    T temp = a;
    a = b;
    b = temp;
}
```

`T` ist kein echter Typ, sondern ein Platzhalter, der erst beim Aufruf mit einem konkreten Typ belegt wird. Der Name `T` (für *Type*) hat sich eingebürgert wie das `i` in Zählschleifen – man könnte ihn auch `TElement` nennen, aber `T` ist die Konvention für den Fall, dass es nur einen Typparameter gibt. Beim Aufruf gibt man den konkreten Typ an:

```csharp
int x = 3, y = 7;
Tausche<int>(ref x, ref y);

string s1 = "Papier", s2 = "Glas";
Tausche<string>(ref s1, ref s2);

Figur f1 = new Kreis("K1", 0, 0, 2);
Figur f2 = new Rechteck("R1", 1, 1, 3, 4);
Tausche<Figur>(ref f1, ref f2);
Console.WriteLine(f1.Name); // R1
```

Der Compiler erzeugt für jeden verwendeten Typ eine passende Variante der Methode und prüft dabei alles wie gewohnt: `Tausche<int>(ref x, ref s1)` wäre ein Compilerfehler, weil `s1` kein `int` ist. Es gibt keine Casts, kein Boxing und keinen Absturz zur Laufzeit.

## Typinferenz: der Compiler errät `T`

Die spitzen Klammern beim Aufruf sind meist überflüssig. Der Compiler kann aus den Argumenten ableiten, welcher Typ für `T` gemeint ist – das nennt man **Typinferenz**:

```csharp
Tausche(ref x, ref y);     // T = int, vom Compiler erkannt
Tausche(ref s1, ref s2);   // T = string
Tausche(ref f1, ref f2);   // T = Figur
```

Die Methode sieht damit beim Aufruf aus wie eine normale überladene Methode – nur dass wir sie ein einziges Mal geschrieben haben. Typinferenz funktioniert immer dann, wenn `T` in mindestens einem Parameter vorkommt. Steht `T` nur im Rückgabetyp, muss man ihn explizit angeben, weil der Compiler sonst keinen Anhaltspunkt hat.

Übung: Schreibe eine generische Methode `Umdrehen<T>(T[] feld)`, die die Reihenfolge der Elemente im Array umkehrt. Verwende dafür intern `Tausche<T>`. Teste sie mit einem `int[]` und einem `string[]`.
{: .notice--info}

## Mehrere Typparameter

Eine generische Methode kann beliebig viele Typparameter haben. Sobald es mehr als einen gibt, sollten die Namen sprechend sein und mit `T` beginnen – so wie bei `Dictionary<TKey, TValue>` in .NET. Ein Beispiel ist eine Methode, die aus einem Array von Schlüsseln und einem Array von Werten ein Dictionary baut:

```csharp
static Dictionary<TKey, TValue> Zuordnen<TKey, TValue>(TKey[] schluessel, TValue[] werte)
    where TKey : notnull
{
    var ergebnis = new Dictionary<TKey, TValue>();
    for (int i = 0; i < schluessel.Length; i++)
    {
        ergebnis[schluessel[i]] = werte[i];
    }
    return ergebnis;
}

string[] namen = { "Glas", "Papier", "Bio" };
int[] leerungen = { 2, 4, 1 };
Dictionary<string, int> plan = Zuordnen(namen, leerungen);
Console.WriteLine(plan["Papier"]); // 4
```

Die Zeile `where TKey : notnull` ist ein erster Vorgeschmack auf **Constraints**: `Dictionary` verlangt, dass Schlüssel nicht `null` sein dürfen, und diese Anforderung müssen wir an unseren Typparameter weiterreichen. Die Typinferenz funktioniert auch hier – aus `string[]` und `int[]` erkennt der Compiler `TKey = string` und `TValue = int`.

## Die Grenze: `T` kann erst einmal nichts

Was passiert, wenn der Algorithmus mehr braucht, als Werte nur hin- und herzuschieben? Versuchen wir, das Maximum zweier Werte generisch zu bestimmen:

```csharp
static T Maximum<T>(T a, T b)
{
    if (a > b)   // Compilerfehler CS0019:
    {            // Der Operator ">" kann nicht auf Operanden vom Typ "T" und "T" angewendet werden.
        return a;
    }
    return b;
}
```

Der Compiler lehnt das ab – und zwar zu Recht. `T` könnte beim Aufruf jeder beliebige Typ sein, auch einer, für den `>` gar nicht definiert ist. Da die Methode für **alle** Typen funktionieren muss, darf sie innerhalb des Rumpfs nur das verwenden, was wirklich jeder Typ hat: die Methoden von `object` wie `ToString()` und `Equals()`. Um dem Compiler zu versprechen, dass `T` vergleichbar ist, brauchen wir eine Einschränkung des Typparameters – dazu mehr im Modul [Generische Constraints](/modules/generische_constraints/generische_constraints.md).

Ohne Constraint ist `T` ein völlig unbekannter Typ. Der Compiler erlaubt nur, was für jeden denkbaren Typ funktioniert: Zuweisungen, Vergleiche mit `Equals`, `ToString()` und das Ablegen in Variablen, Arrays oder Listen.
{: .notice--primary}

## Weitere Quellen

- [Generische Methoden – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/generics/generic-methods)
- [Generics (C#-Programmierhandbuch) – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/types/generics)
- [Boxing und Unboxing – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/types/boxing-and-unboxing)
