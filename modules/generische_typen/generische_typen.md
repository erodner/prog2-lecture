---
title: "Generische Typen"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Auf einem aufgeräumten Schreibtisch liegt für jede Art von Dokument ein eigener Ablagestapel: einer für Rechnungen, einer für Briefe, einer für Notizen. Alle Stapel verhalten sich gleich – man legt etwas obenauf, und was zuletzt draufgelegt wurde, liegt oben und wird als Erstes wieder heruntergenommen. Der einzige Unterschied ist, **was** auf den Stapel darf. Niemand würde für jede Dokumentart eine eigene Stapelklasse konstruieren; man legt einen Stapel an und beschriftet ihn. Genau das leisten **generische Klassen**: Das Verhalten wird einmal implementiert, und der Elementtyp wird erst beim Erzeugen des Objekts festgelegt. Im Modul [Generische Methoden](/modules/generische_methoden/generische_methoden.md) haben wir den Typparameter `T` an einzelnen Methoden kennengelernt – jetzt heben wir ihn auf die Ebene der ganzen Klasse und bauen damit das Inventar unseres Helden.

## Der Ablagestapel ohne Generics

Beginnen wir mit dem, was ohne Generics möglich wäre. Ein Stapel, auf den man alles legen kann, speichert seine Elemente als `object`:

```csharp
class ObjektStapel
{
    private object[] inhalt;
    private int anzahl;

    public ObjektStapel(int kapazitaet)
    {
        inhalt = new object[kapazitaet];
    }

    public void Push(object element)
    {
        inhalt[anzahl] = element;
        anzahl++;
    }

    public object Pop()
    {
        anzahl--;
        return inhalt[anzahl];
    }
}
```

Das funktioniert, hat aber genau die Schwächen, die wir bei `object` schon kennen. Der Stapel weiß nicht, was auf ihm liegt, und der Aufrufer muss beim Herunternehmen raten:

```csharp
ObjektStapel rechnungen = new ObjektStapel(10);
rechnungen.Push("Stromrechnung");
rechnungen.Push(42);                       // kompiliert – 42 ist auch ein object
string oben = (string)rechnungen.Pop();    // InvalidCastException zur Laufzeit!
```

Der Stapel für Rechnungen nimmt klaglos eine nackte Zahl an, und der Cast beim Herunternehmen fliegt uns erst zur Laufzeit um die Ohren. Wir wollen, dass der Compiler den Fehler in der Zeile `rechnungen.Push(42)` meldet – und nicht der Kunde beim Ausführen.

## Der Ablagestapel mit Typparameter

Bei einer generischen Klasse steht der Typparameter direkt hinter dem Klassennamen. Innerhalb der Klasse verwenden wir `T` überall dort, wo vorher `object` stand – für das Array, den Parameter von `Push` und den Rückgabetyp von `Pop`:

```csharp
class Ablagestapel<T>
{
    private T[] inhalt;
    private int anzahl;

    public Ablagestapel(int kapazitaet)
    {
        inhalt = new T[kapazitaet];
    }

    public int Anzahl => anzahl;

    public void Push(T element)
    {
        if (anzahl == inhalt.Length)
        {
            throw new InvalidOperationException("Der Stapel ist voll.");
        }
        inhalt[anzahl] = element;
        anzahl++;
    }

    public T Pop()
    {
        if (anzahl == 0)
        {
            throw new InvalidOperationException("Der Stapel ist leer.");
        }
        anzahl--;
        return inhalt[anzahl];
    }
}
```

Der Rumpf ist fast identisch mit der `object`-Version – nur das Wort `object` ist durch `T` ersetzt. Zusätzlich haben wir die beiden Randfälle abgefangen, die in einer naiven Version gern vergessen werden. Beim Erzeugen eines Objekts geben wir jetzt an, wofür der Stapel gedacht ist:

```csharp
Ablagestapel<string> notizen = new Ablagestapel<string>(10);
notizen.Push("Zahnarzt anrufen");
notizen.Push("Milch kaufen");
string oben = notizen.Pop();     // "Milch kaufen" – kein Cast nötig
Console.WriteLine(oben);         // Milch kaufen

notizen.Push(42);                // Compilerfehler CS1503:
                                 // Argument 1: Konvertierung von "int" in "string" nicht möglich.
```

Der Compiler hat `T` durch `string` ersetzt und prüft jeden Aufruf von `Push` und `Pop` mit diesem Wissen. Die Zahl wird sofort zurückgewiesen, und der Rückgabewert von `Pop` ist ein `string`, ohne dass wir etwas casten müssten.

Der Typ heißt vollständig `Ablagestapel<string>`. `Ablagestapel<string>` und `Ablagestapel<int>` sind zwei **verschiedene Typen** ohne Zuweisungsbeziehung – so wenig, wie man den Rechnungsstapel als Briefstapel verwenden kann.
{: .notice--primary}

## Das Inventar des Helden

Damit können wir die erste eigene generische Klasse des Adventures schreiben. Der Held hebt Schlüssel und Schätze auf und trägt sie mit sich herum; wir brauchen also einen Behälter. Aus [Vorlesung 02](/lectures/02/02.md) wissen wir, dass alles Aufhebbare das Interface `ISammelbar` implementiert – der Behälter selbst bleibt aber offen für mehr als Gegenstände, denn später sollen vielleicht auch Zaubersprüche oder Aufträge gesammelt werden. Genau diese Unterscheidung drückt ein Typparameter aus:

```csharp
public class Inventar<T> : IEnumerable<T> where T : ISammelbar
{
    private readonly List<T> inhalt = new();

    public int Anzahl => inhalt.Count;

    public void Hinzufuegen(T ding)
    {
        inhalt.Add(ding);
    }

    public IEnumerator<T> GetEnumerator() => inhalt.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString()
    {
        return inhalt.Count == 0 ? "leer" : string.Join(", ", inhalt.Select(d => d.Name));
    }
}
```

Zwei Dinge stechen heraus. Der Zusatz `where T : ISammelbar` ist ein **Constraint**: Er verlangt, dass für `T` nur Typen eingesetzt werden, die `ISammelbar` implementieren – nur deshalb darf `ToString` auf `d.Name` zugreifen. Warum das nötig ist und welche Constraints es sonst noch gibt, klärt das Modul [Generische Constraints](/modules/generische_constraints/generische_constraints.md). Und `Inventar<T>` implementiert selbst ein generisches Interface, `IEnumerable<T>`, indem es das Aufzählen an die interne Liste weiterreicht. Das kostet zwei Zeilen und bringt sehr viel: Jedes Inventar lässt sich mit `foreach` durchlaufen.

Der `Spieler` legt sein Inventar direkt bei der Deklaration an und belegt `T` mit `Gegenstand`:

```csharp
public class Spieler : BeweglichesObjekt
{
    public int Lebenspunkte { get; private set; } = MaxLebenspunkte;
    public int Punkte { get; private set; }
    public Inventar<Gegenstand> Inventar { get; } = new();

    public override string Beschreibung()
    {
        return $"{Name} bei {Position}, {Lebenspunkte}/{MaxLebenspunkte} Lebenspunkte, " +
               $"{Punkte} Punkte, Inventar: {Inventar}";
    }
}
```

Ab hier arbeitet der Compiler für uns. Wenn der Spieler im `Spielfeld` über ein Feld läuft, auf dem etwas liegt, landet der Fund im Inventar – aber nur, wenn er wirklich ein `Gegenstand` ist:

```csharp
if (davor is Gegenstand gegenstand)
{
    meldung.Append(gegenstand.Aufheben(Spieler));
    statische.Remove(gegenstand.Position);
    if (gegenstand is not Trank) Spieler.Inventar.Hinzufuegen(gegenstand);
}
```

`Spieler.Inventar.Hinzufuegen(new Wand(...))` wäre ein Compilerfehler, denn eine `Wand` ist kein `Gegenstand` – der Fehler entsteht beim Tippen, nicht beim Spielen. Ein Trank wird übrigens sofort getrunken und deshalb gar nicht erst eingesteckt. Und weil `Inventar<T>` ein `IEnumerable<T>` ist, kann jede Anzeige einfach über den Inhalt laufen:

```csharp
foreach (Gegenstand g in held.Inventar)
{
    Console.WriteLine($"- {g.Name}");   // g ist ein Gegenstand, kein object
}
```

Die Laufvariable hat den Typ `Gegenstand` – ohne Cast, ohne Prüfung. Mit einer `List<object>` stünde hier `object g` und jeder Zugriff auf `g.Name` bräuchte erst einen Cast.

Übung: Erweitere `Ablagestapel<T>` um eine Methode `Clear()`, die alle Elemente entfernt, und eine Methode `Peek()`, die das oberste Element zurückgibt, ohne es zu entfernen. Überlege, ob `Clear()` das Array wirklich leeren muss oder ob es reicht, `anzahl` zurückzusetzen – und was das für Referenztypen und die Garbage Collection bedeutet.
{: .notice--info}

## Das gibt es schon: `Stack<T>`

Das Verhalten „Was zuletzt hinein kam, kommt zuerst heraus“ heißt **LIFO** (*Last In, First Out*) und ist eine der grundlegenden Datenstrukturen der Informatik: ein **Stack** (Stapel). .NET bringt ihn als `Stack<T>` fertig mit – mit exakt den Methoden, die wir gerade selbst geschrieben haben:

```csharp
Stack<Richtung> letzteZuege = new Stack<Richtung>();
letzteZuege.Push(Richtung.Rechts);
letzteZuege.Push(Richtung.Oben);
Console.WriteLine(letzteZuege.Peek());  // Oben
Console.WriteLine(letzteZuege.Pop());   // Oben
Console.WriteLine(letzteZuege.Count);   // 1
```

Der Unterschied zu unserem `Ablagestapel<T>`: `Stack<T>` wächst automatisch, wenn er voll wird, so wie wir es von `List<T>` kennen. Den eigenen Stapel zu schreiben war trotzdem nicht umsonst – wir verstehen jetzt, wie die Klassen in .NET aufgebaut sind, die wir seit Programmierung 1 benutzen. Und genau deshalb speichert `Inventar<T>` seinen Inhalt auch in einer `List<T>`, statt ein Array von Hand zu verwalten: Die Arbeit ist schon gemacht.

## Generische Klassen in .NET

Mit diesem Wissen lesen sich die Collections aus dem Modul [Collections](https://www.erodner.de/prog-lecture/modules/collections/collections/) plötzlich anders. Jede davon ist eine generische Klasse, deren Typparameter wir beim Erzeugen belegen:

| Typ | Bedeutung des Typparameters | Beispiel im Adventure |
| :--- | :--- | :--- |
| `List<T>` | Elementtyp der Liste | `List<Gegner>` im `Spielfeld` |
| `Stack<T>` | Elementtyp des Stapels (LIFO) | `Stack<Richtung>` für eine Rückgängig-Funktion |
| `Queue<T>` | Elementtyp der Warteschlange (FIFO) | `Queue<Richtung>` für geplante Züge |
| `Dictionary<TKey, TValue>` | Schlüssel- und Werttyp | `Dictionary<Position, StatischesObjekt>` im `Spielfeld` |
| `HashSet<T>` | Elementtyp der Menge ohne Duplikate | `HashSet<Position>` für bereits besuchte Felder |
| `Nullable<T>` | Werttyp, der zusätzlich `null` sein darf | `Richtung?` als Rückgabe von `NaechsterZug` |

Die letzte Zeile ist ein schönes Beispiel dafür, wie tief Generics in der Sprache stecken: Das `Richtung?` aus `public abstract Richtung? NaechsterZug(Spielfeld feld)` ist nur eine Kurzschreibweise für die generische Struktur `Nullable<Richtung>` – ein Gegner, der in dieser Runde stehen bleibt, liefert `null`. Auch `IReadOnlyList<Gegner>`, das `Spielfeld.Gegner` nach außen gibt, ist ein generisches Interface.

## Was Generics wirklich leisten

Generische Typen sind keine Laufzeit-Magie, sondern **Typsicherheit zur Kompilierzeit**. Der Compiler kennt für jedes `Inventar<Gegenstand>`-Objekt den Elementtyp und prüft alle Aufrufe damit. Fehler, die in der `object`-Version als `InvalidCastException` mitten im Spiel auftauchen, werden zu roten Wellenlinien in der IDE. Gleichzeitig entfallen Casts und Boxing, sodass generischer Code für Werttypen wie `int` sogar schneller ist als die `object`-Variante.

Im Zweifel: Wann immer du eine Klasse mit `object`-Feldern schreibst oder dieselbe Klasse für mehrere Elementtypen kopierst, ist ein Typparameter die bessere Lösung.
{: .notice--primary}

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v02-interfaces`).

## Weitere Quellen

- [Generische Klassen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/generics/generic-classes)
- [Stack&lt;T&gt;-Klasse – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.collections.generic.stack-1)
- [Generische Sammlungen in .NET – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/generics/collections)
- [Listen und Stapel visualisiert – VisuAlgo](https://visualgo.net/en/list) – zeigt Schritt für Schritt, was `Push`, `Pop` und das Wachsen eines Arrays tatsächlich im Speicher anstellen
- [Generics – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/types/generics) – der kompakte Einstiegsartikel mit allen Sprachdetails auf einer Seite
