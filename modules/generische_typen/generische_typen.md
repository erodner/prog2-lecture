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

Auf einem aufgeräumten Schreibtisch liegt für jede Art von Dokument ein eigener Ablagestapel: einer für Rechnungen, einer für Briefe, einer für Notizen. Alle Stapel verhalten sich gleich – man legt etwas obenauf, und was zuletzt draufgelegt wurde, liegt oben und wird als Erstes wieder heruntergenommen. Der einzige Unterschied ist, **was** auf den Stapel darf. Niemand würde für jede Dokumentart eine eigene Stapelklasse konstruieren; man legt einen Stapel an und beschriftet ihn. Genau das leisten **generische Klassen**: Das Verhalten wird einmal implementiert, und der Elementtyp wird erst beim Erzeugen des Objekts festgelegt. Im Modul [Generische Methoden](/modules/generische_methoden/generische_methoden.md) haben wir den Typparameter `T` an einzelnen Methoden kennengelernt – jetzt heben wir ihn auf die Ebene der ganzen Klasse.

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

Der Rumpf ist fast identisch mit der `object`-Version – nur das Wort `object` ist durch `T` ersetzt. Zusätzlich haben wir die beiden Randfälle abgefangen, die in einer naiven Version gern vergessen werden: Ein voller Stapel darf nichts mehr annehmen, und von einem leeren kann man nichts herunternehmen. Beim Erzeugen eines Objekts geben wir jetzt an, für welche Art von Dokument der Stapel gedacht ist:

```csharp
Ablagestapel<string> notizen = new Ablagestapel<string>(10);
notizen.Push("Zahnarzt anrufen");
notizen.Push("Milch kaufen");
string oben = notizen.Pop();     // "Milch kaufen" – kein Cast nötig
Console.WriteLine(oben);         // Milch kaufen

notizen.Push(42);                // Compilerfehler CS1503:
                                 // Argument 1: Konvertierung von "int" in "string" nicht möglich.
```

Der Stapel `notizen` ist ein `Ablagestapel<string>` – der Compiler hat `T` durch `string` ersetzt und prüft jeden Aufruf von `Push` und `Pop` mit diesem Wissen. Die Zahl wird sofort zurückgewiesen, und der Rückgabewert von `Pop` ist ein `string`, ohne dass wir etwas casten müssten.

Der Typ heißt vollständig `Ablagestapel<string>`. `Ablagestapel<string>` und `Ablagestapel<int>` sind zwei **verschiedene Typen** ohne Zuweisungsbeziehung – so wenig, wie man den Rechnungsstapel als Briefstapel verwenden kann.
{: .notice--primary}

## Ein Stapel für Figuren

Der Elementtyp muss kein eingebauter Typ sein. Ein Stapel für die Figuren des Geometrieeditors nimmt alle Objekte an, die den Typ `Figur` haben – wegen der Vererbung also auch `Kreis` und `Rechteck`:

```csharp
Ablagestapel<Figur> figuren = new Ablagestapel<Figur>(5);
figuren.Push(new Kreis("K1", 0, 0, 1.5));
figuren.Push(new Rechteck("R1", 2, 2, 3, 4));

Figur oben = figuren.Pop();
Console.WriteLine(oben.Beschreibung()); // R1 bei (2, 2) mit Fläche 12,00 (3 x 4)
Console.WriteLine(figuren.Anzahl);      // 1
```

`Pop` liefert eine `Figur` zurück, und über den polymorphen Aufruf von `Beschreibung()` kommt die Variante des Laufzeittyps `Rechteck` zum Zug – Generizität und Vererbung ergänzen sich also. Das `var`-Schlüsselwort und die zieltypisierte `new()`-Syntax machen den Code kürzer, ohne die Typsicherheit aufzugeben:

```csharp
var briefe = new Ablagestapel<string>(20);
Ablagestapel<Figur> figuren = new(5);
```

Übung: Erweitere `Ablagestapel<T>` um eine Methode `Clear()`, die alle Elemente entfernt, und eine Methode `Peek()`, die das oberste Element zurückgibt, ohne es zu entfernen. Überlege, ob `Clear()` das Array wirklich leeren muss oder ob es reicht, `anzahl` zurückzusetzen – und was das für Referenztypen und die Garbage Collection bedeutet.
{: .notice--info}

## Das gibt es schon: `Stack<T>`

Das Verhalten „Was zuletzt hinein kam, kommt zuerst heraus“ heißt **LIFO** (*Last In, First Out*) und ist eine der grundlegenden Datenstrukturen der Informatik: ein **Stack** (Stapel). .NET bringt ihn als `Stack<T>` fertig mit – mit exakt den Methoden, die wir gerade selbst geschrieben haben:

```csharp
Stack<string> briefe = new Stack<string>();
briefe.Push("Brief vom Finanzamt");
briefe.Push("Postkarte aus Rom");
Console.WriteLine(briefe.Peek());  // Postkarte aus Rom
Console.WriteLine(briefe.Pop());   // Postkarte aus Rom
Console.WriteLine(briefe.Count);   // 1
```

Der Unterschied zu unserem `Ablagestapel<T>`: `Stack<T>` wächst automatisch, wenn er voll wird, so wie wir es von `List<T>` kennen. Den eigenen Stapel zu schreiben war trotzdem nicht umsonst – wir verstehen jetzt, wie die Klassen in .NET aufgebaut sind, die wir seit Programmierung 1 benutzen.

## Generische Klassen in .NET

Mit diesem Wissen lesen sich die Collections aus dem Modul [Collections](https://www.erodner.de/prog-lecture/modules/collections/collections/) plötzlich anders. Jede davon ist eine generische Klasse, deren Typparameter wir beim Erzeugen belegen:

| Typ | Bedeutung des Typparameters | Beispiel |
| :--- | :--- | :--- |
| `List<T>` | Elementtyp der Liste | `List<Figur>` in der `FigurenVerwaltung` |
| `Stack<T>` | Elementtyp des Stapels (LIFO) | `Stack<string>` |
| `Queue<T>` | Elementtyp der Warteschlange (FIFO) | `Queue<Auftrag>` |
| `Dictionary<TKey, TValue>` | Schlüssel- und Werttyp | `Dictionary<string, Figur>` |
| `HashSet<T>` | Elementtyp der Menge ohne Duplikate | `HashSet<int>` |
| `Nullable<T>` | Werttyp, der zusätzlich `null` sein darf | `Nullable<int>`, kurz `int?` |

Die letzte Zeile ist ein schönes Beispiel dafür, wie tief Generics in der Sprache stecken: Das `int?` aus dem Modul [Nullable](https://www.erodner.de/prog-lecture/modules/nullable/nullable/) ist nur eine Kurzschreibweise für die generische Struktur `Nullable<int>`. Auch `IReadOnlyList<Figur>`, das die `FigurenVerwaltung` als Typ für `AlleFiguren` zurückgibt, ist ein generisches Interface.

## Was Generics wirklich leisten

Generische Typen sind keine Laufzeit-Magie, sondern **Typsicherheit zur Kompilierzeit**. Der Compiler kennt für jedes `Ablagestapel<string>`-Objekt den Elementtyp und prüft alle Aufrufe damit. Fehler, die in der `object`-Version als `InvalidCastException` beim Kunden auftauchen, werden zu roten Wellenlinien in der IDE. Gleichzeitig entfallen Casts und Boxing, sodass generischer Code für Werttypen wie `int` sogar schneller ist als die `object`-Variante.

Im Zweifel: Wann immer du eine Klasse mit `object`-Feldern schreibst oder dieselbe Klasse für mehrere Elementtypen kopierst, ist ein Typparameter die bessere Lösung.
{: .notice--primary}

## Weitere Quellen

- [Generische Klassen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/generics/generic-classes)
- [Stack&lt;T&gt;-Klasse – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.collections.generic.stack-1)
- [Generische Sammlungen in .NET – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/generics/collections)
