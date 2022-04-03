using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ray1ModRandomizerUI
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const string logFileName = "randomizer.log";
        private const int pollingInterval = 1000;
        private const string Raymanfiles = "RaymanFiles";
        public static Version version = new Version(0, 0, 2);

        private static readonly int offsetCageRequirement = 0x86D74;

        private int cages;

        private readonly Dictionary<RandomizerFlags, bool> CheckedFlags = new Dictionary<RandomizerFlags, bool>();
        private string exception = "";
        private int lastLogLine;
        private bool success;

        public MainWindow()
        {
            InitializeComponent();

            var iter = 0;
            foreach (int v in Enum.GetValues(typeof(RandomizerFlags))) {
                var flag = (RandomizerFlags) v;
                CheckedFlags[flag] = false;
                var checkBox = new CheckBox();
                checkBox.Content = flag.ToString();
                checkBox.Checked += (sender, args) => { CheckedFlags[flag] = true; };
                checkBox.Unchecked += (sender, args) => { CheckedFlags[flag] = false; };
                optionsList.Items.Add(checkBox);

                iter++;
            }

            Title += " " + version;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void checkBoxAll_Checked(object sender, RoutedEventArgs e)
        {
            SetCheckboxes(sender, true);
        }

        private void checkBoxAll_Unchecked(object sender, RoutedEventArgs e)
        {
            SetCheckboxes(sender, false);
        }

        private void SetCheckboxes(object sender, bool check)
        {
            foreach (var item in optionsList.Items)
                if (item != sender && item is CheckBox cb)
                    cb.IsChecked = check;
        }

        private void PreRandomize()
        {
            Dispatcher.Invoke(() => { grid.IsEnabled = false; });
            FileUtil.CloneDirectory(Path.Combine(Raymanfiles, "RAY"), Path.Combine(Raymanfiles, "RAY_BAK"));

            if (int.TryParse(cageInput.Text, out cages)) {
                if (cages < 0 || cages > 255) cages = 102;
            } else {
                cages = 102;
            }

            cageInput.Text = cages.ToString();
        }

        private void PostRandomize()
        {
            Dispatcher.Invoke(() =>
            {
                grid.IsEnabled = true;

                if (restoreOriginalFilesCheckbox.IsChecked == true) {
                    FileUtil.CloneDirectory(Path.Combine(Raymanfiles, "RAY_BAK"), Path.Combine(Raymanfiles, "RAY"));
                    Directory.Delete(Path.Combine(Raymanfiles, "RAY_BAK"), true);
                }
            });

            WriteCageRequirement(cages);
        }

        private void WriteCageRequirement(int cages)
        {
            using var binFile = File.OpenWrite(Path.Combine(Raymanfiles, "Rayman.bin"));
            binFile.Seek(offsetCageRequirement, SeekOrigin.Begin);
            binFile.WriteByte((byte) cages);
            binFile.Flush();
            binFile.Close();
        }

        private void randomizeButton_Click(object sender, RoutedEventArgs e)
        {
            PreRandomize();

            var randomSeed = new Random().Next().ToString();
            var seed = string.IsNullOrWhiteSpace(seedInput.Text) ? randomSeed : seedInput.Text;
            var flags = GetFlags().ToString();
            var currentDir = Directory.GetCurrentDirectory();
            var mkpsxisoLocation = Path.GetFullPath(Path.Combine(currentDir, "Tools", "mkpsxiso.exe"));
            var raymanFilesDir = Path.GetFullPath(Path.Combine(currentDir, Raymanfiles));

            if (File.Exists(logFileName)) File.Delete(logFileName);

            lastLogLine = 0;
            exception = "";

            var process = new Process();
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.FileName = Path.Combine("Ray1Map", "Ray1Map.exe");
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.RedirectStandardOutput = true;

            process.StartInfo.Arguments =
                $"-batchmode -logfile \"{logFileName}\" --GameMode RaymanPS1US --directory \"{raymanFilesDir}\" --RandomizerSeed {seed} --RandomizerFlags {flags} --RandomizeBatch true --mkpsxiso \"{mkpsxisoLocation}\"";
            process.Start();

            success = false;

            new Task(() =>
                {
                    while (!process.HasExited) {
                        if (!File.Exists(logFileName)) continue;

                        var lines = FileUtil.ReadAllLines(logFileName);
                        for (var i = lastLogLine; i < lines.Length; i++) HandleOutput(process, lines[i]);

                        lastLogLine = lines.Length;

                        if (process.HasExited) {
                            

                        }

                        Thread.Sleep(pollingInterval);
                    };
                    PostRandomize();

                    if (success)
                        MessageBox.Show(
                            "Successfully randomized! Copy Rayman.bin and Rayman.cue from the RaymanFiles folder and have fun :)");
                    else
                        MessageBox.Show(
                            $"An error occurred: {exception}{Environment.NewLine}Please send {logFileName} to Robin...");
                    Dispatcher.Invoke(() => { progressBar.Value = 0.0f; });
                }
            ).Start();
        }

        private void HandleOutput(Process process, string line)
        {
            Debug.WriteLine(line);
            if (line.StartsWith("progress:")) {
                var progressObj = JsonConvert.DeserializeObject<JToken>(line.Substring("progress:".Length));
                float progress = 0;
                float.TryParse(progressObj["progress"].ToString(), out progress);

                Dispatcher.Invoke(() => { progressBar.Value = progress; });
            }

            if (line.Contains("Exception")) {
                exception = line;
                process.Kill(true);
            }

            if (line.StartsWith("Randomizer Success")) {
                success = true;
                process.Kill(true);
            }
        }

        private int GetFlags()
        {
            return CheckedFlags.Where(kv => kv.Value).Sum(kv => (int) kv.Key);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}