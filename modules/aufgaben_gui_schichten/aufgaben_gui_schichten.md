---
title: "🧩 Aufgaben und Beispiele: GUI und Schichten"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Programmieren lernt man nicht nur durch Codezeilen tippen – sondern auch durch **Nachdenken**. Die folgenden Aufgaben trainieren *Computational Thinking*: die Fähigkeit, Probleme so zu strukturieren, dass ein Computer sie lösen kann. Bei Oberflächen und Architekturen heißt das vor allem: eine Seite in Bereiche zerlegen, Logik von Anzeige trennen und Abhängigkeiten erkennen, bevor sie zum Problem werden. Nimm dir für jede Aufgabe Zeit, bevor du die Lösung aufklappst.

## Aufgabe 1 — Zerlegung

Entwirf einen einfachen Taschenrechner als Razor-Komponente: oben eine Anzeige, darunter ein Ziffernblock mit den Ziffern 0–9, dem Komma, den vier Grundrechenarten, `=` und `C`. Die Null soll doppelt so breit sein wie die anderen Ziffern, `=` doppelt so hoch, und der Block soll mit CSS-Grid gesetzt werden – ohne dass du jede Taste einzeln positionierst.

- Wie viele Spalten braucht das Grid, und wie erreichst du die doppelt breite Null, ohne jeder Taste eine Position zu geben?
- Wie viele Klick-Handler brauchst du für siebzehn Tasten – und woher weiß ein gemeinsamer Handler, welche Taste gedrückt wurde?
- Welche Felder bilden den Zustand des Rechners? Was muss sich die Komponente zwischen zwei Klicks merken?
- Die Rechenlogik landet in dieser Aufgabe in der Komponente. Warum ist das hier vertretbar, und ab wann nicht mehr?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Zustand festlegen:**

Der Rechner braucht die Anzeige als Text, den linken Operanden, das gewählte Rechenzeichen und ein Merkmal, ob die nächste Ziffer eine neue Zahl beginnt (nach `+` oder `=`) oder die angezeigte fortsetzt.

**Schritt 2 — Tasten als Daten, nicht als Markup:**

Statt siebzehn `<button>`-Elemente zu schreiben, legen wir die Tasten in ein Array und lassen `@foreach` das Markup erzeugen. Der Handler bekommt die Taste über ein Lambda mit – ein Handler für alle:

```razor
@using System.Globalization

<div class="rechner">
    <div class="anzeige">@anzeige</div>
    <div class="tasten">
        @foreach (string taste in tasten)
        {
            <button class="@KlasseFuer(taste)" @onclick="() => TasteGedrueckt(taste)">@taste</button>
        }
    </div>
</div>

@code {
    private static readonly CultureInfo de = CultureInfo.GetCultureInfo("de-DE");

    private readonly string[] tasten =
        ["C", "/", "*", "7", "8", "9", "-", "4", "5", "6", "+", "1", "2", "3", "=", "0", ","];

    private string anzeige = "0";
    private double linkerOperand = 0;
    private string? rechenzeichen;
    private bool neueZahl = true;

    private static string KlasseFuer(string taste) => taste switch
    {
        "C" or "0" => "breit",
        "=" => "hoch",
        _ => ""
    };

    private void TasteGedrueckt(string taste)
    {
        switch (taste)
        {
            case "C":
                anzeige = "0"; linkerOperand = 0; rechenzeichen = null; neueZahl = true;
                break;
            case "+" or "-" or "*" or "/":
                Berechnen();
                rechenzeichen = taste;
                neueZahl = true;
                break;
            case "=":
                Berechnen();
                rechenzeichen = null;
                neueZahl = true;
                break;
            default:
                ZiffernTaste(taste);
                break;
        }
    }

    private void ZiffernTaste(string taste)
    {
        if (neueZahl)
        {
            anzeige = taste == "," ? "0," : taste;
            neueZahl = false;
        }
        else if (taste != "," || !anzeige.Contains(','))
        {
            anzeige = anzeige == "0" && taste != "," ? taste : anzeige + taste;
        }
    }

    private void Berechnen()
    {
        double rechts = double.Parse(anzeige, de);
        double ergebnis = rechenzeichen switch
        {
            "+" => linkerOperand + rechts,
            "-" => linkerOperand - rechts,
            "*" => linkerOperand * rechts,
            "/" => linkerOperand / rechts,
            _ => rechts
        };
        linkerOperand = ergebnis;
        anzeige = ergebnis.ToString(de);
    }
}
```

**Schritt 3 — Layout mit CSS-Grid:**

Vier Spalten, und die Tasten füllen sie in der Reihenfolge des Arrays automatisch. Nur zwei Klassen greifen ein: `breit` belegt zwei Spalten, `hoch` zwei Zeilen. Die Grid-Auto-Platzierung schiebt die folgenden Tasten um die belegten Zellen herum – deshalb steht die Null am Ende des Arrays direkt vor dem Komma, obwohl `=` in der Zeile davor beginnt.

```css
.rechner {
    max-width: 260px;
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
}

.anzeige {
    text-align: right;
    font-size: 1.8rem;
    padding: 0.5rem;
    border: 1px solid #ccc;
}

.tasten {
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: 0.3rem;
}

.tasten button { padding: 0.8rem; font-size: 1.1rem; }
.tasten .breit { grid-column: span 2; }
.tasten .hoch  { grid-row: span 2; }
```

**Zentrale Designentscheidungen:**

- **Ein Handler statt siebzehn:** Das Lambda `() => TasteGedrueckt(taste)` fängt die Schleifenvariable ein, sodass jeder Button „seine“ Taste übergibt. Die Tastenbeschriftung ist damit zugleich der Parameter – Markup und Logik haben eine gemeinsame Datenquelle, das Array.
- **Grid-Auto-Platzierung statt fester Positionen:** Nur die Ausnahmen (`breit`, `hoch`) werden benannt; alles andere ergibt sich aus der Reihenfolge. Eine Taste hinzuzufügen heißt, das Array zu erweitern.
- **Zustand nur in Feldern:** Kein Handler schreibt in die Anzeige – `anzeige` ist ein Feld, das Markup zeigt es. Nach jedem Klick rendert Blazor neu.
- **Logik in der Komponente – vorerst:** Für eine Übung ist das vertretbar. Sobald der Rechner Prozent, Klammern oder eine Verlaufsliste bekommt, wandert `Berechnen` in eine Klasse `Rechenwerk` im Fachkonzept, die man ohne Browser testen kann – genau das üben wir in Aufgabe 2.

</details>

## Aufgabe 2 — Abstraktion

Die folgende Komponente berechnet einen Rabatt. Sie funktioniert – aber sie verletzt die Regel, dass eine Komponente keine Geschäftslogik enthält:

```razor
@using System.Globalization

<label>Bestellwert: <input @bind="bestellwertText" /></label>
<label><input type="checkbox" @bind="stammkunde" /> Stammkunde</label>
<button @onclick="Berechnen">Berechnen</button>
<p>@ergebnis</p>

@code {
    private string bestellwertText = "";
    private bool stammkunde;
    private string ergebnis = "";

    private void Berechnen()
    {
        double wert = double.Parse(bestellwertText, CultureInfo.InvariantCulture);
        double prozent = 0;
        if (wert >= 500) prozent = 10;
        else if (wert >= 100) prozent = 5;
        if (stammkunde) prozent += 3;
        double rabatt = wert * prozent / 100;
        ergebnis = $"Rabatt {prozent} % = {rabatt:F2} €, zu zahlen {wert - rabatt:F2} €";
    }
}
```

Trenne die Komponente in Oberfläche und eine Fachkonzept-Klasse `Rabattrechner`.

- Welche Zeilen bestehen die Konsolen-Probe („könnte unverändert in einer Konsolenversion stehen“) und welche nicht?
- Wohin gehört `double.Parse` – und wohin die Prüfung, dass ein Bestellwert nicht negativ sein darf?
- Was gibt der `Rabattrechner` zurück – einen `double`, einen fertigen Text oder etwas anderes?
- Wie sähe ein NUnit-Test für die Staffel „ab 500 € zehn Prozent“ aus?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Zeilen sortieren:**

Die Staffelung (`>= 500`, `>= 100`), der Stammkundenbonus und die Multiplikation sind Fachregeln – sie würden in einer Konsolenversion genauso stehen. `double.Parse` wandelt eine *Eingabe* um, `ergebnis = $"..."` formatiert eine *Ausgabe* – beides ist Oberfläche. Die Prüfung auf negative Werte ist dagegen eine Fachregel: Ein negativer Bestellwert ist in jeder Version des Programms unsinnig, also gehört sie in den `Rabattrechner`.

**Schritt 2 — Fachkonzept-Klasse:**

Der Rechner gibt kein fertiges Textformat zurück, sondern die drei Zahlen, die die Oberfläche braucht – als `record`, damit das Ergebnis einen Namen hat:

```csharp
namespace Shop.Fachkonzept;

public record RabattErgebnis(double Prozent, double Betrag, double Endpreis);

public class Rabattrechner
{
    public double StammkundenBonus { get; init; } = 3;

    public RabattErgebnis Berechnen(double bestellwert, bool stammkunde)
    {
        if (bestellwert < 0)
        {
            throw new ArgumentException("Der Bestellwert darf nicht negativ sein.");
        }

        double prozent = bestellwert switch
        {
            >= 500 => 10,
            >= 100 => 5,
            _ => 0
        };
        if (stammkunde)
        {
            prozent += StammkundenBonus;
        }

        double betrag = bestellwert * prozent / 100;
        return new RabattErgebnis(prozent, betrag, bestellwert - betrag);
    }
}
```

**Schritt 3 — Komponente ohne Regeln:**

Die Komponente liest Eingaben, ruft den Rechner und zeigt an. Beide Fehlerarten – ungültiges Format aus `double.Parse`, negative Zahl aus dem Fachkonzept – landen als Text im `ergebnis`-Feld:

```razor
@using System.Globalization
@using Shop.Fachkonzept
@inject Rabattrechner Rechner

<label>Bestellwert: <input @bind="bestellwertText" /></label>
<label><input type="checkbox" @bind="stammkunde" /> Stammkunde</label>
<button @onclick="Berechnen">Berechnen</button>
<p>@ergebnis</p>

@code {
    private string bestellwertText = "";
    private bool stammkunde;
    private string ergebnis = "";

    private void Berechnen()
    {
        try
        {
            double wert = double.Parse(bestellwertText, CultureInfo.InvariantCulture);
            RabattErgebnis r = Rechner.Berechnen(wert, stammkunde);
            ergebnis = $"Rabatt {r.Prozent} % = {r.Betrag:F2} €, zu zahlen {r.Endpreis:F2} €";
        }
        catch (Exception ex) when (ex is FormatException || ex is ArgumentException)
        {
            ergebnis = ex.Message;
        }
    }
}
```

In `Program.cs` genügt `builder.Services.AddScoped<Rabattrechner>();`, damit `@inject` den Rechner liefert.

**Schritt 4 — Test ohne Browser:**

```csharp
[Test]
public void Berechnen_Ab500Euro_ZehnProzent()
{
    Rabattrechner rechner = new Rabattrechner();

    RabattErgebnis r = rechner.Berechnen(500, stammkunde: false);

    Assert.That(r.Prozent, Is.EqualTo(10));
    Assert.That(r.Endpreis, Is.EqualTo(450).Within(1e-9));
}
```

**Zentrale Designentscheidungen:**

- **Der Rechner kennt keine Strings:** Er nimmt `double` und `bool` und gibt Zahlen zurück. Ob die Eingabe aus einem Textfeld, einer Datei oder einem Test kommt, ist ihm egal – das macht ihn testbar.
- **Ein `record` statt drei Rückgabewerten:** `RabattErgebnis` benennt, was zusammengehört, und die Oberfläche entscheidet selbst, wie sie es formatiert.
- **Zwei Fehlerarten, zwei Quellen, ein Anzeigeort:** `FormatException` kommt aus der Oberfläche, `ArgumentException` aus dem Fachkonzept; angezeigt werden beide in der Komponente, denn nur sie darf anzeigen.
- **Bonus als Property:** `StammkundenBonus` mit `init` erlaubt es, den Wert im Test oder in einer anderen Konfiguration zu ändern, ohne die Klasse anzufassen.

</details>

## Aufgabe 3 — Algorithmenentwurf

Der Geometrieeditor soll eine ausgewählte Figur bearbeiten können. Entwirf eine Dialog-Komponente `FigurBearbeitenDialog`, die die vorhandene Figur per `[Parameter]` bekommt und die geänderte Figur per `EventCallback<Figur?>` zurückgibt – oder `null` bei Abbruch. Ändern lassen sollen sich Position und Maße.

- Welche Parameter braucht die Komponente, und wie bindet die Startseite sie ein?
- Bearbeitet der Dialog das übergebene Objekt direkt oder eine Kopie? Was passiert bei „Abbrechen“, wenn der Benutzer schon getippt hat?
- Wie kommen die aktuellen Werte der Figur in die Textfelder – und in welcher Lebenszyklus-Methode?
- Warum ist der Name in dieser Lösung nicht änderbar, und was müsste sich ändern, damit er es wird?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Ablauf festlegen:**

1. Die Startseite setzt `bearbeitenOffen = true`, während `ausgewaehlt` eine Figur enthält.
2. Der Dialog entsteht, kopiert die Werte der Figur **in eigene Textfelder** und zeigt sie an.
3. Der Benutzer tippt – nur die Textfelder ändern sich, die Figur bleibt unberührt.
4. Bei „OK“ werden die Texte geparst; erst wenn alle gültig sind, schreibt der Dialog sie in die Figur und ruft `OnGeschlossen` mit der Figur auf.
5. Bei „Abbrechen“ ruft er `OnGeschlossen` mit `null` auf – die Figur ist unverändert.

**Schritt 2 — Die Dialog-Komponente:**

```razor
@using System.Globalization
@using Geometrieeditor.Fachkonzept

<div class="dialog-hintergrund">
    <div class="dialog">
        <h2>@Figur.Name bearbeiten</h2>
        <label>Position: <input @bind="x" size="5" /> <input @bind="y" size="5" /></label>
        <label>@MasseBeschriftung <input @bind="masse" /></label>
        <p class="fehler">@fehler</p>
        <div class="dialog-buttons">
            <button @onclick="Abbrechen">Abbrechen</button>
            <button @onclick="Ok">OK</button>
        </div>
    </div>
</div>

@code {
    [Parameter, EditorRequired]
    public Figur Figur { get; set; } = null!;

    [Parameter]
    public EventCallback<Figur?> OnGeschlossen { get; set; }

    private string x = "", y = "", masse = "", fehler = "";

    private string MasseBeschriftung => Figur switch
    {
        Rechteck => "Breite, Höhe:",
        Kreis => "Radius:",
        _ => "Seiten a, b, c:"
    };

    protected override void OnInitialized()
    {
        x = Text(Figur.X);
        y = Text(Figur.Y);
        masse = Figur switch
        {
            Rechteck r => Text(r.Breite, r.Hoehe),
            Kreis k => Text(k.Radius),
            Dreieck d => Text(d.SeiteA, d.SeiteB, d.SeiteC),
            _ => ""
        };
    }

    private async Task Ok()
    {
        try
        {
            double px = Zahl(x);
            double py = Zahl(y);
            double[] werte = masse
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(Zahl)
                .ToArray();

            switch (Figur)
            {
                case Rechteck r when werte.Length == 2:
                    (r.Breite, r.Hoehe) = (werte[0], werte[1]);
                    break;
                case Kreis k when werte.Length == 1:
                    k.Radius = werte[0];
                    break;
                case Dreieck d when werte.Length == 3:
                    // Der Konstruktor prüft die Dreiecksungleichung – wir nutzen ihn als Probe.
                    _ = new Dreieck(d.Name, px, py, werte[0], werte[1], werte[2]);
                    (d.SeiteA, d.SeiteB, d.SeiteC) = (werte[0], werte[1], werte[2]);
                    break;
                default:
                    throw new ArgumentException("Bitte die richtige Anzahl an Maßen eingeben.");
            }
            Figur.X = px;
            Figur.Y = py;

            await OnGeschlossen.InvokeAsync(Figur);
        }
        catch (Exception ex) when (ex is FormatException || ex is ArgumentException)
        {
            fehler = ex.Message;
        }
    }

    private Task Abbrechen() => OnGeschlossen.InvokeAsync(null);

    private static double Zahl(string text) => double.Parse(text, CultureInfo.InvariantCulture);

    private static string Text(params double[] werte) =>
        string.Join(", ", werte.Select(w => w.ToString(CultureInfo.InvariantCulture)));
}
```

**Schritt 3 — Einbindung in `Home.razor`:**

```razor
<button @onclick="() => bearbeitenOffen = true" disabled="@(ausgewaehlt is null)">Bearbeiten …</button>

@if (bearbeitenOffen && ausgewaehlt is not null)
{
    <FigurBearbeitenDialog Figur="ausgewaehlt" OnGeschlossen="BearbeitenGeschlossen" />
}

@code {
    private bool bearbeitenOffen = false;

    private void BearbeitenGeschlossen(Figur? geaendert)
    {
        bearbeitenOffen = false;
        status = geaendert is null ? "Abgebrochen." : $"{geaendert.Name} geändert.";
        StatusAktualisieren();
    }
}
```

**Zentrale Designentscheidungen:**

- **Textfelder als Kopie:** Der Dialog arbeitet auf eigenen `string`-Feldern und schreibt erst bei „OK“ in die Figur. Deshalb ist „Abbrechen“ trivial – es gibt nichts zurückzusetzen. Würde `@bind` direkt an `Figur.X` hängen, wäre jede Eingabe sofort wirksam.
- **`OnInitialized`, nicht `OnParametersSet`:** Die Parameter sind beim Aufruf von `OnInitialized` bereits gesetzt, und die Methode läuft genau einmal pro Instanz. `OnParametersSet` liefe bei jedem Neu-Rendern der Eltern erneut und würde die Eingaben des Benutzers überschreiben.
- **Validierung wiederverwenden statt duplizieren:** Die Properties von `Dreieck` haben öffentliche Setter, nur der Konstruktor prüft die Dreiecksungleichung. Der Dialog erzeugt deshalb probeweise ein neues `Dreieck` und übernimmt die Werte nur, wenn das gelingt. Sauberer wäre eine Methode `SeitenSetzen` im Fachkonzept, die selbst prüft – eine gute Folgeaufgabe.
- **Der Name bleibt schreibgeschützt:** `FigurenVerwaltung` nutzt den Namen als Schlüssel für die Eindeutigkeit. Änderte der Dialog `Figur.Name` direkt, würde er die Regel „kein doppelter Name“ umgehen. Ein Umbenennen wäre eine Fachoperation `Verwaltung.Umbenennen(figur, neuerName)`, die die Prüfung enthält – der Dialog darf sie nicht nachbauen.

</details>

## Aufgabe 4 — Mustererkennung

In einem Projekt sieht das Abhängigkeitsdiagramm des Geometrieeditors so aus:

```
   Geometrieeditor.Web
   Home.razor  ◄──────────────────────────────────┐
        │ nutzt FigurenVerwaltung                  │
        ▼                                          │
   Geometrieeditor.Fachkonzept                     │ ruft seite.FehlerAnzeigen(...)
   FigurenVerwaltung, IFigurSpeicher               │
        ▲ implementiert                            │
        │                                          │
   Geometrieeditor.Datenhaltung                    │
   JsonFigurSpeicher  ─────────────────────────────┘
```

Der `JsonFigurSpeicher` hat ein Feld `Home seite` bekommen, damit er bei einer fehlenden oder kaputten Datei eine Meldung in der Statuszeile anzeigen kann:

```csharp
public List<Figur> Laden()
{
    if (!File.Exists(pfad))
    {
        seite.FehlerAnzeigen($"Datei {pfad} nicht gefunden.");
        return new List<Figur>();
    }
    string json = File.ReadAllText(pfad);
    return JsonSerializer.Deserialize<List<Figur>>(json, optionen) ?? new List<Figur>();
}
```

- Welche der vier Schichtregeln ist verletzt – und woran würde der Compiler es bemerken, wenn die Schichten eigene Projekte sind?
- Wie kommt die Information „Datei fehlt“ nach oben, ohne dass die Datenhaltung die Oberfläche kennt?
- Wer entscheidet, was bei einem Fehler passiert – die Datenhaltung, die Verwaltung oder die Komponente?
- Schau dir `FigurenVerwaltung.Laden()` an: `figuren.Clear()` steht *vor* `speicher.Laden()`. Was passiert, wenn das Laden eine Exception wirft?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Das Muster erkennen:**

Verletzt ist Regel 2: Eine Schicht darf nie von einer höheren abhängen. Die Datenhaltung ist die unterste Schicht und kennt jetzt die oberste – das ist genau das „nur mal kurz eine Meldung anzeigen“, vor dem das Architektur-Modul warnt. Nebenbei ist auch Regel 1 verletzt, denn die Datenhaltung überspringt das Fachkonzept. Bei getrennten Projekten fällt es sofort auf: `Geometrieeditor.Datenhaltung` bräuchte eine Referenz auf `Geometrieeditor.Web`, das seinerseits auf die Datenhaltung verweist – ein Zirkelbezug, den `dotnet build` ablehnt. Der Code kann also nur entstanden sein, weil alles in einem Projekt liegt.

**Schritt 2 — Information nach oben reichen:**

Die Datenhaltung darf melden, was passiert ist, aber nicht entscheiden, wie es angezeigt wird. Zwei Wege: ein Rückgabewert oder eine Exception. Da `Laden()` bereits eine Liste zurückgibt und „Datei fehlt“ ein Ausnahmefall ist, passt eine Exception – am besten eine eigene, damit die Verwaltung sie von Programmierfehlern unterscheiden kann:

```csharp
namespace Geometrieeditor.Fachkonzept;

public class SpeicherException : Exception
{
    public SpeicherException(string meldung, Exception? innere = null)
        : base(meldung, innere) { }
}
```

Sie liegt im Fachkonzept, neben `IFigurSpeicher` – denn sie ist Teil des Vertrags, den das Interface beschreibt. Die Datenhaltung wirft sie:

```csharp
public List<Figur> Laden()
{
    if (!File.Exists(pfad))
    {
        throw new SpeicherException($"Datei {pfad} nicht gefunden.");
    }
    try
    {
        string json = File.ReadAllText(pfad);
        return JsonSerializer.Deserialize<List<Figur>>(json, optionen) ?? new List<Figur>();
    }
    catch (JsonException ex)
    {
        throw new SpeicherException("Die Datei enthält kein gültiges Figuren-JSON.", ex);
    }
}
```

**Schritt 3 — Anzeigen in der Komponente:**

Die Komponente ist die einzige Schicht, die anzeigen darf. Sie fängt die Exception im Handler und schreibt die Meldung in die Statuszeile:

```razor
@code {
    private void Laden()
    {
        try
        {
            Verwaltung.Laden();
            ausgewaehlt = null;
            StatusAktualisieren();
        }
        catch (SpeicherException ex)
        {
            status = ex.Message;
        }
    }
}
```

Die Abhängigkeit zeigt jetzt wieder nur nach unten: Die Komponente kennt `SpeicherException` aus dem Fachkonzept, die Datenhaltung kennt nur das Fachkonzept.

**Schritt 4 — Die Reihenfolge in `FigurenVerwaltung.Laden`:**

```csharp
public void Laden()
{
    figuren.Clear();                     // zuerst löschen ...
    figuren.AddRange(speicher.Laden());  // ... dann laden – wirft das, sind die alten Figuren weg
}
```

Wirft `speicher.Laden()` die `SpeicherException`, ist die Liste bereits leer: Der Benutzer verliert seine ungespeicherten Figuren, nur weil die Datei fehlte. Die Reparatur ist eine Zeile Umstellung – erst laden, dann ersetzen:

```csharp
public void Laden()
{
    List<Figur> geladen = speicher.Laden();   // wirft ggf. – die Liste bleibt unangetastet
    figuren.Clear();
    figuren.AddRange(geladen);
}
```

**Zentrale Designentscheidungen:**

- **Eigene Exception im Fachkonzept:** `SpeicherException` gehört zum Vertrag `IFigurSpeicher`, nicht zur JSON-Implementierung. Eine spätere Datenbank-Datenhaltung wirft dieselbe Exception, und die Komponente muss nichts ändern.
- **Melden unten, entscheiden oben:** Die Datenhaltung weiß, *dass* etwas schiefging; nur die Oberfläche weiß, *wie* man es dem Benutzer sagt. Dazwischen reicht die Verwaltung die Exception einfach durch.
- **Unveränderter Zustand bei Fehlern:** Eine Operation, die fehlschlägt, sollte nichts halb erledigt hinterlassen. Erst das Riskante tun, dann den Zustand ändern – dieselbe Überlegung wie bei der Reihenfolge von Abheben und Einzahlen in Programmierung 1.

</details>
