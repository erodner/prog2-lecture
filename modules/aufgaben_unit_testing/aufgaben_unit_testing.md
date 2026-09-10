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

Programmieren lernt man nicht nur durch Codezeilen tippen — sondern auch durch **Nachdenken**. Die folgenden Aufgaben trainieren *Computational Thinking*: die Fähigkeit, Probleme so zu strukturieren, dass ein Computer sie lösen kann. Beim Testen heißt das vor allem: systematisch überlegen, welche Fälle es gibt, Fehlermuster in fremdem Code erkennen und Klassen so entwerfen, dass sie sich überhaupt testen lassen. Nimm dir für jede Aufgabe Zeit, bevor du die Lösung aufklappst.

## Aufgabe 1 — Zerlegung

Die Klasse `Bruch` (`examples/12_unittests/Bruch`) hat einen Konstruktor, `Plus` und `Mal`. Die vorhandenen fünf Tests decken längst nicht alles ab. Leite **systematisch** Testfälle ab, statt zufällig Zahlen zu wählen: Teile die Eingaben in **Äquivalenzklassen** (Gruppen, die sich gleich verhalten sollten) und suche an deren Grenzen die **Randfälle**.

- Welche Äquivalenzklassen gibt es für Zähler und Nenner im Konstruktor? Denk an Vorzeichen, Null und schon gekürzte Brüche.
- Welche Fälle sind bei `Plus` und `Mal` besonders – etwa wenn das Ergebnis eine ganze Zahl oder Null wird?
- Gibt es Eingaben, bei denen `Bruch` still ein falsches Ergebnis liefert? Was macht ein Test dann?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Äquivalenzklassen und Randfälle als Tabelle:**

| Methode | Klasse / Randfall | Eingabe | Erwartung |
| :--- | :--- | :--- | :--- |
| Konstruktor | kürzbar / schon gekürzt | `(6, 8)` / `(3, 4)` | `3/4` |
| Konstruktor | negativer Nenner / beide negativ | `(1, -2)` / `(-1, -2)` | `-1/2` / `1/2` |
| Konstruktor | Zähler 0 | `(0, 5)` | `0/1` |
| Konstruktor | Nenner 0 | `(1, 0)` | `ArgumentException` |
| `Plus` | gleicher Nenner | `1/4 + 1/4` | `1/2` |
| `Plus` | Ergebnis ganze Zahl / Null | `1/2 + 1/2` / `1/2 + (-1/2)` | `1` / `0` |
| `Mal` | Kehrwert / mit Null | `2/3 * 3/2` / `2/3 * 0` | `1` / `0` |
| `Mal` | negativ mal negativ | `-1/2 * -2/3` | `1/3` |

**Schritt 2 — Tabelle in `[TestCase]`-Tests übersetzen:**

Jede Zeile wird zu einem Testfall einer parametrisierten Methode – für `Plus` und `Mal` analog mit sechs Parametern:

```csharp
[TestCase(6, 8, 3, 4)]
[TestCase(3, 4, 3, 4)]
[TestCase(1, -2, -1, 2)]
[TestCase(-1, -2, 1, 2)]
[TestCase(0, 5, 0, 1)]
public void Konstruktor_NormalisiertVorzeichenUndKuerzt(int z, int n, int erwZ, int erwN)
{
    Bruch b = new Bruch(z, n);

    Assert.That(b.Zaehler, Is.EqualTo(erwZ));
    Assert.That(b.Nenner, Is.EqualTo(erwN));
}
```

Hier werden bewusst `Zaehler` und `Nenner` einzeln verglichen statt `Is.EqualTo(new Bruch(erwZ, erwN))`: Die Erwartung liefe sonst durch denselben Konstruktor wie das Ergebnis – ein Fehler beim Kürzen träte auf beiden Seiten auf und der Test bliebe grün.

**Schritt 3 — Der stille Fehler:**

`new Bruch(int.MaxValue, 2).Plus(new Bruch(1, 2))` rechnet `Zaehler * anderer.Nenner` in `int` – das läuft über und liefert ohne Exception ein falsches Ergebnis. Ein Test dafür dokumentiert die Grenze der Klasse. Ob man ihn als erwartetes Verhalten (`Throws.TypeOf<OverflowException>()` nach Umstellung auf `checked`) oder als bekannte Einschränkung (`[Ignore("Überlauf bewusst nicht behandelt")]`) formuliert, ist eine Designentscheidung – aber sie sollte im Test stehen, nicht nur im Kopf.

**Zentrale Designentscheidungen:**

- **Äquivalenzklassen statt Zufallszahlen:** Zehn Tests mit positiven kürzbaren Brüchen prüfen alle dasselbe. Ein Test pro Klasse plus Randfälle deckt mehr ab mit weniger Code.
- **`[TestCase]` für gleichförmige Fälle:** Jede Tabellenzeile wird zu einem eigenen Test mit eigenem Namen in der Ausgabe.
- **Nicht durch den Prüfling prüfen:** Wo Erwartung und Ergebnis durch denselben Code laufen, sind rohe Werte die bessere Wahl.

</details>

## Aufgabe 2 — Mustererkennung

Die folgende Testklasse für ein `Konto` (`Einzahlen`, `Abheben`, `Kontostand`, `Inhaber`; `Abheben` wirft `InvalidOperationException` bei fehlender Deckung) ist grün – und trotzdem fast wertlos. Finde mindestens vier Fehlermuster.

```csharp
[TestFixture]
public class KontoTests
{
    private static Konto konto = new Konto("Anna");

    [Test]
    public void Einzahlen_ErhoehtKontostand()
    {
        konto.Einzahlen(100);
        Assert.That(konto.Kontostand, Is.EqualTo(100));
    }

    [Test]
    public void Abheben_ReduziertKontostand()
    {
        konto.Abheben(30);
        Assert.That(konto.Kontostand, Is.EqualTo(70));
    }

    [Test]
    public void Einzahlen_ZweiBetraege_Summiert()
    {
        Konto k = new Konto("Ben");
        k.Einzahlen(0.1);
        k.Einzahlen(0.2);
        Assert.That(k.Kontostand, Is.EqualTo(0.3));
    }

    [Test]
    public void Abheben_OhneDeckung_WirftException()
    {
        Konto k = new Konto("Ben");
        try { k.Abheben(50); }
        catch (InvalidOperationException) { Assert.That(true); }
    }

    [Test]
    public void Inhaber_WirdUebernommen()
    {
        Console.WriteLine(new Konto("Clara").Inhaber);
    }
}
```

- Welche Tests hängen voneinander ab, und was passiert, wenn die IDE nur einen davon ausführt?
- Welche Tests können gar nicht rot werden?
- Welcher Test ist rot, obwohl `Konto` korrekt rechnet?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Abhängige Tests über geteilten Zustand:**

`konto` ist `static` und wird von allen Tests geteilt. `Abheben_ReduziertKontostand` erwartet 70 – das stimmt nur, wenn vorher `Einzahlen_ErhoehtKontostand` gelaufen ist. Führt man den Test allein aus oder in anderer Reihenfolge, wirft `Abheben` eine Exception. Lösung: ein Instanzfeld, das in `[SetUp]` neu erzeugt wird, und jeder Test baut seine Ausgangslage selbst auf.

**Schritt 2 — Tests, die nie rot werden:**

`Abheben_OhneDeckung_WirftException` besteht auch, wenn *keine* Exception fliegt: Dann wird das `catch` übersprungen, und ohne Assertion gilt der Test als bestanden – `Assert.That(true)` prüft ohnehin nichts. `Inhaber_WirdUebernommen` hat überhaupt kein Assert; `Console.WriteLine` ist keine Prüfung. `NUnit.Analyzers` warnt bei beidem.

**Schritt 3 — Gleitkomma ohne Toleranz:**

`0.1 + 0.2` ergibt in `double` `0.30000000000000004`; der Test ist rot, obwohl `Konto` korrekt rechnet. Gleitkommavergleiche brauchen `Within`.

**Schritt 4 — Die korrigierten Stellen:**

```csharp
private Konto konto = null!;

[SetUp]
public void Vorbereiten() => konto = new Konto("Anna");

[Test]
public void Abheben_NachEinzahlung_ReduziertKontostand()
{
    konto.Einzahlen(100);

    konto.Abheben(30);

    Assert.That(konto.Kontostand, Is.EqualTo(70).Within(1e-9));
}

[Test]
public void Abheben_OhneDeckung_WirftInvalidOperationException()
{
    Assert.That(() => konto.Abheben(50), Throws.InvalidOperationException);
}

[Test]
public void Konstruktor_UebernimmtInhaber()
{
    Assert.That(new Konto("Clara").Inhaber, Is.EqualTo("Clara"));
}
```

**Zentrale Designentscheidungen:**

- **Kein geteilter Zustand:** `[SetUp]` gibt jedem Test ein frisches Objekt; Reihenfolge und Auswahl der Tests spielen keine Rolle mehr.
- **Exceptions mit `Throws`:** Das Constraint schlägt fehl, wenn nichts geworfen wird – `try`/`catch` im Test verschluckt genau diesen Fall.
- **Jeder Test hat eine Assertion, die rot werden kann:** Am einfachsten prüft man das, indem man den getesteten Code einmal absichtlich kaputt macht.

</details>

## Aufgabe 3 — Algorithmenentwurf

`FigurenVerwaltung.Hinzufuegen` wirft eine `ArgumentException`, wenn schon eine Figur mit demselben Namen existiert. Entwirf Tests, die dieses Verhalten **vollständig** absichern – nicht nur, dass die Exception kommt. Ergänze danach Tests für `Suchen` mit einem unbekannten Namen.

- Was muss nach dem fehlgeschlagenen `Hinzufuegen` mit der Liste passiert sein – und wie prüfst du das?
- Welche Nachricht sollte die Exception enthalten, damit die GUI sie sinnvoll anzeigen kann?
- Was liefert `Suchen("gibt es nicht")` – und was folgt daraus für den Aufrufer?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Den Fehlerfall vollständig beschreiben:**

„Wirft eine Exception“ ist nur die halbe Zusage. Genauso wichtig: Die Verwaltung bleibt in einem gültigen Zustand – die zweite Figur darf *nicht* trotzdem in der Liste landen, und die erste bleibt erhalten. Die Nachricht sollte den Namen enthalten, damit `Home.razor` sie ohne Nachbearbeitung anzeigen kann.

```csharp
[Test]
public void Hinzufuegen_DoppelterName_WirftArgumentExceptionMitName()
{
    verwaltung.Hinzufuegen(new Kreis("k1", 0, 0, 1));

    Assert.That(() => verwaltung.Hinzufuegen(new Rechteck("k1", 0, 0, 2, 3)),
                Throws.ArgumentException.With.Message.Contains("k1"));
}

[Test]
public void Hinzufuegen_DoppelterName_LaesstListeUnveraendert()
{
    Kreis erster = new Kreis("k1", 0, 0, 1);
    verwaltung.Hinzufuegen(erster);

    try { verwaltung.Hinzufuegen(new Rechteck("k1", 0, 0, 2, 3)); }
    catch (ArgumentException) { }

    Assert.That(verwaltung.AlleFiguren, Has.Count.EqualTo(1));
    Assert.That(verwaltung.AlleFiguren[0], Is.SameAs(erster));
}

[Test]
public void Suchen_UnbekannterName_LiefertNull()
{
    verwaltung.Hinzufuegen(new Kreis("k1", 0, 0, 1));

    Assert.That(verwaltung.Suchen("k2"), Is.Null);
}
```

Der zweite Test fängt die Exception bewusst ab – hier ist sie nicht das Prüfziel, sondern Voraussetzung, um danach den Zustand zu untersuchen. Das ist der einzige gute Grund für `try`/`catch` in einem Test. Der dritte Test macht aus dem `?` in `Figur? Suchen(string)` eine geprüfte Zusage: `null` ist der Vertrag für „nicht gefunden“, und der Aufrufer muss damit rechnen. Das Gegenstück – bekannter Name liefert dieselbe Instanz, prüfbar mit `Is.SameAs` – gehört daneben.

**Zentrale Designentscheidungen:**

- **Fehlerfall = Exception + Zustand:** Ein Test für die Exception, ein zweiter für „nichts ist kaputtgegangen“ – zwei Verhalten, zwei Tests.
- **Nachricht prüfen, wo Nutzer sie sehen:** `With.Message.Contains("k1")` sichert ab, dass die GUI eine brauchbare Meldung bekommt.
- **`null` als dokumentierter Vertrag:** Beide Seiten von `Suchen` testen, sonst weiß niemand, ob `null` Absicht ist.

</details>

## Aufgabe 4 — Abstraktion

Die folgende Klasse holt sich Wetterdaten über [HttpClient](/modules/httpclient_rest/httpclient_rest.md) und leitet daraus einen Kleidungstipp ab. Sie ist so nicht testbar: Jeder Test bräuchte Internet, das Ergebnis hängt vom echten Wetter ab, und der Test wäre langsam. Mach die Klasse testbar, ohne dass die Fachlogik in `KleidungsTippAsync` sich ändert.

```csharp
public class Wetterdienst
{
    private readonly HttpClient client = new HttpClient();

    public async Task<double> AktuelleTemperaturAsync(string stadt)
    {
        string json = await client.GetStringAsync($"https://api.example.org/wetter?stadt={stadt}");
        WetterAntwort? antwort = JsonSerializer.Deserialize<WetterAntwort>(json);
        return antwort?.Temperatur ?? throw new InvalidOperationException("Keine Wetterdaten.");
    }

    public async Task<string> KleidungsTippAsync(string stadt)
    {
        double t = await AktuelleTemperaturAsync(stadt);
        return t < 5 ? "Wintermantel" : t < 18 ? "Jacke" : "T-Shirt";
    }
}
```

- Welcher Teil ist Fachlogik, welcher Teil ist Infrastruktur?
- Wie bekommt die Klasse ihre Datenquelle, ohne sie selbst mit `new` zu erzeugen?
- Wie sieht ein Test für die Grenze bei genau 5 Grad aus?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Die Abhängigkeit hinter ein Interface ziehen:**

Genau wie `FigurenVerwaltung` ihren Speicher nur als `IFigurSpeicher` kennt, soll `Wetterdienst` seine Temperaturquelle nur als Schnittstelle kennen – die Entscheidung aus [Schnittstellen- vs. Implementierungsvererbung](/modules/schnittstellen_vs_implementierungsvererbung/schnittstellen_vs_implementierungsvererbung.md): Die Quelle „kann Temperaturen liefern“, mehr muss der Dienst nicht wissen. Der bisherige HTTP-Code wandert unverändert in eine Klasse `HttpWetterQuelle : IWetterQuelle`.

```csharp
public interface IWetterQuelle
{
    Task<double> TemperaturAsync(string stadt);
}

public class Wetterdienst
{
    private readonly IWetterQuelle quelle;

    public Wetterdienst(IWetterQuelle quelle) => this.quelle = quelle;

    public async Task<string> KleidungsTippAsync(string stadt)
    {
        double t = await quelle.TemperaturAsync(stadt);
        return t < 5 ? "Wintermantel" : t < 18 ? "Jacke" : "T-Shirt";
    }
}
```

**Schritt 2 — Ein Fake für den Test:**

Im Testprojekt braucht es keine HTTP-Verbindung mehr – nur eine Klasse, die das Interface mit einem festen Wert erfüllt. Die Testmethode wird `async Task`, weil `KleidungsTippAsync` asynchron ist; NUnit wartet auf das Ergebnis.

```csharp
class FesteWetterQuelle : IWetterQuelle
{
    private readonly double temperatur;

    public FesteWetterQuelle(double temperatur)
    {
        this.temperatur = temperatur;
    }

    public Task<double> TemperaturAsync(string stadt) => Task.FromResult(temperatur);
}

[TestCase(-3.0, "Wintermantel")]
[TestCase(4.9, "Wintermantel")]
[TestCase(5.0, "Jacke")]
[TestCase(17.9, "Jacke")]
[TestCase(18.0, "T-Shirt")]
public async Task KleidungsTippAsync_Temperaturgrenzen(double temperatur, string erwartet)
{
    Wetterdienst dienst = new Wetterdienst(new FesteWetterQuelle(temperatur));

    string tipp = await dienst.KleidungsTippAsync("Berlin");

    Assert.That(tipp, Is.EqualTo(erwartet));
}
```

Die Grenzwerte 5.0 und 18.0 sind die Randfälle aus Aufgabe 1 – hier entscheidet `<` gegen `<=`, und genau das prüfen die Tests. `HttpWetterQuelle` bekommt höchstens einen Integrationstest, der bewusst getrennt läuft.

**Zentrale Designentscheidungen:**

- **Fachlogik von Infrastruktur trennen:** Der Kleidungstipp ist reine Logik und gehört ins Fachkonzept; HTTP und JSON sind Datenhaltung. Die Grenze ist das Interface.
- **Abhängigkeit übergeben statt erzeugen:** Der Konstruktor bekommt die `IWetterQuelle` – so steckt die App die HTTP-Variante und der Test den Fake hinein.
- **Fake statt Netzwerk:** `FesteWetterQuelle` ist ein Test-Double wie `ArbeitsspeicherFigurSpeicher` – schnell, deterministisch, ohne Internet.

</details>
