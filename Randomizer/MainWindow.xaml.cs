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
using Rayman1Randomizer;

namespace Rayman1Randomizer
{
   /// <summary>
   ///     Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window, INotifyPropertyChanged
   {
      private const string Raymanfiles = "RaymanFiles";
      private const int offsetCageRequirement = 0x86D74;

      public static Version version = new Version(0, 0, 3);

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
            var flag = (RandomizerFlags)v;
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
         binFile.WriteByte((byte)cages);
         binFile.Flush();
         binFile.Close();
      }

      private void randomizeButton_Click(object sender, RoutedEventArgs e)
      {
         PreRandomize();

         int seed = string.IsNullOrWhiteSpace(seedInput.Text) ? new Random().Next() : seedInput.Text.GetHashCode();
         var currentDir = Directory.GetCurrentDirectory();
         var mkpsxisoLocation = Path.GetFullPath(Path.Combine(currentDir, "Tools", "mkpsxiso.exe"));
         var raymanFilesDir = Path.GetFullPath(Path.Combine(currentDir, Raymanfiles));

         var randomizeSettings = new RandomizeSettings(seed, GetFlags(), mkpsxisoLocation, raymanFilesDir);

         success = false;

         new Task(() =>
            {
               new RandomizeProcess(randomizeSettings).Start();

               PostRandomize();

               if (success)
                  MessageBox.Show("Successfully randomized! Copy Rayman.bin and Rayman.cue from the RaymanFiles folder and have fun :)");

               Dispatcher.Invoke(() => { progressBar.Value = 0.0f; });
            }
         ).Start();
      }


      private int GetFlags()
      {
         return CheckedFlags.Where(kv => kv.Value).Sum(kv => (int)kv.Key);
      }

      private void OnPropertyChanged([CallerMemberName] string propertyName = null)
      {
         PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }
   }
}