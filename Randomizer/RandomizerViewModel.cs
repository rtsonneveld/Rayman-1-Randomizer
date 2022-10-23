using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using BinarySerializer;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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
    [ObservableProperty] private double _currentProgress = 0;

    #endregion

    #region Public Properties

    public RandomizerFlagViewModel[] FlagViewModels { get; }

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
            if (File.Exists(destFile))
                File.Delete(destFile);
            File.Copy(file, destFile);
        }
    }

    private int GetStringHash(string str)
    {
        ChecksumCRC32Calculator crc = new();
        byte[] bytes = Encoding.UTF8.GetBytes(str);
        crc.AddBytes(bytes, 0, bytes.Length);
        return (int)crc.ChecksumValue;
    }

    #endregion

    #region Public Methods

    [RelayCommand(IncludeCancelCommand = true)]
    public async Task RandomizeAsync(CancellationToken cancellationToken)
    {
        // Get paths
        string currentDir = Directory.GetCurrentDirectory();
        string gameDir = Path.Combine(currentDir, "RaymanFiles");
        string mkpsxisoLocation = Path.Combine(currentDir, "Tools", "mkpsxiso.exe");

        // TODO: Back up exe file as well?
        CopyDirectory(Path.Combine(gameDir, "RAY"), Path.Combine(gameDir, "RAY_BAK"));

        if (RequiredCages is < 0 or > 102)
            RequiredCages = 102;

        int seed = String.IsNullOrWhiteSpace(Seed) ? new Random().Next() : GetStringHash(Seed);
        RandomizerFlags flags = FlagViewModels.
            Where(flagViewModel => flagViewModel.IsEnabled).
            Aggregate(RandomizerFlags.None, (current, flagViewModel) => current | flagViewModel.Flag);

        RandomizerSettings randomizerSettings = new(seed, gameDir, mkpsxisoLocation, RequiredCages, flags);

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
                CopyDirectory(Path.Combine(gameDir, "RAY_BAK"), Path.Combine(gameDir, "RAY"));
                Directory.Delete(Path.Combine(gameDir, "RAY_BAK"), true);
            }
        }
    }

    #endregion
}