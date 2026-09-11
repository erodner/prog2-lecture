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

Die `SturzErkennung` kennt alle drei Empfänger namentlich, erzeugt sie selbst und ruft sie in fester Reihenfolge auf. Kommt ein vierter Empfänger dazu (eine Protokolldatei etwa), muss der Sensor geändert werden. Soll in der Nacht der Alarmton stumm bleiben, muss der Sensor das entscheiden. Sender und Empfänger sind fest miteinander verdrahtet – dabei sollte der Sensor nur eine Aufgabe haben: Stürze erkennen. **Ereignisse** (*events*) drehen die Abhängigkeit um: Der Sender *veröffentlicht*, dass etwas passiert ist, und wer sich dafür interessiert, *registriert* sich beim Sender. Der Sender kennt seine Empfänger nicht mehr – er kennt nur noch die Signatur, die sie erfüllen müssen. Das ist die Idee des Observer-Musters, und Ereignisse sind die eingebaute C#-Umsetzung davon.

## Von der Delegatvariablen zum `event`

Technisch ist ein Ereignis ein [Multicast-Delegat](/modules/delegaten/delegaten.md): Empfänger hängen ihre Methoden mit `+=` an, der Sender ruft den Delegaten auf. Man könnte also einfach ein öffentliches Delegatfeld anlegen – aber dann darf *jeder* damit alles tun:

```csharp
class Cafe
{
    public EventHandler? FreirundeAngekuendigt;   // öffentliches Feld – riskant
}

Cafe cafe = new();
cafe.FreirundeAngekuendigt = null;                        // löscht alle anderen Gäste
cafe.FreirundeAngekuendigt?.Invoke(cafe, EventArgs.Empty); // ein Gast kündigt selbst eine Freirunde an
```

Das Schlüsselwort `event` vor der Deklaration schließt genau diese beiden Türen: Von außen sind nur noch `+=` und `-=` erlaubt. Zuweisen und Auslösen kann ausschließlich die Klasse, in der das Ereignis deklariert ist:

```csharp
class Cafe
{
    public event EventHandler? FreirundeAngekuendigt;   // Ereignis
}

cafe.FreirundeAngekuendigt = null;          // Fehler CS0070: nur += oder -= erlaubt
cafe.FreirundeAngekuendigt?.Invoke(...);    // Fehler CS0070: Aufruf nur innerhalb von Cafe
cafe.FreirundeAngekuendigt += FreirundeEmpfangen;   // erlaubt
```

Ein Ereignis wird immer von genau *einem* Objekt ausgelöst – dem Café. Registrieren können sich *beliebig viele* – alle Gäste an den Tischen.

## Die .NET-Konvention: `EventHandler` und `EventArgs`

Man könnte für jedes Ereignis einen eigenen Delegattyp deklarieren. .NET gibt aber eine feste Form vor, an die sich das ganze Framework hält – und an die man sich halten sollte, damit die eigenen Ereignisse sich wie alle anderen anfühlen:

```csharp
public delegate void EventHandler(object? sender, EventArgs e);
public delegate void EventHandler<TEventArgs>(object? sender, TEventArgs e);
```

Der Rückgabetyp ist immer `void`. Der erste Parameter `sender` ist das Objekt, das das Ereignis ausgelöst hat – so kann eine Behandlungsmethode, die an mehreren Cafés registriert ist, unterscheiden, in welchem gerade eine Freirunde angekündigt wird. Der zweite Parameter transportiert die Ereignisdaten. Gibt es keine, verwendet man `EventHandler` und übergibt `EventArgs.Empty`. Gibt es welche, schreibt man eine eigene Klasse, die von `EventArgs` erbt, und nutzt die generische Variante. Das Café soll ein bestimmtes Getränk gratis ausgeben:

```csharp
enum GetraenkArt { Heiss, Kalt }

class FreirundeEventArgs : EventArgs
{
    public GetraenkArt Art { get; }

    public FreirundeEventArgs(GetraenkArt art)
    {
        Art = art;
    }
}
```

Die Property hat bewusst keinen Setter: Ereignisdaten beschreiben, was passiert *ist*, und sollten von den Empfängern nicht verändert werden – sonst sieht der nächste Gast in der Liste andere Daten als der erste.

## Der Sender: das Café

Nun kann das Café sein Ereignis mit dem passenden Typ deklarieren und auslösen. Das Auslösen kapselt man in einer geschützten Methode mit dem Präfix `On`:

```csharp
class Cafe
{
    public event EventHandler<FreirundeEventArgs>? FreirundeAngekuendigt;

    public void FreirundeAusgeben(GetraenkArt art)
    {
        Console.WriteLine($"Café: Eine Freirunde {art} für alle!");
        OnFreirundeAngekuendigt(new FreirundeEventArgs(art));
    }

    protected virtual void OnFreirundeAngekuendigt(FreirundeEventArgs e)
    {
        FreirundeAngekuendigt?.Invoke(this, e);
    }
}
```

Das `?.Invoke` ist Pflicht: Solange sich niemand registriert hat, ist das Ereignis `null`, und ein direkter Aufruf würde abstürzen – ein leeres Café soll trotzdem eine Freirunde ausgeben dürfen. Als `sender` übergibt das Café `this`. Dass `OnFreirundeAngekuendigt` `protected virtual` ist, hat einen Grund aus [Vorlesung 01](/lectures/01/01.md): Eine abgeleitete Klasse `Eiscafe` kann die Methode überschreiben und vor oder nach dem Auslösen noch etwas tun, ohne das Ereignis selbst anzufassen.

## Die Empfänger: die Gäste

Ein Gast registriert sich beim Betreten und meldet sich beim Verlassen wieder ab. Die Behandlungsmethode muss exakt zur Signatur von `EventHandler<FreirundeEventArgs>` passen:

```csharp
class Gast
{
    public string Name { get; }

    public Gast(string name)
    {
        Name = name;
    }

    public void Betreten(Cafe cafe) => cafe.FreirundeAngekuendigt += FreirundeEmpfangen;
    public void Verlassen(Cafe cafe) => cafe.FreirundeAngekuendigt -= FreirundeEmpfangen;

    private void FreirundeEmpfangen(object? sender, FreirundeEventArgs e)
    {
        string reaktion = e.Art == GetraenkArt.Heiss ? "nimmt gern einen Cappuccino" : "nimmt gern eine Limo";
        Console.WriteLine($"{Name} {reaktion}.");
    }
}
```

Auch hier steht `FreirundeEmpfangen` ohne Klammern hinter `+=` – es wird die Methode registriert, nicht ihr Ergebnis. `FreirundeEmpfangen` darf `private` sein: Das Café ruft sie nicht über ihren Namen auf, sondern über den Delegaten, und der hat die Referenz beim Registrieren bekommen. Jetzt das Zusammenspiel:

```csharp
Cafe cafe = new();
Gast anna = new("Anna");
Gast ben = new("Ben");

anna.Betreten(cafe);
ben.Betreten(cafe);
cafe.FreirundeAusgeben(GetraenkArt.Heiss);
// Café: Eine Freirunde Heiss für alle!
// Anna nimmt gern einen Cappuccino.
// Ben nimmt gern einen Cappuccino.

anna.Verlassen(cafe);
cafe.FreirundeAusgeben(GetraenkArt.Kalt);
// Café: Eine Freirunde Kalt für alle!
// Ben nimmt gern eine Limo.
```

Das Café hat keine Ahnung, wer Anna und Ben sind. Es hat nicht einmal eine Liste von Gästen – die steckt im Delegaten. Käme morgen eine Klasse `Stammgast` oder `Cafekatze` hinzu, die auf Freirunden reagiert, müsste an `Cafe` nichts geändert werden. Das ist genau die lose Kopplung, die dem Sturzsensor gefehlt hat.

Wer sich registriert, sollte sich auch wieder abmelden. Solange `ben.FreirundeEmpfangen` im Delegaten des Cafés hängt, hält das Café eine Referenz auf `ben` – und der [Garbage Collector](/modules/garbage_collection/garbage_collection.md) kann das Objekt nicht freigeben, selbst wenn es sonst nirgends mehr gebraucht wird. Bei langlebigen Sendern (ein Hauptfenster, ein Sensor, der die ganze Programmlaufzeit existiert) und vielen kurzlebigen Empfängern ist das ein klassisches Speicherleck. Lambdas lassen sich nicht mit `-=` abmelden, weil man keine Referenz darauf hat – für Ereignisse, von denen man sich wieder trennen will, sind benannte Methoden die richtige Wahl.
{: .notice--warning}

## Der `Click`-Handler aus Vorlesung 04

Damit ist auch klar, was in [Blazor](/modules/blazor_ereignisse/blazor_ereignisse.md) hinter `@onclick="Begruessen"` steckt. Der Button löst ein Klick-Ereignis aus, und das Razor-Attribut ist nur eine Kurzschreibweise für das, was wir eben von Hand gemacht haben: Blazor registriert unsere Methode als Empfänger. Wer die Details des Klicks braucht, nimmt sie als Parameter entgegen:

```csharp
// in Home.razor: <button @onclick="Begruessen">Begrüßen</button>

private void Begruessen(MouseEventArgs e)
{
    // e enthält Details zum Klick, etwa die Mausposition
}
```

Der Button ist das Café, die Komponente ist der Gast, `MouseEventArgs` sind die Getränkedaten. Einen `sender`-Parameter gibt es in Blazor nicht, und wer die Ereignisdaten nicht braucht, lässt den Parameter einfach weg – so wie in Vorlesung 04. Das Muster dahinter – ein Sender veröffentlicht, viele Empfänger registrieren sich – ist so grundlegend, dass es einen eigenen Namen hat: das [Observer-Muster](/modules/observer/observer.md), das wir in Vorlesung 08 als eines der klassischen Entwurfsmuster genauer anschauen.

Übung: Ergänze das Café um ein zweites Ereignis `Ladenschluss` ohne Zusatzdaten (Typ `EventHandler`). Schreibe eine Klasse `Taxizentrale`, die sich für `Ladenschluss` registriert und beim Auslösen die Namen aller Gäste ausgibt, die sich zuvor bei *ihr* angemeldet haben. Was muss die Taxizentrale wissen, was das Café nicht wissen darf?
{: .notice--info}

## Weitere Quellen

- [Ereignisse – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/events/)
- [Behandeln und Auslösen von Ereignissen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/events/)
- [EventHandler<TEventArgs>-Delegat – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.eventhandler-1)
- [Ereignisbehandlung in ASP.NET Core Blazor – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/event-handling)
