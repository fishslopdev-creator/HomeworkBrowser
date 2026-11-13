using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
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

    // Tiny VM tab: a v86 x86 emulator running KolibriOS, served from resources embedded in the exe
    const string VmHost = "https://tinyvm.local/";
    readonly TabControl tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10f), Padding = new Point(14, 4) };
    readonly TabPage vmTab = new TabPage("🖥️  Tiny VM");
    readonly WebView2 vmWeb = new WebView2 { Dock = DockStyle.Fill };
    CoreWebView2Environment env;
    bool vmStarted;

    async Task StartVm()
    {
        if (vmStarted || env == null) return;
        vmStarted = true;
        await vmWeb.EnsureCoreWebView2Async(env);
        var vm = vmWeb.CoreWebView2;
        vm.Settings.AreBrowserAcceleratorKeysEnabled = false; // let F5, Ctrl+R etc. go to the VM
        vm.Settings.AreDefaultContextMenusEnabled = false;
        vm.AddWebResourceRequestedFilter(VmHost + "*", CoreWebView2WebResourceContext.All);
        vm.WebResourceRequested += (_, a) =>
        {
            var name = new Uri(a.Request.Uri).AbsolutePath.TrimStart('/');
            if (name.Length == 0) name = "index.html";
            var res = Assembly.GetExecutingAssembly().GetManifestResourceStream("vm/" + name);
            if (res == null) { a.Response = env.CreateWebResourceResponse(null, 404, "Not Found", ""); return; }
            var ms = new MemoryStream();
            res.CopyTo(ms);
            ms.Position = 0;
            a.Response = env.CreateWebResourceResponse(ms, 200, "OK", "Content-Type: " + MimeFor(name));
        };
        vm.Navigate(VmHost + "index.html");
    }

    static string MimeFor(string name) => Path.GetExtension(name).ToLowerInvariant() switch
    {
        ".html" => "text/html; charset=utf-8",
        ".js" => "text/javascript",
        ".wasm" => "application/wasm",
        _ => "application/octet-stream",
    };

    static Button Btn(string t) => new Button { Text = t, Width = 40, Dock = DockStyle.Left, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11f) };

    public BrowserForm(string start)
    {
        startUrl = start;
        Text = "Homework Browser";
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

        var webTab = new TabPage("🌐  Web");
        webTab.Controls.Add(web);
        webTab.Controls.Add(bar);
        vmTab.Controls.Add(vmWeb);
        tabs.TabPages.Add(webTab);
        tabs.TabPages.Add(vmTab);
        tabs.SelectedIndexChanged += async (s, e) => { if (tabs.SelectedTab == vmTab) await StartVm(); };
        Controls.Add(tabs);

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
                var dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HomeworkBrowser");
                env = await CoreWebView2Environment.CreateAsync(null, dataDir);
                await web.EnsureCoreWebView2Async(env);
            }
            catch (WebView2RuntimeNotFoundException)
            {
                MessageBox.Show("Microsoft Edge WebView2 Runtime is required (built into Windows 10/11).\nGet it at https://go.microsoft.com/fwlink/p/?LinkId=2124703",
                    "Homework Browser", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            var core = web.CoreWebView2;
            core.SourceChanged += (_, __) => address.Text = core.Source;
            core.DocumentTitleChanged += (_, __) => Text = core.DocumentTitle + " - Homework Browser";
            core.HistoryChanged += (_, __) => { back.Enabled = core.CanGoBack; fwd.Enabled = core.CanGoForward; };
            // Open popups / target=_blank in the same window
            core.NewWindowRequested += (_, a) => { a.Handled = true; core.Navigate(a.Uri); };
            bool openVm = startUrl == "--vm";
            Navigate(openVm ? Home : startUrl ?? Home);
            if (openVm) tabs.SelectedTab = vmTab;
        };
    }

    void OnKeys(object s, KeyEventArgs e)
    {
        if (tabs.SelectedTab == vmTab) return;
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
