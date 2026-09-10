---
title: "Ereignisse in Blazor"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

In [GUI-Grundbegriffe](/modules/gui_grundbegriffe/gui_grundbegriffe.md) haben wir festgehalten, was Oberflächenprogrammierung ausmacht: Wir schreiben keinen Ablauf mehr, sondern Reaktionen. Ein Klick, ein Tastendruck, eine geänderte Auswahl – jedes dieser Ereignisse ruft eine Methode auf, die wir bereitgestellt haben. In `HalloBlazor` war das ein einziger Button mit `@onclick`. Der Geometrieeditor braucht mehr: Buttons, die je nach Zustand etwas anderes tun, Listeneinträge, die beim Klick „ihre“ Figur auswählen, Eingabefelder, die auf die Enter-Taste reagieren. Dieses Modul zeigt die Formen, in denen Ereignisbehandler in Blazor auftreten – von der benannten Methode bis zum asynchronen Handler – und was Blazor nach jedem Handler von selbst erledigt.

## Eine Methode als Handler

Die Grundform kennen wir: Das Attribut `@onclick` bekommt den Namen einer Methode aus dem `@code`-Block. Die Werkzeugleiste des Geometrieeditors besteht fast nur aus solchen Zuweisungen:

```razor
<button @onclick="Entfernen" disabled="@(ausgewaehlt is null)">Entfernen</button>
<button @onclick="Laden">Laden</button>
<button @onclick="Speichern">Speichern</button>

@code {
    private Figur? ausgewaehlt;
    private string status = "";

    private void Entfernen()
    {
        if (ausgewaehlt is not null)
        {
            Verwaltung.Entfernen(ausgewaehlt);
            ausgewaehlt = null;
            StatusAktualisieren();
        }
    }

    private void Speichern()
    {
        Verwaltung.Speichern();
        status = "Gespeichert.";
    }
}
```

Die Handler sind gewöhnliche Methoden ohne Parameter und ohne Rückgabewert. Sie tun genau das, was ihr Name sagt, und überlassen die eigentliche Arbeit der `FigurenVerwaltung` – warum das so sein sollte, klärt [Schichten mit Blazor umsetzen](/modules/schichten_mit_blazor/schichten_mit_blazor.md). Auffällig ist wieder, was fehlt: `Entfernen` löscht die Figur aus der Verwaltung, aber nirgends steht, dass die Liste auf der Seite aktualisiert werden soll.

Nach jedem Ereignisbehandler rendert Blazor die Komponente **automatisch neu**. Die `@foreach`-Schleife der Figurenliste läuft erneut, `@status` zeigt den neuen Text, und die Buttons prüfen `disabled` noch einmal. Wir ändern nur Felder; die Anzeige folgt. Wann genau Blazor neu rendert und wie man das in Sonderfällen selbst anstößt, sehen wir in [Datenbindung und Render-Zyklus](/modules/blazor_datenbindung/blazor_datenbindung.md).
{: .notice--primary}

Der Name im Markup wird beim Kompilieren aufgelöst: `@onclick="Entfernen"` ist kein String, sondern ein Verweis auf die Methode. Ein Tippfehler wie `@onclick="Entferne"` führt deshalb zu einem **Compilerfehler**, nicht zu einem stillen Fehlverhalten zur Laufzeit – ein großer Vorteil gegenüber Frameworks, die Handler über Zeichenketten verknüpfen.
{: .notice--warning}

## Lambdas als Handler

Nicht jeder Handler verdient eine eigene Methode. Wenn ein Klick nur ein Feld setzt, ist ein **Lambda-Ausdruck** direkt im Markup kürzer. Der Geometrieeditor öffnet so den Dialog und wählt Figuren in der Liste aus:

```razor
<button @onclick="() => dialogOffen = true">Neue Figur …</button>

@foreach (Figur figur in Verwaltung.AlleFiguren)
{
    <li @onclick="() => ausgewaehlt = figur">@figur.Name</li>
}
```

`() => dialogOffen = true` ist eine Methode ohne Namen, die genau eine Zuweisung enthält. Innerhalb von `@foreach` ist diese Form besonders wertvoll: Jedes `<li>` braucht einen Handler, der weiß, *welche* Figur es darstellt – und das Lambda greift auf die Schleifenvariable `figur` zu. Mit einer benannten Methode `Auswaehlen()` ginge das nicht, weil sie beim Aufruf nicht wüsste, welcher Eintrag geklickt wurde. Lambdas haben wir bislang nur am Rande gesehen; ihre genaue Funktionsweise ist Thema von [Lambda-Ausdrücke](/modules/lambda_ausdruecke/lambda_ausdruecke.md). Für den Moment reicht: Alles nach `=>` ist der Rumpf, und Variablen aus der Umgebung dürfen darin verwendet werden.

## Ereignisargumente

Manchmal reicht die Information „es wurde geklickt“ nicht aus. Blazor übergibt dann ein **Ereignisargument**, dessen Typ zum Ereignis passt. Ein Klick-Handler kann `MouseEventArgs` entgegennehmen, um etwa die Mausposition oder gedrückte Zusatztasten abzufragen:

```razor
<button @onclick="Klick">Wo wurde geklickt?</button>

@code {
    private string status = "";

    private void Klick(MouseEventArgs e)
    {
        status = $"Klick bei ({e.ClientX}, {e.ClientY})";
    }
}
```

Blazor erkennt an der Signatur, ob die Methode das Argument haben will – `void Klick()` und `void Klick(MouseEventArgs e)` sind beide zulässig. Bei Eingabefeldern sind zwei Ereignisse wichtig: `@onchange` feuert, wenn das Feld verlassen wird, `@oninput` bei jedem Tastendruck. Beide liefern `ChangeEventArgs`, deren Property `Value` vom Typ `object?` ist:

```razor
<input @oninput="Suchen" placeholder="Filter" />

@code {
    private string filter = "";

    private void Suchen(ChangeEventArgs e)
    {
        filter = e.Value?.ToString() ?? "";
    }
}
```

Das `?.` ist nötig, weil `Value` `null` sein kann, und `?? ""` sorgt dafür, dass `filter` nie `null` wird – die Null-Operatoren aus Programmierung 1 zahlen sich hier aus. Für Tastaturereignisse gibt es `@onkeydown` mit `KeyboardEventArgs`, deren Property `Key` den Namen der Taste als Text enthält. So lässt sich ein Formular mit Enter absenden:

```razor
<input @bind="name" @onkeydown="TasteGedrueckt" />

@code {
    private void TasteGedrueckt(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            Begruessen();
        }
    }
}
```

Die verfügbaren Argumenttypen folgen den Ereignissen des Browsers: `MouseEventArgs`, `KeyboardEventArgs`, `ChangeEventArgs`, `FocusEventArgs` und einige mehr. Man muss sie nicht auswendig kennen – die IDE schlägt beim Schreiben des Handlers den passenden Typ vor.

## Asynchrone Handler

Wenn ein Handler länger dauert – eine Datei laden, einen Webdienst befragen –, darf er die Ereignisschleife nicht blockieren. Blazor akzeptiert deshalb auch Handler mit der Signatur `async Task`. Der Dialog des Geometrieeditors nutzt das, um sein Ergebnis an die Startseite zurückzumelden:

```razor
<button @onclick="Ok">OK</button>

@code {
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
}
```

`await` gibt die Kontrolle ab, während die Arbeit läuft, und der Handler wird fortgesetzt, sobald sie fertig ist. Blazor rendert nach dem Abschluss des gesamten `Task` erneut – und zusätzlich beim ersten `await`, damit die Seite währenddessen nicht eingefroren wirkt. Was `async` und `await` im Detail bewirken, behandeln wir bei den Streams und Dateien; für Handler gilt die einfache Regel: Ruft der Handler etwas mit `Async` im Namen auf, wird er selbst `async Task`.

## Vergleich mit klassischen Ereignissen

Wer ältere C#-GUI-Programme gesehen hat, kennt Ereignisbehandler in dieser Form:

```csharp
// Klassische Form, wie sie Desktop-Frameworks verwenden
button.Click += Button_Click;

private void Button_Click(object? sender, EventArgs e)
{
    // sender: das Steuerelement, das das Ereignis ausgelöst hat
}
```

Die Idee ist dieselbe: Eine Methode wird bei einem Steuerelement für ein Ereignis **registriert** – hier mit `+=` im Code statt mit `@onclick` im Markup. Die feste Signatur mit `sender` und `EventArgs` gibt es bei Blazor nicht; die Methode nimmt nur, was sie braucht. Dass hinter `+=` ein Sprachmittel von C# steckt, mit dem wir eigene Ereignisse in eigenen Klassen definieren können, lernen wir in [Ereignisse](/modules/ereignisse/ereignisse.md). Der Blazor-Rückkanal `EventCallback<T>`, den wir gerade beim Dialog gesehen haben, ist eine Verwandte dieser Technik.

Übung: Erweitere die Figurenliste so, dass ein Doppelklick (`@ondblclick`) die Figur um 10 nach rechts verschiebt, während ein einfacher Klick sie weiterhin nur auswählt. Ergänze dann im Dialog ein `@onkeydown` auf dem Namensfeld, sodass Enter dieselbe Wirkung wie der OK-Button hat und Escape wie Abbrechen. Welche Signatur braucht der Handler dafür?
{: .notice--info}

Das vollständige Projekt findest du im Repository unter `examples/03_blazor/Geometrieeditor`.

## Weitere Quellen

- [Ereignisbehandlung in Blazor – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/event-handling)
- [Ereignisargumenttypen – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/event-handling#event-arguments)
- [Lambda-Ausdrücke – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/operators/lambda-expressions)
- [Tastaturereignisse: `KeyboardEvent.key` – MDN Web Docs](https://developer.mozilla.org/de/docs/Web/API/KeyboardEvent/key)
