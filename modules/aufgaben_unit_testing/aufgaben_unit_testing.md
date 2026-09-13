---
title: "🧩 Aufgaben und Beispiele: Unit-Testing"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Programmieren lernt man nicht nur durch Codezeilen tippen – sondern auch durch **Nachdenken**. Die folgenden Aufgaben trainieren *Computational Thinking*: die Fähigkeit, Probleme so zu strukturieren, dass ein Computer sie lösen kann. Beim Testen heißt das vor allem: systematisch überlegen, welche Fälle es gibt, Fehlermuster in fremdem Code erkennen und Klassen so entwerfen, dass sie sich überhaupt testen lassen. Alle Aufgaben arbeiten am Adventure aus dem Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v12-tests`). Nimm dir für jede Aufgabe Zeit, bevor du die Lösung aufklappst.

## Aufgabe 1 — Zerlegung

`Tuer.Interagieren(Spieler)` ist eine kurze Methode, aber sie trifft mehrere Entscheidungen hintereinander:

```csharp
public string Interagieren(Spieler spieler)
{
    if (IstOffen) return "Die Tür ist schon offen.";
    if (!spieler.Inventar.Enthaelt<Schluessel>()) return "Die Tür ist verschlossen. Du brauchst einen Schlüssel.";
    spieler.Inventar.Entfernen<Schluessel>();
    Aufschliessen();
    return "Du schließt die Tür auf.";
}
```

Leite **systematisch** Testfälle ab, statt drauflos zu testen: Zerlege die Methode in ihre Zweige und überlege pro Zweig, *was* sich ändern muss – nicht nur, was zurückkommt.

- Wie viele Zweige hat die Methode, und welcher Zustand entscheidet jeweils?
- Welche Wirkungen hat ein Aufruf außer dem Rückgabewert – und wo kann man sie ablesen?
- Was muss gelten, wenn der Held mit *einem* Schlüssel vor der *zweiten* Tür steht?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Zweige und Wirkungen als Tabelle:**

Jedes `if` ist eine Verzweigung, also gibt es drei Wege durch die Methode. Für jeden notiert man nicht nur die Meldung, sondern auch die beiden Zustände, die sich ändern können: `IstOffen` der Tür und der Inhalt des Inventars.

| Vorbedingung | Meldung enthält | `IstOffen` danach | Inventar danach |
| :--- | :--- | :--- | :--- |
| Tür schon offen | „schon offen“ | `true` (unverändert) | unverändert |
| Tür zu, kein Schlüssel | „Schlüssel“ | `false` | unverändert (leer) |
| Tür zu, Schlüssel vorhanden | „schließt … auf“ | `true` | ein Schlüssel weniger |
| Tür zu, zwei Schlüssel | „schließt … auf“ | `true` | genau einer verbraucht |

Die dritte Spalte ist der Teil, den man leicht vergisst: „Wirft die richtige Meldung zurück“ ist nur ein Drittel der Zusage.

**Schritt 2 — Die Tests dazu:**

`Interagieren` braucht kein Spielfeld – nur eine Tür und einen Spieler. Das macht diese Tests besonders kurz:

```csharp
private static (Tuer tuer, Spieler held) Aufbau(int schluessel = 0)
{
    Spieler held = new Spieler("Held", new Position(0, 0));
    for (int i = 0; i < schluessel; i++) held.Inventar.Hinzufuegen(new Schluessel(new Position(0, 0)));
    return (new Tuer(new Position(1, 0)), held);
}

[Test] public void Interagieren_OhneSchluessel_BleibtZuUndVerbrauchtNichts()
{
    (Tuer tuer, Spieler held) = Aufbau();

    string meldung = tuer.Interagieren(held);

    Assert.Multiple(() =>
    {
        Assert.That(meldung, Does.Contain("Schlüssel"));
        Assert.That(tuer.IstOffen, Is.False);
        Assert.That(held.Inventar.Anzahl, Is.EqualTo(0));
    });
}

[Test] public void Interagieren_MitSchluessel_OeffnetUndVerbrauchtGenauEinen()
{
    (Tuer tuer, Spieler held) = Aufbau(schluessel: 2);

    tuer.Interagieren(held);

    Assert.That(tuer.IstOffen, Is.True);
    Assert.That(held.Inventar.Anzahl, Is.EqualTo(1));
}

[Test] public void Interagieren_SchonOffen_AendertNichts()
{
    (Tuer tuer, Spieler held) = Aufbau(schluessel: 1);
    tuer.Aufschliessen();

    string meldung = tuer.Interagieren(held);

    Assert.That(meldung, Does.Contain("schon offen"));
    Assert.That(held.Inventar.Anzahl, Is.EqualTo(1));   // kein zweiter Schlüssel verbraucht
}
```

Der Test mit zwei Schlüsseln ist der interessanteste: Mit nur einem Schlüssel würden `Entfernen<Schluessel>()` und ein hypothetisches `Inventar.Leeren()` dasselbe Ergebnis liefern – erst der zweite Schlüssel unterscheidet „einen entfernen“ von „alle entfernen“. Solche Fälle findet man nur, wenn man vom Code ausgeht und fragt, welche falsche Implementierung der Test noch durchließe.

**Schritt 3 — Dieselbe Regel eine Ebene höher:**

Die Zweige sind damit geprüft; offen bleibt das Zusammenspiel mit `Spielfeld.SpielerZieht` – dass der Held beim Aufschließen *stehen bleibt* und erst im nächsten Zug durch die Tür geht. Genau das prüft der vorhandene Test `Schluessel_Aufheben_Und_Tuer_Oeffnen`. Der Fall „ein Schlüssel, zwei Türen“ lässt sich auf der Karte am schönsten formulieren:

```csharp
[Test] public void Zweite_Tuer_Bleibt_Ohne_Zweiten_Schluessel_Zu()
{
    Spielfeld f = Feld("@kD.D");
    for (int i = 0; i < 4; i++) f.SpielerZieht(Richtung.Rechts);

    f.SpielerZieht(Richtung.Rechts);

    Assert.That(f.LetzteMeldung, Does.Contain("Schlüssel"));
    Assert.That(((Tuer)f.StatischesObjektAn(new Position(4, 0))!).IstOffen, Is.False);
}
```

**Zentrale Designentscheidungen:**

- **Zweige zählen, dann Wirkungen suchen:** Drei `if`-Wege ergeben drei Tests; die Zustandsspalten der Tabelle ergeben die zusätzlichen Assertions.
- **Gegen die plausible Falschimplementierung testen:** Zwei Schlüssel statt einem – sonst bliebe „Inventar leeren“ unentdeckt.
- **Unit- und Integrationsebene trennen:** `Interagieren` direkt mit `Tuer` und `Spieler`, das Zusammenspiel über `Feld(...)` und `SpielerZieht`.

</details>

## Aufgabe 2 — Mustererkennung

Die folgende Testklasse ist grün – und trotzdem fast wertlos. Finde mindestens fünf Fehlermuster und schreibe die Klasse um.

```csharp
[TestFixture]
public class SpielTests
{
    private static Spielfeld feld = LevelParser.Parsen(new Level("t", new[] { "@kD.E" }));

    [Test]
    public void Test1()
    {
        feld.SpielerZieht(Richtung.Rechts);
        Assert.That(feld.Spieler.Inventar.Anzahl, Is.EqualTo(1));
    }

    [Test]
    public void TuerOeffnen()
    {
        feld.SpielerZieht(Richtung.Rechts);
        Assert.That(feld.LetzteMeldung, Is.EqualTo("Du schließt die Tür auf."));
    }

    [Test]
    public void OhneSchluessel()
    {
        Spielfeld f = LevelParser.Parsen(new Level("t", new[] { "@D" }));
        try { f.SpielerZieht(Richtung.Rechts); }
        catch (Exception) { Assert.That(true); }
    }

    [Test]
    public void Verfolger()
    {
        Spielfeld f = LevelParser.Parsen(new Level("t", new[] { "@..V" }));
        f.SpielerZieht(Richtung.Oben);
        Console.WriteLine(f.Gegner[0].Position);
    }
}
```

- Welche Tests hängen voneinander ab, und was passiert, wenn die IDE nur einen davon ausführt?
- Welche Tests können gar nicht rot werden?
- Welcher Test wird rot, obwohl das Spiel korrekt funktioniert – und zwar erst in einigen Wochen?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Geteilter Zustand zwischen Tests:**

`feld` ist `static` und wird von allen Tests benutzt. `TuerOeffnen` erwartet, dass der Held schon den Schlüssel hat – das stimmt nur, wenn vorher `Test1` gelaufen ist. Startet man `TuerOeffnen` allein im Test-Explorer, steht der Held noch auf `(0, 0)`, läuft auf das Schlüsselfeld und die Meldung lautet „Held hebt Schlüssel auf.“ Ein Spielfeld ist veränderlicher Zustand; geteilt zwischen Tests macht es die Reihenfolge zum Teil der Erwartung. Lösung: ein Instanzfeld, das in `[SetUp]` neu entsteht – oder besser die Hilfsmethode `Feld(...)`, die jeder Test selbst aufruft.

**Schritt 2 — Tests, die nie rot werden:**

`OhneSchluessel` besteht immer: `SpielerZieht` wirft hier überhaupt keine Exception, also wird das `catch` nie betreten und der Test endet ohne eine einzige Prüfung. Selbst wenn eine Exception flöge, prüfte `Assert.That(true)` nichts. `Verfolger` hat gar kein Assert – `Console.WriteLine` ist keine Prüfung, sondern nur eine Zeile, die niemand liest. Der Compiler schweigt bei beidem: `NUnit.Analyzers` meldet zwar konstante Vergleichswerte wie `Assert.That(true, Is.True)` (Warnung `NUnit2007`), aber weder das einsame `Assert.That(true)` noch einen Test ganz ohne Prüfung. Solche Tests findet man nur, indem man den geprüften Code absichtlich kaputt macht und nachsieht, ob überhaupt etwas rot wird.

**Schritt 3 — Der Test, der später kaputtgeht:**

`Is.EqualTo("Du schließt die Tür auf.")` betoniert den Meldungstext Zeichen für Zeichen ein. Sobald jemand die Meldung umformuliert oder ein Gegner in derselben Runde etwas anhängt (`LetzteMeldung` sammelt die Meldungen einer ganzen Runde), ist der Test rot, obwohl die Spielregel unverändert stimmt. `Does.Contain("auf")` prüft das, worauf es ankommt. Dazu kommt der Name `Test1`, der in der Ausgabe nichts erklärt, und das ausgeschriebene `LevelParser.Parsen(new Level(...))` in jedem Test statt des Helfers.

**Schritt 4 — Die korrigierte Klasse:**

```csharp
[TestFixture]
public class SpielTests
{
    private static Spielfeld Feld(params string[] zeilen) => LevelParser.Parsen(new Level("t", zeilen));

    [Test] public void Schluessel_Wird_Ins_Inventar_Gelegt()
    {
        Spielfeld f = Feld("@kD.E");

        f.SpielerZieht(Richtung.Rechts);

        Assert.That(f.Spieler.Inventar.Enthaelt<Schluessel>(), Is.True);
    }

    [Test] public void Tuer_Mit_Schluessel_Geht_Auf()
    {
        Spielfeld f = Feld("@kD.E");
        f.SpielerZieht(Richtung.Rechts);

        f.SpielerZieht(Richtung.Rechts);

        Assert.That(((Tuer)f.StatischesObjektAn(new Position(2, 0))!).IstOffen, Is.True);
    }

    [Test] public void Tuer_Ohne_Schluessel_Bleibt_Zu()
    {
        Spielfeld f = Feld("@D");

        f.SpielerZieht(Richtung.Rechts);

        Assert.That(f.LetzteMeldung, Does.Contain("Schlüssel"));
        Assert.That(f.Spieler.Position, Is.EqualTo(new Position(0, 0)));
    }

    [Test] public void Verfolger_Rueckt_Nach()
    {
        Spielfeld f = Feld("@..V");

        f.SpielerZieht(Richtung.Oben);

        Assert.That(f.Gegner[0].Position, Is.EqualTo(new Position(2, 0)));
    }
}
```

**Zentrale Designentscheidungen:**

- **Kein geteilter Zustand:** Jeder Test baut sein Level selbst auf; Reihenfolge und Auswahl der Tests spielen keine Rolle mehr.
- **Erwartetes Verhalten statt Wortlaut:** `Does.Contain` prüft die Zusage, `Is.EqualTo` auf ganzen Meldungen prüft die Formulierung.
- **Jeder Test hat eine Assertion, die rot werden kann:** Am einfachsten prüft man das, indem man den getesteten Code einmal absichtlich kaputt macht.

</details>

## Aufgabe 3 — Algorithmenentwurf

`Verfolger.NaechsterZug(Spielfeld)` entscheidet pro Runde, ob und wohin der Verfolger geht. Er jagt nur, wenn der Spieler innerhalb der `Sichtweite` (Standard: 5) liegt **und** `Spielfeld.HatSichtlinie` keine Wand dazwischen findet; sonst liefert die Methode `null`. Entwirf Tests, die dieses Verhalten absichern – mindestens für „außer Sichtweite“, „Sichtlinie durch eine Wand blockiert“ und „direkt neben dem Spieler“.

- Warum lässt sich `NaechsterZug` besonders leicht testen, und was folgt daraus für den Aufbau der Tests?
- Welche Karte braucht man für jeden der drei Fälle – und welche für den Randfall „genau an der Sichtweite“?
- Was passiert, wenn der Verfolger direkt neben dem Helden steht: Auf welches Feld zieht er?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Die Methode als Funktion begreifen:**

`NaechsterZug` verändert nichts: Sie liest `feld.Spieler.Position`, fragt `IstFrei` und `HatSichtlinie` und liefert eine `Richtung?` zurück. Damit ist sie eine reine Abfrage – man kann sie direkt aufrufen, ohne eine Runde zu spielen, und das Ergebnis ist der ganze zu prüfende Effekt. Das ist der bequemste Fall überhaupt: Arrange ist eine Karte, Act ein Aufruf, Assert eine Zeile.

**Schritt 2 — Für jede Bedingung eine Karte:**

Jede Bedingung im `if` bekommt eine eigene Karte, auf der genau diese Bedingung verletzt ist – und eine, auf der sie knapp erfüllt ist:

```csharp
private static Verfolger VerfolgerAuf(Spielfeld f) => (Verfolger)f.Gegner[0];

[Test] public void NaechsterZug_AusserSichtweite_LiefertNull()
{
    Spielfeld f = Feld("@......V");          // Entfernung 7 > Sichtweite 5

    Assert.That(VerfolgerAuf(f).NaechsterZug(f), Is.Null);
}

[Test] public void NaechsterZug_GenauAnDerSichtweite_JagtNoch()
{
    Spielfeld f = Feld("@....V");            // Entfernung 5, nicht größer als 5

    Assert.That(VerfolgerAuf(f).NaechsterZug(f), Is.EqualTo(Richtung.Links));
}

[Test] public void NaechsterZug_WandDazwischen_LiefertNull()
{
    Spielfeld f = Feld("@.#.V");             // in Reichweite, aber keine Sichtlinie

    Assert.That(VerfolgerAuf(f).NaechsterZug(f), Is.Null);
}

[Test] public void NaechsterZug_DirektNebenSpieler_ZiehtAufIhn()
{
    Spielfeld f = Feld("@V");

    Assert.That(VerfolgerAuf(f).NaechsterZug(f), Is.EqualTo(Richtung.Links));
}
```

Der zweite Test ist der wichtigste: Er prüft die Grenze `> Sichtweite`. Würde jemand `>=` schreiben, bliebe der erste Test grün und nur dieser würde rot – genau dafür sind Randfälle da. Der letzte Test deckt eine Feinheit auf, die man leicht übersieht: Das Feld des Spielers ist nach `IstFrei` **nicht** frei, trotzdem liefert `NaechsterZug` die Richtung dorthin, weil die Methode zusätzlich `Position.Verschoben(erste) == ziel` prüft. Ohne diese Bedingung könnte ein Verfolger nie treffen.

**Schritt 3 — Die Wirkung auf das Spiel:**

Was aus der Richtung wird, entscheidet `Spielfeld.GegnerZiehen`: Zielt der Zug auf den Spieler, gibt es Schaden statt Bewegung. Das gehört eine Ebene höher geprüft – der vorhandene Test `Verfolger_Jagt_Und_Trifft` tut das, inklusive des Endes:

```csharp
Spielfeld f = Feld("@..V");
f.SpielerZieht(Richtung.Oben);   // Spieler kann nicht hoch, Verfolger rückt auf (2,0)
f.SpielerZieht(Richtung.Oben);
f.SpielerZieht(Richtung.Oben);   // Treffer
Assert.That(f.Spieler.Lebenspunkte, Is.EqualTo(2));
```

**Zentrale Designentscheidungen:**

- **Reine Abfragen direkt testen:** Kein `SpielerZieht` nötig – weniger Rauschen im Test und eine eindeutige Fehlerquelle, wenn er rot wird.
- **Pro Bedingung eine Karte, plus der Randfall:** Sichtweite knapp erfüllt, knapp verfehlt, Sichtlinie blockiert – drei Karten, drei Tests.
- **Ebenen trennen:** Die Richtung gehört zu `Verfolger`, der Schaden zu `Spielfeld`. Zwei Verantwortlichkeiten, zwei Testklassen-Abschnitte.

</details>

## Aufgabe 4 — Abstraktion

`HttpLevelQuelle` holt die Level von einem Webserver: erst eine `index.json` mit den Namen, dann pro Level eine Textdatei.

```csharp
public class HttpLevelQuelle : ILevelQuelle
{
    private static readonly HttpClient http = new();
    private readonly string basisUrl;
    private readonly List<string> namen;

    public static async Task<HttpLevelQuelle> ErzeugenAsync(string basisUrl) { /* ... index.json holen ... */ }

    public IReadOnlyList<string> LevelNamen => namen;

    public Level Laden(string name)
    {
        string text = http.GetStringAsync(basisUrl + name + ".txt").GetAwaiter().GetResult();
        // Zeilen aufbereiten ...
    }
}
```

Ein Test dieser Klasse bräuchte Internet, einen erreichbaren Server und wäre langsam. Trotzdem soll alles, was *auf* Levelquellen aufbaut, geprüft werden können – auch das Verhalten, wenn der Server nicht antwortet. Entwirf das.

- Welcher Teil des Spiels muss überhaupt wissen, dass es HTTP gibt?
- Wie testest du eine Klasse, die eine Levelquelle benutzt, ohne einen Server zu starten?
- Was bräuchte es, um `HttpLevelQuelle` selbst zu testen – und in welche Ebene der Testpyramide gehört dieser Test?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Die Abstraktion ist schon da:**

Anders als beim Geometrie- oder Wetterbeispiel muss hier kein Interface mehr erfunden werden: `Adventure.Kern` kennt Levelquellen ausschließlich als `ILevelQuelle` mit `LevelNamen` und `Laden(string)`. Das ist die Dependency Inversion aus der [Schichten-Architektur](/modules/schichten_architektur/schichten_architektur.md) – der Kern schreibt den Vertrag, `Adventure.Daten` erfüllt ihn dreifach: fest im Code, aus Dateien, über HTTP. Jede Klasse, die eine Levelquelle *benutzt*, ist damit ohne Netz testbar, sofern sie die Quelle übergeben bekommt statt sie selbst mit `new` zu erzeugen:

```csharp
public class Levelauswahl
{
    private readonly ILevelQuelle quelle;

    public Levelauswahl(ILevelQuelle quelle) => this.quelle = quelle;

    public Spielfeld Starten(string name) => LevelParser.Parsen(quelle.Laden(name));
}
```

**Schritt 2 — Zwei Fakes statt eines Servers:**

Der erste Fake liefert eine feste Karte und zählt die Aufrufe; der zweite tut so, als wäre der Server weg. Beide sind ein paar Zeilen lang und leben im Testprojekt:

```csharp
class FakeLevelQuelle : ILevelQuelle
{
    private readonly string[] zeilen;
    public int Aufrufe { get; private set; }

    public FakeLevelQuelle(params string[] zeilen) => this.zeilen = zeilen;

    public IReadOnlyList<string> LevelNamen => new[] { "test" };

    public Level Laden(string name)
    {
        Aufrufe++;
        return new Level(name, zeilen);
    }
}

class KaputteLevelQuelle : ILevelQuelle
{
    public IReadOnlyList<string> LevelNamen => Array.Empty<string>();
    public Level Laden(string name) => throw new HttpRequestException("Server nicht erreichbar.");
}

[Test] public void Starten_BautSpielfeldAusDerQuelle()
{
    FakeLevelQuelle quelle = new("@kD.E");
    Levelauswahl auswahl = new(quelle);

    Spielfeld f = auswahl.Starten("test");

    Assert.That(f.Spieler.Position, Is.EqualTo(new Position(0, 0)));
    Assert.That(quelle.Aufrufe, Is.EqualTo(1));   // genau einmal geladen, nicht zweimal
}

[Test] public void Starten_QuelleNichtErreichbar_ReichtFehlerWeiter()
{
    Levelauswahl auswahl = new(new KaputteLevelQuelle());

    Assert.That(() => auswahl.Starten("test"), Throws.TypeOf<HttpRequestException>());
}
```

`FakeLevelQuelle` ist dabei mehr als eine Attrappe: Mit `Aufrufe` wird sie zum **Spy** und kann bezeugen, *wie oft* sie benutzt wurde – so fällt auf, wenn jemand in einer Schleife bei jedem Neuzeichnen ein Level nachlädt. Und wer gar keine Sonderwünsche hat, nimmt statt eines eigenen Fakes einfach `EingebauteLevelQuelle`: Sie ist bereits eine vollwertige Implementierung ohne Netzwerk.

**Schritt 3 — Und die HTTP-Klasse selbst?**

Ganz ungetestet bleiben soll sie nicht, schließlich steckt dort das Zusammensetzen der URLs und das Aufbereiten der Zeilen. Damit ein Test ohne Server auskommt, darf der `HttpClient` nicht mehr fest in der Klasse stehen, sondern wird übergeben – dasselbe Prinzip eine Ebene tiefer:

```csharp
public static async Task<HttpLevelQuelle> ErzeugenAsync(string basisUrl, HttpClient? client = null)
```

Im Test bekommt dieser `HttpClient` einen selbstgebauten `HttpMessageHandler`, der auf jede Anfrage eine vorbereitete Antwort zurückgibt – kein Byte verlässt den Rechner. Der Zusatzparameter ist optional, damit `Adventure.Web` weiterhin `ErzeugenAsync(url)` aufrufen kann. Ein Test gegen den echten Server hat trotzdem seinen Platz, aber als **Integrationstest** in der Mitte der Testpyramide: Er gehört mit `[Category("Integration")]` markiert und wird per `dotnet test --filter "TestCategory!=Integration"` aus dem schnellen Lauf herausgehalten.

**Zentrale Designentscheidungen:**

- **Nur eine Klasse kennt HTTP:** `HttpLevelQuelle` ist die einzige Stelle, an der Netzwerkcode steht – alles darüber sieht nur `ILevelQuelle`.
- **Abhängigkeit übergeben statt erzeugen:** Was im Konstruktor hereinkommt, kann der Test austauschen; was mit `new` entsteht, nicht.
- **Fake, Spy, echte Implementierung:** Für den Normalfall genügt `EingebauteLevelQuelle`, für Zählungen ein Spy, für Fehlerfälle eine Quelle, die wirft. Erfinden muss man nur, was der Test wirklich braucht.

</details>
