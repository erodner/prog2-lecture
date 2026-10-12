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

Irgendwann ist das Spiel vorbei: Der Held erreicht den Ausgang, oder die Verfolger waren schneller. In beiden Fällen soll die Oberfläche innehalten, das Ergebnis zeigen und einen Weg zurück ins Spiel anbieten. Das ist die klassische Aufgabe eines **Dialogs**: ein Bereich, der vor die Seite tritt, etwas mitteilt oder Eingaben sammelt und eine Entscheidung zurückliefert. Desktop-Frameworks öffnen dafür ein zweites Fenster und liefern ein `DialogResult` zurück. Im Browser gibt es keine Fenster – ein Dialog in Blazor ist einfach eine **Komponente**, die gezeigt oder nicht gezeigt wird. Man kann sich das wie ein Formular am Schalter vorstellen: Man bekommt es in die Hand, füllt es aus und gibt es zurück. Wie das aussieht, zeigt der `SpielEndeDialog` von `Adventure.Web`.

## Der Dialog ist eine eigene Komponente

`Components/SpielEndeDialog.razor` hat keine `@page`-Anweisung – er ist keine Seite mit eigener Adresse, sondern ein Baustein, den andere Komponenten als Tag `<SpielEndeDialog />` einsetzen. Sein Markup ist erfreulich kurz:

<div class="editoren" markdown="1">
<div class="editor" markdown="1">
<div class="editor-titel"><a href="https://github.com/erodner/prog2-adventure/blob/v03-blazor/Adventure.Web/Components/SpielEndeDialog.razor">Adventure.Web/Components/SpielEndeDialog.razor</a></div>

```razor
@using Adventure.Kern

<div class="dialog-hintergrund">
    <div class="dialog">
        <h2>@(Status == Spielstatus.Gewonnen ? "Geschafft!" : "Besiegt")</h2>
        <p>
            @if (Status == Spielstatus.Gewonnen)
            {
                <text>@Punkte Punkte in @Runden Runden.</text>
            }
            else
            {
                <text>Die Gegner waren schneller. Versuch es noch einmal.</text>
            }
        </p>
        <div class="dialog-buttons">
            <button @onclick="OnNeustart">Neu starten</button>
        </div>
    </div>
</div>
```

</div>
</div>

Zwei Razor-Feinheiten stecken darin. In der Überschrift genügt ein bedingter Ausdruck in Klammern – `@( ... ? ... : ... )` –, weil nur ein Wort unterschiedlich ist. Im Absatz darunter stehen dagegen ganze Sätze mit eingebetteten Werten, und dafür braucht Razor eine Markierung, wo Text beginnt: Das Pseudo-Element `<text>` sagt „ab hier ist Ausgabe“, ohne selbst im HTML zu landen. Ohne es würde der Compiler `@Punkte Punkte in @Runden Runden.` nicht als Markup erkennen.

## Die Parameter: was der Dialog wissen muss

Der `@code`-Block besteht ausschließlich aus Parametern – der Dialog hat keinen eigenen Zustand:

<div class="editoren" markdown="1">
<div class="editor" markdown="1">
<div class="editor-titel"><a href="https://github.com/erodner/prog2-adventure/blob/v03-blazor/Adventure.Web/Components/SpielEndeDialog.razor">Adventure.Web/Components/SpielEndeDialog.razor</a> (<code>@code</code>-Block)</div>

```csharp
@code {
    [Parameter] public Spielstatus Status { get; set; }
    [Parameter] public int Punkte { get; set; }
    [Parameter] public int Runden { get; set; }
    [Parameter] public EventCallback OnNeustart { get; set; }
}
```

</div>
</div>

`Status` ist das `Spielstatus`-Enum aus dem Kern mit den Werten `Laeuft`, `Gewonnen` und `Verloren` – der Dialog bekommt also die *Tatsache*, nicht die Formulierung. Ob daraus „Geschafft!“ oder „Besiegt“ wird, ist eine Darstellungsfrage und darf in der Komponente entschieden werden; *wann* ein Spiel gewonnen ist, entscheidet dagegen `Spielfeld.SpielerZieht`. Diese Trennung ist der rote Faden der ganzen Vorlesung.

Der Dialog kennt weder das `Spielfeld` noch die Startseite noch die Levelquelle. Er bekommt drei Zahlen und einen Rückkanal – und genau deshalb könnte man ihn ohne Änderung in einer anderen Anwendung wiederverwenden. Je weniger eine Komponente kennt, desto länger lebt sie.
{: .notice--primary}

## Anzeigen durch bedingtes Rendern

Es gibt keinen Aufruf wie `dialog.Show()`. Stattdessen entscheidet eine Bedingung im Markup der Startseite, ob der Dialog überhaupt vorkommt:

<div class="editoren" markdown="1">
<div class="editor" markdown="1">
<div class="editor-titel"><a href="https://github.com/erodner/prog2-adventure/blob/v03-blazor/Adventure.Web/Components/Pages/Home.razor">Adventure.Web/Components/Pages/Home.razor</a></div>

```razor
@if (feld.Status != Spielstatus.Laeuft)
{
    <SpielEndeDialog Status="@feld.Status" Punkte="@feld.Spieler.Punkte" Runden="@feld.Runde"
                     OnNeustart="NeuStarten" />
}
```

</div>
</div>

Hier gibt es nicht einmal ein `bool`-Feld `dialogOffen`: Die Bedingung liest den Spielstatus direkt aus dem Kern. Zieht der Spieler auf den Ausgang, setzt `SpielerZieht` den Status auf `Gewonnen`; Blazor rendert nach dem Tastendruck neu, die Bedingung trifft zu, und der Dialog erscheint. Klickt der Spieler auf „Neu starten“, baut `NeuStarten` ein frisches Spielfeld mit dem Status `Laeuft`, die Bedingung trifft nicht mehr zu, und der Dialog verschwindet samt allen seinen Feldern. Ein Zustand weniger, den man synchron halten muss.

Ein Dialog, der eigene Eingaben sammelt – etwa ein Formular für einen Heldennamen –, braucht dagegen sehr wohl ein `bool`-Feld, weil sein Erscheinen dann nicht aus dem Fachzustand ableitbar ist. Die Faustregel: Wenn eine Bedingung über vorhandene Daten den Dialog exakt beschreibt, nimm sie; sonst ein eigenes Feld.

## Das Ergebnis: `EventCallback`

Wie meldet der Dialog, dass der Benutzer „Neu starten“ geklickt hat? Über einen **Parameter** vom Typ `EventCallback`. Ein `EventCallback` ist eine Methode der Elternkomponente, die das Kind aufrufen darf – ein typisiertes Ereignis:

<div class="editoren" markdown="1">
<div class="editor" markdown="1">
<div class="editor-titel"><a href="https://github.com/erodner/prog2-adventure/blob/v03-blazor/Adventure.Web/Components/SpielEndeDialog.razor">Adventure.Web/Components/SpielEndeDialog.razor</a></div>

```razor
<button @onclick="OnNeustart">Neu starten</button>

@code {
    [Parameter] public EventCallback OnNeustart { get; set; }
}
```

</div>
<div class="editor" markdown="1">
<div class="editor-titel"><a href="https://github.com/erodner/prog2-adventure/blob/v03-blazor/Adventure.Web/Components/Pages/Home.razor">Adventure.Web/Components/Pages/Home.razor</a></div>

```razor
<SpielEndeDialog …
                 OnNeustart="NeuStarten" />
```

</div>
</div>

Links löst der Dialog den Rückkanal aus, rechts weist die Startseite ihm eine Methode zu: `OnNeustart="NeuStarten"`. Der Dialog weiß nicht, was diese Methode tut; er weiß nur, dass er sie auslösen darf. Klassische Frameworks liefern an dieser Stelle ein `DialogResult.OK` zurück, und der Aufrufer muss anschließend selbst entscheiden, was das bedeutet.

Braucht der Dialog einen *Wert* zurück – eine Auswahl, eine Eingabe, ein `null` bei Abbruch –, nimmt man die generische Form `EventCallback<T>`. Eine Levelauswahl als eigene Komponente würde `EventCallback<string>` melden, ein Dialog mit Abbrechen-Knopf `EventCallback<string?>`, damit der Abbruch ein eigener, prüfbarer Fall ist. Aufgerufen wird sie mit `InvokeAsync(wert)`, und der Handler der Eltern nimmt den Wert als Parameter entgegen.

Nach einem `EventCallback` rendert Blazor die Elternkomponente automatisch neu. Das ist der Unterschied zu einem gewöhnlichen `Action`-Delegaten – mit dem müsste die Startseite selbst `StateHasChanged` aufrufen. Für Kind-nach-Eltern-Kommunikation deshalb immer `EventCallback`.
{: .notice--primary}

## Der Handler der Eltern

Bemerkenswert an der Startseite ist, dass sie für den Dialog gar keinen eigenen Handler schreibt. `OnNeustart="NeuStarten"` verweist auf dieselbe Methode, die auch am Button „Neu starten“ in der Werkzeugleiste hängt:

<div class="editoren" markdown="1">
<div class="editor" markdown="1">
<div class="editor-titel"><a href="https://github.com/erodner/prog2-adventure/blob/v03-blazor/Adventure.Web/Components/Pages/Home.razor">Adventure.Web/Components/Pages/Home.razor</a> (im <code>@code</code>-Block)</div>

```csharp
private void NeuStarten()
{
    feld = LevelParser.Parsen(LevelQuelle.Laden(levelName));
}
```

</div>
</div>

Drei Wege führen also zu derselben Methode – der Button in der Werkzeugleiste, die geänderte Levelauswahl über `@bind:after` und der Dialog über den `EventCallback`. Das ist kein Zufall, sondern das Ergebnis davon, dass der Handler nichts über seinen Auslöser weiß. Hätte `NeuStarten` einen Parameter „woher komme ich“, wäre diese Wiederverwendung sofort verloren.

## Modal per CSS

Ein Dialog soll **modal** sein: Solange er offen ist, darf der Benutzer nicht in die Seite dahinter klicken. In Blazor braucht das keinen Code, sondern nur die beiden Klassen aus `wwwroot/app.css`, die wir schon aus [Layout mit HTML und CSS](/modules/blazor_layout/blazor_layout.md) kennen:

<div class="editoren" markdown="1">
<div class="editor" markdown="1">
<div class="editor-titel"><a href="https://github.com/erodner/prog2-adventure/blob/v03-blazor/Adventure.Web/wwwroot/app.css">Adventure.Web/wwwroot/app.css</a></div>

```css
.dialog-hintergrund { position: fixed; inset: 0; background: rgba(0, 0, 0, 0.6);
                      display: flex; align-items: center; justify-content: center; }
.dialog { background: #2a2a3c; padding: 1.5rem 2rem; border-radius: 8px;
          min-width: 300px; text-align: center; }
```

</div>
</div>

`position: fixed` mit `inset: 0` spannt den halbtransparenten Hintergrund über das ganze Browserfenster; er fängt alle Klicks ab, die nicht den Dialog treffen. `display: flex` mit zentrierter Ausrichtung setzt den eigentlichen Dialog in die Mitte. Damit ist der Dialog modal, ohne dass die Startseite ihre Buttons deaktivieren müsste.

Die Tastatursteuerung ist dagegen *nicht* blockiert: Das Spielfeld hat noch den Fokus und reagiert weiter auf Pfeiltasten. Schlimmes passiert nicht, weil `SpielerZieht` bei einem beendeten Spiel sofort zurückkehrt (`if (Status != Spielstatus.Laeuft) return;`) – aber verlassen sollte man sich auf so etwas nicht. Sauberer wäre, beim Erscheinen des Dialogs den Fokus auf dessen Button zu setzen.
{: .notice--warning}

Übung: Baue eine Komponente `TruheDialog.razor`, die kurz erscheint, sobald der Held eine Truhe geöffnet hat: Sie zeigt den Wert des gefundenen Schatzes und den neuen Punktestand und hat einen Button „Weiter“. Kläre dabei drei Fragen. Erstens: Welche Parameter braucht sie, und welchen Typ hat ihr Rückkanal? Zweitens: Woran erkennt die Startseite, dass gerade eine Truhe geöffnet wurde – reicht eine Bedingung über den Spielzustand wie beim `SpielEndeDialog`, oder brauchst du ein Feld? Drittens: Wo müsste diese Information entstehen, und welche Schichtregel würdest du verletzen, wenn du die Truhe nach der Komponente fragen ließest, ob sie sich anzeigen soll?
{: .notice--info}

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag [`v03-blazor`](https://github.com/erodner/prog2-adventure/tree/v03-blazor)).

## Weitere Quellen

- [EventCallback – Ereignisbehandlung in Blazor – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/event-handling#eventcallback)
- [Komponentenparameter – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/#component-parameters)
- [Blazor-Formulare und Validierung – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/forms/)
- [Das `<dialog>`-Element – MDN Web Docs](https://developer.mozilla.org/de/docs/Web/HTML/Reference/Elements/dialog) – der Browser bringt inzwischen selbst einen modalen Dialog mit; lohnender Vergleich zu unserer Lösung aus zwei `<div>` und etwas CSS.
- [Blazor University](https://blazor-university.com/) – das Kapitel zu `EventCallback` zeigt an kleinen Beispielen, warum es kein gewöhnlicher Delegat ist.
