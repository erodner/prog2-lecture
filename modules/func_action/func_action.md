---
title: "Func, Action und Co. – vordefinierte Delegaten"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Im Modul [Delegaten](/modules/delegaten/delegaten.md) haben wir für jede Aufgabe einen eigenen Delegattyp deklariert: `RechenHandler` für Rechnungen, `MeldungHandler` für Meldungen. Wer so weitermacht, hat bald Dutzende Delegattypen, die sich nur im Namen unterscheiden – `delegate int RechenHandler(int a, int b)` und `delegate int VergleichsHandler(int x, int y)` beschreiben exakt dieselbe Signatur. Das ist, als würde man für jeden Butler eine neue Stellenbeschreibung tippen, obwohl alle dasselbe können sollen. .NET bringt deshalb eine Handvoll **generischer Delegattypen** mit, die praktisch jede Signatur abdecken. Seit es sie gibt, braucht man eigene Delegattypen nur noch selten.

## `Func<…, TResult>` – Methoden mit Rückgabewert

`Func` steht für Methoden, die etwas zurückgeben. Der **letzte** Typparameter ist immer der Rückgabetyp, alle davor sind die Parameter. `Func<int, int, int>` beschreibt also eine Methode mit zwei `int`-Parametern und `int`-Rückgabe – genau unser `RechenHandler`, ohne dass wir ihn deklarieren müssen:

```csharp
static int Addiere(int x, int y) => x + y;
static bool IstGerade(int zahl) => zahl % 2 == 0;
static string Begruessung() => "Hallo!";

Func<int, int, int> rechnung = Addiere;
Func<int, bool> pruefung = IstGerade;
Func<string> text = Begruessung;

Console.WriteLine(rechnung(5, 3));   // 8
Console.WriteLine(pruefung(4));      // True
Console.WriteLine(text());           // Hallo!
```

Die Zuweisung `rechnung = Addiere` nennt man **Methodengruppen-Konvertierung**: Der Name einer Methode ohne Klammern steht für die Gruppe aller Überladungen dieses Namens, und der Compiler sucht die Überladung heraus, die zur Signatur des Delegaten passt. `Func` gibt es mit bis zu 16 Parametern – in der Praxis braucht man selten mehr als drei.

## `Action<…>` – Methoden ohne Rückgabewert

`void` ist kein Typ und kann deshalb nicht als Typparameter stehen. Für Methoden ohne Rückgabewert gibt es darum die zweite Familie: `Action` (keine Parameter), `Action<T>` (ein Parameter), `Action<T1, T2>` und so weiter:

```csharp
static void Trennlinie() => Console.WriteLine(new string('-', 20));
static void Ausgeben(string text) => Console.WriteLine($"> {text}");

Action trenner = Trennlinie;
Action<string> ausgabe = Ausgeben;

trenner();                // --------------------
ausgabe("Figur erzeugt"); // > Figur erzeugt
```

Richtig nützlich werden `Action` und `Func` als **Parameter**. Die folgende Methode führt eine beliebige Aktion `n`-mal aus – sie muss nicht wissen, was die Aktion tut:

```csharp
static void Wiederhole(int n, Action aktion)
{
    for (int i = 0; i < n; i++)
        aktion();
}

Wiederhole(3, Trennlinie);
// --------------------
// --------------------
// --------------------
```

Ohne Delegaten müssten wir für jede Art von Wiederholung eine eigene Schleife schreiben. Mit `Action` als Parameter trennen wir das *Wie oft* (in `Wiederhole`) vom *Was* (in der übergebenen Methode) – eine saubere **Zerlegung** in zwei unabhängige Verantwortlichkeiten.

## `Predicate<T>` und `Comparison<T>`

Zwei weitere vordefinierte Delegaten begegnen dir ständig, weil `List<T>` und `Array` sie in ihren Methoden verlangen. `Predicate<T>` ist eine Methode, die ein `T` prüft und `bool` liefert – technisch dasselbe wie `Func<T, bool>`, aber älter und in den Signaturen von `FindAll`, `RemoveAll`, `Exists` oder `Find` fest verdrahtet:

```csharp
List<int> zahlen = [3, 8, 12, 5, 20, 7];

Predicate<int> istGerade = IstGerade;
List<int> gerade = zahlen.FindAll(istGerade);
Console.WriteLine(string.Join(", ", gerade));   // 8, 12, 20

zahlen.RemoveAll(IstGerade);                    // Methodengruppe direkt übergeben
Console.WriteLine(string.Join(", ", zahlen));   // 3, 5, 7
```

`Comparison<T>` beschreibt eine Vergleichsmethode `int Vergleiche(T x, T y)` mit der bekannten Rückgabekonvention negativ / null / positiv. Im Modul [IComparable und Sortieren](/modules/icomparable_sortieren/icomparable_sortieren.md) haben wir die Sortierreihenfolge in die Klasse selbst eingebaut, indem sie `IComparable<T>` implementiert. Mit `Comparison<T>` können wir stattdessen die Reihenfolge *von außen* vorgeben, ohne die Klasse anzufassen – und bei jedem `Sort`-Aufruf eine andere:

```csharp
static int NachLaenge(string a, string b) => a.Length.CompareTo(b.Length);

List<string> staedte = ["Berlin", "Bonn", "Bremen", "Ulm"];
staedte.Sort(NachLaenge);
Console.WriteLine(string.Join(", ", staedte));  // Ulm, Bonn, Berlin, Bremen
```

`Berlin` und `Bremen` haben dieselbe Länge – ihre Reihenfolge zueinander ist bei `List<T>.Sort` nicht garantiert, weil das Verfahren nicht stabil ist. Das haben wir bei den [Such- und Sortieralgorithmen](/modules/such_und_sortieralgorithmen/such_und_sortieralgorithmen.md) schon gesehen.

## Wann ein eigener Delegattyp?

Die vordefinierten Typen decken fast alles ab. Ein eigener Delegattyp lohnt sich nur, wenn die Signatur `ref`- oder `out`-Parameter enthält (das können `Func` und `Action` nicht) oder wenn der Name selbst etwas Fachliches ausdrücken soll – ein `EventHandler` sagt mehr als ein `Action<object, EventArgs>`. Genau aus diesem Grund haben Ereignisse in .NET ihre eigenen, benannten Delegattypen, wie wir im Modul [Ereignisse](/modules/ereignisse/ereignisse.md) sehen werden.

Im Zweifel `Func` oder `Action` statt eines eigenen Delegattyps. Jeder zusätzliche Typ ist ein Name mehr, den andere lernen müssen – und `Func<int, int, int>` versteht jede C#-Programmiererin sofort.
{: .notice--primary}

Achtung bei der Reihenfolge der Typparameter: Bei `Func<string, int>` ist `string` der Parameter und `int` der Rückgabetyp – nicht umgekehrt. `Func<int, string>` ist ein völlig anderer Typ, und der Compiler meldet beim Zuweisen einer unpassenden Methode wieder den bekannten Fehler CS0123.
{: .notice--warning}

Übung: Schreibe eine Methode `Transformiere(int[] werte, Func<int, int> abbildung)`, die ein neues Array mit den abgebildeten Werten zurückgibt. Rufe sie mit zwei verschiedenen Methoden auf (`Verdopple`, `Quadriere`). Schreibe anschließend `FuerAlle(int[] werte, Action<int> aktion)` und überlege: Warum kann `FuerAlle` keinen Rückgabewert liefern – und wie könntest du trotzdem eine Summe bilden?
{: .notice--info}

## Weitere Quellen

- [Func<T,TResult>-Delegat – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.func-2)
- [Action<T>-Delegat – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.action-1)
- [Predicate<T>-Delegat – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.predicate-1)
- [Comparison<T>-Delegat – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.comparison-1)
