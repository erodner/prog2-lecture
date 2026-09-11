---
title: "🧩 Aufgaben und Beispiele: Einstieg"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Programmieren lernt man nicht nur durch Codezeilen tippen – sondern auch durch **Nachdenken**. Die folgenden Aufgaben trainieren *Computational Thinking*: die Fähigkeit, Probleme so zu strukturieren, dass ein Computer sie lösen kann. Dazu gehören Abstraktion, Zerlegung, Mustererkennung und Algorithmenentwurf. Alle vier Aufgaben kommen mit dem Stoff aus Programmierung 1 aus – sie zeigen dir, wie sicher du darin bist. Nimm dir für jede Aufgabe Zeit, bevor du die Lösung aufklappst.

## Aufgabe 1 — Mustererkennung

Lies die folgende Klasse und sage die Ausgabe des Programms **ohne Ausführen** voraus. Achte besonders auf `static`, auf die Reihenfolge der Anweisungen im Konstruktor und auf Referenzen.

```csharp
class Sensor
{
    private static int anzahl = 0;
    private int messwert;

    public string Bezeichnung { get; }
    public static int Anzahl => anzahl;

    public int Messwert
    {
        get { return messwert; }
        set
        {
            if (value < -50 || value > 150)
                throw new ArgumentOutOfRangeException(nameof(value));
            messwert = value;
        }
    }

    public Sensor(string bezeichnung, int startwert)
    {
        anzahl++;
        Bezeichnung = $"{bezeichnung}-{anzahl}";
        Messwert = startwert;
    }

    public string Beschreibung() => $"{Bezeichnung}: {Messwert} Grad";
}
```

```csharp
Sensor s1 = new Sensor("Keller", 12);
Sensor s2 = new Sensor("Dach", 30);
Console.WriteLine(s1.Beschreibung());
Console.WriteLine(s2.Beschreibung());
Console.WriteLine(Sensor.Anzahl);

Sensor s3 = s1;
s3.Messwert = 99;
Console.WriteLine(s1.Beschreibung());

try
{
    Sensor s4 = new Sensor("Ofen", 200);
}
catch (ArgumentOutOfRangeException)
{
    Console.WriteLine("Anlegen fehlgeschlagen");
}
Console.WriteLine(Sensor.Anzahl);
```

- Was gibt jede `Console.WriteLine`-Zeile aus?
- Wie viele `Sensor`-Objekte existieren am Ende – und was sagt `Anzahl`?
- Wo liegt der Konstruktionsfehler der Klasse, und wie behebst du ihn?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Die ersten drei Ausgaben:**

```
Keller-1: 12 Grad
Dach-2: 30 Grad
2
```

`anzahl` ist `static`, gehört also zur Klasse und nicht zum Objekt. Jeder Konstruktoraufruf erhöht denselben Zähler, deshalb erhält der zweite Sensor die `2` im Namen.

**Schritt 2 — Referenzen:**

```
Keller-1: 99 Grad
```

`s3 = s1` kopiert keine Daten, sondern die Referenz – beide Variablen zeigen auf dasselbe Objekt. Die Änderung über `s3` ist deshalb über `s1` sichtbar. Das ist das Verhalten von Referenztypen aus [Wert- und Referenztypen](https://www.erodner.de/prog-lecture/modules/werttypen_referenztypen/werttypen_referenztypen/).

**Schritt 3 — Die Exception:**

```
Anlegen fehlgeschlagen
3
```

Der Setter von `Messwert` wirft bei `200` eine Exception, und das Objekt `s4` entsteht nie. Aber `anzahl++` stand **vor** der Zuweisung an `Messwert` – der Zähler wurde bereits erhöht. Es existieren zwei Sensoren, `Anzahl` behauptet drei.

**Zentrale Designentscheidungen:**

- **Erst prüfen, dann Zustand ändern:** Alle Validierungen gehören an den Anfang des Konstruktors, bevor irgendetwas – auch ein statisches Feld – verändert wird. Die einfachste Korrektur: die Zeile `Messwert = startwert;` vor `anzahl++` ziehen.
- **`Anzahl` als Expression-bodied Property:** Der Zähler ist von außen lesbar, aber nicht änderbar – dasselbe Prinzip wie `private set`.
- **`Bezeichnung { get; }`** ohne Setter kann nur im Konstruktor gesetzt werden. So bleibt der Name über die Lebensdauer des Objekts stabil.

</details>

## Aufgabe 2 — Zerlegung

Ein Konsolenprogramm „Notenverwaltung“ ist als ein einziger Block aus Top-Level-Statements geschrieben. Es hält drei parallele Listen – `List<string> namen`, `List<int> matrikelnummern`, `List<List<double>> noten` – und enthält Schleifen, die (a) Studierende mit Noten einlesen, (b) den Durchschnitt pro Person berechnen, (c) die Person mit dem besten Durchschnitt ausgeben und (d) alle auflisten, deren Durchschnitt schlechter als 4,0 ist. Alles steht in einer Datei mit rund 120 Zeilen.

Zerlege dieses Programm in Klassen:
- Welche Klassen brauchst du, und welche **eine** Verantwortung hat jede?
- Welche Properties und Methoden gehören wohin? Wo liegt die Berechnung des Durchschnitts?
- Was bleibt in `Program.cs` übrig?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Verantwortungen finden:**

Die parallelen Listen sind das Hauptproblem: Name, Matrikelnummer und Noten gehören zusammen, liegen aber getrennt. Alles, was *eine Person* betrifft, wandert in `Student`. Alles, was *die Menge aller Studierenden* betrifft – suchen, filtern, den besten finden – wandert in `Notenverwaltung`. Ein- und Ausgabe bleibt in `Program.cs`.

**Schritt 2 — Klasse `Student`:**

```csharp
class Student
{
    private List<double> noten = new List<double>();

    public string Name { get; }
    public int Matrikelnummer { get; }
    public IReadOnlyList<double> Noten => noten;

    public Student(string name, int matrikelnummer)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name darf nicht leer sein.", nameof(name));
        Name = name;
        Matrikelnummer = matrikelnummer;
    }

    public void NoteEintragen(double note)
    {
        if (note < 1.0 || note > 5.0)
            throw new ArgumentOutOfRangeException(nameof(note));
        noten.Add(note);
    }

    public double Durchschnitt()
    {
        if (noten.Count == 0)
            throw new InvalidOperationException("Keine Noten vorhanden.");
        double summe = 0;
        foreach (double n in noten)
            summe += n;
        return summe / noten.Count;
    }
}
```

**Schritt 3 — Klasse `Notenverwaltung`:**

```csharp
class Notenverwaltung
{
    private List<Student> studierende = new List<Student>();

    public void Hinzufuegen(Student student) => studierende.Add(student);

    public Student Beste()
    {
        if (studierende.Count == 0)
            throw new InvalidOperationException("Keine Studierenden vorhanden.");
        Student beste = studierende[0];
        foreach (Student s in studierende)
            if (s.Durchschnitt() < beste.Durchschnitt())
                beste = s;
        return beste;
    }

    public List<Student> Durchgefallene()
    {
        List<Student> ergebnis = new List<Student>();
        foreach (Student s in studierende)
            if (s.Durchschnitt() > 4.0)
                ergebnis.Add(s);
        return ergebnis;
    }
}
```

`Program.cs` erzeugt nur noch eine `Notenverwaltung`, liest Eingaben, ruft `NoteEintragen` und `Hinzufuegen` auf und gibt `Beste()` und `Durchgefallene()` aus – zwanzig Zeilen statt hundertzwanzig.

**Zentrale Designentscheidungen:**

- **Der Durchschnitt gehört zu `Student`:** Er hängt nur von den Daten eines einzelnen Studierenden ab. `Notenverwaltung` nutzt ihn, berechnet ihn aber nicht selbst.
- **`Noten` als `IReadOnlyList<double>`:** Von außen kann man Noten lesen, aber nur über `NoteEintragen` – mit Validierung – hinzufügen.
- **Keine `Console`-Aufrufe in den Klassen:** Die Fachlogik weiß nichts von der Konsole. Genau diese Trennung ermöglicht in [Vorlesung 04](/lectures/04/04.md), dieselben Klassen hinter einer GUI zu verwenden.

</details>

## Aufgabe 3 — Algorithmenentwurf

Schreibe eine Methode `int Zweitgroesste(List<int> zahlen)`, die die zweitgrößte **verschiedene** Zahl liefert, ohne die Liste zu sortieren. Für `[3, 7, 7, 5]` ist das Ergebnis `5`, für `[5, 5, 3]` ist es `3`. Bei weniger als zwei Elementen und wenn alle Elemente gleich sind (`[5, 5]`), soll eine `ArgumentException` fliegen.

- Welche Zwischenwerte musst du dir beim Durchlaufen merken?
- Warum reicht ein einfaches `int` für „bisher zweitgrößte“ nicht aus? Denke an `[-3, -1]`.
- Prüfe deinen Entwurf gegen `[1, 2]`, `[2, 1]`, `[7, 7, 3]` und `[4]`, bevor du ihn aufschreibst.

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Idee:**

Ein einziger Durchlauf genügt: Wir merken uns die bisher größte Zahl und die bisher zweitgrößte. Jede neue Zahl kann die größte verdrängen (dann rutscht die alte größte auf Platz zwei), nur die zweitgrößte verdrängen, oder gar nichts ändern.

**Schritt 2 — Warum `int?`:**

Ein Startwert wie `int.MinValue` für die zweitgrößte wäre eine Falle: Bei `[int.MinValue, 5]` wäre das Ergebnis falsch, und man könnte nicht unterscheiden, ob überhaupt eine zweitgrößte gefunden wurde. `null` sagt eindeutig: „noch keine gefunden“.

**Schritt 3 — Implementierung:**

```csharp
static int Zweitgroesste(List<int> zahlen)
{
    if (zahlen.Count < 2)
        throw new ArgumentException("Mindestens zwei Elemente noetig.", nameof(zahlen));

    int groesste = zahlen[0];
    int? zweitgroesste = null;

    for (int i = 1; i < zahlen.Count; i++)
    {
        int aktuell = zahlen[i];
        if (aktuell > groesste)
        {
            zweitgroesste = groesste;
            groesste = aktuell;
        }
        else if (aktuell < groesste && (zweitgroesste == null || aktuell > zweitgroesste))
        {
            zweitgroesste = aktuell;
        }
    }

    if (zweitgroesste == null)
        throw new ArgumentException("Alle Elemente sind gleich.", nameof(zahlen));
    return zweitgroesste.Value;
}

Console.WriteLine(Zweitgroesste(new List<int> { 3, 7, 7, 5 }));  // 5
Console.WriteLine(Zweitgroesste(new List<int> { 5, 5, 3 }));     // 3
Console.WriteLine(Zweitgroesste(new List<int> { -3, -1 }));      // -3
```

**Zentrale Designentscheidungen:**

- **Duplikate durch `aktuell < groesste` ausschließen:** Eine Zahl, die gleich der größten ist, darf nie zweitgrößte werden – sonst ergäbe `[7, 7, 3]` fälschlich `7`.
- **Zwei getrennte Fehlerfälle:** „zu wenige Elemente“ wird vor der Schleife erkannt, „alle gleich“ erst danach. Beide Meldungen sagen dem Aufrufer genau, was falsch war.
- **Ein Durchlauf statt Sortieren:** Sortieren kostet bei großen Listen deutlich mehr – wie viel, sehen wir in [Vorlesung 06](/lectures/06/06.md).

</details>

## Aufgabe 4 — Abstraktion

Der folgende Warenkorb kompiliert (mit Warnungen) und stürzt trotzdem ab. Finde **mindestens vier** Fehler: einen, der zu einer `NullReferenceException` führt, eine fehlende Validierung, einen Off-by-one-Fehler und eine Designschwäche, die die anderen Fehler begünstigt.

```csharp
class Artikel
{
    public string Name { get; }
    public decimal Preis { get; }
    public Artikel(string name, decimal preis) { Name = name; Preis = preis; }
}

class Warenkorb
{
    private List<Artikel> artikel;
    private List<int> mengen = new List<int>();

    public void Hinzufuegen(Artikel a, int menge)
    {
        artikel.Add(a);
        mengen.Add(menge);
    }

    public decimal Gesamtpreis()
    {
        decimal summe = 0;
        for (int i = 0; i <= artikel.Count; i++)
            summe += artikel[i].Preis * mengen[i];
        return summe;
    }

    public Artikel? Finde(string name)
    {
        foreach (Artikel a in artikel)
            if (a.Name == name)
                return a;
        return null;
    }

    public decimal PreisVon(string name) => Finde(name)!.Preis;
}
```

- Welche Zeile stürzt beim allerersten `Hinzufuegen` ab, und warum?
- Welche Warnungen zeigt der Compiler bei eingeschaltetem `Nullable` – und welche Fehler hätte er dir damit verraten?
- Welche Abstraktion fehlt, damit `artikel` und `mengen` nicht auseinanderlaufen können?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Nicht initialisierte Liste (`NullReferenceException`):**

`artikel` wird deklariert, aber nie mit `new List<Artikel>()` belegt. Der erste Aufruf von `Hinzufuegen` ruft `Add` auf `null` auf. Der Compiler warnt hier bereits (CS8618: nicht-nullable Feld ist beim Verlassen des Konstruktors `null`) – wer Warnungen liest, findet den Fehler vor dem ersten Start.

**Schritt 2 — Off-by-one in `Gesamtpreis`:**

`i <= artikel.Count` läuft einen Schritt zu weit: Bei drei Artikeln greift der letzte Durchlauf auf `artikel[3]` zu und löst eine `ArgumentOutOfRangeException` aus. Richtig ist `i < artikel.Count` – oder gleich `foreach`, das solche Fehler unmöglich macht.

**Schritt 3 — Fehlende Validierung:**

`Hinzufuegen` akzeptiert `menge = 0` oder `-5`, `Artikel` akzeptiert negative Preise und leere Namen. Ein Warenkorb mit „minus fünf Stück“ ist ein Zustand, der nie entstehen dürfte. Beide Konstruktoren beziehungsweise Methoden brauchen eine Prüfung mit `ArgumentOutOfRangeException` oder `ArgumentException`.

**Schritt 4 — `PreisVon` und der Null-Forgiving-Operator:**

`Finde` liefert korrekt `Artikel?`, also möglicherweise `null`. In `PreisVon` schaltet das `!` die Compiler-Warnung stumm, ohne das Problem zu lösen: Ein unbekannter Name führt zur `NullReferenceException`. Sauber ist, den Fall explizit zu behandeln:

```csharp
public decimal PreisVon(string name)
{
    Artikel? gefunden = Finde(name);
    if (gefunden == null)
        throw new KeyNotFoundException($"Artikel '{name}' nicht im Warenkorb.");
    return gefunden.Preis;
}
```

**Schritt 5 — Die fehlende Abstraktion:**

Zwei parallele Listen `artikel` und `mengen` müssen immer gleich lang sein – aber nichts erzwingt das. Die Lösung ist eine Klasse, die zusammengehörige Daten zusammenhält:

```csharp
class Position
{
    public Artikel Artikel { get; }
    public int Menge { get; }

    public Position(Artikel artikel, int menge)
    {
        if (menge <= 0)
            throw new ArgumentOutOfRangeException(nameof(menge));
        Artikel = artikel;
        Menge = menge;
    }

    public decimal Gesamt => Artikel.Preis * Menge;
}
```

Mit `private List<Position> positionen = new List<Position>();` schrumpft `Gesamtpreis` auf eine `foreach`-Schleife über `positionen`, die `Gesamt` addiert – kein Index, kein Off-by-one, keine zweite Liste.

**Zentrale Designentscheidungen:**

- **Warnungen sind Fehler in Wartestellung:** `Nullable` hat zwei der vier Probleme vorab angezeigt. `!` darf nur stehen, wenn man beweisen kann, dass der Wert nicht `null` ist.
- **Zusammengehöriges gehört in eine Klasse:** Parallele Listen sind fast immer ein Zeichen, dass eine Abstraktion fehlt – genau wie in Aufgabe 2.
- **Validieren an der Grenze:** Der Konstruktor von `Position` ist die einzige Stelle, an der eine Menge hereinkommt. Wer dort prüft, muss es nirgendwo sonst tun.

</details>
