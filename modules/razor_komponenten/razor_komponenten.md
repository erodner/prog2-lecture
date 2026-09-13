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

In [Die erste Blazor-App](/modules/blazor_erste_app/blazor_erste_app.md) haben wir gesehen, dass eine Razor-Datei aus Markup und einem `@code`-Block besteht. Die Startseite von `Adventure.Web` muss deutlich mehr können als ein Eingabefeld mit Button: ein Spielfeld aus Dutzenden Zellen zeichnen, das sich nach jedem Zug ändert, daneben Lebenspunkte und Meldungen anzeigen, ein Level zur Auswahl stellen und am Spielende einen Dialog einblenden. Für all das brauchen wir keine neue Sprache, sondern zwei Dinge: die **Razor-Syntax**, mit der C#-Werte und C#-Kontrollfluss ins Markup gelangen, und das Wissen, welche HTML-Elemente die Rolle der Steuerelemente übernehmen. Am Ende sehen wir, dass jede Razor-Datei selbst zu einem Steuerelement wird, das andere Seiten einbetten können.

## Razor-Syntax: C# im Markup

Razor ist HTML, in dem das Zeichen `@` in die Welt von C# umschaltet. Die einfachste Form ist ein Ausdruck, dessen Wert als Text eingefügt wird:

```razor
<p>Runde: @feld.Runde</p>
<p>Held: @feld.Spieler.Name</p>
<p>Verbleibend: @(feld.Spieler.Lebenspunkte * 10) Prozent</p>
```

Ein einzelner Bezeichner wie `@name` oder eine Kette aus Punkten und Methodenaufrufen wie `@feld.Spieler.Beschreibung()` kommt ohne Klammern aus. Sobald Operatoren im Spiel sind, braucht der Ausdruck runde Klammern – `@(a * 10)` –, sonst würde Razor nach `@a` wieder in HTML zurückwechseln und ` * 10` als Text ausgeben. Die Werte werden beim Einfügen automatisch HTML-kodiert; ein `<` in einer Meldung landet also als Text auf der Seite und nicht als Tag.

Neben Ausdrücken gibt es Kontrollfluss. `@if`, `@else`, `@foreach` und `@for` funktionieren wie in C#, nur dass ihr Rumpf Markup enthält statt Anweisungen. Die Levelauswahl der Startseite baut so ihre Einträge:

```razor
<label>Level:
    <select @bind="levelName" @bind:after="NeuStarten">
        @foreach (string name in LevelQuelle.LevelNamen)
        {
            <option value="@name">@name</option>
        }
    </select>
</label>
```

Für jeden Levelnamen entsteht eine `<option>`. Es gibt keine Methode `ListeAktualisieren`, die wir nach einer Änderung aufrufen müssten: Das Markup ist eine **Beschreibung** des gewünschten Zustands, keine Folge von Befehlen. Kämen morgen drei Level dazu, hätte die Auswahlliste drei Einträge mehr, ohne dass eine Zeile Markup sich ändert.

## Das Spielfeld: zwei Schleifen über das Raster

Der interessanteste Teil der Seite ist die Karte. Ein `Spielfeld` kennt seine `Breite` und `Hoehe` und beantwortet mit `ObjektAn(Position)`, was an einer Stelle liegt – oder `null`, wenn dort Boden ist. Genau daraus entsteht das Markup:

```razor
<div class="spielfeld" style="grid-template-columns: repeat(@feld.Breite, 1fr);">
    @for (int y = 0; y < feld.Hoehe; y++)
    {
        for (int x = 0; x < feld.Breite; x++)
        {
            Spielobjekt? objekt = feld.ObjektAn(new Position(x, y));
            <div class="feld @KlasseFuer(objekt)" title="@objekt?.Beschreibung()">@SymbolFuer(objekt)</div>
        }
    }
</div>
```

Im Projekt trägt dieses `<div>` noch drei weitere Attribute, die mit der Tastatursteuerung zu tun haben; sie kommen in [Ereignisse in Blazor](/modules/blazor_ereignisse/blazor_ereignisse.md) dazu. Für den Aufbau des Rasters spielen sie keine Rolle, deshalb fehlen sie hier.

Zwei Feinheiten stecken darin. Erstens braucht nur die **äußere** Schleife das `@` – es schaltet einmal nach C# um, und alles im Rumpf ist bereits C#. Die innere `for`-Schleife und die Zuweisung `Spielobjekt? objekt = ...` stehen deshalb ohne `@` da; nur die Zeile, die mit `<` beginnt, wird wieder als Markup erkannt. Zweitens erzeugen die beiden Schleifen einen *flachen* Strom von `<div>`-Elementen, ohne Zeilenumbrüche im Markup. Dass daraus ein Raster wird, entscheidet das CSS-Grid – dazu mehr in [Layout mit HTML und CSS](/modules/blazor_layout/blazor_layout.md).

Was die einzelne Zelle anzeigt, entscheiden zwei statische Hilfsmethoden im `@code`-Block. Sie sind das Gegenstück zu `Spielobjekt.Symbol` aus dem Kern: Dort steht ein `char` für die Konsole, hier ein Emoji für den Browser.

```csharp
private static string SymbolFuer(Spielobjekt? objekt) => objekt switch
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
    _ => objekt.Symbol.ToString()
};
```

Hier zahlt sich die Klassenhierarchie aus [Vorlesung 01](/lectures/01/01.md) und [Vorlesung 02](/lectures/02/02.md) aus: Der `switch`-Ausdruck arbeitet mit **Typmustern**, prüft also den Laufzeittyp des Objekts. Zwei Zweige benennen ihr Objekt zusätzlich (`Tuer t`), weil sie eine Property abfragen müssen – eine offene Tür sieht anders aus als eine verschlossene. Der letzte Zweig `_ => objekt.Symbol.ToString()` ist die Rückfallebene: Ein Spielobjekt, an das hier niemand gedacht hat, erscheint mit seinem Konsolenzeichen statt gar nicht.

Die Reihenfolge der Zweige ist bei Typmustern entscheidend: `Wache` und `Verfolger` erben beide von `Gegner`. Stünde ein Zweig `Gegner => "👾"` weiter oben, wären beide Zweige darunter unerreichbar – der Compiler meldet das glücklicherweise als Fehler CS8120.
{: .notice--warning}

## HTML-Elemente sind die Steuerelemente

Desktop-Frameworks bringen eigene Klassen wie `TextBox` oder `ComboBox` mit. In Blazor übernehmen die eingebauten HTML-Elemente diese Rolle – der Browser zeichnet sie, wir binden sie an unsere Felder. Die wichtigsten auf einen Blick:

| Steuerelement | HTML-Element | Wichtigstes Binding/Attribut |
| :--- | :--- | :--- |
| Textfeld | `<input>` | `@bind="name"` |
| Zahlenfeld | `<input type="number">` | `@bind="anzahl"` (bindet an `int`/`double`) |
| Checkbox | `<input type="checkbox">` | `@bind="aktiv"` (bindet an `bool`) |
| Auswahlliste | `<select>` mit `<option>` | `@bind="levelName"` |
| Button | `<button>` | `@onclick="NeuStarten"` |
| Bereich mit Tastaturfokus | `<div tabindex="0">` | `@onkeydown="TasteGedrueckt"` |
| Liste | `<ul>` mit `<li>` | `@foreach` |
| Fortschrittsanzeige | `<progress>` | `value="@lebenspunkte" max="3"` |
| Beschriftung | `<label>`, `<p>` | Text mit `@`-Ausdrücken |

Die Werkzeugleiste des Spiels besteht aus zwei dieser Elemente: der Levelauswahl von oben und einem Button, der eine Methode aus dem `@code`-Block auslöst.

```razor
<button @onclick="NeuStarten">Neu starten</button>
```

Auch Attribute dürfen aus C# stammen – nicht nur der Inhalt eines Elements. Das nutzt das Spielfeld gleich zweimal: `style="grid-template-columns: repeat(@feld.Breite, 1fr);"` berechnet die Spaltenzahl aus dem geladenen Level, und `class="feld @KlasseFuer(objekt)"` hängt an die feste Klasse `feld` eine zweite an, die vom Inhalt der Zelle abhängt. Für `bool`-Attribute wie `disabled` ist Blazor besonders hilfreich: Ergibt der Ausdruck `true`, wird das Attribut gesetzt, bei `false` ganz weggelassen – genau so, wie HTML es erwartet. Der Zustand lebt immer in C#, das Aussehen folgt daraus; wir manipulieren nie direkt „das Element auf der Seite“.

## Eigene Komponenten

Bis hierher haben wir nur eingebaute HTML-Elemente verwendet. Die eigentliche Stärke von Blazor ist, dass **jede `.razor`-Datei eine wiederverwendbare Komponente** ist, die sich wie ein Tag einbinden lässt. `<PageTitle>Adventure</PageTitle>` am Anfang von `Home.razor` war schon so ein Fall: keine HTML-Vorschrift, sondern eine Komponente von Blazor, die den Text in der Browser-Registerkarte setzt.

Die Anzeige neben dem Spielfeld – Name, Lebenspunkte, Punkte, Inventar, Runde und letzte Meldung – ist lang genug, um sie aus der Seite herauszulösen. Sie liegt deshalb in einer eigenen Datei `Components/Statusleiste.razor`:

```razor
@using Adventure.Kern

<div class="status">
    <h2>@Spieler.Name</h2>
    <p>Lebenspunkte: <span class="herzen">@Herzen</span></p>
    <p>Punkte: <strong>@Spieler.Punkte</strong></p>
    <p>Inventar: @Spieler.Inventar</p>
    <p>Runde: @Runde</p>
    <p class="meldung">@Meldung</p>
</div>

@code {
    [Parameter, EditorRequired] public Spieler Spieler { get; set; } = null!;
    [Parameter] public int Runde { get; set; }
    [Parameter] public string Meldung { get; set; } = "";

    private string Herzen => new string('♥', Spieler.Lebenspunkte)
                           + new string('♡', Spieler.MaxLebenspunkte - Spieler.Lebenspunkte);
}
```

Die Startseite bindet sie als Tag ein und füllt ihre Parameter wie HTML-Attribute:

```razor
<Statusleiste Spieler="feld.Spieler" Runde="feld.Runde" Meldung="@feld.LetzteMeldung" />
```

Der Tag-Name ist der Dateiname, und die Attribute sind **Parameter** – öffentliche Properties, die im `@code`-Block mit `[Parameter]` markiert sind. Über sie fließen Daten von der einbettenden Seite in die Komponente hinein. Der Zusatz `EditorRequired` bei `Spieler` sorgt dafür, dass der Compiler warnt, wenn jemand die Statusleiste ohne Spieler einbindet; `null!` daneben beruhigt die Nullable-Analyse, weil der Wert garantiert von außen gesetzt wird. `Runde` und `Meldung` haben sinnvolle Standardwerte und sind deshalb optional.

Ein Detail lohnt einen zweiten Blick: Bei `Meldung` steht ein `@` vor dem Wert, bei `Spieler` und `Runde` nicht. Der Grund ist der Parametertyp. `Spieler` und `Runde` sind kein `string`, also liest Razor den Attributwert ohnehin als C#-Ausdruck. `Meldung` ist ein `string` – dort nimmt Razor den Text wörtlich, wenn kein `@` davorsteht. Ohne das `@` stünde in der Statusleiste tatsächlich der Text „feld.LetzteMeldung“ statt der Meldung. Der Compiler meckert nicht, weil eine Zeichenkette an einen `string`-Parameter zu übergeben völlig in Ordnung ist – der Fehler fällt erst im Browser auf.
{: .notice--warning}

Beachte, was `Statusleiste` **nicht** tut: Sie berechnet keine Lebenspunkte, sie beendet kein Spiel, sie kennt das `Spielfeld` gar nicht. Sie bekommt einen `Spieler` und zeigt ihn an. Genau diese Bescheidenheit macht eine Komponente wiederverwendbar – dieselbe Leiste könnte in einer Übersicht über mehrere Helden mehrfach vorkommen.
{: .notice--primary}

Die Property `Herzen` zeigt nebenbei, was in eine Komponente gehören darf: eine reine **Darstellungsfrage**. Dass drei Lebenspunkte als `♥♥♥` und ein verlorener als `♡` erscheinen, ist keine Spielregel – die Konsolenversion schreibt an derselben Stelle schlicht `3/3`. Wie viele Lebenspunkte ein Treffer kostet, steht dagegen im Kern und hat in der Komponente nichts verloren.

Komponenten machen sich als Tags nur bemerkbar, wenn ihr Namespace bekannt ist. Die Datei `_Imports.razor` enthält dafür `@using`-Zeilen, die für alle Razor-Dateien gelten. Verschiebt man eine Komponente in einen neuen Unterordner, ändert sich ihr Namespace, und der Compiler meldet, das Tag sei unbekannt – dann fehlt eine `@using`-Zeile.
{: .notice--warning}

Übung: Zerlege die Werkzeugleiste in eine eigene Komponente `Levelauswahl.razor`. Sie bekommt die Liste der Levelnamen als Parameter und meldet die Auswahl über einen Rückkanal an die Startseite. Überlege zuerst auf Papier: Welche Parameter braucht sie, und welchen Typ muss der Rückkanal haben? Den passenden Typ dafür lernst du in [Dialoge als Komponenten](/modules/blazor_dialoge/blazor_dialoge.md) kennen. Eine ausgearbeitete Lösung steht als Aufgabe 1 in [Aufgaben und Beispiele](/modules/aufgaben_gui_schichten/aufgaben_gui_schichten.md) – schau erst hinein, wenn dein eigener Entwurf steht.
{: .notice--info}

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v04-blazor`).

## Weitere Quellen

- [Razor-Syntaxreferenz für ASP.NET Core – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/mvc/views/razor)
- [Razor-Komponenten in ASP.NET Core – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/)
- [Attribute und Parameter von Komponenten – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/#component-parameters)
- [Mustervergleich mit `switch`-Ausdrücken – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/operators/patterns)
- [HTML-Elementreferenz – MDN Web Docs](https://developer.mozilla.org/de/docs/Web/HTML/Reference/Elements) – das vollständige Sortiment an „Steuerelementen“, aus dem du dir in Blazor bedienst.
- [Mustervergleich üben – SharpLab](https://sharplab.io/) – zeigt dir, in welchen C#-Code der Compiler einen `switch`-Ausdruck mit Typmustern übersetzt.
