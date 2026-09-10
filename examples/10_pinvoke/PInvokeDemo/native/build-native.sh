#!/usr/bin/env bash
# Baut die native Bibliothek libmathe.dylib (macOS) bzw. libmathe.so (Linux)
# neben dieser Datei. Die .csproj kopiert sie danach in den bin/-Ordner.
set -e
cd "$(dirname "$0")"

case "$(uname -s)" in
  Darwin)
    clang -shared -o libmathe.dylib mathe.c
    echo "libmathe.dylib gebaut."
    ;;
  Linux)
    gcc -shared -fPIC -o libmathe.so mathe.c -lm
    echo "libmathe.so gebaut."
    ;;
  *)
    echo "Unbekanntes System – unter Windows bitte build-native.ps1 verwenden." >&2
    exit 1
    ;;
esac
