---
title: "🧩 Aufgaben und Beispiele: Collections und LINQ"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Programmieren lernt man nicht nur durch Codezeilen tippen — sondern auch durch **Nachdenken**. Die folgenden Aufgaben trainieren *Computational Thinking*: die Fähigkeit, Probleme so zu strukturieren, dass ein Computer sie lösen kann. Dazu gehören Abstraktion, Zerlegung, Mustererkennung und Algorithmenentwurf. Bei Collections dreht sich fast alles um eine einzige Frage: Welches Zugriffsmuster hat mein Problem – und welche Datenstruktur ist genau dafür gebaut? Und LINQ zwingt uns, eine Frage in ihre Bestandteile zu zerlegen: Was wird gefiltert, wonach wird sortiert, was wird gruppiert? Nimm dir für jede Aufgabe Zeit, bevor du die Lösung aufklappst.

## Aufgabe 1 — Mustererkennung

Ein Kino verwaltet Reservierungen in einem `Dictionary`, dessen Schlüssel ein Sitzplatz ist. Die Klasse `Sitzplatz` sieht harmlos aus – und trotzdem verhält sich das Programm nicht so, wie der Autor es erwartet.

```csharp
public class Sitzplatz
{
    public int Reihe { get; set; }
    public int Nummer { get; set; }

    public Sitzplatz(int reihe, int nummer)
    {
        Reihe = reihe;
        Nummer = nummer;
    }
}

Dictionary<Sitzplatz, string> reservierungen = new Dictionary<Sitzplatz, string>();

Sitzplatz platz = new Sitzplatz(3, 12);
reservierungen[platz] = "Anna Ahrens";

Console.WriteLine(reservierungen.ContainsKey(platz));                 // ?
Console.WriteLine(reservierungen.ContainsKey(new Sitzplatz(3, 12)));  // ?
Console.WriteLine(reservierungen.Count);                              // ?

reservierungen[new Sitzplatz(3, 12)] = "Bela Brandt";
Console.WriteLine(reservierungen.Count);                              // ?
```

- Sage die vier Ausgaben voraus. Welche davon würde der Autor als Fehler empfinden – und warum ist keine davon aus Sicht des Dictionaries ein Fehler?
- Warum verhält sich `ContainsKey(platz)` anders als `ContainsKey(new Sitzplatz(3, 12))`, obwohl beide Objekte dieselbe Reihe und dieselbe Nummer haben?
- Ein Kommilitone „repariert“ die Klasse, indem er nur `Equals` überschreibt. Was ändert sich an den Ausgaben? Der Compiler meldet dabei eine Warnung – welche und warum?
- Korrigiere die Klasse mit `HashCode.Combine`. Was muss zusätzlich geändert werden, damit ein Schlüssel nach dem Einfügen nicht „verloren gehen“ kann?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Ausgabe vorhersagen:**

```
True
False
1
2
```

Die erste Abfrage findet den Eintrag, weil `platz` **dieselbe Instanz** ist, mit der eingefügt wurde. Die zweite scheitert, obwohl `new Sitzplatz(3, 12)` inhaltlich gleich ist. Und die Zuweisung am Ende legt einen **zweiten** Eintrag an, statt den ersten zu überschreiben – deshalb steigt `Count` auf 2. Aus Sicht des Dictionaries ist das alles korrekt: Es hat zwei verschiedene Schlüssel bekommen.

**Schritt 2 — Das Muster erkennen:**

`Sitzplatz` ist eine eigene Klasse, die weder `GetHashCode` noch `Equals` überschreibt. Damit gelten die Standardimplementierungen von `object`, und die arbeiten mit der **Identität** des Objekts, nicht mit seinem Inhalt – genau die Situation aus dem Modul [Hashcodes und Equals](/modules/hashcodes_equals/hashcodes_equals.md). Beim Nachschlagen berechnet das Dictionary zuerst `GetHashCode()` des gesuchten Schlüssels. Für `new Sitzplatz(3, 12)` kommt ein anderer Wert heraus als für `platz`, also schaut das Dictionary in einen anderen Bucket – oder findet im richtigen Bucket einen Eintrag mit abweichendem Hashcode – und meldet: nicht vorhanden.

**Schritt 3 — Warum `Equals` allein nicht reicht:**

```csharp
public override bool Equals(object? obj)
{
    return obj is Sitzplatz anderer && Reihe == anderer.Reihe && Nummer == anderer.Nummer;
}
// Warnung CS0659: 'Sitzplatz' überschreibt Object.Equals, aber nicht Object.GetHashCode
```

An den Ausgaben ändert sich **nichts**. Das Dictionary vergleicht zuerst den gespeicherten Hashcode mit dem des gesuchten Schlüssels und ruft `Equals` überhaupt nur auf, wenn beide übereinstimmen. Da die Hashcodes weiterhin identitätsbasiert sind, kommt es nie zum `Equals`-Aufruf. Genau davor warnt CS0659: Der Vertrag „gleiche Objekte haben gleiche Hashcodes“ ist verletzt.

**Schritt 4 — Die Korrektur:**

```csharp
public class Sitzplatz
{
    public int Reihe { get; }
    public int Nummer { get; }

    public Sitzplatz(int reihe, int nummer)
    {
        Reihe = reihe;
        Nummer = nummer;
    }

    public override bool Equals(object? obj)
    {
        return obj is Sitzplatz anderer && Reihe == anderer.Reihe && Nummer == anderer.Nummer;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Reihe, Nummer);
    }
}
```

Mit dieser Version lauten die Ausgaben `True`, `True`, `1`, `1` – die letzte Zuweisung überschreibt jetzt die Reservierung von Anna durch Bela, statt einen zweiten Eintrag anzulegen.

Die zweite Änderung ist unscheinbar: Aus `{ get; set; }` wurde `{ get; }`. Hätten die Properties weiterhin Setter, könnte jemand nach dem Einfügen `platz.Reihe = 4` schreiben. Der Eintrag liegt dann noch im Bucket, der aus `HashCode.Combine(3, 12)` berechnet wurde, gesucht wird aber im Bucket von `HashCode.Combine(4, 12)`. `ContainsKey(platz)` liefert `False`, `Count` bleibt 1 – der Eintrag ist da, aber unerreichbar.

**Zentrale Designentscheidungen:**

- **`Equals` und `GetHashCode` immer gemeinsam überschreiben:** Das Dictionary nutzt beide Methoden in fester Reihenfolge – erst den Hashcode für den Bucket, dann `Equals` innerhalb des Buckets. Wer nur eine der beiden anpasst, bekommt ein Dictionary, das je nach Zufall mal findet und mal nicht.
- **`HashCode.Combine` statt eigener Rechnerei:** `Reihe * 31 + Nummer` wäre auch ein gültiger Hashcode, aber `HashCode.Combine` verteilt die Werte besser über die Buckets und liest sich sofort als „Hashcode aus diesen Feldern“. Alle Felder, die in `Equals` verglichen werden, gehören auch hinein – und keine anderen.
- **Schlüssel sind unveränderlich:** Ein Sitzplatz, der seine Reihe ändert, ist ein anderer Sitzplatz. Für solche reinen Wertobjekte bietet C# den `record`-Typ an, der `Equals` und `GetHashCode` automatisch inhaltsbasiert erzeugt: `public record Sitzplatz(int Reihe, int Nummer);` wäre die einzeilige Alternative.

</details>

## Aufgabe 2 — Algorithmenentwurf

Die binäre Suche aus dem Modul [Such- und Sortieralgorithmen](/modules/such_und_sortieralgorithmen/such_und_sortieralgorithmen.md) liefert `-1`, wenn das gesuchte Element fehlt. Für ein Programm, das eine Liste **dauerhaft sortiert** halten will, ist das zu wenig: Es muss wissen, *an welcher Stelle* das fehlende Element eingefügt werden müsste, damit die Ordnung erhalten bleibt.

Entwirf eine Variante `BinaereSucheMitEinfuegeindex`, die bei einem Treffer wie bisher den Index liefert und andernfalls den **Einfügeindex** – und zwar ohne einen zweiten Suchdurchlauf.

- Der Rückgabewert soll beide Informationen tragen: „gefunden an Index i“ oder „nicht gefunden, einfügen bei Index i“. Warum reicht ein einfaches Vorzeichen (`-i`) dafür nicht aus? Welche Kodierung verwendet `Array.BinarySearch`?
- Betrachte die Schleife der bisherigen binären Suche: Welchen Wert haben `unten` und `oben` in dem Moment, in dem die Schleife ohne Treffer endet? Was sagt dieser Wert über den Einfügeindex?
- Sage für `int[] zahlen = [2, 5, 8, 12, 16, 23, 38, 56, 72, 91]` die Rückgabe für die Suche nach `23`, `7`, `1` und `100` voraus. Was liefert die Methode für ein leeres Array?
- Wie sieht damit eine Methode `SortiertEinfuegen(List<int> sortiert, int wert)` aus?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Das Problem mit dem Rückgabewert:**

Ein Index kann `0` sein – „gefunden an Position 0“ und „einfügen an Position 0“ würden beide als `0` zurückkommen, und `-0` ist dasselbe wie `0`. Die Kodierung muss also so verschoben werden, dass auch Einfügeindex 0 negativ wird. `Array.BinarySearch` und `List<T>.BinarySearch` verwenden dafür das **bitweise Komplement** `~index`, das für nicht-negative Zahlen dasselbe ist wie `-(index + 1)`:

| Einfügeindex | Rückgabe `~index` |
| :--- | :--- |
| 0 | -1 |
| 2 | -3 |
| 10 | -11 |

Der Aufrufer prüft `ergebnis < 0` und rechnet mit `~ergebnis` zurück – `~` ist seine eigene Umkehrung. Wir übernehmen diese Konvention, damit unsere Methode zu den Bibliotheksmethoden passt.

**Schritt 2 — Die Schleife verstehen:**

Die Schleife der binären Suche hält die Invariante aufrecht: Alle Elemente links von `unten` sind **kleiner** als das gesuchte, alle Elemente rechts von `oben` sind **größer**. Sie endet ohne Treffer, sobald `unten > oben` gilt – genauer `unten == oben + 1`, weil sich die Grenzen immer nur um eins über die Mitte hinaus bewegen. In diesem Moment liegt zwischen den beiden Grenzen kein Element mehr: Alles bis `unten - 1` ist kleiner, alles ab `unten` ist größer. Das gesuchte Element gehört also genau an Position `unten`. Wir müssen nichts Neues berechnen, nur den Wert von `unten` zurückgeben, den die Schleife ohnehin hinterlässt.

**Schritt 3 — Die Implementierung:**

```csharp
static int BinaereSucheMitEinfuegeindex(IReadOnlyList<int> sortiert, int gesucht)
{
    int unten = 0;
    int oben = sortiert.Count - 1;

    while (unten <= oben)
    {
        int mitte = unten + (oben - unten) / 2;

        if (sortiert[mitte] == gesucht)
        {
            return mitte;
        }
        if (sortiert[mitte] < gesucht)
        {
            unten = mitte + 1;
        }
        else
        {
            oben = mitte - 1;
        }
    }
    return ~unten; // nicht gefunden: Einfügeindex verschlüsselt
}
```

Gegenüber der Version aus dem Modul hat sich nur die letzte Zeile geändert – und der Parametertyp: `IReadOnlyList<int>` akzeptiert Arrays *und* Listen, wie im Modul [Collections im Überblick](/modules/collections_ueberblick/collections_ueberblick.md) empfohlen.

**Schritt 4 — Ausgaben vorhersagen:**

Für die Suche nach `7` läuft die Schleife so: `mitte = 4` (Wert 16, zu groß) → `oben = 3`; `mitte = 1` (Wert 5, zu klein) → `unten = 2`; `mitte = 2` (Wert 8, zu groß) → `oben = 1`. Jetzt ist `unten = 2 > oben = 1`, die Schleife endet, Rückgabe `~2 = -3`. Die 7 gehört zwischen die 5 (Index 1) und die 8 (Index 2) – Index 2 ist richtig.

```csharp
int[] zahlen = [2, 5, 8, 12, 16, 23, 38, 56, 72, 91];

Console.WriteLine(BinaereSucheMitEinfuegeindex(zahlen, 23));  // 5
Console.WriteLine(BinaereSucheMitEinfuegeindex(zahlen, 7));   // -3  (einfügen bei 2)
Console.WriteLine(BinaereSucheMitEinfuegeindex(zahlen, 1));   // -1  (einfügen bei 0)
Console.WriteLine(BinaereSucheMitEinfuegeindex(zahlen, 100)); // -11 (einfügen bei 10 = Length)
Console.WriteLine(BinaereSucheMitEinfuegeindex([], 5));       // -1  (einfügen bei 0)
```

Die Randfälle fallen ohne Sonderbehandlung richtig heraus: Ist das Element kleiner als alle anderen, wandert `oben` bis auf `-1` und `unten` bleibt `0`. Ist es größer als alle, wandert `unten` bis auf `Count` – der Index hinter dem letzten Element, genau dort, wo `Insert` anhängen würde. Beim leeren Array ist `oben = -1` von Anfang an, die Schleife läuft kein einziges Mal, und `~0 = -1` sagt: einfügen bei 0. Ein Vergleich mit `Array.BinarySearch(zahlen, 7)` liefert übrigens dieselben Werte.

**Schritt 5 — Sortiert einfügen:**

```csharp
static void SortiertEinfuegen(List<int> sortiert, int wert)
{
    int ergebnis = BinaereSucheMitEinfuegeindex(sortiert, wert);
    int index = ergebnis >= 0 ? ergebnis : ~ergebnis;
    sortiert.Insert(index, wert);
}

List<int> liste = [2, 5, 8, 12];
SortiertEinfuegen(liste, 7);
SortiertEinfuegen(liste, 20);
SortiertEinfuegen(liste, 5);
Console.WriteLine(string.Join(", ", liste)); // 2, 5, 5, 7, 8, 12, 20
```

Ist der Wert schon enthalten, wird er direkt neben seinem Zwilling eingefügt – die Ordnung bleibt in beiden Fällen erhalten, deshalb braucht `SortiertEinfuegen` keine Fallunterscheidung außer dem Entschlüsseln.

**Zentrale Designentscheidungen:**

- **Kein zweiter Durchlauf:** Der Einfügeindex fällt als Nebenprodukt der Suche ab. Wer stattdessen nach dem `-1` eine lineare Suche nach der ersten größeren Zahl anschließt, hat aus O(log n) wieder O(n) gemacht.
- **Kodierung wie in .NET:** Ein Tupel `(bool Gefunden, int Index)` wäre lesbarer, aber die `~`-Konvention ist der Standard in `Array.BinarySearch` und `List<T>.BinarySearch` – wer sie einmal verstanden hat, kann die Bibliotheksmethoden direkt benutzen, statt eine eigene zu schreiben.
- **Duplikate:** Bei mehreren gleichen Werten liefert die Suche *irgendeinen* Treffer, nicht zwingend den ersten. Für `SortiertEinfuegen` ist das egal; wer den ersten von mehreren gleichen braucht, muss die Schleife so ändern, dass sie bei einem Treffer nicht abbricht, sondern `oben` weiter nach links schiebt.
- **Sortiert einfügen kostet O(n):** Die Suche ist logarithmisch, aber `Insert` verschiebt alle nachfolgenden Elemente. Wer sehr oft einfügt und selten liest, ist mit einem `SortedSet<T>` oder einer `SortedDictionary<K, V>` besser bedient – Einfügen in O(log n) statt O(n).

</details>

## Aufgabe 3 — Abstraktion

Eine Hochschule braucht ein Kursverzeichnis. Jeder Kurs hat ein eindeutiges Kürzel (etwa `"PROG2"`), einen Titel und eine Menge von Teilnehmern; jeder Teilnehmer hat eine eindeutige Matrikelnummer und einen Namen. Das Programm muss folgende Operationen unterstützen:

1. Zu einem Kürzel den Kurs finden – das passiert bei jeder Anmeldung, also sehr häufig.
2. Einen Teilnehmer zu einem Kurs anmelden – niemand darf doppelt im selben Kurs stehen.
3. Prüfen, ob eine Matrikelnummer in einem Kurs angemeldet ist.
4. Die Teilnehmer eines Kurses alphabetisch nach Name ausgeben – einmal pro Semester für die Anwesenheitsliste.
5. Alle Kurse nach Kürzel sortiert ausgeben.

Wähle für das Verzeichnis und für die Teilnehmer eines Kurses je eine Collection aus der Entscheidungstabelle in [Collections im Überblick](/modules/collections_ueberblick/collections_ueberblick.md) und begründe die Wahl über die Zugriffsmuster.

- Welche der fünf Operationen sind häufig, welche selten? Was folgt daraus für die Wahl zwischen „sortiert speichern“ und „bei der Ausgabe sortieren“?
- Ein `HashSet<Studierender>` würde Duplikate von allein verhindern. Was müsste die Klasse `Studierender` dafür können – und gibt es eine Alternative, die ohne diese Zusatzarbeit auskommt?
- Ein `SortedDictionary<string, Studierender>` mit dem Namen als Schlüssel würde Operation 4 kostenlos machen. Warum ist das trotzdem eine schlechte Idee?
- Skizziere die Klassen `Studierender`, `Kurs` und `Kursverzeichnis` mit den nötigen Methoden.
- Zusatz: Später soll auch die Frage „Welche Kurse belegt Anna?“ häufig beantwortet werden. Was ändert sich?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Zugriffsmuster analysieren:**

Operationen 1 bis 3 sind **Nachschlagen über einen Schlüssel** (Kürzel bzw. Matrikelnummer) und passieren ständig. Operationen 4 und 5 sind **sortierte Ausgaben** und passieren selten. Das ist die klassische Konstellation für ein `Dictionary`: O(1) für die häufigen Zugriffe, und die seltene Sortierung bezahlen wir bei der Ausgabe mit einmal O(n log n), statt bei jedem Einfügen den Preis eines sortierten Baums zu zahlen.

**Schritt 2 — Teilnehmer: `Dictionary` statt `HashSet`:**

Ein `HashSet<Studierender>` funktioniert nur, wenn `Studierender` `Equals` und `GetHashCode` inhaltsbasiert überschreibt – sonst ist es genau die Falle aus Aufgabe 1, und dieselbe Person könnte über zwei Instanzen zweimal angemeldet werden. Die Alternative kommt ohne Zusatzarbeit aus: Ein `Dictionary<int, Studierender>` mit der Matrikelnummer als Schlüssel. `int` hat einen inhaltsbasierten Hashcode, Duplikate sind über den Schlüssel ausgeschlossen, und Operation 3 wird zu einem `ContainsKey` – man muss dafür nicht einmal ein `Studierender`-Objekt in der Hand haben.

Ein `SortedDictionary<string, Studierender>` mit dem Namen als Schlüssel scheitert an einer Fachlichkeit, nicht an der Technik: Namen sind nicht eindeutig. Zwei Studierende namens „Anna Ahrens“ würden sich gegenseitig überschreiben. Der Schlüssel einer Collection muss immer das sein, was die Elemente wirklich identifiziert.

**Schritt 3 — Die Klassen:**

```csharp
public class Studierender
{
    public int Matrikelnummer { get; }
    public string Name { get; }

    public Studierender(int matrikelnummer, string name)
    {
        Matrikelnummer = matrikelnummer;
        Name = name;
    }
}

public class Kurs
{
    private readonly Dictionary<int, Studierender> teilnehmer = new Dictionary<int, Studierender>();

    public string Kuerzel { get; }
    public string Titel { get; }
    public int Anzahl => teilnehmer.Count;

    public Kurs(string kuerzel, string titel)
    {
        Kuerzel = kuerzel;
        Titel = titel;
    }

    public bool Anmelden(Studierender studierender)
    {
        return teilnehmer.TryAdd(studierender.Matrikelnummer, studierender);
    }

    public bool IstAngemeldet(int matrikelnummer)
    {
        return teilnehmer.ContainsKey(matrikelnummer);
    }

    public IEnumerable<Studierender> TeilnehmerSortiert()
    {
        return from s in teilnehmer.Values
               orderby s.Name
               select s;
    }
}
```

`TryAdd` fügt nur ein, wenn der Schlüssel neu ist, und meldet über den Rückgabewert, ob das geklappt hat – ein `ContainsKey` vorab ist überflüssig. Die sortierte Ausgabe ist eine LINQ-Abfrage über `teilnehmer.Values`, wie im Modul [LINQ – Query-Syntax](/modules/linq_query_syntax/linq_query_syntax.md). Das Verzeichnis selbst ist nach demselben Muster gebaut:

```csharp
public class Kursverzeichnis
{
    private readonly Dictionary<string, Kurs> kurse = new Dictionary<string, Kurs>();

    public void Hinzufuegen(Kurs kurs)
    {
        kurse.Add(kurs.Kuerzel, kurs); // ArgumentException bei doppeltem Kürzel
    }

    public Kurs? Suchen(string kuerzel)
    {
        return kurse.GetValueOrDefault(kuerzel);
    }

    public IEnumerable<Kurs> KurseSortiert()
    {
        return from k in kurse.Values
               orderby k.Kuerzel
               select k;
    }
}
```

**Schritt 4 — Nutzung:**

```csharp
Kursverzeichnis verzeichnis = new Kursverzeichnis();
Kurs prog2 = new Kurs("PROG2", "Programmierung 2");
Kurs mathe = new Kurs("MATHE1", "Mathematik 1");
verzeichnis.Hinzufuegen(prog2);
verzeichnis.Hinzufuegen(mathe);

Studierender anna = new Studierender(577001, "Anna Ahrens");
Studierender zoe = new Studierender(577002, "Zoe Ziegler");
prog2.Anmelden(zoe);
prog2.Anmelden(anna);
mathe.Anmelden(anna);
Console.WriteLine(prog2.Anmelden(anna));        // False – schon angemeldet
Console.WriteLine(prog2.IstAngemeldet(577002)); // True
Console.WriteLine(verzeichnis.Suchen("BWL") is null); // True

foreach (Kurs kurs in verzeichnis.KurseSortiert())
{
    Console.WriteLine($"{kurs.Kuerzel}: {kurs.Anzahl} Teilnehmer");
    foreach (Studierender s in kurs.TeilnehmerSortiert())
    {
        Console.WriteLine($"  {s.Name}");
    }
}
// MATHE1: 1 Teilnehmer
//   Anna Ahrens
// PROG2: 2 Teilnehmer
//   Anna Ahrens
//   Zoe Ziegler
```

Anna ist in beiden Kursen dasselbe Objekt – die Dictionaries speichern nur Referenzen. Ändert sich ihr Name, sehen es alle Kurse.

**Zentrale Designentscheidungen:**

- **Der Schlüssel ist die fachliche Identität:** Kürzel für Kurse, Matrikelnummer für Studierende. Damit sind Duplikate ausgeschlossen, ohne dass `Equals`/`GetHashCode` überschrieben werden müssen – und der Aufrufer kann mit einer bloßen Nummer nachschlagen.
- **Sortieren bei der Ausgabe, nicht beim Speichern:** Die sortierte Liste wird selten gebraucht. Ein `SortedDictionary` würde bei jeder Anmeldung O(log n) statt O(1) kosten und außerdem einen eindeutigen Sortierschlüssel verlangen, den es beim Namen nicht gibt.
- **Die Collections sind `private`:** Nach außen gibt es `Anmelden`, `IstAngemeldet` und `TeilnehmerSortiert` – niemand kann von außen `teilnehmer.Clear()` aufrufen. `TeilnehmerSortiert` liefert ein `IEnumerable<Studierender>`, also einen Abfrageplan; wer das Ergebnis aufheben will, hängt `ToList()` an (siehe [verzögerte Ausführung](/modules/linq_deferred_execution/linq_deferred_execution.md)).
- **Die Rückfrage „Welche Kurse belegt Anna?“** ist mit dieser Struktur eine Suche über alle Kurse – O(Anzahl Kurse), bei zwanzig Kursen völlig in Ordnung. Wird sie häufig und die Kurszahl groß, kommt ein zweites Dictionary `Dictionary<int, HashSet<string>>` (Matrikelnummer → Kürzel) hinzu. Der Preis ist Redundanz: `Anmelden` muss beide Strukturen konsistent halten, und genau deshalb gehört diese Logik in **eine** Methode des Verzeichnisses, nicht in den Kurs.

</details>

## Aufgabe 4 — Zerlegung

Für diese Aufgabe verwenden wir eine schlankere Variante von `Studierender` mit den drei Properties, die uns interessieren:

```csharp
public class Studierender
{
    public required string Name { get; init; }
    public int Semester { get; init; }
    public double Notenschnitt { get; init; }
}

List<Studierender> studierende =
[
    new Studierender { Name = "Anna Ahrens", Semester = 3, Notenschnitt = 1.7 },
    new Studierender { Name = "Bela Brandt", Semester = 1, Notenschnitt = 2.9 },
    new Studierender { Name = "Cem Celik",   Semester = 3, Notenschnitt = 2.3 },
    new Studierender { Name = "Dana Dorn",   Semester = 5, Notenschnitt = 1.3 },
    new Studierender { Name = "Emil Ernst",  Semester = 1, Notenschnitt = 3.4 },
    new Studierender { Name = "Fara Fuchs",  Semester = 5, Notenschnitt = 2.0 },
    new Studierender { Name = "Zoe Ziegler", Semester = 3, Notenschnitt = 1.7 }
];
```

Zerlege jede der folgenden Fragen in ihre Bestandteile – *Quelle*, *Filter*, *Ordnung*, *Ergebnisform* – und formuliere sie als LINQ-Abfrage in Query-Syntax. Sage dann die Ausgabe voraus, **bevor** du den Code startest.

- (a) Die Namen aller Studierenden mit einem Notenschnitt von höchstens 2,0, aufsteigend nach Notenschnitt. Bei gleichem Schnitt soll der Name entscheiden. Welche Reihenfolge haben Anna und Zoe?
- (b) Alle Studierenden ab dem 3. Semester als Text `"Name (n. Semester)"`, höchstes Semester zuerst, innerhalb eines Semesters alphabetisch.
- (c) Eine Gruppierung nach Semester, die pro Gruppe die Zeile `Semester n: Name, Name, …` ausgibt. In welcher Reihenfolge erscheinen die Gruppen – und wie bekommst du sie nach Semester sortiert?
- (d) Zusatzfrage: Ein Kommilitone gibt das Ergebnis von (a) aus, fügt danach `Gerd Graf` (1. Semester, Schnitt 1,0) hinzu, entfernt Anna und gibt dasselbe Ergebnis erneut aus. Beide Ausgaben unterscheiden sich, obwohl er die Abfrage nicht angefasst hat. Warum – und wie hätte er die erste Ausgabe „einfrieren“ können?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Abfrage (a) zerlegen:**

Quelle `studierende`, Filter `Notenschnitt <= 2.0`, Ordnung nach `Notenschnitt` und dann `Name`, Ergebnis nur der `Name`. Zwei Sortierkriterien werden im `orderby` mit Komma getrennt – das zweite greift nur bei Gleichstand im ersten:

```csharp
IEnumerable<string> gute =
    from s in studierende
    where s.Notenschnitt <= 2.0
    orderby s.Notenschnitt, s.Name
    select s.Name;

Console.WriteLine(string.Join(", ", gute));
// Dana Dorn, Anna Ahrens, Zoe Ziegler, Fara Fuchs
```

Anna und Zoe haben beide 1,7 – das zweite Kriterium sortiert Anna vor Zoe. Ohne `, s.Name` wäre die Reihenfolge der beiden nicht garantiert vorhersagbar. Weil `select s.Name` einen `string` liefert, ist das Ergebnis ein `IEnumerable<string>`, kein `IEnumerable<Studierender>`.

**Schritt 2 — Abfrage (b) mit absteigender Ordnung:**

Neu ist `descending`, das nur für das Kriterium gilt, hinter dem es steht. Das `select` baut mit String-Interpolation ein neues Ergebnis, das es in der Quelle so nicht gibt:

```csharp
IEnumerable<string> hoehere =
    from s in studierende
    where s.Semester >= 3
    orderby s.Semester descending, s.Name
    select $"{s.Name} ({s.Semester}. Semester)";

foreach (string zeile in hoehere)
{
    Console.WriteLine(zeile);
}
// Dana Dorn (5. Semester)
// Fara Fuchs (5. Semester)
// Anna Ahrens (3. Semester)
// Cem Celik (3. Semester)
// Zoe Ziegler (3. Semester)
```

**Schritt 3 — Abfrage (c) gruppieren:**

`group s by s.Semester` ersetzt das `select`; das Ergebnis ist ein `IEnumerable<IGrouping<int, Studierender>>`. Jede Gruppe hat einen `Key` (das Semester) und ist selbst wieder eine Aufzählung, über die eine innere Abfrage die Namen einsammelt:

```csharp
var nachSemester =
    from s in studierende
    group s by s.Semester;

foreach (IGrouping<int, Studierender> gruppe in nachSemester)
{
    IEnumerable<string> namen = from s in gruppe select s.Name;
    Console.WriteLine($"Semester {gruppe.Key}: {string.Join(", ", namen)}");
}
// Semester 3: Anna Ahrens, Cem Celik, Zoe Ziegler
// Semester 1: Bela Brandt, Emil Ernst
// Semester 5: Dana Dorn, Fara Fuchs
```

Die Gruppen erscheinen in der Reihenfolge, in der ihr Schlüssel **zum ersten Mal** in der Quelle auftaucht: Anna (Semester 3) steht an erster Stelle, also kommt Gruppe 3 zuerst – nicht Gruppe 1. Innerhalb einer Gruppe bleibt die Quellreihenfolge erhalten. Wer die Gruppen sortiert haben will, gibt ihnen mit `into` einen Namen und setzt die Abfrage damit fort:

```csharp
var nachSemesterSortiert =
    from s in studierende
    group s by s.Semester into gruppe
    orderby gruppe.Key
    select gruppe;
// Semester 1: Bela Brandt, Emil Ernst
// Semester 3: Anna Ahrens, Cem Celik, Zoe Ziegler
// Semester 5: Dana Dorn, Fara Fuchs
```

Nach `into` ist `s` nicht mehr sichtbar – die Abfrage arbeitet ab hier mit Gruppen, nicht mehr mit einzelnen Studierenden.

**Schritt 4 — Die Zusatzfrage:**

```csharp
Console.WriteLine(string.Join(", ", gute));
// Dana Dorn, Anna Ahrens, Zoe Ziegler, Fara Fuchs

studierende.Add(new Studierender { Name = "Gerd Graf", Semester = 1, Notenschnitt = 1.0 });
studierende.RemoveAt(0); // Anna

Console.WriteLine(string.Join(", ", gute));
// Gerd Graf, Dana Dorn, Zoe Ziegler, Fara Fuchs
```

`gute` ist keine Liste mit vier Namen, sondern ein **Plan**: „nimm `studierende`, filtere, sortiere, gib die Namen“. Der Plan wird bei jedem `string.Join` neu ausgeführt – und beim zweiten Mal enthält `studierende` Gerd statt Anna. Die Abfrage hält nur eine Referenz auf die Liste, keine Kopie ihres Inhalts. Genau das beschreibt das Modul zur [verzögerten Ausführung](/modules/linq_deferred_execution/linq_deferred_execution.md).

Einfrieren lässt sich das Ergebnis mit `ToList()` direkt bei der Definition:

```csharp
List<string> guteFest =
    (from s in studierende
     where s.Notenschnitt <= 2.0
     orderby s.Notenschnitt, s.Name
     select s.Name).ToList();
```

`guteFest` wird einmal berechnet und ist danach von `studierende` entkoppelt – beide Ausgaben wären identisch, und Gerd tauchte nie auf.

**Zentrale Designentscheidungen:**

- **Frage zuerst zerlegen, dann tippen:** Jede Klausel beantwortet genau einen Teil der Frage – `where` das „welche“, `orderby` das „in welcher Reihenfolge“, `select` das „in welcher Form“. Wer die Zerlegung im Kopf hat, schreibt die Abfrage in der Reihenfolge der Klauseln auf, ohne über Schleifen nachzudenken.
- **Sortierkriterien vollständig angeben:** Bei Gleichstand ist die Reihenfolge ohne zweites Kriterium zwar in der Praxis stabil, aber niemand, der den Code liest, kann sich darauf verlassen. `orderby s.Notenschnitt, s.Name` macht die Absicht explizit.
- **Gruppen sind keine sortierten Töpfe:** `group by` ordnet nach erstem Auftreten. Wer sortierte Gruppen braucht, muss es mit `into` und `orderby gruppe.Key` sagen – der Compiler rät nicht.
- **Plan oder Liste – bewusst entscheiden:** Ein `IEnumerable<T>` ist richtig, wenn das Ergebnis einmal durchlaufen wird und die aktuellen Daten zeigen soll. Sobald ein Ergebnis aufgehoben, mehrfach gelesen oder mit einem späteren Zustand verglichen wird, gehört `ToList()` an die Definition – nicht erst dorthin, wo das Problem auffällt.

</details>
