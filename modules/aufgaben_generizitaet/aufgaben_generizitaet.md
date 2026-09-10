---
title: "🧩 Aufgaben und Beispiele: Generizität"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Programmieren lernt man nicht nur durch Codezeilen tippen — sondern auch durch **Nachdenken**. Die folgenden Aufgaben trainieren *Computational Thinking*: die Fähigkeit, Probleme so zu strukturieren, dass ein Computer sie lösen kann. Dazu gehören Abstraktion, Zerlegung, Mustererkennung und Algorithmenentwurf. Generizität ist dabei Mustererkennung in Reinform: Wer erkennt, dass drei Klassen bis auf einen Typ identisch sind, hat den Typparameter schon gefunden. Nimm dir für jede Aufgabe Zeit, bevor du die Lösung aufklappst.

## Aufgabe 1 — Mustererkennung

In einem Projekt sind über die Zeit die folgenden drei Klassen entstanden. Lies sie aufmerksam und vergleiche sie.

```csharp
class IntPaar
{
    public int Erstes { get; }
    public int Zweites { get; }

    public IntPaar(int erstes, int zweites)
    {
        Erstes = erstes;
        Zweites = zweites;
    }

    public IntPaar Vertauscht() => new IntPaar(Zweites, Erstes);
    public override string ToString() => $"({Erstes}, {Zweites})";
}

class StringPaar
{
    public string Erstes { get; }
    public string Zweites { get; }

    public StringPaar(string erstes, string zweites)
    {
        Erstes = erstes;
        Zweites = zweites;
    }

    public StringPaar Vertauscht() => new StringPaar(Zweites, Erstes);
    public override string ToString() => $"({Erstes}, {Zweites})";
}

class NameFigurPaar
{
    public string Erstes { get; }
    public Figur Zweites { get; }

    public NameFigurPaar(string erstes, Figur zweites)
    {
        Erstes = erstes;
        Zweites = zweites;
    }

    public override string ToString() => $"({Erstes}, {Zweites})";
}
```

- Welche Teile sind in allen drei Klassen identisch, welche unterscheiden sich?
- Warum reicht **ein** Typparameter nicht aus, um alle drei Klassen zu ersetzen?
- Was passiert mit der Methode `Vertauscht()`, wenn die beiden Typen verschieden sind? Welchen Rückgabetyp muss sie haben?
- Entwirf eine Klasse `Paar<T1, T2>`, die alle drei ersetzt, und zeige, wie die drei ursprünglichen Verwendungen damit aussehen.

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Das Muster erkennen:**

Alle drei Klassen haben zwei schreibgeschützte Properties, einen Konstruktor, der beide setzt, und dieselbe `ToString`-Methode. Der einzige Unterschied ist der Typ der beiden Properties: `int`/`int`, `string`/`string` und `string`/`Figur`. Genau die Stellen, an denen sich die Klassen unterscheiden, werden zu Typparametern.

**Schritt 2 — Warum zwei Typparameter:**

Bei `NameFigurPaar` haben die beiden Werte unterschiedliche Typen. Mit einem einzigen `T` könnte man nur Paare gleicher Typen bilden. Also braucht die Klasse zwei Typparameter, `T1` und `T2`, die unabhängig voneinander belegt werden – wie `TKey` und `TValue` bei `Dictionary`.

**Schritt 3 — Die generische Klasse:**

```csharp
class Paar<T1, T2>
{
    public T1 Erstes { get; }
    public T2 Zweites { get; }

    public Paar(T1 erstes, T2 zweites)
    {
        Erstes = erstes;
        Zweites = zweites;
    }

    public Paar<T2, T1> Vertauscht() => new Paar<T2, T1>(Zweites, Erstes);

    public override string ToString() => $"({Erstes}, {Zweites})";
}
```

Die drei ursprünglichen Verwendungen werden zu:

```csharp
var zahlen = new Paar<int, int>(3, 7);
var woerter = new Paar<string, string>("Glas", "Papier");
var benannt = new Paar<string, Figur>("Logo", new Kreis("K1", 0, 0, 2));

Console.WriteLine(zahlen.Vertauscht());   // (7, 3)
Paar<Figur, string> gedreht = benannt.Vertauscht();
```

**Zentrale Designentscheidungen:**

- **`Vertauscht()` gibt `Paar<T2, T1>` zurück, nicht `Paar<T1, T2>`:** Beim Vertauschen tauschen auch die Typen die Plätze. Bei `IntPaar` fiel das nicht auf, weil beide Typen gleich waren – erst die generische Version zwingt uns, diese Frage sauber zu beantworten. `NameFigurPaar` hatte die Methode vermutlich deshalb nie bekommen.
- **`ToString` funktioniert ohne Constraint:** Die String-Interpolation ruft `ToString()` auf, und das hat jeder Typ, weil es von `object` geerbt wird. Für die Ausgabe der `Figur` greift dank `override` die polymorphe Variante aus dem Geometrieeditor.
- **Schreibgeschützte Properties:** Ein Paar ist ein Wert, der nach dem Erzeugen nicht mehr verändert wird. Wer ein anderes Paar will, erzeugt ein neues – wie `Vertauscht()` es tut. In .NET gibt es dieses Konzept fertig als `Tuple<T1, T2>` bzw. als Wertetupel `(T1, T2)`.

</details>

## Aufgabe 2 — Algorithmenentwurf

Die `FigurenVerwaltung` des Geometrieeditors soll alle Figuren liefern, die eine bestimmte Bedingung erfüllen – zum Beispiel alle mit einer Fläche über 10 oder alle, deren Name mit „K“ beginnt. Jedes Mal eine neue Methode `AlleMitFlaecheUeber`, `AlleMitNameBeginnendMit` zu schreiben, ist offensichtlich keine gute Idee.

Entwirf eine **generische** Methode `Filtern<T>`, die aus einer `List<T>` alle Elemente heraussucht, die eine Bedingung erfüllen. Delegates und Lambdas kennen wir noch nicht – die Bedingung muss also auf einem anderen Weg an die Methode übergeben werden.

- Wie kann man „eine Bedingung“ als Objekt darstellen, das man einer Methode übergeben kann? Welches Konzept aus Vorlesung 02 hilft dabei?
- Welche Signatur hat `Filtern<T>`?
- Schreibe zwei konkrete Bedingungen für `Figur` und zeige die Nutzung.
- Was ist umständlich an dieser Lösung?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Die Bedingung als Interface:**

Eine Bedingung ist etwas, das für ein Element `true` oder `false` liefert. Das können wir als Interface beschreiben – und weil der Elementtyp offen bleiben soll, ist das Interface selbst generisch:

```csharp
interface IPruefer<T>
{
    bool Pruefen(T element);
}
```

**Schritt 2 — Die generische Methode:**

`Filtern<T>` nimmt eine Liste und einen Prüfer entgegen und sammelt alle Elemente, für die der Prüfer `true` liefert. Der Typparameter `T` verbindet beide Parameter: Der Prüfer muss zu den Elementen der Liste passen.

```csharp
static List<T> Filtern<T>(List<T> elemente, IPruefer<T> pruefer)
{
    var ergebnis = new List<T>();
    foreach (T element in elemente)
    {
        if (pruefer.Pruefen(element))
        {
            ergebnis.Add(element);
        }
    }
    return ergebnis;
}
```

**Schritt 3 — Konkrete Bedingungen:**

Jede Bedingung ist eine eigene Klasse, die `IPruefer<Figur>` implementiert. Parameter wie der Schwellwert wandern in den Konstruktor:

```csharp
class FlaecheUeber : IPruefer<Figur>
{
    private readonly double grenze;
    public FlaecheUeber(double grenze) { this.grenze = grenze; }
    public bool Pruefen(Figur figur) => figur.Flaeche > grenze;
}

class NameBeginntMit : IPruefer<Figur>
{
    private readonly string praefix;
    public NameBeginntMit(string praefix) { this.praefix = praefix; }
    public bool Pruefen(Figur figur) => figur.Name.StartsWith(praefix);
}

List<Figur> figuren = new()
{
    new Kreis("K1", 0, 0, 1),
    new Rechteck("R1", 0, 0, 4, 5),
    new Kreis("K2", 1, 1, 3)
};

List<Figur> grosse = Filtern(figuren, new FlaecheUeber(10));
List<Figur> kreise = Filtern(figuren, new NameBeginntMit("K"));
Console.WriteLine(grosse.Count);  // 2  (R1 mit 20, K2 mit 28,27)
Console.WriteLine(kreise.Count);  // 2
```

**Zentrale Designentscheidungen:**

- **Generisches Interface statt `IPruefer` mit `object`:** `IPruefer<Figur>.Pruefen` bekommt eine `Figur` und kann direkt auf `Flaeche` zugreifen. Mit `object` müsste jede Bedingung erst casten – und `Filtern` könnte einen `IPruefer` für Strings mit einer Figurenliste kombinieren, ohne dass der Compiler es merkt.
- **`Filtern<T>` weiß nichts über Figuren:** Die Methode funktioniert genauso für `List<int>` mit einem `IPruefer<int>`. Der Algorithmus (durchlaufen, prüfen, sammeln) ist vom Elementtyp und von der Bedingung getrennt – das ist das Ziel.
- **Das Umständliche:** Für jede noch so kleine Bedingung braucht man eine ganze Klasse mit Konstruktor und Feld. Der eigentliche Inhalt ist eine einzige Zeile (`figur.Flaeche > grenze`), umgeben von zehn Zeilen Verpackung. Genau dieses Problem lösen Delegates und Lambdas in Vorlesung 06: Dort wird aus `new FlaecheUeber(10)` ein `f => f.Flaeche > 10`, und `Filtern<T>` wird zu `Where` aus LINQ. Das Muster – Algorithmus generisch, Bedingung austauschbar – bleibt dasselbe.

</details>

## Aufgabe 3 — Fehler finden

Der folgende Code stammt aus einem Versuch, ein generisches Lager zu schreiben. Er enthält **drei** Compilerfehler, die alle mit fehlenden oder falschen Constraints zu tun haben.

```csharp
class Lager<T>
{
    private readonly List<T> bestand = new();

    public void Einlagern(T artikel)
    {
        if (artikel == null)
        {
            throw new ArgumentNullException(nameof(artikel));
        }
        bestand.Add(artikel);
    }

    public T Groesster()
    {
        T groesster = bestand[0];
        foreach (T artikel in bestand)
        {
            if (artikel.CompareTo(groesster) > 0)
            {
                groesster = artikel;
            }
        }
        return groesster;
    }

    public T Musterartikel()
    {
        return new T();
    }
}
```

- Finde die drei Stellen, die der Compiler ablehnt, und formuliere in eigenen Worten, warum.
- Welche Constraints beheben die Fehler? Reicht **ein** Constraint für alle drei?
- Diskutiere: Welche Typen kann `Lager<T>` nach deiner Korrektur noch aufnehmen – und ist das ein Problem?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Die drei Fehler:**

1. `artikel == null`: Ein `==`-Vergleich mit `null` ist für einen uneingeschränkten Typparameter nicht erlaubt, weil `T` ein Werttyp wie `int` sein könnte, der nie `null` ist. (Genau genommen erlaubt der Compiler `== null` für uneingeschränkte `T` in neueren Versionen, wertet es für Werttypen aber immer als `false` aus – das ist dann kein Fehler, aber irreführend. Sauber wird es erst mit einem Constraint.)
2. `artikel.CompareTo(groesster)`: `T` hat keine Methode `CompareTo` – der Compiler kennt nur die Mitglieder von `object`.
3. `new T()`: Ohne Constraint weiß der Compiler nicht, ob `T` einen parameterlosen Konstruktor hat.

**Schritt 2 — Constraints hinzufügen:**

Ein Constraint reicht nicht, weil die drei Stellen drei verschiedene Fähigkeiten verlangen: `null`-Vergleich, Vergleichbarkeit und Erzeugbarkeit.

```csharp
class Lager<T> where T : class, IComparable<T>, new()
{
    // Rumpf unverändert
}
```

`class` macht den `null`-Vergleich eindeutig, `IComparable<T>` schaltet `CompareTo` frei, `new()` erlaubt `new T()`. Die Reihenfolge ist vorgeschrieben: `class` (oder eine Basisklasse) zuerst, dann Interfaces, `new()` am Ende.

**Schritt 3 — Was noch hineinpasst:**

Nach der Korrektur akzeptiert `Lager<T>` nur noch Referenztypen, die `IComparable<T>` implementieren und einen parameterlosen Konstruktor haben. `string` fällt heraus (kein parameterloser Konstruktor), `int` fällt heraus (Werttyp), und `Figur` fällt heraus (weder vergleichbar noch parameterlos konstruierbar). Übrig bleiben eigene Klassen, die genau dafür geschrieben wurden.

**Zentrale Designentscheidungen:**

- **Constraints sind ein Tauschgeschäft:** Jeder Constraint erlaubt der Klasse mehr und den Nutzern weniger. Drei Constraints auf einmal sind ein Warnsignal – vermutlich tut die Klasse zu viel.
- **`Musterartikel()` gehört wahrscheinlich nicht hierher:** Warum sollte ein Lager Artikel erzeugen können? Streicht man die Methode, entfällt `new()`, und `string` wird wieder zulässig.
- **Alternative zu `class`:** Wenn auch Werttypen erlaubt sein sollen, ersetzt man den `null`-Vergleich durch `where T : notnull` und lässt die Prüfung weg – der Compiler stellt dann sicher, dass niemand `Lager<string?>` schreibt.
- **`Groesster()` bei leerem Bestand:** `bestand[0]` wirft eine `ArgumentOutOfRangeException`. Das ist kein Compilerfehler, aber ein Randfall, den eine gute Implementierung mit einer aussagekräftigen `InvalidOperationException` abfängt.

</details>

## Aufgabe 4 — Abstraktion

Ein Sensor liefert ständig neue Messwerte, aber nur die letzten `n` sind interessant – ältere Werte dürfen verworfen werden. Diese Datenstruktur heißt **Ringpuffer** (*Ring Buffer*): ein Array fester Größe, in das man reihum schreibt. Ist der Puffer voll, überschreibt jeder neue Wert den ältesten.

Entwirf eine generische Klasse `Ringpuffer<T>` mit fester Kapazität.

- Welche Felder braucht die Klasse? Wie merkt man sich, wo der älteste und wo der nächste freie Platz ist?
- Welche Operationen gehören in die Schnittstelle? Mindestens: `Hinzufuegen(T)`, `Aeltestes()`, `Anzahl`, und ein Weg, alle Elemente in der Reihenfolge vom ältesten zum neuesten zu durchlaufen.
- Randfälle: Was passiert bei `Aeltestes()` auf einem leeren Puffer? Was, wenn die Kapazität 0 ist? Wie berechnet man den Index nach dem letzten Platz im Array?
- Braucht `Ringpuffer<T>` einen Constraint?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Datenmodell:**

Ein Array `T[] daten` fester Größe, ein Index `start` auf das älteste Element und ein Zähler `anzahl`. Die Position des nächsten freien Platzes ergibt sich daraus: `(start + anzahl) % daten.Length`. Der Modulo-Operator sorgt dafür, dass der Index nach dem letzten Platz wieder bei 0 landet – das ist der „Ring“.

**Schritt 2 — Die Klasse:**

```csharp
class Ringpuffer<T>
{
    private readonly T[] daten;
    private int start;
    private int anzahl;

    public Ringpuffer(int kapazitaet)
    {
        if (kapazitaet <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(kapazitaet), "Die Kapazität muss positiv sein.");
        }
        daten = new T[kapazitaet];
    }

    public int Anzahl => anzahl;
    public int Kapazitaet => daten.Length;

    public void Hinzufuegen(T element)
    {
        int naechster = (start + anzahl) % daten.Length;
        daten[naechster] = element;

        if (anzahl < daten.Length)
        {
            anzahl++;
        }
        else
        {
            // Puffer voll: das älteste Element wurde überschrieben,
            // der Start rückt eine Position weiter
            start = (start + 1) % daten.Length;
        }
    }

    public T Aeltestes()
    {
        if (anzahl == 0)
        {
            throw new InvalidOperationException("Der Puffer ist leer.");
        }
        return daten[start];
    }

    public T[] AlsArray()
    {
        T[] ergebnis = new T[anzahl];
        for (int i = 0; i < anzahl; i++)
        {
            ergebnis[i] = daten[(start + i) % daten.Length];
        }
        return ergebnis;
    }
}
```

**Schritt 3 — Verhalten prüfen:**

```csharp
var messwerte = new Ringpuffer<double>(3);
messwerte.Hinzufuegen(1.0);
messwerte.Hinzufuegen(2.0);
messwerte.Hinzufuegen(3.0);
messwerte.Hinzufuegen(4.0);          // überschreibt 1.0

Console.WriteLine(messwerte.Aeltestes());                    // 2
Console.WriteLine(string.Join(", ", messwerte.AlsArray()));  // 2, 3, 4
Console.WriteLine(messwerte.Anzahl);                         // 3
```

Nach dem vierten `Hinzufuegen` steht die `4.0` physisch an Index 0 des Arrays, aber logisch ist sie das neueste Element – `start` zeigt jetzt auf Index 1, wo die `2.0` liegt.

**Zentrale Designentscheidungen:**

- **`start` und `anzahl` statt `start` und `ende`:** Mit zwei Indizes kann man „leer“ und „voll“ nicht unterscheiden – in beiden Fällen wäre `start == ende`. Der Zähler `anzahl` macht beide Zustände eindeutig.
- **Kein Constraint nötig:** Der Ringpuffer speichert, überschreibt und liefert Elemente, vergleicht sie aber nie und erzeugt keine. Er funktioniert daher mit jedem Typ – `double` für Messwerte, `string` für Logzeilen, `Figur` für eine Undo-Historie im Geometrieeditor. Das ist ein gutes Zeichen: Je weniger Constraints eine Datenstruktur braucht, desto allgemeiner ist sie.
- **Kapazität 0 wird im Konstruktor abgelehnt:** Sonst würde `% daten.Length` zu einer `DivideByZeroException` führen – ein Fehler, den man lieber sofort und mit klarer Meldung sieht.
- **`AlsArray()` kopiert in logischer Reihenfolge:** Der Aufrufer sieht nie den internen Ring, sondern immer „ältestes zuerst“. Eleganter wäre es, `IEnumerable<T>` zu implementieren, damit `foreach` direkt funktioniert – wie das geht, sehen wir beim Iterator-Muster in Vorlesung 07.

</details>
