---
title: "🧩 Aufgaben und Beispiele: Delegaten, Lambdas und Ereignisse"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Programmieren lernt man nicht nur durch Codezeilen tippen – sondern auch durch **Nachdenken**. Die folgenden Aufgaben trainieren *Computational Thinking*: die Fähigkeit, Probleme so zu strukturieren, dass ein Computer sie lösen kann. Dazu gehören Abstraktion, Zerlegung, Mustererkennung und Algorithmenentwurf. Delegaten und Ereignisse sind dafür besonders geeignet, weil sie erlauben, das *Was* vom *Wer* zu trennen – und weil man bei Closures und Multicast-Delegaten sehr genau hinschauen muss, was zur Laufzeit wirklich passiert. Alle Aufgaben spielen im Adventure, dem durchgehenden Beispiel des Kurses – das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag [`v03-blazor`](https://github.com/erodner/prog2-adventure/tree/v03-blazor)). Nimm dir für jede Aufgabe Zeit, bevor du die Lösung aufklappst.

## Aufgabe 1 — Ausgabe vorhersagen

Sage die vollständige Ausgabe des folgenden Programms voraus, ohne es auszuführen. Vier Empfänger hängen am Ereignis `Spielfeld.RundeBeendet`, und zwischendurch wird mit `-=` wieder abgemeldet. Nimm an, dass jeder Aufruf von `SpielerZieht` tatsächlich eine Runde zu Ende spielt, `feld.Runde` also von 1 bis 3 hochzählt.

```csharp
Spielfeld feld = LevelParser.Parsen(level);
List<string> protokoll = new List<string>();

void Anzeigen(object? sender, RundeEventArgs e) => Console.WriteLine($"[anzeige] Runde {e.Runde}");
void Protokollieren(object? sender, RundeEventArgs e) => protokoll.Add(e.Meldung);

feld.RundeBeendet += Anzeigen;
feld.RundeBeendet += Protokollieren;
feld.RundeBeendet += Anzeigen;

int gezaehlt = 0;
feld.RundeBeendet += (sender, e) => Console.WriteLine($"[zaehler] {++gezaehlt}");

feld.SpielerZieht(Richtung.Rechts);

feld.RundeBeendet -= Anzeigen;
feld.SpielerZieht(Richtung.Unten);

for (int i = 0; i < 2; i++)
    feld.RundeBeendet += (sender, e) => Console.WriteLine($"[schleife] i = {i}");

feld.RundeBeendet -= (sender, e) => Console.WriteLine($"[zaehler] {++gezaehlt}");
feld.SpielerZieht(Richtung.Links);

Console.WriteLine($"[protokoll] {protokoll.Count}");
```

Leitfragen:
- In welcher Reihenfolge werden die Methoden eines Multicast-Delegaten aufgerufen?
- Was entfernt `-=`, wenn dieselbe Methode zweimal in der Liste steht?
- Welchen Wert hat `i`, wenn die beiden Lambdas der `for`-Schleife *ausgeführt* werden?
- Warum meldet das `-=` mit dem Lambda nichts ab, obwohl der Code buchstabengleich aussieht?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Die Aufrufliste nach den vier Registrierungen:**

Die Liste lautet `[Anzeigen, Protokollieren, Anzeigen, Lambda-Zähler]` und wird in genau dieser Reihenfolge abgearbeitet. Runde 1 ergibt also:

```
[anzeige] Runde 1
[anzeige] Runde 1
[zaehler] 1
```

`Protokollieren` schreibt nur in die Liste und gibt nichts aus – man sieht es erst am Ende an `protokoll.Count`.

**Schritt 2 — `-=` entfernt das letzte Vorkommen:**

`feld.RundeBeendet -= Anzeigen` sucht das *letzte* Vorkommen von `Anzeigen` und entfernt genau dieses eine. Die Liste ist jetzt `[Anzeigen, Protokollieren, Lambda-Zähler]`, und Runde 2 gibt aus:

```
[anzeige] Runde 2
[zaehler] 2
```

**Schritt 3 — Die `for`-Schleife teilt eine Variable:**

Die beiden Lambdas fangen nicht den Wert von `i` ein, sondern die Variable selbst. Die `for`-Schleife hat nur *eine* Variable `i` für alle Durchläufe; nach dem letzten Durchlauf wurde `i++` ausgeführt und die Bedingung `i < 2` verletzt – `i` ist 2. Erst danach wird das Ereignis ausgelöst.

**Schritt 4 — Das wirkungslose `-=`:**

Jedes hingeschriebene Lambda erzeugt ein *neues* Delegatobjekt. Das Lambda in der `-=`-Zeile ist nicht dasselbe Objekt wie das beim `+=` registrierte, auch wenn es Zeichen für Zeichen gleich aussieht – `-=` findet nichts und tut nichts. Der Zähler bleibt also in der Liste. Runde 3 ergibt damit:

```
[anzeige] Runde 3
[zaehler] 3
[schleife] i = 2
[schleife] i = 2
```

**Schritt 5 — Das Protokoll:**

`Protokollieren` war bei allen drei Runden dabei und wurde nie abgemeldet:

```
[protokoll] 3
```

**Zentrale Designentscheidungen:**

- **Closures fangen Variablen ein, keine Werte:** Wer einen Wert festhalten will, muss ihn innerhalb des Schleifenrumpfs in eine neue lokale Variable kopieren (`int kopie = i;`). Bei `foreach` ist das seit C# 5 nicht nötig.
- **`-=` ist kein „alle entfernen“:** Hat man eine Methode mehrfach registriert – ein häufiger Fehler, wenn `+=` in einer Methode steht, die mehrfach aufgerufen wird –, muss man sie ebenso oft abmelden. Genau das passiert in der Konsolenversion ab [Vorlesung 09](/lectures/09/09.md) beinahe, wenn nach `F9` erneut `SchatzGefunden += …` ausgeführt wird; dort ist es nur deshalb korrekt, weil `Spielfeld.Wiederherstellen` einen *neuen* Spieler liefert.
- **Wer sich abmelden will, braucht eine benannte Methode:** Ein anonym hingeschriebenes Lambda lässt sich nicht wiederfinden. Alternativ merkt man sich den Delegaten in einer Variablen und meldet diese wieder ab.
- **Der Sender bemerkt von alldem nichts:** `Spielfeld.SpielerZieht` enthält genau eine Zeile mit `RundeBeendet?.Invoke(...)` – ob dahinter null, ein oder fünf Empfänger stehen, ändert den Sender nicht.

</details>

## Aufgabe 2 — Abstraktion

Die Lebenspunkte des Helden ändern sich an mehreren Stellen: `SchadenNehmen` bei jeder Berührung durch eine Wache oder einen Verfolger, `Heilen` beim Trinken eines Tranks, `Wiederherstellen` beim Laden eines Spielstands. Drei Empfänger sollen darauf reagieren: die **Statusleiste** der Weboberfläche (zeichnet die Herzen neu), die **Konsole** (blinkt rot auf, wenn Schaden genommen wurde) und ein **Heiler-Hinweis**, der nur dann etwas sagt, wenn nach dem Treffer noch genau ein Lebenspunkt übrig ist.

Entwirf dafür ein Ereignis `LebenspunkteGeaendert` in der Klasse `Spieler`:
- Welche Daten muss das Ereignis transportieren? Wie sieht die `EventArgs`-Klasse aus?
- Soll das Ereignis bei *jedem* Aufruf von `SchadenNehmen` ausgelöst werden oder nur, wenn sich der Wert tatsächlich geändert hat? Was bedeutet die Entscheidung für die Statusleiste?
- Wo gehört die „genau ein Lebenspunkt übrig“-Regel hin: in den Spieler oder in den Empfänger?
- Muss `Wiederherstellen` das Ereignis ebenfalls auslösen?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Die Ereignisdaten:**

Die Empfänger brauchen den alten und den neuen Wert: Die Statusleiste zeichnet mit dem neuen, die Konsole entscheidet an der Differenz, ob es Schaden oder Heilung war. Das Maximum kommt dazu, damit niemand `Spieler.MaxLebenspunkte` von außen nachschlagen muss. Alle Properties sind nur lesbar:

```csharp
public class LebenspunkteEventArgs : EventArgs
{
    public int Vorher { get; }
    public int Nachher { get; }
    public int Maximum { get; }

    public LebenspunkteEventArgs(int vorher, int nachher, int maximum)
    {
        Vorher = vorher;
        Nachher = nachher;
        Maximum = maximum;
    }

    public int Differenz => Nachher - Vorher;
    public bool IstSchaden => Differenz < 0;
}
```

`Differenz` und `IstSchaden` sind berechnete Properties – sie speichern nichts, sondern ersparen jedem Empfänger dieselbe Rechnung.

**Schritt 2 — Der Sender:**

Alle drei Stellen führen über eine einzige private Methode, die den Wert setzt und meldet. Ausgelöst wird nur, wenn sich wirklich etwas geändert hat: Ein Treffer bei null Lebenspunkten oder ein Trank bei vollem Leben ändert nichts, und die Statusleiste würde sonst ohne Anlass neu zeichnen.

```csharp
public class Spieler : BeweglichesObjekt
{
    public const int MaxLebenspunkte = 3;

    public int Lebenspunkte { get; private set; } = MaxLebenspunkte;

    public event EventHandler<LebenspunkteEventArgs>? LebenspunkteGeaendert;

    private void LebenspunkteSetzen(int neu)
    {
        int alt = Lebenspunkte;
        if (neu == alt) return;                       // keine Änderung, keine Meldung
        Lebenspunkte = neu;
        LebenspunkteGeaendert?.Invoke(this,
            new LebenspunkteEventArgs(alt, neu, MaxLebenspunkte));
    }

    public void SchadenNehmen(int schaden = 1)
        => LebenspunkteSetzen(Math.Max(0, Lebenspunkte - schaden));

    public void Heilen(int heilung)
        => LebenspunkteSetzen(Math.Min(MaxLebenspunkte, Lebenspunkte + heilung));

    public void Wiederherstellen(int lebenspunkte, int punkte)
    {
        LebenspunkteSetzen(Math.Clamp(lebenspunkte, 0, MaxLebenspunkte));
        Punkte = punkte;
    }
}
```

**Schritt 3 — Die drei Empfänger:**

Jeder Empfänger bringt seine eigene Regel mit; der Spieler kennt keinen von ihnen:

```csharp
// Adventure.Konsole
feld.Spieler.LebenspunkteGeaendert += (sender, e) =>
{
    if (e.IstSchaden) Console.Beep(220, 150);
};

// nur ein Lebenspunkt übrig – eigener Empfänger, eigene Regel
feld.Spieler.LebenspunkteGeaendert += (sender, e) =>
{
    if (e.Nachher == 1) Console.WriteLine("Ein Trank wäre jetzt eine gute Idee.");
};

// Adventure.Web: Statusleiste neu zeichnen
spieler.LebenspunkteGeaendert += (sender, e) => InvokeAsync(StateHasChanged);
```

**Schritt 4 — `Wiederherstellen`:**

Ja, auch das Laden eines Spielstands soll melden – sonst zeigt die Statusleiste nach `F9` noch die Herzen des alten Spielstands. In der Konsolenversion fällt das nicht auf, weil dort ohnehin ein neues `Spielfeld` samt neuem `Spieler` entsteht; in Blazor, wo dieselbe Komponente weiterlebt, wäre es ein sichtbarer Fehler.

**Zentrale Designentscheidungen:**

- **Eine einzige Auslösestelle:** `SchadenNehmen`, `Heilen` und `Wiederherstellen` gehen über `LebenspunkteSetzen`. Wer später eine vierte Schadensquelle ergänzt (eine Falle), bekommt das Ereignis geschenkt und kann es nicht vergessen.
- **Melden nach dem Ändern:** Erst `Lebenspunkte = neu`, dann `Invoke`. Ein Empfänger, der über `sender` den Spieler befragt, sieht den neuen Zustand – konsistent mit `e.Nachher`.
- **Nur bei echter Änderung:** Das spart der Oberfläche nutzlose Renderdurchläufe. Wer eine Live-Anzeige „Treffer!“ braucht, nimmt dafür ein zweites Ereignis, statt dieses aufzuweichen.
- **Empfängerspezifische Regeln beim Empfänger:** Die „genau ein Lebenspunkt“-Regel gehört nicht in `Spieler`. Sonst wäre der Kern wieder mit einer konkreten Anzeige verheiratet – dasselbe Problem wie beim Sturzsensor.

</details>

## Aufgabe 3 — Zerlegung

Für diese Aufgabe bauen wir ein kleines Spielfeld von Hand auf, damit sich jedes Ergebnis auf dem Papier nachrechnen lässt. Der Held steht auf `(6, 3)`, um ihn herum liegen sechs statische Objekte und drei Gegner:

```csharp
Spielfeld feld = new Spielfeld(14, 7, new Spieler("Held", new Koordinate(6, 3)));
feld.Hinzufuegen(new Wand(new Koordinate(0, 3)));
feld.Hinzufuegen(new Wand(new Koordinate(13, 3)));
feld.Hinzufuegen(new Schluessel(new Koordinate(5, 1)));
feld.Hinzufuegen(new Trank(new Koordinate(9, 5)));
feld.Hinzufuegen(new Schatz(new Koordinate(2, 2), wert: 25));
feld.Hinzufuegen(new Truhe(new Koordinate(11, 1), wert: 100));
feld.Hinzufuegen(new Wache(new Koordinate(8, 3)));
feld.Hinzufuegen(new Verfolger(new Koordinate(6, 5)));
feld.Hinzufuegen(new Wache(new Koordinate(1, 5)));

Koordinate held = feld.Spieler.Position;   // (6, 3)
```

Alle Entfernungen sind Manhattan-Entfernungen aus `Koordinate.Entfernung`, also Schritte ohne Diagonalen. Rechne sie einmal für alle Objekte aus, bevor du weiterliest.

Zerlege jede der folgenden Fragen in ihre Bestandteile – *Quelle*, *Filter*, *Ordnung*, *Ergebnisform* – und schreibe sie als LINQ-Methodenkette. Sage dann die Ausgabe voraus, **bevor** du den Code startest.

- (a) Die Namen aller Gegner, die höchstens vier Schritte entfernt sind, der nächste zuerst. Bei gleicher Entfernung soll der Name entscheiden. Welche Quelle nimmst du – `feld.Gegner` oder `feld.AlleObjekte`?
- (b) Alle Gegenstände, die noch auf dem Boden liegen, als Text `"Name (x, y)"`, der entfernteste zuerst, bei gleicher Entfernung alphabetisch. Achtung: Die `Truhe` ist kein `Gegenstand` – warum eigentlich nicht?
- (c) Eine Gruppierung **aller** Objekte nach ihrem `Symbol()` mit der Anzahl je Gruppe. Wie viele Gruppen entstehen, und welchen Typ hat `gruppe.Key`? Sortiere die Gruppen anschließend nach Symbol und erkläre die Reihenfolge, die dabei herauskommt.

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 0 — Die Entfernungen:**

| Objekt | Position | Entfernung zu `(6, 3)` |
| :--- | :--- | :--- |
| Wache | `(8, 3)` | 2 + 0 = 2 |
| Verfolger | `(6, 5)` | 0 + 2 = 2 |
| Wache | `(1, 5)` | 5 + 2 = 7 |
| Schlüssel | `(5, 1)` | 1 + 2 = 3 |
| Trank | `(9, 5)` | 3 + 2 = 5 |
| Schatz | `(2, 2)` | 4 + 1 = 5 |

**Schritt 1 — Abfrage (a):**

Quelle `feld.Gegner` (nicht `AlleObjekte`, denn wir wollen ausschließlich Gegner, und `feld.Gegner` liefert sie schon mit dem richtigen Typ), Filter „Entfernung höchstens 4“, Ordnung nach Entfernung und dann `Name`, Ergebnis nur der `Name`. Das zweite Sortierkriterium hängt man mit `ThenBy` an:

```csharp
IEnumerable<string> nah = feld.Gegner
    .Where(g => g.Position.Entfernung(held) <= 4)
    .OrderBy(g => g.Position.Entfernung(held))
    .ThenBy(g => g.Name)
    .Select(g => g.Name);

Console.WriteLine(string.Join(", ", nah));
// Verfolger, Wache
```

Die zweite Wache fällt mit sieben Schritten aus dem Filter. Die beiden übrigen sind gleich weit entfernt, deshalb entscheidet `ThenBy`, und „Verfolger“ steht alphabetisch vor „Wache“. Weil `Select` einen `string` liefert, ist das Ergebnis ein `IEnumerable<string>`.

**Schritt 2 — Abfrage (b) mit absteigender Ordnung:**

Gegenstände liegen bei den statischen Objekten und haben keine eigene Property am Spielfeld, also ist die Quelle `feld.AlleObjekte`. `OfType<Gegenstand>()` filtert und liefert zugleich den passenden Typ:

```csharp
IEnumerable<string> amBoden = feld.AlleObjekte
    .OfType<Gegenstand>()
    .OrderByDescending(o => o.Position.Entfernung(held))
    .ThenBy(o => o.Name)
    .Select(o => $"{o.Name} {o.Position}");

foreach (string zeile in amBoden)
{
    Console.WriteLine(zeile);
}
// Schatz (2, 2)
// Trank (9, 5)
// Schlüssel (5, 1)
```

Schatz und Trank sind beide fünf Schritte entfernt, alphabetisch kommt „Schatz“ zuerst. Die `Truhe` fehlt zu Recht: Sie erbt von `StatischesObjekt`, nicht von `Gegenstand`, denn man hebt sie nicht auf, sondern öffnet sie mit `Interagieren`.

**Schritt 3 — Abfrage (c) gruppieren:**

```csharp
foreach (var gruppe in feld.AlleObjekte.GroupBy(o => o.Symbol()).OrderBy(gruppe => gruppe.Key))
{
    Console.WriteLine($"{gruppe.Key}: {gruppe.Count()}");
}
// !: 1
// #: 2
// $: 1
// @: 1
// T: 1
// V: 1
// W: 2
// k: 1
```

Acht Gruppen, der `Key` ist ein `char`, weil `Symbol()` ein `char` ist. Warum steht das kleine `k` hinter dem großen `W`? Weil `char` nach seinem Zahlencode sortiert wird: `!` ist 33, `#` 35, `$` 36, `@` 64, die Großbuchstaben liegen bei 65 bis 90, die Kleinbuchstaben erst bei 97 bis 122. Ohne `OrderBy` kämen die Gruppen in der Reihenfolge, in der ihr Symbol zum ersten Mal auftaucht, und darauf sollte man sich nicht verlassen.

**Zentrale Designentscheidungen:**

- **Frage zuerst zerlegen, dann tippen:** Jede Methode beantwortet genau einen Teil der Frage – `Where` das „welche“, `OrderBy`/`ThenBy` das „in welcher Reihenfolge“, `Select` das „in welcher Form“.
- **Die engste passende Quelle wählen:** `feld.Gegner` statt `feld.AlleObjekte` spart die Typprüfung und liefert gleich den richtigen Typ. `AlleObjekte` ist erst dann richtig, wenn es keine speziellere Quelle gibt.
- **Sortierkriterien vollständig angeben:** Bei zwei gleich weit entfernten Wachen ist Gleichstand der Normalfall. `ThenBy` macht die Absicht explizit.

</details>

## Aufgabe 4 — Mustererkennung

Die Konsolenversion des Spiels übersetzt Tastendrücke mit einem `switch`-Ausdruck in eine Richtung. Jede zusätzliche Belegung erfordert einen weiteren Zweig – und der Spieler kann nur, was zur Kompilierzeit dort steht:

```csharp
ConsoleKey taste = Console.ReadKey(true).Key;

Richtung? richtung = taste switch
{
    ConsoleKey.W or ConsoleKey.UpArrow => Richtung.Oben,
    ConsoleKey.S or ConsoleKey.DownArrow => Richtung.Unten,
    ConsoleKey.A or ConsoleKey.LeftArrow => Richtung.Links,
    ConsoleKey.D or ConsoleKey.RightArrow => Richtung.Rechts,
    _ => null
};
```

Erkenne das Muster, das sich in allen Zweigen wiederholt, und ersetze den `switch` durch ein `Dictionary<ConsoleKey, Richtung>`.
- Was haben alle Zweige gemeinsam – welche Abbildung steckt dahinter?
- Wie kommt der Fall „unbekannte Taste“ ohne `default` aus?
- Wie kann jemand zur Laufzeit eine zweite Tastenbelegung ergänzen (etwa `h j k l`), ohne die Spielschleife zu ändern?
- In Vorlesung 09 kommen F5 (speichern) und F9 (laden) dazu; die liefern keine Richtung. Wie passt das in dieselbe Tabelle – und sollte es das?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Das Muster erkennen:**

Jeder Zweig bildet eine Taste auf eine Richtung ab, sonst nichts. Der `switch` ist also keine Fallunterscheidung, sondern eine **Zuordnung** von Schlüssel zu Wert – und dafür gibt es eine Datenstruktur statt einer Kontrollstruktur: das `Dictionary`, das wir im [Collections-Überblick](/modules/collections_ueberblick/collections_ueberblick.md) kennengelernt haben.

**Schritt 2 — Die Tabelle:**

```csharp
class Tastenbelegung
{
    private readonly Dictionary<ConsoleKey, Richtung> belegung = new()
    {
        [ConsoleKey.W] = Richtung.Oben,
        [ConsoleKey.UpArrow] = Richtung.Oben,
        [ConsoleKey.S] = Richtung.Unten,
        [ConsoleKey.DownArrow] = Richtung.Unten,
        [ConsoleKey.A] = Richtung.Links,
        [ConsoleKey.LeftArrow] = Richtung.Links,
        [ConsoleKey.D] = Richtung.Rechts,
        [ConsoleKey.RightArrow] = Richtung.Rechts,
    };

    public IEnumerable<ConsoleKey> BelegteTasten => belegung.Keys;

    public void Belegen(ConsoleKey taste, Richtung richtung) => belegung[taste] = richtung;

    public Richtung? RichtungFuer(ConsoleKey taste)
        => belegung.TryGetValue(taste, out Richtung r) ? r : null;
}
```

`TryGetValue` ersetzt den `default`-Zweig: Steht die Taste nicht in der Tabelle, ist das Ergebnis `null`, und die Spielschleife ignoriert den Tastendruck wie bisher. Der Indexer wäre hier falsch – er würde bei jeder unbelegten Taste eine `KeyNotFoundException` werfen, und das passiert im Spiel ständig.

**Schritt 3 — Die Spielschleife wird kürzer:**

```csharp
Tastenbelegung tasten = new();
tasten.Belegen(ConsoleKey.K, Richtung.Oben);      // Vi-Tasten, zur Laufzeit ergänzt
tasten.Belegen(ConsoleKey.J, Richtung.Unten);
tasten.Belegen(ConsoleKey.H, Richtung.Links);
tasten.Belegen(ConsoleKey.L, Richtung.Rechts);

if (tasten.RichtungFuer(Console.ReadKey(true).Key) is Richtung r)
{
    feld.SpielerZieht(r);
}
```

An `Tastenbelegung` wurde für die vier neuen Tasten keine Zeile geändert. Ein `switch` hätte das nicht erlaubt – und eine Belegung aus einer Konfigurationsdatei wäre damit endgültig unmöglich gewesen.

**Schritt 4 — F5 und F9:**

Diese Tasten liefern keine Richtung, sondern führen eine Aktion aus – im Spielstand-Modul der [Vorlesung 09](/lectures/09/09.md) speichern und laden sie. Man *kann* sie in dieselbe Tabelle legen, wenn man den Werttyp verallgemeinert – dann ist es ein `Dictionary<ConsoleKey, Action<Spielfeld>>`, und auch das Ziehen wird zu einer Aktion:

```csharp
Dictionary<ConsoleKey, Action<Spielfeld>> befehle = new()
{
    [ConsoleKey.UpArrow] = f => f.SpielerZieht(Richtung.Oben),
    [ConsoleKey.F5] = f => speicher.Speichern(f.Erfassen(levelName, level)),
};
```

Ob das besser ist, hängt vom Ziel ab. Die `Action`-Tabelle ist mächtiger, aber sie verliert die Information, *welche* Richtung eine Taste bedeutet – für eine Hilfeanzeige oder eine Tastenkonfiguration im Menü wäre die erste Variante wertvoller. Zwei kleine Tabellen mit klarer Bedeutung sind hier besser als eine große, die alles kann.

**Zentrale Designentscheidungen:**

- **Daten statt Kontrollfluss:** Eine Zuordnung „Schlüssel → Wert“ ist eine Tabelle, kein Verzweigungsbaum. Sobald ein `switch` in jedem Zweig dasselbe Muster hat, ist das ein Signal.
- **Offen für Erweiterung, geschlossen für Änderung:** Neue Belegungen kommen über `Belegen` hinzu; der Code in `RichtungFuer` bleibt unverändert. Das ist dasselbe Prinzip wie bei Ereignissen – der Sender kennt seine Empfänger nicht.
- **Das `Dictionary` ist `private`:** Von außen gibt es nur `Belegen`, `RichtungFuer` und die Liste der Tasten. Niemand kann versehentlich `belegung.Clear()` aufrufen – dieselbe Überlegung wie beim `event`-Schlüsselwort gegenüber einem öffentlichen Delegatfeld.
- **Blazor bekommt dieselbe Behandlung:** In `Home.razor` steht derselbe `switch`, nur über `KeyboardEventArgs.Key` (einem `string`). Dass es zwei Tabellen für zwei Eingabearten braucht, ist kein Zufall – daraus wird in Vorlesung 08 das [Adapter-Muster](/modules/adapter/adapter.md).

</details>
