# Baut die native Bibliothek mathe.dll unter Windows.
# Voraussetzung: entweder der Microsoft-C-Compiler cl.exe
# (Visual Studio "Developer PowerShell") oder gcc aus MinGW-w64 / MSYS2.
Set-Location $PSScriptRoot

if (Get-Command cl -ErrorAction SilentlyContinue) {
    cl /nologo /LD mathe.c /Fe:mathe.dll
}
elseif (Get-Command gcc -ErrorAction SilentlyContinue) {
    gcc -shared -o mathe.dll mathe.c
}
else {
    Write-Error "Kein C-Compiler gefunden (cl oder gcc). Bitte Visual Studio Build Tools oder MinGW-w64 installieren."
    exit 1
}

Write-Host "mathe.dll gebaut."
