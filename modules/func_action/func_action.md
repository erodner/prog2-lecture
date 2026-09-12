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

Im Modul [Delegaten](/modules/delegaten/delegaten.md) haben wir für jede Aufgabe einen eigenen Delegattyp deklariert: `RechenHandler` für Rechnungen, `ZugVerhalten` für Gegnerzüge, `MeldungHandler` für Meldungen. Wer so weitermacht, hat bald Dutzende Delegattypen, die sich nur im Namen unterscheiden – `delegate int RechenHandler(int a, int b)` und `delegate int VergleichsHandler(int x, int y)` beschreiben exakt dieselbe Signatur. Das ist, als würde man für jeden Boten ein eigenes Auftragsformular entwerfen, obwohl alle Aufträge dieselbe Form haben. .NET bringt deshalb eine Handvoll **generischer Delegattypen** mit, die praktisch jede Signatur abdecken. Seit es sie gibt, braucht man eigene Delegattypen nur noch selten.

## `Func<…, TResult>` – Methoden mit Rückgabewert

`Func` steht für Methoden, die etwas zurückgeben. Der **letzte** Typparameter ist immer der Rückgabetyp, alle davor sind die Parameter. `Func<int, int, int>` beschreibt also eine Methode mit zwei `int`-Parametern und `int`-Rückgabe – genau unser `RechenHandler`, ohne dass wir ihn deklarieren müssen:

```csharp
static int Addiere(int x, int y) => x + y;
static bool IstGerade(int zahl) => zahl % 2 == 0;

Func<int, int, int> rechnung = Addiere;
Func<int, bool> pruefung = IstGerade;

Console.WriteLine(rechnung(5, 3));   // 8
Console.WriteLine(pruefung(4));      // True
```

Die Zuweisung `rechnung = Addiere` nennt man **Methodengruppen-Konvertierung**: Der Name einer Methode ohne Klammern steht für die Gruppe aller Überladungen dieses Namens, und der Compiler sucht die Überladung heraus, die zur Signatur des Delegaten passt. `Func` gibt es mit bis zu 16 Parametern – in der Praxis braucht man selten mehr als drei.

Damit verschwindet auch der Delegattyp `ZugVerhalten` aus dem letzten Modul. Seine Signatur `Richtung? (Spielfeld, Gegner)` schreibt man als `Func<Spielfeld, Gegner, Richtung?>` – zwei Parameter, ein Rückgabetyp, und der Rückgabetyp darf ein `Nullable<T>` sein wie jeder andere Typ:

```csharp
public sealed class Zufallsgegner : Gegner
{
    private readonly Func<Spielfeld, Gegner, Richtung?> verhalten;

    public Zufallsgegner(Position position, Func<Spielfeld, Gegner, Richtung?> verhalten)
        : base("Streuner", position)
    {
        this.verhalten = verhalten;
    }

    public override char Symbol => 'Z';

    public override Richtung? NaechsterZug(Spielfeld feld) => verhalten(feld, this);
}
```

Der Rest des Spiels ändert sich nicht: `Spielfeld.GegnerZiehen` ruft weiterhin `g.NaechsterZug(this)` auf. Verloren geht allerdings etwas: `ZugVerhalten` stand als Name im Code, `Func<Spielfeld, Gegner, Richtung?>` erklärt sich nur aus den Typen heraus. Darauf kommen wir am Ende zurück.

## `Action<…>` – Methoden ohne Rückgabewert

`void` ist kein Typ und kann deshalb nicht als Typparameter stehen. Für Methoden ohne Rückgabewert gibt es darum die zweite Familie: `Action` (keine Parameter), `Action<T>` (ein Parameter), `Action<T1, T2>` und so weiter:

```csharp
static void InKonsole(string text) => Console.WriteLine($"> {text}");

Action<string> melden = InKonsole;
melden("Der Held findet einen Schatz");   // > Der Held findet einen Schatz
```

Richtig nützlich werden `Action` und `Func` als **Parameter**. Die folgende Methode spielt eine ganze Runde ab und meldet jedes Ergebnis über die übergebene Aktion – wohin gemeldet wird, entscheidet der Aufrufer:

```csharp
static void RundeSpielen(Spielfeld feld, Richtung richtung, Action<string> melden)
{
    feld.SpielerZieht(richtung);
    melden(feld.LetzteMeldung);
    melden($"Runde {feld.Runde}, {feld.Spieler.Lebenspunkte} Lebenspunkte");
}

List<string> protokoll = [];
RundeSpielen(feld, Richtung.Rechts, InKonsole);
RundeSpielen(feld, Richtung.Oben, protokoll.Add);   // in eine List<string> statt auf die Konsole
```

`protokoll.Add` ist dabei ganz gewöhnliche Methodengruppen-Konvertierung: `List<string>.Add` nimmt einen `string` und gibt nichts zurück, passt also auf `Action<string>`. Ohne Delegaten müssten wir `RundeSpielen` zweimal schreiben – einmal für die Konsole, einmal fürs Protokoll. Mit `Action` als Parameter trennen wir das *Was* (in `RundeSpielen`) vom *Wohin* (in der übergebenen Methode) – eine saubere **Zerlegung** in zwei unabhängige Verantwortlichkeiten.

## `Predicate<T>` und `Comparison<T>`

Zwei weitere vordefinierte Delegaten begegnen dir ständig, weil `List<T>` und `Array` sie in ihren Methoden verlangen. `Predicate<T>` ist eine Methode, die ein `T` prüft und `bool` liefert – technisch dasselbe wie `Func<T, bool>`, aber älter und in den Signaturen von `FindAll`, `RemoveAll`, `Exists` oder `Find` fest verdrahtet:

```csharp
static bool IstVerfolger(Gegner g) => g is Verfolger;
static bool StehtAmRand(Gegner g) => g.Position.X == 0 || g.Position.Y == 0;

List<Gegner> gegner = [new Wache(new Position(3, 1)), new Verfolger(new Position(8, 4))];

Predicate<Gegner> jagt = IstVerfolger;
List<Gegner> jaeger = gegner.FindAll(jagt);
Console.WriteLine(jaeger.Count);   // 1

gegner.RemoveAll(StehtAmRand);     // Methodengruppe direkt übergeben
```

`Comparison<T>` beschreibt eine Vergleichsmethode `int Vergleiche(T x, T y)` mit der bekannten Rückgabekonvention negativ / null / positiv. Im Modul [IComparable und Sortieren](/modules/icomparable_sortieren/icomparable_sortieren.md) haben wir die Sortierreihenfolge in die Klasse selbst eingebaut, indem sie `IComparable<T>` implementiert. Für `Gegner` wäre das die falsche Stelle: „Welcher Gegner ist kleiner?“ hat keine allgemeingültige Antwort – *für diese Anzeige* ist es der, der dem Helden am nächsten steht. Mit `Comparison<T>` geben wir die Reihenfolge deshalb *von außen* vor, ohne die Klasse anzufassen:

```csharp
static readonly Position Ecke = new(0, 0);

static int NachEntfernungZurEcke(Gegner a, Gegner b)
    => a.Position.Entfernung(Ecke).CompareTo(b.Position.Entfernung(Ecke));

gegner.Sort(NachEntfernungZurEcke);
Console.WriteLine(gegner[0].Beschreibung());   // der oberste linke Gegner zuerst
```

Zwei Gegner mit gleicher Entfernung landen dabei in beliebiger Reihenfolge zueinander, weil `List<T>.Sort` nicht stabil ist. Das haben wir bei den [Such- und Sortieralgorithmen](/modules/such_und_sortieralgorithmen/such_und_sortieralgorithmen.md) schon gesehen. Störend ist die Konstante `Ecke`: Eigentlich wollen wir nach dem Abstand zum *Helden* sortieren, und dessen Position steht in einer lokalen Variablen, die eine benannte statische Methode nicht sehen kann. Genau dieses Problem lösen die [Lambda-Ausdrücke](/modules/lambda_ausdruecke/lambda_ausdruecke.md) im nächsten Modul.

## Wann ein eigener Delegattyp?

Die vordefinierten Typen decken fast alles ab. Ein eigener Delegattyp lohnt sich nur, wenn die Signatur `ref`- oder `out`-Parameter enthält (das können `Func` und `Action` nicht) oder wenn der Name selbst etwas Fachliches ausdrücken soll. Genau das war der Unterschied zwischen `ZugVerhalten` und `Func<Spielfeld, Gegner, Richtung?>`: Steht der Typ an einer einzigen Stelle, nimmt man `Func`; taucht er in Feldern, Parametern und Rückgabetypen quer durchs Projekt auf, ist ein Name Gold wert. Aus demselben Grund haben Ereignisse in .NET ihre eigenen, benannten Delegattypen – ein `EventHandler<SchatzEventArgs>` sagt mehr als ein `Action<object, SchatzEventArgs>`, wie wir im Modul [Ereignisse](/modules/ereignisse/ereignisse.md) sehen werden.

Im Zweifel `Func` oder `Action` statt eines eigenen Delegattyps. Jeder zusätzliche Typ ist ein Name mehr, den andere lernen müssen – und `Func<int, int, int>` versteht jede C#-Programmiererin sofort.
{: .notice--primary}

Achtung bei der Reihenfolge der Typparameter: Bei `Func<Spielfeld, Richtung>` ist `Spielfeld` der Parameter und `Richtung` der Rückgabetyp – nicht umgekehrt. `Func<Richtung, Spielfeld>` ist ein völlig anderer Typ, und der Compiler meldet beim Zuweisen einer unpassenden Methode wieder den bekannten Fehler CS0123.
{: .notice--warning}

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v02-interfaces`).

Übung: Schreibe eine Methode `Zaehle(IEnumerable<Spielobjekt> objekte, Func<Spielobjekt, bool> bedingung)`, die zählt, wie viele Objekte die Bedingung erfüllen, und rufe sie für `feld.AlleObjekte` zweimal auf – einmal für Wände, einmal für passierbare Felder. Ersetze anschließend in `Adventure.Kern` die Klassen `Wache` und `Verfolger` durch zwei `Func<Spielfeld, Gegner, Richtung?>`-Werte. Welches der beiden Verhalten lässt sich so *nicht* vollständig nachbauen – und warum?
{: .notice--info}

## Weitere Quellen

- [Func<T,TResult>-Delegat – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.func-2)
- [Action<T>-Delegat – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.action-1)
- [Predicate<T>-Delegat – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.predicate-1)
- [Comparison<T>-Delegat – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.comparison-1)
