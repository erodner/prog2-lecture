---
title: "Datenbindung und Rendern"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

In den letzten Modulen haben wir etwas benutzt, ohne es zu hinterfragen: `TasteGedrueckt` ruft `feld.SpielerZieht(r)` auf – und danach steht der Held ein Feld weiter, die Wache hat sich bewegt, die Lebenspunkte sind vielleicht um eins gesunken und in der Statusleiste steht eine neue Meldung. Nirgends steht „zeichne die Zelle bei (4, 7) neu“. Das ist keine Magie, sondern ein Modell, das man sich einmal klarmachen sollte, weil es die Art bestimmt, wie man in Blazor über Oberflächen denkt. Die Analogie ist eine Tabellenkalkulation: Eine Zelle mit der Formel `=A1*2` merkt sich nicht, was in `A1` stand, sondern wird jedes Mal neu berechnet, wenn sich `A1` ändert. Das Razor-Markup ist so eine Formel – es beschreibt, wie die Seite *für einen gegebenen Zustand* aussieht, und Blazor rechnet sie nach jedem Ereignis neu aus.

## Der Zustand liegt in Feldern

Alles, was eine Komponente anzeigt oder was der Benutzer in ihr ändert, liegt in Feldern des `@code`-Blocks. Die Startseite des Spiels hat genau drei davon:

```razor
@code {
    private Spielfeld feld = null!;
    private string levelName = "";
    private ElementReference feldElement;

    protected override void OnInitialized()
    {
        levelName = LevelQuelle.LevelNamen[0];
        NeuStarten();
    }
}
```

Bemerkenswert ist, was **nicht** in dieser Liste steht: keine Position des Spielers, keine Lebenspunkte, keine Liste der Gegner, kein Spielstatus. All das steckt im `Spielfeld`, und die Komponente hält nur einen Verweis darauf. Sie hat keine zweite, eigene Kopie des Spielzustands, die sie mit dem Kern synchron halten müsste – und damit kann sie auch nichts falsch synchronisieren. `levelName` ist das einzige, was wirklich der Oberfläche gehört: die Auswahl in einer Liste.

`OnInitialized` ist eine Lebenszyklus-Methode; Blazor ruft sie einmal auf, bevor die Komponente zum ersten Mal gerendert wird. Hier ist der richtige Ort, um das erste Level zu laden – im Feldinitialisierer ginge es nicht, weil dort der eingefügte `LevelQuelle` noch nicht zur Verfügung steht.

## Einweg-Bindung: vom Zustand ins Markup

Ein `@` vor einem Ausdruck im Markup fügt seinen Wert an dieser Stelle ein. Das ist die **Einweg-Bindung**: Daten fließen vom Feld in die Seite, nie zurück. Das Spiel nutzt sie überall – als Text, als Methodenaufruf, als Attributwert und als CSS-Klasse:

```razor
<div class="spielfeld" style="grid-template-columns: repeat(@feld.Breite, 1fr);">
    ...
    <div class="feld @KlasseFuer(objekt)" title="@objekt?.Beschreibung()">@SymbolFuer(objekt)</div>
</div>

<Statusleiste Spieler="feld.Spieler" Runde="feld.Runde" Meldung="feld.LetzteMeldung" />
```

Auch die Parameter einer Komponente sind eine Einweg-Bindung: `Runde="feld.Runde"` liest den aktuellen Wert und gibt ihn nach unten weiter. Innerhalb der `Statusleiste` – der Komponente aus [Razor-Komponenten und Steuerelemente](/modules/razor_komponenten/razor_komponenten.md) – entsteht daraus die Herzenanzeige, und zwar nicht in einem Handler, sondern in einer berechneten Property:

```csharp
private string Herzen => new string('♥', Spieler.Lebenspunkte)
                       + new string('♡', Spieler.MaxLebenspunkte - Spieler.Lebenspunkte);
```

Das Markup `<span class="herzen">@Herzen</span>` ruft diese Property bei jedem Rendern neu auf. Es gibt keinen Code, der bei einem Treffer ein Herz „wegnimmt“ – die Property liest schlicht den aktuellen Wert von `Lebenspunkte` und baut die Zeichenkette daraus. Verliert der Spieler einen Punkt, ist beim nächsten Rendern ein `♥` weniger da. Genau das ist der Unterschied zwischen *beschreiben* und *befehlen*.

## Zweiweg-Bindung mit `@bind`

Bei Eingabeelementen soll die Verbindung in beide Richtungen gehen: Das Feld füllt das Steuerelement, und was der Benutzer wählt oder tippt, landet wieder im Feld. Genau das leistet `@bind`. Im Spiel gibt es dafür nur eine Stelle, die Levelauswahl:

```razor
<select @bind="levelName" @bind:after="NeuStarten">
    @foreach (string name in LevelQuelle.LevelNamen)
    {
        <option value="@name">@name</option>
    }
</select>
```

Blazor macht daraus zwei Dinge: Es setzt den `value` des `<select>` aus dem Feld `levelName`, und es registriert einen Handler für das Ereignis `change`, der `levelName` aus der Auswahl aktualisiert. Der gewählte `value` einer `<option>` ist ein `string`, und `levelName` ist ein `string` – die Typen passen zusammen. Bei einem `<input type="number">` würde Blazor den Text automatisch in ein `int` oder `double` umwandeln, bei einer Checkbox in ein `bool` und dabei `checked` statt `value` verwenden.

Bei einem Textfeld feuert `change` erst, wenn das Feld den Fokus verliert oder der Benutzer Enter drückt. Soll das Feld bei jedem Tastendruck aktuell sein, wechselt man das Ereignis mit `@bind:event="oninput"`. Warum am selben Element kein eigener `@onchange`-Handler stehen darf und wofür stattdessen `@bind:after` da ist, steht in [Ereignisse in Blazor](/modules/blazor_ereignisse/blazor_ereignisse.md).
{: .notice--warning}

## Der Render-Zyklus

Warum sieht man den Zug des Spielers, obwohl `TasteGedrueckt` nur `feld.SpielerZieht(r)` aufruft? Weil Blazor nach **jedem Ereignishandler** die Komponente neu rendert: Es wertet das gesamte Markup mit dem aktuellen Zustand aus, vergleicht das Ergebnis mit dem vorigen Rendern und schickt nur die **Unterschiede** über die SignalR-Verbindung an den Browser.

Konkret: Die beiden `for`-Schleifen laufen erneut über alle Zellen und fragen für jede `feld.ObjektAn(...)`. Der allergrößte Teil des erzeugten HTML ist identisch mit dem vorigen Stand – geändert haben sich vielleicht vier Zellen (alte und neue Position des Spielers, alte und neue Position einer Wache) und ein paar Zeilen in der Statusleiste. Nur diese Unterschiede gehen über die Leitung. Wir schreiben Code, als würden wir die ganze Seite neu bauen; der Browser bekommt trotzdem nur die vier Zellen.

Das unterscheidet Blazor von klassischen Desktop-Frameworks. Dort hält jedes Steuerelement seinen eigenen Zustand, und der Handler muss ihn von Hand nachziehen: `label.Text = ...`, `zelle[4, 7].Bild = ...`, `button.Enabled = false`. Vergisst man eine Stelle, zeigt die Oberfläche veraltete Daten. In Blazor gibt es diese Stellen nicht: Es gibt nur den Zustand und das Markup, das ihn beschreibt.
{: .notice--primary}

Die Regel für den Alltag lautet also: Ein Handler verändert Zustand – und sonst nichts. Anzeigen ist Aufgabe des Markups. Im Fall des Spiels heißt „Zustand verändern“ sogar nur: eine einzige Methode des Kerns aufrufen.

## `StateHasChanged` – wenn kein Ereignis im Spiel ist

Das automatische Rendern hängt an Blazor-Ereignissen: Klick, Tastendruck, Eingabe, ein `EventCallback` einer Kindkomponente. Ändert sich der Zustand *außerhalb* eines solchen Ereignisses – ein Timer läuft ab, eine Hintergrundaufgabe wird fertig, ein C#-Ereignis des Kerns wird aus einem anderen Thread ausgelöst –, weiß Blazor nichts davon. Dann sagt man es ihm mit `StateHasChanged()`.

Angenommen, wir wollen eine Spieluhr anzeigen, die unabhängig von den Zügen des Spielers läuft:

```razor
<p>Spielzeit: @spielzeit.ToString(@"mm\:ss")</p>

@code {
    private TimeSpan spielzeit = TimeSpan.Zero;
    private Timer? uhr;

    protected override void OnInitialized()
    {
        uhr = new Timer(_ =>
        {
            spielzeit += TimeSpan.FromSeconds(1);
            InvokeAsync(StateHasChanged);   // Blazor: bitte neu rendern
        }, null, 1000, 1000);
    }
}
```

Der Umweg über `InvokeAsync` ist nötig, weil der Timer auf einem anderen Thread tickt als die Komponente; `InvokeAsync` reiht den Aufruf in den Thread der Komponente ein. Dasselbe gilt, wenn sich eine Komponente an ein C#-Ereignis des Kerns hängt – etwa `feld.Spieler.SchatzGefunden`, um kurz eine Animation zu zeigen: Wird das Ereignis innerhalb eines Blazor-Handlers ausgelöst, rendert Blazor ohnehin neu; kommt es von außen, braucht es `StateHasChanged`.

In gewöhnlichen Handlern brauchst du `StateHasChanged` dagegen **nie**. Wer es dort aufruft, rendert nur doppelt. Wenn eine Anzeige sich nicht aktualisiert, ist die Ursache fast immer eine andere: Der Zustand wurde gar nicht geändert, oder das Markup liest eine Kopie statt des Originals.
{: .notice--warning}

Übung: Ergänze die Statusleiste um eine Anzeige „Schlüssel: ja/nein“, die anzeigt, ob der Held bereits einen Schlüssel im Inventar hat (`Spieler.Inventar.Enthaelt<Schluessel>()`). Überlege dabei: Brauchst du ein neues Feld, einen neuen Parameter oder gar nichts von beidem? Und an welcher Stelle im Code müsstest du etwas ändern, damit die Anzeige *nicht* mehr automatisch aktuell bleibt – diese Gegenprobe zeigt dir, worauf das Modell beruht.
{: .notice--info}

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v04-blazor`).

## Weitere Quellen

- [Datenbindung in ASP.NET Core Blazor – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/data-binding)
- [Rendern von Razor-Komponenten – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/rendering)
- [Lebenszyklus von Razor-Komponenten – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/lifecycle)
- [Blazor University](https://blazor-university.com/) – die Kapitel zu One-way und Two-way Binding zerlegen `@bind` Schritt für Schritt in das, was der Compiler daraus macht.
- [Game Loop – Game Programming Patterns](https://gameprogrammingpatterns.com/game-loop.html) – der Vergleich lohnt sich: Ein Actionspiel rendert 60-mal pro Sekunde, unsere Seite nur nach einem Ereignis.
