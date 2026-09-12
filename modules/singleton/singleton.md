---
title: "Singleton"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Manche Dinge gibt es in einem Programm genau einmal: die geladene Konfiguration, den Logger, der alle Meldungen in dieselbe Datei schreibt, den Treiber für ein angeschlossenes Gerät. Im Adventure wären das die Spielregeln selbst – wie weit ein Verfolger sieht, mit wie vielen Lebenspunkten der Held startet, wie viel eine Truhe wert ist. Würde jede Klasse ihre eigene Konfiguration laden, hätten wir zehn Kopien derselben Werte im Speicher, und wenn eine Stelle die Sichtweite ändert, sehen die anderen neun nichts davon. Das **Singleton**-Muster (Erzeugungsmuster) sorgt dafür, dass von einer Klasse höchstens eine Instanz existiert, dass sie erst beim ersten Zugriff entsteht und dass jeder im Programm sie erreichen kann. Es ist das einfachste aller Entwurfsmuster – und zugleich das umstrittenste, wie wir am Ende sehen werden.

## Problem

Wir brauchen eine Klasse, von der es garantiert nur eine Instanz gibt, und einen zentralen Zugangspunkt zu dieser Instanz. Eine normale Klasse kann das nicht garantieren: Jeder darf `new Spielkonfiguration()` schreiben, so oft er will. Eine statische Klasse wäre eine Alternative, hat aber keinen Zustand im Sinne eines Objekts, kann kein Interface implementieren und lässt sich nicht als Parameter übergeben.

## Lösung 1: privater Konstruktor und statische Instanz

Drei Zutaten machen aus einer Klasse ein Singleton: ein **privater Konstruktor**, damit niemand von außen `new` aufrufen kann, ein **statisches Feld** für die einzige Instanz und eine **statische Property**, die die Instanz beim ersten Zugriff erzeugt und danach immer dieselbe zurückgibt:

```csharp
public sealed class Spielkonfiguration
{
    private static Spielkonfiguration? instanz;

    public int Sichtweite { get; set; } = 5;
    public int Startleben { get; set; } = 3;

    private Spielkonfiguration()
    {
        Console.WriteLine("Spielkonfiguration wird geladen ...");
    }

    public static Spielkonfiguration Instanz
    {
        get
        {
            if (instanz == null)
                instanz = new Spielkonfiguration();
            return instanz;
        }
    }
}

Spielkonfiguration.Instanz.Sichtweite = 8;              // Spielkonfiguration wird geladen ...
Console.WriteLine(Spielkonfiguration.Instanz.Sichtweite); // 8
```

Die Konsolenausgabe im Konstruktor erscheint nur einmal, obwohl wir zweimal auf `Instanz` zugreifen: Beim ersten Mal ist das Feld `null` und das Objekt wird erzeugt, beim zweiten Mal liegt es schon vor. Das `sealed` verhindert, dass jemand über eine Unterklasse (siehe Modul [`sealed`](/modules/sealed/sealed.md)) doch noch weitere Instanzen erzeugt. Man spricht von **verzögerter Initialisierung** (*Lazy Initialization*): Die teure Arbeit im Konstruktor passiert erst, wenn sie wirklich gebraucht wird – ein Vorteil gegenüber einer globalen Variable, die beim Programmstart immer angelegt wird.

Benutzt würde diese Konfiguration im Spiel an genau den Stellen, an denen bisher ein Parameter oder eine Konstante steht:

```csharp
public sealed class Verfolger : Gegner
{
    public override Richtung? NaechsterZug(Spielfeld feld)
    {
        Position ziel = feld.Spieler.Position;
        if (Position.Entfernung(ziel) > Spielkonfiguration.Instanz.Sichtweite) return null;
        // ...
    }
}
```

Das sieht bequem aus – ein Wert an einer Stelle, überall erreichbar. Merke dir diese Zeile; wir kommen am Ende des Moduls darauf zurück, warum genau sie das Problem ist.

## Der Wettlauf

Diese Lösung hat einen versteckten Fehler, der erst auffällt, wenn mehrere Threads gleichzeitig laufen – in einer Webanwendung mit mehreren Spielenden ist das der Normalfall. Die Property besteht aus zwei getrennten Schritten: *prüfen* und *erzeugen*. Zwischen diesen Schritten kann das Betriebssystem jederzeit zu einem anderen Thread wechseln:

```
Thread 1                              Thread 2
----------------------------------    ----------------------------------
if (instanz == null)  → true
                                      if (instanz == null)  → true
                                      instanz = new Spielkonfiguration();  // Objekt A
instanz = new Spielkonfiguration();   // Objekt B überschreibt A
return instanz;                       return instanz;
```

Beide Threads sehen `null`, beide erzeugen ein Objekt, und Thread 2 hat am Ende möglicherweise eine Referenz auf Objekt A in der Hand, während `instanz` auf Objekt B zeigt. Genau die Garantie, für die das Muster da ist, ist verletzt. Man nennt so etwas eine **Race Condition** – einen Wettlauf, dessen Ausgang vom Zufall der Thread-Umschaltung abhängt. Im Workshop stellen wir ihn nach, indem wir zwischen Prüfung und Erzeugung eine künstliche Pause einbauen und zwei Threads starten:

```csharp
// im Getter, nur zur Demonstration:
if (instanz == null)
{
    Thread.Sleep(10);                       // Zeit für den zweiten Thread
    instanz = new Spielkonfiguration();
}

var t1 = new Thread(() => Console.WriteLine(Spielkonfiguration.Instanz.GetHashCode()));
var t2 = new Thread(() => Console.WriteLine(Spielkonfiguration.Instanz.GetHashCode()));
t1.Start(); t2.Start();
// Spielkonfiguration wird geladen ...
// Spielkonfiguration wird geladen ...
// 54267293
// 18643596
```

Zweimal „wird geladen“, zwei verschiedene Hashcodes: zwei Objekte. Setzt Thread 1 danach die Sichtweite auf 8, spielt Thread 2 weiter mit 5. Ohne die künstliche Pause passiert das selten – aber selten heißt nicht nie, und solche Fehler sind extrem schwer zu reproduzieren.

## Lösung 2: `lock` und doppelte Prüfung

Die Reparatur: Prüfung und Erzeugung müssen zusammen **unteilbar** ablaufen. Dafür gibt es in C# die `lock`-Anweisung. Sie sperrt einen Block für alle anderen Threads, bis der aktuelle Thread ihn verlassen hat. Damit nicht jeder Zugriff nach dem ersten die (langsame) Sperre durchlaufen muss, prüft man zweimal – einmal außerhalb und einmal innerhalb des `lock`:

```csharp
private static Spielkonfiguration? instanz;
private static readonly object schloss = new();

public static Spielkonfiguration Instanz
{
    get
    {
        if (instanz == null)                          // schneller Test ohne Sperre
        {
            lock (schloss)
            {
                if (instanz == null)                  // nochmal, jetzt geschützt
                    instanz = new Spielkonfiguration();
            }
        }
        return instanz;
    }
}
```

Dieses **Double-Checked Locking** funktioniert, ist aber fehleranfällig: Wer die zweite Prüfung vergisst, hat den Wettlauf nur verschoben, und die genauen Regeln, wann Threads Schreibzugriffe anderer Threads sehen, sind ein eigenes Fachgebiet. Das ist der Grund, warum man diesen Code in modernem C# fast nie mehr per Hand schreibt.

## Lösung 3: `Lazy<T>` oder statische Initialisierung

.NET bringt mit `Lazy<T>` eine Klasse mit, die genau dieses Problem löst: Sie bekommt eine Fabrikfunktion – ein Lambda, wie wir es aus dem Modul [Lambda-Ausdrücke](/modules/lambda_ausdruecke/lambda_ausdruecke.md) kennen – und ruft sie garantiert nur einmal auf, auch wenn mehrere Threads gleichzeitig `Value` abfragen:

```csharp
public sealed class Spielkonfiguration
{
    private static readonly Lazy<Spielkonfiguration> halter = new(() => new Spielkonfiguration());

    private Spielkonfiguration() { }

    public static Spielkonfiguration Instanz => halter.Value;
}
```

Das Lambda ist nötig, weil `Lazy<T>` den privaten Konstruktor nicht selbst aufrufen darf – nur Code innerhalb der Klasse kann das. Der gesamte Wettlauf-Code ist verschwunden, und die Verzögerung bleibt erhalten. Noch kürzer geht es, wenn die Instanz nicht unbedingt verzögert entstehen muss:

```csharp
private static readonly Spielkonfiguration instanz = new();
public static Spielkonfiguration Instanz => instanz;
```

Statische Felder initialisiert die Laufzeitumgebung garantiert genau einmal und threadsicher, bevor die Klasse das erste Mal benutzt wird. Der Preis: Die Instanz entsteht schon beim ersten Zugriff auf *irgendein* statisches Mitglied der Klasse, nicht erst bei `Instanz`. In den meisten Fällen ist das egal.

Im Zweifel: `Lazy<T>` statt `lock`. Handgeschriebene Synchronisation ist die häufigste Fehlerquelle bei Singletons, und `Lazy<T>` erledigt sie in einer Zeile korrekt.
{: .notice--primary}

## Beispiel in .NET

Singletons begegnen dir in .NET vor allem als statische Properties, die eine gemeinsam genutzte Instanz liefern: `Random.Shared` ist ein threadsicherer Zufallsgenerator, den sich das ganze Programm teilt – genau der, den ein `Zufallsgegner` aus [Vorlesung 07](/lectures/07/07.md) benutzt. `Comparer<T>.Default` und `EqualityComparer<T>.Default` sind die Standardvergleicher, die wir im Modul [`IComparable<T>` und Sortieren](/modules/icomparable_sortieren/icomparable_sortieren.md) indirekt benutzt haben. Ein Konstruktor ist in keinem dieser Fälle von außen erreichbar.

Eine zweite, viel angenehmere Form begegnet uns in `Adventure.Web/Program.cs`:

```csharp
builder.Services.AddSingleton<ILevelQuelle>(_ =>
    new TextdateiLevelQuelle(Path.Combine(AppContext.BaseDirectory, "levels")));
```

Auch hier gibt es zur Laufzeit nur *ein* Objekt, und der Dependency-Injection-Container von ASP.NET Core garantiert das. Der entscheidende Unterschied: `Home.razor` holt es sich nicht mit `TextdateiLevelQuelle.Instanz` ab, sondern bekommt es mit `@inject ILevelQuelle LevelQuelle` hineingereicht – und sieht nur das Interface. Einmaligkeit ist damit eine Entscheidung der Anwendung, keine Eigenschaft der Klasse.

## Vor- und Nachteile

Das Muster hat klare Vorteile gegenüber einer globalen Variable: Die Instanz entsteht erst bei Bedarf, es gibt eine Stelle, die die Erzeugung kontrolliert, und mit `Lazy<T>` ist die Thread-Sicherheit geschenkt. Die Nachteile wiegen aber oft schwerer:

- **Versteckte Abhängigkeiten.** Die Zeile `Spielkonfiguration.Instanz.Sichtweite` von oben steht mitten in `Verfolger.NaechsterZug`. Die Signatur `Richtung? NaechsterZug(Spielfeld feld)` verrät mit keinem Zeichen, dass diese Methode außer dem Spielfeld noch eine globale Konfiguration braucht. Wer `Verfolger` benutzen will, muss den Rumpf lesen.
- **Globaler Zustand.** Jeder kann die Instanz von überall verändern. Setzt die Weboberfläche für eine Vorschau kurz `Sichtweite = 1`, ändert sich das Verhalten *aller* Verfolger in *allen* laufenden Partien.
- **Schwer testbar.** Ein Unit-Test kann das Singleton nicht durch eine Testversion ersetzen – der Aufruf steht fest im Code. Und weil die Instanz zwischen zwei Tests weiterlebt, hängt das Ergebnis des zweiten Tests davon ab, was der erste an der Konfiguration gedreht hat. Wie man das vermeidet, sehen wir in [Aufgabe 4](/modules/aufgaben_entwurfsmuster/aufgaben_entwurfsmuster.md).
- **Unklarer Lebenszyklus.** Wann wird die Instanz freigegeben? Praktisch nie – sie lebt bis zum Programmende, weil das statische Feld sie festhält (siehe Modul [Garbage Collection](/modules/garbage_collection/garbage_collection.md)).

Im Zweifel: Wert übergeben statt Singleton. Genau das macht das Adventure: Ein `Verfolger` bekommt seine Sichtweite im Konstruktor (`public Verfolger(Position position, int sichtweite = 5)`), und `Spieler.MaxLebenspunkte` ist eine `const`, die sich zur Laufzeit niemand umbiegen kann. Beides ist sichtbar, testbar und pro Objekt einstellbar – ein Level mit einem besonders wachsamen Verfolger ist damit eine Zeile, mit dem Singleton ein Umbau. Ein Singleton ist nur dann angebracht, wenn eine zweite Instanz *technisch* falsch wäre, nicht bloß unnötig.
{: .notice--warning}

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v04-blazor`).

Übung: Schreibe eine Klasse `Bestenliste` als Singleton mit `Lazy<T>`, die Einträge aus Name und Punktzahl in einer `List<(string, int)>` sammelt und die besten drei liefert. Registriere sie als Empfänger von `Spieler.SchatzGefunden`. Starte anschließend drei Threads, die je zehn Einträge schreiben, und prüfe, ob am Ende alle 30 in der Liste stehen. Falls nicht: Das Singleton ist zwar threadsicher *erzeugt* – aber ist auch `List<T>.Add` threadsicher?
{: .notice--info}

## Weitere Quellen

- [`Lazy<T>` – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.lazy-1)
- [Verzögerte Initialisierung – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/framework/performance/lazy-initialization)
- [`lock`-Anweisung – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/statements/lock)
