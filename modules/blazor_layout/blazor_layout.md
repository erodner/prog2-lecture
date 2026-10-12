---
title: "Layout mit HTML und CSS"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Ohne CSS stellt der Browser alle Elemente untereinander dar. Aus den 180 Zellen des Levels „Kerker“ würde eine lange Kolonne statt eines Rasters. Die Anordnung auf der Seite heißt **Layout**, und im Web erledigt sie **CSS**. Für das Adventure reichen zwei Werkzeuge: Flexbox und Grid.

Die einzelnen CSS-Eigenschaften musst du nicht auswendig können. Kaum jemand schreibt `align-items` oder `grid-template-columns` aus dem Kopf: Man sucht sie heraus, wenn man sie braucht (zum Beispiel in den [MDN Web Docs](https://developer.mozilla.org/de/docs/Web/CSS)), oder lässt sie sich generieren. Wichtig ist, dass du die Prinzipien verstehst.
{: .notice--primary}

## Klassen verbinden HTML und CSS

**HTML** beschreibt die Struktur der Seite, **CSS** (*Cascading Style Sheets*) ihr Aussehen. Verbunden werden beide über **Klassen**: Im Markup bekommt ein Element ein `class`-Attribut, und in der CSS-Datei legt ein Selektor `.klassenname { … }` fest, wie alle Elemente dieser Klasse aussehen. Das Stylesheet liegt unter `wwwroot/app.css`.

<div class="editoren" markdown="1">
<div class="editor" markdown="1">
<div class="editor-titel"><a href="https://github.com/erodner/prog2-adventure/blob/v03-blazor/Adventure.Web/Components/Pages/Home.razor">Adventure.Web/Components/Pages/Home.razor</a></div>

```razor
<div class="werkzeuge">
    …
</div>
```

</div>
<div class="editor" markdown="1">
<div class="editor-titel"><a href="https://github.com/erodner/prog2-adventure/blob/v03-blazor/Adventure.Web/wwwroot/app.css">Adventure.Web/wwwroot/app.css</a></div>

```css
.werkzeuge { display: flex; gap: 1rem; }
```

</div>
</div>

Den Rahmen jeder Seite liefert `MainLayout.razor`: eine Kopfzeile und darunter `@Body`, die Stelle, an der die jeweilige Seite eingesetzt wird.

<div class="editoren" markdown="1">
<div class="editor" markdown="1">
<div class="editor-titel"><a href="https://github.com/erodner/prog2-adventure/blob/v03-blazor/Adventure.Web/Components/Layout/MainLayout.razor">Adventure.Web/Components/Layout/MainLayout.razor</a></div>

```razor
@inherits LayoutComponentBase

<header class="kopfzeile">
    <h1>Adventure</h1>
</header>

<main>
    @Body
</main>
```

</div>
</div>

## Flexbox: Elemente in einer Reihe

Die Werkzeugleiste enthält die Levelauswahl und einen Button. **Flexbox** ordnet die Kinder eines Containers in einer Reihe an:

<div class="editoren" markdown="1">
<div class="editor" markdown="1">
<div class="editor-titel"><a href="https://github.com/erodner/prog2-adventure/blob/v03-blazor/Adventure.Web/Components/Pages/Home.razor">Adventure.Web/Components/Pages/Home.razor</a></div>

```razor
<div class="werkzeuge">
    <label>Level: <select …>…</select></label>
    <button @onclick="NeuStarten">Neu starten</button>
</div>
```

</div>
<div class="editor" markdown="1">
<div class="editor-titel"><a href="https://github.com/erodner/prog2-adventure/blob/v03-blazor/Adventure.Web/wwwroot/app.css">Adventure.Web/wwwroot/app.css</a></div>

```css
.werkzeuge {
    display: flex;
    gap: 1rem;
    align-items: center;
}
```

</div>
</div>

`display: flex` schaltet Flexbox ein, `gap` legt den Abstand fest, und `align-items: center` richtet die Elemente auf derselben Höhe aus.

## Grid: das Spielfeld als Raster

Für Zeilen *und* Spalten nimmt man **Grid**. Die Razor-Schleifen erzeugen eine flache Folge von `<div class="feld">`-Elementen, und erst Grid macht daraus ein Raster. Die Anzahl der Spalten fehlt im CSS, weil sie vom Level abhängt. Deshalb setzt das Markup sie aus C# heraus, im Attribut `style`:

<div class="editoren" markdown="1">
<div class="editor" markdown="1">
<div class="editor-titel"><a href="https://github.com/erodner/prog2-adventure/blob/v03-blazor/Adventure.Web/Components/Pages/Home.razor">Adventure.Web/Components/Pages/Home.razor</a></div>

```razor
<div class="spielfeld"
     style="grid-template-columns: repeat(@feld.Breite, 1fr);">
    @for (…)
    {
        <div class="feld">…</div>
    }
</div>
```

</div>
<div class="editor" markdown="1">
<div class="editor-titel"><a href="https://github.com/erodner/prog2-adventure/blob/v03-blazor/Adventure.Web/wwwroot/app.css">Adventure.Web/wwwroot/app.css</a></div>

```css
.spielfeld {
    display: grid;
    gap: 1px;
    background: #333;
    width: max-content;
}

.feld {
    width: 28px;
    height: 28px;
    background: #2a2a3c;
}
```

</div>
</div>

`repeat(20, 1fr)` bedeutet zwanzig gleich breite Spalten. Grid füllt sie von links nach rechts und beginnt dann eine neue Zeile, genau in der Reihenfolge, in der die Schleifen die Zellen erzeugen. Der `gap: 1px` auf dunklem Hintergrund ergibt nebenbei ein feines Gitternetz.

Auch Spielfeld und Statusleiste stehen in einem Grid mit zwei Spalten nebeneinander:

<div class="editoren" markdown="1">
<div class="editor" markdown="1">
<div class="editor-titel"><a href="https://github.com/erodner/prog2-adventure/blob/v03-blazor/Adventure.Web/Components/Pages/Home.razor">Adventure.Web/Components/Pages/Home.razor</a></div>

```razor
<div class="arbeitsbereich">
    <div class="spielfeld" …>…</div>
    <Statusleiste … />
</div>
```

</div>
<div class="editor" markdown="1">
<div class="editor-titel"><a href="https://github.com/erodner/prog2-adventure/blob/v03-blazor/Adventure.Web/wwwroot/app.css">Adventure.Web/wwwroot/app.css</a></div>

```css
.arbeitsbereich {
    display: grid;
    grid-template-columns: max-content 240px;
    gap: 1.5rem;
}
```

</div>
</div>

Faustregel: **Flexbox für eine Richtung, Grid für zwei.**
{: .notice--primary}

## Der Dialog über allem

Am Spielende legt sich ein Dialog über die Seite. `position: fixed` mit `inset: 0` lässt den abgedunkelten Hintergrund das ganze Fenster füllen, und Flexbox setzt den Dialog in die Mitte:

<div class="editoren" markdown="1">
<div class="editor" markdown="1">
<div class="editor-titel"><a href="https://github.com/erodner/prog2-adventure/blob/v03-blazor/Adventure.Web/Components/SpielEndeDialog.razor">Adventure.Web/Components/SpielEndeDialog.razor</a></div>

```razor
<div class="dialog-hintergrund">
    <div class="dialog">
        <h2>…</h2>
        …
    </div>
</div>
```

</div>
<div class="editor" markdown="1">
<div class="editor-titel"><a href="https://github.com/erodner/prog2-adventure/blob/v03-blazor/Adventure.Web/wwwroot/app.css">Adventure.Web/wwwroot/app.css</a></div>

```css
.dialog-hintergrund {
    position: fixed;
    inset: 0;
    background: rgba(0, 0, 0, 0.6);
    display: flex;
    align-items: center;
    justify-content: center;
}
```

</div>
</div>

Übung: Setze den Button „Neu starten“ an den rechten Rand der Werkzeugleiste (Tipp: `justify-content`) und mach die Zellen des Spielfelds größer. Welche CSS-Regel musst du jeweils ändern, und warum bleibt der C#-Code dabei unverändert?
{: .notice--info}

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag [`v03-blazor`](https://github.com/erodner/prog2-adventure/tree/v03-blazor)).

## Weitere Quellen

- [Blazor-Layouts – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/layouts)
- [CSS-Isolation in Blazor – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/css-isolation)
- [Flexbox – MDN Web Docs](https://developer.mozilla.org/de/docs/Web/CSS/CSS_flexible_box_layout/Basic_concepts_of_flexbox)
- [Grid-Layout – MDN Web Docs](https://developer.mozilla.org/de/docs/Web/CSS/CSS_grid_layout/Basic_concepts_of_grid_layout)
- [Flexbox Froggy](https://flexboxfroggy.com/#de) – 24 Level, in denen du Frösche mit `justify-content` und `align-items` auf ihre Seerosen schiebst; danach sitzt Flexbox.
- [CSS Grid Garden](https://cssgridgarden.com/#de) – dasselbe Prinzip für Grid: Karotten gießen mit `grid-template-columns` und `repeat()`, genau den Regeln aus unserem Spielfeld.
