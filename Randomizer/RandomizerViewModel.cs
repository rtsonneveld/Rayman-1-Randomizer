using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Rayman1Randomizer;

public partial class RandomizerViewModel : ObservableObject
{
    #region Constructor

    public RandomizerViewModel()
    {
        FlagViewModels = new[]
        {
            new RandomizerFlagViewModel("Cages Only", RandomizerFlags.CagesOnly),
        };
    }

    #endregion

    #region Observable Properties

    [ObservableProperty] private bool _isRandomizing = false;
    [ObservableProperty] private bool _restoreOriginalFiles = true;
    [ObservableProperty] private int _requiredCages = 102;
    [ObservableProperty] private string _seed = String.Empty;
    [ObservableProperty] private string _gamePath = String.Empty;
    [ObservableProperty] private string _mkPsxIsoPath = String.Empty;
    [ObservableProperty] private double _currentProgress = 0;

    #endregion

    #region Public Properties

    public RandomizerFlagViewModel[] FlagViewModels { get; }

    public ICommand SelectGameDirectory => new RelayCommand(ShowSelectGameDirectoryDialog);
    public ICommand SelectMkPsxIsoExecutable => new RelayCommand(ShowSelectMkPsxIsoFileDialog);

    #endregion

    #region Private Methods

    private void CopyDirectory(string root, string dest)
    {
        foreach (string directory in Directory.GetDirectories(root))
        {
            string dirName = Path.GetFileName(directory);
            Directory.CreateDirectory(Path.Combine(dest, dirName));
            CopyDirectory(directory, Path.Combine(dest, dirName));
        }

        foreach (string file in Directory.GetFiles(root))
        {
            string destFile = Path.Combine(dest, Path.GetFileName(file));
            CopyFile(file, destFile);
        }
    }

    private void CopyFile(string file, string destFile)
    {
       if (File.Exists(destFile))
          File.Delete(destFile);
       File.Copy(file, destFile);
    }

    private void ShowSelectGameDirectoryDialog()
    {
       CommonOpenFileDialog dialog = new CommonOpenFileDialog();
       dialog.IsFolderPicker = true;
       if (dialog.ShowDialog() == CommonFileDialogResult.Ok) {
          if (IsValidPath(dialog.FileName)) {
             GamePath = dialog.FileName;
          }
       }
    }

    private void ShowSelectMkPsxIsoFileDialog()
    {
       CommonOpenFileDialog dialog = new CommonOpenFileDialog();
       dialog.Filters.Add(new CommonFileDialogFilter("mkpsxiso.exe", "*.exe"));

       if (dialog.ShowDialog() == CommonFileDialogResult.Ok) {
          MkPsxIsoPath = dialog.FileName;
       }
    }

   private static bool IsValidPath(string gamePath)
    {
       var directories = Directory.GetDirectories(gamePath).Select(Path.GetFileName);
       var files = Directory.GetFiles(gamePath).Select(Path.GetFileName);

      if (!directories.Contains("RAY") || !files.Contains("SLUS-000.05")) {

          MessageBox.Show(
             "Invalid game directory, make sure there's a RAY folder and a game executable file named SLUS-000.05 present.", "Invalid game directory", MessageBoxButton.OK, MessageBoxImage.Error);

          return false;
       }

      return true;
    }

    #endregion

   #region Public Methods

   [RelayCommand(IncludeCancelCommand = true)]
    public async Task RandomizeAsync(CancellationToken cancellationToken)
    {
       const string BackupSuffix = ".BAK";

        if (!IsValidPath(GamePath)) {
           return;
        }

        RemoveReadOnlyFromDirectoryRecursive(GamePath);
      
        CopyDirectory(Path.Combine(GamePath, "RAY"), Path.Combine(GamePath, "RAY"+BackupSuffix));
        CopyFile(Path.Combine(GamePath, Randomizer.ExeFileName), Path.Combine(GamePath, Randomizer.ExeFileName + BackupSuffix));

        if (RequiredCages is < 0 or > 102)
            RequiredCages = 102;

        int seed = String.IsNullOrWhiteSpace(Seed) ? new Random().Next() : Seed.GetHashCode();
        RandomizerFlags flags = FlagViewModels.
            Where(flagViewModel => flagViewModel.IsEnabled).
            Aggregate(RandomizerFlags.None, (current, flagViewModel) => current | flagViewModel.Flag);

        RandomizerSettings randomizerSettings = new(seed, GamePath, MkPsxIsoPath, RequiredCages, flags);

        bool forceRestoreFiles = false;

        try
        {
            IsRandomizing = true;

            await Task.Run(() =>
            {
                Randomizer randomizer = new(randomizerSettings);
                randomizer.Start(x => CurrentProgress = x, cancellationToken);
            }, cancellationToken);

            MessageBox.Show("Successfully randomized! Copy Rayman.bin and Rayman.cue from the RaymanFiles folder and have fun :)");
        }
        catch (OperationCanceledException)
        {
            MessageBox.Show("The randomizer was cancelled");
            forceRestoreFiles = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred: {ex.Message}{Environment.NewLine}{Environment.NewLine}{ex}");
        }
        finally
        {
            IsRandomizing = false;
            CurrentProgress = 0;

            if (RestoreOriginalFiles || forceRestoreFiles)
            {
               CopyDirectory(Path.Combine(GamePath, "RAY" + BackupSuffix), Path.Combine(GamePath, "RAY"));
               CopyFile(Path.Combine(GamePath, Randomizer.ExeFileName + BackupSuffix), Path.Combine(GamePath, Randomizer.ExeFileName));
               Directory.Delete(Path.Combine(GamePath, "RAY" + BackupSuffix), true);
               File.Delete(Path.Combine(GamePath, Randomizer.ExeFileName + BackupSuffix));
            }
        }
    }

    private void RemoveReadOnlyFromDirectoryRecursive(string directory) 
    {
      File.SetAttributes(directory, FileAttributes.Normal);

      foreach (string subDirectory in Directory.GetDirectories(directory)) {
          File.SetAttributes(subDirectory, FileAttributes.Normal);
          RemoveReadOnlyFromDirectoryRecursive(subDirectory);
      }

      foreach (string file in Directory.GetFiles(directory)) {
         File.SetAttributes(file, FileAttributes.Normal);
      }
    }

    #endregion
}