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

Programmieren lernt man nicht nur durch Codezeilen tippen – sondern auch durch **Nachdenken**. Die folgenden Aufgaben trainieren *Computational Thinking*: die Fähigkeit, Probleme so zu strukturieren, dass ein Computer sie lösen kann. Bei abstrakten Klassen und Interfaces geht es vor allem um Abstraktion: Welche Gemeinsamkeit ist eine „Ist-ein“-Beziehung, welche nur eine Fähigkeit? Nimm dir für jede Aufgabe Zeit, bevor du die Lösung aufklappst.

## Aufgabe 1 — Abstraktion

Entwirf die Klassenstruktur für eine Zoo-Simulation. Es gibt Adler, Enten, Pinguine und Löwen. Jedes Tier hat einen Namen und ein Gewicht und macht ein arttypisches Geräusch. Adler und Enten können fliegen, Enten und Pinguine können schwimmen, Löwen keins von beidem. Die Simulation soll alle Tiere in einer Liste halten, jedes Tier „sprechen“ lassen und außerdem alle Flieger starten und alle Schwimmer ins Wasser schicken können.

- Was gehört in eine abstrakte Basisklasse, was in Interfaces? Warum keine Klassen `Flieger` und `Schwimmer`?
- Welche Mitglieder sind `abstract`, welche konkret?
- Wie findet die Simulation in einer `List<Tier>` die Schwimmer?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — „Ist-ein“ von „Kann-das“ trennen:**

Adler, Ente, Pinguin und Löwe *sind* Tiere: gemeinsamer Zustand (`Name`, `Gewicht`), gemeinsamer Konstruktor, ein Geräusch, das jedes Tier anders macht. Das ist eine abstrakte Klasse `Tier` mit `abstract string Laut()`. Fliegen und Schwimmen sind Fähigkeiten, die quer durch die Arten verteilt sind – die Ente kann beides. Mit Basisklassen `Flieger` und `Schwimmer` müsste sich die Ente für eine entscheiden. Also Interfaces.

**Schritt 2 — Klassen:**

```csharp
abstract class Tier
{
    public string Name { get; }
    public double Gewicht { get; set; }
    protected Tier(string name, double gewicht) { Name = name; Gewicht = gewicht; }

    public abstract string Laut();
    public void Vorstellen() => Console.WriteLine($"{Name} ({Gewicht} kg): {Laut()}");
}

interface IKannFliegen { void Starten(); }
interface IKannSchwimmen { void Tauchen(); }

class Ente : Tier, IKannFliegen, IKannSchwimmen
{
    public Ente(string name) : base(name, 1.2) { }
    public override string Laut() => "Quak";
    public void Starten() => Console.WriteLine($"{Name} fliegt los");
    public void Tauchen() => Console.WriteLine($"{Name} taucht");
}

class Pinguin : Tier, IKannSchwimmen
{
    public Pinguin(string name) : base(name, 25) { }
    public override string Laut() => "Kräh";
    public void Tauchen() => Console.WriteLine($"{Name} schießt durchs Wasser");
}
// Adler : Tier, IKannFliegen  und  Loewe : Tier  analog
```

**Schritt 3 — Simulation:**

```csharp
List<Tier> zoo = new() { new Ente("Erna"), new Pinguin("Pingo"), new Adler("Aki"), new Loewe("Leo") };

foreach (Tier t in zoo) t.Vorstellen();
foreach (Tier t in zoo)
    if (t is IKannSchwimmen s) s.Tauchen();
```

**Zentrale Designentscheidungen:**

- **`Vorstellen` ist konkret, `Laut` abstrakt:** Der Ablauf ist für alle gleich, nur das Geräusch variiert – die Basisklasse ruft das abstrakte Mitglied auf und verlässt sich auf den Laufzeittyp.
- **Fähigkeiten als Interfaces:** `Ente` kombiniert beide, ohne dass eine Mehrfachvererbung nötig wäre. Ein neues Tier `Fliegender Fisch` ließe sich ohne Änderung an `Tier` ergänzen.
- **Auswahl per `is`-Muster:** Die Liste bleibt homogen (`List<Tier>`), die Fähigkeit wird pro Objekt geprüft. Alternativ könnte man `zoo.OfType<IKannSchwimmen>()` verwenden – dazu mehr bei LINQ.

</details>

## Aufgabe 2 — Mustererkennung

Sage die Ausgabe des folgenden Programms voraus, bevor du es ausführst. Achte auf explizite Implementierungen, `new` und den Unterschied zwischen Kompilierzeit- und Laufzeittyp.

```csharp
interface IHeissgetraenk { string Info(); }
interface IHatKoffein { string Info(); }

class Kaffee : IHeissgetraenk, IHatKoffein
{
    public string Info() => "Kaffee";
    string IHeissgetraenk.Info() => "heiß";
    string IHatKoffein.Info() => "koffeinhaltig";
}

class Espresso : Kaffee
{
    public new string Info() => "Espresso";
}

Kaffee k = new Espresso();
Console.WriteLine(k.Info());
Console.WriteLine(((IHeissgetraenk)k).Info());
Console.WriteLine(((IHatKoffein)k).Info());
Espresso e = (Espresso)k;
Console.WriteLine(e.Info());
IHeissgetraenk h = e;
Console.WriteLine(h.Info());
Console.WriteLine(k is IHatKoffein);
```

- Welche `Info` ist über eine `Kaffee`-Variable erreichbar, welche nur über einen Interface-Typ?
- Warum ändert `new` in `Espresso` nichts an den Interface-Aufrufen?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Zeile für Zeile:**

```
Kaffee
heiß
koffeinhaltig
Espresso
heiß
True
```

**Schritt 2 — Begründung:**

`k.Info()` ist ein Aufruf über den Kompilierzeittyp `Kaffee`. Dort gibt es eine öffentliche `Info`, und `Espresso` **versteckt** sie mit `new`, statt sie zu überschreiben – wie in [Laufzeittyp und Verstecken](/modules/laufzeittyp_verstecken/laufzeittyp_verstecken.md) besprochen zählt bei `new` der deklarierte Typ, also `Kaffee`. Die beiden Casts wählen jeweils die explizite Implementierung des Interfaces aus; die öffentliche `Info` spielt dabei keine Rolle. Über `e` vom Typ `Espresso` ist die versteckende Methode sichtbar. `h.Info()` geht wieder über das Interface: `Espresso` implementiert `IHeissgetraenk` nicht neu, also gilt weiterhin die explizite Implementierung aus `Kaffee`. `k is IHatKoffein` ist wahr, weil jedes `Espresso` ein `Kaffee` ist und `Kaffee` das Interface implementiert.

**Zentrale Designentscheidungen:**

- **Explizite Implementierung entkoppelt gleichnamige Interface-Mitglieder:** Zwei Interfaces mit `Info()` können unterschiedlich beantwortet werden, die Klasse behält eine eigene, dritte Version.
- **Interface-Aufrufe sind immer polymorph, `new` nicht:** Wer `Info` in `Espresso` für alle Aufrufwege ändern will, muss die Interface-Methoden in `Kaffee` `virtual` machen oder in `Espresso` die Interfaces erneut implementieren.

</details>

## Aufgabe 3 — Zerlegung

Der Geometrieeditor soll ein `Quadrat` bekommen. Der schnelle Vorschlag lautet: `class Quadrat : Rechteck`, denn ein Quadrat ist mathematisch ein Rechteck. Prüfe diesen Vorschlag anhand der Klasse `Rechteck` aus `examples/03_blazor/Geometrieeditor` (`Breite` und `Hoehe` sind `{ get; set; }`) und entscheide, ob `Quadrat` von `Rechteck` oder direkt von `Figur` erben sollte.

- Was passiert mit einem `Quadrat`, wenn jemand über eine `Rechteck`-Variable `Breite = 5` setzt?
- Kann `Quadrat` das verhindern, ohne `Rechteck` zu ändern?
- Was verspricht `Figur` – und was verspricht `Rechteck` zusätzlich?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Das Problem mit `Quadrat : Rechteck`:**

```csharp
Rechteck r = new Quadrat("Q1", 0, 0, 3);
r.Breite = 5;                      // erlaubt – Rechteck verspricht einen Setter
Console.WriteLine(r.Flaeche);      // 15 – aber ein Quadrat mit Fläche 15 und Seite 3?
```

`Rechteck` verspricht, dass `Breite` und `Hoehe` unabhängig gesetzt werden können. Ein Quadrat kann dieses Versprechen nicht einhalten. Da die Properties in `Rechteck` weder `virtual` noch `abstract` sind, kann `Quadrat` die Setter nicht einmal überschreiben, um beide Seiten zu koppeln. Das ist das **Liskov-Substitutionsprinzip**: Eine Unterklasse muss überall dort funktionieren, wo die Basisklasse erwartet wird. Ein `Quadrat`, das als `Rechteck` benutzt wird, verhält sich falsch.

**Schritt 2 — `Quadrat : Figur`:**

`Figur` verspricht nur Name, Position, `Flaeche`, `Umfang`, `Verschieben` und `Beschreibung()`. All das kann ein Quadrat problemlos erfüllen:

```csharp
public class Quadrat : Figur
{
    public double Seite { get; set; }

    public Quadrat(string name, double x, double y, double seite) : base(name, x, y)
    {
        Seite = seite;
    }

    public override double Flaeche => Seite * Seite;
    public override double Umfang => 4 * Seite;
    public override string Beschreibung() => base.Beschreibung() + $" (Seite {Seite})";
}
```

Die `FigurenVerwaltung` und alles, was mit `List<Figur>` arbeitet, funktioniert unverändert.

**Zentrale Designentscheidungen:**

- **Vererbung folgt dem Verhalten, nicht der Mathematik:** „Ist-ein“ im Sinne von OOP bedeutet „kann überall als Ersatz dienen“. Bei veränderbaren Objekten ist ein Quadrat kein ersetzbares Rechteck.
- **Die Basisklasse bestimmt, was man versprechen muss:** `Figur` ist bewusst schlank gehalten, deshalb passt jede Figur darunter. Wäre `Rechteck` unveränderlich (nur Getter), sähe die Entscheidung anders aus.

</details>

## Aufgabe 4 — Mustererkennung

Der folgende Code enthält drei Fehler, die der Compiler meldet. Finde sie, erkläre jede Fehlermeldung und korrigiere den Code so, dass die Absicht erhalten bleibt.

```csharp
interface IHeissgetraenk
{
    int Temperatur { get; set; }
    void Abkuehlen(int grad);
}

abstract class Getraenk
{
    public string Name { get; }
    protected Getraenk(string name) { Name = name; }
    public abstract double Preis();
}

class Tee : Getraenk, IHeissgetraenk
{
    int Temperatur { get; set; }
    public Tee() : base("Tee") { }
    public double Preis() => 2.5;
}

Getraenk g = new Getraenk("Wasser");
IHeissgetraenk t = new Tee();
```

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Interface unvollständig implementiert:**

`Tee` deklariert `Temperatur` ohne `public` – in einer Klasse bedeutet das `private`. Das Interface verlangt ein öffentliches Property, also meldet der Compiler *CS0737: „Tee“ implementiert den Schnittstellenmember „IHeissgetraenk.Temperatur“ nicht (nicht öffentlich)*. Außerdem fehlt `Abkuehlen` vollständig (*CS0535*). Korrektur: `public int Temperatur { get; set; }` und `public void Abkuehlen(int grad) => Temperatur -= grad;`.

**Schritt 2 — Fehlendes `override`:**

`Preis()` in `Tee` hat dieselbe Signatur wie das abstrakte `Preis()` in `Getraenk`, aber kein `override`. Für den Compiler ist das eine neue Methode, die die geerbte versteckt – die abstrakte bleibt unimplementiert: *CS0534: „Tee“ implementiert den geerbten abstrakten Member „Getraenk.Preis()“ nicht*, dazu die Warnung *CS0108*, dass `Preis` ohne `new` versteckt. Korrektur: `public override double Preis() => 2.5;`.

**Schritt 3 — Abstrakte Klasse instanziiert:**

`new Getraenk("Wasser")` ist *CS0144: Eine Instanz der abstrakten Klasse „Getraenk“ kann nicht erstellt werden*. Wer ein Wasser braucht, schreibt eine Klasse `Wasser : Getraenk` mit `override double Preis()`. Die Variable `Getraenk g` selbst ist in Ordnung – als Kompilierzeittyp ist eine abstrakte Klasse erlaubt.

**Zentrale Designentscheidungen:**

- **Der Compiler prüft Verträge vollständig:** Ob Interface oder abstrakte Klasse – jedes fehlende oder falsch sichtbare Mitglied wird beim Kompilieren gemeldet, nicht erst zur Laufzeit.
- **`override` ist keine Formsache:** Ohne das Schlüsselwort entsteht eine zweite, unabhängige Methode. Genau deshalb sollte man die Warnung CS0108 nie ignorieren.

</details>
