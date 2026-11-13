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
- No extensions, nothing extra

## 🖥️ Tiny VM tab
Click the **Tiny VM** tab for a little virtual computer that runs right inside the app, still with no install.
It boots [KolibriOS](https://kolibrios.org), a tiny desktop OS with a file manager, text editor, calculator, paint and some small games.

- **Drag files** onto the Tiny VM tab to put them on the VM's disk. The VM restarts and the files show up at **`/hd0/1`**. Open the file manager (KFM) on its desktop to browse them.
- Click the VM screen to use the mouse, and press **Esc** to get your mouse back
- **↻ Restart** reboots it. Nothing inside the VM is saved after you close the app.

It's an emulated 32-bit PC, so it's for small, light stuff only. It can't run Windows apps or games like Roblox.
Powered by [v86](https://github.com/copy/v86). See `vm/THIRD-PARTY-NOTICES.txt`.

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
