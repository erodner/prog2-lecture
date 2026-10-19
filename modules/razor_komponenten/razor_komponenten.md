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

In [Die erste Blazor-App](/modules/blazor_erste_app/blazor_erste_app.md) bestand eine Razor-Datei aus Markup und einem `@code`-Block. Für das Adventure brauchen wir drei Dinge mehr: C#-Werte und Schleifen im Markup, die passenden HTML-Elemente als Steuerelemente und eigene Komponenten, die wir wie ein Tag einbinden.

## Razor-Syntax: C# im Markup

In Razor schaltet das Zeichen `@` von HTML nach C# um. Ein Ausdruck hinter `@` wird als Text eingefügt:

```razor
<p>Runde: @feld.Runde</p>
<p>Held: @feld.Spieler.Name</p>
<p>Verbleibend: @(feld.Spieler.Lebenspunkte * 10) Prozent</p>
```

Sobald Operatoren vorkommen, braucht der Ausdruck runde Klammern wie in der letzten Zeile, sonst würde ` * 10` als Text erscheinen.

Auch `@if`, `@foreach` und `@for` funktionieren wie in C#, nur dass im Rumpf Markup steht. Die Levelauswahl erzeugt so für jeden Levelnamen einen Eintrag:

<div class="editoren" markdown="1">
<div class="editor" markdown="1">
<div class="editor-titel"><a href="https://github.com/erodner/prog2-adventure/blob/v03-blazor/Adventure.Web/Components/Pages/Home.razor">Adventure.Web/Components/Pages/Home.razor</a></div>

```razor
<select @bind="levelName">
    @foreach (string name in LevelQuelle.LevelNamen)
    {
        <option value="@name">@name</option>
    }
</select>
```

</div>
</div>

Das Markup beschreibt, wie die Seite aussehen soll. Kommen neue Level dazu, hat die Liste automatisch mehr Einträge.

## Das Spielfeld: zwei Schleifen über das Raster

Für die Karte gehen zwei Schleifen über alle Felder und fragen das `Spielfeld` mit `ObjektAn`, was dort liegt (vereinfacht gegenüber dem Projekt):

<div class="editoren" markdown="1">
<div class="editor" markdown="1">
<div class="editor-titel"><a href="https://github.com/erodner/prog2-adventure/blob/v03-blazor/Adventure.Web/Components/Pages/Home.razor">Adventure.Web/Components/Pages/Home.razor</a></div>

```razor
<div class="spielfeld" style="grid-template-columns: repeat(@feld.Breite, 1fr);">
    @for (int y = 0; y < feld.Hoehe; y++)
    {
        for (int x = 0; x < feld.Breite; x++)
        {
            Spielobjekt? objekt = feld.ObjektAn(new Koordinate(x, y));
            <div class="feld">@SymbolFuer(objekt)</div>
        }
    }
</div>
```

</div>
</div>

Das `@` steht nur vor der äußeren Schleife: Danach sind wir schon in C#, und erst die Zeile mit `<div` ist wieder Markup. Dass aus den vielen `<div>`-Elementen ein Raster wird, regelt das CSS, siehe [Layout mit HTML und CSS](/modules/blazor_layout/blazor_layout.md).

Was in einer Zelle steht, entscheidet die Hilfsmethode `SymbolFuer` im `@code`-Block. Sie ist das Gegenstück zu `Spielobjekt.Symbol()` aus dem Kern: Dort steht ein `char` für die Konsole, hier ein Emoji für den Browser.

<div class="editoren" markdown="1">
<div class="editor" markdown="1">
<div class="editor-titel"><a href="https://github.com/erodner/prog2-adventure/blob/v03-blazor/Adventure.Web/Components/Pages/Home.razor">Adventure.Web/Components/Pages/Home.razor</a> (im <code>@code</code>-Block)</div>

```csharp
private static string SymbolFuer(Spielobjekt? objekt)
{
    return objekt switch
    {
        null => "",
        Wand => "🧱",
        Spieler => "🧝",
        Wache => "💂",
        Verfolger => "👹",
        Schluessel => "🔑",
        Tuer t => t.IstOffen ? "▫️" : "🚪",
        Truhe t => t.IstGeoeffnet ? "📭" : "🎁",
        Trank => "🧪",
        Schatz => "💰",
        Ausgang => "🏁",
        _ => objekt.Symbol().ToString()
    };
}
```

</div>
</div>

Der `switch` prüft den **Laufzeittyp** des Objekts ([Kompilierzeittyp und Laufzeittyp](/modules/laufzeittyp_verstecken/laufzeittyp_verstecken.md)): Ist es eine `Wand`, kommt `🧱` heraus. `Tuer t` benennt das Objekt zusätzlich, damit man abfragen kann, ob die Tür offen ist. Der letzte Zweig `_` fängt alles ab, woran niemand gedacht hat, und zeigt dann das Konsolenzeichen.

## HTML-Elemente sind die Steuerelemente

In Blazor übernehmen die normalen HTML-Elemente die Rolle der Steuerelemente. Die wichtigsten:

| Steuerelement | HTML-Element | Verbindung zum C#-Code |
| :--- | :--- | :--- |
| Textfeld | `<input>` | `@bind="name"` |
| Checkbox | `<input type="checkbox">` | `@bind="aktiv"` (an ein `bool`) |
| Auswahlliste | `<select>` mit `<option>` | `@bind="levelName"` |
| Button | `<button>` | `@onclick="NeuStarten"` |
| Bereich, der Tasten empfängt | `<div tabindex="0">` | `@onkeydown="TasteGedrueckt"` |

Auch Attributwerte dürfen aus C# kommen, etwa die Spaltenzahl im `style` des Spielfelds oben. Den Zustand halten wir immer in C#-Feldern, die Seite folgt daraus.

## Eigene Komponenten

Jede `.razor`-Datei ist eine **Komponente**, die andere Seiten wie ein Tag einbinden können. Die Anzeige neben dem Spielfeld liegt deshalb in einer eigenen Datei (links, vereinfacht). Rechts bindet die Startseite sie ein und übergibt die Werte wie HTML-Attribute:

<div class="editoren" markdown="1">
<div class="editor" markdown="1">
<div class="editor-titel"><a href="https://github.com/erodner/prog2-adventure/blob/v03-blazor/Adventure.Web/Components/Statusleiste.razor">Adventure.Web/Components/Statusleiste.razor</a></div>

```razor
@using Adventure.Kern

<div class="status">
    <h2>@Spieler.Name</h2>
    <p>Lebenspunkte: @Spieler.Lebenspunkte / @Spieler.MaxLebenspunkte</p>
    <p>Punkte: @Spieler.Punkte</p>
    <p>Runde: @Runde</p>
    <p class="meldung">@Meldung</p>
</div>

@code {
    [Parameter] public Spieler Spieler { get; set; } = null!;
    [Parameter] public int Runde { get; set; }
    [Parameter] public string Meldung { get; set; } = "";
}
```

</div>
<div class="editor" markdown="1">
<div class="editor-titel"><a href="https://github.com/erodner/prog2-adventure/blob/v03-blazor/Adventure.Web/Components/Pages/Home.razor">Adventure.Web/Components/Pages/Home.razor</a></div>

```razor
<Statusleiste Spieler="@feld.Spieler" Runde="@feld.Runde" Meldung="@feld.LetzteMeldung" />
```

</div>
</div>

Der Tag-Name ist der Dateiname. Die Attribute heißen **Parameter**: öffentliche Properties, die mit `[Parameter]` markiert sind. Über sie fließen Daten von der Seite in die Komponente. `= null!` sagt dem Compiler, dass der Spieler garantiert von außen gesetzt wird.

Im Zweifel schreibst du bei Parametern immer ein `@` vor den Wert. Bei einem `string`-Parameter ist es Pflicht: Ohne `@` stünde in der Statusleiste wörtlich „feld.LetzteMeldung“, und der Compiler merkt es nicht.
{: .notice--warning}

Die Statusleiste berechnet nichts und kennt das `Spielfeld` nicht. Sie bekommt einen `Spieler` und zeigt ihn an. Genau das macht sie wiederverwendbar.
{: .notice--primary}

## Weitere Quellen

- [Razor-Syntaxreferenz für ASP.NET Core – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/mvc/views/razor)
- [Razor-Komponenten in ASP.NET Core – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/)
- [Attribute und Parameter von Komponenten – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/#component-parameters)
- [Mustervergleich mit `switch`-Ausdrücken – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/operators/patterns)
- [HTML-Elementreferenz – MDN Web Docs](https://developer.mozilla.org/de/docs/Web/HTML/Reference/Elements) – das vollständige Sortiment an „Steuerelementen“, aus dem du dir in Blazor bedienst.
- [Mustervergleich üben – SharpLab](https://sharplab.io/) – zeigt dir, in welchen C#-Code der Compiler einen `switch`-Ausdruck mit Typmustern übersetzt.
