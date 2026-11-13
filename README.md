# Homework Browser

A simple, no-frills web browser for doing homework and research. It opens straight to Google, so you can search, read and look things up without the clutter.

One `.exe`, no install, no setup.

## Download
Grab **`HomeworkBrowser.exe`** from the [latest release](../../releases/latest) and double-click it.

Works on Windows 10 and 11 (64-bit). It uses the Microsoft Edge WebView2 engine that's already built into Windows.

> The first time you run it, Windows SmartScreen may say it "protected your PC" because the app isn't code-signed. Click **More info → Run anyway**.

## How to use
- Type a question or topic in the address bar to search Google, or type a site like `wikipedia.org` to go straight there
- **Back**, **Forward**, **Reload** and **Home** buttons along the top
- No extensions, no tabs, nothing extra

### Keyboard shortcuts
| Key | Does |
| --- | --- |
| `Ctrl+L` | Jump to the address bar |
| `F5` | Reload |
| `Alt+←` / `Alt+→` | Back / Forward |
| `Alt+Home` | Go to Google |

## Build it yourself
Needs the [.NET 8 SDK](https://dotnet.microsoft.com/download).
```
dotnet publish -c Release -o out
```
Output: `out/HomeworkBrowser.exe`. Pushing a tag like `v1.2.0` builds the exe and publishes a GitHub Release automatically.
