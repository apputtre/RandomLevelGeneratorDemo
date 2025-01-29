using System.Text;
using System.Windows;
using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RandomLevelGeneratorDemo;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private LevelViewer levelViewer;
    private LevelBuilder levelBuilder;
    private RandomLevelGenerator levelGenerator;

    public MainWindow()
    {
        InitializeComponent();

        levelBuilder = new();
        levelGenerator = new(levelBuilder);
    }

    public void OnLoad(object sender, EventArgs e)
    {
        levelViewer = (LevelViewer)FindName("Viewer");

        levelViewer.LevelWidth = levelGenerator.GetParameters().Width;
        levelViewer.LevelHeight = levelGenerator.GetParameters().Height;
    }

    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        GenerateLevel();
    }

    private async void GenerateLevel()
    {
        Label statusLabel = (Label)FindName("StatusLabel");
        statusLabel.Content = "Generating...";
        Button generateButton = (Button)FindName("GenerateButton");
        generateButton.IsEnabled = false;
        await Task.Run(() => levelGenerator.Generate());
        statusLabel.Content = "Done";
        generateButton.IsEnabled = true;
        levelViewer.Level = levelBuilder.Level;
        levelViewer.Update();
    }

    private void LevelWidthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        levelViewer.LevelWidth = (int) e.NewValue;
        levelViewer.Update();

        LevelParameters currParams = levelGenerator.GetParameters();
        LevelParameters newParams = currParams with { Width = (int)e.NewValue };
        levelGenerator.SetParameters(newParams);
    }
    private void LevelHeightSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        levelViewer.LevelHeight = (int) e.NewValue;
        levelViewer.Update();

        LevelParameters currParams = levelGenerator.GetParameters();
        LevelParameters newParams = currParams with { Height = (int)e.NewValue };
        levelGenerator.SetParameters(newParams);
    }
}