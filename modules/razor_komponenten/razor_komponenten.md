---
title: "Razor-Komponenten und Steuerelemente"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

In [Die erste Blazor-App](/modules/blazor_erste_app/blazor_erste_app.md) haben wir gesehen, dass eine Razor-Datei aus Markup und einem `@code`-Block besteht. Die Startseite des Geometrieeditors muss deutlich mehr können als `HalloBlazor`: eine Liste aller Figuren anzeigen, die sich ständig ändert, Details nur dann zeigen, wenn eine Figur ausgewählt ist, Buttons abschalten, solange nichts ausgewählt ist, und einen Dialog einblenden. Für all das brauchen wir keine neue Sprache, sondern zwei Dinge: die **Razor-Syntax**, mit der C#-Werte und C#-Kontrollfluss ins Markup gelangen, und das Wissen, welche HTML-Elemente die Rolle der Steuerelemente übernehmen. Am Ende sehen wir, dass jede Razor-Datei selbst zu einem Steuerelement wird, das andere Seiten einbetten können.

## Razor-Syntax: C# im Markup

Razor ist HTML, in dem das Zeichen `@` in die Welt von C# umschaltet. Die einfachste Form ist ein Ausdruck, dessen Wert als Text eingefügt wird:

```razor
<p>Hallo, @name!</p>
<p>Summe: @(a + b)</p>
<p>Umfang: @figur.Umfang.ToString("F2")</p>
```

Ein einzelner Bezeichner wie `@name` oder eine Kette aus Punkten und Methodenaufrufen wie `@figur.Umfang.ToString("F2")` kommt ohne Klammern aus. Sobald Operatoren im Spiel sind, braucht der Ausdruck runde Klammern – `@(a + b)` –, sonst würde Razor nach `@a` wieder in HTML zurückwechseln und ` + b` als Text ausgeben. Die Werte werden beim Einfügen automatisch HTML-kodiert; ein `<` im Namen einer Figur landet also als Text auf der Seite und nicht als Tag.

Neben Ausdrücken gibt es Kontrollfluss. `@if`, `@else` und `@foreach` funktionieren wie in C#, nur dass ihr Rumpf Markup enthält statt Anweisungen. Genau so baut die Startseite des Geometrieeditors ihre Detailansicht und die Figurenliste:

```razor
<div class="details">
    @if (ausgewaehlt is null)
    {
        <p>Keine Figur ausgewählt.</p>
    }
    else
    {
        <p>@ausgewaehlt.Beschreibung()</p>
        <p>Umfang: @ausgewaehlt.Umfang.ToString("F2")</p>
    }
</div>
```

Innerhalb der geschweiften Klammern gilt: Was mit `<` beginnt, ist Markup, alles andere C#. Deshalb kann der `else`-Zweig ohne weiteres `@ausgewaehlt.Beschreibung()` aufrufen – die Methode aus der `Figur`-Basisklasse, die wir in [Abstrakte Klassen](/modules/abstrakte_klassen/abstrakte_klassen.md) definiert haben. Die Liste entsteht mit `@foreach`:

```razor
<ul class="figurenliste">
    @foreach (Figur figur in Verwaltung.AlleFiguren)
    {
        <li class="@(figur == ausgewaehlt ? "ausgewaehlt" : "")"
            @onclick="() => ausgewaehlt = figur">@figur.Name</li>
    }
</ul>
```

Für jede Figur in `Verwaltung.AlleFiguren` entsteht ein `<li>`. Es gibt keine Methode `ListeAktualisieren`, die wir nach jeder Änderung aufrufen müssten: Fügt jemand eine Figur hinzu, rendert Blazor die Komponente neu, die Schleife läuft erneut und die Liste hat ein Element mehr. Das Markup ist eine **Beschreibung** des gewünschten Zustands, keine Folge von Befehlen.

Die Schleifenvariable `figur` wird in dem Lambda `() => ausgewaehlt = figur` verwendet. Weil `foreach` in C# für jeden Durchlauf eine neue Variable anlegt, merkt sich jedes `<li>` seine eigene Figur – das ist gewollt und funktioniert zuverlässig. Bei einer klassischen `for`-Schleife mit Index müsste man den Wert dagegen erst in eine lokale Variable kopieren.
{: .notice--primary}

## HTML-Elemente sind die Steuerelemente

Desktop-Frameworks bringen eigene Klassen wie `TextBox` oder `CheckBox` mit. In Blazor übernehmen die eingebauten HTML-Elemente diese Rolle – der Browser zeichnet sie, wir binden sie an unsere Felder. Die wichtigsten auf einen Blick:

| Steuerelement | HTML-Element | Wichtigstes Binding/Attribut |
| :--- | :--- | :--- |
| Textfeld | `<input>` | `@bind="name"` |
| Zahlenfeld | `<input type="number">` | `@bind="anzahl"` (bindet an `int`/`double`) |
| Checkbox | `<input type="checkbox">` | `@bind="aktiv"` (bindet an `bool`) |
| Auswahlliste | `<select>` mit `<option>` | `@bind="art"` |
| Button | `<button>` | `@onclick="Methode"` |
| Mehrzeiliger Text | `<textarea>` | `@bind="text"` |
| Liste | `<ul>` mit `<li>` | `@foreach` |
| Fortschrittsanzeige | `<progress>` | `value="@fortschritt" max="100"` |
| Bild | `<img>` | `src="@pfad"` |
| Beschriftung | `<label>`, `<p>` | Text mit `@`-Ausdrücken |

Ein Beispiel für die Auswahlliste liefert der Dialog `NeueFigurDialog` des Geometrieeditors: Ein `<select>` mit drei `<option>`-Einträgen ist per `@bind="art"` an ein `string`-Feld gebunden, und je nach Wert ändert sich die Beschriftung des Maße-Feldes:

```razor
<label>Art:
    <select @bind="art">
        <option value="Rechteck">Rechteck</option>
        <option value="Kreis">Kreis</option>
        <option value="Dreieck">Dreieck</option>
    </select>
</label>

<label>@MasseBeschriftung <input @bind="masse" placeholder="z. B. 4, 3" /></label>
```

`MasseBeschriftung` ist dabei eine Property im `@code`-Block mit einem `switch`-Ausdruck über `art` – Markup und Logik bleiben getrennt, obwohl sie in einer Datei stehen.

## Attribute aus C# setzen

Nicht nur der Inhalt, auch die Attribute eines Elements dürfen aus C# stammen. Das nutzt die Werkzeugleiste des Geometrieeditors, um Buttons abzuschalten, solange keine Figur ausgewählt ist:

```razor
<button @onclick="Entfernen" disabled="@(ausgewaehlt is null)">Entfernen</button>
<button @onclick="Verschieben" disabled="@(ausgewaehlt is null)">Verschieben</button>
```

Für `bool`-Attribute wie `disabled` ist Blazor besonders hilfreich: Ergibt der Ausdruck `true`, wird das Attribut gesetzt, bei `false` ganz weggelassen – genau so, wie HTML es erwartet. Beim `class`-Attribut haben wir das Muster schon in der Figurenliste gesehen: `class="@(figur == ausgewaehlt ? "ausgewaehlt" : "")"` hängt nur dem ausgewählten Eintrag die CSS-Klasse `ausgewaehlt` an, die ihn farblich hervorhebt. Der Zustand lebt in einem C#-Feld, das Aussehen folgt daraus – wir manipulieren nie direkt „das Element auf der Seite“.

## Eigene Komponenten

Bis hierher haben wir nur eingebaute HTML-Elemente verwendet. Die eigentliche Stärke von Blazor ist, dass **jede `.razor`-Datei eine wiederverwendbare Komponente** ist, die sich wie ein Tag einbinden lässt. `PageTitle` war schon so ein Fall: keine HTML-Vorschrift, sondern eine Komponente von Blazor, die den Titel der Browser-Registerkarte setzt. Der Geometrieeditor besitzt eine eigene Komponente `NeueFigurDialog.razor` im Ordner `Components`, und die Startseite bindet sie so ein:

```razor
@if (dialogOffen)
{
    <NeueFigurDialog OnGeschlossen="DialogGeschlossen" />
}
```

Der Tag-Name ist der Dateiname, und die Komponente erscheint nur, solange `dialogOffen` den Wert `true` hat. Das Attribut `OnGeschlossen` ist kein HTML-Attribut, sondern ein **Parameter** der Komponente – eine öffentliche Property, die im `@code`-Block des Dialogs mit `[Parameter]` markiert ist:

```razor
@code {
    [Parameter]
    public EventCallback<Figur?> OnGeschlossen { get; set; }
    // ...
}
```

Über Parameter fließen Daten von der einbettenden Seite in die Komponente hinein: eine Überschrift, eine Figur zum Bearbeiten oder – wie hier – ein Rückkanal vom Typ `EventCallback<Figur?>`, über den der Dialog sein Ergebnis zurückmeldet. Wie der Dialog mit diesem Rückkanal arbeitet und warum wir ihn nicht als Fenster, sondern als Komponente bauen, ist Thema von [Dialoge als Komponenten](/modules/blazor_dialoge/blazor_dialoge.md).

Komponenten machen sich als Tags nur bemerkbar, wenn ihr Namespace bekannt ist. Die Datei `_Imports.razor` enthält dafür `@using`-Zeilen, die für alle Razor-Dateien gelten. Verschiebt man eine Komponente in einen neuen Unterordner, ändert sich ihr Namespace, und der Compiler meldet, das Tag sei unbekannt – dann fehlt eine `@using`-Zeile.
{: .notice--warning}

Übung: Baue eine Komponente `FigurKarte.razor` mit einem Parameter `public Figur Figur { get; set; }`, die Name, Position und Fläche als kleine Karte anzeigt. Ersetze dann in der Figurenliste den Text `@figur.Name` durch `<FigurKarte Figur="figur" />`. Was muss sich am `@onclick` ändern, damit die Auswahl weiterhin funktioniert?
{: .notice--info}

Das vollständige Projekt findest du im Repository unter `examples/04_blazor/Geometrieeditor`.

## Weitere Quellen

- [Razor-Syntaxreferenz für ASP.NET Core – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/mvc/views/razor)
- [Razor-Komponenten in ASP.NET Core – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/)
- [Attribute und Parameter von Komponenten – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/#component-parameters)
- [`<input>` – MDN Web Docs](https://developer.mozilla.org/de/docs/Web/HTML/Element/input)
