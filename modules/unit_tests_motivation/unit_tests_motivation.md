---
title: "Warum Unit-Tests?"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Kennst du das? Du baust an einer kleinen Stelle etwas um – eine Methode bekommt einen zusätzlichen Parameter, eine Bedingung wird verschärft – und plötzlich funktioniert an ganz anderer Stelle die Hälfte des Programms nicht mehr. Niemand weiß sofort, warum. Also wird wieder alles von Hand durchprobiert: jeder Button, jede Eingabe, jeder Sonderfall. Das kostet Zeit, ist langweilig und wird deshalb gern weggelassen – bis der Fehler beim Kunden auffällt. Unit-Tests lösen genau dieses Problem: Sie machen aus dem mühsamen Durchklicken ein Programm, das der Rechner in Sekunden ausführt.

## Das Problem: Regressionen

Ein Fehler, der durch eine Änderung an anderer Stelle *neu* entsteht, heißt **Regression** – etwas, das schon funktioniert hat, geht wieder kaputt. Je größer ein Programm wird, desto mehr Stellen hängen direkt oder indirekt voneinander ab, und desto wahrscheinlicher sind solche unerwünschten Nebenwirkungen. Manuelles Testen skaliert damit nicht: Der Aufwand pro Änderung wächst mit dem Programm, und Menschen prüfen nach dem zehnten Mal nicht mehr alle Fälle gleich sorgfältig.

## Die Lösung: Code, der Code testet

Ein **Unit-Test** ist nichts anderes als Programmcode, der ein kleines Stück anderen Programmcode (eine *Unit* – meist eine Methode oder Klasse) aufruft und prüft, ob das Ergebnis dem entspricht, was wir erwarten. So sieht das mit der Klasse `Bruch` aus dem Beispielprojekt aus:

```csharp
[Test]
public void Plus_EinHalbPlusEinDrittel_ErgibtFuenfSechstel()
{
    Bruch a = new Bruch(1, 2);
    Bruch b = new Bruch(1, 3);

    Bruch summe = a.Plus(b);

    Assert.That(summe, Is.EqualTo(new Bruch(5, 6)));
}
```

Der Test legt zwei Brüche an, ruft `Plus` auf und behauptet, dass fünf Sechstel herauskommen. Stimmt das nicht, schlägt der Test fehl und nennt Erwartung und tatsächlichen Wert. Drei Eigenschaften machen daraus mehr als ein Probeprogramm: Der Test ist **klein** (eine Methode, ein Szenario), **automatisiert** (ein Kommando führt alle Tests aus) und **wiederholbar** (nach jeder Änderung, ohne dass jemand tippen muss). Schlägt nach einem Umbau ein Test fehl, hast du die Regression gefunden, bevor sie irgendwem auffällt.

Unit-Tests beweisen nicht, dass ein Programm fehlerfrei ist – sie zeigen nur, dass die geprüften Fälle funktionieren. Aber jeder Fall, den ein Test abdeckt, muss nie wieder von Hand geprüft werden. Und jeder Fehler, den du findest, bekommt einen Test, damit er nicht zurückkommt.
{: .notice--primary}

## Die Testpyramide

Unit-Tests sind nicht die einzige Testart, aber die Basis. Ganz unten in der **Testpyramide** stehen viele schnelle Unit-Tests, die einzelne Klassen isoliert prüfen. Darüber liegen weniger Integrationstests, die das Zusammenspiel mehrerer Teile testen – etwa ob `JsonFigurSpeicher` eine Datei wirklich schreibt und wieder lesen kann. Ganz oben stehen wenige End-to-End-Tests, die das ganze Programm über die Oberfläche bedienen; sie sind langsam und brüchig, deshalb sollte man möglichst viel schon weiter unten absichern.

## Was einen guten Test ausmacht

Nicht jeder Test hilft. Ein guter Unit-Test ist:

- **schnell** – Millisekunden, nicht Sekunden. Wer auf Tests warten muss, führt sie seltener aus.
- **unabhängig** – jeder Test baut sich seine Daten selbst auf; die Reihenfolge der Tests spielt keine Rolle.
- **deterministisch** – dasselbe Ergebnis bei jedem Lauf. Kein Zufall, keine aktuelle Uhrzeit, kein Netzwerk.
- **auf eine Sache konzentriert** – ein Test prüft ein Verhalten. Schlägt er fehl, weiß man sofort, was kaputt ist.

Der letzte Punkt zeigt sich schon im Namen: `Plus_EinHalbPlusEinDrittel_ErgibtFuenfSechstel` sagt, welche Methode unter welchen Bedingungen was liefern soll. Ein Test namens `TestAlles` sagt nichts.

## Testbarkeit durch Schichten

Wie testet man eigentlich die Logik eines GUI-Programms, ohne die Oberfläche zu starten? Die Antwort haben wir in der [Schichten-Architektur](/modules/schichten_architektur/schichten_architektur.md) vorbereitet: Die `FigurenVerwaltung` liegt im Fachkonzept, weiß nichts von Fenstern und kennt ihre Datenhaltung nur über die Schnittstelle `IFigurSpeicher`. Im Test steckt man ihr einfach den einfachsten Speicher hinein, den es gibt:

```csharp
[SetUp]
public void Vorbereiten()
{
    // Für Tests reicht der Speicher im Arbeitsspeicher – keine Datei, keine GUI.
    verwaltung = new FigurenVerwaltung(new ArbeitsspeicherFigurSpeicher());
}

[Test]
public void GesamtFlaeche_RechteckUndKreis_SummiertFlaechen()
{
    verwaltung.Hinzufuegen(new Rechteck("r", 0, 0, 2, 3));   // 6
    verwaltung.Hinzufuegen(new Kreis("k", 0, 0, 1));         // pi

    Assert.That(verwaltung.GesamtFlaeche(), Is.EqualTo(6 + Math.PI).Within(1e-9));
}
```

`ArbeitsspeicherFigurSpeicher` ist hier ein **Test-Double**: ein Ersatzobjekt, das die Schnittstelle erfüllt, aber ohne Datei, Netzwerk oder Datenbank auskommt. Der Test bleibt dadurch schnell und deterministisch – und die Fachlogik wird unabhängig von der Datenhaltung geprüft. Hätten wir die Flächensumme im Klick-Handler des Fensters berechnet, gäbe es keine Möglichkeit, sie so zu testen. Testbarkeit ist damit kein Zufall, sondern ein direktes Ergebnis guter Architektur.

Wenn eine Klasse sich schlecht testen lässt, liegt das selten am Test-Framework. Meist steckt Logik dort, wo sie nicht hingehört: im `@code`-Block einer Razor-Seite, in einem `static`-Helfer, der direkt auf eine Datei zugreift, oder in einer Klasse, die sich ihre Abhängigkeiten selbst mit `new` erzeugt statt sie übergeben zu bekommen.
{: .notice--warning}

## Tests als Dokumentation

Ein Nebeneffekt, den man leicht unterschätzt: Tests beschreiben, wie eine Klasse benutzt werden soll – und im Gegensatz zu Kommentaren veralten sie nicht unbemerkt, weil sie bei jeder Abweichung fehlschlagen. Wer wissen will, was `new Bruch(6, 8)` tut, liest den Test `Konstruktor_KuerztDenBruch` und weiß es. Wer wissen will, was bei einem Nenner von 0 passiert, findet `Konstruktor_NennerNull_WirftArgumentException`. Eine gut benannte Testklasse ist die ehrlichste Spezifikation, die es gibt.

## Test-Driven Development in Kürze

Manche Teams schreiben den Test sogar *vor* dem Code. Bei **Test-Driven Development (TDD)** läuft ein kurzer Zyklus: erst einen Test schreiben, der fehlschlägt (rot), dann gerade so viel Code, dass er besteht (grün), dann den Code aufräumen, ohne dass ein Test rot wird (refactor). Der rote Schritt ist dabei kein Umweg, sondern der Beweis, dass der Test überhaupt etwas prüft – ein Test, der nie rot war, könnte auch immer grün sein, weil er nichts aussagt. Du musst nicht dogmatisch nach TDD arbeiten; aber die Gewohnheit, jede neue Funktion mit einem Test zu beginnen, sorgt fast automatisch für testbaren Code.

Übung: Schau dir den Ereignisbehandler `DialogGeschlossen` im `@code`-Block von `Components/Pages/Home.razor` des Geometrieeditors an (`examples/04_blazor/Geometrieeditor/Geometrieeditor.Web`). Welche Zeilen darin ließen sich testen, welche nicht – und woran liegt das?
{: .notice--info}

## Weitere Quellen

- [Komponententests in .NET – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/testing/)
- [Bewährte Methoden für Komponententests – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/testing/unit-testing-best-practices)
- [NUnit-Dokumentation](https://docs.nunit.org/)
