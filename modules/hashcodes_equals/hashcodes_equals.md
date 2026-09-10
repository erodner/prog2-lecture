---
title: "Hashcodes und Equals – wie ein Dictionary seine Schlüssel findet"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Ein Bibliothekar, der ein Buch sucht, geht nicht Regal für Regal durch – er schaut auf die Signatur, weiß dadurch sofort, in welchem Regal das Buch steht, und muss nur noch in diesem einen Regal nachsehen. Genau nach diesem Prinzip arbeitet ein `Dictionary<K, V>`: Aus dem Schlüssel wird eine Zahl berechnet, die direkt sagt, in welchem „Regal“ der Wert liegt. Deshalb ist der Zugriff so schnell, egal ob zehn oder zehn Millionen Einträge gespeichert sind. Aus [Programmierung 1](https://www.erodner.de/prog-lecture/modules/dictionary/dictionary/) kennen wir das `Dictionary` als Werkzeug – in diesem Modul schauen wir hinein, denn sobald wir eine **eigene Klasse als Schlüssel** verwenden, funktioniert das Werkzeug ohne dieses Wissen nicht mehr.

## Assoziative Arrays

Ein Array kennt nur ganzzahlige Indizes von `0` bis `Length - 1`. Ein `Dictionary` verallgemeinert diese Idee: Der „Index“ darf ein beliebiger Typ sein – ein `string`, ein `int`, ein `DateTime` oder eine eigene Klasse. Weil Schlüssel und Wert fest zusammengehören (assoziiert sind), spricht man von einem **assoziativen Array**.

```csharp
Dictionary<string, int> mitarbeiterNummern = new Dictionary<string, int>();
mitarbeiterNummern.Add("Schroedinger", 88);
mitarbeiterNummern["Heisenberg"] = 42;

Console.WriteLine(mitarbeiterNummern["Schroedinger"]); // 88
Console.WriteLine(mitarbeiterNummern.ContainsKey("Bohr")); // False
```

Von außen sieht das aus wie ein Array mit Strings als Index. Wie aber kommt das Dictionary von `"Schroedinger"` zu einer Speicherstelle, ohne alle Einträge zu vergleichen?

## Wie eine Hashtabelle funktioniert

Intern besitzt ein `Dictionary` ein Array von **Buckets** (Behältern). Beim Einfügen und beim Nachschlagen passiert dreimal dasselbe:

1. Aus dem Schlüssel wird mit einer **Hashfunktion** eine ganze Zahl berechnet, der **Hashcode**.
2. Der Hashcode wird mit `Modulo Anzahl der Buckets` auf einen gültigen Array-Index abgebildet.
3. In diesem Bucket wird der Eintrag abgelegt beziehungsweise gesucht.

Eine Hashfunktion bildet eine riesige Eingabemenge (alle möglichen Strings) auf eine kleine Zielmenge (die `int`-Werte) ab. Zwangsläufig bekommen dabei manchmal zwei verschiedene Schlüssel denselben Hashcode – das nennt man eine **Kollision**. Das `Dictionary` löst Kollisionen durch **Verkettung**: Jeder Bucket kann mehrere Einträge aufnehmen, die wie in einer kurzen verketteten Liste hintereinanderhängen. Beim Nachschlagen wird also zuerst über den Hashcode der Bucket bestimmt und dann innerhalb des Buckets mit `Equals` der wirklich passende Schlüssel gesucht.

Zwei Regeln muss eine brauchbare Hashfunktion erfüllen: Gleiche Schlüssel liefern **immer** denselben Hashcode, und verschiedene Schlüssel liefern **möglichst oft** verschiedene Hashcodes. Die erste Regel ist Pflicht, die zweite bestimmt die Geschwindigkeit – wenn alle Schlüssel im selben Bucket landen, ist aus dem Dictionary eine langsame Liste geworden.
{: .notice--primary}

## Was `GetHashCode()` liefert

Die Hashfunktion ist in .NET keine Zauberei, sondern die Methode `GetHashCode()`, die jede Klasse von [`object`](/modules/object_basisklasse/object_basisklasse.md) erbt. Was sie zurückgibt, hängt davon ab, wer sie überschrieben hat:

```csharp
Console.WriteLine(42.GetHashCode());        // 42
Console.WriteLine("Hallo".GetHashCode());   // z. B. -1327847390 (inhaltsbasiert)
Console.WriteLine("Hallo".GetHashCode() == "Hallo".GetHashCode()); // True

object a = new object();
object b = new object();
Console.WriteLine(a.GetHashCode() == b.GetHashCode()); // False
```

Für **Werttypen** wie `int` oder eigene `struct`s berechnet .NET den Hashcode aus den Feldwerten – zwei Strukturen mit gleichem Inhalt haben denselben Hashcode. Der eingebaute Referenztyp **`string`** überschreibt `GetHashCode` ebenfalls inhaltsbasiert, weshalb zwei gleiche Zeichenketten denselben Bucket finden. Für **eigene Klassen** dagegen gilt die Standardimplementierung von `object`: Der Hashcode hängt an der Objektidentität. Zwei Objekte, die inhaltlich gleich sind, aber an verschiedenen Stellen im Speicher liegen, bekommen verschiedene Hashcodes.

## Das Problem: eine eigene Klasse als Schlüssel

Genau hier lauert die Falle. Angenommen, wir modellieren einen Schlüssel als kleine Klasse mit einer `Id`:

```csharp
public class Schluessel
{
    public int Id { get; }

    public Schluessel(int id)
    {
        Id = id;
    }
}
```

Nun legen wir mit einer Instanz etwas ab und versuchen, es mit einer zweiten, inhaltlich gleichen Instanz wiederzufinden:

```csharp
Dictionary<Schluessel, string> eintraege = new Dictionary<Schluessel, string>();

Schluessel schluessel1 = new Schluessel(5);
eintraege.Add(schluessel1, "Hallo");

Schluessel schluessel2 = new Schluessel(5);
string wert = eintraege[schluessel2]; // KeyNotFoundException!
```

Aus unserer Sicht sind `schluessel1` und `schluessel2` derselbe Schlüssel – beide haben die `Id` 5. Aus Sicht des Dictionaries sind es zwei Fremde: `schluessel2.GetHashCode()` liefert einen anderen Wert als `schluessel1.GetHashCode()`, das Dictionary schaut in den falschen Bucket, findet nichts und wirft die Exception. Selbst wenn beide zufällig im selben Bucket landen würden, scheiterte der zweite Schritt: `Equals` vergleicht standardmäßig ebenfalls nur die Identität.

## Die Lösung: `Equals` und `GetHashCode` überschreiben

Damit das Dictionary unsere Vorstellung von Gleichheit teilt, müssen wir ihm beide Schritte beibringen – die Bucket-Wahl über `GetHashCode` und den Vergleich innerhalb des Buckets über `Equals`:

```csharp
public class Schluessel
{
    public int Id { get; }

    public Schluessel(int id)
    {
        Id = id;
    }

    public override bool Equals(object? obj)
    {
        return obj is Schluessel anderer && Id == anderer.Id;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id);
    }
}
```

`obj is Schluessel anderer` prüft den Typ und legt in einem Schritt die Variable `anderer` an – für `null` oder fremde Typen ergibt der Ausdruck `false`. `HashCode.Combine` ist die empfohlene Art, aus einem oder mehreren Feldern einen gut verteilten Hashcode zu erzeugen; bei mehreren Feldern schreibt man einfach `HashCode.Combine(Id, Name)`. Mit dieser Version findet `eintraege[schluessel2]` das `"Hallo"`.

Der **Vertrag** zwischen beiden Methoden ist nicht verhandelbar: Wenn `a.Equals(b)` `true` liefert, **muss** `a.GetHashCode() == b.GetHashCode()` gelten. Die Umkehrung ist nicht gefordert – gleiche Hashcodes bei ungleichen Objekten sind eine erlaubte Kollision. Wer nur `Equals` überschreibt, verletzt den Vertrag, und das Dictionary schaut weiterhin in den falschen Bucket. Der Compiler warnt in diesem Fall (CS0659).
{: .notice--warning}

## Schlüssel sollten unveränderlich sein

In `Schluessel` hat `Id` bewusst nur einen Getter. Der Grund: Der Bucket wird beim Einfügen aus dem damaligen Hashcode bestimmt. Änderte sich `Id` später, würde `GetHashCode` einen anderen Wert liefern, das Dictionary suchte im neuen Bucket – der Eintrag liegt aber noch im alten und ist damit praktisch verloren. Felder, die in `Equals` und `GetHashCode` eingehen, sollten deshalb nach der Konstruktion nicht mehr veränderbar sein. Für reine Datenträger wie `Schluessel` bietet C# übrigens `record`-Typen an, die `Equals` und `GetHashCode` automatisch inhaltsbasiert erzeugen – wir bleiben hier bei der ausgeschriebenen Variante, damit sichtbar ist, was passiert.

Übung: Schreibe eine Klasse `Koordinate` mit `X` und `Y` und verwende sie als Schlüssel in einem `Dictionary<Koordinate, string>`, das Ortsnamen speichert. Überschreibe `Equals` und `GetHashCode` und prüfe, dass `new Koordinate(52, 13)` den Eintrag findet, der mit einer anderen Instanz derselben Werte abgelegt wurde. Was passiert, wenn du `GetHashCode` konstant `0` zurückgeben lässt – funktioniert es noch?
{: .notice--info}

## Weitere Quellen

- [Dictionary<TKey,TValue> – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.collections.generic.dictionary-2)
- [Object.GetHashCode – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.object.gethashcode)
- [HashCode.Combine – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.hashcode.combine)
- [Gleichheitsvergleiche – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/statements-expressions-operators/equality-comparisons)
