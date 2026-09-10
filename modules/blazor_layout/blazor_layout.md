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

Mit den Steuerelementen aus [Razor-Komponenten und Steuerelemente](/modules/razor_komponenten/razor_komponenten.md) können wir alles auf die Seite bringen, was der Geometrieeditor braucht – aber der Browser stellt es dann untereinander dar, wie einen Fließtext. Eine Werkzeugleiste, in der die Buttons nebeneinander stehen, eine Figurenliste links neben der Detailansicht, ein Dialog, der über allem liegt: Das ist die Aufgabe des **Layouts**. Auf dem Desktop übernehmen das spezielle Container-Steuerelemente, im Web erledigt es **CSS**. Die gute Nachricht: Für einfache, aufgeräumte Oberflächen reichen zwei CSS-Werkzeuge, Flexbox und Grid, und wenige Zeilen pro Bereich. In diesem Modul bauen wir das Layout des Geometrieeditors mit genau diesen Mitteln.

## Struktur und Gestaltung trennen

Eine Webseite besteht aus zwei Ebenen. **HTML** beschreibt die Struktur: Was ist eine Überschrift, was eine Liste, was gehört zusammen. **CSS** (*Cascading Style Sheets*) beschreibt die Gestaltung: Farben, Abstände, Schriften – und eben die Anordnung. Diese Trennung ist dieselbe Idee wie Markup und `@code`: Jede Ebene hat ihre Aufgabe, und man kann eine ändern, ohne die andere anzufassen.

Die Brücke zwischen beiden sind **Klassen**. Im Markup bekommt ein Element ein `class`-Attribut, in der CSS-Datei beschreibt ein Selektor `.klassenname { ... }`, wie alle Elemente dieser Klasse aussehen. Die Stylesheet-Datei liegt in unseren Projekten unter `wwwroot/app.css` – der Ordner `wwwroot` enthält alles, was der Server unverändert an den Browser ausliefert, und `App.razor` bindet die Datei mit einem `<link>` ein.

## Das Layout: Rahmen für jede Seite

Den äußeren Rahmen liefert `MainLayout.razor`. Beim Geometrieeditor haben wir die fast leere Vorlage um eine Kopfzeile ergänzt:

```razor
@inherits LayoutComponentBase

<header class="kopfzeile">
    <h1>Geometrieeditor</h1>
</header>

<main>
    @Body
</main>
```

`@Body` ist die Stelle, an der der Router die jeweilige Seite einsetzt; alles außerhalb bleibt auf jeder Seite gleich. Die Kopfzeile besteht aus einem `<header>` mit der Klasse `kopfzeile`, und in `app.css` steht, wie sie aussieht:

```css
body {
    font-family: system-ui, sans-serif;
    margin: 0;
}

.kopfzeile {
    background: #2b5797;
    color: white;
    padding: 0.5rem 1.5rem;
}

main {
    padding: 1rem 1.5rem;
}
```

Farbe, Textfarbe und Innenabstand – mehr braucht ein blauer Balken nicht. Die Einheit `rem` ist relativ zur Standardschriftgröße, sodass die Seite bei vergrößerter Schrift mitwächst. Damit ist der Rahmen fertig; die eigentliche Anordnung passiert innerhalb der Startseite.

## Flexbox: Elemente in einer Reihe

Die Werkzeugleiste der Startseite ist ein `<div class="werkzeuge">` mit fünf Buttons. Ohne CSS stünden sie zwar nebeneinander, aber ohne kontrollierten Abstand. **Flexbox** macht aus einem Container eine Reihe (oder Spalte), in der die Kinder verteilt werden:

```css
.werkzeuge {
    display: flex;
    gap: 0.5rem;
    margin-bottom: 1rem;
}
```

`display: flex` schaltet Flexbox ein, `gap` legt den Abstand zwischen den Kindern fest. Mit `flex-direction: column` würde die Reihe zur Spalte, `justify-content` verteilt die Kinder entlang der Richtung (etwa `flex-end` für rechtsbündig, `space-between` für maximalen Abstand), und `align-items` richtet sie quer dazu aus, zum Beispiel `center` für vertikal mittig. Der Dialog des Geometrieeditors zeigt beide Richtungen auf einmal:

```css
.dialog {
    display: flex;
    flex-direction: column;
    gap: 0.6rem;
}

.dialog-buttons {
    display: flex;
    justify-content: flex-end;
    gap: 0.5rem;
}
```

Die Formularzeilen des Dialogs stehen untereinander, die Buttons „Abbrechen“ und „OK“ am Ende rechts. Flexbox denkt immer in **einer Richtung** – das ist ihre Stärke und ihre Grenze.

## Grid: Flächen in Zeilen und Spalten

Unter der Werkzeugleiste soll links die Figurenliste und rechts die Detailansicht stehen, wobei die Details doppelt so breit sind wie die Liste. Sobald Breiten in Spalten und Zeilen gleichzeitig eine Rolle spielen, ist **CSS-Grid** das richtige Werkzeug:

```css
.arbeitsbereich {
    display: grid;
    grid-template-columns: 1fr 2fr;
    gap: 1rem;
    min-height: 250px;
}
```

`grid-template-columns: 1fr 2fr` definiert zwei Spalten; die Einheit `fr` (*fraction*) verteilt den verfügbaren Platz im Verhältnis 1 : 2. Die beiden Kinder des Containers – `<ul class="figurenliste">` und `<div class="details">` – füllen die Spalten der Reihe nach. Kämen weitere Kinder hinzu, begänne automatisch eine neue Zeile. Für Formulare ist `grid-template-columns: auto 1fr` ein bewährtes Muster: Beschriftungen so breit wie nötig, Eingabefelder nehmen den Rest.

Faustregel: **Flexbox für eine Richtung, Grid für zwei.** Eine Werkzeugleiste, eine Button-Reihe, eine Spalte aus Formularzeilen – Flexbox. Ein Arbeitsbereich mit Spalten fester Verhältnisse, ein Formular mit Beschriftungs- und Eingabespalte – Grid. Beide lassen sich verschachteln: Ein Grid-Feld kann innen wieder ein Flex-Container sein.
{: .notice--primary}

## Overlays: der Dialog über allem

Der Dialog für neue Figuren soll die Seite abdunkeln und in der Mitte erscheinen. Dafür kombinieren wir eine Positionierung mit Flexbox:

```css
.dialog-hintergrund {
    position: fixed;
    inset: 0;
    background: rgba(0, 0, 0, 0.4);
    display: flex;
    align-items: center;
    justify-content: center;
}
```

`position: fixed` löst das Element aus dem normalen Textfluss und heftet es an das Browserfenster; `inset: 0` setzt alle vier Abstände zum Rand auf null, sodass der Hintergrund das ganze Fenster füllt. Die halbtransparente Farbe dunkelt ab, und weil der Hintergrund gleichzeitig ein Flex-Container mit zentrierter Ausrichtung ist, sitzt der eigentliche `.dialog` genau in der Mitte. Das ist alles, was ein modaler Dialog im Web braucht – kein zweites Fenster, keine Bibliothek.

Damit ist das Layout komplett: Kopfzeile aus dem `MainLayout`, Werkzeugleiste als Flex-Reihe, Arbeitsbereich als Grid mit zwei Spalten, darunter die Statuszeile `<p class="status">` als grau hinterlegter Absatz, und bei Bedarf der Dialog als Overlay darüber. Die zugehörige Hervorhebung der Auswahl haben wir in Razor-Komponenten gesehen: `.figurenliste li.ausgewaehlt` färbt genau den Eintrag ein, dem das Markup die Klasse `ausgewaehlt` gibt.

## Scoped CSS und CSS-Frameworks

Neben `app.css` erlaubt Blazor **komponentenbezogene** Stylesheets: Eine Datei `MainLayout.razor.css` neben `MainLayout.razor` gilt nur für das Markup dieser Komponente. Blazor sorgt beim Bauen dafür, dass die Regeln nicht auf andere Komponenten durchschlagen – die Vorlage nutzt das für die Fehlerleiste `#blazor-error-ui`. Für kleine Projekte ist eine zentrale `app.css` übersichtlicher; sobald mehrere Komponenten gleiche Klassennamen mit unterschiedlicher Bedeutung verwenden, lohnt sich die Trennung.

Die Standardvorlage von `dotnet new blazor` bringt außerdem das CSS-Framework **Bootstrap** mit: fertige Klassen wie `btn` oder `container`, die ein einheitliches Aussehen ohne eigenes CSS ergeben. Wir haben die Vorlage mit `--empty` erzeugt und bleiben bei eigenem CSS – so bleibt nachvollziehbar, welche Regel welche Wirkung hat, und die wenigen Zeilen für Flexbox und Grid lernt man dabei gleich mit.

Übung: Ergänze den Arbeitsbereich um eine dritte Spalte mit einer Vorschau der ausgewählten Figur (zunächst nur Name und Fläche) und wähle die Spaltenverhältnisse so, dass die Liste schmal, Vorschau und Details gleich breit sind. Verändere anschließend nur das CSS so, dass die Statuszeile in der Kopfzeile rechts neben der Überschrift erscheint. Welche Eigenschaft braucht die Kopfzeile dafür?
{: .notice--info}

Das vollständige Projekt findest du im Repository unter `examples/03_blazor/Geometrieeditor`.

## Weitere Quellen

- [Blazor-Layouts – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/layouts)
- [CSS-Isolation in Blazor – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/css-isolation)
- [Flexbox – MDN Web Docs](https://developer.mozilla.org/de/docs/Web/CSS/CSS_flexible_box_layout/Basic_concepts_of_flexbox)
- [Grid-Layout – MDN Web Docs](https://developer.mozilla.org/de/docs/Web/CSS/CSS_grid_layout/Basic_concepts_of_grid_layout)
