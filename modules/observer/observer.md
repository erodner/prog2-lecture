---
title: "Observer"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Eine Zeitung weiß nicht, wer sie liest. Sie hat eine Liste von Abonnenten, und wenn eine neue Ausgabe erscheint, bekommt jeder auf der Liste ein Exemplar – wer kündigt, wird gestrichen, wer neu abonniert, kommt dazu. Die Zeitung muss dafür weder wissen, was die Leser mit dem Blatt tun, noch muss sie geändert werden, wenn ein neuer Leser hinzukommt. Das **Observer**-Muster (Beobachter, Verhaltensmuster) überträgt genau dieses Abonnement-Prinzip auf Objekte: Ein Objekt ändert seinen Zustand, und alle, die sich dafür angemeldet haben, erfahren davon. Wir haben das Muster im Modul [Ereignisse](/modules/ereignisse/ereignisse.md) schon *benutzt* – jetzt schauen wir es uns als Entwurfsmuster an und sehen, was C# uns dabei abnimmt.

## Problem

Ändert sich der Zustand eines Objekts, sollen andere Objekte darauf reagieren – ohne dass das erste Objekt diese anderen kennt. Der Klassiker ist ein Datenmodell mit mehreren Ansichten: Eine Messstation liefert eine neue Temperatur, und eine Digitalanzeige, ein Diagramm und ein Alarmmodul sollen sich aktualisieren. Wenn die Messstation die drei direkt aufruft, ist sie fest mit ihnen verdrahtet: Jede neue Ansicht bedeutet eine Änderung an der Messstation, und ohne die Ansichten lässt sich die Messstation nicht einmal kompilieren – geschweige denn testen. Das war das Problem des Sturzsensors aus dem Ereignisse-Modul.

## Lösung: Subjekt und Beobachter

Das Muster kehrt die Abhängigkeit um. Zwei Rollen sind beteiligt:

- **Subjekt** (*Subject*, das Beobachtete): kennt eine Liste von Beobachtern und bietet Methoden zum **Registrieren** und **Abmelden**. Ändert sich sein Zustand, geht es die Liste durch und **benachrichtigt** jeden Beobachter.
- **Beobachter** (*Observer*): implementiert ein Interface mit einer Methode, die das Subjekt bei jeder Änderung aufruft. Was der Beobachter damit tut, ist seine Sache.

Das Subjekt kennt seine Beobachter nur über das Interface. In der klassischen Variante schreibt man dieses Interface und die Liste selbst:

```csharp
public interface IBeobachter
{
    void Aktualisieren(double temperatur);
}

public class Messstation
{
    private readonly List<IBeobachter> beobachter = new();
    private double temperatur;

    public void Registrieren(IBeobachter b) => beobachter.Add(b);
    public void Abmelden(IBeobachter b) => beobachter.Remove(b);

    public double Temperatur
    {
        get => temperatur;
        set
        {
            temperatur = value;
            foreach (IBeobachter b in beobachter)
                b.Aktualisieren(temperatur);
        }
    }
}
```

Die Benachrichtigung steckt im Setter: Jede Zuweisung an `Temperatur` löst einen Rundruf aus. Die Beobachter sind gewöhnliche Klassen, die das Interface erfüllen:

```csharp
public class Anzeige : IBeobachter
{
    public void Aktualisieren(double t) => Console.WriteLine($"Anzeige: {t:F1} °C");
}

public class Frostalarm : IBeobachter
{
    public void Aktualisieren(double t)
    {
        if (t < 0) Console.WriteLine("ALARM: Frost!");
    }
}

Messstation station = new();
Anzeige anzeige = new();
station.Registrieren(anzeige);
station.Registrieren(new Frostalarm());

station.Temperatur = 12.5;   // Anzeige: 12,5 °C
station.Temperatur = -2.0;   // Anzeige: -2,0 °C
                             // ALARM: Frost!
station.Abmelden(anzeige);
station.Temperatur = 3.0;    // (nichts – der Frostalarm schweigt über 0 °C)
```

Die Messstation enthält keine Zeile, in der `Anzeige` oder `Frostalarm` vorkommt. Ein `Diagramm` morgen ist eine neue Klasse mit `IBeobachter` und ein `Registrieren`-Aufruf – sonst nichts.

## In C#: `event` und Delegat

Wer die Messstation mit dem Café-Beispiel aus dem Modul [Ereignisse](/modules/ereignisse/ereignisse.md) vergleicht, erkennt dieselbe Struktur. Und tatsächlich ist ein `event` nichts anderes als das Observer-Muster mit eingebauter Sprachunterstützung: Der Multicast-Delegat *ist* die Liste der Beobachter, `+=` ist `Registrieren`, `-=` ist `Abmelden`, und `?.Invoke` ist der Rundruf. Das Interface `IBeobachter` wird durch die Delegat-Signatur ersetzt – ein Beobachter muss keine Klasse mehr sein, eine passende Methode genügt:

```csharp
public class Messstation
{
    private double temperatur;

    public event EventHandler<double>? TemperaturGeaendert;

    public double Temperatur
    {
        get => temperatur;
        set
        {
            temperatur = value;
            TemperaturGeaendert?.Invoke(this, temperatur);
        }
    }
}

Messstation station = new();
station.TemperaturGeaendert += (s, t) => Console.WriteLine($"Anzeige: {t:F1} °C");
station.TemperaturGeaendert += (s, t) => { if (t < 0) Console.WriteLine("ALARM: Frost!"); };
station.Temperatur = -2.0;
// Anzeige: -2,0 °C
// ALARM: Frost!
```

Die Liste, die `Registrieren`- und die `Abmelden`-Methode sind verschwunden; sie stecken im `event`. Das `event`-Schlüsselwort schützt außerdem die Liste: Von außen kann niemand alle Beobachter auf einmal löschen oder den Rundruf selbst auslösen. In C# ist die `event`-Variante immer die richtige Wahl; das handgeschriebene Interface lohnt sich nur, wenn ein Beobachter mehrere Methoden anbieten muss oder das Subjekt seine Beobachter befragen will.

Der Vergleich mit dem Setter zeigt auch, was das Muster *nicht* regelt: In welcher Reihenfolge die Beobachter aufgerufen werden, ist offiziell undefiniert (bei Delegaten praktisch die Reihenfolge des `+=`), und wenn ein Beobachter eine Exception wirft, bekommen die nach ihm gar nichts mehr mit.
{: .notice--primary}

## Model und View

Der wichtigste Einsatz des Musters ist die Verbindung von Datenmodell und Oberfläche. Im Geometrieeditor aus Vorlesung 04 rendert Blazor die Seite nach jedem Klick von selbst neu – aber nur, wenn die Komponente den Klick selbst behandelt hat. Ändert sich das Modell von außen, muss die Oberfläche davon erfahren. Mit Observer wird die Richtung umgedreht: Das Modell meldet, dass sich etwas geändert hat, und die Oberfläche hört zu. Desktop-Frameworks wie WPF und MAUI haben dafür ein standardisiertes Interface, `INotifyPropertyChanged`, das aus genau einem Ereignis besteht:

```csharp
public class FigurModell : INotifyPropertyChanged
{
    private string name = "";

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Name
    {
        get => name;
        set
        {
            name = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
        }
    }
}
```

Jeder Setter meldet, *welche* Property sich geändert hat. Ein Textfeld in WPF kann sich für `PropertyChanged` registrieren und liest dann `Name` neu – das ist dort die technische Grundlage der Datenbindung. In Blazor sieht das Gegenstück so aus: Die Komponente registriert sich für ein Ereignis des Modells und ruft im Handler `StateHasChanged()` auf, woraufhin Blazor sie neu rendert – siehe Modul [Datenbindung in Blazor](/modules/blazor_datenbindung/blazor_datenbindung.md). Die Sammlung `ObservableCollection<T>` macht dasselbe für Listen: Ihr Ereignis `CollectionChanged` feuert bei jedem `Add` und `Remove`, und eine Komponente, die zuhört, aktualisiert sich von allein.

## `IObservable<T>` und `IObserver<T>`

Neben `event` gibt es in .NET noch eine zweite, dem GoF-Original näher stehende Umsetzung: das Interface-Paar `IObservable<T>` (Subjekt) und `IObserver<T>` (Beobachter). Der Beobachter hat drei Methoden – `OnNext(T wert)` für jede Änderung, `OnError(Exception)` bei einem Fehler und `OnCompleted()`, wenn keine Werte mehr kommen. Das Subjekt bietet `Subscribe(IObserver<T>)` und gibt ein `IDisposable` zurück; wer `Dispose()` darauf aufruft, ist abgemeldet. Diese Variante ist die Basis von *Reactive Extensions* (Rx) und eignet sich für Datenströme wie Sensorwerte oder Netzwerkpakete, bei denen Fehler und Ende der Übertragung eine Rolle spielen. Für gewöhnliche Benachrichtigungen bleibt `event` das Mittel der Wahl.

## Vor- und Nachteile

Subjekt und Beobachter sind nur über das Interface bzw. die Delegat-Signatur gekoppelt – keine Seite kennt die konkreten Klassen der anderen. Beobachter kommen und gehen zur Laufzeit, und ein Subjekt lässt sich isoliert testen, indem der Test selbst zuhört. Diese lose Kopplung ist der Grund, warum das Muster in jeder GUI-Bibliothek steckt.

Die Nachteile folgen aus derselben Unsichtbarkeit. Wer `station.Temperatur = 3` liest, sieht nicht, dass dahinter fünf Beobachter loslaufen – und wenn einer davon selbst wieder ein Subjekt ist, entsteht eine Kaskade von Benachrichtigungen, die schwer zu durchschauen und bei einem Zyklus endlos ist. Bei vielen Beobachtern und häufigen Änderungen kostet der Rundruf spürbar Zeit.

Das Subjekt hält Referenzen auf alle registrierten Beobachter. Ein Beobachter, der sich nie mit `-=` abmeldet, wird vom [Garbage Collector](/modules/garbage_collection/garbage_collection.md) nicht freigegeben, solange das Subjekt lebt – bei einem langlebigen Subjekt (Hauptfenster, Sensor, Singleton) und vielen kurzlebigen Beobachtern (Dialoge, Listeneinträge) ist das ein klassisches Speicherleck. Deshalb: Wer sich mit einer benannten Methode registriert, kann sich abmelden. Wer sich mit einem Lambda registriert, kann es nicht.
{: .notice--warning}

Übung: Die Klasse `FigurenVerwaltung` des Geometrieeditors soll ein Ereignis `FigurenGeaendert` bekommen, das bei `Hinzufuegen`, `Entfernen` und `Laden` ausgelöst wird. Passe `Home.razor` so an, dass die Komponente sich in `OnInitialized` registriert und im Handler `StateHasChanged()` aufruft. Welche Aufrufe im `@code`-Block werden dadurch überflüssig – und wo muss `-=` stehen?
{: .notice--info}

## Weitere Quellen

- [Beobachterentwurfsmuster – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/events/observer-design-pattern)
- [`INotifyPropertyChanged`-Schnittstelle – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.componentmodel.inotifypropertychanged)
- [`IObservable<T>`-Schnittstelle – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.iobservable-1)
