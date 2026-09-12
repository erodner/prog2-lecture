---
title: "🧩 Aufgaben und Beispiele: Abstrakte Klassen und Interfaces"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Programmieren lernt man nicht nur durch Codezeilen tippen – sondern auch durch **Nachdenken**. Die folgenden Aufgaben trainieren *Computational Thinking*: die Fähigkeit, Probleme so zu strukturieren, dass ein Computer sie lösen kann. Bei abstrakten Klassen und Interfaces geht es vor allem um Abstraktion: Welche Gemeinsamkeit ist eine „Ist-ein“-Beziehung, welche nur eine Fähigkeit? Alle Aufgaben spielen im Dungeon aus der Vorlesung – du findest den Code im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v02-interfaces`). Nimm dir für jede Aufgabe Zeit, bevor du die Lösung aufklappst.

## Aufgabe 1 — Abstraktion

Der Dungeon soll eine neue Objektart bekommen: eine **Falle**. Eine Falle liegt fest an ihrem Platz, man kann über sie hinweglaufen (sie blockiert also nicht), und wer auf sie tritt, verliert Lebenspunkte. Ausgelöst wird sie nur einmal.

- An welche Stelle der Hierarchie `Spielobjekt` → `StatischesObjekt`/`BeweglichesObjekt` gehört sie, und welche Mitglieder musst du überschreiben?
- Reichen die vorhandenen Interfaces `IInteragierbar` und `ISammelbar` aus? Was spricht dagegen, `Falle` von `Gegenstand` abzuleiten und `Aufheben` zu missbrauchen?
- Welche Zeile in `Spielfeld.SpielerZieht` muss ergänzt werden – und warum genügt dafür eine, die den Namen `Falle` nicht enthält?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Die Einordnung:** Eine Falle bewegt sich nie, also `StatischesObjekt`. Pflicht ist `Symbol`, weil es abstrakt aus `Spielobjekt` durchgereicht wird. `IstPassierbar` muss auf `true` überschrieben werden, sonst prallt der Spieler davon ab wie von einer Wand.

**Schritt 2 — Warum kein `Gegenstand`:** `Gegenstand` implementiert `ISammelbar`, und `SpielerZieht` behandelt jeden Gegenstand nach demselben Muster: Meldung ausgeben, vom Feld entfernen, ins Inventar legen (außer beim Trank). Eine Falle im Rucksack des Helden ist Unsinn, und sie soll auch nicht verschwinden, sondern sichtbar ausgelöst liegen bleiben. „Draufgetreten“ ist eine eigene Fähigkeit – und die hat bisher kein Interface. `IInteragierbar` passt nicht, weil Interaktion in `SpielerZieht` nur für **nicht passierbare** Objekte ausgelöst wird (siehe Aufgabe 3).

**Schritt 3 — Neues Interface und neue Klasse:**

```csharp
public interface IBetretbar
{
    /// <summary>Wird aufgerufen, sobald ein Spieler das Feld betritt.</summary>
    string Betreten(Spieler spieler);
}

public sealed class Falle : StatischesObjekt, IBetretbar
{
    public int Schaden { get; }
    public bool Ausgeloest { get; private set; }

    public Falle(Position position, int schaden = 1) : base("Falle", position)
    {
        Schaden = schaden;
    }

    // Solange die Falle versteckt ist, sieht sie aus wie normaler Boden.
    public override char Symbol => Ausgeloest ? '^' : '.';
    public override bool IstPassierbar => true;

    public string Betreten(Spieler spieler)
    {
        if (Ausgeloest) return "Die Falle ist schon ausgelöst.";
        Ausgeloest = true;
        spieler.SchadenNehmen(Schaden);
        return $"Eine Falle schnappt zu! (-{Schaden})";
    }
}
```

**Schritt 4 — Die eine Zeile im Spielfeld:**

```csharp
else if (Spieler.Bewegen(richtung, this))
{
    if (davor is IBetretbar betretbar)
    {
        meldung.Append(betretbar.Betreten(Spieler));
    }
    if (davor is Gegenstand gegenstand) { /* ... wie bisher ... */ }
}
```

Damit die Falle auch in Karten vorkommen kann, braucht `LevelParser.ObjektFuer` noch einen Fall `'^' => new Falle(pos)`.

**Zentrale Designentscheidungen:**

- **Klasse für das, was die Falle *ist*, Interface für das, was sie *kann*:** `StatischesObjekt` liefert Name, Position und den Symbolvertrag; `IBetretbar` beschreibt eine Fähigkeit, die später auch ein `Teleporter` oder eine `Feuerstelle` haben darf – ohne gemeinsame Basisklasse.
- **Die Spielregel kennt keine Falle:** `davor is IBetretbar` funktioniert für jede künftige Klasse mit dieser Fähigkeit. Genau dafür sind Interfaces da.
- **Der Schaden erledigt sich von selbst:** `GegnerZiehen` prüft am Ende jeder Runde `Spieler.IstAmLeben` und setzt den `Spielstatus` auf `Verloren`. Die Falle muss davon nichts wissen.
- **Ein Symbol, das lügen darf:** Weil `Symbol` ein Property und keine Konstante ist, kann die Falle ihren Zustand im Zeichen ausdrücken – dasselbe Muster wie bei `Tuer` (`'/'` oder `'D'`) und `Truhe` (`'t'` oder `'T'`).

</details>

## Aufgabe 2 — Mustererkennung

Ein `Zauberbuch` soll sowohl eingesteckt als auch gelesen werden können. Beide Interfaces verlangen zufällig ein Mitglied `Beschreibung()`. Sage die Ausgabe des folgenden Programms voraus, bevor du es ausführst:

```csharp
interface ISammelbar { string Beschreibung(); }
interface IInteragierbar { string Beschreibung(); }

class Zauberbuch : ISammelbar, IInteragierbar
{
    public string Beschreibung() => "Ein altes Buch";
    string ISammelbar.Beschreibung() => "kann eingesteckt werden";
    string IInteragierbar.Beschreibung() => "kann gelesen werden";
}

class Zauberfolio : Zauberbuch
{
    public new string Beschreibung() => "Ein dickes Folio";
}

Zauberbuch b = new Zauberfolio();
Console.WriteLine(b.Beschreibung());
Console.WriteLine(((ISammelbar)b).Beschreibung());
Console.WriteLine(((IInteragierbar)b).Beschreibung());
Zauberfolio f = (Zauberfolio)b;
Console.WriteLine(f.Beschreibung());
ISammelbar s = f;
Console.WriteLine(s.Beschreibung());
Console.WriteLine(b is IInteragierbar);
```

- Welche `Beschreibung` ist über eine `Zauberbuch`-Variable erreichbar, welche nur über einen Interface-Typ?
- Warum ändert `new` in `Zauberfolio` nichts an den Interface-Aufrufen?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Die Ausgabe:**

```
Ein altes Buch
kann eingesteckt werden
kann gelesen werden
Ein dickes Folio
kann eingesteckt werden
True
```

**Schritt 2 — Begründung:**

`b.Beschreibung()` ist ein Aufruf über den Kompilierzeittyp `Zauberbuch`. Dort gibt es eine öffentliche `Beschreibung`, und `Zauberfolio` **versteckt** sie mit `new`, statt sie zu überschreiben – wie in [Laufzeittyp und Verstecken](/modules/laufzeittyp_verstecken/laufzeittyp_verstecken.md) besprochen, zählt bei `new` der deklarierte Typ. Die beiden Casts wählen jeweils die explizite Implementierung des Interfaces aus; die öffentliche Methode spielt dabei keine Rolle, und genau deshalb können zwei gleichnamige Verträge nebeneinander bestehen. Über `f` vom Typ `Zauberfolio` ist die versteckende Methode sichtbar. `s.Beschreibung()` geht wieder über das Interface: `Zauberfolio` implementiert `ISammelbar` nicht neu, also gilt weiterhin die explizite Implementierung aus `Zauberbuch`. `b is IInteragierbar` ist wahr, weil jedes `Zauberfolio` ein `Zauberbuch` ist und `Zauberbuch` das Interface implementiert.

**Zentrale Designentscheidungen:**

- **Explizite Implementierung entkoppelt gleichnamige Interface-Mitglieder:** Zwei Verträge mit `Beschreibung()` können unterschiedlich beantwortet werden, die Klasse behält eine eigene, dritte Version.
- **Interface-Aufrufe sind polymorph, `new` ist es nicht:** Wer `Beschreibung` in `Zauberfolio` für alle Aufrufwege ändern will, muss die Methode in `Zauberbuch` `virtual` machen (dann wirkt `override` auch über den Interface-Typ) oder in `Zauberfolio` die Interfaces erneut implementieren.
- **Im echten Spiel würde man das vermeiden:** Drei Bedeutungen für denselben Methodennamen sind ein Rätsel für jeden Leser. Explizite Implementierung ist ein Werkzeug für Namenskollisionen, kein Stilmittel.

</details>

## Aufgabe 3 — Zerlegung

Der Dungeon soll einen **Teleporter** bekommen, der den Spieler an eine feste Zielposition versetzt. Ein Kommilitone schlägt vor:

```csharp
public sealed class Teleporter : StatischesObjekt, IInteragierbar
{
    public Position Ziel { get; }

    public Teleporter(Position position, Position ziel) : base("Teleporter", position)
    {
        Ziel = ziel;
    }

    public override char Symbol => 'O';
    public override bool IstPassierbar => true;      // man soll hineinlaufen können

    public string Interagieren(Spieler spieler)
    {
        spieler.Versetzen(Ziel);
        return "Es flimmert – du stehst woanders.";
    }
}
```

Der Code kompiliert fehlerfrei, aber der Teleporter tut nichts. Zerlege die Zugregel aus `Spielfeld.SpielerZieht` und finde den Grund.

- Unter welcher Bedingung wird `Interagieren` überhaupt aufgerufen?
- Welche zwei Entwürfe sind möglich (betretbar oder blockierend), und was kostet jeder?
- `Interagieren(Spieler spieler)` bekommt das Spielfeld nicht übergeben. Welche Folgen hat das für den Teleporter – und was passiert, wenn du die Signatur im Interface änderst?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Die Bedingung im Spielfeld:**

```csharp
if (davor is IInteragierbar interagierbar && !davor.IstPassierbar)
{
    meldung.Append(interagierbar.Interagieren(Spieler));
}
else if (Spieler.Bewegen(richtung, this)) { /* ... */ }
```

Interaktion ist in unserem Spiel als **Ersatzhandlung** definiert: Sie passiert genau dann, wenn der Spieler gegen etwas läuft, das ihn nicht durchlässt. Der Teleporter hat `IstPassierbar => true`, also greift der zweite Zweig – der Spieler läuft auf das Feld, und `Interagieren` wird nie aufgerufen. Das Interface ist korrekt implementiert und trotzdem wirkungslos. Ein Compiler kann so etwas nicht finden; ein Vertrag legt fest, *was* aufrufbar ist, nicht *wann* es jemand aufruft.

**Schritt 2 — Zwei mögliche Entwürfe:**

*Variante A – blockierender Teleporter:* `IstPassierbar => false` und alles bleibt, wie es ist. Der Spieler läuft gegen den Teleporter und wird versetzt, genau wie er gegen eine Tür läuft und sie aufschließt. Kosten: keine Änderung am Spielfeld, aber das Gefühl „ich laufe hinein“ geht verloren.

*Variante B – betretbarer Teleporter:* `IstPassierbar => true` plus das Interface `IBetretbar` aus Aufgabe 1. Dann wird `Betreten` nach dem Zug aufgerufen. Kosten: `SpielerZieht` braucht die zusätzliche Zeile – die sich aber sofort auch für Fallen, Feuerstellen und Rutschen lohnt.

Variante B ist der bessere Entwurf, weil sie die Lücke im Modell schließt: Bisher konnte ein Feld nur „blockieren und reagieren“ (Tür, Truhe) oder „passierbar sein und aufgehoben werden“ (Gegenstände). Alles dazwischen war nicht ausdrückbar.

**Schritt 3 — Das fehlende Spielfeld:**

`Interagieren` sieht nur den Spieler. Der Teleporter kann deshalb nicht prüfen, ob die Zielposition frei ist – er versetzt den Helden notfalls in eine Wand oder auf einen Gegner. Drei Auswege:

1. Die Prüfung beim Bauen des Levels erledigen (`LevelParser`) und die Zielposition als gültig voraussetzen.
2. Die Signatur ändern: `string Interagieren(Spieler spieler, Spielfeld feld)`. Das bricht **jede** bestehende Implementierung – `Tuer` und `Truhe` müssten angefasst werden, obwohl sie das Spielfeld nicht brauchen. Genau davor warnt die Zeile „neues Mitglied bricht alle Implementierungen“ in der [Gegenüberstellung](/modules/schnittstellen_vs_implementierungsvererbung/schnittstellen_vs_implementierungsvererbung.md).
3. Ein zweites, spezielleres Interface einführen, das vom ersten erbt:

```csharp
public interface IOrtsgebunden : IBetretbar
{
    string Betreten(Spieler spieler, Spielfeld feld);
}
```

Der Teleporter implementiert das größere Interface, alle anderen bleiben unberührt – das ist derselbe Trick wie bei `ITragbar : ISammelbar` im Modul [Interfaces – Erweiterte Konzepte](/modules/interfaces_erweitert/interfaces_erweitert.md).

**Zentrale Designentscheidungen:**

- **Ein Interface beschreibt eine Fähigkeit, kein Ereignis:** Wer eine neue Fähigkeit einführt, muss auch festlegen, an welcher Stelle der Spielschleife sie ausgelöst wird. Sonst entsteht toter Code, den kein Compiler meldet.
- **`IstPassierbar` ist mehr als eine Kollisionsregel:** Es entscheidet mit darüber, welcher Zweig der Zugregel läuft. Solche versteckten Kopplungen gehören dokumentiert.
- **Verträge wachsen nach außen, nicht nach innen:** Neue Anforderungen kommen in ein neues Interface, statt ein bestehendes umzubauen.

</details>

## Aufgabe 4 — Mustererkennung

Der folgende Code soll einen Brunnen ins Spiel bringen, aus dem der Held trinken kann. Er enthält drei Fehler. Finde sie, erkläre jede Meldung und korrigiere den Code so, dass die Absicht erhalten bleibt.

```csharp
interface IInteragierbar
{
    string Interagieren(Spieler spieler);
}

abstract class Spielobjekt
{
    public string Name { get; }
    protected Spielobjekt(string name) { Name = name; }
    public abstract char Symbol { get; }
}

class Brunnen : Spielobjekt, IInteragierbar
{
    public Brunnen() : base("Brunnen") { }
    public char Symbol => 'o';
    string Interagieren(Spieler spieler) => "Du trinkst aus dem Brunnen.";
}

Spielobjekt s = new Spielobjekt("Ding");
IInteragierbar b = new Brunnen();
```

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Fehlendes `override`:**

`Symbol` in `Brunnen` hat dieselbe Signatur wie das abstrakte `Symbol` in `Spielobjekt`, aber kein `override`. Für den Compiler ist das ein neues Property, das das geerbte versteckt – das abstrakte bleibt unimplementiert: *CS0534: „Brunnen“ implementiert den geerbten abstrakten Member „Spielobjekt.Symbol.get“ nicht*, dazu die Warnung *CS0114*, die genau sagt, was fehlt („To make the current member override that implementation, add the override keyword“). Korrektur: `public override char Symbol => 'o';`.

**Schritt 2 — Interface nicht öffentlich implementiert:**

`Interagieren` steht ohne Sichtbarkeit da – in einer Klasse bedeutet das `private`. Das Interface verlangt ein öffentliches Mitglied: *CS0737: „Brunnen“ implementiert den Schnittstellenmember „IInteragierbar.Interagieren(Spieler)“ nicht. „Brunnen.Interagieren(Spieler)“ kann den Schnittstellenmember nicht implementieren, da er nicht öffentlich ist.* Korrektur: `public string Interagieren(Spieler spieler) => ...`. Wäre eine explizite Implementierung gewollt, hieße die Zeile `string IInteragierbar.Interagieren(Spieler spieler) => ...` – dann bliebe sie über den Interface-Typ erreichbar.

**Schritt 3 — Abstrakte Klasse instanziiert:**

`new Spielobjekt("Ding")` ist *CS0144: Es kann keine Instanz des abstrakten Typs oder der Schnittstelle „Spielobjekt“ erstellt werden.* Wer ein Objekt braucht, schreibt eine konkrete Klasse mit `override char Symbol`. Die Variable `Spielobjekt s` selbst ist in Ordnung – als Kompilierzeittyp ist eine abstrakte Klasse erlaubt.

Kleines Detail beim Ausprobieren: Solange die beiden Fehler in der Klassendeklaration bestehen, meldet der Compiler CS0144 noch gar nicht – Methodenrümpfe werden erst geprüft, wenn die Typen selbst fehlerfrei sind. Fehlermeldungen abarbeiten heißt deshalb immer: oben anfangen und neu übersetzen.

**Zentrale Designentscheidungen:**

- **Der Compiler prüft Verträge vollständig:** Ob Interface oder abstrakte Klasse – jedes fehlende oder falsch sichtbare Mitglied wird beim Kompilieren gemeldet, nicht erst zur Laufzeit. Genau deshalb haben wir `Symbol` in Vorlesung 02 von `virtual` auf `abstract` umgestellt.
- **`override` ist keine Formsache:** Ohne das Schlüsselwort entsteht ein zweites, unabhängiges Mitglied. Die Warnung CS0114 sollte man nie ignorieren.
- **Ein fertiger Brunnen gehört an `StatischesObjekt`:** In der echten Hierarchie schreibt man `class Brunnen : StatischesObjekt, IInteragierbar` und bekommt Position und Konstruktor geschenkt.

</details>
