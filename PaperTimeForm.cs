using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Mono.Options;
using DDay.iCal;
using PaperTime.Properties;
using PaperTime.Data;
using System.Reflection;

namespace PaperTime
{
    public class PaperTimeForm : Form
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        enum ShowWindowCommands
        {
            Restore = 9
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

        static Reporter reporter = new Reporter() { FileNamePattern = PaperTime.Text.DefaultFileNamePattern };
        static Parser parser = new Parser();
        static bool outputStackTrace = false;
        static bool launchLastFile = false;
        static bool renderOnly = false;
        static Format format = Format.None;
        static TimeSpan timePerDay = TimeSpan.FromHours(8.0);
        static Func<Record, bool> filter = r => true;

        [STAThread]
        static void Main(string[] args)
        {
            bool addDefaultWorkdays = true;
            var p = new OptionSet()
            {
                { "render-only", v => renderOnly = v != null},
                { "output-stack-trace", v => outputStackTrace = v != null},                
                { "culture=", (string v) => { System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(v); }},                
                { "ui-culture=", (string v) => { System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(v); }},                
                { "p|filename-pattern=", (string v) => reporter.FileNamePattern = v },                
                { "f|filter=", (string v) => { filter = r => r.Category.TrimStart().ToUpper().StartsWith(v.ToUpper()); } },
                { "l|launch-last-file", v => launchLastFile = v != null },                                
                { "max-duration-limit=", (double v) => parser.MaxDurationLimit = TimeSpan.FromHours(v)},
                { "multiplyer=", (string v) => {
                    var a = v.Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);
                    double m;
                    if (a.Length < 2 || !double.TryParse(a[1], out m)) { return; }
                    parser.AddMultiplyer(a[0], m);                
                }},
                { "h|hours-per-day=", (double v) => timePerDay = TimeSpan.FromHours(v)},
                { "u|hours-per-day-valid-until=", 
                    (DateTime v) => 
                    { 
                        reporter.AddTargetHours(v, timePerDay); 
                        timePerDay = TimeSpan.Zero; 
                    }},
                { "format=", (Format v) => format |= v},
                { "holiday-calendar=",
                    (Uri v) => {
                        var calendarCollection = iCalendar.LoadFromUri(v);
                        if (null == calendarCollection) { return; }
                        var iCal = calendarCollection.First();
                        foreach (var e in iCal.Events)
                        {
                            reporter.AddKnownHolidays(e.Summary, e.Start.Date);
                        }
                    }},
                { "w|workday=",
                    (DayOfWeek v) => {
                        addDefaultWorkdays = false;
                        reporter.AddWorkday(v);
                    }}
            };

            List<string> fileNames = p.Parse(args);
            reporter.AddTargetHours(DateTime.MaxValue, timePerDay);

            if (addDefaultWorkdays)
            {
                reporter.AddWorkday(DayOfWeek.Monday);
                reporter.AddWorkday(DayOfWeek.Tuesday);
                reporter.AddWorkday(DayOfWeek.Wednesday);
                reporter.AddWorkday(DayOfWeek.Thursday);
                reporter.AddWorkday(DayOfWeek.Friday);
            }
            if (Format.None == format)
            {
                format = Format.Html;
            }
            if (0 == fileNames.Count)
            {
                if (null == Settings.Default.RecentFiles ||
                    0 == Settings.Default.RecentFiles.Count)
                {
                    fileNames.Add(PaperTime.Text.DefaultFileName);
                }
                else
                {
                    foreach (string fileName in Settings.Default.RecentFiles)
                    {
                        fileNames.Add(fileName);
                    }
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string lastFileName = fileNames.Last();
            bool createdNew;
            using (System.Threading.Mutex mutex = new System.Threading.Mutex(true, 
                "PaperTime" + lastFileName.GetHashCode().ToString("X8"), out createdNew))
            {
                if (createdNew)
                {
                    Application.Run(new PaperTimeForm(fileNames));
                }
                else
                {
                    LaunchEditor(lastFileName);
                }
            }
        }

        static Process LaunchEditor(string fileName)
        {
            if (!string.IsNullOrEmpty(PaperTime.Properties.Settings.Default.NotepadProcessWindowTitle))
            {
                foreach (Process process in Process.GetProcesses())
                {
                    if (process.MainWindowTitle == PaperTime.Properties.Settings.Default.NotepadProcessWindowTitle)
                    {
                        ShowWindow(process.MainWindowHandle, ShowWindowCommands.Restore);
                        SetForegroundWindow(process.MainWindowHandle);
                        return process;
                    }
                }
            }

            if (!File.Exists(fileName))
            {
                File.WriteAllText(fileName,
                    string.Format(PaperTime.Text.ExampleFileContent,
                        new string('-', string.Format("{0:t} {0:d}", DateTime.MaxValue).Length),
                        DateTime.Now - TimeSpan.FromHours(2.33),
                        DateTime.Now - TimeSpan.FromHours(2),
                        DateTime.Now - TimeSpan.FromHours(1),
                        DateTime.Now - TimeSpan.FromHours(0.44),
                        DateTime.Now - TimeSpan.FromHours(0.22),
                        DateTime.Now - TimeSpan.FromHours(0.11)
                        ));
            }

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "Notepad.exe";
            startInfo.Arguments = fileName;
            Process p = Process.Start(startInfo);
            p.WaitForInputIdle();
            PaperTime.Properties.Settings.Default.NotepadProcessWindowTitle = p.MainWindowTitle;
            PaperTime.Properties.Settings.Default.Save();
            return p;
        }

        private static IEnumerable<IEnumerable<Record>> PerMonth(IEnumerable<Record> records)
        {
            records = records.OrderBy(r => r.Begin);
            DateTime currentMonth = records.First().Begin;
            List<Record> result = new List<Record>();
            foreach (Record r in records)
            {
                if (currentMonth.Year != r.Begin.Year || currentMonth.Month != r.Begin.Month)
                {
                    yield return result;
                    result = new List<Record>();
                    currentMonth = r.Begin;
                }
                result.Add(r);
            }
            if (result.Count > 0)
            {
                yield return result;
            }
        }

        NotifyIcon notifyIcon;

        public PaperTimeForm(IEnumerable<string> fileNames)
        {            
            notifyIcon = new NotifyIcon()
            {
                Text = "Paper Time",
                ContextMenu = new ContextMenu(),
                Visible = true
            };
            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("PaperTime.Icon.ico"))
            {
                notifyIcon.Icon = new System.Drawing.Icon(s);
            }
            notifyIcon.BalloonTipClicked += new EventHandler(OpenReportClick);
            var launchEditorHandler = new EventHandler((s, e) => LaunchEditor(Path.GetFullPath(fileNames.Last())));
            notifyIcon.DoubleClick += launchEditorHandler;
            var launchEditorItem = new MenuItem()
            {
                DefaultItem = true,
                Text = PaperTime.Text.ContextMenuLaunchEditor,
            };
            launchEditorItem.Click += launchEditorHandler;
            notifyIcon.ContextMenu.MenuItems.Add(launchEditorItem);
            notifyIcon.ContextMenu.MenuItems.Add(PaperTime.Text.ContextMenuOpenReport, OpenReportClick);
            notifyIcon.ContextMenu.MenuItems.Add("-");
            notifyIcon.ContextMenu.MenuItems.Add(PaperTime.Text.ContextMenuExit, (s, e) => { this.Close(); });
            LoadFiles(fileNames);
        }

        void OpenReportClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lastFileName))
            {
                Process.Start(lastFileName);
            }
        }

        void UpdateRecentFiles(IEnumerable<string> fileNames)
        {
            foreach (var fileName in fileNames)
            {
                if (null != Settings.Default.RecentFiles &&
                    Settings.Default.RecentFiles.Contains(fileName)) { continue; }
                var rf = new System.Collections.Specialized.StringCollection();
                rf.AddRange(fileNames.ToArray());
                Settings.Default.RecentFiles = rf;
                Settings.Default.Save();
                break;
            }
        }

        void LoadFiles(IEnumerable<string> fileNames)
        {
            UpdateRecentFiles(fileNames);
            if (renderOnly)
            {
                Render(fileNames);
                notifyIcon.BalloonTipClosed += (s, e) => this.Close();
            }
            else
            {
                var renderTimer = new Timer() { Interval = 100 };
                renderTimer.Tick += (s, e) =>
                {
                    renderTimer.Stop();
                    Render(fileNames);
                };
                renderTimer.Start();
                var fileName = Path.GetFullPath(fileNames.Last());
                var path = Path.GetDirectoryName(fileName);
                var fileSystemWatcher = new FileSystemWatcher(path, Path.GetFileName(fileName))
                {
                    EnableRaisingEvents = true,
                    NotifyFilter = NotifyFilters.LastWrite,
                    SynchronizingObject = this
                };
                fileSystemWatcher.Changed += (s, e) =>
                {
                    renderTimer.Stop();
                    renderTimer.Start();
                };
                LaunchEditor(fileName);
            }
        }

        string lastFileName = null;
        void Render(IEnumerable<string> fileNames)
        {
            try
            {
                lastFileName = null;
                var reports = reporter.Generate(parser.Load(fileNames).Where(filter));
                if (0 == reports.Count()) { return; }
                List<string> reportFileNames = new List<string>();
                foreach (var r in reports)
                {
                    reportFileNames.AddRange(Renderer.Save(r, format));
                }
                lastFileName = reportFileNames.Last();
                notifyIcon.ShowBalloonTip(
                    15000,
                    PaperTime.Text.SuccessfullyRendered,
                    Renderer.Preview(reports.Last()),
                    ToolTipIcon.Info);
            }
            catch (Exception ex)
            {
                notifyIcon.ShowBalloonTip(
                    15000,
                    PaperTime.Text.Error,
                    outputStackTrace ? ex.ToString() : ex.Message,
                    ToolTipIcon.Error);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;
            base.OnLoad(e);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                notifyIcon.Dispose();
            }
            base.Dispose(isDisposing);
        }
    }
}
