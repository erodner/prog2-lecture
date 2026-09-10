---
title: "Die erste Blazor-App"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Bisher begann jedes unserer Programme mit `Console.WriteLine`. Heute beginnt es mit einer Seite im Browser – und trotzdem schreiben wir ausschließlich C#. Damit das gelingt, müssen ein paar Dinge zusammenspielen, die uns bei Konsolenprogrammen erspart blieben: ein Webserver, der auf Anfragen wartet, ein HTML-Grundgerüst, das der Browser anzeigt, und eine Verbindung zwischen beiden, über die Klicks in die eine und neue Seiteninhalte in die andere Richtung wandern. Das klingt nach viel, aber die Projektvorlage von Blazor legt fast alles davon für uns an. In diesem Modul erzeugen wir das Projekt `HalloBlazor`, sehen uns die erzeugten Dateien an und verstehen dann Zeile für Zeile, wie eine Seite mit Eingabefeld, Button und Ausgabe funktioniert.

## Projekt anlegen und starten

Wie in [dotnet-CLI und Projekte](/modules/dotnet_cli_projekte/dotnet_cli_projekte.md) erzeugen wir das Projekt aus einer Vorlage. Die Vorlage heißt `blazor`, und drei Optionen legen fest, was wir bekommen:

```bash
dotnet new blazor -o HalloBlazor --empty -int Server -ai
cd HalloBlazor
dotnet run
```

`--empty` lässt die Beispielseiten und das CSS-Framework Bootstrap weg, `-int Server` wählt den Render-Modus **Interactive Server** aus [GUI-Grundbegriffe](/modules/gui_grundbegriffe/gui_grundbegriffe.md), und `-ai` schaltet die Interaktivität für die gesamte App ein, nicht nur für einzelne Seiten. `dotnet run` startet den Webserver und gibt in der Konsole eine Adresse wie `http://localhost:5123` aus – diese öffnest du im Browser. Praktischer ist während der Entwicklung `dotnet watch`: Es startet die App ebenfalls, beobachtet aber zusätzlich alle Dateien und überträgt Änderungen per **Hot Reload** direkt in die laufende Seite, meist ohne Neustart.

Interactive Server heißt: Dein C#-Code läuft auf dem Server, also in dem Prozess, den `dotnet run` gestartet hat – nicht im Browser. Der Browser zeigt nur HTML an und meldet Ereignisse. Beendest du das Konsolenprogramm, bleibt die Seite zwar sichtbar, reagiert aber auf nichts mehr.
{: .notice--primary}

## Der Aufbau des Projekts

Die Vorlage erzeugt mehr Dateien als eine Konsolen-App, aber nur eine Handvoll ist für uns wichtig:

| Datei | Aufgabe |
| :--- | :--- |
| `Program.cs` | Startpunkt: Webserver konfigurieren und starten |
| `Components/App.razor` | HTML-Grundgerüst der Seite (`<html>`, `<head>`, `<body>`) |
| `Components/Routes.razor` | Ordnet Adressen (`/`, `/about`) den Seiten zu |
| `Components/Layout/MainLayout.razor` | Rahmen, der um jede Seite gelegt wird |
| `Components/Pages/Home.razor` | Unsere Startseite – hier arbeiten wir |
| `Components/_Imports.razor` | `@using`-Zeilen, die für alle Razor-Dateien gelten |
| `wwwroot/app.css` | Stylesheet der App |

Der Reihe nach: `Program.cs` ist eine Top-Level-Statements-Datei wie bei einer Konsolen-App, nur dass sie keinen Ablauf beschreibt, sondern einen Server aufsetzt:

```csharp
using HalloBlazor.Components;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();
// ... (Fehlerseiten, HTTPS, statische Dateien)
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

`WebApplication.CreateBuilder` bereitet die Anwendung vor, `AddRazorComponents().AddInteractiveServerComponents()` schaltet Razor-Komponenten und den Server-Render-Modus ein, und `MapRazorComponents<App>()` sagt: Die Wurzel der Oberfläche ist die Komponente `App`. Die letzte Zeile `app.Run()` kehrt erst zurück, wenn der Server beendet wird – das ist die **Ereignisschleife** aus den GUI-Grundbegriffen. Der Server wartet auf Anfragen und Ereignisse aus dem Browser und ruft unsere Handler auf.

`App.razor` liefert das HTML-Grundgerüst, das der Browser beim ersten Aufruf bekommt. Zwei Zeilen darin sind entscheidend:

```razor
<Routes @rendermode="InteractiveServer" />
<script src="@Assets["_framework/blazor.web.js"]"></script>
```

Das Skript `blazor.web.js` ist das einzige JavaScript, das wir je brauchen – es baut die SignalR-Verbindung zum Server auf und tauscht geänderte Seitenteile aus. `@rendermode="InteractiveServer"` legt fest, dass alles innerhalb von `<Routes>` interaktiv auf dem Server läuft. `Routes.razor` enthält den `<Router>`, der anhand der Adresse die passende Seite auswählt und sie in das Layout `MainLayout` einbettet. Dieses ist in der leeren Vorlage fast leer – die Zeile `@Body` markiert die Stelle, an der die jeweilige Seite eingesetzt wird. Was man aus dem Layout machen kann, sehen wir in [Layout mit HTML und CSS](/modules/blazor_layout/blazor_layout.md).

## Eine Razor-Datei ist eine Klasse

Damit sind wir bei `Home.razor`, der einzigen Datei, die wir in diesem Modul selbst verändern. Eine `.razor`-Datei besteht aus zwei Teilen: oben **Markup** (HTML mit eingestreuten `@`-Ausdrücken), unten ein `@code`-Block mit gewöhnlichem C#. Beim Kompilieren wird aus jeder Razor-Datei **eine C#-Klasse** – bei `Home.razor` die Klasse `Home`. Das Markup wird zu einer Methode, die das HTML erzeugt, der `@code`-Block liefert Felder und Methoden dieser Klasse.

Ältere Frameworks haben Aussehen und Verhalten auf zwei Dateien verteilt: eine vom Designer erzeugte Datei mit den Steuerelementen und eine Code-Behind-Datei mit den Handlern. Razor legt beides in eine Datei, hält es aber genauso sauber getrennt: Das Markup beschreibt, *was* zu sehen ist, der `@code`-Block, *was passiert*. Hier die vollständige Startseite von `HalloBlazor`:

```razor
@page "/"

<PageTitle>Hallo Blazor</PageTitle>

<h1>Hallo Blazor</h1>

<p>Wie heißt du?</p>
<input @bind="name" placeholder="Name eingeben" />
<button @onclick="Begruessen">Begrüßen</button>

<p class="ausgabe">@ausgabe</p>

@code {
    private string name = "";
    private string ausgabe = "";
    private int anzahlKlicks = 0;

    private void Begruessen()
    {
        anzahlKlicks++;
        string wer = string.IsNullOrWhiteSpace(name) ? "Unbekannte:r" : name;
        ausgabe = $"Hallo, {wer}! (Klick Nr. {anzahlKlicks})";
    }
}
```

Gehen wir die Datei von oben nach unten durch. `@page "/"` macht aus der Komponente eine **Seite**, die unter der Wurzeladresse erreichbar ist – ohne diese Zeile wäre `Home` nur ein Baustein, den andere Seiten einbetten könnten. `<PageTitle>` setzt den Text in der Browser-Registerkarte. `<h1>` und `<p>` sind gewöhnliches HTML; das kann der Browser ohne unser Zutun anzeigen.

Interessant wird es bei `<input @bind="name" />`. Das Attribut `@bind` verbindet das Eingabefeld mit dem Feld `name` aus dem `@code`-Block in beide Richtungen: Der Anfangswert des Feldes erscheint im Textfeld, und sobald der Benutzer das Feld verlässt, landet der eingegebene Text in `name`. `<button @onclick="Begruessen">` registriert die Methode `Begruessen` als Ereignisbehandler für den Klick – wir schreiben die Methode, aufgerufen wird sie von Blazor. Und `@ausgabe` in der letzten Markup-Zeile fügt den aktuellen Wert des Feldes `ausgabe` als Text in den Absatz ein.

Im `@code`-Block stehen drei private Felder und eine Methode – ganz gewöhnlicher C#-Code, so wie wir ihn seit [Klassen und Objekten](https://www.erodner.de/prog-lecture/modules/klassen/klassen/) schreiben. `Begruessen` zählt den Klick, prüft mit `string.IsNullOrWhiteSpace`, ob überhaupt ein Name eingegeben wurde, und setzt `ausgabe` neu. Auffällig ist, was fehlt: Nirgends steht „schreibe `ausgabe` jetzt in den Absatz“. Das erledigt Blazor.

## Was beim Klick passiert

Verfolgen wir einen Klick vom Anfang bis zum Ende. Der Benutzer tippt „Anna“ in das Textfeld und klickt auf „Begrüßen“. Der Browser meldet über die SignalR-Verbindung an den Server: „Feld verlassen, neuer Text ‚Anna‘“ und dann „Button geklickt“. Auf dem Server setzt Blazor daraufhin `name = "Anna"` und ruft `Begruessen()` auf. Nach dem Handler rendert Blazor die Komponente neu, das heißt, es erzeugt das HTML aus dem Markup noch einmal – diesmal mit dem neuen Wert von `@ausgabe` – und vergleicht es mit dem vorherigen Stand. Nur die Unterschiede werden an den Browser geschickt, der sie in die angezeigte Seite einbaut. Für den Benutzer sieht es aus, als hätte sich der Absatz „von selbst“ geändert.

Dieser Zyklus – Ereignis, Handler, neu rendern, Unterschiede übertragen – ist das Grundmuster jeder Blazor-Anwendung. Wir schreiben nur den mittleren Schritt. Wie Blazor entscheidet, wann neu gerendert wird, und was `@bind` dabei genau tut, sehen wir uns in [Datenbindung und Render-Zyklus](/modules/blazor_datenbindung/blazor_datenbindung.md) genauer an.

Hot Reload hat Grenzen: Änderungen am Markup und an Methodenrümpfen übernimmt `dotnet watch` sofort, aber neue Felder, geänderte Signaturen oder Änderungen in `Program.cs` erfordern einen Neustart. Wenn sich die Seite trotz Speichern nicht ändert, hilft in der Konsole von `dotnet watch` die Taste `Strg+R` für einen vollständigen Neustart.
{: .notice--warning}

Übung: Erweitere `Home.razor` um ein zweites Eingabefeld für das Alter und lasse `Begruessen` ausgeben, in welchem Jahr die Person 100 wird. Ändere anschließend den Methodennamen im `@onclick`-Attribut absichtlich falsch und beobachte, wo und wann der Fehler auftaucht.
{: .notice--info}

Das vollständige Projekt findest du im Repository unter `examples/03_blazor/HalloBlazor`.

## Weitere Quellen

- [Blazor-Tooling und Projektvorlagen – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/tooling)
- [Projektstruktur einer Blazor-App – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/project-structure)
- [Razor-Komponenten – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/)
- [Hot Reload mit `dotnet watch` – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/test/hot-reload)
