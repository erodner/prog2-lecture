---
title: "GUI-Grundbegriffe und Framework-Landschaft"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Unser Adventure lässt sich seit [Vorlesung 02](/lectures/02/02.md) spielen – in der Konsole. Die Schleife in `Adventure.Konsole` ist dabei ein Musterbeispiel für die Art, wie wir bisher programmiert haben: Das Programm zeichnet die Karte, wartet mit `Console.ReadKey` auf eine Taste, rechnet eine Runde und beginnt von vorn. Das Programm bestimmt die Reihenfolge, der Benutzer folgt. Bei einer grafischen Oberfläche ist es genau umgekehrt: Der Benutzer entscheidet, ob er eine Pfeiltaste drückt, ein anderes Level aus einer Liste wählt oder auf „Neu starten“ klickt – und das Programm muss auf alles reagieren können, in beliebiger Reihenfolge. Dieser Perspektivwechsel ist die eigentliche Neuerung, nicht die bunten Schaltflächen. Bevor wir Blazor anfassen, klären wir deshalb die Begriffe, die jedes GUI-Framework gemeinsam hat – egal ob es Fenster auf dem Desktop oder Seiten im Browser zeichnet.

## Steuerelemente und Ereignisse

**GUI** steht für *Graphical User Interface*: Der Benutzer interagiert grafisch mit der Anwendung, per Maus, Tastatur oder Touch. Eine GUI setzt sich aus **Steuerelementen** zusammen (englisch *Controls* oder *Widgets*). Sie haben zwei Aufgaben: Informationen anzeigen – ein Beschriftungstext, eine Lebenspunkteanzeige, eine Liste – und Interaktion ermöglichen – ein Button, ein Textfeld, eine Auswahlliste. Steuerelemente liegen in einem Container: auf dem Desktop ist das ein **Fenster**, im Browser eine **Seite**.

Wenn der Benutzer etwas tut, entsteht ein **Ereignis** (*Event*): ein Klick, ein Tastendruck, eine geänderte Auswahl. Unser Code reagiert darauf mit einem **Ereignisbehandler** (*Event Handler*) – einer Methode, die wir schreiben, die aber nicht wir aufrufen, sondern das Framework, sobald das Ereignis eintritt.

## Die Ereignisschleife – das Programm wartet

Wer ruft unsere Handler-Methoden auf? Nach dem Start tritt jedes GUI-Programm in eine Schleife ein, die **Ereignisschleife** (*Event Loop* oder *Message Loop*). Stark vereinfacht sieht sie so aus:

```csharp
// So arbeitet jedes GUI-Framework im Kern – diesen Code schreibt das Framework, nicht wir
while (anwendungLaeuft)
{
    Ereignis ereignis = WarteAufNaechstesEreignis();   // Klick, Taste, Auswahl geändert ...
    Steuerelement ziel = FindeSteuerelement(ereignis);  // welcher Button wurde getroffen?
    ziel.LoeseEreignisAus(ereignis);                    // ruft unsere Handler-Methode auf
}
```

Das Programm tut also die meiste Zeit nichts – es wartet. Erst wenn ein Ereignis kommt, wird kurz unser Code ausgeführt, danach wartet die Schleife auf das nächste. Das ist die Kernidee der Oberflächenprogrammierung: **Wir schreiben keinen Ablauf mehr, sondern Reaktionen.**

Daraus folgt eine wichtige Regel: Solange ein Handler läuft, kann die Oberfläche nicht auf weitere Eingaben reagieren. Wer im Klick-Handler zehn Sekunden rechnet, hat für zehn Sekunden eine eingefrorene Oberfläche. Handler müssen deshalb schnell zurückkehren; lange Aufgaben lagert man aus (dazu später mehr bei `async`).
{: .notice--warning}

## Was das für unser Spiel bedeutet

Die Konsolenversion des Adventures ist eine `while`-Schleife, die selbst auf die Taste wartet:

```csharp
while (feld.Status == Spielstatus.Laeuft)
{
    Console.WriteLine(feld.AlsText());
    ConsoleKey taste = Console.ReadKey(true).Key;   // hier steht das Programm still
    // ... Taste in eine Richtung übersetzen ...
    feld.SpielerZieht(r);
}
```

In der Browserversion verschwindet diese Schleife ersatzlos – die Ereignisschleife des Frameworks übernimmt sie. Übrig bleibt eine Methode, die eine gedrückte Taste entgegennimmt und genau eine Runde spielt. Und das ist die gute Nachricht: `feld.SpielerZieht(r)` bleibt in beiden Versionen identisch. Was verschwindet, ist nur das Drumherum – Warten, Zeichnen, Abfragen. Der Spielkern merkt nichts davon, dass er plötzlich aus einem Browser aufgerufen wird. Warum das kein Zufall ist, sondern das Ergebnis bewusster Trennung, klären wir in [Schichten-Architekturen](/modules/schichten_architektur/schichten_architektur.md).

## Desktop-GUI und Web-GUI

Historisch lief eine GUI als eigenes Programm auf dem Rechner des Benutzers und öffnete dort Fenster – eine **Desktop-GUI**. Das Betriebssystem liefert die Ereignisse, das Framework zeichnet die Steuerelemente. Bei einer **Web-GUI** übernimmt der Browser diese Rolle: Er ist das Fenster, die HTML-Elemente `<button>`, `<input>` oder `<select>` sind die Steuerelemente, und die Ereignisse (`click`, `change`, `keydown`) meldet der Browser. Das Modell ist dasselbe – Steuerelemente, Ereignisse, Handler –, nur der Ort, an dem gezeichnet wird, ist ein anderer. Wer die Begriffe einmal verstanden hat, findet sich in beiden Welten zurecht.

## GUI-Frameworks für C#

Für C# gibt es mehrere Frameworks, die dieses Modell umsetzen, sich aber in Alter und Plattform deutlich unterscheiden:

- **Windows Forms** und **WPF** – die klassischen Desktop-Frameworks für Windows. Windows Forms (seit .NET 1.0) kapselt die nativen Windows-Steuerelemente; WPF hat die Beschreibungssprache XAML eingeführt. Beide laufen ausschließlich unter Windows und sind historisch prägend, für neue Projekte aber selten die erste Wahl.
- **.NET MAUI** – Microsofts Framework für Mobil-Apps (Android, iOS) mit Desktop-Unterstützung für Windows und macOS.
- **Avalonia UI** – ein Open-Source-Framework mit XAML, das seine Steuerelemente selbst zeichnet und dadurch auf Windows, macOS und Linux identisch aussieht.
- **Blazor** – Microsofts Framework für **Web-Oberflächen in C#**. Die Oberfläche wird in HTML beschrieben und im Browser angezeigt, das Verhalten aber in C# programmiert – dort, wo sonst JavaScript stehen würde.

Wir verwenden in dieser Vorlesung **Blazor**, aus vier Gründen: Es läuft auf jedem Rechner, auf dem ein Browser läuft – ohne Installation beim Benutzer. Es ist reines C#, wir brauchen keine zweite Sprache. Seine **Komponenten** verhalten sich wie die Steuerelemente eines Desktop-Frameworks: Sie haben Eigenschaften, kapseln Markup und Logik und lassen sich verschachteln. Und Ereignisse funktionieren wie in jedem GUI-Framework – eine Methode reagiert auf einen Klick. Was du hier über Steuerelemente, Ereignisse und die Trennung von Oberfläche und Logik lernst, überträgt sich auf jedes der anderen Frameworks.

## Render-Modi: Wo läuft der C#-Code?

Ein Browser führt von Haus aus kein C# aus. Blazor löst das auf zwei Arten, die man **Render-Modi** nennt. Beim Modus **Interactive Server** läuft unser C#-Code auf dem Webserver – bei uns auf dem eigenen Rechner, gestartet mit `dotnet run`. Der Browser zeigt nur das fertige HTML an und hält eine dauerhafte Verbindung zum Server (per **SignalR**, einer Bibliothek für Echtzeitkommunikation). Drückt der Benutzer eine Pfeiltaste, geht das Ereignis über diese Verbindung zum Server, dort läuft unser Handler und damit `Spielfeld.SpielerZieht`, und der Server schickt die geänderten Stellen der Seite zurück. Beim Modus **Interactive WebAssembly** wird die .NET-Laufzeit dagegen in den Browser geladen und der C#-Code läuft direkt dort; **Auto** kombiniert beides. Wir nutzen durchgehend **Interactive Server**: Er ist am einfachsten einzurichten, braucht keinen großen Download und verhält sich beim Debuggen wie ein gewöhnliches C#-Programm – ein Haltepunkt in `SpielerZieht` hält genauso an wie in der Konsolenversion.

Ein Ereignis in Blazor Server ist also eine Nachricht vom Browser an den Server, und die Ereignisschleife von oben ist die Schleife des Webservers, die auf solche Nachrichten wartet. Das ändert nichts an der Art, wie wir Handler schreiben – aber es erklärt, warum die Seite ohne laufenden Server nur noch anzeigt und nicht mehr reagiert.
{: .notice--primary}

Der Preis des Server-Modus ist die Latenz: Jeder Tastendruck ist eine Netzwerknachricht. Bei einem rundenbasierten Spiel wie unserem fällt das nicht auf – bei einem Actionspiel mit 60 Bildern pro Sekunde wäre WebAssembly die richtige Wahl. Die Wahl des Render-Modus ist also auch eine Entscheidung über die Art der Interaktion.
{: .notice--warning}

Übung: Schreibe auf, welche Steuerelemente die Browserversion des Adventures braucht, damit sie mindestens so viel kann wie die Konsolenversion: Karte anzeigen, Lebenspunkte und Punkte anzeigen, letzte Meldung anzeigen, Zug machen, Level wechseln, neu starten. Ordne jedem Steuerelement das Ereignis zu, auf das es reagiert – und markiere anschließend alles, was dafür im Spielkern geändert werden müsste. Wenn deine Markierung leer bleibt, ist die Trennung gelungen.
{: .notice--info}

## Weitere Quellen

- [ASP.NET Core Blazor – Übersicht – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/)
- [Blazor-Rendermodi – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/render-modes)
- [Was ist .NET MAUI? – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/maui/what-is-maui)
- [Avalonia UI – Dokumentation](https://docs.avaloniaui.net/)
