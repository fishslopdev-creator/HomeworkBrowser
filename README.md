# MiniBrowser

A bare-bones Windows browser with Google as the built-in search engine. One `.exe`, no install.

## Download
Grab `MiniBrowser.exe` from the [Releases](../../releases) page and run it.

Requires Windows 10/11 (x64). Uses the Edge WebView2 engine that ships with Windows.

## Use
- Type a URL (`github.com`) or anything else to search Google
- Back / Forward / Reload / Home buttons
- `Ctrl+L` address bar, `F5` reload, `Alt+←/→` back/forward, `Alt+Home` home

## Build
```
dotnet publish -c Release -o out
```
Output: `out/MiniBrowser.exe`. Pushing a tag like `v1.0.0` builds and attaches the exe to a GitHub Release automatically.
