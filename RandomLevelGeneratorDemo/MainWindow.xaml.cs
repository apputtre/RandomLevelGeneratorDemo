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
using System.Text.RegularExpressions;

namespace RandomLevelGeneratorDemo;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private LevelViewer levelViewer;
    private LevelBuilder levelBuilder;
    private RandomLevelGenerator levelGenerator;
    private TextBox seedTextBox;
    private bool manualSeeding = false;

    public MainWindow()
    {
        InitializeComponent();

        levelBuilder = new();
        levelGenerator = new(levelBuilder);
    }

    public void OnLoad(object sender, EventArgs e)
    {
        levelViewer = (LevelViewer)FindName("Viewer");
        seedTextBox = (TextBox)FindName("SeedTextBox");

        levelViewer.LevelWidth = levelGenerator.GetParameters().Width;
        levelViewer.LevelHeight = levelGenerator.GetParameters().Height;

        levelViewer.UpdateViewBorder();
    }

    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        if (manualSeeding)
            if (seedTextBox.Text == string.Empty)
            {
                MessageBox.Show("Please enter a seed or select auto seeding mode.");
                return;
            }

        GenerateLevel();
    }

    private async void GenerateLevel()
    {
        Label statusLabel = (Label)FindName("StatusLabel");
        statusLabel.Content = "Generating...";
        Button generateButton = (Button)FindName("GenerateButton");
        generateButton.IsEnabled = false;
        try
        {
            if (manualSeeding)
                levelGenerator.NextSeed = int.Parse(seedTextBox.Text);

            await Task.Run(() => levelGenerator.Generate());
            levelViewer.Level = levelBuilder.Level;
            levelViewer.UpdateLevelView();
            seedTextBox.Text = levelGenerator.Seed.ToString();
            Console.WriteLine($"Level Width: {levelGenerator.GetParameters().Width}");
            Console.WriteLine($"Level Height: {levelGenerator.GetParameters().Height}");
        }
        catch(Exception e)
        {
            MessageBox.Show("A level generation error has occurred.");
        }
        statusLabel.Content = "Done";
        generateButton.IsEnabled = true;
    }

    private void LevelWidthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        levelViewer.LevelWidth = (int) e.NewValue;
        levelViewer.UpdateViewBorder();

        LevelParameters currParams = levelGenerator.GetParameters();
        LevelParameters newParams = currParams with { Width = (int)e.NewValue };
        levelGenerator.SetParameters(newParams);
    }

    private void LevelHeightSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        levelViewer.LevelHeight = (int) e.NewValue;
        levelViewer.UpdateViewBorder();

        LevelParameters currParams = levelGenerator.GetParameters();
        LevelParameters newParams = currParams with { Height = (int)e.NewValue };
        levelGenerator.SetParameters(newParams);
    }

    private void NumRoomsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        LevelParameters currParams = levelGenerator.GetParameters();
        LevelParameters newParams = currParams with { NumRooms = (int)e.NewValue };
        levelGenerator.SetParameters(newParams);
    }

    private void SeedTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!IsLoaded)
            return;

        string text = seedTextBox.Text;
        string strippedText = Regex.Replace(text, "[^0-9]", "");

        if (text != strippedText)
        {
            seedTextBox.Text = strippedText;
            MessageBox.Show("Seeds must include only numeric characters.");
        }
    }

    private void AutoSeedingSelected(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded)
            return;

        manualSeeding = false;
        seedTextBox.IsReadOnly = true;
    }

    private void ManualSeedingSelected(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded)
            return;

        manualSeeding = true;
        seedTextBox.IsReadOnly = false;
    }
}