using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.Run(new BrowserForm(args.Length > 0 ? args[0] : null));
    }
}

class BrowserForm : Form
{
    const string Home = "https://www.google.com";
    readonly WebView2 web = new WebView2 { Dock = DockStyle.Fill };
    readonly TextBox address = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11f) };
    readonly Button back = Btn("←"), fwd = Btn("→"), reload = Btn("⟳"), home = Btn("⌂"), go = Btn("Go");
    readonly string startUrl;

    static Button Btn(string t) => new Button { Text = t, Width = 40, Dock = DockStyle.Left, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11f) };

    public BrowserForm(string start)
    {
        startUrl = start;
        Text = "MiniBrowser";
        Size = new Size(1280, 800);
        StartPosition = FormStartPosition.CenterScreen;
        KeyPreview = true;

        var bar = new Panel { Dock = DockStyle.Top, Height = 36, Padding = new Padding(4) };
        go.Dock = DockStyle.Right;
        var addrHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(6, 2, 6, 0) };
        addrHost.Controls.Add(address);
        bar.Controls.Add(addrHost);
        bar.Controls.Add(go);
        bar.Controls.Add(home);
        bar.Controls.Add(reload);
        bar.Controls.Add(fwd);
        bar.Controls.Add(back);

        Controls.Add(web);
        Controls.Add(bar);

        back.Click += (s, e) => { if (web.CanGoBack) web.GoBack(); };
        fwd.Click += (s, e) => { if (web.CanGoForward) web.GoForward(); };
        reload.Click += (s, e) => web.Reload();
        home.Click += (s, e) => Navigate(Home);
        go.Click += (s, e) => Navigate(address.Text);
        address.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { Navigate(address.Text); e.SuppressKeyPress = true; }
        };
        address.Enter += (s, e) => BeginInvoke(new Action(address.SelectAll));
        KeyDown += OnKeys;

        Load += async (s, e) =>
        {
            try
            {
                var dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MiniBrowser");
                var env = await CoreWebView2Environment.CreateAsync(null, dataDir);
                await web.EnsureCoreWebView2Async(env);
            }
            catch (WebView2RuntimeNotFoundException)
            {
                MessageBox.Show("Microsoft Edge WebView2 Runtime is required (built into Windows 10/11).\nGet it at https://go.microsoft.com/fwlink/p/?LinkId=2124703",
                    "MiniBrowser", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            var core = web.CoreWebView2;
            core.SourceChanged += (_, __) => address.Text = core.Source;
            core.DocumentTitleChanged += (_, __) => Text = core.DocumentTitle + " - MiniBrowser";
            core.HistoryChanged += (_, __) => { back.Enabled = core.CanGoBack; fwd.Enabled = core.CanGoForward; };
            // Open popups / target=_blank in the same window
            core.NewWindowRequested += (_, a) => { a.Handled = true; core.Navigate(a.Uri); };
            Navigate(startUrl ?? Home);
        };
    }

    void OnKeys(object s, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.L) { address.Focus(); address.SelectAll(); }
        else if (e.KeyCode == Keys.F5) web.Reload();
        else if (e.Alt && e.KeyCode == Keys.Left && web.CanGoBack) web.GoBack();
        else if (e.Alt && e.KeyCode == Keys.Right && web.CanGoForward) web.GoForward();
        else if (e.Alt && e.KeyCode == Keys.Home) Navigate(Home);
        else return;
        e.Handled = true;
    }

    void Navigate(string input)
    {
        if (web.CoreWebView2 == null) return;
        var text = (input ?? "").Trim();
        if (text.Length == 0) return;
        web.CoreWebView2.Navigate(ToUrl(text));
        web.Focus();
    }

    static string ToUrl(string t)
    {
        if (Uri.TryCreate(t, UriKind.Absolute, out var u) && (u.Scheme == "http" || u.Scheme == "https" || u.Scheme == "file"))
            return t;
        bool looksLikeHost = !t.Contains(' ') && (t.Contains('.') || t.StartsWith("localhost")) &&
                             Uri.TryCreate("https://" + t, UriKind.Absolute, out var h) && h.Host.Length > 0;
        if (looksLikeHost) return (t.StartsWith("localhost") ? "http://" : "https://") + t;
        return "https://www.google.com/search?q=" + Uri.EscapeDataString(t);
    }
}
