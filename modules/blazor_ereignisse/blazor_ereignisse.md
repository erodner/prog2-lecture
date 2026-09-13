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

In [GUI-Grundbegriffe](/modules/gui_grundbegriffe/gui_grundbegriffe.md) haben wir festgehalten, was Oberflächenprogrammierung ausmacht: Wir schreiben keinen Ablauf mehr, sondern Reaktionen. Die Konsolenversion des Adventures fragt mit `Console.ReadKey` aktiv nach der nächsten Taste; die Browserversion wird *gefragt*, sobald eine Taste fällt. Genau drei Ereignisse hält die Startseite bereit: ein Klick auf „Neu starten“, eine geänderte Auswahl in der Levelliste und – das wichtigste – ein Tastendruck auf dem Spielfeld. Dieses Modul zeigt die Formen, in denen Ereignisbehandler in Blazor auftreten, was Blazor nach jedem Handler von selbst erledigt und welche drei Details nötig sind, damit ein `<div>` überhaupt Tastendrücke bekommt.

## Eine Methode als Handler

Die Grundform kennen wir: Das Attribut `@onclick` bekommt den Namen einer Methode aus dem `@code`-Block.

```razor
<button @onclick="NeuStarten">Neu starten</button>

@code {
    private Spielfeld feld = null!;
    private string levelName = "";

    private void NeuStarten()
    {
        feld = LevelParser.Parsen(LevelQuelle.Laden(levelName));
    }
}
```

Der Handler ist eine gewöhnliche Methode ohne Parameter und ohne Rückgabewert. Er tut genau das, was sein Name sagt – ein frisches Spielfeld aus dem gewählten Level bauen – und überlässt die eigentliche Arbeit dem `LevelParser` aus dem Spielkern. Auffällig ist wieder, was fehlt: Nirgends steht, dass die 180 Zellen des Spielfelds neu gezeichnet werden sollen.

Nach jedem Ereignisbehandler rendert Blazor die Komponente **automatisch neu**. Die beiden `for`-Schleifen laufen erneut, diesmal über das neue `feld`, und die Statusleiste bekommt die neuen Parameterwerte. Wir ändern nur Felder; die Anzeige folgt. Wann genau Blazor neu rendert und wie man das in Sonderfällen selbst anstößt, sehen wir in [Datenbindung und Rendern](/modules/blazor_datenbindung/blazor_datenbindung.md).
{: .notice--primary}

Der Name im Markup wird beim Kompilieren aufgelöst: `@onclick="NeuStarten"` ist kein String, sondern ein Verweis auf die Methode. Ein Tippfehler wie `@onclick="NeuStaten"` führt deshalb zu einem **Compilerfehler**, nicht zu einem stillen Fehlverhalten zur Laufzeit – ein großer Vorteil gegenüber Frameworks, die Handler über Zeichenketten verknüpfen.
{: .notice--warning}

## Ereignisargumente: die Tastatursteuerung

Manchmal reicht die Information „es ist etwas passiert“ nicht aus. Blazor übergibt dann ein **Ereignisargument**, dessen Typ zum Ereignis passt: `MouseEventArgs` bei Mausereignissen, `ChangeEventArgs` bei Eingaben, `FocusEventArgs` beim Fokuswechsel – und `KeyboardEventArgs` bei Tastendrücken. Dessen Property `Key` enthält den Namen der gedrückten Taste als Text, genau wie ihn der Browser meldet:

```razor
<div class="spielfeld" tabindex="0" @onkeydown="TasteGedrueckt" @onkeydown:preventDefault>
    @* ... die Zellen ... *@
</div>

@code {
    private void TasteGedrueckt(KeyboardEventArgs e)
    {
        Richtung? richtung = e.Key switch
        {
            "ArrowUp" or "w" or "W" => Richtung.Oben,
            "ArrowDown" or "s" or "S" => Richtung.Unten,
            "ArrowLeft" or "a" or "A" => Richtung.Links,
            "ArrowRight" or "d" or "D" => Richtung.Rechts,
            _ => null
        };
        if (richtung is Richtung r)
        {
            feld.SpielerZieht(r);   // danach rendert Blazor die Komponente automatisch neu
        }
    }
}
```

Das ist die zentrale Methode der Oberfläche – und sie ist bemerkenswert dumm. Sie übersetzt einen Tastennamen in eine `Richtung` und ruft `feld.SpielerZieht(r)` auf. Was ein Zug bedeutet, ob eine Tür aufgeht, ob ein Verfolger zuschlägt, ob das Spiel endet: Davon steht hier kein Wort. Vergleiche das mit der Konsolenversion, die dieselbe Übersetzung von `ConsoleKey.UpArrow` nach `Richtung.Oben` macht – zwei Oberflächen, dieselbe eine Zeile Spiellogik. Warum das kein Zufall sein darf, vertieft [Schichten mit Blazor umsetzen](/modules/schichten_mit_blazor/schichten_mit_blazor.md).

Der `switch`-Ausdruck liefert ein `Richtung?`, also `null` für jede Taste, die uns nicht interessiert – drückt jemand die Leertaste, passiert schlicht nichts. Das Muster `if (richtung is Richtung r)` packt den Wert gleichzeitig aus und prüft auf `null`; das ist kürzer und sicherer als `richtung.Value`.

Blazor erkennt an der Signatur, ob die Methode das Argument haben will – `void Klick()` und `void Klick(MouseEventArgs e)` sind beide zulässig. Man muss die Argumenttypen nicht auswendig kennen: Die IDE schlägt beim Schreiben des Handlers den passenden Typ vor.

## Drei Details, damit Tasten ankommen

Die Methode oben allein reicht nicht. Ein `<div>` ist im Browser kein Bedienelement, und drei Ergänzungen sind nötig, damit die Steuerung funktioniert – jede behebt genau ein Problem:

**`tabindex="0"`** macht das `<div>` überhaupt erst **fokussierbar**. Ohne dieses Attribut bekommt es niemals ein `keydown`-Ereignis, egal was der Benutzer tippt, weil der Fokus im Browser nur auf Elementen liegen kann, die dafür vorgesehen sind – Buttons, Eingabefelder, Links. Mit `tabindex="0"` reiht sich das Spielfeld in diese Reihe ein.

**`@onkeydown:preventDefault`** unterdrückt die Standardreaktion des Browsers. Ohne diesen Zusatz würde jeder Druck auf die Pfeiltasten die Seite scrollen – der Spieler zieht, und die Ansicht springt weg. Der Zusatz hinter dem Doppelpunkt ist eine Blazor-Direktive und braucht keinen Wert; es gibt daneben `@onkeydown:stopPropagation`, das verhindert, dass das Ereignis an umgebende Elemente weitergereicht wird.

**Fokus setzen beim ersten Rendern.** Fokussierbar heißt noch nicht fokussiert: Beim Laden der Seite liegt der Fokus nirgends, und der Spieler müsste erst einmal auf das Feld klicken. Das nimmt die Komponente ihm ab, und zwar in einer Lebenszyklus-Methode:

```razor
<div class="spielfeld" tabindex="0" @ref="feldElement" ...>

@code {
    private ElementReference feldElement;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await feldElement.FocusAsync();
        }
    }
}
```

`@ref="feldElement"` verschafft der Komponente einen Griff auf das erzeugte HTML-Element, gespeichert in einer `ElementReference`. Blazor ruft `OnAfterRenderAsync` nach jedem Rendern auf und setzt `firstRender` nur beim allerersten Mal auf `true` – genau dort gehört der Fokus hin, denn später soll er dem Benutzer nicht mehr weggenommen werden. Früher geht es nicht: In `OnInitialized` existiert das `<div>` im Browser noch gar nicht.

`ElementReference` ist die einzige Stelle, an der wir ein konkretes HTML-Element anfassen – und sie ist bewusst mager: Man kann den Fokus setzen, mehr nicht. Wer versucht, darüber Texte oder Farben zu ändern, ist auf dem falschen Weg; dafür gibt es Felder und Markup.
{: .notice--warning}

## Lambdas als Handler

Nicht jeder Handler verdient eine eigene Methode. Wenn ein Klick nur ein Feld setzt oder einen Wert weiterreicht, ist ein **Lambda-Ausdruck** direkt im Markup kürzer. Der Spielende-Dialog nutzt das, um seinen Rückkanal auszulösen:

```razor
<button @onclick="() => OnNeustart.InvokeAsync()">Neu starten</button>
```

Besonders wertvoll ist diese Form innerhalb einer Schleife, wenn jeder Eintrag „seinen“ Wert übergeben soll:

```razor
@foreach (string name in LevelQuelle.LevelNamen)
{
    <button @onclick="() => LevelWechseln(name)">@name</button>
}
```

Mit einer benannten Methode `LevelWechseln()` ohne Parameter ginge das nicht, weil sie beim Aufruf nicht wüsste, welcher Button geklickt wurde. Weil `foreach` in C# für jeden Durchlauf eine neue Variable anlegt, merkt sich jeder Button zuverlässig seinen eigenen Namen. Die genaue Funktionsweise von Lambdas ist Thema von [Lambda-Ausdrücke](/modules/lambda_ausdruecke/lambda_ausdruecke.md); für den Moment reicht: Alles nach `=>` ist der Rumpf, und Variablen aus der Umgebung dürfen darin verwendet werden.

## Nach der Eingabe etwas tun: `@bind:after`

Die Levelauswahl ist ein Sonderfall. `@bind` belegt das `change`-Ereignis des `<select>` bereits selbst – ein zusätzliches `@onchange` am selben Element lehnt der Compiler ab. Soll nach der Eingabe noch etwas passieren, hängt man die Methode mit `@bind:after` an:

```razor
<select @bind="levelName" @bind:after="NeuStarten">
```

Die Reihenfolge ist garantiert: Erst schreibt Blazor den gewählten Wert in `levelName`, dann läuft `NeuStarten` – und lädt damit bereits das *neue* Level. Stünde die Zuweisung hinterher, würde das Spiel jedes Mal ein Level zu spät wechseln.

## Asynchrone Handler

Wenn ein Handler länger dauert – eine Datei laden, einen Webdienst befragen –, darf er die Ereignisschleife nicht blockieren. Blazor akzeptiert deshalb auch Handler mit der Signatur `async Task`; `OnAfterRenderAsync` oben war bereits ein Beispiel. `await` gibt die Kontrolle ab, während die Arbeit läuft, und der Handler wird fortgesetzt, sobald sie fertig ist. Blazor rendert nach dem Abschluss des gesamten `Task` erneut – und zusätzlich beim ersten `await`, damit die Seite währenddessen nicht eingefroren wirkt. Was `async` und `await` im Detail bewirken, behandeln wir bei den Streams und Dateien; für Handler gilt die einfache Regel: Ruft der Handler etwas mit `Async` im Namen auf, wird er selbst `async Task`.

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

Die Idee ist dieselbe: Eine Methode wird bei einem Steuerelement für ein Ereignis **registriert** – hier mit `+=` im Code statt mit `@onclick` im Markup. Die feste Signatur mit `sender` und `EventArgs` gibt es bei Blazor nicht; die Methode nimmt nur, was sie braucht. Genau diese klassische Form verwendet übrigens unser Spielkern selbst: `Spielfeld.RundeBeendet` und `Spieler.SchatzGefunden` sind C#-Ereignisse, an die sich jede Oberfläche mit `+=` hängen darf. Die Konsolenversion macht von `SchatzGefunden` Gebrauch und lässt bei jedem Fund einen Ton erklingen. Dass dahinter ein Sprachmittel steckt, mit dem wir eigene Ereignisse in eigenen Klassen definieren können, lernen wir in [Ereignisse](/modules/ereignisse/ereignisse.md).

Übung: Ergänze die Steuerung um eine Wartetaste: Drückt der Spieler die Leertaste, soll eine Runde vergehen, in der er stehen bleibt, die Gegner sich aber bewegen. Überlege zuerst, wo diese Regel hingehört – in `TasteGedrueckt` oder in den Spielkern – und begründe deine Antwort damit, was die Konsolenversion davon mitbekommen soll. Ergänze anschließend `@onkeydown` um die Taste `r` für „neu starten“.
{: .notice--info}

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v04-blazor`).

## Weitere Quellen

- [Ereignisbehandlung in Blazor – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/event-handling)
- [Ereignisargumenttypen – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/event-handling#event-arguments)
- [Lebenszyklus von Razor-Komponenten – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/lifecycle)
- [Tastaturereignisse: `KeyboardEvent.key` – MDN Web Docs](https://developer.mozilla.org/de/docs/Web/API/KeyboardEvent/key)
- [Das `keydown`-Ereignis – MDN Web Docs](https://developer.mozilla.org/de/docs/Web/API/Element/keydown_event) – mit einem Probierfeld auf der Seite, in dem du dir für jede Taste den `Key`-Namen anzeigen lassen kannst.
- [Command – Game Programming Patterns](https://gameprogrammingpatterns.com/command.html) – beginnt mit genau unserem Problem: Tastendrücke möglichst früh in Befehle des Spiels übersetzen, statt die Steuerung überall zu verteilen.
