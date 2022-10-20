using CommunityToolkit.Mvvm.ComponentModel;

namespace Rayman1Randomizer;

public partial class RandomizerFlagViewModel : ObservableObject
{
    #region Constructor

    public RandomizerFlagViewModel(string displayName, RandomizerFlags flag)
    {
        DisplayName = displayName;
        Flag = flag;
    }

    #endregion

    #region Observable Properties

    [ObservableProperty] private bool _isEnabled;

    #endregion

    #region Public Properties

    public string DisplayName { get; }
    public RandomizerFlags Flag { get; }

    #endregion
}