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

Stell dir vor, du baust ins Adventure eine neue Falle ein: ein statisches Objekt, das beim Betreten einen Lebenspunkt kostet. Du fasst dafür `Spielfeld.SpielerZieht` an, weil die Falle beim Hineinlaufen reagieren muss – und dieselbe Methode entscheidet auch, ob vor einer verschlossenen Tür interagiert oder gegangen wird. Am Abend steht die Frage im Raum: **Habe ich mit der neuen Falle die Tür kaputt gemacht?** Um das von Hand zu beantworten, müsstest du das Spiel starten, den Schlüssel holen, zur Tür laufen, aufschließen, weiter zur Truhe, zum Ausgang – und das für jedes Level und jeden Sonderfall, bei jeder Änderung. Das macht niemand zwanzig Mal. Unit-Tests machen aus diesem Durchspielen ein Programm, das der Rechner in Millisekunden erledigt.

## Das Problem: Regressionen

Ein Fehler, der durch eine Änderung an anderer Stelle *neu* entsteht, heißt **Regression** – etwas, das schon funktioniert hat, geht wieder kaputt. Je größer ein Programm wird, desto mehr Stellen hängen direkt oder indirekt voneinander ab. Im Adventure genügt eine einzige Zeile in `SpielerZieht`, um Tür, Truhe, Trank, Ausgang und die Gegnerzüge gleichzeitig zu beeinflussen, denn alle laufen durch dieselbe Runde. Manuelles Testen skaliert dagegen nicht: Der Aufwand pro Änderung wächst mit dem Programm, und Menschen prüfen nach dem zehnten Durchlauf nicht mehr alle Fälle gleich sorgfältig – gerade die langweiligen nicht, in denen die Fehler stecken.

## Die Lösung: Code, der Code testet

Ein **Unit-Test** ist nichts anderes als Programmcode, der ein kleines Stück anderen Programmcode (eine *Unit* – meist eine Methode oder Klasse) aufruft und prüft, ob das Ergebnis dem entspricht, was wir erwarten. Genau die Tür-Frage von oben steht so im Testprojekt des Spiels:

```csharp
[Test] public void Tuer_Ohne_Schluessel_Bleibt_Zu()
{
    Spielfeld f = Feld("@D");
    f.SpielerZieht(Richtung.Rechts);
    Assert.That(f.LetzteMeldung, Does.Contain("Schlüssel"));
    Assert.That(f.Spieler.Position, Is.EqualTo(new Position(0, 0)));
}
```

`Feld("@D")` baut aus einer Zeichenkette ein winziges Level – Held links, Tür rechts daneben. Der Test zieht einmal nach rechts und behauptet zweierlei: Die Meldung erwähnt den fehlenden Schlüssel, und der Held steht noch da, wo er war. Drei Eigenschaften machen daraus mehr als ein Probeprogramm: Der Test ist **klein** (eine Methode, ein Szenario), **automatisiert** (ein Kommando führt alle Tests aus) und **wiederholbar** (nach jeder Änderung, ohne dass jemand eine Taste drückt). Baust du die Falle ein und drehst dabei versehentlich die Bedingung in `SpielerZieht` um, wird dieser Test rot, bevor du das Spiel überhaupt gestartet hast.

Unit-Tests beweisen nicht, dass ein Programm fehlerfrei ist – sie zeigen nur, dass die geprüften Fälle funktionieren. Aber jeder Fall, den ein Test abdeckt, muss nie wieder von Hand durchgespielt werden. Und jeder Fehler, den du findest, bekommt einen Test, damit er nicht zurückkommt.
{: .notice--primary}

## Was einen guten Test ausmacht

Nicht jeder Test hilft. Ein guter Unit-Test ist:

- **schnell** – Millisekunden, nicht Sekunden. Wer auf Tests warten muss, führt sie seltener aus.
- **unabhängig** – jeder Test baut sich sein Spielfeld selbst auf; die Reihenfolge der Tests spielt keine Rolle.
- **deterministisch** – dasselbe Ergebnis bei jedem Lauf. Kein Zufall, keine aktuelle Uhrzeit, kein Netzwerk.
- **auf eine Sache konzentriert** – ein Test prüft ein Verhalten. Schlägt er fehl, weiß man sofort, was kaputt ist.

Der letzte Punkt zeigt sich schon im Namen: `Tuer_Ohne_Schluessel_Bleibt_Zu` sagt, welches Spielobjekt unter welcher Bedingung wie reagieren soll. Ein Test namens `TestAlles` sagt nichts – und wenn er rot wird, weißt du nur, dass *irgendetwas* nicht stimmt.

## Die Testpyramide

Unit-Tests sind nicht die einzige Testart, aber die Basis. Die **Testpyramide** ordnet die Arten nach Anzahl und Aufwand:

```
        ╱ E2E ╲          wenige: ganzes Spiel über den Browser bedienen
      ╱─────────╲        langsam, brüchig, findet Integrationsfehler
    ╱ Integration ╲      einige: JsonSpielstandSpeicher schreibt und liest
  ╱─────────────────╲    eine echte Datei; TextdateiLevelQuelle liest einen Ordner
╱     Unit-Tests      ╲  viele: Tuer.Interagieren, Verfolger.NaechsterZug,
───────────────────────  Inventar.Enthaelt – isoliert, in Millisekunden
```

Ganz unten stehen viele schnelle Unit-Tests, die einzelne Klassen isoliert prüfen – im Adventure etwa, ob eine `Wache` am Hindernis umdreht. Darüber liegen weniger **Integrationstests**, die das Zusammenspiel mehrerer Teile prüfen: dass ein Spielstand als JSON auf die Platte geschrieben und daraus dasselbe Spielfeld wiederhergestellt wird, ist erst geprüft, wenn eine echte Datei im Spiel war. Ganz oben stehen wenige **End-to-End-Tests**, die die fertige Anwendung über die Oberfläche bedienen – bei uns also einen Browser starten, Pfeiltasten schicken und das gezeichnete Raster ansehen. Sie sind langsam und gehen bei jeder Layout-Änderung kaputt, deshalb sichert man möglichst viel schon weiter unten ab.

## Testbarkeit durch Schichten

Wie testet man eigentlich ein Spiel, ohne es zu spielen? Die Antwort haben wir in der [Schichten-Architektur](/modules/schichten_architektur/schichten_architektur.md) vorbereitet. `Adventure.Kern` enthält die kompletten Spielregeln und weiß nichts von Konsole und Browser: keine `Console.WriteLine`, kein `@onkeydown`, keine CSS-Klassen. Deshalb lässt sich ein ganzes Spiel im Test in einer Zeile aufbauen und in weiteren drei durchspielen:

```csharp
Spielfeld f = Feld("@kD.E");
f.SpielerZieht(Richtung.Rechts);   // Schlüssel aufheben
f.SpielerZieht(Richtung.Rechts);   // Tür aufschließen
Assert.That(f.Spieler.Inventar.Enthaelt<Schluessel>(), Is.False);
```

Hätten wir die Regel „vor einer verschlossenen Tür wird interagiert statt gegangen“ im Tastatur-Handler von `Home.razor` untergebracht, gäbe es diesen Test nicht – man käme an die Logik nur über einen laufenden Browser heran.

Die zweite Vorbereitung war die Schnittstelle `ILevelQuelle`. Der Kern schreibt nur vor, *was* er von einer Levelquelle braucht (`LevelNamen` und `Laden`); woher die Karten kommen, ist ihm egal. In der Web-App steckt die Dependency Injection eine Quelle hinein, die Dateien oder HTTP benutzt – im Test nimmt man die einfachste Implementierung, die es gibt:

```csharp
[Test] public void Eingebaute_Level_Parsen()
{
    EingebauteLevelQuelle q = new();
    foreach (string n in q.LevelNamen)
    {
        Spielfeld f = LevelParser.Parsen(q.Laden(n));
        Assert.That(f.AlsText().TrimEnd().Split('\n'), Has.Length.EqualTo(q.Laden(n).Zeilen.Count));
        Assert.That(f.AlsText(), Does.Contain("@"));
    }
}
```

`EingebauteLevelQuelle` hat ihre beiden Karten fest im Code stehen und ist damit ein **Test-Double**: ein Ersatzobjekt, das die Schnittstelle erfüllt, aber ohne Datei, Netzwerk oder Datenbank auskommt. Der Test bleibt dadurch schnell und deterministisch – er läuft auch im Zug ohne Empfang. Testbarkeit ist damit kein Zufall, sondern ein direktes Ergebnis guter Architektur: Was wir in Vorlesung 04 aus Gründen der Ordnung getrennt haben, zahlt sich heute als Prüfbarkeit aus.

Wenn eine Klasse sich schlecht testen lässt, liegt das selten am Test-Framework. Meist steckt Logik dort, wo sie nicht hingehört: im `@code`-Block einer Razor-Seite, in einer `static`-Methode, die direkt eine Datei liest, oder in einer Klasse, die sich ihre Abhängigkeiten selbst mit `new` erzeugt, statt sie im Konstruktor übergeben zu bekommen.
{: .notice--warning}

## Tests als Dokumentation

Ein Nebeneffekt, den man leicht unterschätzt: Tests beschreiben, wie eine Klasse benutzt werden soll – und im Gegensatz zu Kommentaren veralten sie nicht unbemerkt, weil sie bei jeder Abweichung fehlschlagen. Wer wissen will, ob der Schlüssel beim Aufschließen verbraucht wird, liest `Schluessel_Aufheben_Und_Tuer_Oeffnen` und sieht dort `Assert.That(f.Spieler.Inventar.Anzahl, Is.EqualTo(0))`. Wer wissen will, wie weit ein `Verfolger` sieht, findet `Verfolger_Sieht_Nicht_Durch_Waende`. Eine gut benannte Testklasse ist die ehrlichste Spezifikation der Spielregeln, die es gibt – ehrlicher als jedes Regelheft, weil sie sich beim Lügen selbst verrät.

## Test-Driven Development in Kürze

Manche Teams schreiben den Test sogar *vor* dem Code. Bei **Test-Driven Development (TDD)** läuft ein kurzer Zyklus: erst einen Test schreiben, der fehlschlägt (rot), dann gerade so viel Code, dass er besteht (grün), dann aufräumen, ohne dass ein Test rot wird (refactor). Für unsere Falle hieße das: zuerst `Feld("@X")` und die Erwartung „Lebenspunkte sinken um 1“ hinschreiben, dann die Klasse `Falle` bauen. Der rote Schritt ist dabei kein Umweg, sondern der Beweis, dass der Test überhaupt etwas prüft – ein Test, der nie rot war, könnte auch immer grün sein, weil er nichts aussagt. Du musst nicht dogmatisch nach TDD arbeiten; aber die Gewohnheit, jede neue Spielregel mit einem Test zu beginnen, sorgt fast automatisch für testbaren Code.

Übung: Schau dir den Tastatur-Handler im `@code`-Block von `Components/Pages/Home.razor` in `Adventure.Web` an. Welche Zeilen darin ließen sich ohne Browser testen, welche nicht – und woran liegt das? Formuliere für eine der nicht testbaren Zeilen, in welche Klasse des Kerns ihre Logik gehören würde.
{: .notice--info}

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v12-tests`), die Tests aus diesem Modul im Projekt `Adventure.Tests`.

## Weitere Quellen

- [Komponententests in .NET – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/testing/)
- [Bewährte Methoden für Komponententests – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/testing/unit-testing-best-practices)
- [NUnit-Dokumentation](https://docs.nunit.org/)
