---
title: "Rückblick auf Programmierung 1"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Wer ein Haus baut, prüft vorher das Fundament. In Programmierung 2 bauen wir auf dem auf, was du in [Programmierung 1](https://www.erodner.de/prog-lecture/) gelernt hast – und zwar ohne es noch einmal zu erklären. Wenn wir in der nächsten Vorlesung über Vererbung sprechen, setzen wir voraus, dass du weißt, was ein Konstruktor ist, was `private` bedeutet und warum eine `List<int>` etwas anderes ist als ein `int[]`. Dieses Modul ist deshalb keine Vorlesung, sondern eine Checkliste: Geh sie ehrlich durch und lies die verlinkten Prog-1-Module dort nach, wo du zögerst. Es ist völlig normal, dass nach der Pause nicht mehr alles präsent ist – wichtig ist nur, dass du die Lücken in den ersten zwei Wochen schließt.

## Was wir voraussetzen

Die folgende Tabelle listet die Themen aus Programmierung 1, auf die wir uns ab sofort stützen. Die mittlere Spalte beschreibt, was du konkret können solltest; die rechte Spalte verweist auf die passenden Module der Prog-1-Webseite.

| Thema | Das solltest du können | Nachlesen |
| :--- | :--- | :--- |
| IDE und Debugging | Ein Projekt öffnen, bauen und starten; Haltepunkte setzen, schrittweise ausführen, Variablen im Debugger beobachten; Compilerfehler von Laufzeitfehlern unterscheiden | [Debugging-Strategien](https://www.erodner.de/prog-lecture/modules/debugging_strategien/debugging_strategien/), [Compilerfehler](https://www.erodner.de/prog-lecture/modules/compilerfehler/compilerfehler/), [Laufzeitfehler](https://www.erodner.de/prog-lecture/modules/laufzeitfehler/laufzeitfehler/) |
| Variablen und Datentypen | `int`, `double`, `bool`, `char`, `string` sicher einsetzen; implizite und explizite Konvertierung (`(int)`, `int.Parse`, `int.TryParse`) | [Variablen](https://www.erodner.de/prog-lecture/modules/variablen_intro/variablen_intro/), [Datentypen](https://www.erodner.de/prog-lecture/modules/datentypen/datentypen/), [Typkonvertierung](https://www.erodner.de/prog-lecture/modules/typkonvertierung/typkonvertierung/) |
| Wert- und Referenztypen | Erklären, warum zwei Variablen auf dasselbe Objekt zeigen können; Stack und Heap grob einordnen; `null` und `?` verstehen | [Werttypen und Referenztypen](https://www.erodner.de/prog-lecture/modules/werttypen_referenztypen/werttypen_referenztypen/), [Structs](https://www.erodner.de/prog-lecture/modules/structs/structs/), [Nullable](https://www.erodner.de/prog-lecture/modules/nullable/nullable/) |
| Operatoren | Arithmetik (inkl. `%` und Ganzzahldivision), Vergleiche, `&&`/`\|\|`/`!`, zusammengesetzte Zuweisungen und Rangfolge | [Arithmetische Operatoren](https://www.erodner.de/prog-lecture/modules/operatoren_arithmetik/operatoren_arithmetik/), [Logische Operatoren](https://www.erodner.de/prog-lecture/modules/operatoren_logik/operatoren_logik/), [Operatorrangfolge](https://www.erodner.de/prog-lecture/modules/operatorrangfolge/operatorrangfolge/) |
| Verzweigungen | `if`/`else if`/`else`, `switch` (auch mit Pattern Matching) und den ternären Operator `?:` lesen und schreiben | [if/else](https://www.erodner.de/prog-lecture/modules/if_else/if_else/), [switch](https://www.erodner.de/prog-lecture/modules/switch/switch/), [Ternärer Operator](https://www.erodner.de/prog-lecture/modules/ternary/ternary/) |
| Schleifen | `for`, `while`, `do-while`, `foreach` passend auswählen; `break` und `continue`; verschachtelte Schleifen über Arrays | [Schleifen](https://www.erodner.de/prog-lecture/modules/schleifen/schleifen/), [Arrays und Schleifen](https://www.erodner.de/prog-lecture/modules/arrays_schleifen/arrays_schleifen/) |
| Strings und Formatierung | `Length`, `Substring`, `Split`, `Contains`, `ToUpper`; Interpolation `$"…"` mit Formatangaben wie `{wert:F2}` | [Strings](https://www.erodner.de/prog-lecture/modules/strings/strings/), [String-Formatierung](https://www.erodner.de/prog-lecture/modules/string_formatierung/string_formatierung/) |
| Methoden und Überladung | Parameter, Rückgabewert, `static`; mehrere Methoden mit gleichem Namen und unterschiedlicher Signatur; Gültigkeitsbereich von Variablen | [Methoden](https://www.erodner.de/prog-lecture/modules/methoden/methoden/), [Überladung](https://www.erodner.de/prog-lecture/modules/methoden_overload/methoden_overload/), [Scope](https://www.erodner.de/prog-lecture/modules/scope/scope/) |
| Klassen | Felder, Properties (auch mit `private set`), Konstruktoren, Instanzmethoden, `this`, Sichtbarkeit `public`/`private`, `static` gegenüber Instanz | [Klassen](https://www.erodner.de/prog-lecture/modules/klassen/klassen/), [Properties](https://www.erodner.de/prog-lecture/modules/properties/properties/), [Konstruktoren](https://www.erodner.de/prog-lecture/modules/konstruktoren/konstruktoren/), [Instanzmethoden](https://www.erodner.de/prog-lecture/modules/instanzmethoden/instanzmethoden/), [this](https://www.erodner.de/prog-lecture/modules/this/this/), [Sichtbarkeit](https://www.erodner.de/prog-lecture/modules/sichtbarkeit/sichtbarkeit/) |
| Collections | `List<T>` mit `Add`, `Remove`, `Count`, Indexzugriff; `Dictionary<TKey, TValue>` mit `ContainsKey` und `TryGetValue`; Unterschied zu Arrays | [List](https://www.erodner.de/prog-lecture/modules/list/list/), [Dictionary](https://www.erodner.de/prog-lecture/modules/dictionary/dictionary/), [Collections](https://www.erodner.de/prog-lecture/modules/collections/collections/) |
| Ausnahmebehandlung | `try`/`catch`/`finally`; gängige Exception-Typen kennen; selbst `throw new ArgumentException(…)` einsetzen | [try/catch](https://www.erodner.de/prog-lecture/modules/try_catch/try_catch/), [Exceptions](https://www.erodner.de/prog-lecture/modules/exceptions/exceptions/), [Eigene Exceptions](https://www.erodner.de/prog-lecture/modules/custom_exceptions/custom_exceptions/) |

Es geht nicht darum, jedes Detail auswendig zu wissen. Entscheidend ist, dass du Code mit diesen Konstrukten **flüssig lesen** kannst und beim Schreiben weißt, wo du nachschlagen musst. Wenn du in einer Zeile mehr als eine Sache nicht verstehst, ist das ein Signal, das Modul noch einmal durchzuarbeiten.
{: .notice--primary}

## Selbsttest

Der folgende Selbsttest ist bewusst kein neues Konzept, sondern eine gewöhnliche Klasse, wie du sie am Ende von Programmierung 1 hättest schreiben können. Sie modelliert eine Person, die Schritte zählt – ein Fitness-Tracker im Kleinformat.

```csharp
public class Person
{
    public Person(string name, DateTime geburtstag)
    {
        this.Name = name;
        this.Geburtstag = geburtstag;
    }

    public string Name { get; private set; }
    public DateTime Geburtstag { get; private set; }
    public float Gewicht { get; set; }
    public int Schritte { get; private set; }

    public float GelaufeneKm
    {
        get
        {
            // ein Schritt sind ungefaehr 75 cm
            return this.Schritte * 0.75F / 1000.0F;
        }
    }

    public void Gehen(int schritte)
    {
        if (schritte > -1)
            this.Schritte += schritte;
    }
}
```

So sieht die Klasse in der Benutzung aus. Überlege bei jeder Zeile, ob sie kompiliert – und wenn nicht, warum:

```csharp
Person anna = new Person("Anna", new DateTime(2004, 5, 17));
anna.Gehen(4000);
anna.Gehen(2000);
anna.Gewicht = 62.5F;
Console.WriteLine(anna.GelaufeneKm);   // 4.5
Console.WriteLine(anna.Name);          // Anna

anna.Schritte = 0;                     // kompiliert nicht
anna.GelaufeneKm = 10;                 // kompiliert nicht
```

Die beiden letzten Zeilen scheitern aus unterschiedlichen Gründen: `Schritte` hat einen Setter, aber er ist `private` – nur die Klasse selbst darf Schritte hinzufügen, und zwar ausschließlich über `Gehen`. `GelaufeneKm` hat gar keinen Setter; die Property wird bei jedem Zugriff aus `Schritte` **berechnet** und speichert selbst keinen Wert.

Übung: Beantworte die folgenden Fragen schriftlich, bevor du im Workshop mit anderen vergleichst. (1) Welche Bestandteile hat die Klasse? Benenne jeden einzeln: Konstruktor, gewöhnliche Properties, berechnete Property, Methode. (2) Welche Sichtbarkeiten kommen vor? Warum sind `Name`, `Geburtstag` und `Schritte` mit `private set` deklariert, `Gewicht` aber nicht? (3) Was unterscheidet `GelaufeneKm` von `Schritte`? Was würde sich ändern, wenn `GelaufeneKm` ein Feld wäre, das im Konstruktor gesetzt wird? (4) Was macht `this` im Konstruktor – und ist es hier notwendig? (5) Was passiert bei `anna.Gehen(-500)`? Ist das eine gute Entscheidung, oder wäre eine Exception besser? Argumentiere. (6) Welche weitere berechnete Property wäre sinnvoll? Skizziere `Alter` in Jahren aus `Geburtstag` und `DateTime.Today`.
{: .notice--info}

Wenn du bei Frage 2 oder 3 unsicher bist, lies die Module [Properties](https://www.erodner.de/prog-lecture/modules/properties/properties/) und [Sichtbarkeit](https://www.erodner.de/prog-lecture/modules/sichtbarkeit/sichtbarkeit/) noch einmal – genau diese Konzepte werden in der [nächsten Vorlesung](/lectures/01/01.md) mit `protected` und `virtual` erweitert.
{: .notice--warning}

## Weitere Quellen

- [Programmierung 1 – Kurswebseite](https://www.erodner.de/prog-lecture/)
- [Einführung in C# – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/tour-of-csharp/)
- [Eigenschaften (Properties) – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/classes-and-structs/properties)
