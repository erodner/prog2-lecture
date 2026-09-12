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

Programmieren lernt man nicht nur durch Codezeilen tippen — sondern auch durch **Nachdenken**. Die folgenden Aufgaben trainieren *Computational Thinking*: die Fähigkeit, Probleme so zu strukturieren, dass ein Computer sie lösen kann. Dazu gehören Abstraktion, Zerlegung, Mustererkennung und Algorithmenentwurf. Generizität ist dabei Mustererkennung in Reinform: Wer erkennt, dass drei Klassen bis auf einen Typ identisch sind, hat den Typparameter schon gefunden. Alle Aufgaben spielen im Adventure – nimm dir für jede Zeit, bevor du die Lösung aufklappst.

## Aufgabe 1 — Mustererkennung

Im Laufe der Entwicklung sind im Spiel die folgenden drei Klassen entstanden: eine für die Maße eines Levels, eine für ein Paar von Levelnamen und eine, die einem Feld das darauf stehende Objekt zuordnet. Lies sie aufmerksam und vergleiche sie.

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

class PositionObjektPaar
{
    public Position Erstes { get; }
    public Spielobjekt Zweites { get; }

    public PositionObjektPaar(Position erstes, Spielobjekt zweites)
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

Alle drei Klassen haben zwei schreibgeschützte Properties, einen Konstruktor, der beide setzt, und dieselbe `ToString`-Methode. Der einzige Unterschied ist der Typ der beiden Properties: `int`/`int`, `string`/`string` und `Position`/`Spielobjekt`. Genau die Stellen, an denen sich die Klassen unterscheiden, werden zu Typparametern.

**Schritt 2 — Warum zwei Typparameter:**

Bei `PositionObjektPaar` haben die beiden Werte unterschiedliche Typen. Mit einem einzigen `T` könnte man nur Paare gleicher Typen bilden. Also braucht die Klasse zwei Typparameter, `T1` und `T2`, die unabhängig voneinander belegt werden – wie `TKey` und `TValue` bei `Dictionary`.

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
var masse = new Paar<int, int>(20, 9);                       // Breite und Höhe des Kerkers
var levelpaar = new Paar<string, string>("Kerker", "Katakomben");
var belegung = new Paar<Position, Spielobjekt>(new Position(5, 2), new Wache(new Position(5, 2)));

Console.WriteLine(masse.Vertauscht());     // (9, 20)
Console.WriteLine(belegung);               // ((5, 2), Wache bei (5, 2))
Paar<Spielobjekt, Position> gedreht = belegung.Vertauscht();
```

**Zentrale Designentscheidungen:**

- **`Vertauscht()` gibt `Paar<T2, T1>` zurück, nicht `Paar<T1, T2>`:** Beim Vertauschen tauschen auch die Typen die Plätze. Bei `IntPaar` fiel das nicht auf, weil beide Typen gleich waren – erst die generische Version zwingt uns, diese Frage sauber zu beantworten. `PositionObjektPaar` hatte die Methode vermutlich deshalb nie bekommen.
- **`ToString` funktioniert ohne Constraint:** Die String-Interpolation ruft `ToString()` auf, und das hat jeder Typ, weil es von `object` geerbt wird. Für die Ausgabe des `Spielobjekt` greift dank `override` die polymorphe Variante, die `Beschreibung()` aufruft – deshalb steht dort „Wache bei (5, 2)“ und nicht der Klassenname.
- **Schreibgeschützte Properties:** Ein Paar ist ein Wert, der nach dem Erzeugen nicht mehr verändert wird. Wer ein anderes Paar will, erzeugt ein neues – wie `Vertauscht()` es tut. In .NET gibt es dieses Konzept fertig als Wertetupel `(T1, T2)`; genau das nutzt der `LevelParser` mit seiner `List<(char zeichen, Position pos)>`.

</details>

## Aufgabe 2 — Algorithmenentwurf

Immer wieder braucht das Spiel eine Teilmenge seiner Objekte: alle Gegner in Reichweite des Helden, alle Gegenstände, die noch auf dem Boden liegen, alle Türen, die noch verschlossen sind. Jedes Mal eine neue Methode `AlleGegnerInReichweite`, `AlleOffenenTueren` zu schreiben, ist offensichtlich keine gute Idee.

Entwirf eine **generische** Methode `Filtern<T>`, die aus einer `List<T>` alle Elemente heraussucht, die eine Bedingung erfüllen. Delegates und Lambdas kennen wir noch nicht – die Bedingung muss also auf einem anderen Weg an die Methode übergeben werden.

- Wie kann man „eine Bedingung“ als Objekt darstellen, das man einer Methode übergeben kann? Welches Konzept aus Vorlesung 02 hilft dabei?
- Welche Signatur hat `Filtern<T>`?
- Schreibe zwei konkrete Bedingungen für `Spielobjekt` und zeige die Nutzung.
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

Jede Bedingung ist eine eigene Klasse, die `IPruefer<Spielobjekt>` implementiert. Parameter wie der Bezugspunkt oder die Reichweite wandern in den Konstruktor:

```csharp
class InReichweite : IPruefer<Spielobjekt>
{
    private readonly Position bezug;
    private readonly int reichweite;

    public InReichweite(Position bezug, int reichweite)
    {
        this.bezug = bezug;
        this.reichweite = reichweite;
    }

    public bool Pruefen(Spielobjekt o) => o.Position.Entfernung(bezug) <= reichweite;
}

class SymbolIst : IPruefer<Spielobjekt>
{
    private readonly char symbol;
    public SymbolIst(char symbol) { this.symbol = symbol; }
    public bool Pruefen(Spielobjekt o) => o.Symbol == symbol;
}

Spielfeld feld = LevelParser.Parsen(new EingebauteLevelQuelle().Laden("Kerker"));
List<Spielobjekt> alle = new List<Spielobjekt>(feld.AlleObjekte);

List<Spielobjekt> nah = Filtern(alle, new InReichweite(feld.Spieler.Position, 5));
List<Spielobjekt> waende = Filtern(alle, new SymbolIst('#'));
Console.WriteLine(nah.Count);
Console.WriteLine(waende.Count);
```

**Zentrale Designentscheidungen:**

- **Generisches Interface statt `IPruefer` mit `object`:** `IPruefer<Spielobjekt>.Pruefen` bekommt ein `Spielobjekt` und kann direkt auf `Position` und `Symbol` zugreifen. Mit `object` müsste jede Bedingung erst casten – und `Filtern` könnte einen `IPruefer` für Strings mit einer Objektliste kombinieren, ohne dass der Compiler es merkt.
- **`Filtern<T>` weiß nichts über das Spiel:** Die Methode funktioniert genauso für `List<int>` mit einem `IPruefer<int>`. Der Algorithmus (durchlaufen, prüfen, sammeln) ist vom Elementtyp und von der Bedingung getrennt – das ist das Ziel.
- **Das Umständliche:** Für jede noch so kleine Bedingung braucht man eine ganze Klasse mit Konstruktor und Feldern. Der eigentliche Inhalt ist eine einzige Zeile (`o.Symbol == symbol`), umgeben von zehn Zeilen Verpackung. Genau dieses Problem lösen Delegates und Lambdas in Vorlesung 07: Dort wird aus `new SymbolIst('#')` ein `o => o.Symbol == '#'`, und `Filtern<T>` wird zu `Where` aus LINQ. Das Muster – Algorithmus generisch, Bedingung austauschbar – bleibt dasselbe.

</details>

## Aufgabe 3 — Fehler finden

Für die Schatzkammer des Spiels wurde eine generische Sammelklasse begonnen. Sie enthält **drei** Compilerfehler, die alle mit fehlenden oder falschen Constraints zu tun haben.

```csharp
class Schatzkammer<T>
{
    private readonly List<T> bestand = new();

    public void Einlagern(T stueck)
    {
        if (stueck == null)
        {
            throw new ArgumentNullException(nameof(stueck));
        }
        bestand.Add(stueck);
    }

    public T Wertvollstes()
    {
        T bestes = bestand[0];
        foreach (T stueck in bestand)
        {
            if (stueck.CompareTo(bestes) > 0)
            {
                bestes = stueck;
            }
        }
        return bestes;
    }

    public T Musterstueck()
    {
        return new T();
    }
}
```

- Finde die drei Stellen, die der Compiler ablehnt, und formuliere in eigenen Worten, warum.
- Welche Constraints beheben die Fehler? Reicht **ein** Constraint für alle drei?
- Diskutiere: Welche Typen kann `Schatzkammer<T>` nach deiner Korrektur noch aufnehmen – und ist das ein Problem für ein Spiel, in dem `Schatz` im Konstruktor eine `Position` und einen `Wert` braucht?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Die drei Fehler:**

1. `stueck == null`: Ein `==`-Vergleich mit `null` ist für einen uneingeschränkten Typparameter nicht erlaubt, weil `T` ein Werttyp wie `int` oder `Position` sein könnte, der nie `null` ist. (Genau genommen erlaubt der Compiler `== null` für uneingeschränkte `T` in neueren Versionen, wertet es für Werttypen aber immer als `false` aus – das ist dann kein Fehler, aber irreführend. Sauber wird es erst mit einem Constraint.)
2. `stueck.CompareTo(bestes)`: `T` hat keine Methode `CompareTo` – der Compiler kennt nur die Mitglieder von `object`.
3. `new T()`: Ohne Constraint weiß der Compiler nicht, ob `T` einen parameterlosen Konstruktor hat.

**Schritt 2 — Constraints hinzufügen:**

Ein Constraint reicht nicht, weil die drei Stellen drei verschiedene Fähigkeiten verlangen: `null`-Vergleich, Vergleichbarkeit und Erzeugbarkeit.

```csharp
class Schatzkammer<T> where T : class, IComparable<T>, new()
{
    // Rumpf unverändert
}
```

`class` macht den `null`-Vergleich eindeutig, `IComparable<T>` schaltet `CompareTo` frei, `new()` erlaubt `new T()`. Die Reihenfolge ist vorgeschrieben: `class` (oder eine Basisklasse) zuerst, dann Interfaces, `new()` am Ende.

**Schritt 3 — Was noch hineinpasst:**

Nach der Korrektur akzeptiert `Schatzkammer<T>` nur noch Referenztypen, die `IComparable<T>` implementieren und einen parameterlosen Konstruktor haben. `string` fällt heraus (kein parameterloser Konstruktor), `int` und `Position` fallen heraus (Werttypen), und ausgerechnet `Schatz` fällt heraus: Die Klasse ist weder vergleichbar noch lässt sie sich ohne `Position` und `Wert` erzeugen. Übrig bleibt fast nichts – ein deutliches Zeichen, dass die Klasse zu viel verlangt.

**Zentrale Designentscheidungen:**

- **Constraints sind ein Tauschgeschäft:** Jeder Constraint erlaubt der Klasse mehr und den Nutzern weniger. Drei Constraints auf einmal sind ein Warnsignal – vermutlich tut die Klasse zu viel.
- **`Musterstueck()` gehört nicht hierher:** Warum sollte eine Schatzkammer Schätze erzeugen können? Streicht man die Methode, entfällt `new()`, und die Klasse wird sofort brauchbarer.
- **`Wertvollstes()` ohne `IComparable`:** Für `Schatz` gibt es eine viel natürlichere Lösung, als die Klasse vergleichbar zu machen: eine Schleife über `bestand`, die `Wert` vergleicht. Sobald man aber „wertvollstes“ generisch für beliebige `T` formulieren will, braucht man entweder `IComparable<T>` oder – ab der nächsten Vorlesung – einen `IComparer<T>`, den der Aufrufer mitbringt.
- **Alternative zu `class`:** Wenn auch Werttypen erlaubt sein sollen, ersetzt man den `null`-Vergleich durch `where T : notnull` und lässt die Prüfung weg – der Compiler stellt dann sicher, dass niemand `Schatzkammer<string?>` schreibt.
- **`Wertvollstes()` bei leerem Bestand:** `bestand[0]` wirft eine `ArgumentOutOfRangeException`. Das ist kein Compilerfehler, aber ein Randfall, den eine gute Implementierung mit einer aussagekräftigen `InvalidOperationException` abfängt.

</details>

## Aufgabe 4 — Abstraktion

Nach jeder Runde liefert das `Spielfeld` in `LetzteMeldung` einen Satz wie „Held hebt Schlüssel auf. Wache erwischt dich!“. Die Oberfläche soll nicht nur die aktuelle, sondern die **letzten fünf** Meldungen anzeigen – alles Ältere darf verschwinden. Eine `List<string>`, in die man ewig anhängt, wächst dabei unbegrenzt; eine, aus der man vorne mit `RemoveAt(0)` löscht, verschiebt bei jedem Zug alle Elemente.

Die passende Datenstruktur heißt **Ringpuffer** (*Ring Buffer*): ein Array fester Größe, in das man reihum schreibt. Ist der Puffer voll, überschreibt jeder neue Wert den ältesten. Entwirf eine generische Klasse `Ringpuffer<T>` mit fester Kapazität.

- Welche Felder braucht die Klasse? Wie merkt man sich, wo die älteste Meldung und wo der nächste freie Platz steht?
- Welche Operationen gehören in die Schnittstelle? Mindestens: `Hinzufuegen(T)`, `Aeltestes()`, `Anzahl`, und ein Weg, alle Elemente in der Reihenfolge vom ältesten zum neuesten zu durchlaufen.
- Randfälle: Was passiert bei `Aeltestes()` auf einem leeren Puffer? Was, wenn die Kapazität 0 ist? Wie berechnet man den Index nach dem letzten Platz im Array?
- Braucht `Ringpuffer<T>` einen Constraint? Wofür könnte man denselben Puffer im Spiel sonst noch verwenden?

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

**Schritt 3 — Im Spiel verwendet:**

```csharp
var meldungen = new Ringpuffer<string>(3);
Spielfeld feld = LevelParser.Parsen(new EingebauteLevelQuelle().Laden("Kerker"));

feld.SpielerZieht(Richtung.Unten);
meldungen.Hinzufuegen(feld.LetzteMeldung);
feld.SpielerZieht(Richtung.Unten);
meldungen.Hinzufuegen(feld.LetzteMeldung);
feld.SpielerZieht(Richtung.Rechts);
meldungen.Hinzufuegen(feld.LetzteMeldung);
feld.SpielerZieht(Richtung.Rechts);
meldungen.Hinzufuegen(feld.LetzteMeldung);   // überschreibt die Meldung aus Runde 1

Console.WriteLine(meldungen.Anzahl);                          // 3
foreach (string m in meldungen.AlsArray())
{
    Console.WriteLine(m);                                     // Runde 2, 3, 4 – in dieser Reihenfolge
}
```

Nach dem vierten `Hinzufuegen` steht die jüngste Meldung physisch an Index 0 des Arrays, aber logisch ist sie die neueste – `start` zeigt jetzt auf Index 1. Im fertigen Spiel füllt man den Puffer nicht von Hand nach jedem Zug, sondern hängt sich an das Ereignis `RundeBeendet` des Spielfelds; wie das geht, lernen wir in Vorlesung 07.

**Zentrale Designentscheidungen:**

- **`start` und `anzahl` statt `start` und `ende`:** Mit zwei Indizes kann man „leer“ und „voll“ nicht unterscheiden – in beiden Fällen wäre `start == ende`. Der Zähler `anzahl` macht beide Zustände eindeutig.
- **Kein Constraint nötig:** Der Ringpuffer speichert, überschreibt und liefert Elemente, vergleicht sie aber nie und erzeugt keine. Er funktioniert daher mit jedem Typ – `string` für Meldungen, `Richtung` für die letzten Züge des Spielers, `Position` für eine Spur, die der Verfolger hinterlässt. Das ist ein gutes Zeichen: Je weniger Constraints eine Datenstruktur braucht, desto allgemeiner ist sie.
- **Kapazität 0 wird im Konstruktor abgelehnt:** Sonst würde `% daten.Length` zu einer `DivideByZeroException` führen – ein Fehler, den man lieber sofort und mit klarer Meldung sieht.
- **`AlsArray()` kopiert in logischer Reihenfolge:** Der Aufrufer sieht nie den internen Ring, sondern immer „ältestes zuerst“. Eleganter wäre es, `IEnumerable<T>` zu implementieren, so wie `Inventar<T>` es tut, damit `foreach` direkt funktioniert – wie man das ohne die Weiterreichung an eine Liste schreibt, sehen wir beim Iterator-Muster in Vorlesung 08.

</details>
