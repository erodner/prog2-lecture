---
title: "Abstrakte Klassen"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Stell dir einen Bauplan für „ein Gebäude“ vor: Er legt fest, dass es ein Fundament, Wände und ein Dach gibt – aber niemand kann nach diesem Plan bauen, weil er nicht sagt, ob es ein Einfamilienhaus oder eine Lagerhalle wird. Trotzdem ist der Plan nützlich: Jeder konkrete Bauplan muss diese Punkte ausfüllen. Genau das leistet eine **abstrakte Klasse** in C#. Sie fasst zusammen, was alle Unterklassen gemeinsam haben, schreibt vor, was jede Unterklasse selbst liefern muss, und lässt sich bewusst nicht instanziieren. In diesem Modul bauen wir damit die Klasse `Spielobjekt` aus unserem Dungeon-Spiel um – die Wurzel der Hierarchie, die uns bis zum Ende des Semesters begleitet.

## Das Problem mit `new Spielobjekt(...)`

In der [letzten Vorlesung](/lectures/01/01.md) ist `Spielobjekt` als ganz normale Basisklasse entstanden: Alles, was auf dem Spielfeld liegt, hat einen Namen, eine Position, ein Zeichen für die Karte und die Information, ob man darüberlaufen darf.

```csharp
public class Spielobjekt
{
    public string Name { get; }
    public Position Position { get; protected set; }

    public Spielobjekt(string name, Position position)
    {
        Name = name;
        Position = position;
    }

    public virtual char Symbol => '?';          // Notnagel!
    public virtual bool IstPassierbar => false;
}
```

Das `'?'` ist der wunde Punkt. Es steht da nur, weil ein `virtual`-Property einen Rumpf braucht – eine sinnvolle Antwort gibt es nicht, denn „irgendein Spielobjekt“ hat kein Zeichen. Schlimmer noch: Der Compiler erlaubt damit Code, der fachlich Unsinn ist.

```csharp
Spielobjekt ding = new Spielobjekt("Ding", new Position(3, 4));
feld.Hinzufuegen(ding);   // liegt jetzt als '?' im Dungeon herum
```

Ein Objekt, das weder Wand noch Tür noch Trank ist, kann es im Spiel nicht geben. Und wenn jemand später eine neue Objektart schreibt und das `override` für `Symbol` vergisst, fällt das nicht beim Kompilieren auf – es erscheint einfach ein `?` auf der Karte. Beide Probleme lösen wir mit einem einzigen Schlüsselwort.

## Die abstrakte Klasse `Spielobjekt`

Mit `abstract` erklären wir eine Klasse zu einer reinen Verallgemeinerung: Sie beschreibt kein konkretes Objekt, sondern nur, was alle gemeinsam haben. Und sie darf **abstrakte Mitglieder** enthalten – Methoden oder Properties, die nur aus ihrer Signatur bestehen und keinen Rumpf haben.

```csharp
public abstract class Spielobjekt
{
    public string Name { get; }
    public Position Position { get; protected set; }

    protected Spielobjekt(string name, Position position)
    {
        Name = name;
        Position = position;
    }

    /// <summary>Das Zeichen, mit dem das Objekt auf der Karte gezeichnet wird.</summary>
    public abstract char Symbol { get; }

    /// <summary>Darf ein bewegliches Objekt dieses Feld betreten?</summary>
    public virtual bool IstPassierbar => false;

    public virtual string Beschreibung() => $"{Name} bei {Position}";

    public override string ToString() => Beschreibung();
}
```

Zwei Dinge fallen auf. Erstens steht hinter `Symbol` keine Berechnung mehr, nur ein `{ get; }` – die Basisklasse verspricht, dass es dieses Property gibt, überlässt die Antwort aber den Unterklassen. Zweitens ist der Konstruktor `protected`: Er wird nur noch von abgeleiteten Klassen über `base(...)` aufgerufen, denn von außen kann ohnehin niemand ein `Spielobjekt` erzeugen. `IstPassierbar` und `Beschreibung()` bleiben dagegen `virtual`, weil „Wände blockieren, alles andere überschreibt bei Bedarf“ eine brauchbare Standardantwort ist – mehr dazu im [nächsten Modul](/modules/abstrakte_mitglieder/abstrakte_mitglieder.md).

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v02-interfaces`).
{: .notice--primary}

## Keine Objekte aus abstrakten Klassen

Was passiert jetzt mit dem Codestück von oben?

```csharp
Spielobjekt ding = new Spielobjekt("Ding", new Position(3, 4));
// error CS0144: Eine Instanz des abstrakten Typs oder der abstrakten Schnittstelle
//               "Spielobjekt" kann nicht erstellt werden.
```

Der Compiler verweigert das – und das ist genau gewollt. Aus einem Laufzeitproblem („warum steht da ein `?`“) ist ein Kompilierzeitfehler geworden. `Spielobjekt` existiert nur noch, damit andere Klassen von ihr erben; als Typ für Variablen, Parameter und Sammlungen bleibt sie uneingeschränkt erlaubt.

Eine Klasse, die auch nur ein einziges abstraktes Mitglied enthält, muss selbst als `abstract` markiert sein. Umgekehrt darf eine abstrakte Klasse durchaus ohne abstrakte Mitglieder auskommen – dann drückt `abstract` nur aus, dass Instanzen keinen Sinn ergeben.
{: .notice--warning}

## Zwei abstrakte Zwischenklassen

Genau dieser Fall tritt in unserem Spiel sofort ein. Die Objekte zerfallen in zwei Gruppen: Manche liegen fest an ihrem Platz, andere laufen über die Karte. Diese Unterscheidung braucht das Spielfeld ständig – Wände werden in einem `Dictionary<Position, StatischesObjekt>` abgelegt, Gegner in einer Liste. Also bekommt sie zwei eigene Klassen:

```csharp
/// <summary>Objekte, die sich nie bewegen: Wände, Türen, Truhen, Gegenstände auf dem Boden.</summary>
public abstract class StatischesObjekt : Spielobjekt
{
    protected StatischesObjekt(string name, Position position) : base(name, position)
    {
    }
}

/// <summary>Objekte, die sich über das Spielfeld bewegen: der Spieler und alle Gegner.</summary>
public abstract class BeweglichesObjekt : Spielobjekt
{
    protected BeweglichesObjekt(string name, Position position) : base(name, position)
    {
    }

    /// <summary>Versucht einen Schritt; bleibt stehen, wenn das Zielfeld nicht frei ist.</summary>
    public bool Bewegen(Richtung richtung, Spielfeld feld)
    {
        Position ziel = Position.Verschoben(richtung);
        if (!feld.IstFrei(ziel)) return false;
        Position = ziel;
        return true;
    }
}
```

`StatischesObjekt` enthält nichts als einen Konstruktor und ist trotzdem `abstract` – sie ist eine reine Einordnung, und „ein statisches Objekt“ ohne genauere Art gibt es nicht. `BeweglichesObjekt` bringt zusätzlich echten Code mit, den sich Spieler und Gegner teilen. Beide erben das abstrakte `Symbol` weiter, ohne es zu implementieren: Eine abstrakte Klasse darf einen abstrakten Vertrag an ihre Erben durchreichen.

Mit diesen beiden Zwischenklassen steht das Gerüst, das uns bis zum Semesterende trägt:

<svg viewBox="0 0 700 310" role="img" aria-labelledby="titel-hierarchie" xmlns="http://www.w3.org/2000/svg" style="max-width:100%;height:auto;font-family:system-ui,sans-serif">
<title id="titel-hierarchie">Klassenhierarchie des Adventure: die abstrakte Wurzel Spielobjekt, darunter die abstrakten Zwischenklassen StatischesObjekt und BeweglichesObjekt und deren konkrete Unterklassen.</title>
<defs>
<marker id="uml-spitze-hierarchie" viewBox="0 0 10 10" refX="0" refY="5" markerWidth="10" markerHeight="10" markerUnits="userSpaceOnUse" orient="auto">
<path d="M0 0 L10 5 L0 10 Z" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linejoin="round"/>
</marker>
</defs>
<text x="8" y="18" font-size="15" fill="currentColor">Die Klassenhierarchie des Adventure</text>
<g fill="none" stroke="currentColor" stroke-width="1.5">
<path d="M190 86 H574"/>
<path d="M190 86 V104"/>
<path d="M574 86 V104"/>
<path d="M382 86 V78" marker-end="url(#uml-spitze-hierarchie)"/>
<path d="M36 160 H327"/>
<path d="M36 160 V178"/>
<path d="M107 160 V178"/>
<path d="M178 160 V178"/>
<path d="M244 160 V178"/>
<path d="M327 160 V178"/>
<path d="M190 160 V152" marker-end="url(#uml-spitze-hierarchie)"/>
<path d="M533 160 H612"/>
<path d="M533 160 V178"/>
<path d="M612 160 V178"/>
<path d="M574 160 V152" marker-end="url(#uml-spitze-hierarchie)"/>
<path d="M259 234 H408"/>
<path d="M259 234 V252"/>
<path d="M340 234 V252"/>
<path d="M408 234 V252"/>
<path d="M327 234 V226" marker-end="url(#uml-spitze-hierarchie)"/>
<path d="M567 234 H645"/>
<path d="M567 234 V252"/>
<path d="M645 234 V252"/>
<path d="M612 234 V226" marker-end="url(#uml-spitze-hierarchie)"/>
</g>
<g fill="currentColor" fill-opacity="0.06" stroke="currentColor" stroke-width="1.5">
<rect x="334" y="30" width="96" height="38" rx="4"/>
<rect x="124" y="104" width="132" height="38" rx="4"/>
<rect x="504" y="104" width="140" height="38" rx="4"/>
<rect x="8" y="178" width="56" height="26" rx="4"/>
<rect x="74" y="178" width="66" height="26" rx="4"/>
<rect x="150" y="178" width="56" height="26" rx="4"/>
<rect x="216" y="178" width="56" height="26" rx="4"/>
<rect x="282" y="178" width="90" height="38" rx="4"/>
<rect x="500" y="178" width="66" height="26" rx="4"/>
<rect x="576" y="178" width="72" height="38" rx="4"/>
<rect x="216" y="252" width="86" height="26" rx="4"/>
<rect x="312" y="252" width="56" height="26" rx="4"/>
<rect x="378" y="252" width="60" height="26" rx="4"/>
<rect x="539" y="252" width="56" height="26" rx="4"/>
<rect x="605" y="252" width="80" height="26" rx="4"/>
</g>
<g fill="currentColor" text-anchor="middle">
<text x="382" y="45" font-size="11">«abstrakt»</text>
<text x="382" y="61" font-size="12" font-style="italic" font-family="ui-monospace,monospace">Spielobjekt</text>
<text x="190" y="119" font-size="11">«abstrakt»</text>
<text x="190" y="135" font-size="12" font-style="italic" font-family="ui-monospace,monospace">StatischesObjekt</text>
<text x="574" y="119" font-size="11">«abstrakt»</text>
<text x="574" y="135" font-size="12" font-style="italic" font-family="ui-monospace,monospace">BeweglichesObjekt</text>
<text x="36" y="195" font-size="12" font-family="ui-monospace,monospace">Wand</text>
<text x="107" y="195" font-size="12" font-family="ui-monospace,monospace">Ausgang</text>
<text x="178" y="195" font-size="12" font-family="ui-monospace,monospace">Tuer</text>
<text x="244" y="195" font-size="12" font-family="ui-monospace,monospace">Truhe</text>
<text x="327" y="193" font-size="11">«abstrakt»</text>
<text x="327" y="209" font-size="12" font-style="italic" font-family="ui-monospace,monospace">Gegenstand</text>
<text x="533" y="195" font-size="12" font-family="ui-monospace,monospace">Spieler</text>
<text x="612" y="193" font-size="11">«abstrakt»</text>
<text x="612" y="209" font-size="12" font-style="italic" font-family="ui-monospace,monospace">Gegner</text>
<text x="259" y="269" font-size="12" font-family="ui-monospace,monospace">Schluessel</text>
<text x="340" y="269" font-size="12" font-family="ui-monospace,monospace">Trank</text>
<text x="408" y="269" font-size="12" font-family="ui-monospace,monospace">Schatz</text>
<text x="567" y="269" font-size="12" font-family="ui-monospace,monospace">Wache</text>
<text x="645" y="269" font-size="12" font-family="ui-monospace,monospace">Verfolger</text>
</g>
<text x="8" y="298" font-size="12" fill="currentColor">«abstrakt» + Kursivschrift = abstrakte Klasse · Pfeil mit leerer Spitze = »erbt von«</text>
</svg>

Jeder Pfeil im Bild zeigt mit seiner leeren Spitze nach oben auf die Basisklasse und liest sich als „erbt von“: `Wache` erbt von `Gegner`, `Gegner` von `BeweglichesObjekt`, `BeweglichesObjekt` von `Spielobjekt`. Kursiv gesetzt und mit `«abstrakt»` überschrieben sind genau die Klassen, die `abstract` sind – von ihnen kann `new` kein Objekt erzeugen, sie beschreiben nur den gemeinsamen Nenner ihrer Erben. Entstehen können Objekte ausschließlich aus den aufrecht gesetzten Kästen: `Wand`, `Ausgang`, `Spieler` und ihre Geschwister. Die beiden Zwischenklassen teilen den Baum dabei sauber in zwei Äste, und jede neue Objektart wird sich später in genau einen davon einordnen. Die rechte Hälfte mit `Gegner`, `Wache` und `Verfolger` bauen wir in Vorlesung 05 und 07 aus; `Tuer`, `Truhe` und `Gegenstand` bekommen im [nächsten Modul über Interfaces](/modules/interfaces_grundlagen/interfaces_grundlagen.md) zusätzliche Fähigkeiten, die quer zu diesem Baum liegen.

## Abgeleitete Klassen müssen liefern

Erst die konkreten Klassen am Ende der Kette lösen das Versprechen ein. Jede nicht-abstrakte Unterklasse **muss** alle geerbten abstrakten Mitglieder mit `override` implementieren – vergisst man eines, kompiliert die Klasse nicht.

```csharp
public sealed class Wand : StatischesObjekt
{
    public Wand(Position position) : base("Wand", position) { }

    public override char Symbol => '#';
}

public sealed class Ausgang : StatischesObjekt
{
    public Ausgang(Position position) : base("Ausgang", position) { }

    public override char Symbol => 'E';
    public override bool IstPassierbar => true;
}
```

Das `override` ist dasselbe Schlüsselwort wie bei [`virtual`-Methoden](/modules/virtual_override/virtual_override.md) – nur dass es hier keine Wahl ist, sondern Pflicht. Schön sichtbar wird der Unterschied an `Ausgang`: `Symbol` **muss** überschrieben werden, `IstPassierbar` **darf** überschrieben werden, weil man durch den Ausgang hindurchlaufen können soll. Die `Wand` verzichtet darauf und erbt die Standardantwort `false`. Und `sealed` sorgt dafür, dass an dieser Stelle Schluss ist – von einer Wand muss niemand mehr erben.

## Polymorphie mit abstrakten Klassen

Der Gewinn zeigt sich, sobald das Spielfeld alle Objekte gemeinsam behandelt. Beim Zeichnen der Karte fragt es jedes Feld nach dem Objekt, das dort liegt, und holt sich dessen `Symbol`:

```csharp
public string AlsText()
{
    StringBuilder sb = new();
    for (int y = 0; y < Hoehe; y++)
    {
        for (int x = 0; x < Breite; x++)
        {
            sb.Append(ObjektAn(new Position(x, y))?.Symbol ?? '.');
        }
        sb.AppendLine();
    }
    return sb.ToString();
}
```

`ObjektAn` liefert ein `Spielobjekt?` – welche Art dahintersteckt, weiß die Schleife nicht. Trotzdem erscheint für eine Wand ein `#`, für den Spieler ein `@` und für eine offene Tür ein `/`, weil – wie beim [Laufzeittyp](/modules/laufzeittyp_verstecken/laufzeittyp_verstecken.md) besprochen – die Implementierung des tatsächlichen Objekts aufgerufen wird. Eine abstrakte Methode ist damit automatisch polymorph; ein zusätzliches `virtual` braucht sie nicht.

```
####################
#@.....#...........#
#......#.....W.....#
#..k...#...........#
#......D...........#
```

Der entscheidende Punkt: Das `?` kann in dieser Ausgabe nicht mehr auftauchen. Der Compiler hat jede Objektart gezwungen, sich für ein Zeichen zu entscheiden – und jede neue Objektart, die wir in den nächsten Modulen ergänzen, wird beim Kompilieren daran erinnert.

Übung: Ergänze eine Klasse `Statue : StatischesObjekt` mit dem Symbol `'S'`, die nicht passierbar ist. Lass zuerst das `override` bei `Symbol` weg und lies die Fehlermeldung genau. Versuche danach, `StatischesObjekt` direkt zu instanziieren, und vergleiche die Fehlernummer mit CS0144. Welche der beiden Fehlermeldungen hättest du bei der alten, nicht-abstrakten Version aus Vorlesung 01 bekommen?
{: .notice--info}

## Weitere Quellen

- [abstract (C#-Referenz) – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/keywords/abstract)
- [Abstrakte und versiegelte Klassen und Klassenmember – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/classes-and-structs/abstract-and-sealed-classes-and-class-members)
- [Compilerfehler CS0144 – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/misc/cs0144)
- [Type Object – Game Programming Patterns](https://gameprogrammingpatterns.com/type-object.html) – freies Buch über Spielarchitektur; das Kapitel zeigt, wann eine Klassenhierarchie für Gegnerarten an ihre Grenzen stößt.
- [dotnetfiddle.net](https://dotnetfiddle.net/) – C# im Browser ausprobieren: ideal, um CS0144 und ein vergessenes `override` in zwei Minuten selbst zu provozieren.
