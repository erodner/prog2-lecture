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

In der ersten Blazor-App haben wir etwas benutzt, ohne es zu hinterfragen: Der Klick-Handler `Begruessen` hat ein Feld `ausgabe` gesetzt – und der Absatz `<p>@ausgabe</p>` zeigte danach den neuen Text. Nirgends stand „schreibe den Text in das Absatz-Element“. Das ist keine Magie, sondern ein Modell, das man sich einmal klarmachen sollte, weil es die Art bestimmt, wie man in Blazor über Oberflächen denkt. Die Analogie ist eine Tabellenkalkulation: Eine Zelle mit der Formel `=A1*2` merkt sich nicht, was in `A1` stand, sondern wird jedes Mal neu berechnet, wenn sich `A1` ändert. Das Razor-Markup ist so eine Formel – es beschreibt, wie die Seite *für einen gegebenen Zustand* aussieht, und Blazor rechnet sie nach jedem Ereignis neu aus.

## Der Zustand liegt in Feldern

Alles, was eine Komponente anzeigt oder was der Benutzer in ihr ändert, liegt in Feldern des `@code`-Blocks. Die Startseite von HalloBlazor hat drei davon:

```razor
<input @bind="name" placeholder="Name eingeben" />
<button @onclick="Begruessen">Begrüßen</button>

<p class="ausgabe">@ausgabe</p>

@code {
    private string name = "";
    private string ausgabe = "";
    private int anzahlKlicks = 0;

    private void Begruessen()
    {
        anzahlKlicks++;
        string wer = string.IsNullOrWhiteSpace(name) ? "Unbekannte:r" : name;
        ausgabe = $"Hallo, {wer}! (Klick Nr. {anzahlKlicks})";
    }
}
```

Die Komponente ist eine ganz normale C#-Klasse, die Felder sind ihr Objektzustand, und `Begruessen` ist eine Methode, die diesen Zustand verändert – so weit nichts Neues. Neu ist nur, dass das Markup und die Felder in beide Richtungen verbunden sind. Das vollständige Projekt findest du im Repository unter `examples/04_blazor/HalloBlazor`.

## Einweg-Bindung: vom Feld ins Markup

Ein `@` vor einem Ausdruck im Markup fügt seinen Wert an dieser Stelle ein. Das ist die **Einweg-Bindung**: Daten fließen vom Feld in die Seite, nie zurück. Der Geometrieeditor nutzt sie an vielen Stellen – als Text, als Methodenaufruf, als Attributwert und als CSS-Klasse:

```razor
<button @onclick="Entfernen" disabled="@(ausgewaehlt is null)">Entfernen</button>

@foreach (Figur figur in Verwaltung.AlleFiguren)
{
    <li class="@(figur == ausgewaehlt ? "ausgewaehlt" : "")"
        @onclick="() => ausgewaehlt = figur">@figur.Name</li>
}

<p>Umfang: @ausgewaehlt.Umfang.ToString("F2")</p>
<p class="status">@status</p>
```

Bei `disabled="@(ausgewaehlt is null)"` ist der Wert ein `bool`; Blazor setzt das Attribut nur, wenn er `true` ist. Kompliziertere Ausdrücke stehen in Klammern `@( ... )`, damit klar ist, wo der C#-Teil endet.

## Zweiweg-Bindung mit `@bind`

Bei Eingabeelementen soll die Verbindung in beide Richtungen gehen: Das Feld füllt das Textfeld, und was der Benutzer tippt, landet wieder im Feld. Genau das leistet `@bind`:

```razor
<input @bind="name" />
```

Blazor macht daraus zwei Dinge: Es setzt das `value`-Attribut aus dem Feld, und es registriert einen Handler für das Ereignis `onchange`, der das Feld aus der Eingabe aktualisiert. `onchange` feuert erst, wenn das Textfeld den Fokus verliert oder der Benutzer Enter drückt. Soll das Feld bei jedem Tastendruck aktuell sein, wechselt man das Ereignis:

```razor
<input @bind="name" @bind:event="oninput" />
<p>@name.Length Zeichen</p>
```

Damit zählt der Absatz beim Tippen mit. Für den Geometrieeditor reicht `onchange`, denn dort werden die Eingaben erst beim Klick auf „OK“ ausgewertet.

`@bind` und ein eigener `@onchange`-Handler am selben Element vertragen sich nicht – `@bind` belegt das Ereignis bereits, und der Compiler meldet einen Fehler. Wer nach einer Eingabe noch etwas tun will, hängt eine Methode mit `@bind:after="Methode"` an; sie läuft, nachdem das Feld aktualisiert wurde.
{: .notice--warning}

## `@bind` bei Auswahllisten und Checkboxen

`@bind` funktioniert mit jedem Eingabeelement, nicht nur mit Textfeldern. Der `NeueFigurDialog` bindet ein `<select>` an ein `string`-Feld; der gewählte `value` der `<option>` landet im Feld, und eine berechnete Property nutzt ihn sofort für die Beschriftung:

```razor
<select @bind="art">
    <option value="Rechteck">Rechteck</option>
    <option value="Kreis">Kreis</option>
    <option value="Dreieck">Dreieck</option>
</select>

<label>@MasseBeschriftung <input @bind="masse" placeholder="z. B. 4, 3" /></label>

@code {
    private string art = "Rechteck";
    private string masse = "";

    private string MasseBeschriftung => art switch
    {
        "Rechteck" => "Breite, Höhe:",
        "Kreis" => "Radius:",
        _ => "Seiten a, b, c:"
    };
}
```

Wählt der Benutzer „Kreis“, ändert sich `art`, und beim nächsten Rendern zeigt das Label „Radius:“ – ohne dass irgendwo ein Handler das Label anfasst. Eine Checkbox bindet man auf dieselbe Weise an ein `bool`:

```razor
<label><input type="checkbox" @bind="gefuellt" /> Figur ausfüllen</label>

@code {
    private bool gefuellt = false;
}
```

Blazor wählt je nach Elementtyp das passende Attribut: `value` beim Textfeld und `<select>`, `checked` bei der Checkbox.

## Der Render-Zyklus

Warum aktualisiert sich die Figurenliste im Geometrieeditor von selbst, nachdem `DialogGeschlossen` die Zeile `Verwaltung.Hinzufuegen(neueFigur)` ausgeführt hat? Weil Blazor nach **jedem Ereignishandler** die Komponente neu rendert: Es wertet das gesamte Markup mit dem aktuellen Zustand aus, vergleicht das Ergebnis mit dem vorigen Rendern und schickt nur die **Unterschiede** über die SignalR-Verbindung an den Browser. Die `@foreach`-Schleife über `Verwaltung.AlleFiguren` läuft dabei einfach noch einmal – jetzt mit einem Element mehr –, und der Browser bekommt genau das eine neue `<li>`.

Das unterscheidet Blazor von klassischen Desktop-Frameworks. Dort hält jedes Steuerelement seinen eigenen Zustand, und der Handler muss ihn von Hand nachziehen: `label.Text = ...`, `listBox.Items.Add(...)`, `button.Enabled = false`. Vergisst man eine Stelle, zeigt die Oberfläche veraltete Daten. In Blazor gibt es diese Stellen nicht: Es gibt nur den Zustand in den Feldern und das Markup, das ihn beschreibt.
{: .notice--primary}

Die Regel für den Alltag lautet also: Ein Handler verändert Felder – und sonst nichts. Anzeigen ist Aufgabe des Markups.

## `StateHasChanged` – wenn kein Ereignis im Spiel ist

Das automatische Rendern hängt an Blazor-Ereignissen: Klick, Eingabe, ein `EventCallback` einer Kindkomponente. Ändert sich der Zustand *außerhalb* eines solchen Ereignisses – ein Timer läuft ab, eine Hintergrundaufgabe wird fertig –, weiß Blazor nichts davon. Dann sagt man es ihm mit `StateHasChanged()`:

```razor
<p>Uhrzeit: @jetzt.ToLongTimeString()</p>

@code {
    private DateTime jetzt = DateTime.Now;
    private Timer? timer;

    protected override void OnInitialized()
    {
        timer = new Timer(_ =>
        {
            jetzt = DateTime.Now;
            InvokeAsync(StateHasChanged);   // Blazor: bitte neu rendern
        }, null, 0, 1000);
    }
}
```

Der Umweg über `InvokeAsync` ist nötig, weil der Timer auf einem anderen Thread tickt als die Komponente; `InvokeAsync` reiht den Aufruf in den Thread der Komponente ein. In gewöhnlichen Handlern brauchst du `StateHasChanged` dagegen nie – wer es dort aufruft, rendert nur doppelt.

Übung: Erweitere den `NeueFigurDialog` so, dass unter dem Maße-Textfeld live die Fläche der Figur erscheint, noch bevor der Benutzer auf „OK“ klickt. Welches Ereignis brauchst du bei `@bind`, und wo berechnest du die Fläche – in einem Handler oder in einer Property?
{: .notice--info}

## Weitere Quellen

- [Datenbindung in ASP.NET Core Blazor – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/data-binding)
- [Rendern von Razor-Komponenten – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/rendering)
- [Ereignisbehandlung in Blazor – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/event-handling)
