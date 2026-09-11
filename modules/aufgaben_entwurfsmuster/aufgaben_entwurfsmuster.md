---
title: "🧩 Aufgaben und Beispiele: Entwurfsmuster"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Programmieren lernt man nicht nur durch Codezeilen tippen — sondern auch durch **Nachdenken**. Die folgenden Aufgaben trainieren *Computational Thinking*: die Fähigkeit, Probleme so zu strukturieren, dass ein Computer sie lösen kann. Dazu gehören Abstraktion, Zerlegung, Mustererkennung und Algorithmenentwurf. Bei Entwurfsmustern ist die Mustererkennung wörtlich gemeint: Die Kunst besteht weniger darin, ein Muster zu implementieren, als darin, es in fremdem Code wiederzuerkennen, das passende auszuwählen – und zu merken, wenn eines fehl am Platz ist. Nimm dir für jede Aufgabe Zeit, bevor du die Lösung aufklappst.

## Aufgabe 1 — Mustererkennung

Die folgenden vier Codeausschnitte stammen aus verschiedenen Projekten. Bestimme für jeden, welches Entwurfsmuster aus dieser Vorlesung er umsetzt – oder ob er nur so aussieht.

```csharp
// Ausschnitt A
public class Protokoll
{
    private static readonly Lazy<Protokoll> halter = new(() => new Protokoll());
    private Protokoll() { }
    public static Protokoll Instanz => halter.Value;
    public void Schreiben(string text) { /* ... */ }
}
```

```csharp
// Ausschnitt B
public class Menue : IMenueEintrag
{
    private readonly List<IMenueEintrag> eintraege = new();
    public void Hinzufuegen(IMenueEintrag e) => eintraege.Add(e);
    public void Anzeigen(int tiefe)
    {
        foreach (IMenueEintrag e in eintraege)
            e.Anzeigen(tiefe + 1);
    }
}
```

```csharp
// Ausschnitt C
public class KundenExport : IExportierbar
{
    private readonly LegacyKundenDatei datei;
    public KundenExport(LegacyKundenDatei datei) => this.datei = datei;
    public string AlsJson() => JsonSerializer.Serialize(datei.LeseAlleZeilen());
}
```

```csharp
// Ausschnitt D
public class Lager
{
    private readonly Dictionary<string, int> bestand = new();
    public IEnumerable<string> Knapp(int grenze)
    {
        foreach (var (artikel, menge) in bestand)
            if (menge < grenze)
                yield return artikel;
    }
}
```

Leitfragen:
- Woran erkennst du das Muster – an einem Schlüsselwort, an einer Struktur, an den beteiligten Rollen?
- Welche Rolle des Musters spielt die gezeigte Klasse, und welche Rollen fehlen im Ausschnitt?
- Gibt es Ausschnitte, bei denen du ohne den fehlenden Kontext nicht sicher sein kannst?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Ausschnitt A: Singleton.**

Die drei Erkennungsmerkmale sind vollständig da: privater Konstruktor, statisches Feld für die einzige Instanz, statische Zugriffs-Property. Das `Lazy<T>` verrät zusätzlich, dass die threadsichere Variante aus dem [Singleton-Modul](/modules/singleton/singleton.md) verwendet wird. Es fehlt `sealed` – eine geschachtelte Unterklasse könnte theoretisch weitere Instanzen erzeugen.

**Schritt 2 — Ausschnitt B: Composite.**

Merkmal: Eine Klasse implementiert ein Interface *und* hält eine Liste desselben Interface-Typs, und ihre Operation besteht nur aus dem Weiterreichen an die Kinder. `Menue` ist das Kompositum, `IMenueEintrag` die Komponente. Die Blätter (etwa ein `Befehl : IMenueEintrag`) sind nicht zu sehen, müssen aber existieren, sonst würde die Rekursion nie enden. Dass `Hinzufuegen` nur in `Menue` steht, ist die LSP-freundliche Variante aus dem [Composite-Modul](/modules/composite/composite.md).

**Schritt 3 — Ausschnitt C: Objektadapter.**

Merkmal: Die Klasse implementiert ein Ziel-Interface (`IExportierbar`), hält ein Objekt einer fremden Klasse (`LegacyKundenDatei`) als Feld und übersetzt in der Interface-Methode dessen Aufruf (`LeseAlleZeilen`) in die erwartete Form (`AlsJson`). Der Client, der `IExportierbar` verwendet, fehlt im Ausschnitt. Hier ist Vorsicht angebracht: Dieselbe Struktur – Interface plus Feld plus Weiterleitung – hat auch ein *Decorator* oder ein *Proxy*. Der Unterschied liegt darin, dass beim Adapter das Feld einen *anderen* Typ als das Interface hat (`LegacyKundenDatei` ist kein `IExportierbar`), während Decorator und Proxy dasselbe Interface wie ihr inneres Objekt implementieren. Der Typunterschied ist das entscheidende Indiz.

**Schritt 4 — Ausschnitt D: Iterator.**

Merkmal: `yield return` in einer Methode mit Rückgabetyp `IEnumerable<T>`. Das ist ein *spezifischer Iterator* wie `Bereich(2, 7)` aus dem [Iterator-Modul](/modules/iterator/iterator.md): Der Client durchläuft die knappen Artikel per `foreach`, ohne zu wissen, dass dahinter ein Dictionary steckt. Die Enumerator-Klasse erzeugt der Compiler. Bemerkenswert ist, was *nicht* da ist: `Lager` implementiert `IEnumerable<T>` nicht – ein Iterator muss nicht die ganze Klasse durchlaufbar machen.

**Zentrale Designentscheidungen:**

- **Struktur schlägt Namen:** Kein Ausschnitt enthält das Wort „Adapter“ oder „Composite“ im Klassennamen. Muster erkennt man an den Rollen und Beziehungen, nicht an der Benennung.
- **Ähnliche Strukturen, verschiedene Muster:** Adapter, Decorator und Proxy sehen auf den ersten Blick gleich aus. Erst die Frage „welchen Typ hat das innere Objekt?“ trennt sie.
- **Der Kontext gehört dazu:** Ein Muster besteht aus mehreren Rollen. Wer nur eine Klasse sieht, sollte benennen, welche Rollen er voraussetzt.

</details>

## Aufgabe 2 — Abstraktion

Entwirf ein Modell für ein Dateisystem. Es gibt Dateien mit einem Namen und einer Größe in Bytes und Ordner, die Dateien und weitere Ordner enthalten. Für jeden Eintrag soll sich die Größe abfragen lassen – bei einem Ordner ist das die Summe aller enthaltenen Einträge, beliebig tief. Außerdem soll sich jeder Eintrag eingerückt ausgeben lassen, so dass die Baumstruktur sichtbar wird.

- Welches Muster passt, und welche Rollen übernehmen Datei und Ordner?
- Was gehört in die gemeinsame Schnittstelle, was nur in den Ordner?
- Wie wird `Groesse()` für einen Ordner berechnet, ohne dass der Ordner wissen muss, wie tief er ist?
- Was liefert ein leerer Ordner, und was passiert, wenn ein Ordner sich selbst enthält?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Muster wählen:**

Einzelobjekte (Dateien) und Gruppen (Ordner) sollen über dieselben Operationen `Groesse()` und `Ausgeben()` angesprochen werden, und Ordner können Ordner enthalten – eine rekursive Baumstruktur. Das ist das Composite-Muster: `IEintrag` ist die Komponente, `Datei` das Blatt, `Ordner` das Kompositum.

**Schritt 2 — Schnittstelle festlegen:**

In das Interface gehört nur, was *jeder* Eintrag sinnvoll kann. `Hinzufuegen` gehört nicht dazu – eine Datei kann nichts aufnehmen, und eine Methode, die bei Dateien immer eine Exception wirft, würde das Substitutionsprinzip verletzen.

```csharp
public interface IEintrag
{
    string Name { get; }
    long Groesse();
    void Ausgeben(int einrueckung);
}
```

**Schritt 3 — Blatt:**

Die Datei kennt ihre Größe direkt. Die Einrückung ist ein Parameter, damit der Aufrufer – der Ordner – die Tiefe vorgibt.

```csharp
public class Datei : IEintrag
{
    private readonly long bytes;

    public string Name { get; }

    public Datei(string name, long bytes)
    {
        Name = name;
        this.bytes = bytes;
    }

    public long Groesse() => bytes;

    public void Ausgeben(int einrueckung)
        => Console.WriteLine($"{new string(' ', einrueckung)}{Name} ({bytes} B)");
}
```

**Schritt 4 — Kompositum mit rekursiver Größe:**

Der Ordner summiert die Größen seiner Einträge. Ob ein Eintrag eine Datei oder wieder ein Ordner ist, spielt keine Rolle: `eintrag.Groesse()` ruft im zweiten Fall dieselbe Methode eine Ebene tiefer auf. Die Rekursion endet bei den Dateien von selbst.

```csharp
public class Ordner : IEintrag
{
    private readonly List<IEintrag> eintraege = new();

    public string Name { get; }

    public Ordner(string name) => Name = name;

    public void Hinzufuegen(IEintrag eintrag)
    {
        if (ReferenceEquals(eintrag, this))
            throw new ArgumentException("Ein Ordner kann sich nicht selbst enthalten.");
        eintraege.Add(eintrag);
    }

    public long Groesse() => eintraege.Sum(e => e.Groesse());

    public void Ausgeben(int einrueckung)
    {
        Console.WriteLine($"{new string(' ', einrueckung)}{Name}/ ({Groesse()} B)");
        foreach (IEintrag e in eintraege)
            e.Ausgeben(einrueckung + 2);
    }
}
```

**Schritt 5 — Verwenden:**

```csharp
Ordner projekt = new("projekt");
Ordner src = new("src");
src.Hinzufuegen(new Datei("Figur.cs", 1200));
src.Hinzufuegen(new Datei("Kreis.cs", 480));
projekt.Hinzufuegen(src);
projekt.Hinzufuegen(new Datei("README.md", 300));

projekt.Ausgeben(0);
// projekt/ (1980 B)
//   src/ (1680 B)
//     Figur.cs (1200 B)
//     Kreis.cs (480 B)
//   README.md (300 B)
```

Ein leerer Ordner liefert `Sum` über eine leere Liste, also `0` – ohne Sonderfall im Code.

**Zentrale Designentscheidungen:**

- **`Hinzufuegen` nur im Ordner:** Das Interface bleibt auf die Operationen beschränkt, die für Blatt und Kompositum gleichermaßen gelten.
- **Rekursion ohne Tiefenwissen:** `Ordner.Groesse()` kennt nur seine direkten Kinder. Die Gesamttiefe ergibt sich aus der Verschachtelung der Aufrufe, nicht aus einer Schleife über alle Ebenen.
- **Zyklen abfangen:** Der Selbstbezug ist der einfachste Zyklus; `Ausgeben` würde sonst endlos laufen. Indirekte Zyklen (A enthält B enthält A) fängt die Prüfung nicht – in einem echten Dateisystem ist das das Problem symbolischer Links.
- **`long` statt `int`:** Dateigrößen überschreiten 2 GB schnell; die Summe eines Ordners erst recht.

</details>

## Aufgabe 3 — Algorithmenentwurf

Die folgende Iterator-Methode arbeitet mit den Figuren des Geometrieeditors (`Rechteck` und `Kreis` aus `examples/04_blazor/Geometrieeditor`). Sie soll nur Figuren liefern, deren Fläche mindestens `minFlaeche` ist – und von diesen nur jede zweite.

```csharp
static IEnumerable<Figur> GrosseJedeZweite(List<Figur> figuren, double minFlaeche)
{
    int treffer = 0;
    foreach (Figur f in figuren)
    {
        Console.WriteLine($"  pruefe {f.Name}");
        if (f.Flaeche < minFlaeche)
            continue;
        treffer++;
        if (treffer % 2 == 0)
            yield return f;
    }
}

List<Figur> figuren =
[
    new Rechteck("R1", 0, 0, 2, 3),
    new Kreis("K1", 0, 0, 1),
    new Rechteck("R2", 0, 0, 4, 4),
    new Kreis("K2", 0, 0, 2),
    new Rechteck("R3", 0, 0, 1, 1),
];

IEnumerable<Figur> auswahl = GrosseJedeZweite(figuren, 5);
Console.WriteLine("Auswahl definiert");
figuren.Add(new Kreis("K3", 0, 0, 3));

foreach (Figur f in auswahl)
    Console.WriteLine(f.Name);
```

- Sage die vollständige Konsolenausgabe voraus, Zeile für Zeile.
- Erscheint `K3` in der Ausgabe, obwohl es erst *nach* der Definition von `auswahl` hinzugefügt wurde?
- Was ändert sich, wenn die `foreach`-Schleife durch `Console.WriteLine(auswahl.First().Name)` ersetzt wird?
- Was passiert, wenn man direkt hinter der Schleife noch `Console.WriteLine(auswahl.Count())` aufruft?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Flächen berechnen:**

| Figur | Fläche | ≥ 5? |
| :--- | :--- | :--- |
| R1 (2 × 3) | 6 | ja |
| K1 (r = 1) | 3,14 | nein |
| R2 (4 × 4) | 16 | ja |
| K2 (r = 2) | 12,57 | ja |
| R3 (1 × 1) | 1 | nein |
| K3 (r = 3) | 28,27 | ja |

**Schritt 2 — Verzögerte Ausführung erkennen:**

`GrosseJedeZweite(figuren, 5)` führt *keine* Zeile des Rumpfs aus – die Methode enthält `yield`, also liefert sie nur ein Iterator-Objekt, das eine Referenz auf die Liste hält. Deshalb erscheint „Auswahl definiert“ als erste Zeile, ohne dass vorher „pruefe“ ausgegeben wird. Und deshalb gehört `K3` zur Iteration: Als die Schleife den Iterator startet, hat die Liste sechs Elemente.

**Schritt 3 — Ausgabe Schritt für Schritt:**

Der Iterator läuft bei jedem `MoveNext()` bis zum nächsten `yield return`. Die Zählung `treffer` erhöht sich nur bei Figuren über der Grenze; geliefert wird bei geradem Zähler.

```
Auswahl definiert
  pruefe R1        → Treffer 1, kein yield
  pruefe K1        → zu klein
  pruefe R2        → Treffer 2, yield
R2
  pruefe K2        → Treffer 3, kein yield
  pruefe R3        → zu klein
  pruefe K3        → Treffer 4, yield
K3
```

Ohne die Kommentare rechts ist das die exakte Konsolenausgabe: neun Zeilen, davon zwei Figurennamen. Die Prüfzeilen und die Namen sind *verzahnt* – nicht erst alle Prüfungen, dann alle Namen. Das ist das elementweise Verhalten aus dem Modul [verzögerte Ausführung](/modules/linq_deferred_execution/linq_deferred_execution.md).

**Schritt 4 — `First()`:**

`First()` ruft `MoveNext()` genau einmal und bricht dann ab. Der Iterator läuft bis zum ersten `yield return` und nicht weiter:

```
Auswahl definiert
  pruefe R1
  pruefe K1
  pruefe R2
R2
```

`K2`, `R3` und `K3` werden nie geprüft. Das ist der praktische Nutzen der Faulheit: Bei einer Liste mit einer Million Figuren würde `First()` nach drei Prüfungen aufhören.

**Schritt 5 — `Count()` nach der Schleife:**

`Count()` muss alle Elemente zählen und startet dafür einen *neuen* Enumerator – `GetEnumerator()` wird erneut aufgerufen, `treffer` beginnt wieder bei 0. Der komplette Rumpf läuft ein zweites Mal, alle sechs „pruefe“-Zeilen erscheinen erneut, und dann steht `2` in der Konsole. Wer das Ergebnis mehrfach braucht, sollte es einmal mit `ToList()` einsammeln.

**Zentrale Designentscheidungen:**

- **Der Aufruf einer Iterator-Methode ist kostenlos:** Er erzeugt nur das Iterator-Objekt. Arbeit passiert erst beim Durchlaufen – und bei jedem Durchlaufen erneut.
- **Der Iterator sieht die Quelle zur Laufzeit:** Er hält eine Referenz auf `figuren`, keine Kopie. Änderungen *vor* dem Start sind sichtbar; Änderungen *während* der Iteration würden eine `InvalidOperationException` auslösen.
- **Zustand lebt im Iterator:** `treffer` ist eine lokale Variable, die der Compiler in ein Feld der erzeugten Enumerator-Klasse verwandelt. Jeder neue Enumerator hat seinen eigenen Zähler.

</details>

## Aufgabe 4 — Zerlegung

Eine Kollegin hat für den Datenbankzugriff ein Singleton geschrieben und benutzt es überall:

```csharp
public sealed class Datenbank
{
    private static readonly Lazy<Datenbank> halter = new(() => new Datenbank());
    public static Datenbank Instanz => halter.Value;

    private Datenbank()
    {
        // baut eine echte Verbindung zum Datenbankserver auf
    }

    public List<string> LadeKundennamen() { /* SQL ... */ return new(); }
    public void SpeichereKunde(string name) { /* SQL ... */ }
}

public class Kundenverwaltung
{
    public int AnzahlKunden() => Datenbank.Instanz.LadeKundennamen().Count;

    public void Anlegen(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name fehlt.");
        Datenbank.Instanz.SpeichereKunde(name);
    }
}
```

Jetzt soll `Kundenverwaltung` mit NUnit getestet werden – und jeder Test braucht plötzlich einen laufenden Datenbankserver. Ein Test, der prüft, dass `Anlegen("")` eine Exception wirft, funktioniert zwar, aber `AnzahlKunden()` liefert je nach Inhalt der echten Datenbank jedes Mal etwas anderes.

- Warum lässt sich `Kundenverwaltung` in diesem Zustand nicht sinnvoll testen? Was genau steht im Weg?
- Zerlege die Abhängigkeit: Welches Interface braucht `Kundenverwaltung`, und wer entscheidet, welche Implementierung dahintersteckt?
- Wie sieht ein Test aus, der ohne Datenbank auskommt?
- Ist `Datenbank` nach dem Umbau noch ein Singleton – und muss es das sein?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Das Problem benennen:**

`Kundenverwaltung` verrät in ihrer Signatur nicht, dass sie eine Datenbank braucht – die Abhängigkeit ist im Methodenrumpf über `Datenbank.Instanz` fest verdrahtet. Ein Test kann diesen Aufruf nicht abfangen: Es gibt keine Stelle, an der man etwas anderes hineinreichen könnte. Der private Konstruktor verhindert sogar, dass man eine Test-Datenbank selbst erzeugt. Das sind die beiden Singleton-Nachteile „versteckte Abhängigkeit“ und „schwer testbar“ aus dem [Singleton-Modul](/modules/singleton/singleton.md) in Reinform.

**Schritt 2 — Die Abhängigkeit hinter ein Interface legen:**

`Kundenverwaltung` braucht nicht *die Datenbank*, sondern *irgendetwas, das Kunden laden und speichern kann*. Genau das beschreibt ein Interface – dieselbe Idee wie `IFigurSpeicher` im Geometrieeditor:

```csharp
public interface IKundenSpeicher
{
    List<string> LadeKundennamen();
    void SpeichereKunde(string name);
}
```

**Schritt 3 — Die Implementierung übergeben statt holen:**

`Kundenverwaltung` bekommt den Speicher im Konstruktor und spricht nur noch über das Interface. Wer die Verwaltung erzeugt, entscheidet, was dahintersteckt:

```csharp
public class Kundenverwaltung
{
    private readonly IKundenSpeicher speicher;

    public Kundenverwaltung(IKundenSpeicher speicher)
    {
        this.speicher = speicher;
    }

    public int AnzahlKunden() => speicher.LadeKundennamen().Count;

    public void Anlegen(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name fehlt.");
        speicher.SpeichereKunde(name);
    }
}
```

Die echte `Datenbank` implementiert das Interface und bekommt einen öffentlichen Konstruktor. Die Anwendung erzeugt beim Start *eine* Instanz und reicht sie an alle weiter, die sie brauchen – so wie `Program.cs` im Geometrieeditor den `JsonFigurSpeicher` für die `FigurenVerwaltung` registriert:

```csharp
public sealed class Datenbank : IKundenSpeicher
{
    public Datenbank() { /* Verbindung aufbauen */ }
    public List<string> LadeKundennamen() { /* SQL ... */ return new(); }
    public void SpeichereKunde(string name) { /* SQL ... */ }
}

// beim Programmstart, einmal:
IKundenSpeicher speicher = new Datenbank();
Kundenverwaltung verwaltung = new(speicher);
```

**Schritt 4 — Ein Test ohne Datenbank:**

Für den Test schreibt man eine zweite Implementierung, die die Kunden nur in einer Liste hält – das Gegenstück zum `ArbeitsspeicherFigurSpeicher`:

```csharp
public class ArbeitsspeicherKundenSpeicher : IKundenSpeicher
{
    private readonly List<string> kunden = new();
    public List<string> LadeKundennamen() => new(kunden);
    public void SpeichereKunde(string name) => kunden.Add(name);
}

[Test]
public void Anlegen_ErhoehtAnzahlKunden()
{
    Kundenverwaltung verwaltung = new(new ArbeitsspeicherKundenSpeicher());

    verwaltung.Anlegen("Anna");
    verwaltung.Anlegen("Ben");

    Assert.That(verwaltung.AnzahlKunden(), Is.EqualTo(2));
}
```

Der Test läuft in Millisekunden, braucht keinen Server und liefert bei jedem Lauf dasselbe Ergebnis, weil jeder Test mit einem frischen, leeren Speicher beginnt. Mehr zu NUnit kommt in Vorlesung 12.

**Schritt 5 — Und das Singleton?**

Nach dem Umbau ist `Datenbank` eine gewöhnliche Klasse. Dass es zur Laufzeit nur eine Verbindung gibt, stellt der Programmstart sicher, indem er nur einmal `new Datenbank()` aufruft – das Muster war nie nötig, um „nur eine Instanz“ zu erreichen. Wenn eine zweite Verbindung wirklich *technisch* falsch wäre, kann `Datenbank` intern weiterhin ein Singleton bleiben; entscheidend ist, dass `Kundenverwaltung` davon nichts weiß und nur das Interface sieht.

**Zentrale Designentscheidungen:**

- **Abhängigkeiten in die Signatur:** Was eine Klasse braucht, steht im Konstruktor. Wer `Kundenverwaltung` liest, sieht sofort, dass sie einen `IKundenSpeicher` benötigt.
- **Interface im Fachkonzept, Implementierung außen:** `IKundenSpeicher` gehört zur Geschäftslogik, `Datenbank` zur Datenhaltung – dieselbe Abhängigkeitsrichtung wie in der [Schichtenarchitektur](/modules/schichten_architektur/schichten_architektur.md).
- **Einmaligkeit ist eine Entscheidung des Aufrufers:** Ob es eine oder zehn Instanzen gibt, entscheidet der Code, der `new` aufruft – nicht die Klasse selbst.
- **Testdoubles statt echter Infrastruktur:** Eine Arbeitsspeicher-Implementierung des Interfaces ist die einfachste Form eines Testdoubles und reicht für die meisten Tests aus.

</details>
