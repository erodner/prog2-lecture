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

Alle Programme, die wir bisher geschrieben haben, funktionieren nach demselben Muster: `Console.WriteLine` gibt etwas aus, `Console.ReadLine` wartet auf eine Eingabe, dann geht es in der nächsten Zeile weiter. Das Programm bestimmt die Reihenfolge, der Benutzer folgt. Bei einer grafischen Oberfläche ist es genau umgekehrt: Der Benutzer entscheidet, ob er zuerst in ein Textfeld tippt, eine Liste durchscrollt oder einen Button klickt – und das Programm muss auf alles reagieren können. Dieser Perspektivwechsel ist die eigentliche Neuerung, nicht die bunten Schaltflächen. Bevor wir Blazor anfassen, klären wir deshalb die Begriffe, die jedes GUI-Framework gemeinsam hat – egal ob es Fenster auf dem Desktop oder Seiten im Browser zeichnet.

## Steuerelemente und Ereignisse

**GUI** steht für *Graphical User Interface*: Der Benutzer interagiert grafisch mit der Anwendung, per Maus, Tastatur oder Touch. Eine GUI setzt sich aus **Steuerelementen** zusammen (englisch *Controls* oder *Widgets*). Sie haben zwei Aufgaben: Informationen anzeigen – ein Beschriftungstext, eine Fortschrittsanzeige, eine Liste – und Interaktion ermöglichen – ein Button, ein Textfeld, eine Checkbox, eine Auswahlliste. Steuerelemente liegen in einem Container: auf dem Desktop ist das ein **Fenster**, im Browser eine **Seite**.

Wenn der Benutzer etwas tut, entsteht ein **Ereignis** (*Event*): ein Klick, ein Tastendruck, eine geänderte Auswahl. Unser Code reagiert darauf mit einem **Ereignisbehandler** (*Event Handler*) – einer Methode, die wir schreiben, die aber nicht wir aufrufen, sondern das Framework, sobald das Ereignis eintritt.

## Die Ereignisschleife – das Programm wartet

Wer ruft unsere Handler-Methoden auf? Nach dem Start tritt jedes GUI-Programm in eine Schleife ein, die **Ereignisschleife** (*Event Loop* oder *Message Loop*). Stark vereinfacht sieht sie so aus:

```csharp
// So arbeitet jedes GUI-Framework im Kern – diesen Code schreibt das Framework, nicht wir
while (anwendungLaeuft)
{
    Ereignis ereignis = WarteAufNaechstesEreignis();   // Klick, Taste, Eingabe geändert ...
    Steuerelement ziel = FindeSteuerelement(ereignis);  // welcher Button wurde getroffen?
    ziel.LoeseEreignisAus(ereignis);                    // ruft unsere Handler-Methode auf
}
```

Das Programm tut also die meiste Zeit nichts – es wartet. Erst wenn ein Ereignis kommt, wird kurz unser Code ausgeführt, danach wartet die Schleife auf das nächste. Das ist die Kernidee der Oberflächenprogrammierung: **Wir schreiben keinen Ablauf mehr, sondern Reaktionen.**

Daraus folgt eine wichtige Regel: Solange ein Handler läuft, kann die Oberfläche nicht auf weitere Eingaben reagieren. Wer im Klick-Handler zehn Sekunden rechnet, hat für zehn Sekunden eine eingefrorene Oberfläche. Handler müssen deshalb schnell zurückkehren; lange Aufgaben lagert man aus (dazu später mehr bei `async`).
{: .notice--warning}

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

Ein Browser führt von Haus aus kein C# aus. Blazor löst das auf zwei Arten, die man **Render-Modi** nennt. Beim Modus **Interactive Server** läuft unser C#-Code auf dem Webserver – bei uns auf dem eigenen Rechner, gestartet mit `dotnet run`. Der Browser zeigt nur das fertige HTML an und hält eine dauerhafte Verbindung zum Server (per **SignalR**, einer Bibliothek für Echtzeitkommunikation). Klickt der Benutzer, geht das Ereignis über diese Verbindung zum Server, dort läuft unser Handler, und der Server schickt die geänderten Stellen der Seite zurück. Beim Modus **Interactive WebAssembly** wird die .NET-Laufzeit dagegen in den Browser geladen und der C#-Code läuft direkt dort; **Auto** kombiniert beides. Wir nutzen durchgehend **Interactive Server**: Er ist am einfachsten einzurichten, braucht keinen großen Download und verhält sich beim Debuggen wie ein gewöhnliches C#-Programm.

Ein Ereignis in Blazor Server ist also eine Nachricht vom Browser an den Server, und die Ereignisschleife von oben ist die Schleife des Webservers, die auf solche Nachrichten wartet. Das ändert nichts an der Art, wie wir Handler schreiben – aber es erklärt, warum die Seite ohne laufenden Server nur noch anzeigt und nicht mehr reagiert.
{: .notice--primary}

Übung: Nimm das Ratespiel aus Programmierung 1 (Zufallszahl zwischen 1 und 100, Hinweise „zu groß“ / „zu klein“). Skizziere auf Papier, welche Steuerelemente eine Seite dafür braucht, welche Ereignisse auftreten und welcher Teil der Spiellogik völlig unabhängig von der Oberfläche bleibt. Die letzte Frage wird uns in der Schichten-Architektur wieder begegnen.
{: .notice--info}

## Weitere Quellen

- [ASP.NET Core Blazor – Übersicht – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/)
- [Blazor-Rendermodi – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/render-modes)
- [Was ist .NET MAUI? – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/maui/what-is-maui)
