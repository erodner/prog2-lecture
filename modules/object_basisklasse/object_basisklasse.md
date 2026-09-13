---
title: "Die Basisklasse object"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Schon in Programmierung 1 haben wir `Console.WriteLine(meinObjekt)` geschrieben und irgendetwas ausgegeben bekommen, obwohl unsere Klasse gar keine Ausgabemethode hatte. Und `ToString()` konnte man auf *jedem* Objekt aufrufen, egal welcher Klasse. Woher kommt diese Methode? Die Antwort ist die Wurzel der gesamten Klassenhierarchie: **Jede Klasse erbt von `object`**, ob man es hinschreibt oder nicht. `class Spielobjekt` bedeutet in Wahrheit `class Spielobjekt : object`. Was `object` mitbringt, besitzt also jedes Objekt in .NET – und wer die Standardimplementierungen kennt, versteht, warum man einige davon fast immer überschreiben sollte.

## Was jedes Objekt kann

Die Klasse `object` (in .NET `System.Object`) ist klein. Stark vereinfacht sieht sie so aus:

```csharp
class Object
{
    public virtual string ToString() { ... }        // Textdarstellung
    public virtual bool Equals(object? obj) { ... } // inhaltliche Gleichheit
    public virtual int GetHashCode() { ... }        // Zahl für Hash-Tabellen
    public Type GetType() { ... }                   // Laufzeittyp
    protected object MemberwiseClone() { ... }      // flache Kopie
}
```

Zwei Methoden sind direkt verwendbar: `GetType()` liefert den Laufzeittyp (siehe [Laufzeittyp und Verstecken](/modules/laufzeittyp_verstecken/laufzeittyp_verstecken.md)) und `MemberwiseClone()` erzeugt eine Kopie – sie ist `protected`, kann also nur innerhalb der eigenen Klasse aufgerufen werden. Die drei anderen sind `virtual`, und das ist eine Einladung: Sie sind dafür gedacht, in eigenen Klassen überschrieben zu werden.

## `ToString` – einmal schreiben, überall passend

Ohne Überschreibung liefert `ToString()` nur den vollqualifizierten Klassennamen: `Console.WriteLine(new Wand(...))` würde `Adventure.Kern.Wand` ausgeben. Das nützt niemandem. `Spielobjekt` überschreibt die Methode deshalb – und zwar mit einer einzigen Zeile, die den ganzen Polymorphie-Mechanismus ausnutzt:

```csharp
public class Spielobjekt
{
    public virtual string Beschreibung() => $"{Name} bei {Position}";

    public override string ToString() => Beschreibung();
}
```

`ToString` steht nur in der Basisklasse, ruft aber `Beschreibung()` auf – und weil diese Methode `virtual` ist, entscheidet der Laufzeittyp, welche Fassung läuft:

```csharp
List<Spielobjekt> objekte = new()
{
    new Wand(new Position(5, 2)),
    new Spieler("Held", new Position(1, 1))
};

foreach (Spielobjekt o in objekte)
{
    Console.WriteLine(o);
}
// Wand bei (5, 2)
// Held bei (1, 1), 3 Lebenspunkte
```

Keine der abgeleiteten Klassen überschreibt `ToString`, und trotzdem gibt jede das Richtige aus. Das ist ein Muster, das sich merken lohnt: **eine `ToString`-Implementierung in der Basisklasse, die eine `virtual`-Methode aufruft**, statt `ToString` in jeder Klasse erneut zu schreiben.

## Gleichheit: die Standardimplementierung enttäuscht

Bei `Equals` und `GetHashCode` ist mehr Handarbeit nötig. Nehmen wir eine ganz gewöhnliche Klasse für ein Koordinatenpaar:

```csharp
class Koordinate
{
    public int X { get; }
    public int Y { get; }

    public Koordinate(int x, int y) { X = x; Y = y; }
}

Koordinate a = new Koordinate(1, 2);
Koordinate b = new Koordinate(1, 2);
Console.WriteLine(a);                                    // Koordinate
Console.WriteLine(a.Equals(b));                          // False
Console.WriteLine(a.GetHashCode() == b.GetHashCode());   // False (fast sicher)
```

Drei Enttäuschungen: `ToString()` liefert nur den Klassennamen. `Equals()` vergleicht **Referenzen** – `a` und `b` sind zwei verschiedene Objekte auf dem Heap, also „ungleich“, obwohl beide dasselbe Feld bezeichnen. Und `GetHashCode()` liefert eine Art Objektnummer, die für gleiche Inhalte nicht übereinstimmt. Für Referenztypen bedeutet Gleichheit standardmäßig **Identität**, wie wir es schon aus [Programmierung 1](https://www.erodner.de/prog-lecture/modules/werttypen_referenztypen/werttypen_referenztypen/) kennen.

Für ein Spielfeld wäre das fatal. Die Methode `ObjektAn` vergleicht in jeder Runde Positionen:

```csharp
foreach (Spielobjekt o in objekte)
{
    if (o.Position == position) return o;
}
```

Mit einer Klasse `Koordinate` als Positionstyp würde diese Schleife **nie** etwas finden, denn die gesuchte Position ist ein anderes Objekt als die gespeicherte. Der Held liefe ungebremst durch jede Wand.

## Wertsemantik geschenkt: `record struct`

Genau deshalb ist `Position` im Adventure kein gewöhnlicher Referenztyp, sondern ein `readonly record struct`:

```csharp
public readonly record struct Position(int X, int Y)
{
    /// <summary>Manhattan-Entfernung: Anzahl der Schritte ohne Diagonalen.</summary>
    public int Entfernung(Position andere) => Math.Abs(X - andere.X) + Math.Abs(Y - andere.Y);

    public override string ToString() => $"({X}, {Y})";
}
```

Der Compiler erzeugt für einen `record` automatisch `Equals`, `GetHashCode` sowie die Operatoren `==` und `!=` – alle auf Basis der Felder. Damit gilt:

```csharp
Position p1 = new Position(3, 4);
Position p2 = new Position(3, 4);
Console.WriteLine(p1.Equals(p2));                          // True
Console.WriteLine(p1 == p2);                               // True
Console.WriteLine(p1.GetHashCode() == p2.GetHashCode());   // True
Console.WriteLine(p1.Entfernung(new Position(3, 0)));      // 4
```

Zwei Positionen, die unabhängig voneinander entstanden sind, gelten als gleich, weil ihr *Inhalt* gleich ist. Das ist die Semantik, die wir für eine Koordinate wollen: Position (3, 4) ist Position (3, 4), egal wer sie erzeugt hat. `ToString` haben wir hier von Hand überschrieben, weil uns `(3, 4)` in der Statusausgabe besser gefällt als die generierte Fassung `Position { X = 3, Y = 4 }`.

Ein `record` nimmt dir die Arbeit ab – aber nur, solange die generierte Gleichheit die richtige ist. Vergleicht ein Typ alle seine Felder, passt es. Soll nur ein Teil zählen (etwa eine Kennnummer, nicht der aktuelle Zustand), musst du `Equals` und `GetHashCode` weiterhin selbst schreiben.
{: .notice--primary}

## `Equals` und `GetHashCode` von Hand

Wollen wir dieselbe Wertsemantik in einer gewöhnlichen Klasse, müssen wir beide Methoden mit `override` selbst ersetzen – demselben Mechanismus wie in [`virtual` und `override`](/modules/virtual_override/virtual_override.md):

```csharp
class Koordinate
{
    // Properties und Konstruktor wie oben

    public override string ToString() => $"({X}, {Y})";

    public override bool Equals(object? obj)
    {
        return obj is Koordinate andere && X == andere.X && Y == andere.Y;
    }

    public override int GetHashCode() => HashCode.Combine(X, Y);
}

Koordinate a = new Koordinate(1, 2);
Koordinate b = new Koordinate(1, 2);
Console.WriteLine(a);                                    // (1, 2)
Console.WriteLine(a.Equals(b));                          // True
Console.WriteLine(a.GetHashCode() == b.GetHashCode());   // True
Console.WriteLine(a == b);                               // False
```

`Equals` nimmt ein `object?` entgegen, weil es die Signatur der Basisklasse ist – es könnte also alles hereinkommen, auch `null` oder ein `string`. Das Pattern `obj is Koordinate andere` erledigt den Typtest, die Null-Prüfung und die Umwandlung in einem Schritt. `HashCode.Combine` mischt die Felder zu einem gut verteilten Hashcode; einen eigenen Algorithmus braucht man nicht. Der Vergleich mit `Position` zeigt, wie viel Tipparbeit uns der `record struct` erspart – und wie viel man falsch machen kann, wenn man es selbst tut.

## `Equals` versus `==`

Die letzte Ausgabe überrascht: `a == b` bleibt `False`. Der Operator `==` hat mit `Equals` erst einmal nichts zu tun. Für Referenztypen vergleicht er Referenzen, und das Überschreiben von `Equals` ändert daran nichts. Wer `==` mit Wertsemantik möchte, muss den Operator **überladen** – und dann zwingend auch `!=`:

```csharp
public static bool operator ==(Koordinate? a, Koordinate? b) => a is null ? b is null : a.Equals(b);
public static bool operator !=(Koordinate? a, Koordinate? b) => !(a == b);
```

Ein `record` erledigt genau das automatisch, weshalb `p1 == p2` bei Positionen funktioniert. Für eigene Klassen ist es eine Abwägung: Ein Wert wie `Koordinate` profitiert davon, für ein `Spielobjekt` wäre es irreführend – zwei Wände auf demselben Feld wären dann „dieselbe“ Wand, obwohl es zwei Objekte mit eigener Geschichte sind. Mit `(object)a == (object)b` kann man jederzeit auf den Referenzvergleich zurückgreifen.

`Equals` und `GetHashCode` gehören **immer zusammen** überschrieben. Der Vertrag lautet: Sind zwei Objekte laut `Equals` gleich, müssen sie denselben Hashcode liefern. `Dictionary` und `HashSet` verlassen sich darauf – sie suchen zuerst per Hashcode und vergleichen erst dann mit `Equals`. Wer nur `Equals` überschreibt, bekommt einen Compiler-Hinweis (CS0659) und ein `HashSet<Koordinate>`, das (1, 2) zweimal enthält. Warum genau, sehen wir im Modul [Hashcodes und `Equals`](/modules/hashcodes_equals/hashcodes_equals.md) in Vorlesung 06 – dort wird aus der Objektliste des Spielfelds übrigens ein `Dictionary<Position, ...>`, das ohne korrektes `GetHashCode` nicht funktionieren würde.
{: .notice--warning}

## `MemberwiseClone` – die flache Kopie

`MemberwiseClone()` legt ein neues Objekt derselben Klasse an und kopiert alle Felder hinein. Weil die Methode `protected` ist, stellt man sie üblicherweise über eine eigene öffentliche Methode bereit – etwa um vor einem riskanten Zug einen Spielstand zu sichern:

```csharp
public class Spielfeld
{
    private readonly List<Spielobjekt> objekte = new();
    public Spielfeld Kopie() => (Spielfeld)MemberwiseClone();
}

Spielfeld original = new Spielfeld(10, 6, held);
original.Hinzufuegen(new Wand(new Position(5, 2)));

Spielfeld sicherung = original.Kopie();
sicherung.Hinzufuegen(new Wand(new Position(5, 3)));

Console.WriteLine(original.ObjektAn(new Position(5, 3)));   // Wand bei (5, 3)
```

Die neue Wand taucht im Original auf, obwohl wir sie der Kopie hinzugefügt haben: `MemberwiseClone` kopiert bei Referenzfeldern nur die **Referenz**, nicht das Objekt dahinter. Beide Spielfelder teilen sich dieselbe Liste – man spricht von einer **flachen Kopie** (*shallow copy*). Als Sicherung taugt das nicht. Wer eine echte, **tiefe Kopie** braucht, muss die enthaltenen Objekte selbst kopieren; im Adventure lösen wir das später anders, nämlich über einen serialisierbaren Spielstand (siehe [Vorlesung 09](/lectures/09/09.md)).

Übung: Erweitere `Koordinate` um eine Methode `Entfernung`, die wie bei `Position` die Manhattan-Distanz liefert, und lege 100 gleiche Koordinaten in ein `HashSet<Koordinate>`. Wie viele Elemente enthält das Set vor und nach dem Überschreiben von `Equals`/`GetHashCode`? Überlege anschließend, warum `Spielobjekt` seine Gleichheit **nicht** überschreiben sollte, obwohl zwei Wände auf demselben Feld denselben Namen und dieselbe Position hätten.
{: .notice--info}

## Weitere Quellen

- [`System.Object` – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.object)
- [`Equals` überschreiben – Richtlinien – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/statements-expressions-operators/how-to-define-value-equality-for-a-type)
- [Datensatztypen (`record`) – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/builtin-types/record)
- [Gleichheitsoperatoren überladen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/operators/equality-operators)
- [`Object.MemberwiseClone` – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.object.memberwiseclone)
- [SharpLab – sehen, was der Compiler erzeugt](https://sharplab.io/) – links einen `record struct` eintippen, rechts „C#“ als Ausgabe wählen: Dort stehen die generierten `Equals`, `GetHashCode` und `==` ausgeschrieben.
