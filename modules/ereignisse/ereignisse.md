---
title: "Ereignisse"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Ein Sturzsensor in einem Smart-Home-System erkennt, dass jemand gestürzt ist. Daraufhin soll eine Notfall-E-Mail verschickt, eine Push-Nachricht ans Handy gesendet und ein Alarmton ausgelöst werden. Der naive Entwurf ist schnell geschrieben – und hat ein Problem, das erst auffällt, wenn das System wächst:

```csharp
class SturzErkennung
{
    private EMailVersand email = new();
    private PushDienst push = new();
    private Alarmton alarm = new();

    public void SturzErkannt()
    {
        email.NotfallSenden();
        push.Benachrichtigen();
        alarm.Ausloesen();
    }
}
```

Die `SturzErkennung` kennt alle drei Empfänger namentlich, erzeugt sie selbst und ruft sie in fester Reihenfolge auf. Kommt ein vierter Empfänger dazu (eine Protokolldatei etwa), muss der Sensor geändert werden. Soll in der Nacht der Alarmton stumm bleiben, muss der Sensor das entscheiden. Sender und Empfänger sind fest miteinander verdrahtet – dabei sollte der Sensor nur eine Aufgabe haben: Stürze erkennen. **Ereignisse** (*events*) drehen die Abhängigkeit um: Der Sender *veröffentlicht*, dass etwas passiert ist, und wer sich dafür interessiert, *registriert* sich beim Sender. Der Sender kennt seine Empfänger nicht mehr – er kennt nur noch die Signatur, die sie erfüllen müssen.

Im Adventure steckt genau dieses Problem in der Klasse `Spieler`. Wenn der Held einen Schatz aufhebt, soll die Konsole einen Ton ausgeben, die Weboberfläche ihre Punkteanzeige aktualisieren und ein Erfolgssystem mitzählen. Der Spieler darf von keinem dieser drei etwas wissen – `Adventure.Kern` kennt weder `Console` noch Blazor, das war die Abhängigkeitsrichtung aus der [Schichtenarchitektur](/modules/schichten_architektur/schichten_architektur.md).

## Von der Delegatvariablen zum `event`

Technisch ist ein Ereignis ein [Multicast-Delegat](/modules/delegaten/delegaten.md): Empfänger hängen ihre Methoden mit `+=` an, der Sender ruft den Delegaten auf. Man könnte also einfach ein öffentliches Delegatfeld anlegen – aber dann darf *jeder* damit alles tun:

```csharp
public class Spieler : BeweglichesObjekt
{
    public EventHandler<SchatzEventArgs>? SchatzGefunden;   // öffentliches Feld – riskant
}

feld.Spieler.SchatzGefunden = null;                          // löscht alle anderen Zuhörer
feld.Spieler.SchatzGefunden?.Invoke(feld.Spieler, irgendwas); // die Oberfläche jubelt ohne Schatz
```

Das Schlüsselwort `event` vor der Deklaration schließt genau diese beiden Türen: Von außen sind nur noch `+=` und `-=` erlaubt. Zuweisen und Auslösen kann ausschließlich die Klasse, in der das Ereignis deklariert ist:

```csharp
public event EventHandler<SchatzEventArgs>? SchatzGefunden;   // Ereignis

feld.Spieler.SchatzGefunden = null;          // Fehler CS0070: nur += oder -= erlaubt
feld.Spieler.SchatzGefunden?.Invoke(...);    // Fehler CS0070: Aufruf nur innerhalb von Spieler
feld.Spieler.SchatzGefunden += SchatzGemeldet;   // erlaubt
```

Ein Ereignis wird immer von genau *einem* Objekt ausgelöst – dem Spieler. Registrieren können sich *beliebig viele*: die Konsole, die Statusleiste, ein Protokoll.

## Die .NET-Konvention: `EventHandler` und `EventArgs`

Man könnte für jedes Ereignis einen eigenen Delegattyp deklarieren. .NET gibt aber eine feste Form vor, an die sich das ganze Framework hält – und an die man sich halten sollte, damit die eigenen Ereignisse sich wie alle anderen anfühlen:

```csharp
public delegate void EventHandler(object? sender, EventArgs e);
public delegate void EventHandler<TEventArgs>(object? sender, TEventArgs e);
```

Der Rückgabetyp ist immer `void`. Der erste Parameter `sender` ist das Objekt, das das Ereignis ausgelöst hat – so kann eine Behandlungsmethode, die an mehreren Spielern registriert ist, unterscheiden, wer gerade einen Schatz gefunden hat. Der zweite Parameter transportiert die Ereignisdaten. Gibt es keine, verwendet man `EventHandler` und übergibt `EventArgs.Empty`. Gibt es welche, schreibt man eine eigene Klasse, die von `EventArgs` erbt, und nutzt die generische Variante. Im Adventure sind das der gefundene Schatz und der neue Punktestand:

```csharp
public class SchatzEventArgs : EventArgs
{
    public Schatz Schatz { get; }
    public int Punkte { get; }

    public SchatzEventArgs(Schatz schatz, int punkte)
    {
        Schatz = schatz;
        Punkte = punkte;
    }
}
```

Die Properties haben bewusst keinen Setter: Ereignisdaten beschreiben, was passiert *ist*, und sollten von den Empfängern nicht verändert werden – sonst sieht der zweite Zuhörer in der Liste andere Daten als der erste.

## Der Sender: der Spieler

Nun kann `Spieler` sein Ereignis deklarieren und an der einen Stelle auslösen, an der ein Schatz tatsächlich eingesammelt wird:

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

Das `?.Invoke` ist Pflicht: Solange sich niemand registriert hat, ist das Ereignis `null`, und ein direkter Aufruf würde abstürzen – auch ein Spiel ohne Oberfläche muss Schätze einsammeln dürfen. Als `sender` übergibt der Spieler `this`. Wichtig ist die Reihenfolge im Rumpf: Erst wird `Punkte` erhöht, dann wird gemeldet. Ein Empfänger, der `sender` befragt, sieht so bereits den neuen Zustand.

Ausgelöst wird das Ganze durch die Spielregeln, ohne dass dort ein `event` auftaucht. `Schatz.Aufheben` ist die Methode, die `Spielfeld.SpielerZieht` aufruft, sobald der Held über ein `$` läuft:

```csharp
public override string Aufheben(Spieler spieler)
{
    spieler.SchatzEinsammeln(this);
    return $"{spieler.Name} findet einen Schatz im Wert von {Wert}!";
}
```

In größeren Projekten kapselt man das Auslösen zusätzlich in einer Methode mit dem Präfix `On`, die `protected virtual` ist – `protected OnSchatzGefunden(SchatzEventArgs e)`. Der Grund stammt aus [Vorlesung 01](/lectures/01/01.md): Eine abgeleitete Klasse kann die Methode überschreiben und vor oder nach dem Auslösen noch etwas tun, ohne das Ereignis selbst anzufassen. Bei einer `sealed`-Klasse oder einem Ereignis mit genau einer Auslösestelle ist das unnötige Zeremonie.
{: .notice--primary}

## Die Empfänger: Konsole und Oberfläche

Ein Empfänger registriert eine Methode, die exakt zur Signatur von `EventHandler<SchatzEventArgs>` passt. Die Konsolenversion tut das in einer einzigen Zeile, direkt nachdem das Spielfeld gebaut wurde:

```csharp
Spielfeld feld = LevelParser.Parsen(level);

feld.Spieler.SchatzGefunden += (sender, e) => Console.Beep();
```

Der Kern piept nicht selbst – er meldet nur. Dass daraus ein Ton wird, entscheidet `Adventure.Konsole`, und zwar in einem Lambda mit zwei Parametern, dessen Typen der Compiler aus dem Ereignistyp erschließt. Die Weboberfläche wird sich an dasselbe Ereignis hängen, aber etwas völlig anderes tun: Sie lässt die `Statusleiste` neu zeichnen. Wie das in Blazor aussieht, sehen wir im Modul [Observer](/modules/observer/observer.md) in der nächsten Vorlesung. Ein dritter Zuhörer könnte mitschreiben, ohne dass Kern, Konsole oder Web davon erfahren:

```csharp
List<string> erfolge = [];
feld.Spieler.SchatzGefunden += (sender, e) =>
    erfolge.Add($"{((Spieler)sender!).Name}: {e.Schatz.Wert} Punkte, jetzt {e.Punkte}");
```

## Ein zweites Ereignis: `RundeBeendet`

Nicht nur der Spieler meldet sich, auch das Spielfeld. Eine Runde besteht aus dem Zug des Helden und den Zügen aller Gegner; wenn sie vorbei ist, hat sich fast alles auf der Karte verändert, und jede Anzeige muss neu gezeichnet werden:

```csharp
public class RundeEventArgs : EventArgs
{
    public int Runde { get; }
    public string Meldung { get; }

    public RundeEventArgs(int runde, string meldung)
    {
        Runde = runde;
        Meldung = meldung;
    }
}

public class Spielfeld
{
    /// <summary>Wird nach jeder Runde ausgelöst – die Oberfläche zeichnet dann neu.</summary>
    public event EventHandler<RundeEventArgs>? RundeBeendet;

    public void SpielerZieht(Richtung richtung)
    {
        // ... Spieler zieht, Gegner ziehen, Meldung wird zusammengebaut ...
        LetzteMeldung = meldung.ToString().Trim();
        RundeBeendet?.Invoke(this, new RundeEventArgs(Runde, LetzteMeldung));
    }
}
```

`RundeBeendet` steht am **Ende** von `SpielerZieht`, nach der Zuweisung an `LetzteMeldung`. Das ist kein Zufall: Ein Ereignis meldet einen abgeschlossenen Zustand. Würde es mittendrin ausgelöst, sähen die Empfänger ein halb gezogenes Spielfeld, in dem der Held schon steht, die Gegner aber noch nicht. Mehrere Zuhörer nebeneinander sind dabei der Normalfall:

```csharp
feld.RundeBeendet += (sender, e) => Console.WriteLine($"Runde {e.Runde}: {e.Meldung}");
feld.RundeBeendet += (sender, e) => protokoll.Add(e.Meldung);
```

Das Spielfeld hat keine Ahnung, wer da zuhört. Es hat nicht einmal eine Liste von Empfängern – die steckt im Delegaten. Käme morgen eine Klasse `Erfolgsverwaltung` oder ein Netzwerkclient hinzu, müsste an `Spielfeld` nichts geändert werden. Das ist genau die lose Kopplung, die dem Sturzsensor gefehlt hat.

Wer sich registriert, sollte sich auch wieder abmelden. Solange eine Methode im Delegaten des Spielfelds hängt, hält das Spielfeld eine Referenz auf ihr Objekt – und der [Garbage Collector](/modules/garbage_collection/garbage_collection.md) kann es nicht freigeben, selbst wenn es sonst nirgends mehr gebraucht wird. Bei einem langlebigen Sender und vielen kurzlebigen Empfängern (Dialoge, Anzeigen) ist das ein klassisches Speicherleck. Und Vorsicht bei der Konsolenversion, sobald sie in [Vorlesung 09](/lectures/09/09.md) Spielstände laden kann: Nach `F9` entsteht ein *neues* `Spielfeld` mit einem neuen `Spieler`, weshalb `Program.cs` `SchatzGefunden` dort erneut abonnieren muss. Wer stattdessen dasselbe Objekt zweimal abonniert, hört den Ton doppelt – und Lambdas lassen sich nicht mit `-=` abmelden, weil man keine Referenz darauf hat.
{: .notice--warning}

## Der Tastendruck aus Vorlesung 04

Damit ist auch klar, was in [Blazor](/modules/blazor_ereignisse/blazor_ereignisse.md) hinter `@onkeydown="TasteGedrueckt"` steckt. Das Spielfeld-`div` löst ein Tastenereignis aus, und das Razor-Attribut ist nur eine Kurzschreibweise für das, was wir eben von Hand gemacht haben: Blazor registriert unsere Methode als Empfänger. Die Details des Tastendrucks kommen als Parameter:

```csharp
private void TasteGedrueckt(KeyboardEventArgs e)
{
    Richtung? richtung = e.Key switch
    {
        "ArrowUp" or "w" or "W" => Richtung.Oben,
        // ...
        _ => null
    };
    if (richtung is Richtung r)
    {
        feld.SpielerZieht(r);   // danach rendert Blazor die Komponente automatisch neu
    }
}
```

Das `div` ist der Sender, die Komponente der Empfänger, `KeyboardEventArgs` sind die Ereignisdaten – dieselben drei Rollen wie beim Schatz. Einen `sender`-Parameter gibt es in Blazor nicht, und wer die Ereignisdaten nicht braucht, lässt den Parameter einfach weg. Das Muster dahinter – ein Sender veröffentlicht, viele Empfänger registrieren sich – ist so grundlegend, dass es einen eigenen Namen hat: das [Observer-Muster](/modules/observer/observer.md), das wir in Vorlesung 08 als eines der klassischen Entwurfsmuster genauer anschauen.

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v04-blazor`).

Übung: Ergänze `Spielfeld` um ein Ereignis `SpielBeendet` ohne Zusatzdaten (Typ `EventHandler`), das ausgelöst wird, sobald `Status` von `Laeuft` auf `Gewonnen` oder `Verloren` wechselt. Registriere in der Konsolenversion einen Empfänger, der die Endabrechnung ausgibt, und einen zweiten, der den Spielstand automatisch speichert. An welcher Stelle in `SpielerZieht` muss das Ereignis ausgelöst werden, damit `Status` und `LetzteMeldung` beide schon stimmen – und warum darf der Speicher-Empfänger den Sender nicht kennen?
{: .notice--info}

## Weitere Quellen

- [Ereignisse – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/events/)
- [Behandeln und Auslösen von Ereignissen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/events/)
- [EventHandler<TEventArgs>-Delegat – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.eventhandler-1)
- [Ereignisbehandlung in ASP.NET Core Blazor – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/event-handling)
- [Observer – Game Programming Patterns](https://gameprogrammingpatterns.com/observer.html) – das freie Buch erklärt am Beispiel eines Erfolgssystems im Spiel, warum Ereignisse Sender und Empfänger entkoppeln und wo die Grenzen liegen.
- [Game Programming Patterns – das ganze Buch](https://gameprogrammingpatterns.com/) – kostenlos online, mit vielen Mustern, die genau zu unserem Adventure passen.
