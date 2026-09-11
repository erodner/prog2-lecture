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

Schon in Programmierung 1 haben wir `Console.WriteLine(meinObjekt)` geschrieben und irgendetwas ausgegeben bekommen, obwohl unsere Klasse gar keine Ausgabemethode hatte. Und `ToString()` konnte man auf *jedem* Objekt aufrufen, egal welcher Klasse. Woher kommt diese Methode? Die Antwort ist die Wurzel der gesamten Klassenhierarchie: **Jede Klasse erbt von `object`**, ob man es hinschreibt oder nicht. `class Bruch` bedeutet in Wahrheit `class Bruch : object`. Was `object` mitbringt, besitzt also jedes Objekt in .NET – und wer die Standardimplementierungen kennt, versteht, warum man einige davon fast immer überschreiben sollte.

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

## Die Standardimplementierungen

Warum überschreiben? Weil die geerbten Versionen für Referenztypen wenig nützen. Nehmen wir die Klasse `Bruch` aus dem Kurs, zunächst ohne jede Überschreibung:

```csharp
class Bruch
{
    public int Zaehler { get; }
    public int Nenner { get; }

    public Bruch(int zaehler, int nenner)
    {
        Zaehler = zaehler;
        Nenner = nenner;
    }
}

Bruch a = new Bruch(1, 2);
Bruch b = new Bruch(1, 2);
Console.WriteLine(a);              // Bruch
Console.WriteLine(a.Equals(b));    // False
Console.WriteLine(a.GetHashCode() == b.GetHashCode());   // False (fast sicher)
```

Drei Enttäuschungen: `ToString()` liefert nur den Klassennamen. `Equals()` vergleicht **Referenzen** – `a` und `b` sind zwei verschiedene Objekte im Speicher, also „ungleich“, obwohl beide ½ darstellen. Und `GetHashCode()` liefert eine Art Objektnummer, die für gleiche Inhalte nicht übereinstimmt. Für Referenztypen bedeutet Gleichheit standardmäßig **Identität**, wie wir es schon aus [Programmierung 1](https://www.erodner.de/prog-lecture/modules/werttypen_referenztypen/werttypen_referenztypen/) kennen.

## `ToString`, `Equals` und `GetHashCode` überschreiben

Für einen Bruch wollen wir Wertsemantik: ½ ist ½, egal in welchem Objekt. Mit `override` – demselben Mechanismus wie in [`virtual` und `override`](/modules/virtual_override/virtual_override.md) – ersetzen wir die drei Methoden:

```csharp
class Bruch
{
    // Properties und Konstruktor wie oben

    public override string ToString() => $"{Zaehler}/{Nenner}";

    public override bool Equals(object? obj)
    {
        return obj is Bruch anderer && Zaehler == anderer.Zaehler && Nenner == anderer.Nenner;
    }

    public override int GetHashCode() => HashCode.Combine(Zaehler, Nenner);
}

Bruch a = new Bruch(1, 2);
Bruch b = new Bruch(1, 2);
Console.WriteLine(a);                                    // 1/2
Console.WriteLine(a.Equals(b));                          // True
Console.WriteLine(a.GetHashCode() == b.GetHashCode());   // True
Console.WriteLine(a == b);                               // False
```

`Equals` nimmt ein `object?` entgegen, weil es die Signatur der Basisklasse ist – es könnte also alles hereinkommen, auch `null` oder ein `string`. Das Pattern `obj is Bruch anderer` erledigt den Typtest, die Null-Prüfung und die Umwandlung in einem Schritt. `HashCode.Combine` mischt die Felder zu einem gut verteilten Hashcode; einen eigenen Algorithmus braucht man nicht. Das vollständige Projekt findest du im Repository unter `examples/12_unittests/Bruch` – dort ist die Klasse zusätzlich um das Interface `IEquatable<Bruch>` ergänzt, dazu mehr in [Vorlesung 02](/lectures/02/02.md).

## `Equals` versus `==`

Die letzte Ausgabe überrascht: `a == b` bleibt `False`. Der Operator `==` hat mit `Equals` erst einmal nichts zu tun. Für Referenztypen vergleicht er Referenzen, und das Überschreiben von `Equals` ändert daran nichts. Wer `==` mit Wertsemantik möchte, muss den Operator **überladen** – und dann zwingend auch `!=`:

```csharp
public static bool operator ==(Bruch? a, Bruch? b) => a is null ? b is null : a.Equals(b);
public static bool operator !=(Bruch? a, Bruch? b) => !(a == b);
```

`string` macht genau das, weshalb `"abc" == "abc"` trotz Referenztyp inhaltlich vergleicht. Für eigene Klassen ist es eine Abwägung: Ein Wert wie `Bruch` profitiert davon, für einen `Roboter` mit Identität wäre es irreführend. Mit `(object)a == (object)b` kann man jederzeit auf den Referenzvergleich zurückgreifen.

`Equals` und `GetHashCode` gehören **immer zusammen** überschrieben. Der Vertrag lautet: Sind zwei Objekte laut `Equals` gleich, müssen sie denselben Hashcode liefern. `Dictionary` und `HashSet` verlassen sich darauf – sie suchen zuerst per Hashcode und vergleichen erst dann mit `Equals`. Wer nur `Equals` überschreibt, bekommt einen Compiler-Hinweis (CS0659) und ein `HashSet<Bruch>`, das ½ zweimal enthält. Warum genau, sehen wir in [Vorlesung 06](/modules/hashcodes_equals/hashcodes_equals.md).
{: .notice--warning}

## `MemberwiseClone` – die flache Kopie

`MemberwiseClone()` legt ein neues Objekt derselben Klasse an und kopiert alle Felder hinein. Weil die Methode `protected` ist, stellt man sie üblicherweise über eine eigene öffentliche Methode bereit:

```csharp
class Fabrikhalle
{
    public string Adresse { get; set; } = "";
    public List<Roboter> Maschinenpark { get; set; } = new List<Roboter>();

    public Fabrikhalle Kopie() => (Fabrikhalle)MemberwiseClone();
}

Fabrikhalle original = new Fabrikhalle { Adresse = "Fabrikstraße 1" };
original.Maschinenpark.Add(new Roboter("Robbi"));

Fabrikhalle kopie = original.Kopie();
kopie.Adresse = "Fabrikstraße 2";
kopie.Maschinenpark.Add(new Roboter("Wischi"));

Console.WriteLine(original.Adresse);               // Fabrikstraße 1
Console.WriteLine(original.Maschinenpark.Count);   // 2
```

Die Adresse ist unabhängig, der Maschinenpark nicht: `MemberwiseClone` kopiert bei Referenzfeldern nur die **Referenz**, nicht das Objekt dahinter. Beide Hallen teilen sich dieselbe Liste – man spricht von einer **flachen Kopie** (*shallow copy*). Wer eine echte, **tiefe Kopie** braucht, muss die enthaltenen Objekte selbst kopieren, etwa mit `Maschinenpark = new List<Roboter>(original.Maschinenpark)`.

Übung: Überschreibe in der Klasse `Roboter` die Methode `ToString()` so, dass `Console.WriteLine(r)` den Namen und den Laufzeittyp ausgibt, etwa „Wischi (Putzroboter)“ – ohne dass `Putzroboter` die Methode noch einmal überschreiben muss. Welche geerbte Methode hilft dir dabei?
{: .notice--info}

## Weitere Quellen

- [`System.Object` – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.object)
- [`Equals` überschreiben – Richtlinien – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/statements-expressions-operators/how-to-define-value-equality-for-a-type)
- [Gleichheitsoperatoren überladen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/operators/equality-operators)
- [`Object.MemberwiseClone` – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.object.memberwiseclone)
