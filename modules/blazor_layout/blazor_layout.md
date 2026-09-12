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

Mit den Steuerelementen aus [Razor-Komponenten und Steuerelemente](/modules/razor_komponenten/razor_komponenten.md) können wir alles auf die Seite bringen, was das Spiel braucht – aber der Browser stellt es dann untereinander dar, wie einen Fließtext. Die 200 `<div>`-Elemente des Spielfelds würden zu einer endlosen Kolonne, die Werkzeugleiste stünde in drei Zeilen, und die Statusleiste läge irgendwo darunter statt daneben. Aus dieser Kolonne ein Raster zu machen, ist die Aufgabe des **Layouts**. Auf dem Desktop übernehmen das spezielle Container-Steuerelemente, im Web erledigt es **CSS**. Die gute Nachricht: Für eine aufgeräumte Oberfläche reichen zwei CSS-Werkzeuge, Flexbox und Grid, und wenige Zeilen pro Bereich – das gesamte Layout von `Adventure.Web` passt auf eine knappe Bildschirmseite.

## Struktur und Gestaltung trennen

Eine Webseite besteht aus zwei Ebenen. **HTML** beschreibt die Struktur: Was ist eine Überschrift, was eine Liste, was gehört zusammen. **CSS** (*Cascading Style Sheets*) beschreibt die Gestaltung: Farben, Abstände, Schriften – und eben die Anordnung. Diese Trennung ist dieselbe Idee wie Markup und `@code`: Jede Ebene hat ihre Aufgabe, und man kann eine ändern, ohne die andere anzufassen.

Die Brücke zwischen beiden sind **Klassen**. Im Markup bekommt ein Element ein `class`-Attribut, in der CSS-Datei beschreibt ein Selektor `.klassenname { ... }`, wie alle Elemente dieser Klasse aussehen. Die Stylesheet-Datei liegt in unseren Projekten unter `wwwroot/app.css` – der Ordner `wwwroot` enthält alles, was der Server unverändert an den Browser ausliefert, und `App.razor` bindet die Datei mit einem `<link>` ein.

## Das Layout: Rahmen für jede Seite

Den äußeren Rahmen liefert `MainLayout.razor`. Wir haben die fast leere Vorlage um eine Kopfzeile ergänzt:

```razor
@inherits LayoutComponentBase

<header class="kopfzeile">
    <h1>Adventure</h1>
</header>

<main>
    @Body
</main>
```

`@Body` ist die Stelle, an der der Router die jeweilige Seite einsetzt; alles außerhalb bleibt auf jeder Seite gleich. Die Grundfarben der Anwendung stehen in `app.css` – ein dunkler Hintergrund, wie es sich für einen Kerker gehört:

```css
body { font-family: system-ui, sans-serif; margin: 0; background: #1e1e2e; color: #eee; }
.kopfzeile { background: #11111b; padding: 0.5rem 1.5rem; }
.kopfzeile h1 { margin: 0; font-size: 1.4rem; }
main { padding: 1rem 1.5rem; }
```

Mehr braucht der Rahmen nicht. Die Einheit `rem` ist relativ zur Standardschriftgröße, sodass die Seite bei vergrößerter Schrift mitwächst. Die eigentliche Anordnung passiert innerhalb der Startseite.

## Flexbox: Elemente in einer Reihe

Die Werkzeugleiste ist ein `<div class="werkzeuge">` mit der Levelauswahl und einem Button. Ohne CSS stünden sie zwar nebeneinander, aber ohne kontrollierten Abstand und mit unterschiedlicher Höhenausrichtung. **Flexbox** macht aus einem Container eine Reihe (oder Spalte), in der die Kinder verteilt werden:

```css
.werkzeuge { display: flex; gap: 1rem; align-items: center; margin-bottom: 1rem; }
```

`display: flex` schaltet Flexbox ein, `gap` legt den Abstand zwischen den Kindern fest, und `align-items: center` richtet sie quer zur Reihe mittig aus – so sitzt der Button auf derselben Höhe wie die Beschriftung „Level:“. Mit `flex-direction: column` würde die Reihe zur Spalte, `justify-content` verteilt die Kinder entlang der Richtung (etwa `flex-end` für rechtsbündig, `space-between` für maximalen Abstand). Dieselben vier Zeilen ordnen auch die Buttons im Spielende-Dialog:

```css
.dialog-buttons { display: flex; justify-content: center; gap: 0.5rem; margin-top: 1rem; }
```

Flexbox denkt immer in **einer Richtung** – das ist ihre Stärke und ihre Grenze. Sobald Zeilen *und* Spalten gleichzeitig eine Rolle spielen, ist ein anderes Werkzeug dran.

## Grid: das Spielfeld als Raster

Unter der Werkzeugleiste liegt der Arbeitsbereich: links das Spielfeld, rechts die Statusleiste in fester Breite. Das ist der erste Einsatz von **CSS-Grid**, und er ist noch harmlos:

```css
.arbeitsbereich { display: grid; grid-template-columns: auto 240px; gap: 1.5rem; align-items: start; }
```

`grid-template-columns: auto 240px` definiert zwei Spalten: Die erste ist so breit wie ihr Inhalt – das Spielfeld –, die zweite immer 240 Pixel. Die beiden Kinder des Containers, das Spielfeld und die `<Statusleiste />`, füllen sie der Reihe nach. `align-items: start` sorgt dafür, dass die Statusleiste oben beginnt und nicht auf die Höhe des Spielfelds gestreckt wird. Die Einheit `fr` (*fraction*) wäre die Alternative: `1fr 2fr` verteilt den verfügbaren Platz im Verhältnis 1 : 2.

Das eigentlich Interessante ist das Spielfeld selbst. Die Razor-Schleifen erzeugen einen flachen Strom von `<div class="feld">`-Elementen ohne jede Schachtelung – aus ihm wird erst durch Grid ein Raster:

```css
.spielfeld { display: grid; gap: 1px; background: #333; border: 3px solid #555; outline: none; width: max-content; }
.feld { width: 28px; height: 28px; display: flex; align-items: center; justify-content: center; font-size: 20px; background: #2a2a3c; }
```

Auffällig ist, was in `.spielfeld` **fehlt**: die Anzahl der Spalten. Sie kann dort nicht stehen, denn sie hängt vom geladenen Level ab – „Kerker“ ist 20 Zellen breit, „Katakomben“ 24. Deshalb setzt das Markup sie zur Laufzeit:

```razor
<div class="spielfeld" style="grid-template-columns: repeat(@feld.Breite, 1fr);">
```

`repeat(20, 1fr)` ist die Kurzform für zwanzig gleich breite Spalten. Grid füllt sie automatisch von links nach rechts und beginnt nach der zwanzigsten Zelle eine neue Zeile – genau die Reihenfolge, in der unsere beiden `for`-Schleifen die Zellen erzeugen. Der `gap: 1px` auf dem dunkelgrauen Hintergrund des Containers ergibt nebenbei ein feines Gitternetz, ohne dass eine einzige Zelle einen Rahmen bekäme.

Faustregel: **Flexbox für eine Richtung, Grid für zwei.** Eine Werkzeugleiste, eine Button-Reihe – Flexbox. Ein Arbeitsbereich mit Spalten fester Verhältnisse, ein Spielfeld mit Zeilen und Spalten – Grid. Beide lassen sich verschachteln: Jede `.feld`-Zelle ist innen wieder ein winziger Flex-Container, nur damit das Emoji exakt in der Mitte sitzt.
{: .notice--primary}

## Zustand sichtbar machen: Klassen statt Zeichenbefehle

Eine Wand soll anders aussehen als Boden, der Spieler anders als ein Gegner. In einem klassischen Framework würde man dafür beim Zeichnen Farben setzen. In Blazor gibt die Komponente jeder Zelle eine zweite CSS-Klasse mit, die aus dem Objekt abgeleitet ist – `KlasseFuer(objekt)` liefert `"wand"`, `"spieler"`, `"gegner"`, `"objekt"` oder `"boden"` –, und das Stylesheet entscheidet über das Aussehen:

```css
.feld.wand { background: #45475a; }
.feld.spieler { background: #1e3a5f; }
.feld.gegner { background: #5a1e1e; }
```

Der Selektor `.feld.wand` (ohne Leerzeichen!) trifft Elemente, die **beide** Klassen haben. Will man die Farbgebung ändern, fasst man nur CSS an; will man ein neues Spielobjekt farblich auszeichnen, ergänzt man einen Zweig in `KlasseFuer` und eine Zeile CSS. Die Schleife, die das Feld zeichnet, bleibt in beiden Fällen unberührt.

Zum Spielfeld gehört noch eine Regel, die man leicht übersieht: `.spielfeld:focus { border-color: #89b4fa; }`. Sie färbt den Rahmen, sobald das Feld den Tastaturfokus hat – für ein Spiel, das mit Pfeiltasten gesteuert wird, ist das kein Schmuck, sondern eine notwendige Rückmeldung. Warum das Feld überhaupt fokussierbar ist, klärt [Ereignisse in Blazor](/modules/blazor_ereignisse/blazor_ereignisse.md).
{: .notice--warning}

## Overlays: der Dialog über allem

Ist das Spiel gewonnen oder verloren, legt sich ein Dialog über die Seite. Dafür kombinieren wir eine Positionierung mit Flexbox:

```css
.dialog-hintergrund { position: fixed; inset: 0; background: rgba(0, 0, 0, 0.6); display: flex; align-items: center; justify-content: center; }
.dialog { background: #2a2a3c; padding: 1.5rem 2rem; border-radius: 8px; min-width: 300px; text-align: center; }
```

`position: fixed` löst das Element aus dem normalen Textfluss und heftet es an das Browserfenster; `inset: 0` setzt alle vier Abstände zum Rand auf null, sodass der Hintergrund das ganze Fenster füllt. Die halbtransparente Farbe dunkelt ab, und weil der Hintergrund gleichzeitig ein Flex-Container mit zentrierter Ausrichtung ist, sitzt der eigentliche `.dialog` genau in der Mitte. Das ist alles, was ein modaler Dialog im Web braucht – kein zweites Fenster, keine Bibliothek.

Damit ist das Layout komplett: Kopfzeile aus dem `MainLayout`, Werkzeugleiste als Flex-Reihe, Arbeitsbereich als Grid mit zwei Spalten, darin das Spielfeld als eigenes Grid, und bei Bedarf der Dialog als Overlay darüber.

## Scoped CSS und CSS-Frameworks

Neben `app.css` erlaubt Blazor **komponentenbezogene** Stylesheets: Eine Datei `MainLayout.razor.css` neben `MainLayout.razor` gilt nur für das Markup dieser Komponente. Blazor sorgt beim Bauen dafür, dass die Regeln nicht auf andere Komponenten durchschlagen – die Vorlage nutzt das für die Fehlerleiste `#blazor-error-ui`. Für ein Projekt dieser Größe ist eine zentrale `app.css` übersichtlicher; sobald mehrere Komponenten gleiche Klassennamen mit unterschiedlicher Bedeutung verwenden, lohnt sich die Trennung.

Die Standardvorlage von `dotnet new blazor` bringt außerdem das CSS-Framework **Bootstrap** mit: fertige Klassen wie `btn` oder `container`, die ein einheitliches Aussehen ohne eigenes CSS ergeben. Wir haben die Vorlage mit `--empty` erzeugt und bleiben bei eigenem CSS – so bleibt nachvollziehbar, welche Regel welche Wirkung hat, und die wenigen Zeilen für Flexbox und Grid lernt man dabei gleich mit.

Übung: Große Level passen bei 28 Pixeln pro Zelle nicht mehr auf den Bildschirm. Ändere `.feld` so, dass die Zellengröße aus einer CSS-Variablen kommt (`--zellengroesse`), und setze sie im Markup abhängig von `feld.Breite`. Sorge anschließend dafür, dass die Statusleiste unter das Spielfeld rutscht, statt daneben, sobald das Fenster schmaler als 700 Pixel ist – welche CSS-Regel brauchst du dafür, und welche Zeile von `.arbeitsbereich` musst du darin überschreiben?
{: .notice--info}

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v04-blazor`).

## Weitere Quellen

- [Blazor-Layouts – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/layouts)
- [CSS-Isolation in Blazor – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/css-isolation)
- [Flexbox – MDN Web Docs](https://developer.mozilla.org/de/docs/Web/CSS/CSS_flexible_box_layout/Basic_concepts_of_flexbox)
- [Grid-Layout – MDN Web Docs](https://developer.mozilla.org/de/docs/Web/CSS/CSS_grid_layout/Basic_concepts_of_grid_layout)
