using System.Windows;

namespace Rayman1Randomizer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Title += $" {App.Version}";
    }
}