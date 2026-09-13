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

Programmieren lernt man nicht nur durch Codezeilen tippen – sondern auch durch **Nachdenken**. Die folgenden Aufgaben trainieren *Computational Thinking*: die Fähigkeit, Probleme so zu strukturieren, dass ein Computer sie lösen kann. Bei Oberflächen und Architekturen heißt das vor allem: eine Seite in Komponenten zerlegen, Spielregeln von Anzeige trennen und Abhängigkeiten erkennen, bevor sie zum Problem werden. Alle vier Aufgaben spielen im Adventure; das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v04-blazor`). Nimm dir für jede Aufgabe Zeit, bevor du die Lösung aufklappst.

## Aufgabe 1 — Zerlegung

In `Home.razor` steht die Levelauswahl direkt im Markup der Seite: ein `<label>`, ein `<select>` mit `@bind` und `@bind:after`, eine `@foreach`-Schleife über `LevelQuelle.LevelNamen`. Zerlege das in eine eigene Komponente `Levelauswahl.razor`, die von außen die Liste der Namen bekommt und die getroffene Auswahl über einen `EventCallback<string>` zurückmeldet.

- Welche Parameter braucht die Komponente – und welche davon dürfen fehlen?
- Warum ein `EventCallback<string>` und kein gewöhnlicher `Action<string>`-Delegat?
- Wo lebt danach der aktuell gewählte Levelname: in der Komponente, in der Seite oder in beiden?
- Die Komponente soll die Liste anzeigen können, ohne `ILevelQuelle` zu kennen. Warum ist das besser, obwohl `@inject ILevelQuelle` auch in ihr funktionieren würde?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Schnittstelle der Komponente festlegen:**

Bevor eine Zeile Markup entsteht, klären wir, was hinein- und was herausfließt. Hinein: die Liste der Namen und der aktuell gewählte Name. Heraus: die neue Auswahl. Mehr braucht die Komponente nicht – und alles, was sie nicht braucht, bekommt sie auch nicht.

**Schritt 2 — Die Komponente:**

```razor
@* Components/Levelauswahl.razor *@

<label>Level:
    <select value="@Ausgewaehlt" @onchange="Gewechselt">
        @foreach (string name in Namen)
        {
            <option value="@name">@name</option>
        }
    </select>
</label>

@code {
    [Parameter, EditorRequired] public IReadOnlyList<string> Namen { get; set; } = [];
    [Parameter] public string Ausgewaehlt { get; set; } = "";
    [Parameter] public EventCallback<string> OnLevelGewaehlt { get; set; }

    private Task Gewechselt(ChangeEventArgs e)
    {
        return OnLevelGewaehlt.InvokeAsync(e.Value?.ToString() ?? "");
    }
}
```

Statt `@bind` steht hier das Paar `value="@Ausgewaehlt"` und `@onchange="Gewechselt"` – von Hand auseinandergezogen, weil die Komponente den Wert nicht selbst besitzt, sondern nur anzeigt und die Änderung weitermeldet. Das ist genau das, was `@bind` intern auch tut.

**Schritt 3 — Einbindung in `Home.razor`:**

```razor
<div class="werkzeuge">
    <Levelauswahl Namen="LevelQuelle.LevelNamen" Ausgewaehlt="levelName"
                  OnLevelGewaehlt="LevelWechseln" />
    <button @onclick="NeuStarten">Neu starten</button>
</div>

@code {
    private void LevelWechseln(string name)
    {
        levelName = name;
        NeuStarten();
    }
}
```

`LevelWechseln` ersetzt das frühere `@bind:after`: erst den Zustand ändern, dann das neue Level laden. Die Reihenfolge ist wichtig – stünde `NeuStarten()` zuerst, würde das alte Level noch einmal geladen.

**Schritt 4 — Die Variante mit `@bind-`:**

Blazor kennt eine Namenskonvention für Zweiweg-Bindung an eigene Komponenten: Heißt der Parameter `Ausgewaehlt` und der Rückkanal `AusgewaehltChanged` vom Typ `EventCallback<string>`, darf die Seite ihn wie ein eingebautes Eingabeelement binden:

```razor
<Levelauswahl Namen="LevelQuelle.LevelNamen" @bind-Ausgewaehlt="levelName"
              @bind-Ausgewaehlt:after="NeuStarten" />
```

Das ist eleganter, sobald die Komponente wirklich einen Wert *bearbeitet*. Für den Einstieg ist die explizite Variante aus Schritt 2 lehrreicher, weil man sieht, dass hinter `@bind` nichts Magisches steckt.

**Zentrale Designentscheidungen:**

- **`EventCallback<string>` statt `Action<string>`:** Nach einem `EventCallback` rendert Blazor die Elternkomponente automatisch neu. Mit einem gewöhnlichen Delegaten müsste `Home` selbst `StateHasChanged` aufrufen – eine Zeile, die man genau einmal vergisst und dann lange sucht.
- **Der Zustand bleibt oben:** `levelName` gehört weiterhin `Home`, denn `NeuStarten` braucht ihn. Die Komponente hat **kein** eigenes Feld für die Auswahl; sie zeigt an, was sie bekommt. Zwei Kopien desselben Werts sind zwei Gelegenheiten, auseinanderzulaufen.
- **Keine Abhängigkeit von `ILevelQuelle`:** Eine Komponente, die nur eine `IReadOnlyList<string>` braucht, lässt sich für Spielstände, Schwierigkeitsgrade oder Tastaturlayouts wiederverwenden – und in einem Test mit drei erfundenen Namen ausprobieren. Mit `@inject ILevelQuelle` wäre sie für immer an Level gekettet.
- **`EditorRequired` nur dort, wo es weh tut:** Ohne `Namen` ist die Komponente sinnlos, ohne `Ausgewaehlt` nur unschön. Deshalb ist nur der erste Parameter als erforderlich markiert.

</details>

## Aufgabe 2 — Abstraktion

Jemand hat die Tastatursteuerung „verbessert“. Die Komponente prüft jetzt selbst, was vor dem Spieler liegt:

```csharp
private void TasteGedrueckt(KeyboardEventArgs e)
{
    Richtung? richtung = e.Key switch
    {
        "ArrowUp" or "w" or "W" => Richtung.Oben,
        "ArrowDown" or "s" or "S" => Richtung.Unten,
        "ArrowLeft" or "a" or "A" => Richtung.Links,
        "ArrowRight" or "d" or "D" => Richtung.Rechts,
        _ => null
    };
    if (richtung is not Richtung r) return;

    Position ziel = feld.Spieler.Position.Verschoben(r);
    Spielobjekt? davor = feld.ObjektAn(ziel);

    if (davor is Wand)
    {
        meldung = "Da ist eine Wand.";
        return;
    }
    if (davor is Tuer tuer && !tuer.IstOffen)
    {
        if (feld.Spieler.Inventar.Enthaelt<Schluessel>())
        {
            meldung = tuer.Interagieren(feld.Spieler);
        }
        else
        {
            meldung = "Die Tür ist verschlossen. Du brauchst einen Schlüssel.";
        }
        return;
    }

    feld.SpielerZieht(r);
}
```

Es funktioniert – im Browser. Schiebe die Logik dorthin zurück, wo sie hingehört.

- Welche Zeilen bestehen die Konsolen-Probe aus dem [Architektur-Modul](/modules/schichten_architektur/schichten_architektur.md) und welche nicht?
- Wie verhält sich dieselbe Situation in `Adventure.Konsole` – und warum ist der Unterschied ein Fehler und nicht nur eine Unschönheit?
- Es steckt zusätzlich ein echter Spielfehler in diesem Code, den man erst beim Spielen bemerkt. Welcher?
- Wie sähe ein NUnit-Test für „verschlossene Tür ohne Schlüssel“ aus, und warum lässt er sich mit der Fassung oben nicht schreiben?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Zeilen sortieren:**

Der `switch`-Ausdruck über `e.Key` besteht die Probe **nicht** – `"ArrowUp"` ist ein Begriff des Browsers, die Konsole kennt `ConsoleKey.UpArrow`. Er bleibt also in der Komponente. Alles darunter besteht die Probe: Ob vor dem Spieler eine Wand steht, ob eine Tür verschlossen ist und ob ein Schlüssel im Inventar liegt, ist in jeder Oberfläche dieselbe Frage. Diese Zeilen gehören in den Kern – und stehen dort längst.

**Schritt 2 — Der Fehler, den man beim Spielen merkt:**

Die drei `return`-Anweisungen überspringen `feld.SpielerZieht(r)` – und damit den **Zug der Gegner**. Läuft der Spieler gegen eine Wand, passiert im ganzen Spiel nichts: Die Wachen patrouillieren nicht, die Verfolger rücken nicht nach. Eine Wand wird so zum Pausenknopf, hinter dem man sich beliebig lange verstecken kann. Das ist keine Stilfrage mehr, sondern eine kaputte Spielregel – und sie ist genau deshalb entstanden, weil eine Oberfläche über den Ablauf einer Runde entschieden hat.

**Schritt 3 — Die Regeln stehen schon im Kern:**

`Spielfeld.SpielerZieht` behandelt beide Fälle korrekt und lässt in **jedem** Fall die Gegner ziehen:

```csharp
public void SpielerZieht(Richtung richtung)
{
    if (Status != Spielstatus.Laeuft) return;

    Runde++;
    StringBuilder meldung = new();
    Position ziel = Spieler.Position.Verschoben(richtung);
    StatischesObjekt? davor = StatischesObjektAn(ziel);

    if (davor is IInteragierbar interagierbar && !davor.IstPassierbar)
    {
        // Vor einer verschlossenen Tür oder einer Truhe: interagieren statt gehen.
        meldung.Append(interagierbar.Interagieren(Spieler));
    }
    else if (Spieler.Bewegen(richtung, this))
    {
        // ... Gegenstand aufheben, Ausgang prüfen ...
    }
    else
    {
        meldung.Append("Da geht es nicht weiter.");
    }

    if (Status == Spielstatus.Laeuft)
    {
        GegnerZiehen(meldung);
    }

    LetzteMeldung = meldung.ToString().Trim();
    RundeBeendet?.Invoke(this, new RundeEventArgs(Runde, LetzteMeldung));
}
```

Beachte den Unterschied in der Abstraktionshöhe: Der Kern fragt nicht `davor is Tuer`, sondern `davor is IInteragierbar` – das [Interface aus Vorlesung 02](/modules/interfaces_grundlagen/interfaces_grundlagen.md). Dadurch funktioniert derselbe Zweig für Türen *und* Truhen und für alles, was später dazukommt. Die Fassung in der Komponente hätte für jede neue Objektart ein weiteres `if` gebraucht.

**Schritt 4 — Die Komponente schrumpft zurück:**

```csharp
private void TasteGedrueckt(KeyboardEventArgs e)
{
    Richtung? richtung = e.Key switch
    {
        "ArrowUp" or "w" or "W" => Richtung.Oben,
        "ArrowDown" or "s" or "S" => Richtung.Unten,
        "ArrowLeft" or "a" or "A" => Richtung.Links,
        "ArrowRight" or "d" or "D" => Richtung.Rechts,
        _ => null
    };
    if (richtung is Richtung r)
    {
        feld.SpielerZieht(r);
    }
}
```

Das Feld `meldung` in der Komponente entfällt ebenfalls: Die Meldung steht nach dem Zug in `feld.LetzteMeldung` und wird über den Parameter `Meldung` an die `Statusleiste` gereicht. Eine Kopie weniger, die veralten kann.

**Schritt 5 — Der Test, der vorher unmöglich war:**

```csharp
[Test]
public void SpielerZieht_VerschlosseneTuerOhneSchluessel_SpielerBleibtStehen()
{
    Spielfeld feld = LevelParser.Parsen(new Level("Test", ["###", "#@D", "###"]));
    Position vorher = feld.Spieler.Position;

    feld.SpielerZieht(Richtung.Rechts);

    Assert.That(feld.Spieler.Position, Is.EqualTo(vorher));
    Assert.That(feld.LetzteMeldung, Does.Contain("Schlüssel"));
}
```

Mit der ursprünglichen Fassung wäre dieser Test nicht schreibbar gewesen: Die Regel lebte in einer Razor-Komponente, und um sie zu prüfen, hätte man einen Browser, eine SignalR-Verbindung und ein simuliertes `keydown`-Ereignis gebraucht. Testbarkeit ist kein Nebeneffekt sauberer Schichten – sie ist ihr bestes Messgerät.

**Zentrale Designentscheidungen:**

- **Übersetzen ja, entscheiden nein:** Eine GUI darf Eingaben in Begriffe des Fachkonzepts übersetzen (`"ArrowUp"` → `Richtung.Oben`). Sobald sie *entscheidet*, was daraus folgt, ist die Grenze überschritten.
- **Eine Runde ist unteilbar:** `SpielerZieht` ist die kleinste sinnvolle Einheit des Spiels – Spielerzug *und* Gegnerzug. Wer sie von außen aufbricht, bekommt Zustände, die die Spielregeln nie vorgesehen haben.
- **Interfaces statt Typaufzählung:** `is IInteragierbar` bleibt richtig, wenn neue Objektarten dazukommen; `is Tuer || is Truhe` muss jedes Mal angefasst werden.
- **Ein Zustand, eine Quelle:** `LetzteMeldung` gehört dem Spielfeld. Die Komponente zeigt sie an, statt eine zweite Fassung zu führen.

</details>

## Aufgabe 3 — Algorithmenentwurf

Die Statusleiste zeigt das Inventar bisher als eine Zeile Text: `@Spieler.Inventar` ruft `Inventar<T>.ToString()` auf, das die Namen mit Komma verbindet. Entwirf stattdessen eine Komponente `Inventarpanel.razor`, die jeden Gegenstand als Kachel mit Symbol und Namen zeigt, gleiche Gegenstände zusammenfasst („🔑 Schlüssel ×2“) und bei leerem Inventar einen Hinweis anzeigt.

- Welchen Typ hat der Parameter – `Inventar<Gegenstand>`, `IEnumerable<Gegenstand>` oder `List<string>`? Was gewinnt und was verliert man jeweils?
- Wie fasst du gleiche Gegenstände zusammen, ohne den Kern zu ändern?
- Das Emoji für einen Schlüssel steht bereits in `SymbolFuer` in `Home.razor`. Wie vermeidest du, es ein zweites Mal hinzuschreiben – und in welches Projekt gehört die gemeinsame Stelle?
- Später soll ein Klick auf eine Kachel den Gegenstand benutzen. Welche Teile davon gehören in die Komponente, welche in den Kern?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Die gemeinsame Symboltabelle herausziehen:**

`SymbolFuer` wird an zwei Stellen gebraucht: auf dem Spielfeld und im Panel. Doppelt schreiben wäre die Garantie dafür, dass ein neues Objekt irgendwann an einer Stelle als Fragezeichen erscheint. Also wandert die Methode in eine eigene statische Klasse – **im Web-Projekt**, nicht im Kern:

```csharp
using Adventure.Kern;

namespace Adventure.Web;

/// <summary>Die Übersetzung von Spielobjekten in Emojis – reine Darstellung.</summary>
public static class Symbole
{
    public static string Fuer(Spielobjekt? objekt) => objekt switch
    {
        null => "",
        Wand => "🧱",
        Spieler => "🧝",
        Schluessel => "🔑",
        Trank => "🧪",
        Schatz => "💰",
        // ... wie gehabt ...
        _ => objekt.Symbol.ToString()
    };
}
```

Der Kern wäre der falsche Ort: Er hat mit `Spielobjekt.Symbol` bereits eine Darstellung – ein `char` für die Konsole. Ein zweites, browserspezifisches Symbol dort einzubauen hieße, dem Fachkonzept eine Oberfläche aufzudrängen. Emojis sind Sache derjenigen Schicht, die Emojis anzeigen kann.

**Schritt 2 — Die Komponente:**

```razor
@* Components/Inventarpanel.razor *@
@using Adventure.Kern

<div class="inventar">
    <h3>Inventar</h3>
    @if (!Inhalt.Any())
    {
        <p class="leer">Noch nichts gefunden.</p>
    }
    else
    {
        @foreach (var gruppe in Inhalt.GroupBy(g => g.Name))
        {
            <div class="kachel" title="@gruppe.Key">
                <span class="symbol">@Symbole.Fuer(gruppe.First())</span>
                <span class="name">@gruppe.Key</span>
                @if (gruppe.Count() > 1)
                {
                    <span class="anzahl">×@gruppe.Count()</span>
                }
            </div>
        }
    }
</div>

@code {
    [Parameter, EditorRequired]
    public IEnumerable<Gegenstand> Inhalt { get; set; } = [];
}
```

```css
.inventar { margin-top: 1rem; }
.inventar h3 { font-size: 1rem; margin: 0 0 0.4rem; }
.kachel { display: flex; align-items: center; gap: 0.5rem; padding: 0.2rem 0.4rem;
          background: #1e1e2e; border-radius: 4px; margin-bottom: 0.3rem; }
.kachel .symbol { font-size: 1.3rem; }
.kachel .anzahl { margin-left: auto; color: #999; }
.inventar .leer { color: #777; font-style: italic; }
```

Eingebunden wird sie in der `Statusleiste` – dort, wo bisher die Textzeile stand:

```razor
<Inventarpanel Inhalt="Spieler.Inventar" />
```

Das funktioniert ohne Umweg, weil `Inventar<T>` das Interface `IEnumerable<T>` implementiert (siehe [Interfaces](/modules/interfaces_grundlagen/interfaces_grundlagen.md)) – die Komponente bekommt also genau das, was sie braucht, und nichts weiter.

**Schritt 3 — Der Klick auf eine Kachel:**

Die Komponente meldet nur, *dass* geklickt wurde, und *was* geklickt wurde:

```razor
<div class="kachel" @onclick="() => OnBenutzen.InvokeAsync(gruppe.First())">

@code {
    [Parameter] public EventCallback<Gegenstand> OnBenutzen { get; set; }
}
```

Die Startseite reicht das an den Kern weiter – und der Kern braucht dafür eine neue Fachoperation, etwa `Spielfeld.Benutzen(Gegenstand)`. Was ein Trank bewirkt, ob ein Schlüssel verbraucht wird und ob das Benutzen eine Runde kostet (die Gegner also ziehen), sind allesamt Spielregeln. Die Komponente darf davon nur wissen, dass sie hinterher neu rendern muss – und das tut Blazor von allein.

**Zentrale Designentscheidungen:**

- **`IEnumerable<Gegenstand>` als Parametertyp:** Der engste Typ, der reicht. `Inventar<Gegenstand>` würde die Komponente an eine konkrete Klasse binden; `List<string>` würde die Objekte plattdrücken und die Symbolwahl unmöglich machen. So kann man das Panel auch mit dem Inhalt einer Truhe oder einer Testliste füttern.
- **Gruppieren in der Anzeige, nicht im Kern:** `GroupBy` ist eine Darstellungsentscheidung – das Inventar *hat* zwei Schlüssel, es *zeigt* sie nur zusammengefasst. Würde man den Kern umbauen, verlöre man die Möglichkeit, zwei Schlüssel getrennt zu verbrauchen.
- **Symbole ins Web-Projekt:** Das Fachkonzept kennt `char`-Symbole, die GUI-Schicht kennt Emojis. Jede Schicht darf ihre eigene Darstellung haben – aber nur einmal.
- **Leerer Zustand ist ein Zustand:** „Noch nichts gefunden.“ ist kein Schmuck. Eine Anzeige, die bei leerer Liste einfach verschwindet, wirkt wie ein Fehler.

</details>

## Aufgabe 4 — Mustererkennung

In einem Projekt sieht das Abhängigkeitsdiagramm des Adventures so aus:

```
   Adventure.Web
   Home.razor  ◄──────────────────────────────────┐
        │ nutzt ILevelQuelle                       │
        ▼                                          │
   Adventure.Kern                                  │ ruft seite.FehlerAnzeigen(...)
   Spielfeld, LevelParser, ILevelQuelle            │
        ▲ implementiert                            │
        │                                          │
   Adventure.Daten                                 │
   EingebauteLevelQuelle  ─────────────────────────┘
```

Die `EingebauteLevelQuelle` hat ein Feld `Home seite` bekommen, damit sie bei einem unbekannten Levelnamen eine Meldung in der Statusleiste anzeigen kann:

```csharp
public Level Laden(string name)
{
    if (!level.TryGetValue(name, out string[]? zeilen))
    {
        seite.FehlerAnzeigen($"Es gibt kein Level namens '{name}'.");
        return new Level(name, ["###", "#@#", "###"]);   // Notfall-Level
    }
    return new Level(name, zeilen);
}
```

- Welche der vier Schichtregeln ist verletzt – und woran würde der Compiler es bemerken, weil die Schichten eigene Projekte sind?
- Wie kommt die Information „Level unbekannt“ nach oben, ohne dass die Datenschicht die Oberfläche kennt?
- Wer entscheidet, was bei einem Fehler passiert – die Datenschicht, der Kern oder die Komponente?
- Was passiert in der Konsolenversion mit diesem Code? Und was wäre passiert, wenn `NeuStarten` sein Spielfeld *vor* dem Laden leeren würde?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Das Muster erkennen:**

Verletzt ist Regel 2: Eine Schicht darf nie von einer höheren abhängen. `Adventure.Daten` ist die unterste Schicht und kennt jetzt die oberste – das ist genau das „nur mal kurz eine Meldung anzeigen“, vor dem das Architektur-Modul warnt. Nebenbei ist auch Regel 1 verletzt, denn die Datenschicht überspringt den Kern.

Bei getrennten Projekten fällt es sofort auf: `Adventure.Daten` bräuchte eine Referenz auf `Adventure.Web`, das seinerseits auf `Adventure.Daten` verweist – ein Zirkelbezug, den `dotnet build` ablehnt (Fehler NU1108 bzw. „A circular dependency was detected“). Der Code kann also gar nicht erst entstehen, solange die Projektgrenzen stimmen. Und in der Konsolenversion existiert die Klasse `Home` überhaupt nicht: `Adventure.Konsole` ließe sich nicht mehr kompilieren. Eine einzige „hilfreiche“ Zeile hätte eine ganze Oberfläche mitgerissen.

**Schritt 2 — Information nach oben reichen:**

Die Datenschicht darf melden, *was* passiert ist, aber nicht entscheiden, *wie* es angezeigt wird. Zwei Wege: ein Rückgabewert oder eine Exception. Da `Laden` bereits ein `Level` zurückgibt und „Level unbekannt“ ein Ausnahmefall ist, passt eine Exception – am besten eine eigene, damit der Aufrufer sie von Programmierfehlern unterscheiden kann:

```csharp
namespace Adventure.Kern;

public class LevelException : Exception
{
    public LevelException(string meldung, Exception? innere = null)
        : base(meldung, innere) { }
}
```

Sie liegt im **Kern**, direkt neben `ILevelQuelle` – denn sie ist Teil des Vertrags, den das Interface beschreibt. Die Datenschicht wirft sie:

```csharp
public Level Laden(string name)
{
    if (!level.TryGetValue(name, out string[]? zeilen))
    {
        throw new LevelException($"Es gibt kein Level namens '{name}'.");
    }
    return new Level(name, zeilen);
}
```

Eine spätere `TextdateiLevelQuelle` wirft dieselbe Exception, wenn die Datei fehlt oder unlesbar ist – und keine Komponente muss dafür angefasst werden.

**Schritt 3 — Anzeigen in der Komponente:**

Die GUI-Schicht ist die einzige, die anzeigen darf. Sie fängt die Exception dort, wo sie entstehen kann:

```csharp
private string fehler = "";

private void NeuStarten()
{
    try
    {
        feld = LevelParser.Parsen(LevelQuelle.Laden(levelName));
        fehler = "";
    }
    catch (LevelException ex)
    {
        fehler = ex.Message;
    }
}
```

Im Markup zeigt ein `@if (fehler != "")` den Text an. Die Abhängigkeit zeigt jetzt wieder nur nach unten: Die Komponente kennt `LevelException` aus dem Kern, die Datenschicht kennt nur den Kern. Die Konsolenversion fängt dieselbe Exception und schreibt sie mit `Console.WriteLine` – dieselbe Information, eine andere Darstellung, und genau das ist der Sinn der Übung.

**Schritt 4 — Die Reihenfolge in `NeuStarten`:**

Die Originalfassung ist nicht zufällig einzeilig:

```csharp
private void NeuStarten()
{
    feld = LevelParser.Parsen(LevelQuelle.Laden(levelName));
}
```

Die Zuweisung an `feld` passiert **zuletzt**. Wirft `Laden` oder `Parsen`, behält `feld` sein altes Spielfeld, und die Seite zeigt weiter das laufende Spiel. Hätte jemand „zum Aufräumen“ vorher `feld = null!;` geschrieben, wäre der Zustand nach einem Fehler kaputt: Das nächste Rendern liefe in eine `NullReferenceException` beim Zugriff auf `feld.Breite` – aus einem harmlosen Tippfehler im Levelnamen würde eine abgestürzte Seite. Erst das Riskante tun, dann den Zustand ändern.

**Zentrale Designentscheidungen:**

- **Eigene Exception im Kern:** `LevelException` gehört zum Vertrag `ILevelQuelle`, nicht zu einer bestimmten Implementierung. Datei, Netz oder fest im Code – die Oberfläche fängt immer denselben Typ.
- **Melden unten, entscheiden oben:** Die Datenschicht weiß, *dass* etwas schiefging; nur die Oberfläche weiß, *wie* man es dem Benutzer sagt. Ein Notfall-Level stillschweigend zurückzugeben, ist die schlechteste aller Antworten – der Fehler verschwindet, ohne behoben zu sein.
- **Zwei Oberflächen als Prüfmittel:** Die Verletzung wäre in einem Einzelprojekt vielleicht monatelang unbemerkt geblieben. Weil es `Adventure.Konsole` gibt, bricht der Build sofort.
- **Unveränderter Zustand bei Fehlern:** Eine Operation, die fehlschlägt, sollte nichts halb erledigt hinterlassen – dieselbe Überlegung wie bei der Reihenfolge von Abheben und Einzahlen in Programmierung 1.

</details>
