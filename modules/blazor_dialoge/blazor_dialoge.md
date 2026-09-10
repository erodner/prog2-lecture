---
title: "Dialoge als Komponenten"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Eine neue Figur anzulegen braucht mehrere Eingaben: Art, Name, Position, Maße. Diese Felder dauerhaft auf der Startseite zu zeigen wäre verschwendeter Platz – sie werden nur kurz gebraucht, und dann soll der Benutzer wieder die Liste sehen. Dafür gibt es **Dialoge**: ein Bereich, der vor die Seite tritt, Eingaben sammelt und ein Ergebnis zurückliefert oder abgebrochen wird. Desktop-Frameworks öffnen dafür ein zweites Fenster und liefern ein `DialogResult` zurück. Im Browser gibt es keine Fenster – ein Dialog in Blazor ist einfach eine **Komponente**, die gezeigt oder nicht gezeigt wird. Man kann sich das wie ein Formular am Schalter vorstellen: Man bekommt es in die Hand, füllt es aus und gibt es zurück – oder zerreißt es. Wie das Formular aussieht und wie es zurückkommt, klären wir am `NeueFigurDialog` des Geometrieeditors.

## Der Dialog ist eine eigene Komponente

`NeueFigurDialog.razor` liegt im Ordner `Components` und hat keine `@page`-Anweisung – er ist keine Seite mit eigener Adresse, sondern ein Baustein, den andere Komponenten als Tag `<NeueFigurDialog />` einsetzen. Sein Markup ist ein gewöhnliches Formular mit `@bind` an den Feldern, wie wir es im Modul [Datenbindung](/modules/blazor_datenbindung/blazor_datenbindung.md) gesehen haben:

```razor
<div class="dialog-hintergrund">
    <div class="dialog">
        <h2>Neue Figur</h2>

        <label>Art:
            <select @bind="art">
                <option value="Rechteck">Rechteck</option>
                <option value="Kreis">Kreis</option>
                <option value="Dreieck">Dreieck</option>
            </select>
        </label>
        <label>Name: <input @bind="name" /></label>
        <label>Position: <input @bind="x" size="5" /> <input @bind="y" size="5" /></label>
        <label>@MasseBeschriftung <input @bind="masse" placeholder="z. B. 4, 3" /></label>

        <p class="fehler">@fehler</p>

        <div class="dialog-buttons">
            <button @onclick="Abbrechen">Abbrechen</button>
            <button @onclick="Ok">OK</button>
        </div>
    </div>
</div>
```

Der Dialog weiß nichts von der Startseite, von der Figurenliste oder von der `FigurenVerwaltung`. Er kennt nur seine eigenen Eingabefelder – und einen Weg, sein Ergebnis loszuwerden.

## Anzeigen durch bedingtes Rendern

Es gibt keinen Aufruf wie `dialog.Show()`. Stattdessen entscheidet ein `bool`-Feld der Startseite, ob der Dialog im Markup vorkommt:

```razor
<button @onclick="() => dialogOffen = true">Neue Figur …</button>

@if (dialogOffen)
{
    <NeueFigurDialog OnGeschlossen="DialogGeschlossen" />
}

@code {
    private bool dialogOffen = false;
}
```

Der Klick setzt `dialogOffen` auf `true`, Blazor rendert die Seite neu, und weil die Bedingung jetzt zutrifft, entsteht eine neue Instanz der Dialog-Komponente. Wird `dialogOffen` später wieder `false`, verschwindet die Komponente samt ihren Feldern – beim nächsten Öffnen beginnt der Dialog also leer. Das ist meistens genau das gewünschte Verhalten.

## Das Ergebnis: `EventCallback<Figur?>`

Wie kommt die fertige Figur zurück zur Startseite? Über einen **Parameter** vom Typ `EventCallback<T>`. Ein `EventCallback` ist eine Methode der Elternkomponente, die das Kind aufrufen darf – ein typisiertes Ereignis, dessen Argument hier die neue Figur ist:

```razor
@code {
    // Ergebnis des Dialogs: die neue Figur oder null bei Abbruch.
    [Parameter]
    public EventCallback<Figur?> OnGeschlossen { get; set; }

    private async Task Ok()
    {
        try
        {
            Figur figur = FigurAusEingaben();
            await OnGeschlossen.InvokeAsync(figur);
        }
        catch (Exception ex) when (ex is FormatException || ex is ArgumentException)
        {
            fehler = ex.Message;
        }
    }

    private Task Abbrechen() => OnGeschlossen.InvokeAsync(null);
}
```

`InvokeAsync(figur)` ruft den Handler der Eltern auf und übergibt die Figur; `InvokeAsync(null)` meldet den Abbruch. Klassische Frameworks liefern an dieser Stelle ein `DialogResult.OK` oder `DialogResult.Cancel`, und die Figur muss man sich anschließend aus Properties des Dialogs holen. Hier ist das Ergebnis **typisiert**: Der Handler bekommt direkt eine `Figur?`, und das `?` sagt dem Compiler, dass der Abbruch ein erlaubter Fall ist, den man prüfen muss.

Nach einem `EventCallback` rendert Blazor die Elternkomponente automatisch neu. Das ist der Unterschied zu einem gewöhnlichen `Action<Figur?>`-Delegaten – mit dem müsste die Startseite selbst `StateHasChanged` aufrufen. Für Kind-nach-Eltern-Kommunikation deshalb immer `EventCallback`.
{: .notice--primary}

## Der Handler der Eltern

Auf der Startseite steht die Methode, die als `OnGeschlossen` übergeben wurde. Sie schließt den Dialog und übergibt die Figur an das Fachkonzept:

```razor
@code {
    private void DialogGeschlossen(Figur? neueFigur)
    {
        dialogOffen = false;
        if (neueFigur is null)
        {
            status = "Abgebrochen.";
            return;
        }

        try
        {
            Verwaltung.Hinzufuegen(neueFigur);
            StatusAktualisieren();
        }
        catch (ArgumentException ex)
        {
            status = ex.Message;
        }
    }
}
```

`dialogOffen = false` als erste Zeile sorgt dafür, dass der Dialog beim Neu-Rendern verschwindet – egal, ob danach eine Figur hinzukommt oder nicht. Die Figur selbst wird nicht in eine Liste der Seite gesteckt, sondern an `Verwaltung.Hinzufuegen` gegeben; die `@foreach`-Schleife über `Verwaltung.AlleFiguren` zeigt sie beim nächsten Rendern von selbst.

## Validierung: zwei Ebenen

Wo werden Eingaben geprüft? Der Dialog prüft, was nur er wissen kann: ob die Textfelder überhaupt Zahlen enthalten und ob die Anzahl der Maße zur gewählten Art passt. Das erledigt `FigurAusEingaben`, gekürzt:

```csharp
private Figur FigurAusEingaben()
{
    string figurName = name.Trim();
    if (figurName == "")
    {
        throw new ArgumentException("Bitte einen Namen eingeben.");
    }

    double px = Zahl(x);   // double.Parse – wirft FormatException bei "abc"
    double py = Zahl(y);
    double[] werte = masse
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(Zahl)
        .ToArray();

    return art switch
    {
        "Rechteck" when werte.Length == 2 => new Rechteck(figurName, px, py, werte[0], werte[1]),
        "Kreis" when werte.Length == 1 => new Kreis(figurName, px, py, werte[0]),
        "Dreieck" when werte.Length == 3 => new Dreieck(figurName, px, py, werte[0], werte[1], werte[2]),
        _ => throw new ArgumentException("Bitte die richtige Anzahl an Maßen eingeben.")
    };
}
```

Schlägt etwas fehl, fängt `Ok` die Exception, schreibt die Meldung in `fehler`, und der Absatz `<p class="fehler">@fehler</p>` zeigt sie beim Neu-Rendern an – der Dialog bleibt offen, der Benutzer kann korrigieren. Auch der `Dreieck`-Konstruktor wirft eine `ArgumentException`, wenn die Seiten kein Dreieck ergeben; diese Regel steckt im Fachkonzept und wird hier nur angezeigt. Die zweite Ebene liegt in `Verwaltung.Hinzufuegen`: Ob der Name schon vergeben ist, kann nur die Verwaltung wissen, und deshalb fängt die Startseite diese `ArgumentException` und nicht der Dialog. Formatfragen gehören in die Oberfläche, Fachregeln ins Fachkonzept.

## Modal per CSS

Ein Dialog soll **modal** sein: Solange er offen ist, darf der Benutzer nicht in die Seite dahinter klicken. In Blazor braucht das keinen Code, sondern nur die beiden Klassen aus `wwwroot/app.css`:

```css
.dialog-hintergrund {
    position: fixed;
    inset: 0;
    background: rgba(0, 0, 0, 0.4);
    display: flex;
    align-items: center;
    justify-content: center;
}

.dialog {
    background: white;
    padding: 1.5rem;
    border-radius: 6px;
    min-width: 320px;
}
```

`position: fixed` mit `inset: 0` spannt den halbtransparenten Hintergrund über das ganze Browserfenster; er fängt alle Klicks ab, die nicht den Dialog treffen. `display: flex` mit zentrierter Ausrichtung setzt den eigentlichen Dialog in die Mitte. Damit ist der Dialog modal, ohne dass die Startseite ihre Buttons deaktivieren müsste.

## Ausblick: `EditForm` und Validierungsattribute

Für größere Formulare bringt Blazor die Komponente `EditForm` mit: Sie bindet an ein Modellobjekt, dessen Properties mit Attributen wie `[Required]` oder `[Range(0.1, 1000)]` aus `System.ComponentModel.DataAnnotations` beschrieben sind, und ein `<DataAnnotationsValidator />` prüft sie automatisch – Fehlermeldungen erscheinen per `<ValidationMessage>` neben dem jeweiligen Feld. Für unseren Dialog wäre das mehr Gerüst als Nutzen; sobald Formulare aber zehn Felder mit ähnlichen Regeln haben, spart es das Handschreiben jeder einzelnen Prüfung.

Übung: Der Dialog soll sich auch mit der Escape-Taste schließen lassen. Welches Ereignis brauchst du, an welchem Element, und welchen Wert übergibst du an `OnGeschlossen`? Überlege außerdem: Sollte der Benutzer eine Warnung sehen, wenn er nach dem Ausfüllen aller Felder auf „Abbrechen“ klickt – und wo würde diese Entscheidung im Code stehen?
{: .notice--info}

## Weitere Quellen

- [EventCallback – Ereignisbehandlung in Blazor – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/event-handling#eventcallback)
- [Komponentenparameter – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/#component-parameters)
- [Blazor-Formulare und Validierung – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/forms/)
