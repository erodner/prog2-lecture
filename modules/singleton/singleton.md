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

Manche Dinge gibt es in einem Programm genau einmal: die geladene Konfiguration, den Logger, der alle Meldungen in dieselbe Datei schreibt, den Treiber für ein angeschlossenes Gerät. Würde jede Klasse ihre eigene Konfiguration laden, hätten wir zehn Kopien derselben Datei im Speicher – und wenn eine Stelle den Wert ändert, sehen die anderen neun nichts davon. Das **Singleton**-Muster (Erzeugungsmuster) sorgt dafür, dass von einer Klasse höchstens eine Instanz existiert, dass sie erst beim ersten Zugriff entsteht und dass jeder im Programm sie erreichen kann. Es ist das einfachste aller Entwurfsmuster – und zugleich das umstrittenste, wie wir am Ende sehen werden.

## Problem

Wir brauchen eine Klasse, von der es garantiert nur eine Instanz gibt, und einen zentralen Zugangspunkt zu dieser Instanz. Eine normale Klasse kann das nicht garantieren: Jeder darf `new Konfiguration()` schreiben, so oft er will. Eine statische Klasse wäre eine Alternative, hat aber keinen Zustand im Sinne eines Objekts, kann kein Interface implementieren und lässt sich nicht als Parameter übergeben.

## Lösung 1: privater Konstruktor und statische Instanz

Drei Zutaten machen aus einer Klasse ein Singleton: ein **privater Konstruktor**, damit niemand von außen `new` aufrufen kann, ein **statisches Feld** für die einzige Instanz und eine **statische Property**, die die Instanz beim ersten Zugriff erzeugt und danach immer dieselbe zurückgibt:

```csharp
public sealed class Konfiguration
{
    private static Konfiguration? instanz;

    public string Sprache { get; set; } = "de";

    private Konfiguration()
    {
        Console.WriteLine("Konfiguration wird geladen ...");
    }

    public static Konfiguration Instanz
    {
        get
        {
            if (instanz == null)
                instanz = new Konfiguration();
            return instanz;
        }
    }
}

Konfiguration.Instanz.Sprache = "en";          // Konfiguration wird geladen ...
Console.WriteLine(Konfiguration.Instanz.Sprache); // en
```

Die Konsolenausgabe im Konstruktor erscheint nur einmal, obwohl wir zweimal auf `Instanz` zugreifen: Beim ersten Mal ist das Feld `null` und das Objekt wird erzeugt, beim zweiten Mal liegt es schon vor. Das `sealed` verhindert, dass jemand über eine Unterklasse (siehe Modul [`sealed`](/modules/sealed/sealed.md)) doch noch weitere Instanzen erzeugt. Man spricht von **verzögerter Initialisierung** (*Lazy Initialization*): Die teure Arbeit im Konstruktor passiert erst, wenn sie wirklich gebraucht wird – ein Vorteil gegenüber einer globalen Variable, die beim Programmstart immer angelegt wird.

## Der Wettlauf

Diese Lösung hat einen versteckten Fehler, der erst auffällt, wenn mehrere Threads gleichzeitig laufen – in einer GUI-Anwendung mit Hintergrundaufgaben ist das der Normalfall. Die Property besteht aus zwei getrennten Schritten: *prüfen* und *erzeugen*. Zwischen diesen Schritten kann das Betriebssystem jederzeit zu einem anderen Thread wechseln:

```
Thread 1                              Thread 2
----------------------------------    ----------------------------------
if (instanz == null)  → true
                                      if (instanz == null)  → true
                                      instanz = new Konfiguration();   // Objekt A
instanz = new Konfiguration();   // Objekt B überschreibt A
return instanz;                       return instanz;
```

Beide Threads sehen `null`, beide erzeugen ein Objekt, und Thread 2 hat am Ende möglicherweise eine Referenz auf Objekt A in der Hand, während `instanz` auf Objekt B zeigt. Genau die Garantie, für die das Muster da ist, ist verletzt. Man nennt so etwas eine **Race Condition** – einen Wettlauf, dessen Ausgang vom Zufall der Thread-Umschaltung abhängt. Im Workshop stellen wir ihn nach, indem wir zwischen Prüfung und Erzeugung eine künstliche Pause einbauen und zwei Threads starten:

```csharp
// im Getter, nur zur Demonstration:
if (instanz == null)
{
    Thread.Sleep(10);               // Zeit für den zweiten Thread
    instanz = new Konfiguration();
}

var t1 = new Thread(() => Console.WriteLine(Konfiguration.Instanz.GetHashCode()));
var t2 = new Thread(() => Console.WriteLine(Konfiguration.Instanz.GetHashCode()));
t1.Start(); t2.Start();
// Konfiguration wird geladen ...
// Konfiguration wird geladen ...
// 54267293
// 18643596
```

Zweimal „wird geladen“, zwei verschiedene Hashcodes: zwei Objekte. Ohne die künstliche Pause passiert das selten – aber selten heißt nicht nie, und solche Fehler sind extrem schwer zu reproduzieren.

## Lösung 2: `lock` und doppelte Prüfung

Die Reparatur: Prüfung und Erzeugung müssen zusammen **unteilbar** ablaufen. Dafür gibt es in C# die `lock`-Anweisung. Sie sperrt einen Block für alle anderen Threads, bis der aktuelle Thread ihn verlassen hat. Damit nicht jeder Zugriff nach dem ersten die (langsame) Sperre durchlaufen muss, prüft man zweimal – einmal außerhalb und einmal innerhalb des `lock`:

```csharp
private static Konfiguration? instanz;
private static readonly object schloss = new();

public static Konfiguration Instanz
{
    get
    {
        if (instanz == null)                    // schneller Test ohne Sperre
        {
            lock (schloss)
            {
                if (instanz == null)            // nochmal, jetzt geschützt
                    instanz = new Konfiguration();
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
public sealed class Konfiguration
{
    private static readonly Lazy<Konfiguration> halter = new(() => new Konfiguration());

    private Konfiguration() { }

    public static Konfiguration Instanz => halter.Value;
}
```

Das Lambda ist nötig, weil `Lazy<T>` den privaten Konstruktor nicht selbst aufrufen darf – nur Code innerhalb der Klasse kann das. Der gesamte Wettlauf-Code ist verschwunden, und die Verzögerung bleibt erhalten. Noch kürzer geht es, wenn die Instanz nicht unbedingt verzögert entstehen muss:

```csharp
private static readonly Konfiguration instanz = new();
public static Konfiguration Instanz => instanz;
```

Statische Felder initialisiert die Laufzeitumgebung garantiert genau einmal und threadsicher, bevor die Klasse das erste Mal benutzt wird. Der Preis: Die Instanz entsteht schon beim ersten Zugriff auf *irgendein* statisches Mitglied der Klasse, nicht erst bei `Instanz`. In den meisten Fällen ist das egal.

Im Zweifel: `Lazy<T>` statt `lock`. Handgeschriebene Synchronisation ist die häufigste Fehlerquelle bei Singletons, und `Lazy<T>` erledigt sie in einer Zeile korrekt.
{: .notice--primary}

## Beispiel in .NET

Singletons begegnen dir in .NET vor allem als statische Properties, die eine gemeinsam genutzte Instanz liefern: `Random.Shared` ist ein threadsicherer Zufallsgenerator, den sich das ganze Programm teilt, `Comparer<T>.Default` und `EqualityComparer<T>.Default` sind die Standardvergleicher, die wir im Modul [`IComparable<T>` und Sortieren](/modules/icomparable_sortieren/icomparable_sortieren.md) indirekt benutzt haben. In Desktop-Frameworks wie WPF oder MAUI liefert `Application.Current` die eine laufende Anwendung. Ein Konstruktor ist in keinem dieser Fälle von außen erreichbar.

## Vor- und Nachteile

Das Muster hat klare Vorteile gegenüber einer globalen Variable: Die Instanz entsteht erst bei Bedarf, es gibt eine Stelle, die die Erzeugung kontrolliert, und mit `Lazy<T>` ist die Thread-Sicherheit geschenkt. Die Nachteile wiegen aber oft schwerer:

- **Versteckte Abhängigkeiten.** Eine Klasse, die irgendwo in einer Methode `Konfiguration.Instanz` aufruft, verrät in ihrer Signatur nicht, dass sie eine Konfiguration braucht. Wer sie benutzen will, muss den Rumpf lesen.
- **Globaler Zustand.** Jeder kann die Instanz von überall verändern. Fehler, die davon abhängen, in welcher Reihenfolge Klassen auf das Singleton zugreifen, sind kaum zu finden.
- **Schwer testbar.** Ein Unit-Test kann das Singleton nicht durch eine Testversion ersetzen – der Aufruf `Konfiguration.Instanz` steht fest im Code. Ein Test, der eine „Datenbank-Instanz“ braucht, greift dann auf die echte Datenbank zu. Wie man das vermeidet, sehen wir in [Aufgabe 4](/modules/aufgaben_entwurfsmuster/aufgaben_entwurfsmuster.md).
- **Unklarer Lebenszyklus.** Wann wird die Instanz freigegeben? Praktisch nie – sie lebt bis zum Programmende, weil das statische Feld sie festhält (siehe Modul [Garbage Collection](/modules/garbage_collection/garbage_collection.md)).

Im Zweifel: Instanz übergeben statt Singleton. Genau das macht der Geometrieeditor: `FigurenVerwaltung` holt sich ihren Speicher nicht über `JsonFigurSpeicher.Instanz`, sondern bekommt ein `IFigurSpeicher`-Objekt im Konstruktor übergeben. Dass es davon nur eines gibt, entscheidet der Aufrufer – und ein Test kann einen `ArbeitsspeicherFigurSpeicher` hineinreichen. Ein Singleton ist nur dann angebracht, wenn eine zweite Instanz *technisch* falsch wäre, nicht bloß unnötig.
{: .notice--warning}

Übung: Schreibe eine Klasse `Protokoll` als Singleton mit `Lazy<T>`, die Meldungen mit Zeitstempel in einer `List<string>` sammelt. Starte anschließend drei Threads, die je zehn Meldungen schreiben, und prüfe, ob am Ende alle 30 Meldungen in der Liste stehen. Falls nicht: Das Singleton ist zwar threadsicher erzeugt – aber ist auch `List<string>.Add` threadsicher?
{: .notice--info}

## Weitere Quellen

- [`Lazy<T>` – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.lazy-1)
- [Verzögerte Initialisierung – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/framework/performance/lazy-initialization)
- [`lock`-Anweisung – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/statements/lock)
