---
title: "Remote-Repositorys"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Bis jetzt lebt unser Repository nur auf einem Rechner. Das schützt vor versehentlichen Änderungen, aber nicht vor einer kaputten Festplatte – und es hilft nicht beim Arbeiten im Team. Dafür braucht es ein zweites Repository an einem Ort, den alle erreichen: ein **Remote**. In [Git-Konzepte](/modules/git_konzepte/git_konzepte.md) haben wir gesehen, dass Git dezentral ist – ein Remote ist technisch nichts Besonderes, nur ein weiteres vollständiges Repository, mit dem man Commits austauscht. In der Praxis liegt es auf einem Server wie dem GitLab des Fachbereichs (`gitlab.f2.htw-berlin.de`, Anmeldung mit dem HTW-Account) oder auf GitHub. Beide bieten neben dem reinen Speicher einen Webblick auf die Historie, einen Issue-Tracker und Merge Requests.

## Ein Remote-Repository klonen

Der häufigste Einstieg ist nicht `git init`, sondern `git clone`: Ein Projekt existiert bereits auf dem Server, und du holst dir eine vollständige Kopie samt Historie. Die URL findest du auf der Projektseite unter „Clone“:

```bash
git clone git@gitlab.f2.htw-berlin.de:gruppe/geometrieeditor.git
# Cloning into 'geometrieeditor'...
# remote: Enumerating objects: 87, done.
# Receiving objects: 100% (87/87), 24.10 KiB, done.
cd geometrieeditor
git remote -v
# origin  git@gitlab.f2.htw-berlin.de:gruppe/geometrieeditor.git (fetch)
# origin  git@gitlab.f2.htw-berlin.de:gruppe/geometrieeditor.git (push)
```

`git clone` hat automatisch ein Remote namens `origin` eingerichtet, das auf die Quelle zeigt. `origin` ist nur ein Name, so wie `main` ein Name für einen Branch ist – man könnte ihn ändern, tut es aber praktisch nie.

Hat man umgekehrt schon ein lokales Repository wie in [Erste Schritte mit Git](/modules/git_erste_schritte/git_erste_schritte.md), legt man auf dem GitLab ein **leeres** Projekt an (ohne README, sonst gibt es gleich zwei unterschiedliche erste Commits) und verbindet beide von Hand:

```bash
git remote add origin git@gitlab.f2.htw-berlin.de:gruppe/geometrieeditor.git
git push -u origin main
# Enumerating objects: 24, done.
# To gitlab.f2.htw-berlin.de:gruppe/geometrieeditor.git
#  * [new branch]      main -> main
# branch 'main' set up to track 'origin/main'.
```

Die Option `-u` (*upstream*) merkt sich, dass der lokale Branch `main` zu `origin/main` gehört. Ab dem zweiten Mal reicht ein nacktes `git push`.

## Authentifizierung: SSH-Schlüssel

Der Server muss wissen, wer da pusht. Die sauberste Lösung ist ein **SSH-Schlüsselpaar**: Der private Schlüssel bleibt auf deinem Rechner, der öffentliche wird im GitLab-Profil hinterlegt. Ab dann läuft jede Verbindung ohne Passwortabfrage:

```bash
ssh-keygen -t ed25519 -C "vorname.nachname@student.htw-berlin.de"
# Generating public/private ed25519 key pair.
# Enter file in which to save the key (~/.ssh/id_ed25519): [Enter]
# Enter passphrase (empty for no passphrase): [optional, empfohlen]
cat ~/.ssh/id_ed25519.pub
# ssh-ed25519 AAAAC3Nza... vorname.nachname@student.htw-berlin.de
```

Den Inhalt der `.pub`-Datei kopierst du im GitLab unter *Preferences → SSH Keys* hinein. Die Datei **ohne** `.pub` ist der private Schlüssel – er verlässt deinen Rechner nie und landet insbesondere nicht im Repository. Alternativ funktioniert HTTPS: Dann verwendest du statt eines Passworts ein *Personal Access Token*, das du im GitLab erzeugst und das ein Credential Manager auf deinem Rechner speichert.

Passwörter oder Tokens gehören nie in eine Datei im Repository – auch nicht „nur kurz“ in `appsettings.json` oder in eine Klon-URL der Form `https://name:token@server/...`. Was einmal committet ist, bleibt in der Historie und ist für alle sichtbar, die das Repository klonen können. Ein versehentlich committetes Secret gilt als kompromittiert und muss sofort zurückgezogen werden.
{: .notice--warning}

## push, fetch und pull

Drei Befehle bewegen Commits zwischen deinem Repository und dem Remote. `git push` schickt deine neuen Commits zum Server. `git fetch` holt neue Commits vom Server, **ohne** deine Arbeitskopie anzurühren – sie landen im lokalen Zeiger `origin/main`, den du dir in Ruhe ansehen kannst. `git pull` ist `fetch` plus `merge`: Es holt die Commits und führt sie sofort mit deinem Branch zusammen:

```bash
git fetch
# From gitlab.f2.htw-berlin.de:gruppe/geometrieeditor
#    a1f9c3e..e4b7d10  main       -> origin/main
git log --oneline main..origin/main       # was ist neu auf dem Server?
# e4b7d10 Kreis: Umfang mit 2·π·r berechnen
git pull
# Updating a1f9c3e..e4b7d10
# Fast-forward
#  Geometrieeditor.Fachkonzept/Kreis.cs | 2 +-
```

Im Alltag reicht meist `git pull`; `git fetch` ist der vorsichtige Weg, wenn man erst sehen will, was die anderen getan haben. Das „Fast-forward“ in der Ausgabe bedeutet, dass dein `main` einfach auf den neuen Commit vorgespult wurde, weil du selbst nichts geändert hattest – die andere Variante lernen wir in [Branches und Merges](/modules/git_branching/git_branching.md) kennen.

## Ein typischer Arbeitstag

Der Rhythmus im Team ist immer derselbe: **erst holen, dann arbeiten, dann teilen**.

```bash
git pull                                  # 1. aktuellen Stand holen
# ... in der IDE arbeiten ...
git status                                # 2. was habe ich geändert?
git add Geometrieeditor.Fachkonzept/Dreieck.cs
git commit -m "Dreieck: Umfang aus drei Seitenlängen berechnen"
git push                                  # 3. Commits zum Server
```

Wer morgens pullt und abends pusht, hat selten Probleme. Wer eine Woche lang lokal sammelt, bekommt beim Push die folgende Meldung – und dann oft auch Konflikte:

```bash
git push
# ! [rejected]        main -> main (fetch first)
# error: failed to push some refs to 'gitlab.f2.htw-berlin.de:gruppe/geometrieeditor.git'
# hint: Updates were rejected because the remote contains work that you do not
# hint: have locally. ... Integrate the remote changes (e.g. 'git pull ...')
```

Git verweigert den Push, weil auf dem Server inzwischen Commits liegen, die du nicht hast. Dein Push würde sie nicht überschreiben – Git lässt das gar nicht zu –, aber die Historie wäre nicht mehr eine Linie. Die Lösung steht in der Meldung: erst `git pull`, dabei die fremden Commits mit deinen zusammenführen (meist automatisch, im Konfliktfall siehe [Merge-Konflikte lösen](/modules/git_merge_konflikte/git_merge_konflikte.md)), dann erneut `git push`.

Niemals `git push --force` auf `main`, um eine abgelehnte Übertragung zu erzwingen. Damit überschreibst du die Commits der anderen auf dem Server – genau das, was Git eigentlich verhindert.
{: .notice--primary}

## Was ins Remote gehört

Das GitLab ist ein Ort für Quelltexte, Projektdateien und Dokumentation im Textformat – nicht für Build-Ausgaben, große Datensätze, Videos oder das Abgabe-PDF. Solche Dateien lassen die Größe des Repositorys bei jeder Änderung wachsen, und jeder Klon lädt die gesamte Historie herunter. Ein `README.md` mit Bauanleitung und eine `LICENSE` sollten dagegen von Anfang an dabei sein; das GitLab bietet beim Anlegen des Projekts Vorlagen für beides.

Übung: Lege auf dem GitLab ein leeres Projekt an, verbinde es mit deinem Repository aus dem letzten Modul und pushe. Klone das Projekt anschließend in einen zweiten Ordner, ändere dort die `README.md`, committe und pushe. Versuche dann, aus dem ersten Ordner eine andere Änderung zu pushen – welche Meldung erhältst du, und wie löst du sie auf?
{: .notice--info}

## Weitere Quellen

- [Mit Remotes arbeiten – Pro Git (git-scm.com)](https://git-scm.com/book/de/v2/Git-Grundlagen-Mit-Remotes-arbeiten)
- [Git auf dem Server – SSH-Schlüssel erzeugen – Pro Git (git-scm.com)](https://git-scm.com/book/de/v2/Git-auf-dem-Server-Erstellung-eines-SSH-Public-Keys)
- [GitLab-Dokumentation: SSH keys](https://docs.gitlab.com/user/ssh/)
