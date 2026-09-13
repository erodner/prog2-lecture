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

Hebt der Held im Adventure einen Schatz auf, soll die Konsole einen Ton ausgeben, die Weboberfläche ihre Punkteanzeige aktualisieren und ein Erfolgssystem mitzählen. Würde `Spieler` diese drei direkt aufrufen, wäre `Adventure.Kern` fest mit `Console` und mit Blazor verdrahtet – die Abhängigkeitsrichtung der [Schichtenarchitektur](/modules/schichten_architektur/schichten_architektur.md) wäre umgekehrt, jede neue Anzeige eine Änderung am Spieler, und ohne Oberfläche ließe sich der Kern nicht einmal übersetzen, geschweige denn testen. Das ist dasselbe Problem wie beim Sturzsensor aus dem Ereignisse-Modul, nur im eigenen Code.

## Lösung: Subjekt und Beobachter

Das Muster kehrt die Abhängigkeit um. Zwei Rollen sind beteiligt:

- **Subjekt** (*Subject*, das Beobachtete): kennt eine Liste von Beobachtern und bietet Methoden zum **Registrieren** und **Abmelden**. Ändert sich sein Zustand, geht es die Liste durch und **benachrichtigt** jeden Beobachter.
- **Beobachter** (*Observer*): implementiert ein Interface mit einer Methode, die das Subjekt bei jeder Änderung aufruft. Was der Beobachter damit tut, ist seine Sache.

Das Subjekt kennt seine Beobachter nur über das Interface. In der klassischen Variante, wie sie die Gang of Four beschreibt, schreibt man dieses Interface und die Liste selbst:

```csharp
public interface IBeobachter
{
    void SchatzGefunden(Spieler spieler, Schatz schatz);
}

public class Spieler : BeweglichesObjekt
{
    private readonly List<IBeobachter> beobachter = new();

    public void Registrieren(IBeobachter b) => beobachter.Add(b);
    public void Abmelden(IBeobachter b) => beobachter.Remove(b);

    public void SchatzEinsammeln(Schatz schatz)
    {
        Punkte += schatz.Wert;
        foreach (IBeobachter b in beobachter)      // der Rundruf
            b.SchatzGefunden(this, schatz);
    }
}
```

Die Benachrichtigung steht am Ende von `SchatzEinsammeln`, also *nachdem* die Punkte erhöht wurden: Ein Beobachter soll einen abgeschlossenen Zustand sehen. Die Beobachter selbst sind gewöhnliche Klassen, die das Interface erfüllen:

```csharp
public class Piepser : IBeobachter
{
    public void SchatzGefunden(Spieler spieler, Schatz schatz) => Console.Beep();
}

public class Erfolgsliste : IBeobachter
{
    public List<string> Eintraege { get; } = new();

    public void SchatzGefunden(Spieler spieler, Schatz schatz)
        => Eintraege.Add($"{spieler.Name}: {schatz.Wert} Punkte, jetzt {spieler.Punkte}");
}

Piepser piepser = new();
feld.Spieler.Registrieren(piepser);
feld.Spieler.Registrieren(new Erfolgsliste());
// ... spielen ...
feld.Spieler.Abmelden(piepser);       // ab jetzt still
```

In `Spieler` kommt weder `Piepser` noch `Erfolgsliste` vor. Ein Protokoll morgen ist eine neue Klasse mit `IBeobachter` und ein `Registrieren`-Aufruf – sonst nichts.

## In C#: `event` und Delegat

Genau dieses Gerüst ist in C# in die Sprache eingebaut. Ein `event` *ist* das Observer-Muster: Der Multicast-Delegat ist die Liste der Beobachter, `+=` ist `Registrieren`, `-=` ist `Abmelden`, und `?.Invoke` ist der Rundruf. Das Interface `IBeobachter` wird durch die Delegat-Signatur ersetzt – ein Beobachter muss keine Klasse mehr sein, eine passende Methode genügt. So sieht `Spieler` im Adventure tatsächlich aus:

```csharp
public class Spieler : BeweglichesObjekt
{
    public int Punkte { get; private set; }

    /// <summary>Wird ausgelöst, wenn der Spieler einen Schatz findet – z. B. für die Anzeige.</summary>
    public event EventHandler<SchatzEventArgs>? SchatzGefunden;

    public void SchatzEinsammeln(Schatz schatz)
    {
        Punkte += schatz.Wert;
        SchatzGefunden?.Invoke(this, new SchatzEventArgs(schatz, Punkte));
    }
}
```

| Rolle im Muster | Klassische Variante | C# mit `event` |
| :--- | :--- | :--- |
| Liste der Beobachter | `List<IBeobachter>` | steckt im Multicast-Delegaten |
| Registrieren | `Registrieren(b)` | `+=` |
| Abmelden | `Abmelden(b)` | `-=` |
| Beobachter-Schnittstelle | Interface `IBeobachter` | Signatur `EventHandler<SchatzEventArgs>` |
| Benachrichtigen | `foreach`-Schleife | `?.Invoke(this, e)` |

Das `event`-Schlüsselwort schützt zusätzlich die Liste: Von außen kann niemand alle Beobachter auf einmal löschen oder den Rundruf selbst auslösen – bei der handgeschriebenen Variante müsste man dafür die Liste sorgfältig privat halten. In C# ist die `event`-Variante deshalb fast immer die richtige Wahl; das handgeschriebene Interface lohnt sich nur, wenn ein Beobachter mehrere zusammengehörige Methoden anbieten muss (`Gestartet`, `Geaendert`, `Beendet`) oder das Subjekt seine Beobachter befragen will.

Was das Muster *nicht* regelt: In welcher Reihenfolge die Beobachter aufgerufen werden, ist offiziell undefiniert (bei Delegaten praktisch die Reihenfolge des `+=`), und wenn ein Beobachter eine Exception wirft, bekommen die nach ihm gar nichts mehr mit. Beobachter sollten deshalb kurz sein und nichts tun, was fehlschlagen kann.
{: .notice--primary}

## Zwei Beobachter desselben Ereignisses

Der eigentliche Gewinn zeigt sich, wenn zwei völlig verschiedene Oberflächen an demselben Ereignis hängen. `Adventure.Konsole` registriert sich in einer einzigen Zeile, direkt nachdem das Spielfeld gebaut wurde:

```csharp
Spielfeld feld = LevelParser.Parsen(level);

feld.Spieler.SchatzGefunden += (sender, e) => Console.Beep();
```

In `Adventure.Web` hängen wir uns als zweiten Beobachter an dasselbe Ereignis – und tun etwas ganz anderes: Die Seite merkt sich eine Jubelmeldung und lässt sich neu zeichnen. Bisher braucht `Home.razor` das nicht, weil Blazor eine Komponente nach jedem Tastendruck ohnehin neu rendert – aber eben nur, weil die Komponente den Tastendruck selbst behandelt hat. Kommt die Änderung aus dem Modell (später etwa durch einen Timer oder einen zweiten Spieler), muss die Komponente selbst zuhören und `StateHasChanged()` aufrufen. Diese Ergänzung zu `Home.razor` sieht so aus:

```razor
@implements IDisposable

<Statusleiste Spieler="feld.Spieler" Runde="feld.Runde" Meldung="@jubel" />

@code {
    private Spielfeld feld = null!;
    private string jubel = "";

    private void NeuStarten()
    {
        if (feld is not null) feld.Spieler.SchatzGefunden -= SchatzGemeldet;
        feld = LevelParser.Parsen(LevelQuelle.Laden(levelName));
        feld.Spieler.SchatzGefunden += SchatzGemeldet;
    }

    private void SchatzGemeldet(object? sender, SchatzEventArgs e)
    {
        jubel = $"Schatz im Wert von {e.Schatz.Wert} gefunden – jetzt {e.Punkte} Punkte!";
        StateHasChanged();               // Statusleiste wird mit neuen Parametern gerendert
    }

    public void Dispose() => feld.Spieler.SchatzGefunden -= SchatzGemeldet;
}
```

Zwei Beobachter, ein Subjekt, und `Spieler` kennt weder `Console` noch `StateHasChanged`. Der Unterschied zur Konsole ist nur die Registrierungsform: Hier ist der Beobachter eine *benannte* Methode, und deshalb funktioniert `-=` in `Dispose` und beim Neustart. Ein Lambda wie in `Program.cs` lässt sich nicht mehr abmelden – man hat keine Referenz darauf.

Nicht nur der Spieler ist ein Subjekt. Auch `Spielfeld` meldet sich, wenn eine komplette Runde vorbei ist – also nachdem der Held gezogen ist und alle Gegner nachgezogen sind:

```csharp
public event EventHandler<RundeEventArgs>? RundeBeendet;

public void SpielerZieht(Richtung richtung)
{
    // ... Spieler zieht, Gegner ziehen, Meldung wird zusammengebaut ...
    LetzteMeldung = meldung.ToString().Trim();
    RundeBeendet?.Invoke(this, new RundeEventArgs(Runde, LetzteMeldung));
}
```

Ein Subjekt kann also mehrere Ereignisse anbieten, und ein Beobachter darf sich für mehrere registrieren. Ein Rundenprotokoll hört bei beiden zu, ohne dass Kern, Konsole oder Web davon etwas erfahren:

```csharp
feld.RundeBeendet += (s, e) => protokoll.Add($"Runde {e.Runde}: {e.Meldung}");
feld.Spieler.SchatzGefunden += (s, e) => protokoll.Add($"Schatz! {e.Punkte} Punkte");
```

## Model und View

Der wichtigste Einsatz des Musters ist genau diese Verbindung von Datenmodell und Oberfläche. Desktop-Frameworks wie WPF und MAUI haben dafür ein standardisiertes Interface, `INotifyPropertyChanged`, das aus einem einzigen Ereignis besteht. Eine Modellklasse implementiert es und löst es in jedem Setter aus:

```csharp
public event PropertyChangedEventHandler? PropertyChanged;

public int Punkte
{
    get => punkte;
    set
    {
        punkte = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Punkte)));
    }
}
```

Jeder Setter meldet, *welche* Property sich geändert hat; ein Textfeld, das sich registriert hat, liest daraufhin den neuen Wert – das ist dort die technische Grundlage der Datenbindung, deren Blazor-Gegenstück wir im Modul [Datenbindung in Blazor](/modules/blazor_datenbindung/blazor_datenbindung.md) gesehen haben. Die Sammlung `ObservableCollection<T>` macht dasselbe für Listen: Ihr Ereignis `CollectionChanged` feuert bei jedem `Add` und `Remove`, und eine Anzeige, die zuhört, aktualisiert sich von allein. Für ein `Inventar<T>`, das die Statusleiste von sich aus aktualisieren soll, wäre das die passende Basis.

## `IObservable<T>` und `IObserver<T>`

Neben `event` gibt es in .NET noch eine zweite, dem GoF-Original näher stehende Umsetzung: das Interface-Paar `IObservable<T>` (Subjekt) und `IObserver<T>` (Beobachter). Der Beobachter hat drei Methoden – `OnNext(T wert)` für jede Änderung, `OnError(Exception)` bei einem Fehler und `OnCompleted()`, wenn keine Werte mehr kommen. Das Subjekt bietet `Subscribe(IObserver<T>)` und gibt ein `IDisposable` zurück; wer `Dispose()` darauf aufruft, ist abgemeldet – das Abmelden ist hier also nicht mehr vergessbar, sondern passt zum `using` aus dem Modul [`IDisposable` und `using`](/modules/idisposable_using/idisposable_using.md). Diese Variante ist die Basis von *Reactive Extensions* (Rx) und eignet sich für Datenströme wie Sensorwerte oder Netzwerkpakete, bei denen Fehler und Ende der Übertragung eine Rolle spielen. Für gewöhnliche Benachrichtigungen wie `SchatzGefunden` bleibt `event` das Mittel der Wahl.

## Vor- und Nachteile

Subjekt und Beobachter sind nur über das Interface bzw. die Delegat-Signatur gekoppelt – keine Seite kennt die konkreten Klassen der anderen. Beobachter kommen und gehen zur Laufzeit, und ein Subjekt lässt sich isoliert testen, indem der Test selbst zuhört: Genau das macht `SpielfeldTests`, wenn es `f.Spieler.SchatzGefunden += (s, e) => punkte = e.Punkte;` registriert und danach prüft, ob die Truhe 100 Punkte gemeldet hat.

Die Nachteile folgen aus derselben Unsichtbarkeit. Wer `spieler.SchatzEinsammeln(schatz)` liest, sieht nicht, dass dahinter fünf Beobachter loslaufen – und wenn einer davon selbst wieder ein Subjekt ist, entsteht eine Kaskade von Benachrichtigungen, die schwer zu durchschauen und bei einem Zyklus endlos ist. Bei vielen Beobachtern und häufigen Ereignissen kostet der Rundruf spürbar Zeit.

Das Subjekt hält Referenzen auf alle registrierten Beobachter. Ein Beobachter, der sich nie mit `-=` abmeldet, wird vom [Garbage Collector](/modules/garbage_collection/garbage_collection.md) nicht freigegeben, solange das Subjekt lebt – bei einem langlebigen Subjekt und vielen kurzlebigen Beobachtern (Dialoge, Anzeigen, Browsersitzungen) ist das ein klassisches Speicherleck. Im Adventure sieht man das ab [Vorlesung 09](/lectures/09/09.md): `Program.cs` registriert nach `F9` einen zweiten Lambda-Beobachter am neuen Spieler, weil sich der alte weder abmelden lässt noch abgemeldet werden muss; würde dabei versehentlich derselbe Spieler zweimal abonniert, piepste die Konsole doppelt. Deshalb: Wer sich mit einer benannten Methode registriert, kann sich abmelden. Wer sich mit einem Lambda registriert, kann es nicht.
{: .notice--warning}

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v04-blazor`).

Übung: Schreibe eine Klasse `Rundenprotokoll`, die sich bei `Spielfeld.RundeBeendet` *und* bei `Spieler.SchatzGefunden` registriert und beide Meldungen in einer gemeinsamen Liste sammelt. Baue sie zuerst in der klassischen Variante mit einem Interface `IBeobachter` und dann mit `event` – welche Variante braucht mehr Code, und welche kommt ohne Änderung an `Spielfeld` aus? Ergänze anschließend eine Methode `Abmelden()`, die beide Registrierungen wieder löst, und überlege, warum sie mit Lambdas nicht funktioniert.
{: .notice--info}

## Weitere Quellen

- [Beobachterentwurfsmuster – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/events/observer-design-pattern)
- [`INotifyPropertyChanged`-Schnittstelle – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.componentmodel.inotifypropertychanged)
- [`IObservable<T>`-Schnittstelle – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.iobservable-1)
- [Observer – Game Programming Patterns](https://gameprogrammingpatterns.com/observer.html) – baut das Muster am Beispiel eines Erfolgssystems auf und diskutiert ausführlich, wann es zu langsam oder zu unübersichtlich wird.
- [Observer – Refactoring.Guru](https://refactoring.guru/design-patterns/observer) – Rollen, Diagramme und C#-Code der klassischen Variante mit Interface und Liste.
